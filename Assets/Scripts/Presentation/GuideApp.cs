using System;
using UnityEngine;
using UnityEngine.UI;

namespace CyberUnderground.Presentation
{
    public sealed class GuideApp
    {
        public Action Changed;
        static readonly string[] Steps =
        {
            "This is your computer.\n\nIcons on the left open programs. The bar at the bottom is only for open windows and the clock.\n\nNothing about your money sits on the desktop.",
            "Open Browser.\n\nYour euros are in the bank:\nhttps://helio.bank/account\n\nYou can also type bank.",
            "Stores are websites.\n\nThe parts counter is:\nhttps://parts.grayhaven.shop\n\nYou can also type shop or parts.",
            "Work is only posted contracts.\n\nThe first one is from Lumen IT.\nRead https://www.lumen.edu/it\nIn Terminal type: scan lumen\nThen submit the survey in Work.\n\nPay shows up in Helio."
        };

        RectTransform _content;
        int _step;

        public void Mount(RectTransform parent)
        {
            var scroll = UIManager.Scroll(parent, out _content);
            UIManager.Stretch(scroll.GetComponent<RectTransform>(), 0, 0, 0, 0);
            Refresh();
        }

        public void Refresh()
        {
            if (_content == null)
                return;
            UIManager.Clear(_content);
            var title = UIManager.Label(_content, "Desktop guide  " + (_step + 1) + " / " + Steps.Length, 16, UIManager.Accent, TextAnchor.UpperLeft);
            var titleEl = title.gameObject.AddComponent<LayoutElement>();
            titleEl.preferredHeight = 24;

            var body = UIManager.Label(_content, Steps[_step], 15, UIManager.Text, TextAnchor.UpperLeft);
            var bodyEl = body.gameObject.AddComponent<LayoutElement>();
            bodyEl.preferredHeight = 180;

            if (_step > 0)
                UIManager.MakeButton(_content, "Back", delegate { _step--; if (Changed != null) Changed(); }, UIManager.ButtonBg);
            if (_step < Steps.Length - 1)
                UIManager.MakeButton(_content, "Next", delegate { _step++; if (Changed != null) Changed(); }, UIManager.Accent);
            else
            {
                var done = UIManager.Label(_content, "You can close this window. Open Guide again any time from the desktop.", 14, UIManager.Muted, TextAnchor.UpperLeft);
                var doneEl = done.gameObject.AddComponent<LayoutElement>();
                doneEl.preferredHeight = 40;
            }
        }
    }
}
