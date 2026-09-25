using System.Collections.Generic;
using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class MissionSystem
    {
        readonly MissionDef[] _missions;
        readonly HashSet<string> _done;

        public MissionSystem(WorldData world)
        {
            _missions = world.Missions ?? new MissionDef[0];
            _done = new HashSet<string>();
        }

        public MissionDef Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            id = id.ToLowerInvariant();
            for (int i = 0; i < _missions.Length; i++)
            {
                if (_missions[i].Id == id)
                    return _missions[i];
            }
            return null;
        }

        public bool IsComplete(string id)
        {
            return _done.Contains(id);
        }

        public bool IsAvailable(string id)
        {
            var mission = Find(id);
            if (mission == null || _done.Contains(id))
                return false;
            if (mission.Requires == null)
                return true;
            for (int i = 0; i < mission.Requires.Length; i++)
            {
                if (!_done.Contains(mission.Requires[i]))
                    return false;
            }
            return true;
        }

        public void MarkComplete(string id)
        {
            _done.Add(id);
        }

        public IEnumerable<string> CompletedIds()
        {
            return _done;
        }

        public void Restore(IEnumerable<string> completed)
        {
            _done.Clear();
            if (completed == null)
                return;
            foreach (var id in completed)
            {
                if (Find(id) != null)
                    _done.Add(id);
            }
        }

        public string BoardText()
        {
            var sb = new StringBuilder();
            sb.Append("JOB BOARD");
            for (int i = 0; i < _missions.Length; i++)
            {
                var m = _missions[i];
                sb.Append("\n\n");
                sb.Append(m.Title);
                sb.Append("  [");
                sb.Append(m.Id);
                sb.Append("]\n");
                if (IsComplete(m.Id))
                    sb.Append("Status: done");
                else if (IsAvailable(m.Id))
                    sb.Append("Status: open");
                else
                    sb.Append("Status: locked");
                sb.Append("\nClient: ");
                sb.Append(m.Client);
                sb.Append("\n");
                sb.Append(m.Summary);
            }
            return sb.ToString();
        }

        public MissionDef[] All()
        {
            return _missions;
        }
    }
}
