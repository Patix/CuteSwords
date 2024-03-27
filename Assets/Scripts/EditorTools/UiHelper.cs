using UnityEditor;
using UnityEngine;

namespace Io.Assets.Scripts.Editor
{
    public class UiHelper : MonoBehaviour
    {
        [MenuItem("CONTEXT/RectTransform/SetAnchors")]
        private static void SetAnchors(MenuCommand menuCommand)
        {
            var rectTransfomr = menuCommand.context as RectTransform;
            if(rectTransfomr == null)
                return;
            if(!(rectTransfomr.parent is RectTransform))
                return;

            var parentRectSize = ((RectTransform)rectTransfomr.parent).rect.size;

            var leftTop = ((Vector2)rectTransfomr.localPosition - rectTransfomr.rect.size / 2f) +
                          parentRectSize / 2f;
            var rightBottom = ((Vector2)rectTransfomr.localPosition + rectTransfomr.rect.size / 2f) +
                          parentRectSize / 2f;

            Undo.RecordObject(rectTransfomr, "Set up anchors");
            rectTransfomr.anchorMin = new Vector2(leftTop.x / parentRectSize.x, leftTop.y / parentRectSize.y);
            rectTransfomr.anchorMax = new Vector2(rightBottom.x / parentRectSize.x, rightBottom.y / parentRectSize.y);
            rectTransfomr.anchoredPosition = Vector2.zero;
            rectTransfomr.sizeDelta = Vector2.zero;
        }
    }
}
