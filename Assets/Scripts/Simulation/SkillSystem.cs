using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class SkillSystem
    {
        public const int Level1Xp = 8;
        public const int Level2Xp = 30;
        public const int Level3Xp = 80;
        public const int Level4Xp = 150;
        public const int Level5Xp = 240;

        readonly int[] _xp;
        static readonly string[] BranchLabels =
        {
            "OSINT", "NETWORK", "SECURITY", "SOCIAL", "CRYPTO", "OPSEC", "PROGRAMMING", "INTELLIGENCE"
        };

        static readonly string[] OsintNames = { "Untrained", "Basic Search", "Identity Research", "Corporate Research", "Relationship Mapping", "Advanced Intelligence" };
        static readonly string[] NetworkNames = { "Untrained", "Address Literacy", "Service Recognition", "Path Reading", "Segmentation Sense", "Architecture Review" };
        static readonly string[] SecurityNames = { "Untrained", "Hygiene", "Authentication Basics", "Flaw Recognition", "Defense Review", "Security Leadership" };
        static readonly string[] SocialNames = { "Untrained", "Signal Recognition", "Pretext Awareness", "Trust Mapping", "Influence Judgment", "Campaign Reading" };
        static readonly string[] CryptoNames = { "Untrained", "Wallet Literacy", "Market Literacy", "Flow Reading", "Pattern Judgment", "Market Operations" };
        static readonly string[] OpsecNames = { "Untrained", "Footprint Awareness", "Account Separation", "Heat Control", "Cover Discipline", "Organization Security" };
        static readonly string[] ProgrammingNames = { "Untrained", "Script Reading", "Data Handling", "Tool Building", "Automation", "Platform Engineering" };
        static readonly string[] IntelligenceNames = { "Untrained", "Source Doubt", "Claim Checking", "Collection Planning", "Network Judgment", "Strategic Intelligence" };

        public SkillSystem()
        {
            _xp = new int[8];
            _xp[(int)SkillBranch.Osint] = Level1Xp;
        }

        public int Xp(SkillBranch branch)
        {
            return _xp[(int)branch];
        }

        public int Level(SkillBranch branch)
        {
            return LevelFor(_xp[(int)branch]);
        }

        public static int LevelFor(int xp)
        {
            if (xp >= Level5Xp) return 5;
            if (xp >= Level4Xp) return 4;
            if (xp >= Level3Xp) return 3;
            if (xp >= Level2Xp) return 2;
            if (xp >= Level1Xp) return 1;
            return 0;
        }

        public void Add(SkillBranch branch, int amount)
        {
            if (amount <= 0)
                return;
            _xp[(int)branch] += amount;
        }

        public void Restore(int[] xp)
        {
            for (int i = 0; i < _xp.Length; i++)
                _xp[i] = 0;
            if (xp == null)
                return;
            int n = xp.Length < _xp.Length ? xp.Length : _xp.Length;
            for (int i = 0; i < n; i++)
                _xp[i] = xp[i] < 0 ? 0 : xp[i];
        }

        public int[] ExportXp()
        {
            var copy = new int[_xp.Length];
            for (int i = 0; i < _xp.Length; i++)
                copy[i] = _xp[i];
            return copy;
        }

        public string RankName(SkillBranch branch)
        {
            int level = Level(branch);
            string[] names = NamesFor(branch);
            if (level < 0 || level >= names.Length)
                return "Untrained";
            return names[level];
        }

        public string Summary()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < BranchLabels.Length; i++)
            {
                var branch = (SkillBranch)i;
                if (i > 0)
                    sb.Append('\n');
                sb.Append(BranchLabels[i]);
                sb.Append("  L");
                sb.Append(Level(branch));
                sb.Append("  ");
                sb.Append(RankName(branch));
                sb.Append("  (");
                sb.Append(Xp(branch));
                sb.Append(" XP)");
            }
            return sb.ToString();
        }

        static string[] NamesFor(SkillBranch branch)
        {
            switch (branch)
            {
                case SkillBranch.Osint: return OsintNames;
                case SkillBranch.Network: return NetworkNames;
                case SkillBranch.Security: return SecurityNames;
                case SkillBranch.Social: return SocialNames;
                case SkillBranch.Crypto: return CryptoNames;
                case SkillBranch.Opsec: return OpsecNames;
                case SkillBranch.Programming: return ProgrammingNames;
                default: return IntelligenceNames;
            }
        }
    }
}
