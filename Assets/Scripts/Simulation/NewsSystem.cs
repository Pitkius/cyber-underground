using System.Collections.Generic;
using System.Text;

namespace CyberUnderground.Simulation
{
    public sealed class NewsSystem
    {
        readonly List<NewsItem> _items;

        public NewsSystem(WorldData world)
        {
            _items = new List<NewsItem>();
            if (world.StartingNews == null)
                return;
            for (int i = 0; i < world.StartingNews.Length; i++)
                _items.Add(world.StartingNews[i]);
        }

        public IReadOnlyList<NewsItem> Items
        {
            get { return _items; }
        }

        public void Publish(string id, string headline, string body)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                    return;
            }
            _items.Add(new NewsItem { Id = id, Headline = headline, Body = body });
        }

        public void Restore(IEnumerable<NewsItem> extras)
        {
            if (extras == null)
                return;
            foreach (var item in extras)
            {
                if (item == null || string.IsNullOrEmpty(item.Id))
                    continue;
                Publish(item.Id, item.Headline, item.Body);
            }
        }

        public void KeepBaselineAnd(WorldData world, IEnumerable<NewsItem> extras)
        {
            var baseline = new HashSet<string>();
            if (world != null && world.StartingNews != null)
            {
                for (int i = 0; i < world.StartingNews.Length; i++)
                    baseline.Add(world.StartingNews[i].Id);
            }

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (!baseline.Contains(_items[i].Id))
                    _items.RemoveAt(i);
            }

            Restore(extras);
        }

        public List<NewsItem> ExportExtras(WorldData world)
        {
            var baseline = new HashSet<string>();
            if (world.StartingNews != null)
            {
                for (int i = 0; i < world.StartingNews.Length; i++)
                    baseline.Add(world.StartingNews[i].Id);
            }

            var extras = new List<NewsItem>();
            for (int i = 0; i < _items.Count; i++)
            {
                if (!baseline.Contains(_items[i].Id))
                    extras.Add(_items[i]);
            }
            return extras;
        }

        public string FeedText()
        {
            var sb = new StringBuilder();
            sb.Append("DAILY GRID");
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                sb.Append("\n\n");
                sb.Append(_items[i].Headline);
                sb.Append("\n");
                sb.Append(_items[i].Body);
            }
            return sb.ToString();
        }
    }
}
