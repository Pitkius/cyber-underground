using System.Collections.Generic;

namespace CyberUnderground.Simulation
{
    public enum RepKind
    {
        Hacker = 0,
        Underground = 1,
        Corporate = 2,
        Intelligence = 3
    }

    public enum SkillBranch
    {
        Osint = 0,
        Network = 1,
        Security = 2,
        Social = 3,
        Crypto = 4,
        Opsec = 5,
        Programming = 6,
        Intelligence = 7
    }

    public enum MissionKind
    {
        TurnIn = 0,
        Choice = 1,
        Quiz = 2
    }

    public sealed class CommandResult
    {
        public bool Ok;
        public string Text;
        public bool ClearScreen;
        public bool RequestSave;
        public bool RequestLoad;

        public static CommandResult Success(string text)
        {
            return new CommandResult { Ok = true, Text = text ?? "" };
        }

        public static CommandResult Fail(string text)
        {
            return new CommandResult { Ok = false, Text = text ?? "" };
        }
    }

    public sealed class CompanyDef
    {
        public string Id;
        public string Name;
        public string Blurb;
        public string[] Aliases;
        public string NodeId;
        public string ScanReport;
    }

    public struct WebLink
    {
        public string Label;
        public string Url;

        public WebLink(string label, string url)
        {
            Label = label;
            Url = url;
        }
    }

    public sealed class WebPage
    {
        public string Url;
        public string Title;
        public string Body;
        public WebLink[] Links;
        public string[] Reveals;
        public string QuizId;
    }

    public sealed class OsintNode
    {
        public string Id;
        public string Title;
        public string Kind;
        public string Fact;
        public string RequiresKnown;
        public string[] Keywords;
    }

    public sealed class OsintEdge
    {
        public string A;
        public string B;
        public bool RequiredForFootprint;
    }

    public sealed class MissionDef
    {
        public string Id;
        public string Title;
        public string Client;
        public int ListedPay;
        public string Summary;
        public string Objectives;
        public string[] Requires;
        public MissionKind Kind;
    }

    public sealed class UpgradeDef
    {
        public string Id;
        public string Name;
        public int Cost;
        public string Detail;
        public bool Purchasable;
    }

    public sealed class NewsItem
    {
        public string Id;
        public string Headline;
        public string Body;
    }

    public sealed class QuizOption
    {
        public string Id;
        public string Title;
        public string Body;
    }

    public sealed class QuizDef
    {
        public string Id;
        public string Prompt;
        public string CorrectId;
        public string Success;
        public string Hint;
        public QuizOption[] Options;
    }

    public sealed class WorldData
    {
        public CompanyDef[] Companies;
        public WebPage[] Pages;
        public OsintNode[] Nodes;
        public OsintEdge[] Edges;
        public MissionDef[] Missions;
        public UpgradeDef[] Upgrades;
        public NewsItem[] StartingNews;
        public QuizDef[] Quizzes;

        public IEnumerable<string> AuditStrings()
        {
            var list = new List<string>();
            AddCompanies(list);
            AddPages(list);
            AddNodes(list);
            AddMissions(list);
            AddUpgrades(list);
            AddNews(list);
            AddQuizzes(list);
            return list;
        }

        void AddCompanies(List<string> list)
        {
            if (Companies == null) return;
            for (int i = 0; i < Companies.Length; i++)
            {
                var c = Companies[i];
                list.Add(c.Name);
                list.Add(c.Blurb);
                list.Add(c.ScanReport);
            }
        }

        void AddPages(List<string> list)
        {
            if (Pages == null) return;
            for (int i = 0; i < Pages.Length; i++)
            {
                list.Add(Pages[i].Title);
                list.Add(Pages[i].Body);
            }
        }

        void AddNodes(List<string> list)
        {
            if (Nodes == null) return;
            for (int i = 0; i < Nodes.Length; i++)
            {
                list.Add(Nodes[i].Title);
                list.Add(Nodes[i].Fact);
            }
        }

        void AddMissions(List<string> list)
        {
            if (Missions == null) return;
            for (int i = 0; i < Missions.Length; i++)
            {
                list.Add(Missions[i].Title);
                list.Add(Missions[i].Summary);
                list.Add(Missions[i].Objectives);
            }
        }

        void AddUpgrades(List<string> list)
        {
            if (Upgrades == null) return;
            for (int i = 0; i < Upgrades.Length; i++)
            {
                list.Add(Upgrades[i].Name);
                list.Add(Upgrades[i].Detail);
            }
        }

        void AddNews(List<string> list)
        {
            if (StartingNews == null) return;
            for (int i = 0; i < StartingNews.Length; i++)
            {
                list.Add(StartingNews[i].Headline);
                list.Add(StartingNews[i].Body);
            }
        }

        void AddQuizzes(List<string> list)
        {
            if (Quizzes == null) return;
            for (int i = 0; i < Quizzes.Length; i++)
            {
                var q = Quizzes[i];
                list.Add(q.Prompt);
                list.Add(q.Success);
                list.Add(q.Hint);
                if (q.Options == null) continue;
                for (int j = 0; j < q.Options.Length; j++)
                {
                    list.Add(q.Options[j].Title);
                    list.Add(q.Options[j].Body);
                }
            }
        }
    }
}
