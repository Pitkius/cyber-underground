using System.Collections.Generic;
using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class SkillNode
    {
        public string Id;
        public string Title;
        public SkillBranch Branch;
        public int NeedLevel;
        public string Unlocks;
    }

    public static class ProgressCatalog
    {
        public static SkillNode[] Nodes()
        {
            return new[]
            {
                new SkillNode { Id = "osint_footprint", Title = "Digital Footprint", Branch = SkillBranch.Osint, NeedLevel = 2, Unlocks = "Connect public records in Notes. The terminal command connect is the action." },
                new SkillNode { Id = "osint_map", Title = "Relationship Mapping", Branch = SkillBranch.Osint, NeedLevel = 3, Unlocks = "Read a token rumor with analyze. It does not tell you whether the post is true." },
                new SkillNode { Id = "social_inbox", Title = "Signal Recognition", Branch = SkillBranch.Social, NeedLevel = 1, Unlocks = "Grade a training inbox once Northline posts the contract." },
                new SkillNode { Id = "opsec_notes", Title = "Basic Privacy", Branch = SkillBranch.Opsec, NeedLevel = 1, Unlocks = "A campus kiosk can clear local notes. People who already copied you still have them." },
                new SkillNode { Id = "defense_logs", Title = "Log Reading", Branch = SkillBranch.Defense, NeedLevel = 1, Unlocks = "Later incidents against this laptop can be read instead of only felt." }
            };
        }
    }

    public sealed class ProgressSystem
    {
        readonly HashSet<string> _abilities = new HashSet<string>();

        public int CyberXp { get; private set; }
        public int SkillPoints { get; private set; }
        public int TechPoints { get; private set; }
        public int Knowledge { get; private set; }

        public int Level
        {
            get { return 1 + CyberXp / 100; }
        }

        public IEnumerable<string> UnlockedIds()
        {
            return _abilities;
        }

        public bool Has(string id)
        {
            return _abilities.Contains(id);
        }

        public string Grant(int cyberXp, int skillPoints, int knowledge)
        {
            if (cyberXp > 0)
                CyberXp += cyberXp;
            if (skillPoints > 0)
                SkillPoints += skillPoints;
            if (knowledge > 0)
                Knowledge += knowledge;
            var sb = new StringBuilder();
            sb.Append("+");
            sb.Append(cyberXp);
            sb.Append(" Cyber XP");
            if (skillPoints > 0)
            {
                sb.Append("   +");
                sb.Append(skillPoints);
                sb.Append(" skill point");
            }
            if (knowledge > 0)
            {
                sb.Append("   knowledge ");
                sb.Append(Knowledge);
            }
            sb.Append("\nOperator level ");
            sb.Append(Level);
            return sb.ToString();
        }

        public string TryUnlock(string id, SkillSystem skills)
        {
            var node = Find(id);
            if (node == null)
                return "That ability is not on the tree.";
            if (_abilities.Contains(id))
                return "You already have " + node.Title + ".";
            if (skills.Level(node.Branch) < node.NeedLevel)
                return node.Title + " needs " + node.Branch + " L" + node.NeedLevel + ".";
            if (SkillPoints < 1)
                return "No skill point. Finish a contract first.";
            SkillPoints--;
            _abilities.Add(id);
            return node.Title + "\n" + node.Unlocks;
        }

        public string NextGoals(SkillSystem skills, int money)
        {
            var sb = new StringBuilder();
            sb.Append("NEXT\n");
            if (skills.Level(SkillBranch.Osint) < 2)
                sb.Append("OSINT L2  connect public records\n");
            else if (!_abilities.Contains("osint_footprint") && SkillPoints > 0)
                sb.Append("Spend a skill point on Digital Footprint\n");
            if (skills.Level(SkillBranch.Osint) < 3)
                sb.Append("OSINT L3  analyze a public rumor\n");
            if (money < 45)
                sb.Append("€45  used 8 GB RAM, parts shop\n");
            if (Knowledge < 2)
                sb.Append("Knowledge 2  a hint on the staging page\n");
            else
                sb.Append("Knowledge is high enough for the staging hint\n");
            return sb.ToString().TrimEnd();
        }

        public void Restore(int cyber, int skillPoints, int techPoints, int knowledge, IEnumerable<string> abilities)
        {
            CyberXp = cyber < 0 ? 0 : cyber;
            SkillPoints = skillPoints < 0 ? 0 : skillPoints;
            TechPoints = techPoints < 0 ? 0 : techPoints;
            Knowledge = knowledge < 0 ? 0 : knowledge;
            _abilities.Clear();
            if (abilities == null)
                return;
            foreach (var id in abilities)
            {
                if (!string.IsNullOrEmpty(id))
                    _abilities.Add(id);
            }
        }

        static SkillNode Find(string id)
        {
            var nodes = ProgressCatalog.Nodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Id == id)
                    return nodes[i];
            }
            return null;
        }
    }
}
