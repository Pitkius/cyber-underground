using System;
using UnityEngine;
using UnityEngine.UI;

namespace CyberUnderground.Presentation
{
    public static class UIManager
    {
        public static readonly Color Wallpaper = Hex(0x070b14);
        public static readonly Color Taskbar = Hex(0x0a101c);
        public static readonly Color Window = Hex(0x16243a);
        public static readonly Color Title = Hex(0x1e2d48);
        public static readonly Color Input = Hex(0x0c1422);
        public static readonly Color Text = Hex(0xe7eefc);
        public static readonly Color Muted = Hex(0x8ea0bd);
        public static readonly Color Accent = Hex(0x3d8bfd);
        public static readonly Color Money = Hex(0x3dd68c);
        public static readonly Color Ok = Hex(0x3dd68c);
        public static readonly Color Danger = Hex(0xff5d73);
        public static readonly Color ButtonBg = Hex(0x1a2740);
        public static readonly Color Line = Hex(0x243044);

        static Font _font;
        static Sprite _white;
        static Sprite _round;
        static Sprite _circle;

        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null)
                        _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                }
                return _white;
            }
        }

        public static Sprite Round
        {
            get
            {
                if (_round == null)
                    _round = MakeRound(64, 18);
                return _round;
            }
        }

        static Sprite MakeRound(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dx = 0f;
                    float dy = 0f;
                    if (px < r) dx = r - px;
                    else if (px > size - r) dx = px - (size - r);
                    if (py < r) dy = r - py;
                    else if (py > size - r) dy = py - (size - r);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(r - dist + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                    _circle = MakeCircle(64);
                return _circle;
            }
        }

        static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float radius = size * 0.5f;
            var center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite WallpaperSprite()
        {
            const int w = 640;
            const int h = 360;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                var top = Hex(0x16365f);
                var bottom = Hex(0x070910);
                var sky = Color.Lerp(bottom, top, t);
                for (int x = 0; x < w; x++)
                {
                    float nx = x / (float)(w - 1);
                    float glow = Mathf.Exp(-((nx - 0.78f) * (nx - 0.78f) + (t - 0.72f) * (t - 0.72f)) * 8f);
                    var pixel = sky + new Color(0.10f, 0.16f, 0.28f, 0f) * glow;
                    if (x % 32 == 0 || y % 32 == 0)
                        pixel = Color.Lerp(pixel, Hex(0x8fb4d8), 0.045f);
                    if (y < 46)
                        pixel = Color.Lerp(pixel, Hex(0x1d4d73), (46 - y) / 46f * 0.35f);
                    pixel.a = 1f;
                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite AppIcon(string id)
        {
            var draw = new IconDraw(160, Hex(0xe7eefc));
            switch (id)
            {
                case "guide":
                    draw.Ring(80, 80, 46, 5.5f);
                    draw.Line(80, 58, 80, 86, 6f);
                    draw.Disc(80, 104, 4.2f);
                    break;
                case "browser":
                    draw.Ring(80, 80, 48, 5.5f);
                    draw.ArcHorizontal(80, 80, 48, 5.5f);
                    draw.Line(80, 32, 80, 128, 5f);
                    draw.Line(36, 80, 124, 80, 5f);
                    break;
                case "terminal":
                    draw.RoundRect(28, 36, 104, 88, 5.5f);
                    draw.Line(48, 96, 68, 80, 5.5f);
                    draw.Line(68, 80, 48, 64, 5.5f);
                    draw.Line(78, 68, 108, 68, 5.5f);
                    break;
                case "notes":
                    draw.RoundRect(40, 28, 80, 108, 5.5f);
                    draw.Line(58, 108, 102, 108, 4.5f);
                    draw.Line(58, 92, 102, 92, 4.5f);
                    draw.Line(58, 76, 90, 76, 4.5f);
                    break;
                case "work":
                    draw.RoundRect(36, 40, 88, 84, 5.5f);
                    draw.Line(58, 40, 58, 28, 5.5f);
                    draw.Line(102, 40, 102, 28, 5.5f);
                    draw.Line(58, 28, 102, 28, 5.5f);
                    draw.Line(56, 96, 68, 84, 4.5f);
                    draw.Line(68, 84, 92, 108, 4.5f);
                    break;
                case "pc":
                    draw.RoundRect(28, 48, 104, 72, 5.5f);
                    draw.Line(80, 48, 80, 34, 5.5f);
                    draw.Line(58, 34, 102, 34, 5.5f);
                    break;
                case "helio":
                    draw.Line(48, 36, 48, 124, 8f);
                    draw.Line(112, 36, 112, 124, 8f);
                    draw.Line(48, 80, 112, 80, 8f);
                    break;
                case "bag":
                    draw.Line(46, 48, 114, 48, 6f);
                    draw.Line(46, 48, 46, 108, 6f);
                    draw.Line(114, 48, 114, 108, 6f);
                    draw.Line(46, 108, 114, 108, 6f);
                    draw.Line(62, 108, 62, 126, 6f);
                    draw.Line(98, 108, 98, 126, 6f);
                    draw.Line(62, 126, 98, 126, 6f);
                    break;
                case "ram":
                    draw.RoundRect(34, 46, 92, 68, 6f);
                    draw.Line(48, 80, 112, 80, 5f);
                    draw.Line(48, 64, 96, 64, 5f);
                    draw.Line(52, 46, 52, 32, 5f);
                    draw.Line(72, 46, 72, 32, 5f);
                    draw.Line(92, 46, 92, 32, 5f);
                    draw.Line(108, 46, 108, 32, 5f);
                    break;
                case "fiber":
                    draw.Disc(80, 48, 7f);
                    draw.Line(58, 70, 80, 92, 5.5f);
                    draw.Line(80, 92, 102, 70, 5.5f);
                    draw.Line(42, 88, 80, 122, 5.5f);
                    draw.Line(80, 122, 118, 88, 5.5f);
                    break;
                case "cpu":
                    draw.RoundRect(44, 44, 72, 72, 6f);
                    draw.RoundRect(62, 62, 36, 36, 5f);
                    draw.Line(58, 44, 58, 28, 5f);
                    draw.Line(80, 44, 80, 28, 5f);
                    draw.Line(102, 44, 102, 28, 5f);
                    draw.Line(58, 116, 58, 132, 5f);
                    draw.Line(80, 116, 80, 132, 5f);
                    draw.Line(102, 116, 102, 132, 5f);
                    break;
                case "tower":
                    draw.RoundRect(48, 24, 64, 112, 6f);
                    draw.Disc(80, 108, 6f);
                    draw.Line(64, 84, 96, 84, 5f);
                    draw.Line(64, 68, 96, 68, 5f);
                    break;
                default:
                    draw.Line(36, 108, 124, 108, 5.5f);
                    draw.Line(36, 80, 124, 80, 5.5f);
                    draw.Line(36, 52, 124, 52, 5.5f);
                    draw.Disc(96, 108, 6f);
                    draw.Disc(64, 80, 6f);
                    draw.Disc(108, 52, 6f);
                    break;
            }
            return draw.ToSprite();
        }

        sealed class IconDraw
        {
            readonly int _size;
            readonly Color[] _pixels;
            readonly Color _ink;

            public IconDraw(int size, Color ink)
            {
                _size = size;
                _ink = ink;
                _pixels = new Color[size * size];
            }

            public void Disc(float cx, float cy, float radius)
            {
                Stamp(delegate (float x, float y)
                {
                    return Cover(radius - Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)));
                });
            }

            public void Ring(float cx, float cy, float radius, float weight)
            {
                Stamp(delegate (float x, float y)
                {
                    float d = Mathf.Abs(Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) - radius);
                    return Cover(weight * 0.5f - d);
                });
            }

            public void Line(float x0, float y0, float x1, float y1, float weight)
            {
                Stamp(delegate (float x, float y)
                {
                    return Cover(weight * 0.5f - DistanceToSegment(x, y, x0, y0, x1, y1));
                });
            }

            public void RoundRect(float x, float y, float w, float h, float weight)
            {
                Line(x, y + h, x + w, y + h, weight);
                Line(x, y, x + w, y, weight);
                Line(x, y, x, y + h, weight);
                Line(x + w, y, x + w, y + h, weight);
            }

            public void ArcHorizontal(float cx, float cy, float radius, float weight)
            {
                Stamp(delegate (float x, float y)
                {
                    float dx = (x - cx) / 1.55f;
                    float dy = y - cy;
                    float d = Mathf.Abs(Mathf.Sqrt(dx * dx + dy * dy) - radius * 0.62f);
                    if (Mathf.Abs(x - cx) > radius * 0.72f)
                        return 0f;
                    return Cover(weight * 0.5f - d);
                });
            }

            public Sprite ToSprite()
            {
                var tex = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.SetPixels(_pixels);
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, _size, _size), new Vector2(0.5f, 0.5f), 100f);
            }

            void Stamp(Func<float, float, float> coverage)
            {
                for (int y = 0; y < _size; y++)
                {
                    for (int x = 0; x < _size; x++)
                    {
                        float a = coverage(x + 0.5f, y + 0.5f);
                        if (a <= 0f)
                            continue;
                        int i = y * _size + x;
                        var src = _pixels[i];
                        float outA = src.a + a * (1f - src.a);
                        _pixels[i] = new Color(_ink.r, _ink.g, _ink.b, outA);
                    }
                }
            }

            static float Cover(float signed)
            {
                return Mathf.Clamp01(signed + 0.8f);
            }

            static float DistanceToSegment(float px, float py, float x0, float y0, float x1, float y1)
            {
                float vx = x1 - x0;
                float vy = y1 - y0;
                float len = vx * vx + vy * vy;
                float t = len <= 0.0001f ? 0f : ((px - x0) * vx + (py - y0) * vy) / len;
                t = Mathf.Clamp01(t);
                float dx = px - (x0 + vx * t);
                float dy = py - (y0 + vy * t);
                return Mathf.Sqrt(dx * dx + dy * dy);
            }
        }

        public static Color Hex(int rgb)
        {
            float r = ((rgb >> 16) & 255) / 255f;
            float g = ((rgb >> 8) & 255) / 255f;
            float b = (rgb & 255) / 255f;
            return new Color(r, g, b, 1f);
        }

        public static RectTransform Stretch(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static RectTransform Panel(Transform parent, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = White;
            image.color = color;
            image.type = Image.Type.Simple;
            return go.GetComponent<RectTransform>();
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<Text>();
            label.font = Font;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        public static UnityEngine.UI.Button MakeButton(Transform parent, string text, Action onClick, Color bg)
        {
            var rt = Panel(parent, bg);
            rt.gameObject.name = "Button";
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = rt.GetComponent<Image>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;
            var label = Label(rt, text, 14, Text, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform, 8, 4, 8, 4);
            label.raycastTarget = false;
            button.onClick.AddListener(UiSound.Click);
            if (onClick != null)
                button.onClick.AddListener(delegate { onClick(); });
            var element = rt.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 32;
            element.preferredHeight = 32;
            return button;
        }

        public static InputField Field(Transform parent, string placeholder)
        {
            var rt = Panel(parent, Input);
            rt.gameObject.name = "Field";
            var input = rt.gameObject.AddComponent<InputField>();

            var text = Label(rt, "", 15, Text, TextAnchor.MiddleLeft);
            Stretch(text.rectTransform, 8, 4, 8, 4);
            text.supportRichText = false;
            text.raycastTarget = false;

            var ghost = Label(rt, placeholder, 15, Muted, TextAnchor.MiddleLeft);
            Stretch(ghost.rectTransform, 8, 4, 8, 4);
            ghost.fontStyle = FontStyle.Italic;
            ghost.raycastTarget = false;

            input.textComponent = text;
            input.placeholder = ghost;
            input.lineType = InputField.LineType.SingleLine;
            input.customCaretColor = true;
            input.caretColor = Text;
            int seen = 0;
            input.onValueChanged.AddListener(delegate (string value)
            {
                if (input.isFocused && value.Length != seen)
                    UiSound.Key();
                seen = value == null ? 0 : value.Length;
            });
            return input;
        }

        public static ScrollRect Scroll(Transform parent, out RectTransform content)
        {
            var root = Panel(parent, new Color(0, 0, 0, 0.01f));
            root.gameObject.name = "Scroll";
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            var viewport = Panel(root, new Color(0, 0, 0, 0.01f));
            viewport.gameObject.name = "Viewport";
            Stretch(viewport, 0, 0, 0, 0);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var body = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            body.transform.SetParent(viewport, false);
            content = body.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = new Vector2(0, content.offsetMin.y);
            content.offsetMax = new Vector2(0, content.offsetMax.y);

            var layout = body.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fit = body.GetComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = viewport;
            scroll.content = content;
            return scroll;
        }

        public static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
