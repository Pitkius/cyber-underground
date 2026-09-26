using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CyberUnderground.Simulation
{
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;

        public static string Export(GameSession game)
        {
            var sb = new StringBuilder();
            sb.Append("v=1\n");
            Line(sb, "money", game.Player.MoneyEuros.ToString(CultureInfo.InvariantCulture));
            var ledger = game.Player.Ledger;
            for (int i = 0; i < ledger.Count; i++)
                Line(sb, "tx", ledger[i].Amount.ToString(CultureInfo.InvariantCulture) + "|" + (ledger[i].Label ?? ""));
            Line(sb, "heat", game.Heat.Value.ToString(CultureInfo.InvariantCulture));
            Line(sb, "rep", game.Reputation.Get(RepKind.Hacker) + "," + game.Reputation.Get(RepKind.Underground) + "," + game.Reputation.Get(RepKind.Corporate) + "," + game.Reputation.Get(RepKind.Intelligence));
            var xp = game.Skills.ExportXp();
            Line(sb, "xp", JoinInts(xp));
            Line(sb, "prog", game.Progress.CyberXp + "," + game.Progress.SkillPoints + "," + game.Progress.TechPoints + "," + game.Progress.Knowledge);
            Line(sb, "abilities", Join(game.Progress.UnlockedIds()));
            Line(sb, "hw", game.Hardware.RamGb + "," + game.Hardware.CpuTier + "," + game.Hardware.StorageGb + "," + game.Hardware.NetTier + "," + game.Hardware.MaxWindows);
            Line(sb, "upgrades", Join(game.Hardware.OwnedIds()));
            Line(sb, "done", Join(game.Missions.CompletedIds()));
            Line(sb, "flags", Join(game.ExportFlags()));
            Line(sb, "nodes", Join(game.Osint.KnownIds()));
            Line(sb, "links", Join(game.Osint.LinkKeys()));
            Line(sb, "cool", game.CoolUses.ToString(CultureInfo.InvariantCulture));
            var extras = game.News.ExportExtras(game.World);
            for (int i = 0; i < extras.Count; i++)
            {
                sb.Append("news=");
                sb.Append(Escape(extras[i].Id));
                sb.Append('\t');
                sb.Append(Escape(extras[i].Headline));
                sb.Append('\t');
                sb.Append(Escape(extras[i].Body));
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public static bool TryApply(GameSession game, string text, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Save text is empty.";
                return false;
            }

            int version = 0;
            int money = 0;
            var txs = new List<LedgerLine>();
            int heat = 0;
            int hacker = 0;
            int underground = 0;
            int corporate = 0;
            int intelligence = 0;
            int[] xp = new int[9];
            int cyber = 0;
            int skillPoints = 0;
            int techPoints = 0;
            int knowledge = 0;
            List<string> abilities = new List<string>();
            int ram = 4;
            int cpu = 0;
            int storage = 128;
            int net = 0;
            int windows = 3;
            List<string> upgrades = new List<string>();
            List<string> done = new List<string>();
            List<string> flags = new List<string>();
            List<string> nodes = new List<string>();
            List<string> links = new List<string>();
            int cool = 0;
            var news = new List<NewsItem>();
            bool sawVersion = false;

            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim('\r').Trim();
                if (line.Length == 0)
                    continue;
                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    error = "That file is not a Nexus save.";
                    return false;
                }

                string key = line.Substring(0, eq);
                string value = line.Substring(eq + 1);
                if (key == "v")
                {
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out version))
                    {
                        error = "That file is not a Nexus save.";
                        return false;
                    }
                    sawVersion = true;
                }
                else if (key == "money") money = Int(value);
                else if (key == "tx")
                {
                    int bar = value.IndexOf('|');
                    if (bar > 0)
                        txs.Add(new LedgerLine { Amount = Int(value.Substring(0, bar)), Label = value.Substring(bar + 1) });
                }
                else if (key == "heat") heat = Int(value);
                else if (key == "rep")
                {
                    var parts = value.Split(',');
                    if (parts.Length >= 4)
                    {
                        hacker = Int(parts[0]);
                        underground = Int(parts[1]);
                        corporate = Int(parts[2]);
                        intelligence = Int(parts[3]);
                    }
                }
                else if (key == "xp") xp = Ints(value, 9);
                else if (key == "prog")
                {
                    var parts = value.Split(',');
                    if (parts.Length >= 4)
                    {
                        cyber = Int(parts[0]);
                        skillPoints = Int(parts[1]);
                        techPoints = Int(parts[2]);
                        knowledge = Int(parts[3]);
                    }
                }
                else if (key == "abilities") abilities = Split(value);
                else if (key == "hw")
                {
                    var parts = value.Split(',');
                    if (parts.Length >= 5)
                    {
                        ram = Int(parts[0]);
                        cpu = Int(parts[1]);
                        storage = Int(parts[2]);
                        net = Int(parts[3]);
                        windows = Int(parts[4]);
                    }
                }
                else if (key == "upgrades") upgrades = Split(value);
                else if (key == "done") done = Split(value);
                else if (key == "flags") flags = Split(value);
                else if (key == "nodes") nodes = Split(value);
                else if (key == "links") links = Split(value);
                else if (key == "cool") cool = Int(value);
                else if (key == "news")
                {
                    var parts = value.Split('\t');
                    if (parts.Length >= 3)
                    {
                        news.Add(new NewsItem
                        {
                            Id = Unescape(parts[0]),
                            Headline = Unescape(parts[1]),
                            Body = Unescape(parts[2])
                        });
                    }
                }
            }

            if (!sawVersion || version != CurrentVersion)
            {
                error = "This save is from a different build.";
                return false;
            }

            game.Player.RestoreMoney(money);
            game.Player.RestoreLedger(txs);
            game.Heat.Restore(heat);
            game.Reputation.Restore(hacker, underground, corporate, intelligence);
            game.Skills.Restore(xp);
            game.Progress.Restore(cyber, skillPoints, techPoints, knowledge, abilities);
            game.Hardware.Restore(ram, cpu, storage, net, windows, upgrades);
            game.Missions.Restore(done);
            game.Osint.Restore(nodes, links);
            game.ImportFlags(flags);
            game.News.KeepBaselineAnd(game.World, news);
            game.RestoreCoolUses(cool);
            return true;
        }

        static void Line(StringBuilder sb, string key, string value)
        {
            sb.Append(key);
            sb.Append('=');
            sb.Append(value ?? "");
            sb.Append('\n');
        }

        static string Join(IEnumerable<string> values)
        {
            var sb = new StringBuilder();
            bool any = false;
            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value))
                    continue;
                if (any)
                    sb.Append(';');
                sb.Append(value);
                any = true;
            }
            return sb.ToString();
        }

        static string JoinInts(int[] values)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(values[i].ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        static List<string> Split(string value)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(value))
                return list;
            var parts = value.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    list.Add(parts[i]);
            }
            return list;
        }

        static int Int(string value)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                return 0;
            return parsed;
        }

        static int[] Ints(string value, int size)
        {
            var result = new int[size];
            var parts = value.Split(',');
            int n = parts.Length < size ? parts.Length : size;
            for (int i = 0; i < n; i++)
                result[i] = Int(parts[i]);
            return result;
        }

        static string Escape(string value)
        {
            if (value == null)
                return "";
            return value.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "");
        }

        static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            var sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\\' && i + 1 < value.Length)
                {
                    char n = value[i + 1];
                    if (n == 'n') { sb.Append('\n'); i++; continue; }
                    if (n == 't') { sb.Append('\t'); i++; continue; }
                    if (n == '\\') { sb.Append('\\'); i++; continue; }
                }
                sb.Append(value[i]);
            }
            return sb.ToString();
        }
    }
}
