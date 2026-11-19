using UnityEngine;
using UnityEngine.EventSystems;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Drag handling for PopupDialog windows.
    /// KSP's own dragging stops the window some distance before the screen border.
    /// This component runs alongside it and sets the final position in LateUpdate
    /// (after KSP has handled the drag), so a window can be moved right up to the
    /// screen border. The window is always kept completely on screen.
    /// </summary>
    internal class KKWindowDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform windowTransform;
        private RectTransform parentTransform;
        private Camera eventCamera;

        private bool isDragging = false;

        // all in screen pixels
        private Vector2 pointerStart;
        private Vector2 windowStart;
        private Vector2 desiredScreenPos;
        // distance from the window pivot to its left/bottom and right/top edge
        private Vector2 extentMin;
        private Vector2 extentMax;


        /// <summary>
        /// Call right after PopupDialog.SpawnPopupDialog
        /// </summary>
        internal static void Attach(PopupDialog dialog)
        {
            if (dialog == null)
            {
                return;
            }

            // Unity sends drag events to every handler on the object that receives the drag.
            // That is the window itself or the child that holds KSP's own drag handler,
            // so attach to the window and to every object with a (non-control) drag handler.
            AttachTo(dialog.gameObject, dialog.RTrf);
            foreach (IDragHandler handler in dialog.gameObject.GetComponentsInChildren<IDragHandler>(true))
            {
                Component component = handler as Component;
                if (component == null || component is KKWindowDrag)
                {
                    continue;
                }
                // scroll lists, sliders, scrollbars and input fields need their own dragging
                if (component is UnityEngine.UI.ScrollRect || component is UnityEngine.UI.Selectable)
                {
                    continue;
                }
                AttachTo(component.gameObject, dialog.RTrf);
            }
        }


        private static void AttachTo(GameObject target, RectTransform window)
        {
            if (target.GetComponent<KKWindowDrag>() != null)
            {
                return;
            }
            KKWindowDrag drag = target.AddComponent<KKWindowDrag>();
            drag.windowTransform = window;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (windowTransform == null)
            {
                windowTransform = transform as RectTransform;
            }
            parentTransform = windowTransform.parent as RectTransform;
            eventCamera = eventData.pressEventCamera;

            pointerStart = eventData.position;
            windowStart = RectTransformUtility.WorldToScreenPoint(eventCamera, windowTransform.position);

            // window size on screen, relative to its pivot
            Vector3[] corners = new Vector3[4];
            windowTransform.GetWorldCorners(corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);
            extentMin = windowStart - bottomLeft;
            extentMax = topRight - windowStart;

            desiredScreenPos = windowStart;
            isDragging = true;
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }
            Vector2 target = windowStart + (eventData.position - pointerStart);

            // keep the whole window on screen, but allow it to touch the border
            target.x = Mathf.Clamp(target.x, extentMin.x, Screen.width - extentMax.x);
            target.y = Mathf.Clamp(target.y, extentMin.y, Screen.height - extentMax.y);

            desiredScreenPos = target;
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }
            OnDrag(eventData);
            ApplyPosition();
            isDragging = false;
        }


        private void LateUpdate()
        {
            if (isDragging)
            {
                ApplyPosition();
            }
        }


        private void ApplyPosition()
        {
            if (windowTransform == null)
            {
                return;
            }
            RectTransform reference = (parentTransform != null) ? parentTransform : windowTransform;
            Vector3 worldPos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(reference, desiredScreenPos, eventCamera, out worldPos))
            {
                windowTransform.position = new Vector3(worldPos.x, worldPos.y, windowTransform.position.z);
            }
        }
    }
}
