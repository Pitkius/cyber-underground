using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public class GameManager : MonoBehaviour
    {
        GameSession _session;
        WindowManager _windows;
        Canvas _canvas;
        Text _clock;
        Text _note;
        Text _toast;
        RectTransform _taskApps;
        float _clockTimer;
        float _toastTimer;
        int _seenNotices;
        bool _dirty;

        TerminalApp _terminal;
        BrowserApp _browser;
        ProfileApp _profile;
        OsintApp _osint;
        MissionsApp _missions;
        SystemApp _system;
        GuideApp _guide;

        public GameSession Session
        {
            get { return _session; }
        }

        void Awake()
        {
            _session = GameSession.NewGame();
            _session.StateChanged += MarkDirty;
            _terminal = new TerminalApp();
            _browser = new BrowserApp();
            _profile = new ProfileApp();
            _osint = new OsintApp();
            _missions = new MissionsApp();
            _system = new SystemApp();
            _guide = new GuideApp();
            _guide.Changed = MarkDirty;
            BuildDesktop();
            RefreshHud();
            DrainNotices();
            OpenGuide();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && _windows != null)
                _windows.FocusFromPointer();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                if (selected == null || selected.GetComponent<InputField>() == null)
                    _windows.CloseFocused();
            }

            _clockTimer += Time.unscaledDeltaTime;
            if (_clockTimer >= 1f && _clock != null)
            {
                _clockTimer = 0f;
                _clock.text = DateTime.Now.ToString("HH:mm  dd MMM");
            }

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f && _toast != null)
                    _toast.transform.parent.gameObject.SetActive(false);
            }
        }

        void LateUpdate()
        {
            if (!_dirty)
                return;
            _dirty = false;
            RefreshHud();
            DrainNotices();
            RefreshOpenApps();
            RebuildTaskbar();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string SaveToDisk()
        {
            string path = SavePath();
            File.WriteAllText(path, _session.ExportSave());
            return "Saved to " + path;
        }

        public string LoadFromDisk()
        {
            string path = SavePath();
            if (!File.Exists(path))
                return "No save file yet.";
            string error;
            if (!_session.TryImportSave(File.ReadAllText(path), out error))
                return error;
            MarkDirty();
            return "Loaded.";
        }

        void BuildDesktop()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem");
                events.AddComponent<EventSystem>();
                events.AddComponent<StandaloneInputModule>();
            }

            var canvasGo = new GameObject("DesktopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;

            var wallpaper = UIManager.Panel(_canvas.transform, Color.white);
            UIManager.Stretch(wallpaper, 0, 0, 0, 0);
            var wallImage = wallpaper.GetComponent<Image>();
            wallImage.sprite = UIManager.WallpaperSprite();
            wallImage.type = Image.Type.Simple;
            wallImage.color = Color.white;

            var icons = UIManager.Panel(wallpaper, UIManager.Hex(0x0c1424));
            icons.gameObject.name = "Icons";
            UIManager.Stretch(icons, 10, 0, 0, 58);
            icons.anchorMax = new Vector2(0, 1);
            icons.offsetMax = new Vector2(86, 0);
            var iconLayout = icons.gameObject.AddComponent<VerticalLayoutGroup>();
            iconLayout.padding = new RectOffset(4, 4, 12, 8);
            iconLayout.spacing = 6;
            iconLayout.childControlWidth = true;
            iconLayout.childControlHeight = true;
            iconLayout.childForceExpandHeight = false;

            AddIcon(icons, "guide", "Guide", OpenGuide);
            AddIcon(icons, "browser", "Browser", OpenBrowser);
            AddIcon(icons, "terminal", "Terminal", OpenTerminal);
            AddIcon(icons, "notes", "Notes", OpenOsint);
            AddIcon(icons, "work", "Work", OpenJobs);
            AddIcon(icons, "pc", "This PC", OpenProfile);
            AddIcon(icons, "settings", "Settings", OpenSystem);

            var note = UIManager.Panel(wallpaper, UIManager.Title);
            note.anchorMin = new Vector2(1, 1);
            note.anchorMax = new Vector2(1, 1);
            note.pivot = new Vector2(1, 1);
            note.sizeDelta = new Vector2(300, 150);
            note.anchoredPosition = new Vector2(-28, -28);
            _note = UIManager.Label(note, "", 14, UIManager.Text, TextAnchor.UpperLeft);
            UIManager.Stretch(_note.rectTransform, 14, 12, 14, 12);

            var windows = UIManager.Panel(wallpaper, new Color(0, 0, 0, 0));
            windows.gameObject.name = "Windows";
            UIManager.Stretch(windows, 0, 0, 0, 52);
            windows.GetComponent<Image>().raycastTarget = false;
            _windows = new WindowManager(windows, _canvas);

            var toast = UIManager.Panel(wallpaper, UIManager.Title);
            toast.anchorMin = new Vector2(1, 1);
            toast.anchorMax = new Vector2(1, 1);
            toast.pivot = new Vector2(1, 1);
            toast.sizeDelta = new Vector2(420, 64);
            toast.anchoredPosition = new Vector2(-28, -190);
            _toast = UIManager.Label(toast, "", 14, UIManager.Text, TextAnchor.MiddleLeft);
            UIManager.Stretch(_toast.rectTransform, 12, 8, 12, 8);
            toast.gameObject.SetActive(false);

            var taskbar = UIManager.Panel(wallpaper, UIManager.Taskbar);
            taskbar.anchorMin = new Vector2(0, 0);
            taskbar.anchorMax = new Vector2(1, 0);
            taskbar.pivot = new Vector2(0.5f, 0);
            taskbar.sizeDelta = new Vector2(0, 48);
            taskbar.anchoredPosition = Vector2.zero;

            var brand = UIManager.Label(taskbar, "NEXUS", 14, UIManager.Accent, TextAnchor.MiddleCenter);
            brand.rectTransform.anchorMin = new Vector2(0, 0);
            brand.rectTransform.anchorMax = new Vector2(0, 1);
            brand.rectTransform.pivot = new Vector2(0, 0.5f);
            brand.rectTransform.sizeDelta = new Vector2(84, 0);

            _taskApps = UIManager.Panel(taskbar, new Color(0, 0, 0, 0));
            _taskApps.anchorMin = new Vector2(0, 0);
            _taskApps.anchorMax = new Vector2(1, 1);
            _taskApps.offsetMin = new Vector2(90, 6);
            _taskApps.offsetMax = new Vector2(-140, -6);
            var taskLayout = _taskApps.gameObject.AddComponent<HorizontalLayoutGroup>();
            taskLayout.spacing = 6;
            taskLayout.childControlWidth = false;
            taskLayout.childControlHeight = true;
            taskLayout.childForceExpandWidth = false;
            taskLayout.childForceExpandHeight = true;

            _clock = UIManager.Label(taskbar, DateTime.Now.ToString("HH:mm  dd MMM"), 13, UIManager.Muted, TextAnchor.MiddleRight);
            _clock.rectTransform.anchorMin = new Vector2(1, 0);
            _clock.rectTransform.anchorMax = new Vector2(1, 1);
            _clock.rectTransform.pivot = new Vector2(1, 0.5f);
            _clock.rectTransform.sizeDelta = new Vector2(110, 0);
            _clock.rectTransform.anchoredPosition = new Vector2(-8, 0);
        }

        void OpenTerminal()
        {
            Open("terminal", "Terminal", new Vector2(760, 480), delegate (RectTransform content)
            {
                _terminal.Mount(content, _session, this);
                _terminal.FocusInput();
            });
        }

        void OpenBrowser()
        {
            Open("browser", "Browser", new Vector2(860, 540), delegate (RectTransform content)
            {
                _browser.Mount(content, _session, this);
            });
        }

        public void OpenAddress(string url)
        {
            var existing = _windows.Find("browser");
            if (existing == null)
                OpenBrowser();
            else if (existing.Minimized)
                _windows.Restore(existing);
            else
                _windows.Focus(existing);
            _browser.Navigate(url);
            RebuildTaskbar();
        }

        void OpenGuide()
        {
            Open("guide", "Guide", new Vector2(520, 420), delegate (RectTransform content)
            {
                _guide.Mount(content);
            });
        }

        void OpenProfile()
        {
            Open("profile", "This PC", new Vector2(440, 520), delegate (RectTransform content)
            {
                _profile.Mount(content, _session);
            });
        }

        void OpenOsint()
        {
            Open("osint", "Notes", new Vector2(640, 520), delegate (RectTransform content)
            {
                _osint.Mount(content, _session, this);
            });
        }

        void OpenJobs()
        {
            Open("jobs", "Work", new Vector2(560, 540), delegate (RectTransform content)
            {
                _missions.Mount(content, _session, this);
            });
        }

        void OpenSystem()
        {
            Open("system", "Settings", new Vector2(440, 360), delegate (RectTransform content)
            {
                _system.Mount(content, this);
            });
        }

        void Open(string id, string title, Vector2 size, Action<RectTransform> mount)
        {
            var existing = _windows.Find(id);
            if (existing != null)
            {
                if (existing.Minimized)
                    _windows.Restore(existing);
                else
                    _windows.Focus(existing);
                RebuildTaskbar();
                return;
            }

            string blocked;
            var window = _windows.Open(id, title, size, _session.Hardware.MaxWindows, out blocked);
            if (window == null)
            {
                ShowToast(blocked);
                return;
            }
            mount(window.Content);
            RebuildTaskbar();
        }

        void RefreshOpenApps()
        {
            if (_windows.Find("guide") != null) _guide.Refresh();
            if (_windows.Find("profile") != null) _profile.Refresh();
            if (_windows.Find("osint") != null) _osint.Refresh();
            if (_windows.Find("jobs") != null) _missions.Refresh();
            if (_windows.Find("system") != null) _system.Refresh();
            if (_windows.Find("browser") != null) _browser.Refresh();
        }

        void RefreshHud()
        {
            _note.text = "STICKY\n\nTuition is still due.\nThe number is in Helio, not here.\n\nOpen Guide if you get lost.";
        }

        void DrainNotices()
        {
            var notices = _session.Notices;
            if (notices.Count < _seenNotices)
                _seenNotices = 0;
            while (_seenNotices < notices.Count)
            {
                ShowToast(notices[_seenNotices]);
                _seenNotices++;
            }
        }

        void ShowToast(string text)
        {
            if (string.IsNullOrEmpty(text) || _toast == null)
                return;
            _toast.text = text;
            _toast.transform.parent.gameObject.SetActive(true);
            _toastTimer = 4.5f;
        }

        void RebuildTaskbar()
        {
            UIManager.Clear(_taskApps);
            var open = _windows.Windows;
            for (int i = 0; i < open.Count; i++)
            {
                var window = open[i];
                var button = UIManager.MakeButton(_taskApps, window.Title, delegate
                {
                    if (window.Minimized)
                        _windows.Restore(window);
                    else
                        _windows.Focus(window);
                    RebuildTaskbar();
                }, window.Minimized ? UIManager.ButtonBg : UIManager.Accent);
                var element = button.GetComponent<LayoutElement>();
                element.minWidth = 96;
                element.preferredWidth = 110;
                element.minHeight = 32;
            }
        }

        void AddIcon(RectTransform parent, string mark, string label, Action open)
        {
            var button = UIManager.MakeButton(parent, "", open, new Color(0f, 0f, 0f, 0f));
            var element = button.GetComponent<LayoutElement>();
            element.minHeight = 62;
            element.preferredHeight = 62;
            button.GetComponent<Image>().sprite = UIManager.White;
            button.GetComponent<Image>().type = Image.Type.Simple;

            var badgeGo = new GameObject("Mark", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(button.transform, false);
            var badge = badgeGo.GetComponent<RectTransform>();
            badge.anchorMin = new Vector2(0.5f, 1);
            badge.anchorMax = new Vector2(0.5f, 1);
            badge.pivot = new Vector2(0.5f, 1);
            badge.sizeDelta = new Vector2(34, 34);
            badge.anchoredPosition = new Vector2(0, -2);
            var badgeImage = badgeGo.GetComponent<Image>();
            badgeImage.sprite = UIManager.AppIcon(mark);
            badgeImage.color = Color.white;
            badgeImage.preserveAspect = true;
            badgeImage.raycastTarget = false;

            var name = UIManager.Label(button.transform, label, 11, UIManager.Muted, TextAnchor.UpperCenter);
            name.rectTransform.anchorMin = new Vector2(0, 0);
            name.rectTransform.anchorMax = new Vector2(1, 0);
            name.rectTransform.pivot = new Vector2(0.5f, 0);
            name.rectTransform.sizeDelta = new Vector2(0, 16);
            name.rectTransform.anchoredPosition = new Vector2(0, 2);
        }

        static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, "nexus-save.json");
        }
    }
}
