using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Modules.Career;


namespace KerbalKonstructs.Career
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class KerbalKonstructsSettings : ScenarioModule
    {
        [Persistent(isPersistant = true)]
        public bool initialized = false;

        [Persistent(isPersistant = true)]
        public double saveTime;

        /// <summary>
        /// called at the OnLoad()
        /// </summary>
        /// <param name="node">The name of the config node</param>
        public override void OnLoad(ConfigNode node)
        {
            // check if we have been in the mainmenue before (gameTime == -1)
            if (KerbalKonstructs.gameTime != -1d) return;

            CareerMapDecals.LoadDecals(node);

            CareerGroups.LoadGroups(node);

            CareerObjects.LoadBuildings(node);

            // resetting old state in caase it is needed
            CareerState.ResetFacilitiesOpenState();

            if (node.HasValue("initialized"))
            {
                initialized = bool.Parse(node.GetValue("initialized"));
            }

            if (!initialized)
            {
                Log.Normal("Resetting OpenCloseStates for new Games");
                CareerState.ResetFacilitiesOpenState();
                initialized = true;
            }

            if (node.HasValue("savetime"))
            {
                saveTime = double.Parse(node.GetValue("savetime"));
                KerbalKonstructs.gameTime = saveTime;
            }


            if (!initialized)
            {
                return;
            }

            KerbalKonstructs.instance.LoadKKConfig(node);

            Log.Normal("KKScenario loading facility states");
            CareerState.Load(node);

            ConnectionManager.LoadGroundStations();

        }

        /// <summary>
        /// called at the OnSave()
        /// </summary>
        /// <param name="node">The name of the config node</param>
        public override void OnSave(ConfigNode node)
        {
            UI2.WindowManager2.SavePresets();
            KerbalKonstructs.gameTime = HighLogic.CurrentGame.UniversalTime;
            saveTime = KerbalKonstructs.gameTime;

            // save the state, that we got the initialisation done
            node.SetValue("initialized", initialized, true);

            node.SetValue("savetime", saveTime, true);


            KerbalKonstructs.instance.SaveKKConfig(node);

            Log.Normal("KKScenario saving career state");
            CareerState.Save(node);

            CareerMapDecals.SaveDecals(node);

            CareerGroups.SaveGroups(node);

            CareerObjects.SaveBuildings(node);
        }

        public void Start()
        {
            Log.Normal("Career Module Start Called");

        }

    }
}
