using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs;
using System.Collections;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    public class KKCustomParameters : GameParameters.CustomParameterNode
    {
        private static KKCustomParameters instance;


        public static KKCustomParameters Instance
        {
            get
            {
                if (instance == null)
                {
                    if (HighLogic.CurrentGame != null)
                    {
                        instance = HighLogic.CurrentGame.Parameters.CustomParams<KKCustomParameters>();
                    }
                }
                return instance;
            }
        }



        public override string Title { get { return "Kerbal Konstructs Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Kerbal Konstructs"; } }
        public override string DisplaySection { get { return "Kerbal Konstructs"; } }
        public override int SectionOrder { get { return 0; } }
        public override bool HasPresets { get { return false; } }
  




        // GamePlay settings
        [GameParameters.CustomParameterUI("Enable RemoteTech GroundStation", title = "GamePlay Settings", toolTip = "Kerbal Konstricts will place RemoteTech ground antennas to any open GroundStation", autoPersistance = true)]
        public bool enableRT = true;
        [GameParameters.CustomParameterUI("Enable CommNet GroundStations", toolTip = "Kerbal Konstricts will place CommNet ground antennas to any open GroundStation", autoPersistance = true)]
        public bool enableCommNet = true;

        // difficulty setting
        [GameParameters.CustomParameterUI("Open bases only when landed", toolTip = "Enable this if you don't want to use the trackingstation to open new bases.\n " +
                                            "With this enabled you need to land at a base to open it", title = "Difficulty Settings", autoPersistance = true)]
        public bool disableRemoteBaseOpening = false;


        public double facilityUseRange = 300;

        [GameParameters.CustomParameterUI("Disable Remote Recoovery", toolTip = "Disable the usage of open bases for the calculation of the recovery value", autoPersistance = true)]
        public bool disableRemoteRecovery = false;
        [GameParameters.CustomFloatParameterUI("Default recovery factor" , toolTip = "How good is KK base at recovering a vessel, this might be overwritten the bases configuration" , minValue = 0 , maxValue = 100 , autoPersistance = true)]
        public float defaultRecoveryFactor = 50;

        public float defaultEffectiveRange = 100000;


        // cheats
        [GameParameters.CustomParameterUI("Launch from any Site", toolTip = "With this set to true you could launch a plane from the SHP on a rocket launchpad. or vice versa",title = "Cheats", autoPersistance = true)]
        public bool launchFromAnySite = false;
        [GameParameters.CustomParameterUI("Open everything", toolTip = "Use every base and facility without paying money", autoPersistance = true , gameMode = GameParameters.GameMode.CAREER)]
        public bool disableCareerStrategyLayer = false;

        // remove this
        [GameParameters.CustomParameterUI("Show Icons only with LS selector", toolTip = "Show only the icons on the map, when the KK selector is opened",title = "Map Control", autoPersistance = true)]
        public bool toggleIconsWithBB = false;



        // editor settings

        public double maxEditorVisRange = 100000;
        [GameParameters.CustomParameterUI("Spawn preview models", toolTip = "just leave this to true", autoPersistance = true)]
        public bool spawnPreviewModels = true;
        [GameParameters.CustomParameterUI("Debug Mode", toolTip = "enable this to get a lot of debug messages.", autoPersistance = true)]
        public bool DebugMode = false;










        [GameParameters.CustomParameterUI("My Boolean - Mod Enabled?", toolTip = "test bool tooltip")]
        public bool MyBool = true;
        [GameParameters.CustomStringParameterUI("Test String UI", autoPersistance = true, lines = 2, title = "This is what should show Test string#1", toolTip = "test string tooltip")]
        public string UIstring = "";
        [GameParameters.CustomFloatParameterUI("My Float", minValue = 0.0f, maxValue = 50.0f)]
        public double MyFloat = 1.0f;
        [GameParameters.CustomIntParameterUI("My Integer", maxValue = 10)]
        public int MyInt = 1;
        [GameParameters.CustomIntParameterUI("My Non-Sandbox Integer", gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE)]
        public int MyCareerInt = 1;
        [GameParameters.CustomIntParameterUI("My New Game Integer", newGameOnly = true)]
        public int MyNewGameInt = 1;
        [GameParameters.CustomFloatParameterUI("My Percentage", asPercentage = true)]
        public double MyPercentage = 0.5f;
        [GameParameters.CustomIntParameterUI("My Stepped Integer", maxValue = 500000, stepSize = 1000)]
        public int MySteppedInt = 1000;



        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "MyBool") //This Field must always be enabled.
                return true;
            if (MyBool == false) //Otherwise it depends on the value of MyBool if it's false return false
            {
                if (member.Name == "UIstring" || member.Name == "MyFloat") // Example these fields are Enabled (visible) all the time.
                    return true;
                return false;
            }

            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "MyBool") //This Field must always be Interactible.
                return true;
            if (MyBool == false)  //Otherwise it depends on the value of MyBool if it's false return false
                return false;
            return true; //otherwise return true
        }



    }
}
