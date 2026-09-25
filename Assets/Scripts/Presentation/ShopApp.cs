using UnityEngine;
using UnityEngine.UI;
using CyberUnderground.Simulation;

namespace CyberUnderground.Presentation
{
    public sealed class ShopApp
    {
        RectTransform _content;
        GameSession _game;
        GameManager _shell;
        string _note = "";

        static readonly Color Ink = new Color(0.12f, 0.14f, 0.18f, 1f);
        static readonly Color Faint = new Color(0.35f, 0.40f, 0.48f, 1f);
        static readonly Color Navy = new Color(0.10f, 0.16f, 0.28f, 1f);

        public void Mount(RectTransform parent, GameSession game, GameManager shell)
        {
            var scroll = UIManager.Scroll(parent, out _content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            Fill(_content, game, shell);
        }

        public void Fill(RectTransform content, GameSession game, GameManager shell)
        {
            _content = content;
            _game = game;
            _shell = shell;
            Refresh();
        }

        public void Refresh()
        {
            if (_content == null)
                return;
            UIManager.Clear(_content);

            var hero = UIManager.Panel(_content, Navy);
            var heroEl = hero.gameObject.AddComponent<LayoutElement>();
            heroEl.minHeight = 108;
            heroEl.preferredHeight = 108;
            Plate(hero, "bag", new Color(0.18f, 0.32f, 0.55f, 1f), 16, 20, 64);
            var brand = UIManager.Label(hero, "GRAYHAVEN PARTS", 20, Color.white, TextAnchor.UpperLeft);
            brand.rectTransform.anchorMin = new Vector2(0, 1);
            brand.rectTransform.anchorMax = new Vector2(1, 1);
            brand.rectTransform.pivot = new Vector2(0, 1);
            brand.rectTransform.sizeDelta = new Vector2(-110, 28);
            brand.rectTransform.anchoredPosition = new Vector2(96, -22);
            var sub = UIManager.Label(hero, "Used hardware for campus laptops", 14, new Color(0.75f, 0.84f, 0.95f, 1f), TextAnchor.UpperLeft);
            sub.rectTransform.anchorMin = new Vector2(0, 1);
            sub.rectTransform.anchorMax = new Vector2(1, 1);
            sub.rectTransform.pivot = new Vector2(0, 1);
            sub.rectTransform.sizeDelta = new Vector2(-110, 22);
            sub.rectTransform.anchoredPosition = new Vector2(96, -54);

            if (!string.IsNullOrEmpty(_note))
            {
                var banner = UIManager.Label(_content, _note, 14, Ink, TextAnchor.MiddleLeft);
                var bannerEl = banner.gameObject.AddComponent<LayoutElement>();
                bannerEl.preferredHeight = 28;
            }

            var offers = _game.World.Upgrades;
            for (int i = 0; i < offers.Length; i++)
                Card(offers[i]);

            Service();
        }

        void Card(UpgradeDef offer)
        {
            bool owned = _game.Hardware.Owns(offer.Id);
            var card = UIManager.Panel(_content, Color.white);
            var cardEl = card.gameObject.AddComponent<LayoutElement>();
            cardEl.minHeight = 132;
            cardEl.preferredHeight = 132;

            string icon = IconFor(offer.Id);
            Color plate = owned ? new Color(0.20f, 0.48f, 0.34f, 1f) : new Color(0.16f, 0.28f, 0.48f, 1f);
            if (!offer.Purchasable)
                plate = new Color(0.35f, 0.38f, 0.44f, 1f);
            Plate(card, icon, plate, 16, 34, 64);

            var kind = UIManager.Label(card, KindFor(offer.Id), 12, Faint, TextAnchor.UpperLeft);
            kind.rectTransform.anchorMin = new Vector2(0, 1);
            kind.rectTransform.anchorMax = new Vector2(1, 1);
            kind.rectTransform.pivot = new Vector2(0, 1);
            kind.rectTransform.sizeDelta = new Vector2(-200, 16);
            kind.rectTransform.anchoredPosition = new Vector2(96, -14);

            var title = UIManager.Label(card, offer.Name, 18, Ink, TextAnchor.UpperLeft);
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0, 1);
            title.rectTransform.sizeDelta = new Vector2(-200, 24);
            title.rectTransform.anchoredPosition = new Vector2(96, -32);

            var price = UIManager.Label(card, "€" + offer.Cost, 20, Ink, TextAnchor.UpperRight);
            price.rectTransform.anchorMin = new Vector2(1, 1);
            price.rectTransform.anchorMax = new Vector2(1, 1);
            price.rectTransform.pivot = new Vector2(1, 1);
            price.rectTransform.sizeDelta = new Vector2(90, 28);
            price.rectTransform.anchoredPosition = new Vector2(-16, -16);

            var detail = UIManager.Label(card, offer.Detail, 13, Faint, TextAnchor.UpperLeft);
            detail.rectTransform.anchorMin = new Vector2(0, 1);
            detail.rectTransform.anchorMax = new Vector2(1, 1);
            detail.rectTransform.pivot = new Vector2(0, 1);
            detail.rectTransform.sizeDelta = new Vector2(-112, 36);
            detail.rectTransform.anchoredPosition = new Vector2(96, -58);

            if (owned)
            {
                var badge = UIManager.Label(card, "In this laptop", 14, new Color(0.05f, 0.45f, 0.28f, 1f), TextAnchor.MiddleRight);
                badge.rectTransform.anchorMin = new Vector2(1, 0);
                badge.rectTransform.anchorMax = new Vector2(1, 0);
                badge.rectTransform.pivot = new Vector2(1, 0);
                badge.rectTransform.sizeDelta = new Vector2(140, 28);
                badge.rectTransform.anchoredPosition = new Vector2(-16, 14);
                return;
            }

            if (!offer.Purchasable)
            {
                var locked = UIManager.Label(card, "Not sold yet", 14, Faint, TextAnchor.MiddleRight);
                locked.rectTransform.anchorMin = new Vector2(1, 0);
                locked.rectTransform.anchorMax = new Vector2(1, 0);
                locked.rectTransform.pivot = new Vector2(1, 0);
                locked.rectTransform.sizeDelta = new Vector2(140, 28);
                locked.rectTransform.anchoredPosition = new Vector2(-16, 14);
                return;
            }

            string id = offer.Id;
            var buy = UIManager.MakeButton(card, "Buy", delegate
            {
                var result = _game.TryUpgrade(id);
                _note = result.Text;
                if (result.Ok)
                    UiSound.Confirm();
                _shell.MarkDirty();
            }, UIManager.Accent);
            var buyRt = buy.GetComponent<RectTransform>();
            var buyEl = buy.GetComponent<LayoutElement>();
            if (buyEl != null)
                buyEl.ignoreLayout = true;
            buyRt.anchorMin = new Vector2(1, 0);
            buyRt.anchorMax = new Vector2(1, 0);
            buyRt.pivot = new Vector2(1, 0);
            buyRt.sizeDelta = new Vector2(88, 32);
            buyRt.anchoredPosition = new Vector2(-16, 14);
        }

