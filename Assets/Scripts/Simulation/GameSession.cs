using System;
using System.Collections.Generic;
using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class GameSession
    {
        readonly HashSet<string> _flags;
        readonly List<string> _notices;

        public WorldData World { get; private set; }
        public PlayerManager Player { get; private set; }
        public ReputationSystem Reputation { get; private set; }
        public HeatSystem Heat { get; private set; }
        public HardwareSystem Hardware { get; private set; }
        public SkillSystem Skills { get; private set; }
        public MissionSystem Missions { get; private set; }
        public OsintSystem Osint { get; private set; }
        public NewsSystem News { get; private set; }
        public BrowserSystem Browser { get; private set; }
        public OrganizationSystem Organizations { get; private set; }
        public TerminalSystem Terminal { get; private set; }
        public int CoolUses { get; private set; }

        public IReadOnlyList<string> Notices
        {
            get { return _notices; }
        }

        public event Action StateChanged;

        GameSession(WorldData world)
        {
            World = world;
            _flags = new HashSet<string>();
            _notices = new List<string>();
            Player = new PlayerManager();
            Reputation = new ReputationSystem();
            Heat = new HeatSystem();
            Hardware = new HardwareSystem();
            Skills = new SkillSystem();
            Missions = new MissionSystem(world);
            Osint = new OsintSystem(world);
            News = new NewsSystem(world);
            Browser = new BrowserSystem(world);
            Organizations = new OrganizationSystem(world);
            Terminal = new TerminalSystem(this);
        }

        public static GameSession NewGame()
        {
            var game = new GameSession(WorldCatalog.Create());
            game.PushNotice("The desktop guide is open. Your money is in the bank site, not on the screen.");
            return game;
        }

        public string ExportSave()
        {
            return SaveSystem.Export(this);
        }

        public bool TryImportSave(string json, out string error)
        {
            var fresh = new GameSession(World);
            if (!SaveSystem.TryApply(fresh, json, out error))
                return false;

            Player.RestoreMoney(fresh.Player.MoneyEuros);
            Player.RestoreLedger(new List<LedgerLine>(fresh.Player.Ledger));
            Heat.Restore(fresh.Heat.Value);
            Reputation.Restore(
                fresh.Reputation.Get(RepKind.Hacker),
                fresh.Reputation.Get(RepKind.Underground),
                fresh.Reputation.Get(RepKind.Corporate),
                fresh.Reputation.Get(RepKind.Intelligence));
            Skills.Restore(fresh.Skills.ExportXp());
            Hardware.Restore(fresh.Hardware.RamGb, fresh.Hardware.CpuTier, fresh.Hardware.StorageGb, fresh.Hardware.NetTier, fresh.Hardware.MaxWindows, fresh.Hardware.OwnedIds());
            Missions.Restore(fresh.Missions.CompletedIds());
            Osint.Restore(fresh.Osint.KnownIds(), fresh.Osint.LinkKeys());
            _flags.Clear();
            foreach (var flag in fresh.ExportFlags())
                _flags.Add(flag);
            News.KeepBaselineAnd(World, fresh.News.ExportExtras(World));
            CoolUses = fresh.CoolUses;
            _notices.Clear();
            Touch();
            return true;
        }

        public void ResetNew()
        {
            string error;
            TryImportSave(NewGame().ExportSave(), out error);
            PushNotice("New session. The semester clock starts again.");
        }

        public CommandResult Execute(string line)
        {
            return Terminal.Execute(line);
        }

        public WebPage Visit(string url)
        {
            var page = Browser.Find(url);
            if (page == null)
                return null;
            SetFlag("visited:" + page.Url);
            if (page.Reveals != null)
            {
                for (int i = 0; i < page.Reveals.Length; i++)
                    Osint.Reveal(page.Reveals[i]);
            }
            Touch();
            return page;
        }

        public CommandResult Scan(string query)
        {
            var company = Organizations.Find(query);
            if (company == null)
                return CommandResult.Fail("No fictional organization matches that name.");

            bool first = SetFlag("scanned:" + company.Id);
            if (first)
                Skills.Add(SkillBranch.Osint, 4);
            if (!string.IsNullOrEmpty(company.NodeId))
                Osint.Reveal(company.NodeId);

            var text = company.ScanReport;
            if (Hardware.CpuTier == 0)
                text += "\n\n(The fan spins hard. A better CPU would finish this cleaner.)";
            else
                text += "\n\n(The refurbished CPU returns the reading cleanly.)";
            Touch();
            return CommandResult.Success(text);
        }

        public CommandResult Lookup(string query)
        {
            if (Skills.Level(SkillBranch.Osint) < 1)
                return CommandResult.Fail("You do not know how to search notes yet.");
            if (string.IsNullOrEmpty(query) || query.Length < 3)
                return CommandResult.Fail("Give a name of at least 3 letters.");

            query = query.ToLowerInvariant();
            bool blocked;
            bool ambiguous;
            var matches = Osint.Match(query, out blocked, out ambiguous);
            if (ambiguous)
                return CommandResult.Fail("That word matches more than one record. Use the id.");
            if (matches.Count == 0)
                return CommandResult.Fail("No public record matches.");

            var sb = new StringBuilder();
            bool any = false;
            for (int i = 0; i < matches.Count; i++)
            {
                var node = matches[i];
                if (!Osint.IsKnown(node.Id) && !Osint.CanDiscover(node.Id))
                {
                    blocked = true;
                    continue;
                }

                bool revealed = !Osint.IsKnown(node.Id);
                Osint.Reveal(node.Id);
                if (revealed)
                    Skills.Add(SkillBranch.Osint, 6);
                if (any)
                    sb.Append("\n\n");
                sb.Append(Dossier(node));
                any = true;
            }

            if (!any && blocked)
                return CommandResult.Fail("You need a related public record before that search returns anything.");
            Touch();
            return CommandResult.Success(sb.ToString());
        }

        public CommandResult Inspect(string token)
        {
            if (Skills.Level(SkillBranch.Osint) < 1)
                return CommandResult.Fail("Inspection comes after basic search.");
            var id = Osint.ResolveId(token == null ? "" : token.ToLowerInvariant());
            if (id == null)
                return CommandResult.Fail("Unknown record.");
            if (!Osint.IsKnown(id))
                return CommandResult.Fail("You have not found that record yet.");
            Touch();
            return CommandResult.Success(Dossier(Osint.Get(id)));
        }

        public CommandResult Connect(string left, string right)
        {
            if (Skills.Level(SkillBranch.Osint) < 2)
                return CommandResult.Fail("You can see records, but you cannot judge which ones support each other yet. Finish the campus survey.");

            var a = Osint.ResolveId(left == null ? "" : left.ToLowerInvariant());
            var b = Osint.ResolveId(right == null ? "" : right.ToLowerInvariant());
            if (a == null || b == null)
                return CommandResult.Fail("Use two record ids. inspect shows them.");
            if (a == b)
                return CommandResult.Fail("Pick two different records.");
            if (!Osint.IsKnown(a) || !Osint.IsKnown(b))
                return CommandResult.Fail("You do not have both records.");
            if (Osint.HasLink(a, b))
                return CommandResult.Fail("Those records are already linked.");
            if (!Osint.IsTrueEdge(a, b))
                return CommandResult.Fail("Those records do not support each other.");

            Osint.TryLink(a, b);
            Skills.Add(SkillBranch.Osint, 8);
            int done = Osint.RequiredFootprintDone();
            int need = Osint.RequiredFootprintCount();
            Touch();
            return CommandResult.Success("Linked " + a + " → " + b + ". Required map " + done + "/" + need + ".");
        }

        public CommandResult Analyze(string query)
        {
            if (Skills.Level(SkillBranch.Osint) < 3)
                return CommandResult.Fail("Analysis is still a course you have not finished. Keep mapping public records.");
            if (!IsNovaToken(query))
                return CommandResult.Fail("You only have notes on one ticker: nova.");

            SetFlag("analyzed_nova");
            Skills.Add(SkillBranch.Crypto, 8);
            string text;
            if (Hardware.CpuTier >= 1)
            {
                SetFlag("analyzed_full");
                text =
                    "$NOVA — full reading\n\n" +
                    "Index: 1.20 fictional units\n" +
                    "Nightwire sentiment: loud and cheerful\n" +
                    "Liquidity: thin\n" +
                    "Post author: graywhale (no older posts)\n" +
                    "Developer wallet: created 9 days ago\n" +
                    "Volume spike: two wallets, not a crowd\n" +
                    "Site text: matches an older ticker called NOVA-TWO\n\n" +
                    "Public pattern: hype, thin liquidity, a brand-new wallet.\n" +
                    "This is not a buy or sell instruction.";
            }
            else
            {
                text =
                    "$NOVA — partial reading\n" +
                    "The fan ran out of breath before the last rows.\n\n" +
                    "Index: 1.20 fictional units\n" +
                    "Nightwire sentiment: loud and cheerful\n" +
                    "Liquidity: thin\n" +
                    "Post author: graywhale (no older posts)\n\n" +
                    "This is not a buy or sell instruction.\n" +
                    "A refurbished CPU would keep the rows this laptop dropped.";
            }

            Touch();
            return CommandResult.Success(text);
        }

        public CommandResult TryTurnIn(string missionId)
        {
            var mission = Missions.Find(missionId);
            if (mission == null)
                return CommandResult.Fail("No job uses that id. Type missions.");
            if (!Missions.IsAvailable(mission.Id))
            {
                if (Missions.IsComplete(mission.Id))
                    return CommandResult.Fail("That job is already filed.");
                return CommandResult.Fail("That job is not open yet.");
            }

            if (mission.Kind == MissionKind.Choice)
                return CommandResult.Fail("This one is a decision. Use choose " + mission.Id + " and an option.");
            if (mission.Kind == MissionKind.Quiz)
                return CommandResult.Fail("Grade it in the browser: https://northline.security/inbox");

            if (mission.Id == "campus_survey")
            {
                if (!HasFlag("visited:www.lumen.edu/it"))
                    return CommandResult.Fail("Read https://www.lumen.edu/it first.");
                if (!HasFlag("scanned:lumen"))
                    return CommandResult.Fail("You still need to scan lumen.");
                CompleteCampus();
                return CommandResult.Success("Survey filed. Lumen IT pays €40. OSINT is no longer only a word in a lecture.");
            }

            if (mission.Id == "novamart_footprint")
            {
                if (!Osint.RequiredFootprintComplete())
                    return CommandResult.Fail("The map is incomplete. " + Osint.RequiredFootprintDone() + "/" + Osint.RequiredFootprintCount() + " required links.");
                CompleteFootprint();
                return CommandResult.Success("Map filed with Northline. €80 for public facts. The staging page is still up if you want to answer it yourself.");
            }

            return CommandResult.Fail("That job cannot be filed from here.");
        }

        public CommandResult Choose(string missionId, string optionId)
        {
            var mission = Missions.Find(missionId);
            if (mission == null || mission.Kind != MissionKind.Choice)
                return CommandResult.Fail("That is not a decision on your desk.");
            if (!Missions.IsAvailable(mission.Id))
            {
                if (Missions.IsComplete(mission.Id))
                    return CommandResult.Fail("You already settled that.");
                return CommandResult.Fail("That decision is not open yet.");
            }

            optionId = optionId == null ? "" : optionId.ToLowerInvariant();
            if (mission.Id == "novamart_decision")
                return ChooseNovaMart(optionId);
            if (mission.Id == "nova_rumor")
                return ChooseRumor(optionId);
            return CommandResult.Fail("No such option.");
        }

        public CommandResult SubmitQuiz(string quizId, string optionId)
        {
            var quiz = Browser.FindQuiz(quizId);
            if (quiz == null)
                return CommandResult.Fail("No such training.");
            if (!Missions.IsAvailable("helio_awareness"))
            {
                if (Missions.IsComplete("helio_awareness"))
                    return CommandResult.Fail("Northline already paid you for that inbox.");
                return CommandResult.Fail("Northline is not hiring you yet. Finish the campus survey.");
            }

            if (optionId != quiz.CorrectId)
                return CommandResult.Fail(quiz.Hint);

            Missions.MarkComplete("helio_awareness");
            Player.Earn(120, "Northline training");
            Reputation.Add(RepKind.Corporate, 8);
            Reputation.Add(RepKind.Hacker, 2);
            Skills.Add(SkillBranch.Social, 30);
            Skills.Add(SkillBranch.Security, 10);
            News.Publish("news_quiz", "Northline keeps hiring student graders.", "Helio's training inbox is busy. The firm is small and picky.");
            PushNotice("Northline paid €120. You spotted the lookalike message.");
            Touch();
            return CommandResult.Success(quiz.Success + " Paid €120.");
        }

        public CommandResult TryUpgrade(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId))
                return CommandResult.Success(UpgradeList());

            UpgradeDef offer = null;
            var offers = World.Upgrades;
            for (int i = 0; i < offers.Length; i++)
            {
                if (offers[i].Id == upgradeId)
                    offer = offers[i];
            }
            if (offer == null)
                return CommandResult.Fail("The shop does not stock that.");
            if (!offer.Purchasable)
                return CommandResult.Fail(offer.Detail);
            if (Hardware.Owns(offer.Id))
                return CommandResult.Fail("You already installed that.");
            if (!Player.TrySpend(offer.Cost, offer.Name))
                return CommandResult.Fail("Short €" + (offer.Cost - Player.MoneyEuros) + ". " + offer.Name + " costs €" + offer.Cost + ".");

            Hardware.ApplyUpgrade(offer.Id);
            PushNotice(offer.Name + " installed.");
            Touch();
            return CommandResult.Success(offer.Name + " installed. Cash is now €" + Player.MoneyEuros + ".");
        }

        public CommandResult CoolOff()
        {
            if (Heat.Value <= 0)
                return CommandResult.Fail("You are not warm.");
            if (!Player.TrySpend(10, "Campus kiosk"))
                return CommandResult.Fail("A campus kiosk wants €10 to clear local notes. You do not have it.");

            int reduce = CoolUses == 0 ? 12 : CoolUses == 1 ? 8 : 4;
            int before = Heat.Value;
            int applied = reduce < before ? reduce : before;
            Heat.Add(-applied);
            CoolUses++;
            Skills.Add(SkillBranch.Opsec, 6);
            PushNotice("Local notes cleared at a campus kiosk.");
            Touch();
            return CommandResult.Success(
                "You paid €10 at a campus kiosk to clear local notes. Anyone who already copied you is unaffected.");
        }

        public string StatusText()
        {
            return Hardware.Summary() + "\nOSINT: L" + Skills.Level(SkillBranch.Osint) + " " + Skills.RankName(SkillBranch.Osint);
        }

        public string UpgradeList()
        {
            var sb = new StringBuilder();
            sb.Append("CAMPUS SHOP");
            var offers = World.Upgrades;
            for (int i = 0; i < offers.Length; i++)
            {
                var offer = offers[i];
                sb.Append("\n\n");
                sb.Append(offer.Name);
                sb.Append("  [");
                sb.Append(offer.Id);
                sb.Append("]  €");
                sb.Append(offer.Cost);
                sb.Append("\n");
                sb.Append(offer.Detail);
                sb.Append("\n");
                if (!offer.Purchasable)
                    sb.Append("Status: not for sale");
                else if (Hardware.Owns(offer.Id))
                    sb.Append("Status: installed");
                else
                    sb.Append("Status: buy with upgrade " + offer.Id);
            }
            return sb.ToString();
        }

        public bool HasFlag(string id)
        {
            return _flags.Contains(id);
        }

        public List<string> ExportFlags()
        {
            var list = new List<string>(_flags);
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        public void ImportFlags(IEnumerable<string> flags)
        {
            _flags.Clear();
            if (flags == null)
                return;
            foreach (var flag in flags)
            {
                if (!string.IsNullOrEmpty(flag))
                    _flags.Add(flag);
            }
        }

        public void RestoreCoolUses(int value)
        {
            CoolUses = value < 0 ? 0 : value;
        }

        public void PushNotice(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            _notices.Add(text);
            Touch();
        }

        void CompleteCampus()
        {
            Missions.MarkComplete("campus_survey");
            Player.Earn(40, "Lumen IT survey");
            Reputation.Add(RepKind.Corporate, 3);
            Reputation.Add(RepKind.Hacker, 1);
            Skills.Add(SkillBranch.Osint, 24);
            PushNotice("€40 from Lumen IT. Connect is now something you can try.");
            Touch();
        }

        void CompleteFootprint()
        {
            Missions.MarkComplete("novamart_footprint");
            Player.Earn(80, "Northline map");
            Reputation.Add(RepKind.Corporate, 2);
            Reputation.Add(RepKind.Hacker, 4);
            Reputation.Add(RepKind.Intelligence, 2);
            Skills.Add(SkillBranch.Osint, 24);
            PushNotice("Northline paid €80 for the public map.");
            Touch();
        }

        CommandResult ChooseNovaMart(string optionId)
        {
            if (optionId == "report")
            {
                Missions.MarkComplete("novamart_decision");
                Player.Earn(70, "NovaMart note");
                Reputation.Add(RepKind.Corporate, 12);
                Reputation.Add(RepKind.Intelligence, 4);
                News.Publish("news_report", "NovaMart closes a public staging link after a student note.", "The retailer thanked an unnamed student and took the page down.");
                PushNotice("NovaMart paid €70 for the responsible note.");
                Touch();
                return CommandResult.Success("You sent the map to NovaMart's public contact. They pay €70 and take the staging link down. The payment lands in your Helio account.");
            }
            if (optionId == "sell")
            {
                Missions.MarkComplete("novamart_decision");
                Player.Earn(180, "Nightwire broker");
                Reputation.Add(RepKind.Underground, 14);
                Reputation.Add(RepKind.Hacker, 3);
                Reputation.Add(RepKind.Corporate, -4);
                AddHeat(24);
                News.Publish("news_sell", "A NovaMart staging rumor is moving through Grayhaven chats.", "Nobody signs the posts. The retailer has not confirmed anything.");
                PushNotice("A broker paid €180. Campus IT may notice the noise.");
                Touch();
                return CommandResult.Success("You hand the public staging notes to a Nightwire broker. They pay €180 into your Helio account. They do not say what they will do with the file. People may start asking questions.");
            }
            if (optionId == "hold")
            {
                Missions.MarkComplete("novamart_decision");
                Reputation.Add(RepKind.Intelligence, 8);
                Skills.Add(SkillBranch.Opsec, 10);
                PushNotice("You kept the map. No pay. A little more judgment.");
                Touch();
                return CommandResult.Success("You keep the map in local notes. No pay arrives in the bank.");
            }
            return CommandResult.Fail("Options: report, sell, hold.");
        }

        CommandResult ChooseRumor(string optionId)
        {
            if (!HasFlag("analyzed_nova"))
                return CommandResult.Fail("Read the ticker first: analyze nova.");
            if (!HasFlag("visited:nightwire.social"))
                return CommandResult.Fail("Read the post on https://nightwire.social before you answer it.");

            bool full = HasFlag("analyzed_full");
            if (optionId == "warn")
            {
                Missions.MarkComplete("nova_rumor");
                Player.Earn(90, "Nightwire warning");
                Reputation.Add(RepKind.Intelligence, full ? 12 : 5);
                if (full)
                    Reputation.Add(RepKind.Corporate, 2);
                News.Publish("news_warn", "Nightwire moderators label the $NOVA post unverified.", "A student note listed thin liquidity and a new wallet. The post stays up, with a warning.");
                PushNotice(full ? "The warning cited the dropped rows. €90." : "You warned the board with a partial reading. €90.");
                Touch();
                return CommandResult.Success(full
                    ? "You warn the board with the full reading. Moderators pin it. €90, and intelligence reputation moves more than a guess would."
                    : "You warn the board, but the laptop dropped rows, so the note is thin. €90.");
            }
            if (optionId == "amplify")
            {
                Missions.MarkComplete("nova_rumor");
                Player.Earn(160, "Nightwire post");
                Reputation.Add(RepKind.Underground, 10);
                AddHeat(full ? 28 : 18);
                News.Publish("news_amp", "The $NOVA post is everywhere on Nightwire.", "So are the arguments about who is being used.");
                PushNotice("You boosted the post. €160 landed in Helio. People may start asking questions.");
                Touch();
                return CommandResult.Success(full
                    ? "You already saw the new wallet and still boosted graywhale. €160 is in Helio. More people noticed because the reading was public."
                    : "You boost the post. €160 is in Helio. You did not have the full reading.");
            }
            return CommandResult.Fail("Options: warn, amplify.");
        }

        void AddHeat(int delta)
        {
            int before = Heat.Value;
            Heat.Add(delta);
            if (before < 20 && Heat.Value >= 20)
            {
                PushNotice("Campus IT flagged your login for review. Nothing formal yet.");
                News.Publish("news_heat", "Campus IT opens a quiet review of unusual logins.", "No names are public. Students are talking anyway.");
            }
            else if (before < 40 && Heat.Value >= 40)
            {
                PushNotice("Helio Mutual paused optional transfers on your student account.");
            }
            else if (before < 60 && Heat.Value >= 60)
            {
                PushNotice("A Nightwire contact stopped replying.");
            }
        }

        bool SetFlag(string id)
        {
            if (_flags.Contains(id))
                return false;
            _flags.Add(id);
            return true;
        }

        static bool IsNovaToken(string query)
        {
            if (query == null)
                return false;
            query = query.ToLowerInvariant();
            return query == "nova" || query == "$nova" || query == "token_nova" || query == "nova-token";
        }

        static string Dossier(OsintNode node)
        {
            return node.Title + "\nkind: " + node.Kind + "\nid: " + node.Id + "\n" + node.Fact;
        }

        void Touch()
        {
            var handler = StateChanged;
            if (handler != null)
                handler();
        }
    }
}
