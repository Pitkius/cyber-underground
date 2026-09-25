using UnityEngine;
using UnityEngine.EventSystems;

namespace CyberUnderground.Presentation
{
    public class WindowResize : MonoBehaviour, IDragHandler
    {
        public RectTransform Target;
        public Canvas Canvas;
        public Vector2 MinSize = new Vector2(360, 240);

        public void OnDrag(PointerEventData eventData)
        {
            float scale = Canvas != null && Canvas.scaleFactor > 0f ? Canvas.scaleFactor : 1f;
            var delta = eventData.delta / scale;
            var size = Target.sizeDelta;
            size.x = Mathf.Max(MinSize.x, size.x + delta.x);
            size.y = Mathf.Max(MinSize.y, size.y - delta.y);
            Target.sizeDelta = size;
        }
    }
}
