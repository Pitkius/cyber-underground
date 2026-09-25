using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class MissionsApp
    {
        RectTransform _content;
        GameSession _game;
        GameManager _shell;
        string _status = "";

        public void Mount(RectTransform parent, GameSession game, GameManager shell)
        {
            _game = game;
            _shell = shell;
            var scroll = UIManager.Scroll(parent, out _content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            Refresh();
        }

        public void Refresh()
        {
            if (_content == null)
                return;
            UIManager.Clear(_content);

            var head = UIManager.Label(_content, "Posted contracts", 18, UIManager.Text, TextAnchor.UpperLeft);
            var headEl = head.gameObject.AddComponent<LayoutElement>();
            headEl.preferredHeight = 26;
            var intro = UIManager.Label(_content, "Only jobs a client actually posted. Personal choices stay on the website where they happen.", 14, UIManager.Muted, TextAnchor.UpperLeft);
            var introEl = intro.gameObject.AddComponent<LayoutElement>();
            introEl.preferredHeight = 40;

            if (!string.IsNullOrEmpty(_status))
            {
                var status = UIManager.Label(_content, _status, 14, UIManager.Money, TextAnchor.UpperLeft);
                var statusEl = status.gameObject.AddComponent<LayoutElement>();
                statusEl.preferredHeight = 48;
            }

            var missions = _game.Missions.All();
            for (int i = 0; i < missions.Length; i++)
            {
                var mission = missions[i];
                if (mission.Kind == MissionKind.Choice)
                    continue;
                Card(mission);
            }

            if (_game.Missions.IsAvailable("novamart_decision"))
                Hint("The NovaMart staging page is still public. Answer it there, not here.");
            if (_game.Missions.IsAvailable("nova_rumor"))
                Hint("Nightwire has a post you can answer on the board itself.");
        }

        void Card(MissionDef mission)
        {
            bool done = _game.Missions.IsComplete(mission.Id);
            bool open = _game.Missions.IsAvailable(mission.Id);
            var card = UIManager.Panel(_content, UIManager.Title);
            var cardEl = card.gameObject.AddComponent<LayoutElement>();
            cardEl.minHeight = open ? 168 : 96;
            cardEl.preferredHeight = open ? 176 : 104;

            string state = done ? "Paid" : open ? "Open" : "Later";
            var who = UIManager.Label(card, mission.Client + "   ·   " + state, 12, UIManager.Muted, TextAnchor.UpperLeft);
            who.rectTransform.anchorMin = new Vector2(0, 1);
            who.rectTransform.anchorMax = new Vector2(1, 1);
            who.rectTransform.pivot = new Vector2(0, 1);
            who.rectTransform.sizeDelta = new Vector2(-24, 18);
            who.rectTransform.anchoredPosition = new Vector2(12, -10);

            var title = UIManager.Label(card, mission.Title, 16, UIManager.Text, TextAnchor.UpperLeft);
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0, 1);
            title.rectTransform.sizeDelta = new Vector2(-110, 22);
            title.rectTransform.anchoredPosition = new Vector2(12, -30);

            if (mission.ListedPay > 0)
            {
                var pay = UIManager.Label(card, "€" + mission.ListedPay, 16, UIManager.Money, TextAnchor.UpperRight);
                pay.rectTransform.anchorMin = new Vector2(1, 1);
                pay.rectTransform.anchorMax = new Vector2(1, 1);
                pay.rectTransform.pivot = new Vector2(1, 1);
                pay.rectTransform.sizeDelta = new Vector2(72, 22);
                pay.rectTransform.anchoredPosition = new Vector2(-12, -30);
            }

            var body = UIManager.Label(card, open || done ? mission.Summary + "\n" + mission.Objectives : "Posted after you finish the campus survey.", 13, UIManager.Muted, TextAnchor.UpperLeft);
            body.rectTransform.anchorMin = new Vector2(0, 1);
            body.rectTransform.anchorMax = new Vector2(1, 1);
            body.rectTransform.pivot = new Vector2(0, 1);
            body.rectTransform.sizeDelta = new Vector2(-24, 52);
            body.rectTransform.anchoredPosition = new Vector2(12, -56);

            if (!open)
                return;

            if (mission.Id == "campus_survey" || mission.Id == "novamart_footprint")
            {
                string id = mission.Id;
                string label = mission.Id == "campus_survey" ? "Submit survey" : "Submit map";
                var button = UIManager.MakeButton(card, label, delegate
                {
                    _status = _game.TryTurnIn(id).Text;
                    _shell.MarkDirty();
                }, UIManager.Accent);
                Place(button, 12);
                return;
            }

            if (mission.Id == "helio_awareness")
            {
                var button = UIManager.MakeButton(card, "Open inbox", delegate
                {
                    _shell.OpenAddress("https://northline.security/inbox");
                }, UIManager.Accent);
                Place(button, 12);
            }
        }

        void Hint(string text)
        {
            var label = UIManager.Label(_content, text, 13, UIManager.Muted, TextAnchor.UpperLeft);
            var element = label.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 36;
        }

        static void Place(Button button, float x)
        {
            var rt = button.GetComponent<RectTransform>();
            var element = button.GetComponent<LayoutElement>();
            if (element != null)
                element.ignoreLayout = true;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(160, 32);
            rt.anchoredPosition = new Vector2(x, 12);
        }
    }
}
