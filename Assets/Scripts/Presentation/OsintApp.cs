using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class OsintApp
    {
        RectTransform _content;
        GameSession _game;
        GameManager _shell;
        string _left = "";
        string _note = "Select two records, then link them.";

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
            var heading = UIManager.Label(_content, "Public records  " + _game.Osint.RequiredFootprintDone() + "/" + _game.Osint.RequiredFootprintCount(), 16, UIManager.Accent, TextAnchor.UpperLeft);
            var head = heading.gameObject.AddComponent<LayoutElement>();
            head.preferredHeight = 24;

            var note = UIManager.Label(_content, _note, 14, UIManager.Text, TextAnchor.UpperLeft);
            var noteEl = note.gameObject.AddComponent<LayoutElement>();
            noteEl.preferredHeight = 36;

            foreach (var node in _game.Osint.KnownNodes())
            {
                string id = node.Id;
                UIManager.MakeButton(_content, node.Kind + "  " + node.Title + "  [" + node.Id + "]", delegate { Pick(id); }, _left == id ? UIManager.Accent : UIManager.ButtonBg);
                var fact = UIManager.Label(_content, node.Fact, 13, UIManager.Muted, TextAnchor.UpperLeft);
                var factEl = fact.gameObject.AddComponent<LayoutElement>();
                factEl.preferredHeight = 32;
            }

            var hint = UIManager.Label(_content, "Click one record, then the record it supports.", 13, UIManager.Muted, TextAnchor.UpperLeft);
            var hintEl = hint.gameObject.AddComponent<LayoutElement>();
            hintEl.preferredHeight = 22;
        }

        void Pick(string id)
        {
            if (string.IsNullOrEmpty(_left))
            {
                _left = id;
                _note = "First record: " + id + ". Click the one it supports.";
                _shell.MarkDirty();
                return;
            }

            var result = _game.Connect(_left, id);
            _note = result.Text;
            _left = "";
            _shell.MarkDirty();
        }
    }
}
