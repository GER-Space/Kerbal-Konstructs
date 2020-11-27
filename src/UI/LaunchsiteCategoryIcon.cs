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

		LaunchSiteCategoryIcon Category (LaunchSiteCategory category)
		{
			switch (category) {
				case LaunchSiteCategory.Runway:
					icon.sprite = UIMain.runWayIcon;
					break;
				case LaunchSiteCategory.Helipad:
					icon.sprite = UIMain.heliPadIcon;
					break;
				case LaunchSiteCategory.RocketPad:
					icon.sprite = UIMain.VABIcon;
					break;
				case LaunchSiteCategory.Waterlaunch:
					icon.sprite = UIMain.waterLaunchIcon;
					break;
				case LaunchSiteCategory.Other:
					icon.sprite = UIMain.ANYIcon;
					break;
				default:
					icon.sprite = null;
					break;
			}
			return this;
		}
    }
}
