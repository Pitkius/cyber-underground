namespace CyberUnderground.Simulation
{
    public sealed class BrowserSystem
    {
        readonly WebPage[] _pages;
        readonly QuizDef[] _quizzes;

        public BrowserSystem(WorldData world)
        {
            _pages = world.Pages ?? new WebPage[0];
            _quizzes = world.Quizzes ?? new QuizDef[0];
        }

        public static string Normalize(string url)
        {
            if (url == null)
                return "";
            url = url.Trim().ToLowerInvariant();
            if (url.StartsWith("https://"))
                url = url.Substring(8);
            else if (url.StartsWith("http://"))
                url = url.Substring(7);
            while (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);
            return url;
        }

        public WebPage Find(string url)
        {
            url = Normalize(url);
            for (int i = 0; i < _pages.Length; i++)
            {
                if (_pages[i].Url == url)
                    return _pages[i];
            }
            return null;
        }

        public WebPage Home()
        {
            return Find("nexus.search");
        }

        public QuizDef FindQuiz(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            for (int i = 0; i < _quizzes.Length; i++)
            {
                if (_quizzes[i].Id == id)
                    return _quizzes[i];
            }
            return null;
        }
    }
}
