using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyberUnderground.Presentation
{
    public sealed class DesktopWindow
    {
        public string Id;
        public string Title;
        public GameObject RootObject;
        public RectTransform Root;
        public RectTransform Content;
        public bool Minimized;
        public bool Maximized;
        public Vector2 RestorePos;
        public Vector2 RestoreSize;
        public Action Refresh;
    }

    public sealed class WindowManager
    {
        readonly RectTransform _layer;
        readonly Canvas _canvas;
        readonly List<DesktopWindow> _windows = new List<DesktopWindow>();
        int _cascade;

        public WindowManager(RectTransform layer, Canvas canvas)
        {
            _layer = layer;
            _canvas = canvas;
        }

        public int OpenCount
        {
            get { return _windows.Count; }
        }

        public IReadOnlyList<DesktopWindow> Windows
        {
            get { return _windows; }
        }

        public DesktopWindow Find(string id)
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                if (_windows[i].Id == id)
                    return _windows[i];
            }
            return null;
        }

        public DesktopWindow Open(string id, string title, Vector2 size, int maxWindows, out string blocked)
        {
            blocked = null;
            var existing = Find(id);
            if (existing != null)
            {
                if (existing.Minimized)
                    Restore(existing);
                Focus(existing);
                return existing;
            }

            if (_windows.Count >= maxWindows)
            {
                blocked = "The laptop is out of memory. Close a window, or buy RAM.";
                return null;
            }

            var window = new DesktopWindow();
            window.Id = id;
            window.Title = title;
            window.Root = UIManager.Panel(_layer, UIManager.Window);
            window.Root.gameObject.name = title;
            window.RootObject = window.Root.gameObject;
            var tag = window.Root.gameObject.AddComponent<WindowTag>();
            tag.Id = id;

            var outline = window.Root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.06f);
            outline.effectDistance = new Vector2(1f, -1f);

            window.Root.anchorMin = new Vector2(0, 1);
            window.Root.anchorMax = new Vector2(0, 1);
            window.Root.pivot = new Vector2(0, 1);
            window.Root.sizeDelta = size;
            float x = 168 + (_cascade % 5) * 32;
            float y = 36 + (_cascade % 5) * 28;
            _cascade++;
            window.Root.anchoredPosition = new Vector2(x, -y);

            var titleBar = UIManager.Panel(window.Root, UIManager.Title);
            titleBar.anchorMin = new Vector2(0, 1);
            titleBar.anchorMax = new Vector2(1, 1);
            titleBar.pivot = new Vector2(0.5f, 1);
            titleBar.sizeDelta = new Vector2(0, 34);
            titleBar.anchoredPosition = Vector2.zero;

            var titleLabel = UIManager.Label(titleBar, title, 14, UIManager.Text, TextAnchor.MiddleLeft);
            UIManager.Stretch(titleLabel.rectTransform, 12, 0, 108, 0);

            var drag = titleBar.gameObject.AddComponent<WindowDrag>();
            drag.Target = window.Root;
            drag.Canvas = _canvas;
            var captured = window;
            drag.BeforeDrag = delegate
            {
                if (captured.Maximized)
                    RestoreFromMax(captured);
                Focus(captured);
            };

            AddChrome(titleBar, "–", 78, delegate { Minimize(captured); });
            AddChrome(titleBar, "□", 48, delegate { ToggleMax(captured); });
            AddChrome(titleBar, "×", 18, delegate { Close(captured); }, UIManager.Danger);

            window.Content = UIManager.Panel(window.Root, UIManager.Window);
            UIManager.Stretch(window.Content, 0, 34, 0, 0);

            var grip = UIManager.Panel(window.Root, UIManager.Line);
            grip.anchorMin = new Vector2(1, 0);
            grip.anchorMax = new Vector2(1, 0);
            grip.pivot = new Vector2(1, 0);
            grip.sizeDelta = new Vector2(16, 16);
            grip.anchoredPosition = Vector2.zero;
            var resize = grip.gameObject.AddComponent<WindowResize>();
            resize.Target = window.Root;
            resize.Canvas = _canvas;

            _windows.Add(window);
            Focus(window);
            return window;
        }

        public void FocusFromPointer()
        {
            if (EventSystem.current == null)
                return;
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            for (int i = 0; i < hits.Count; i++)
            {
                var tag = hits[i].gameObject.GetComponentInParent<WindowTag>();
                if (tag == null)
                    continue;
                var window = Find(tag.Id);
                if (window != null)
                    Focus(window);
                break;
            }
        }

        public void CloseFocused()
        {
            if (_windows.Count == 0)
                return;
            Close(_windows[_windows.Count - 1]);
        }

        public void Focus(DesktopWindow window)
        {
            if (window == null || window.Root == null)
                return;
            window.Root.SetAsLastSibling();
            _windows.Remove(window);
            _windows.Add(window);
        }

        public void Minimize(DesktopWindow window)
        {
            window.Minimized = true;
            window.RootObject.SetActive(false);
        }

        public void Restore(DesktopWindow window)
        {
            window.Minimized = false;
            window.RootObject.SetActive(true);
            Focus(window);
        }

        public void Close(DesktopWindow window)
        {
            _windows.Remove(window);
            if (window.RootObject != null)
                UnityEngine.Object.Destroy(window.RootObject);
        }

        void ToggleMax(DesktopWindow window)
        {
            if (!window.Maximized)
            {
                window.RestorePos = window.Root.anchoredPosition;
                window.RestoreSize = window.Root.sizeDelta;
                window.Maximized = true;
                window.Root.anchorMin = Vector2.zero;
                window.Root.anchorMax = Vector2.one;
                window.Root.offsetMin = Vector2.zero;
                window.Root.offsetMax = Vector2.zero;
            }
            else
            {
                RestoreFromMax(window);
            }
            Focus(window);
        }

        void RestoreFromMax(DesktopWindow window)
        {
            if (!window.Maximized)
                return;
            window.Maximized = false;
            window.Root.anchorMin = new Vector2(0, 1);
            window.Root.anchorMax = new Vector2(0, 1);
            window.Root.pivot = new Vector2(0, 1);
            window.Root.sizeDelta = window.RestoreSize;
            window.Root.anchoredPosition = window.RestorePos;
        }

        void AddChrome(RectTransform title, string glyph, float fromRight, Action click, Color? color = null)
        {
            var button = UIManager.MakeButton(title, glyph, click, color ?? UIManager.ButtonBg);
            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(26, 24);
            rt.anchoredPosition = new Vector2(-fromRight, 0);
            var element = button.GetComponent<LayoutElement>();
            if (element != null)
                element.ignoreLayout = true;
        }
    }
}
