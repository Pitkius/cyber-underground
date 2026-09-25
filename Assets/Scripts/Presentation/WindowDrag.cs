using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyberUnderground.Presentation
{
    public class WindowDrag : MonoBehaviour, IDragHandler
    {
        public RectTransform Target;
        public Canvas Canvas;
        public Action BeforeDrag;

        public void OnDrag(PointerEventData eventData)
        {
            if (BeforeDrag != null)
                BeforeDrag();
            float scale = Canvas != null && Canvas.scaleFactor > 0f ? Canvas.scaleFactor : 1f;
            Target.anchoredPosition += eventData.delta / scale;
        }
    }
}