        void Service()
        {
            var card = UIManager.Panel(_content, Color.white);
            var cardEl = card.gameObject.AddComponent<LayoutElement>();
            cardEl.minHeight = 88;
            cardEl.preferredHeight = 88;
            Plate(card, "notes", new Color(0.28f, 0.36f, 0.50f, 1f), 16, 12, 56);
            var title = UIManager.Label(card, "Local cleanup", 16, Ink, TextAnchor.UpperLeft);
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0, 1);
            title.rectTransform.sizeDelta = new Vector2(-200, 22);
            title.rectTransform.anchoredPosition = new Vector2(88, -16);
            var detail = UIManager.Label(card, "€10  ·  clears local notes on this machine", 13, Faint, TextAnchor.UpperLeft);
            detail.rectTransform.anchorMin = new Vector2(0, 1);
            detail.rectTransform.anchorMax = new Vector2(1, 1);
            detail.rectTransform.pivot = new Vector2(0, 1);
            detail.rectTransform.sizeDelta = new Vector2(-200, 20);
            detail.rectTransform.anchoredPosition = new Vector2(88, -40);
            var buy = UIManager.MakeButton(card, "Pay", delegate
            {
                _note = _game.CoolOff().Text;
                _shell.MarkDirty();
            }, UIManager.Accent);
            var buyRt = buy.GetComponent<RectTransform>();
            var buyEl = buy.GetComponent<LayoutElement>();
            if (buyEl != null)
                buyEl.ignoreLayout = true;
            buyRt.anchorMin = new Vector2(1, 0.5f);
            buyRt.anchorMax = new Vector2(1, 0.5f);
            buyRt.pivot = new Vector2(1, 0.5f);
            buyRt.sizeDelta = new Vector2(88, 32);
            buyRt.anchoredPosition = new Vector2(-16, 0);
        }

        static void Plate(RectTransform parent, string icon, Color color, float x, float y, float size)
        {
            var plate = UIManager.Panel(parent, color);
            plate.anchorMin = new Vector2(0, 1);
            plate.anchorMax = new Vector2(0, 1);
            plate.pivot = new Vector2(0, 1);
            plate.sizeDelta = new Vector2(size, size);
            plate.anchoredPosition = new Vector2(x, -y);
            var go = new GameObject("Glyph", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(plate, false);
            var image = go.GetComponent<Image>();
            image.sprite = UIManager.AppIcon(icon);
            image.preserveAspect = true;
            image.raycastTarget = false;
            UIManager.Stretch(image.rectTransform, 10, 10, 10, 10);
        }

        static string IconFor(string id)
        {
            if (id == "ram_8")
                return "ram";
            if (id == "net_share")
                return "fiber";
            if (id == "cpu_refurb")
                return "cpu";
            return "tower";
        }

        static string KindFor(string id)
        {
            if (id == "ram_8")
                return "Memory";
            if (id == "net_share")
                return "Network";
            if (id == "cpu_refurb")
                return "Processor";
            return "Desktop";
        }
    }
}
