using System;
using UnityEngine;
using KerbalKonstructs.API;

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSite
	{
		[PersistentKey]
		public string name;

		public string author;
		public SiteType type;
		public Texture logo;
		public Texture icon;
		public string description;

		public string category;
		public float opencost;
		public float closevalue;

		[PersistentField]
		public string openclosestate;

		[PersistentField]
		public string favouritesite;

		public float reflon;
		public float reflat;
		public float refalt;
		public float sitelength;
		public float sitewidth;
		public float launchrefund;
		public float recoveryfactor;
		public float recoveryrange;

		public GameObject GameObject;
		public PSystemSetup.SpaceCenterFacility facility;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, Texture sIcon,
			string sDescription, string sDevice, float fOpenCost, float fCloseValue, string sOpenCloseState, float fRefLon, 
			float fRefLat, float fRefAlt, float fLength, float fWidth, float fRefund, float fRecoveryFactor, float fRecoveryRange,
			GameObject gameObject, PSystemSetup.SpaceCenterFacility newFacility = null, string sFavourite = "No")
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			icon = sIcon;
			description = sDescription;
			category = sDevice;
			opencost = fOpenCost;
			closevalue = fCloseValue;
			openclosestate = sOpenCloseState;
			GameObject = gameObject;
			facility = newFacility;
			reflon = fRefLon;
			reflat = fRefLat;
			refalt = fRefAlt;
			sitelength = fLength;
			sitewidth = fWidth;
			launchrefund = fRefund;
			recoveryfactor = fRecoveryFactor;
			recoveryrange = fRecoveryRange;
			favouritesite = sFavourite;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
