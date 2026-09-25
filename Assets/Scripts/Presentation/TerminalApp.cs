using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class TerminalApp
    {
        Text _log;
        ScrollRect _scroll;
        InputField _input;
        GameSession _game;
        GameManager _shell;

        public void Mount(RectTransform parent, GameSession game, GameManager shell)
        {
            _game = game;
            _shell = shell;

            var root = parent;
            _scroll = UIManager.Scroll(root, out var content);
            UIManager.Stretch(_scroll.GetComponent<RectTransform>(), 0, 48, 0, 0);
            _log = UIManager.Label(content, "", 15, UIManager.Text, TextAnchor.UpperLeft);
            _log.rectTransform.sizeDelta = new Vector2(0, 0);

            var bar = UIManager.Panel(root, UIManager.Title);
            bar.anchorMin = new Vector2(0, 0);
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 44);
            bar.anchoredPosition = Vector2.zero;

            var prompt = UIManager.Label(bar, ">", 16, UIManager.Accent, TextAnchor.MiddleCenter);
            prompt.rectTransform.anchorMin = new Vector2(0, 0);
            prompt.rectTransform.anchorMax = new Vector2(0, 1);
            prompt.rectTransform.pivot = new Vector2(0, 0.5f);
            prompt.rectTransform.sizeDelta = new Vector2(28, 0);
            prompt.rectTransform.anchoredPosition = Vector2.zero;

            _input = UIManager.Field(bar, "help");
            var fieldRt = _input.GetComponent<RectTransform>();
            fieldRt.anchorMin = new Vector2(0, 0);
            fieldRt.anchorMax = new Vector2(1, 1);
            fieldRt.offsetMin = new Vector2(32, 6);
            fieldRt.offsetMax = new Vector2(-78, -6);

            var run = UIManager.MakeButton(bar, "Run", Submit, UIManager.Accent);
            var runRt = run.GetComponent<RectTransform>();
            runRt.anchorMin = new Vector2(1, 0.5f);
            runRt.anchorMax = new Vector2(1, 0.5f);
            runRt.pivot = new Vector2(1, 0.5f);
            runRt.sizeDelta = new Vector2(64, 30);
            runRt.anchoredPosition = new Vector2(-8, 0);

            _input.onSubmit.AddListener(delegate { Submit(); });
            Append("NEXUS TERM 0.1\nFictional operator console. Type help.");
        }

        public void FocusInput()
        {
            if (_input != null)
                _input.ActivateInputField();
        }

        public void Append(string text)
        {
            if (_log == null || string.IsNullOrEmpty(text))
                return;
            if (_log.text.Length > 0)
                _log.text += "\n\n";
            _log.text += text;
            Canvas.ForceUpdateCanvases();
            if (_scroll != null)
                _scroll.verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            if (_log != null)
                _log.text = "";
        }

        void Submit()
        {
            if (_input == null)
                return;
            string line = _input.text;
            _input.text = "";
            _input.ActivateInputField();
            if (string.IsNullOrEmpty(line))
                return;
            Append("> " + line);
            var result = _game.Execute(line);
            if (result.ClearScreen)
                Clear();
            if (!string.IsNullOrEmpty(result.Text))
                Append(result.Text);
            if (result.RequestSave)
                _shell.SaveToDisk();
            if (result.RequestLoad)
                _shell.LoadFromDisk();
        }
    }
}
