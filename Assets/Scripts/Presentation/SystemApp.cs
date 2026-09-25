using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class SystemApp
    {
        RectTransform _content;
        GameManager _shell;
        string _note = "Saves stay on this machine.";

        public void Mount(RectTransform parent, GameManager shell)
        {
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
            var body = UIManager.Label(_content,
                "Nexus OS 0.1\n\nThis desktop is a fictional simulation. The terminal never reaches a real network.\n\n" + _note,
                14, UIManager.Text, TextAnchor.UpperLeft);
            var bodyEl = body.gameObject.AddComponent<LayoutElement>();
            bodyEl.preferredHeight = 110;

            UIManager.MakeButton(_content, "Save", delegate
            {
                _note = _shell.SaveToDisk();
                _shell.MarkDirty();
            }, UIManager.Accent);
            UIManager.MakeButton(_content, "Load", delegate
            {
                _note = _shell.LoadFromDisk();
                _shell.MarkDirty();
            }, UIManager.ButtonBg);
            UIManager.MakeButton(_content, "New session", delegate
            {
                _shell.Session.ResetNew();
                _note = "Started over at €17.";
                _shell.MarkDirty();
            }, UIManager.Danger);
        }
    }
}
