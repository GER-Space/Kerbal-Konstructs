/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/

namespace KerbalKonstructs {

	public static class KKLocalization
	{
		public static string Opened { get; } = "Opened";
		public static string Closed { get; } = "Closed";
		public static string TrackingStations { get; } = "Tracking Stations";
		public static string Rocketpads { get; } = "Rocketpads";
		public static string Helipads { get; } = "Helipads";
		public static string Runways { get; } = "Runways";
		public static string WaterLaunch { get; } = "Water Launch";
		public static string Other { get; } = "Other";
		public static string Occlude { get; } = "Occlude";
		public static string OnlyFavorites { get; } = "Only Favorites";
		public static string ALL { get; } = "ALL";
		public static string CreateWaypoint { get; } = "Create Waypoint";
		public static string DeleteWaypoint { get; } = "Delete Waypoint";
		public static string OpenBaseManager { get; } = "Open BaseManager";
		public static string Close { get; } = "Close";
		public static string BaseManager { get; } = "Base Manager";
		public static string Altitude { get; } = "Altitude";
		public static string Longitude { get; } = "Longitude";
		public static string Latitude { get; } = "Latitude";
		public static string MaxLength { get; } = "Max Length";
		public static string MaxWidth { get; } = "Max Width";
		public static string MaxHeight { get; } = "Max Height";
		public static string MaxMass { get; } = "Max Mass";
		public static string MaxParts { get; } = "Max Parts";
		public static string Stats { get; } = "Stats";
		public static string Log { get; } = "Log";
		public static string Unlimited { get; } = "unlimited";
		public static string HasBeenSetAsLaunchsite { get; } = "<<1>> has been set as the launchsite.";
		public static string HasBeenSetAsDefault { get; } = "<<1>> has been set as the default.";
		public static string OpenBaseForFunds { get; } = "Open base for <<1>> funds.";
		public static string OpenSiteForFunds { get; } = "Open <<1>> for <<2>> funds.";
		public static string OpenFacilityForFunds { get; } = "Open for <<1>> funds.";
		public static string CloseFacilityForFunds { get; } = "Close for <<1>> funds.";
		public static string AlwaysOpen { get; } = "Always Open";
		public static string CannotClose { get; } = "Cannot Close";
		public static string CloseBaseForFunds { get; } = "Close base for <<1>> funds.";
		public static string InsuficientFundsToOpenBase { get; } = "Insufficient funds to open this base!";
		public static string InsuficientFundsToOpenSite { get; } = "Insufficient funds to open this site!";
		public static string InsuficientFundsToOpenFacility { get; } = "Insufficient funds to open this facility!";
		public static string SetLaunchsite { get; } = "Set as launchsite";
		public static string SetDefault { get; } = "Set as Default";
		public static string UseDefault { get; } = "Use Default";
		public static string NoLog { get; } = "No log.";
		public static string LaunchsiteSelector { get; } = "Launchsite Selector";
		public static string CurrentLaunchsite { get; } = "Current Launchsite";

		public static string InflightBaseBoss { get; } = "Inflight Base Boss";
		public static string FlightTools { get; } = "Flight Tools";
		public static string LandingGuide { get; } = "Landing Guide";
		public static string NGS { get; } = "NGS";
		public static string SelectedBase { get; } = "Selected Base";
		public static string NoBasesBeacon { get; } = "No base's beacon in range at this altitude.";
		public static string NoNearestBase { get; } = "No nearest base found.";
		public static string NearestBase { get; } = "Nearest Base";
		public static string NGSSetTo { get; } = "NGS set to <<1>>";
		public static string BaseStatus { get; } = "Base Status";
		public static string BaseIsOpen { get; } = "Base is Open";
		public static string BaseCannotBeOpened { get; } = "Base cannot be opened or closed";
		public static string SiteOpened { get; } = "<<1>> opened.";
		public static string BasesCanBeOpened { get; } = "Bases can be opened or closed only when landed within 5km of the base.";
		public static string OperationalFacilities { get; } = "Operational Facilities";
		public static string NoFacilitiesWithin { get; } = "No facilities within 5000m";
		public static string NearbyFacilitiesCanBeShown { get; } = "Nearby facilities can be shown only when landed.";
		public static string ScanForFacilities { get; } = "Scan For Facilities";
		public static string OtherFeatures { get; } = "Other Features";
		public static string StartAirRacing { get; } = "Start Air Racing!";
		public static string BasicOrbitalData { get; } = "Basic Orbital Data";
		public static string FacilityStatus { get; } = "Facility Status";

