using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class ProgressApp
    {
        RectTransform _content;
        GameSession _game;
        GameManager _shell;
        string _note = "";
        int _tab;

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
            var progress = _game.Progress;
            var head = UIManager.Label(_content, "Operator L" + progress.Level + "    Cyber XP " + progress.CyberXp + "    Skill points " + progress.SkillPoints + "    Knowledge " + progress.Knowledge, 15, UIManager.Text, TextAnchor.UpperLeft);
            var headEl = head.gameObject.AddComponent<LayoutElement>();
            headEl.preferredHeight = 36;

            Tab("Skills", 0);
            Tab("Knowledge", 1);
            Tab("Goals", 2);

            if (!string.IsNullOrEmpty(_note))
            {
                var note = UIManager.Label(_content, _note, 14, UIManager.Money, TextAnchor.UpperLeft);
                var noteEl = note.gameObject.AddComponent<LayoutElement>();
                noteEl.preferredHeight = 56;
            }

            if (_tab == 1)
            {
                Body(progress.Knowledge < 2
                    ? "Knowledge 0–1\nPages stay vague. The staging note does not explain who matters."
                    : "Knowledge 2\nThe NovaMart staging page now names the human layer. It still does not complete the map.");
                return;
            }

            if (_tab == 2)
            {
                Body(progress.NextGoals(_game.Skills, _game.Player.MoneyEuros));
                Body("Reputation stays off the desktop.\nCyber " + _game.Reputation.Get(RepKind.Hacker) + "   Underground " + _game.Reputation.Get(RepKind.Underground) + "   Corporate " + _game.Reputation.Get(RepKind.Corporate) + "   Intelligence " + _game.Reputation.Get(RepKind.Intelligence));
                return;
            }

            var nodes = ProgressCatalog.Nodes();
            for (int i = 0; i < nodes.Length; i++)
                Node(nodes[i]);
        }

        void Node(SkillNode node)
        {
            bool owned = _game.Progress.Has(node.Id);
            int level = _game.Skills.Level(node.Branch);
            string state = owned ? "Unlocked" : "Locked";
            var title = UIManager.Label(_content, node.Title + "   " + state, 16, owned ? UIManager.Money : UIManager.Text, TextAnchor.UpperLeft);
            var titleEl = title.gameObject.AddComponent<LayoutElement>();
            titleEl.preferredHeight = 22;
            Body(node.Branch + " L" + node.NeedLevel + "   you are L" + level + "\n" + node.Unlocks);
            if (owned)
                return;
            string id = node.Id;
            UIManager.MakeButton(_content, "Unlock", delegate
            {
                var result = _game.UnlockAbility(id);
                _note = result.Text;
                _shell.MarkDirty();
            }, UIManager.Accent);
        }

        void Tab(string label, int index)
        {
            int captured = index;
            UIManager.MakeButton(_content, label, delegate
            {
                _tab = captured;
                _shell.MarkDirty();
            }, _tab == index ? UIManager.Accent : UIManager.ButtonBg);
        }

        void Body(string text)
        {
            var label = UIManager.Label(_content, text, 14, UIManager.Muted, TextAnchor.UpperLeft);
            var element = label.gameObject.AddComponent<LayoutElement>();
            int lines = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    lines++;
            }
            element.preferredHeight = lines * 20 + 8;
        }
    }
}
