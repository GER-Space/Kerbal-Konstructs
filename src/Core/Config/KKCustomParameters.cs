using KerbalKonstructs.Addons;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    public class KKCustomParameters0 : GameParameters.CustomParameterNode
    {
        private static KKCustomParameters0 _instance;


        public static KKCustomParameters0 instance
        {
            get
            {
                //if (_instance == null)
                {
                    if (HighLogic.CurrentGame != null)
                    {
                        _instance = HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters0>();
                    }
                }
                return _instance;
            }
        }

        public override string Title { get { return "Gameplay Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kerbal Konstructs"; } }
#if !KSP12
        public override string DisplaySection { get { return "Kerbal Konstructs"; } }
#endif
        public override int SectionOrder { get { return 0; } }
        public override bool HasPresets { get { return false; } }


        // GamePlay settings
        [GameParameters.CustomStringParameterUI("Gameplay settings", title = "GamePlay Settings", lines = 1)]
        public string blank0 = "";
        [GameParameters.CustomParameterUI("Enable RemoteTech GroundStation", toolTip = "Kerbal Konstructs will place RemoteTech ground antennas to any open GroundStation", autoPersistance = true)]
        public bool enableRT = false;
        [GameParameters.CustomParameterUI("Enable CommNet GroundStations", toolTip = "Kerbal Konstructs will place CommNet ground antennas to any open GroundStation", autoPersistance = true)]
        public bool enableCommNet = false;

        // difficulty setting
        [GameParameters.CustomStringParameterUI("", title = "", lines = 1)]
        public string blank01 = "";
        [GameParameters.CustomStringParameterUI("", title = "Difficulty Settings", lines = 1)]
        public string blank1 = "";
        [GameParameters.CustomParameterUI("Open bases only when landed", toolTip = "Enable this if you don't want to use the trackingstation to open new bases.\n " +
                                            "With this enabled you need to land at a base to open it", autoPersistance = true)]
        public bool disableRemoteBaseOpening = false;

        [GameParameters.CustomFloatParameterUI("max facility range", toolTip = "Until which distance should a facility be usable", minValue = 50, maxValue = 8000, stepCount = 50, autoPersistance = true)]
        public float facilityUseRange = 300;

        [GameParameters.CustomParameterUI("Disable Remote Recoovery", toolTip = "Disable the usage of open bases for the calculation of the recovery value", autoPersistance = true)]
        public bool disableRemoteRecovery = false;
        //[GameParameters.CustomFloatParameterUI("Default recovery factor" , toolTip = "How good is KK base at recovering a vessel, this might be overwritten the bases configuration" , minValue = 0 , maxValue = 100 , autoPersistance = true)]
        //public float defaultRecoveryFactor = 50;
        //[GameParameters.CustomFloatParameterUI("Default recovery range", toolTip = "until which distance should a base be able to recover a vessel, this might be overwritten the bases configuration",  minValue = 0, maxValue = 500000, stepCount = 100, autoPersistance = true)]
        //public float defaultEffectiveRange = 100000;

        // misc settings
        [GameParameters.CustomStringParameterUI("", title = "", lines = 1)]
        public string blank03 = "";
        [GameParameters.CustomStringParameterUI("", title = "Misc Settings", lines = 1)]
        public string blank3 = "";
        [GameParameters.CustomStringParameterUI("", title = "", lines = 1)]
        public string blank04 = "";
        [GameParameters.CustomParameterUI("Show Icons only with LS selector", toolTip = "Show only the icons on the map, when the KK selector is opened", autoPersistance = true)]
        public bool toggleIconsWithBB = false;
        [GameParameters.CustomFloatParameterUI("The master volume for KK sound objects", toolTip = "Set here to adjust the loudness of the AudioPlayer objects.", minValue = 0, maxValue = 1, asPercentage = true, stepCount = 100, autoPersistance = true)]
        public float soundMasterVolume = 1f;
        [GameParameters.CustomParameterUI("Focus last LaunchSite", toolTip = "Switch the SpaceCenter view to the Last LaunchSite used", autoPersistance = true)]
        public bool focusLastLaunchSite = false;

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "enableRT") //This Field must always be Interactible.
            {
                if (RemoteTechAddon.isInstalled)
                {
                    return true;
                }
                else
                {
                    enableRT = false;
                    return false;
                }
            }
            else
                return true;
        }

    }

    public class KKCustomParameters1 : GameParameters.CustomParameterNode
    {
        public enum NewInstancePath
        {
            INTERNAL,
            EXTERNAL,
        }

        public override string Title { get { return "Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kerbal Konstructs"; } }
#if !KSP12
        public override string DisplaySection { get { return "Kerbal Konstructs"; } }
#endif
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }



        [GameParameters.CustomStringParameterUI("", title = "Misc Settings", lines = 1)]
        public string blank1 = "";
        // editor settings
        [GameParameters.CustomFloatParameterUI("Visiblility Range", minValue = 5000, maxValue = 200000, stepCount = 2500, autoPersistance = true)]
        public float maxEditorVisRange = 25000;
        [GameParameters.CustomParameterUI("Enable inflight highlighting", toolTip = "Facilities will glow when you move the curser over them", autoPersistance = true)]
        public bool enableInflightHighlight = false;

        public string blank11 = "";
        [GameParameters.CustomStringParameterUI("", title = "Editor Settings", lines = 1)]
        public string blank01 = "";

        [GameParameters.CustomParameterUI("Spawn preview models", toolTip = "just leave this to true", autoPersistance = true)]
        public bool spawnPreviewModels = true;
        [GameParameters.CustomParameterUI("Use alterative editor camera", toolTip = "Use the legacy KK editor camera, which can go under the surface", autoPersistance = true)]
        public bool useLegacyCamera = false;
        [GameParameters.CustomParameterUI("RenderCamera for underground colors", toolTip = "Use a lighting dependent camera instead of a calculated color for getting the underground grass color", autoPersistance = true)]
        public bool useCamUnderdroudColors = false;


        [GameParameters.CustomParameterUI("Directory for new Instances", toolTip = "Path under GameData where newly placed static configs should be saved", autoPersistance = true)]
        public NewInstancePath newInstanceEnum = NewInstancePath.INTERNAL;
        [GameParameters.CustomStringParameterUI("Directory for new Instances", lines = 2, autoPersistance = true)]
        public string newInstancePath = "KerbalKonstructs/NewInstances";


        [GameParameters.CustomStringParameterUI("", title = "", lines = 1)]
        public string blank02 = "";
        [GameParameters.CustomStringParameterUI("", title = "Debug Settings", lines = 1)]
        public string blank2 = "";
        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "enable this to get a lot of debug messages.", autoPersistance = true)]
        public bool DebugMode = false;


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (newInstanceEnum == NewInstancePath.INTERNAL)
            {
                newInstancePath = "KerbalKonstructs/NewInstances";
            }
            else
            {
                newInstancePath = "NewInstances";
            }

            return true;
        }



    }


    public class KKCustomParameters2 : GameParameters.CustomParameterNode
    {
        private static KKCustomParameters2 _instance;



        public static KKCustomParameters2 instance
        {
            get
            {
                if (_instance == null)
                {
                    if (HighLogic.CurrentGame != null)
                    {
                        _instance = HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters2>();
                    }
                }
                return _instance;
            }
        }

        public override string Title { get { return "Cheats"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kerbal Konstructs"; } }
#if !KSP12
        public override string DisplaySection { get { return "Kerbal Konstructs"; } }
#endif
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return false; } }




        [GameParameters.CustomParameterUI("Launch from any Site", toolTip = "With this set to true you could launch a plane from the SHP on a rocket launchpad. or vice versa", autoPersistance = true)]
        public bool launchFromAnySite = false;
        [GameParameters.CustomParameterUI("Open everything", toolTip = "Set every base to open", autoPersistance = true, gameMode = GameParameters.GameMode.ANY)]
        public bool disableCareerStrategyLayer = false;

        [GameParameters.CustomParameterUI("Show hidden bases", toolTip = "Discover every hidden base without traveling there", autoPersistance = true, gameMode = GameParameters.GameMode.ANY)]
        public bool discoverAllBases = false;

        [GameParameters.CustomParameterUI("Leave Stock CommNet", toolTip = "Do not remove the overpowered Stock CoommNet Stations around Kerbin", autoPersistance = true, gameMode = GameParameters.GameMode.ANY)]
        public bool dontRemoveStockCommNet = true;





    }

}
