using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class TerminalSystem
    {
        readonly GameSession _game;

        public TerminalSystem(GameSession game)
        {
            _game = game;
        }

        public CommandResult Execute(string line)
        {
            if (line == null)
                line = "";
            line = line.Trim();
            if (line.Length == 0)
                return CommandResult.Fail("Type help.");

            var parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            var verb = parts[0].ToLowerInvariant();
            if (verb.StartsWith("$"))
                verb = verb.Substring(1);

            switch (verb)
            {
                case "help":
                    return CommandResult.Success(Help());
                case "status":
                    return CommandResult.Success(_game.StatusText());
                case "skills":
                    return CommandResult.Success(_game.Skills.Summary());
                case "missions":
                case "jobs":
                    return CommandResult.Success(_game.Missions.BoardText());
                case "clear":
                    return new CommandResult { Ok = true, Text = "", ClearScreen = true };
                case "save":
                    return new CommandResult { Ok = true, Text = "Writing local save.", RequestSave = true };
                case "load":
                    return new CommandResult { Ok = true, Text = "Reading local save.", RequestLoad = true };
                case "scan":
                    if (parts.Length < 2)
                        return CommandResult.Fail("Usage: scan <name>");
                    return _game.Scan(parts[1]);
                case "lookup":
                case "search":
                    if (parts.Length < 2)
                        return CommandResult.Fail("Usage: lookup <name>");
                    return _game.Lookup(Join(parts, 1));
                case "inspect":
                    if (parts.Length < 2)
                        return CommandResult.Fail("Usage: inspect <id>");
                    return _game.Inspect(parts[1]);
                case "connect":
                    if (parts.Length < 3)
                        return CommandResult.Fail("Usage: connect <record> <record>");
                    return _game.Connect(parts[1], parts[2]);
                case "analyze":
                    if (parts.Length < 2)
                        return CommandResult.Fail("Usage: analyze nova");
                    return _game.Analyze(parts[1]);
                case "report":
                    if (parts.Length < 2)
                        return CommandResult.Fail("Usage: report <job-id>");
                    return _game.TryTurnIn(parts[1]);
                case "choose":
                    if (parts.Length < 3)
                        return CommandResult.Fail("Usage: choose <job-id> <option>");
                    return _game.Choose(parts[1], parts[2]);
                case "upgrade":
                    return _game.TryUpgrade(parts.Length < 2 ? null : parts[1]);
                case "cool":
                    return _game.CoolOff();
                case "download":
                    return CommandResult.Fail("Nothing is queued. No job has offered you a file.");
                case "decrypt":
                    return CommandResult.Fail("Decrypt is not on this laptop. Nobody has offered you a cipher job.");
                case "trace":
                    return CommandResult.Fail("Trace needs a network course you have not taken.");
                case "monitor":
                    return CommandResult.Fail("This laptop cannot stay awake long enough to monitor anything.");
                default:
                    return CommandResult.Fail("Unknown command. Type help.");
            }
        }

        string Help()
        {
            int osint = _game.Skills.Level(SkillBranch.Osint);
            var sb = new StringBuilder();
            sb.Append("NEXUS TERM\n");
            sb.Append("Fictional console. It only reads this simulation.\n\n");
            sb.Append("Available\n");
            sb.Append("  help status skills missions\n");
            sb.Append("  scan <name>\n");
            if (osint >= 1)
            {
                sb.Append("  lookup <name>\n");
                sb.Append("  inspect <id>\n");
            }
            if (osint >= 2)
                sb.Append("  connect <record> <record>\n");
            if (osint >= 3)
                sb.Append("  analyze nova\n");
            sb.Append("  report <job-id>\n");
            sb.Append("  choose <job-id> <option>\n");
            sb.Append("  upgrade [id]\n");
            sb.Append("  cool\n");
            sb.Append("  save load clear\n\n");
            sb.Append("Locked\n");
            if (osint < 2)
                sb.Append("  connect — OSINT 2 Identity Research\n");
            if (osint < 3)
                sb.Append("  analyze — OSINT 3 Corporate Research\n");
            sb.Append("  trace — NETWORK 1\n");
            sb.Append("  decrypt — no cipher job yet\n");
            sb.Append("  monitor — laptop cannot stay awake\n");
            sb.Append("  download — queue is empty");
            return sb.ToString();
        }

        static string Join(string[] parts, int start)
        {
            var sb = new StringBuilder();
            for (int i = start; i < parts.Length; i++)
            {
                if (i > start)
                    sb.Append(' ');
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }
    }
}
