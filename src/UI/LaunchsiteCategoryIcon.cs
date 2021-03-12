using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;

namespace KerbalKonstructs.UI
{
    class LaunchSiteCategoryIcon : UIObject
    {
		Image icon;

		public override void CreateUI()
		{
			icon = gameObject.AddComponent<Image> ();
		}

		public override void Style()
		{
		}

		public LaunchSiteCategoryIcon Category (LaunchSiteCategory category)
		{
			switch (category) {
				case LaunchSiteCategory.Runway:
					icon.sprite = UIMain.runWayIcon;
					icon.enabled = true;
					break;
				case LaunchSiteCategory.Helipad:
					icon.sprite = UIMain.heliPadIcon;
					icon.enabled = true;
					break;
				case LaunchSiteCategory.RocketPad:
					icon.sprite = UIMain.VABIcon;
					icon.enabled = true;
					break;
				case LaunchSiteCategory.Waterlaunch:
					icon.sprite = UIMain.waterLaunchIcon;
					icon.enabled = true;
					break;
				case LaunchSiteCategory.Other:
					icon.sprite = UIMain.ANYIcon;
					icon.enabled = true;
					break;
				default:
					icon.sprite = null;
					icon.enabled = false;
					break;
			}
			return this;
		}
    }
}
