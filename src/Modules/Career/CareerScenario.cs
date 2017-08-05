using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;


namespace KerbalKonstructs.Career
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class KerbalKonstructsSettings : ScenarioModule
    {

        internal bool initialized = false;

        /// <summary>
        /// called at the OnLoad()
        /// </summary>
        /// <param name="node">The name of the config node</param>
        public override void OnLoad(ConfigNode node)
        {

           
            // resetting old state in caase it is needed
            CareerState.ResetFacilitiesOpenState();

            if (node.HasValue("initialized"))
            {
                initialized = bool.Parse(node.GetValue("initialized"));
            }

            if (!initialized)
            {
                return;
            }

            KerbalKonstructs.instance.LoadKKConfig(node);

            if (CareerUtils.isCareerGame)
            {
                Log.Normal("KKScenario loading facility states");
                CareerState.Load(node);
            }

            RemoteNet.LoadGroundStations();

        }

        /// <summary>
        /// called at the OnSave()
        /// </summary>
        /// <param name="node">The name of the config node</param>
        public override void OnSave(ConfigNode node)
        {            
            // save the state, that we got the initialisation done
            node.SetValue("initialized", initialized, true);

            KerbalKonstructs.instance.SaveKKConfig(node);

            if (CareerUtils.isCareerGame)
            {
                Log.Normal("KKScenario saving career state");
                CareerState.Save(node);
            }
        }

        public void Start()
        {
            Log.Normal("Carrer Module Start Called");

            if (!initialized)
            {
                Log.Normal("Resetting OpenCloseStates for new Games");
                CareerState.ResetFacilitiesOpenState();
                RemoteNet.LoadGroundStations();
            }

            initialized = true;

        }

    }
}
