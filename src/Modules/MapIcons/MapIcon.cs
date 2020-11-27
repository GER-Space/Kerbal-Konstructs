using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using KSP.UI.Screens.Mapview;

namespace KerbalKonstructs.Modules
{

	public interface IMapIcon
	{
		Vector3d Position { get; }
		string Tooltip { get; }
		Sprite Icon { get; }
		bool IsOccluded { get; }
		bool IsHidden { get; }
		MapIcon MapIcon { get; set; }
		void OnClick();
	}

    public class MapIcon : UIBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
		RectTransform rectTransform { get { return transform as RectTransform; } }

		IMapIcon iconObject;
		Image image;

        public static bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
                return false;

            return true;
        }

		public static MapIcon Create(IMapIcon iconObject)
		{
			var go = new GameObject("KKMapIcon", typeof (MapIcon), typeof (Image));
			var mapIcon = go.GetComponent<MapIcon> ();
			var image = go.GetComponent<Image> ();

			var rect = mapIcon.rectTransform;

			rect.SetParent(MapViewCanvasUtil.NodeContainer);
			rect.anchorMin = new Vector2(0, 0);
			rect.anchorMax = new Vector2(0, 0);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(16, 16);

			mapIcon.iconObject = iconObject;
			mapIcon.image = image;

			image.sprite = iconObject.Icon;

			return mapIcon;
		}

		void Update()
		{
			Vector3d pos = ScaledSpace.LocalToScaledSpace(iconObject.Position);

			bool hide = MapIconDraw.mapHideIconsBehindBody && iconObject.IsOccluded;
			//Vector3 spos = MapView.MapCamera.GetComponent<Camera>().WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(pos));
			bool visible = true;
			Vector3 spos = MapViewCanvasUtil.ScaledToUISpacePos(pos, ref visible, MapNode.zSpaceEasing, MapNode.zSpaceMidpoint, MapNode.zSpaceUIStart, MapNode.zSpaceLength);

			float radarRadius = 12800 / spos.z;
			if (radarRadius < 15) {
				hide = true;
			}
			//Debug.Log($"[MapIcon] LateUpdate {iconObject.Tooltip} {pos} {spos} {hide} {visible}");
			hide = false;
			image.enabled = !hide;

			rectTransform.localPosition = spos;
		}

		public void UpdateActive(bool isVisible)
		{
			Debug.Log($"[MapIcon] UpdateActive {isVisible} {iconObject.IsHidden}");
			gameObject.SetActive(isVisible && !iconObject.IsHidden);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			MapIconSelector.eventData = eventData;
			iconObject.OnClick();
		}

		public void OnPointerEnter (PointerEventData eventData)
		{
		}

		public void OnPointerExit (PointerEventData eventData)
		{
		}
    }
}