		public static string Alt { get; } = "Alt.";
		public static string Lat { get; } = "Lat.";
		public static string Lon { get; } = "Lon.";

		public static string FacilityManager { get; } = "Facility Manager";
		public static string FacilityPurposeHangar { get; } = "Craft can be stored in this building for launching from the base at a later date. The building has limited space.";
		public static string FacilityPurposeBarracks { get; } = "This facility provides a temporary home for base-staff. Other facilities can draw staff from the pool available at this facility.";
		public static string FacilityPurposeResearch { get; } = "This facility carries out research and generates Science.";
		public static string FacilityPurposeBusiness { get; } = "This facility carries out business related to the space program in order to generate Funds.";
		public static string FacilityPurposeGroundStation { get; } = "This facility can be a GroundStation for RemoteTech/CommNet.";
		public static string FacilityPurposeMerchant { get; } = "You can buy and sell Resources here.";
		public static string FacilityPurposeStorage { get; } = "You can store Resources here.";

		public static string GroundStation { get; } = "Ground Station";

		public static string HangarMessage { get; } = "Where necessary craft are disassembled for storage or reassembled before being rolled out. Please note that for game purposes, this procedure is instantaneous.";
		public static string HangarMaxCraft { get; } = "Max Craft";
		public static string HangarMaxMassPerCraft { get; } = "Max Mass/Craft";
		public static string HangarNoCraft { get; } = "No currently held in this facility.";
		public static string HangarStoredCraft { get; } = "Stored Craft (<<1>>/<<2>>)";
		public static string HangarBlocked { get; } = "Cannot roll craft out. Clear the way first!";
		public static string HangarStoreVessel { get; } = "Store Vessel";
		public static string HangarTooHeavy { get; } = "Vessel too heavy";
		public static string HangarIsFull { get; } = "Hangar is Full!";
		public static string HangarTooFar { get; } = "You are <<1>>m too far away to store your craft!";

		public static string Range { get; } = "Range";
		public static string MemberOfGroup { get; } = "Member of Group";

		public static string StaffNoStaffRequired { get; } = "This facility does not require staff assigned to it.";
		public static string StaffNoRoom { get; } = "There's no room left in a barracks or apartment for this kerbal to go.";
		//public static string StaffMustHaveCaretaker { get; } = "An open facility must have one resident caretaker.";
		public static string StaffNoFacilityWithStaff { get; } = "No facility with available staff is nearby.";
		public static string StaffNoUnassignedStaffAvailable { get; } = "No unassigned staff available.";
		public static string StaffFullyStaffed { get; } = "Facility is fully staffed.";
		public static string StaffRefundForFiring { get; } = "Refund for firing";
		public static string StaffReputationLost { get; } = "Rep lost";
		public static string StaffCostToHire { get; } = "Cost to hire next kerbal";
		public static string StaffAllStaffAssigned { get; } = "All staff are assigned to duties. Staff must be unassigned in order to fire them.";
		public static string StaffMustHaveCaretaker { get; } = "This facility must have at least one caretaker.";
		public static string StaffInsufficientFunds { get; } = "Insufficient funds to hire staff!";
		public static string StaffFacilityIsFull { get; } = "Facility is full.";
		public static string StaffAssign { get; } = "Assign";
		public static string StaffUnassign { get; } = "Unassign";
		public static string StaffHire { get; } = "Hire";
		public static string StaffFire { get; } = "Fire";
		public static string Staff { get; } = "Staff";
		public static string StaffUnassigned { get; } = "Unassigned";
		public static string StaffAssigned { get; } = "Assigned Staff";

		public static string StackSize { get; } = "Stack size:";
		public static string VolumeUsed { get; } = "Volume used";
		public static string StoreRetrieveResources { get; } = "Store or retrieve these resources:";
		public static string ThanksForUsing { get; } = "Thanks for using";
		public static string StorageStored { get; } = "Stored";
		public static string StorageVolume { get; } = "Volume";
		public static string StorageHeld { get; } = "Held";
		public static string BuySellResources { get; } = "Buy or sell these resources:";
		public static string ThanksForTrading { get; } = "Thanks for trading at";
		public static string MerchantHeld { get; } = "Held";
		public static string MerchantSellFor { get; } = "Sell for";
		public static string MerchantBuyFor { get; } = "Buy for";
	}
}
