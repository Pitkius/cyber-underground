using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class BrowserApp
    {
        static readonly Color Chrome = new Color(0.20f, 0.22f, 0.25f, 1f);
        static readonly Color Omnibox = new Color(0.11f, 0.12f, 0.14f, 1f);
        static readonly Color Page = new Color(0.96f, 0.97f, 0.98f, 1f);
        static readonly Color Ink = new Color(0.12f, 0.14f, 0.18f, 1f);
        static readonly Color Faint = new Color(0.35f, 0.40f, 0.48f, 1f);
        static readonly Color Link = new Color(0.10f, 0.45f, 0.91f, 1f);

        GameSession _game;
        GameManager _shell;
        bool _repaint;
        InputField _address;
        Text _tab;
        RectTransform _body;
        readonly List<string> _history = new List<string>();
        int _at = -1;
        string _url = "nexus.search";
        string _quizNote = "";
        ShopApp _shop;
        bool _helioIn;
        bool _signing;
        Coroutine _signRoutine;

        public void Mount(RectTransform parent, GameSession game, GameManager shell)
        {
            _game = game;
            _shell = shell;

            var chrome = UIManager.Panel(parent, Chrome);
            chrome.anchorMin = new Vector2(0, 1);
            chrome.anchorMax = new Vector2(1, 1);
            chrome.pivot = new Vector2(0.5f, 1);
            chrome.sizeDelta = new Vector2(0, 78);
            chrome.anchoredPosition = Vector2.zero;

            var tab = UIManager.Panel(chrome, new Color(0.27f, 0.29f, 0.33f, 1f));
            tab.anchorMin = new Vector2(0, 1);
            tab.anchorMax = new Vector2(0, 1);
            tab.pivot = new Vector2(0, 1);
            tab.sizeDelta = new Vector2(220, 30);
            tab.anchoredPosition = new Vector2(8, -4);
            _tab = UIManager.Label(tab, "New tab", 13, Color.white, TextAnchor.MiddleLeft);
            UIManager.Stretch(_tab.rectTransform, 12, 0, 12, 0);

            Tool(chrome, "<", 8, delegate { Back(); });
            Tool(chrome, ">", 40, delegate { Forward(); });
            Tool(chrome, "R", 72, delegate { Navigate(_url, false); });

            _address = UIManager.Field(chrome, "Search or type an address");
            var field = _address.GetComponent<RectTransform>();
            field.anchorMin = new Vector2(0, 0);
            field.anchorMax = new Vector2(1, 0);
            field.pivot = new Vector2(0.5f, 0);
            field.sizeDelta = new Vector2(-120, 32);
            field.anchoredPosition = new Vector2(52, 6);
            _address.text = "";
            var box = _address.GetComponent<Image>();
            box.color = Omnibox;
            _address.onSubmit.AddListener(delegate { Go(_address.text); });

            _body = UIManager.Panel(parent, Page);
            UIManager.Stretch(_body, 0, 78, 0, 0);
            Navigate("nexus.search", true);
        }

        public void Refresh()
        {
            if (_body == null)
                return;
            if (_signing && _url == "helio.bank/account")
                return;
            if (_repaint || _url == "dailygrid.news" || _url == "helio.bank/account" || _url == "parts.grayhaven.shop" || _url == "www.novamart.com/dev-notes" || _url == "nightwire.social")
            {
                _repaint = false;
                Render();
            }
        }

        public void Navigate(string url)
        {
            Navigate(url, true);
        }

        void Go(string text)
        {
            text = text == null ? "" : text.Trim();
            if (text.Length == 0)
                return;
            string url = Resolve(BrowserSystem.Normalize(text));
            if (_game.Browser.Find(url) != null || url == "nexus.search")
            {
                Navigate(url, true);
                return;
            }
            Navigate("search:" + text.ToLowerInvariant(), true);
        }

        void Navigate(string url, bool record)
        {
            StopSignIn();
            url = Resolve(BrowserSystem.Normalize(url));
            if (url.Length == 0)
                url = "nexus.search";
            if (url != "helio.bank/account")
                _helioIn = false;
            if (url.StartsWith("search:"))
            {
                _url = url;
            }
            else
            {
                _url = url;
                var page = _game.Visit(url);
                if (page == null && url != "nexus.search")
                    _quizNote = "missing";
                else
                    _quizNote = "";
            }

            if (record)
            {
                if (_at < _history.Count - 1)
                    _history.RemoveRange(_at + 1, _history.Count - _at - 1);
                if (_history.Count == 0 || _history[_history.Count - 1] != _url)
                    _history.Add(_url);
                _at = _history.Count - 1;
            }

            if (_address != null && !_url.StartsWith("search:"))
                _address.text = "https://" + _url;
            _repaint = true;
            if (_shell != null)
                _shell.MarkDirty();
            else
                Render();
        }

        void Back()
        {
            if (_at <= 0)
                return;
            _at--;
            Navigate(_history[_at], false);
        }

        void Forward()
        {
            if (_at >= _history.Count - 1)
                return;
            _at++;
            Navigate(_history[_at], false);
        }

        void Render()
        {
            UIManager.Clear(_body);
            _body.GetComponent<Image>().color = Page;

            if (_url == "nexus.search")
            {
                SetTab("New tab");
                RenderStart();
                return;
            }

            if (_url.StartsWith("search:"))
            {
                SetTab("Search");
                RenderSearch(_url.Substring(7));
                return;
            }

            var page = _game.Browser.Find(_url);
            SetTab(page == null ? "Not found" : page.Title);

            var scroll = UIManager.Scroll(_body, out var content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            scroll.GetComponent<Image>().color = _body.GetComponent<Image>().color;

            if (_url == "helio.bank/account")
            {
                if (_helioIn)
                    RenderBank(content);
                else
                {
                    Object.Destroy(scroll.gameObject);
                    RenderHelioGate();
                }
                return;
            }
            if (_url == "parts.grayhaven.shop")
            {
                RenderShop(content);
                return;
            }
            if (_url == "dailygrid.news")
            {
                SiteHead(content, "Daily Grid", "Grayhaven news");
                var items = _game.News.Items;
                for (int i = items.Count - 1; i >= 0; i--)
                    Article(content, items[i].Headline, items[i].Body);
                return;
            }
            if (page == null)
            {
                SiteHead(content, "Can't open that", _url);
                Paragraph(content, "Nothing on this computer uses that address.");
                TextLink(content, "Back to search", "nexus.search");
                return;
            }

            SiteHead(content, page.Title, page.Url);
            if (!string.IsNullOrEmpty(page.Body))
                Paragraph(content, page.Body);
            if (!string.IsNullOrEmpty(page.QuizId))
                RenderQuiz(content, _game.Browser.FindQuiz(page.QuizId));
            if (!string.IsNullOrEmpty(_quizNote) && _quizNote != "missing")
                Paragraph(content, _quizNote);
            if (page.Links != null)
            {
                for (int i = 0; i < page.Links.Length; i++)
                    TextLink(content, page.Links[i].Label, page.Links[i].Url);
            }
            if (_url == "www.novamart.com/dev-notes")
                RenderNoteChoice(content);
            if (_url == "nightwire.social")
                RenderRumorChoice(content);
        }

        void RenderStart()
        {
            var block = UIManager.Panel(_body, Page);
            block.anchorMin = new Vector2(0.5f, 0.5f);
            block.anchorMax = new Vector2(0.5f, 0.5f);
            block.pivot = new Vector2(0.5f, 0.5f);
            block.sizeDelta = new Vector2(560, 280);
            block.GetComponent<Image>().color = Page;

            var logo = UIManager.Label(block, "NEXUS", 54, new Color(0.16f, 0.45f, 0.93f, 1f), TextAnchor.MiddleCenter);
            logo.rectTransform.anchorMin = new Vector2(0, 1);
            logo.rectTransform.anchorMax = new Vector2(1, 1);
            logo.rectTransform.pivot = new Vector2(0.5f, 1);
            logo.rectTransform.sizeDelta = new Vector2(0, 70);
            logo.rectTransform.anchoredPosition = new Vector2(0, -8);

            var search = UIManager.Field(block, "Search the fictional web");
            var searchRt = search.GetComponent<RectTransform>();
            searchRt.anchorMin = new Vector2(0.5f, 1);
            searchRt.anchorMax = new Vector2(0.5f, 1);
            searchRt.pivot = new Vector2(0.5f, 1);
            searchRt.sizeDelta = new Vector2(460, 40);
            searchRt.anchoredPosition = new Vector2(0, -88);
            search.GetComponent<Image>().color = Color.white;
            var typed = search.textComponent;
            typed.color = Ink;
            search.onSubmit.AddListener(delegate { Go(search.text); });

            Shortcut(block, "Helio", "helio.bank/account", -180);
            Shortcut(block, "Parts", "parts.grayhaven.shop", -60);
            Shortcut(block, "Campus", "www.lumen.edu/it", 60);
            Shortcut(block, "News", "dailygrid.news", 180);
        }

        void RenderSearch(string query)
        {
            var scroll = UIManager.Scroll(_body, out var content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            SiteHead(content, "Results", query);
            int found = 0;
            var pages = _game.World.Pages;
            for (int i = 0; i < pages.Length; i++)
            {
                var page = pages[i];
                if (page.Url == "nexus.search")
                    continue;
                string hay = ((page.Title ?? "") + " " + (page.Body ?? "") + " " + page.Url).ToLowerInvariant();
                if (hay.IndexOf(query) < 0)
                    continue;
                found++;
                TextLink(content, page.Title + "  —  " + page.Url, page.Url);
            }
            if (found == 0)
                Paragraph(content, "No pages mention that. Try Helio, NovaMart, campus, or parts.");
        }

        void StopSignIn()
        {
            _signing = false;
            if (_signRoutine != null && _shell != null)
                _shell.StopCoroutine(_signRoutine);
            _signRoutine = null;
        }

        void RenderHelioGate()
        {
            var card = UIManager.Panel(_body, Color.white);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(440, 480);

            var stripe = UIManager.Panel(card, new Color(0.07f, 0.16f, 0.38f, 1f));
            stripe.anchorMin = new Vector2(0, 1);
            stripe.anchorMax = new Vector2(1, 1);
            stripe.pivot = new Vector2(0.5f, 1);
            stripe.sizeDelta = new Vector2(0, 8);
            stripe.anchoredPosition = Vector2.zero;

            Glyph(card, "helio", new Color(0.07f, 0.16f, 0.38f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -28), new Vector2(64, 64));

            var brand = UIManager.Label(card, "HELIO MUTUAL", 13, new Color(0.07f, 0.16f, 0.38f, 1f), TextAnchor.MiddleCenter);
            brand.rectTransform.anchorMin = new Vector2(0, 1);
            brand.rectTransform.anchorMax = new Vector2(1, 1);
            brand.rectTransform.pivot = new Vector2(0.5f, 1);
            brand.rectTransform.sizeDelta = new Vector2(-32, 20);
            brand.rectTransform.anchoredPosition = new Vector2(0, -100);

            var title = UIManager.Label(card, "Sign in", 28, Ink, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0.5f, 1);
            title.rectTransform.sizeDelta = new Vector2(-32, 36);
            title.rectTransform.anchoredPosition = new Vector2(0, -124);

            var idLabel = UIManager.Label(card, "Student ID", 13, Faint, TextAnchor.MiddleLeft);
            idLabel.rectTransform.anchorMin = new Vector2(0, 1);
            idLabel.rectTransform.anchorMax = new Vector2(1, 1);
            idLabel.rectTransform.pivot = new Vector2(0, 1);
            idLabel.rectTransform.sizeDelta = new Vector2(-64, 18);
            idLabel.rectTransform.anchoredPosition = new Vector2(32, -176);

            var idBox = UIManager.Panel(card, new Color(0.95f, 0.96f, 0.98f, 1f));
            idBox.anchorMin = new Vector2(0, 1);
            idBox.anchorMax = new Vector2(1, 1);
            idBox.pivot = new Vector2(0.5f, 1);
            idBox.sizeDelta = new Vector2(-64, 44);
            idBox.anchoredPosition = new Vector2(0, -198);
            var idText = UIManager.Label(idBox, "", 20, Ink, TextAnchor.MiddleLeft);
            UIManager.Stretch(idText.rectTransform, 14, 0, 14, 0);

            var pinLabel = UIManager.Label(card, "Access code", 13, Faint, TextAnchor.MiddleLeft);
            pinLabel.rectTransform.anchorMin = new Vector2(0, 1);
            pinLabel.rectTransform.anchorMax = new Vector2(1, 1);
            pinLabel.rectTransform.pivot = new Vector2(0, 1);
            pinLabel.rectTransform.sizeDelta = new Vector2(-64, 18);
            pinLabel.rectTransform.anchoredPosition = new Vector2(32, -260);

            var digits = new Text[4];
            var boxes = new Image[4];
            float start = -126f;
            for (int i = 0; i < 4; i++)
            {
                var box = UIManager.Panel(card, new Color(0.95f, 0.96f, 0.98f, 1f));
                box.anchorMin = new Vector2(0.5f, 1);
                box.anchorMax = new Vector2(0.5f, 1);
                box.pivot = new Vector2(0.5f, 1);
                box.sizeDelta = new Vector2(64, 64);
                box.anchoredPosition = new Vector2(start + i * 84f, -284);
                boxes[i] = box.GetComponent<Image>();
                digits[i] = UIManager.Label(box, "", 28, Ink, TextAnchor.MiddleCenter);
                UIManager.Stretch(digits[i].rectTransform, 0, 0, 0, 0);
            }

            var status = UIManager.Label(card, "Saved session on this laptop", 14, Faint, TextAnchor.MiddleCenter);
            status.rectTransform.anchorMin = new Vector2(0, 1);
            status.rectTransform.anchorMax = new Vector2(1, 1);
            status.rectTransform.pivot = new Vector2(0.5f, 1);
            status.rectTransform.sizeDelta = new Vector2(-32, 22);
            status.rectTransform.anchoredPosition = new Vector2(0, -368);

            if (_shell == null)
            {
                _helioIn = true;
                Render();
                return;
            }
            StopSignIn();
            _signing = true;
            _signRoutine = _shell.StartCoroutine(SignIn(idText, digits, boxes, status));
        }

        IEnumerator SignIn(Text idText, Text[] digits, Image[] boxes, Text status)
        {
            const string student = "GH-204817";
            const string pin = "7149";
            yield return new WaitForSecondsRealtime(0.35f);
            if (idText == null)
                yield break;
            status.text = "Entering student ID";
            for (int i = 0; i < student.Length; i++)
            {
                if (idText == null)
                    yield break;
                idText.text += student[i];
                UiSound.Key();
                yield return new WaitForSecondsRealtime(0.07f);
            }
            yield return new WaitForSecondsRealtime(0.28f);
            if (status == null)
                yield break;
            status.text = "Entering access code";
            var active = new Color(0.86f, 0.93f, 1f, 1f);
            for (int i = 0; i < pin.Length; i++)
            {
                if (digits[i] == null)
                    yield break;
                digits[i].text = pin[i].ToString();
                boxes[i].color = active;
                UiSound.Key();
                yield return new WaitForSecondsRealtime(0.2f);
            }
            if (status == null)
                yield break;
            status.text = "Signed in";
            status.color = new Color(0.05f, 0.45f, 0.28f, 1f);
            UiSound.Confirm();
            yield return new WaitForSecondsRealtime(0.45f);
            _signing = false;
            _signRoutine = null;
            _helioIn = true;
            Render();
        }

        static void Glyph(RectTransform parent, string id, Color plate, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var box = UIManager.Panel(parent, plate);
            box.anchorMin = anchor;
            box.anchorMax = anchor;
            box.pivot = new Vector2(0.5f, 1f);
            box.sizeDelta = size;
            box.anchoredPosition = pos;
            var go = new GameObject("Glyph", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(box, false);
            var image = go.GetComponent<Image>();
            image.sprite = UIManager.AppIcon(id);
            image.preserveAspect = true;
            image.raycastTarget = false;
            UIManager.Stretch(image.rectTransform, 12, 12, 12, 12);
        }

        void RenderBank(RectTransform content)
        {
            var hero = UIManager.Panel(content, new Color(0.07f, 0.16f, 0.38f, 1f));
            var heroEl = hero.gameObject.AddComponent<LayoutElement>();
            heroEl.minHeight = 176;
            heroEl.preferredHeight = 176;
            Glyph(hero, "helio", new Color(0.12f, 0.28f, 0.58f, 1f), new Vector2(0, 1), new Vector2(52, -16), new Vector2(48, 48));
            var brand = UIManager.Label(hero, "HELIO", 13, new Color(0.65f, 0.78f, 1f, 1f), TextAnchor.UpperLeft);
            brand.rectTransform.anchorMin = new Vector2(0, 1);
            brand.rectTransform.anchorMax = new Vector2(1, 1);
            brand.rectTransform.pivot = new Vector2(0, 1);
            brand.rectTransform.sizeDelta = new Vector2(-32, 22);
            brand.rectTransform.anchoredPosition = new Vector2(84, -18);
            var acct = UIManager.Label(hero, "Student current account", 16, Color.white, TextAnchor.UpperLeft);
            acct.rectTransform.anchorMin = new Vector2(0, 1);
            acct.rectTransform.anchorMax = new Vector2(1, 1);
            acct.rectTransform.pivot = new Vector2(0, 1);
            acct.rectTransform.sizeDelta = new Vector2(-32, 24);
            acct.rectTransform.anchoredPosition = new Vector2(84, -40);
            var money = UIManager.Label(hero, "€" + _game.Player.MoneyEuros.ToString("0"), 40, Color.white, TextAnchor.UpperLeft);
            money.rectTransform.anchorMin = new Vector2(0, 1);
            money.rectTransform.anchorMax = new Vector2(1, 1);
            money.rectTransform.pivot = new Vector2(0, 1);
            money.rectTransform.sizeDelta = new Vector2(-32, 48);
            money.rectTransform.anchoredPosition = new Vector2(20, -70);
            var iban = UIManager.Label(hero, "HM00 HELIO 0000 0017", 13, new Color(0.75f, 0.84f, 1f, 1f), TextAnchor.UpperLeft);
            iban.rectTransform.anchorMin = new Vector2(0, 1);
            iban.rectTransform.anchorMax = new Vector2(1, 1);
            iban.rectTransform.pivot = new Vector2(0, 1);
            iban.rectTransform.sizeDelta = new Vector2(-32, 20);
            iban.rectTransform.anchoredPosition = new Vector2(20, -124);

            var heading = UIManager.Label(content, "Transactions", 16, Ink, TextAnchor.MiddleLeft);
            var headingEl = heading.gameObject.AddComponent<LayoutElement>();
            headingEl.preferredHeight = 28;
            var ledger = _game.Player.Ledger;
            for (int i = ledger.Count - 1; i >= 0; i--)
            {
                var line = ledger[i];
                bool inn = line.Amount >= 0;
                var row = UIManager.Panel(content, Color.white);
                var rowEl = row.gameObject.AddComponent<LayoutElement>();
                rowEl.minHeight = 52;
                rowEl.preferredHeight = 52;
                var name = UIManager.Label(row, line.Label, 15, Ink, TextAnchor.MiddleLeft);
                name.rectTransform.anchorMin = new Vector2(0, 0);
                name.rectTransform.anchorMax = new Vector2(0.7f, 1);
                name.rectTransform.offsetMin = new Vector2(16, 0);
                name.rectTransform.offsetMax = new Vector2(0, 0);
                string sign = inn ? "+€" + line.Amount : "−€" + (-line.Amount);
                var amt = UIManager.Label(row, sign, 15, inn ? new Color(0.05f, 0.45f, 0.28f, 1f) : new Color(0.70f, 0.16f, 0.20f, 1f), TextAnchor.MiddleRight);
                amt.rectTransform.anchorMin = new Vector2(0.7f, 0);
                amt.rectTransform.anchorMax = new Vector2(1, 1);
                amt.rectTransform.offsetMin = new Vector2(0, 0);
                amt.rectTransform.offsetMax = new Vector2(-16, 0);
            }
        }

        void RenderShop(RectTransform content)
        {
            SiteHead(content, "Grayhaven Parts", "parts.grayhaven.shop");
            if (_shop == null)
                _shop = new ShopApp();
            _shop.Fill(content, _game, _shell);
        }

        void RenderNoteChoice(RectTransform content)
        {
            if (!_game.Missions.IsAvailable("novamart_decision"))
                return;
            Paragraph(content, "Northline already has the public map. This page is separate. You can tell NovaMart, pass the note to a Nightwire broker, or leave it in your notes.");
            Choice(content, "Tell NovaMart  €70", "novamart_decision", "report");
            Choice(content, "Pass it to a broker  €180", "novamart_decision", "sell");
            Choice(content, "Leave it in your notes", "novamart_decision", "hold");
        }

        void RenderRumorChoice(RectTransform content)
        {
            if (!_game.Missions.IsAvailable("nova_rumor"))
                return;
            Paragraph(content, "Reply on this board. Read the token with analyze nova in the terminal first.");
            Choice(content, "Warn the board", "nova_rumor", "warn");
            Choice(content, "Boost the post", "nova_rumor", "amplify");
        }

        void Choice(RectTransform content, string label, string mission, string option)
        {
            TextLink(content, label, null, delegate
            {
                var result = _game.Choose(mission, option);
                _quizNote = result.Text;
                _repaint = true;
                _shell.MarkDirty();
            });
        }

        void RenderQuiz(RectTransform content, QuizDef quiz)
        {
            if (quiz == null)
                return;
            Paragraph(content, quiz.Prompt);
            if (quiz.Options == null)
                return;
            for (int i = 0; i < quiz.Options.Length; i++)
            {
                var option = quiz.Options[i];
                Paragraph(content, option.Title + "\n" + option.Body);
                string id = option.Id;
                TextLink(content, "This is the odd one: " + option.Title, null, delegate
                {
                    var result = _game.SubmitQuiz(quiz.Id, id);
                    _quizNote = result.Text;
                    _repaint = true;
                    _shell.MarkDirty();
                });
            }
        }

        void Shortcut(RectTransform parent, string label, string url, float x)
        {
            var button = UIManager.MakeButton(parent, label, delegate { Navigate(url, true); }, Color.white);
            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(108, 36);
            rt.anchoredPosition = new Vector2(x, 28);
            var element = button.GetComponent<LayoutElement>();
            if (element != null)
                element.ignoreLayout = true;
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.color = Ink;
        }

        void Tool(RectTransform chrome, string glyph, float x, UnityEngine.Events.UnityAction act)
        {
            var button = UIManager.MakeButton(chrome, glyph, null, Chrome);
            button.onClick.AddListener(act);
            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(28, 28);
            rt.anchoredPosition = new Vector2(x, 8);
            var element = button.GetComponent<LayoutElement>();
            if (element != null)
                element.ignoreLayout = true;
        }

        void SetTab(string title)
        {
            if (_tab != null)
                _tab.text = title;
        }

        void SiteHead(RectTransform content, string title, string host)
        {
            var band = UIManager.Panel(content, Color.white);
            var bandEl = band.gameObject.AddComponent<LayoutElement>();
            bandEl.minHeight = 64;
            bandEl.preferredHeight = 64;
            var name = UIManager.Label(band, title, 22, Ink, TextAnchor.UpperLeft);
            name.rectTransform.anchorMin = new Vector2(0, 0.45f);
            name.rectTransform.anchorMax = new Vector2(1, 1);
            name.rectTransform.offsetMin = new Vector2(8, 0);
            name.rectTransform.offsetMax = new Vector2(-8, -4);
            var sub = UIManager.Label(band, host, 12, Faint, TextAnchor.UpperLeft);
            sub.rectTransform.anchorMin = new Vector2(0, 0);
            sub.rectTransform.anchorMax = new Vector2(1, 0.45f);
            sub.rectTransform.offsetMin = new Vector2(8, 4);
            sub.rectTransform.offsetMax = new Vector2(-8, 0);
        }

        void Paragraph(RectTransform content, string text)
        {
            var label = UIManager.Label(content, text, 15, Ink, TextAnchor.UpperLeft);
            var element = label.gameObject.AddComponent<LayoutElement>();
            int lines = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    lines++;
            }
            element.minHeight = lines * 22;
            element.preferredHeight = lines * 22 + 8;
        }

        void Article(RectTransform content, string title, string body)
        {
            var card = UIManager.Panel(content, Color.white);
            var cardEl = card.gameObject.AddComponent<LayoutElement>();
            cardEl.minHeight = 72;
            cardEl.preferredHeight = 84;
            var head = UIManager.Label(card, title, 16, Ink, TextAnchor.UpperLeft);
            head.rectTransform.anchorMin = new Vector2(0, 0.4f);
            head.rectTransform.anchorMax = new Vector2(1, 1);
            head.rectTransform.offsetMin = new Vector2(10, 0);
            head.rectTransform.offsetMax = new Vector2(-10, -6);
            var copy = UIManager.Label(card, body, 13, Faint, TextAnchor.UpperLeft);
            copy.rectTransform.anchorMin = new Vector2(0, 0);
            copy.rectTransform.anchorMax = new Vector2(1, 0.45f);
            copy.rectTransform.offsetMin = new Vector2(10, 6);
            copy.rectTransform.offsetMax = new Vector2(-10, 0);
        }

        void TextLink(RectTransform content, string label, string url)
        {
            TextLink(content, label, url, null);
        }

        void TextLink(RectTransform content, string label, string url, UnityEngine.Events.UnityAction act)
        {
            var button = UIManager.MakeButton(content, label, null, new Color(0.90f, 0.94f, 0.99f, 1f));
            if (act != null)
                button.onClick.AddListener(act);
            else
                button.onClick.AddListener(delegate { Navigate(url, true); });
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = Link;
                text.alignment = TextAnchor.MiddleLeft;
            }
        }

        static string Resolve(string url)
        {
            if (url.StartsWith("search:"))
                return url;
            if (url == "bank" || url == "helio" || url == "heliobank" || url == "account")
                return "helio.bank/account";
            if (url == "shop" || url == "store" || url == "parts" || url == "parduotuve" || url == "grayhaven")
                return "parts.grayhaven.shop";
            if (url == "news" || url == "dailygrid")
                return "dailygrid.news";
            if (url == "campus" || url == "lumen")
                return "www.lumen.edu/it";
            return url;
        }
    }
}
