using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class ProfileApp
    {
        RectTransform _content;
        GameSession _game;

        public void Mount(RectTransform parent, GameSession game)
        {
            _game = game;
            var scroll = UIManager.Scroll(parent, out _content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            Refresh();
        }

        public void Refresh()
        {
            if (_content == null)
                return;
            UIManager.Clear(_content);
            TextBlock("Computer", _game.StatusText());
            TextBlock("Skills", _game.Skills.Summary());
        }

        void TextBlock(string title, string body)
        {
            var heading = UIManager.Label(_content, title, 16, UIManager.Accent, TextAnchor.UpperLeft);
            var head = heading.gameObject.AddComponent<LayoutElement>();
            head.minHeight = 22;
            head.preferredHeight = 24;
            var label = UIManager.Label(_content, body, 14, UIManager.Text, TextAnchor.UpperLeft);
            var element = label.gameObject.AddComponent<LayoutElement>();
            int lines = 1;
            for (int i = 0; i < body.Length; i++)
            {
                if (body[i] == '\n')
                    lines++;
            }
            element.minHeight = lines * 20;
            element.preferredHeight = lines * 20 + 8;
        }
    }
}
