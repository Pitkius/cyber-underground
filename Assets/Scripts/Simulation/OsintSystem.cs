using System.Collections.Generic;

namespace CyberUnderground.Simulation
{
    public sealed class OsintSystem
    {
        readonly Dictionary<string, OsintNode> _nodes;
        readonly HashSet<string> _known;
        readonly HashSet<string> _links;
        readonly List<OsintEdge> _truth;

        public OsintSystem(WorldData world)
        {
            _nodes = new Dictionary<string, OsintNode>();
            _known = new HashSet<string>();
            _links = new HashSet<string>();
            _truth = new List<OsintEdge>();
            if (world.Nodes != null)
            {
                for (int i = 0; i < world.Nodes.Length; i++)
                    _nodes[world.Nodes[i].Id] = world.Nodes[i];
            }
            if (world.Edges != null)
            {
                for (int i = 0; i < world.Edges.Length; i++)
                    _truth.Add(world.Edges[i]);
            }
        }

        public bool IsKnown(string id)
        {
            return _known.Contains(id);
        }

        public OsintNode Get(string id)
        {
            OsintNode node;
            if (id != null && _nodes.TryGetValue(id, out node))
                return node;
            return null;
        }

        public void Reveal(string id)
        {
            if (id != null && _nodes.ContainsKey(id))
                _known.Add(id);
        }

        public bool CanDiscover(string id)
        {
            var node = Get(id);
            if (node == null)
                return false;
            if (string.IsNullOrEmpty(node.RequiresKnown))
                return true;
            return _known.Contains(node.RequiresKnown);
        }

        public IEnumerable<OsintNode> KnownNodes()
        {
            var list = new List<OsintNode>();
            foreach (var pair in _nodes)
            {
                if (_known.Contains(pair.Key))
                    list.Add(pair.Value);
            }
            list.Sort(delegate (OsintNode a, OsintNode b) { return string.CompareOrdinal(a.Id, b.Id); });
            return list;
        }

        public IEnumerable<string> KnownIds()
        {
            return _known;
        }

        public IEnumerable<string> LinkKeys()
        {
            return _links;
        }

        public string ResolveId(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;
            token = token.ToLowerInvariant();
            if (_nodes.ContainsKey(token))
                return token;

            string found = null;
            foreach (var pair in _nodes)
            {
                var node = pair.Value;
                if (node.Keywords == null)
                    continue;
                for (int i = 0; i < node.Keywords.Length; i++)
                {
                    if (node.Keywords[i] != token)
                        continue;
                    if (found != null && found != node.Id)
                        return null;
                    found = node.Id;
                }
            }
            return found;
        }

        public bool HasLink(string a, string b)
        {
            return _links.Contains(Key(a, b));
        }

        public bool IsTrueEdge(string a, string b)
        {
            string key = Key(a, b);
            for (int i = 0; i < _truth.Count; i++)
            {
                if (Key(_truth[i].A, _truth[i].B) == key)
                    return true;
            }
            return false;
        }

        public bool RequiredFootprintComplete()
        {
            for (int i = 0; i < _truth.Count; i++)
            {
                if (!_truth[i].RequiredForFootprint)
                    continue;
                if (!_links.Contains(Key(_truth[i].A, _truth[i].B)))
                    return false;
            }
            return true;
        }

        public int RequiredFootprintCount()
        {
            int n = 0;
            for (int i = 0; i < _truth.Count; i++)
            {
                if (_truth[i].RequiredForFootprint)
                    n++;
            }
            return n;
        }

        public int RequiredFootprintDone()
        {
            int n = 0;
            for (int i = 0; i < _truth.Count; i++)
            {
                if (!_truth[i].RequiredForFootprint)
                    continue;
                if (_links.Contains(Key(_truth[i].A, _truth[i].B)))
                    n++;
            }
            return n;
        }

        public bool TryLink(string a, string b)
        {
            if (!IsTrueEdge(a, b))
                return false;
            _links.Add(Key(a, b));
            return true;
        }

        public void Restore(IEnumerable<string> known, IEnumerable<string> links)
        {
            _known.Clear();
            _links.Clear();
            if (known != null)
            {
                foreach (var id in known)
                {
                    if (_nodes.ContainsKey(id))
                        _known.Add(id);
                }
            }
            if (links != null)
            {
                foreach (var link in links)
                {
                    if (!string.IsNullOrEmpty(link))
                        _links.Add(link);
                }
            }
        }

        public static string Key(string a, string b)
        {
            if (string.CompareOrdinal(a, b) <= 0)
                return a + "|" + b;
            return b + "|" + a;
        }

        public List<OsintNode> Match(string query, out bool blockedByParent, out bool ambiguous)
        {
            blockedByParent = false;
            ambiguous = false;
            var found = new List<OsintNode>();
            if (string.IsNullOrEmpty(query))
                return found;

            string id = ResolveId(query);
            if (id == null && KeywordHits(query) > 1)
            {
                ambiguous = true;
                return found;
            }

            if (id != null)
            {
                var node = Get(id);
                if (node != null)
                    found.Add(node);
                return found;
            }

            foreach (var pair in _nodes)
            {
                if (pair.Value.Keywords == null)
                    continue;
                for (int i = 0; i < pair.Value.Keywords.Length; i++)
                {
                    if (pair.Value.Keywords[i] == query)
                    {
                        found.Add(pair.Value);
                        break;
                    }
                }
            }

            if (found.Count == 0)
            {
                foreach (var pair in _nodes)
                {
                    if (pair.Key == query)
                        found.Add(pair.Value);
                }
            }

            if (found.Count == 0)
                blockedByParent = false;
            return found;
        }

        int KeywordHits(string query)
        {
            int hits = 0;
            foreach (var pair in _nodes)
            {
                if (pair.Value.Keywords == null)
                    continue;
                for (int i = 0; i < pair.Value.Keywords.Length; i++)
                {
                    if (pair.Value.Keywords[i] == query)
                    {
                        hits++;
                        break;
                    }
                }
            }
            return hits;
        }
    }
}
