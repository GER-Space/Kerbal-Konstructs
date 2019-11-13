using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace KerbalKonstructs
{
    class SquadStatics
    {

        /// <summary>
        /// Loads all Squad assets into the ModelDatabase
        /// </summary>
        internal static void LoadSquadModels()
        {
            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {
                LoadDesertSiteAssets();
                ExtractDesertLights();
            }

            LoadSquadKSCModels();
            LoadSquadAnomalies();
            LoadSquadAnomaliesLevel2();
            LoadSquadAnomaliesLevel3();
            // MangleTrackingDishes();

        }


        /// <summary>
        /// Loads the Models from the KSC into the model Database
        /// </summary>
        public static void LoadSquadKSCModels()
        {
            var dotAnim = Resources.FindObjectsOfTypeAll<TimeOfDayAnimation>().Where(x => x.gameObject.name == "SpaceCenter").First();

            if (dotAnim != null)
            {
                //Log.Normal("DN found: " + dotAnim.gameObject.name);
                //Log.Normal("DN emissiveColorProperty: " + dotAnim.emissiveColorProperty);
                //Log.Normal("DN tgtColor: " + dotAnim.emissiveTgtColor);

                InstanceUtil.dayNightEmissives = dotAnim.emissives;
                InstanceUtil.dotPoperty = dotAnim.emissiveColorProperty;
                InstanceUtil.dotColor = dotAnim.emissiveTgtColor;
                InstanceUtil.dotAnimationCurve = dotAnim.emissivesCurve;

                foreach (TimeOfDayAnimation.MaterialProperty mp in dotAnim.emissives)
                {
                    //Log.Normal("DN matname: " + mp.material.name);
                    InstanceUtil.materialPropertyNames.Add(mp.material.name);
                }
            }


            // first we find get all upgradeable facilities
            Upgradeables.UpgradeableObject[] upgradeablefacilities;
            upgradeablefacilities = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>();

            foreach (var facility in upgradeablefacilities)
            {
                for (int i = 0; i < facility.UpgradeLevels.Length; i++)
                {

                    string modelName = "KSC_" + facility.name + "_level_" + (i + 1).ToString();
                    string modelTitle = "KSC " + facility.name + " lv " + (i + 1).ToString();


                    if (modelName == "KSC_Runway_level_2")
                    {
                        if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
                        {
                            PimpLv2Runway(facility.UpgradeLevels[i].facilityPrefab);
                            if (facility.UpgradeLevels[i].facilityInstance != null)
                            {
                                PimpLv2Runway(facility.UpgradeLevels[i].facilityInstance);
                            }

                        }
                    }

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                    {
                        continue;
                    }

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad KSC";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;

                    // the runways have all the same spawnpoint.
                    if (facility.name.Equals("Runway", StringComparison.CurrentCultureIgnoreCase))
                    {
                        model.DefaultLaunchPadTransform = "End09/SpawnPoint";
                    }

                    // Launchpads also 
                    if (facility.name.Equals("LaunchPad", StringComparison.CurrentCultureIgnoreCase))
                    {
                        model.DefaultLaunchPadTransform = "LaunchPad_spawn";
                    }

                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    //model.prefab = GameObject.Instantiate(facility.UpgradeLevels[i].facilityPrefab);
                    //GameObject.DontDestroyOnLoad(model.prefab);
                    //model.prefab.SetActive(false);
                    model.prefab = facility.UpgradeLevels[i].facilityPrefab;



                    ////Debug code for Material dumping
                    //foreach (Renderer renderer in model.prefab.GetComponentsInChildren<Renderer>(true))
                    //{
                    //    foreach (Material material in renderer.materials)
                    //    {
                    //        try
                    //        {
                    //            string textures = "";

                    //            foreach (string textName in material.GetTexturePropertyNames())
                    //            {
                    //                textures = textures + textName + ": " + material.GetTexture(textName).name + " ;  " + material.GetTextureScale(textName).ToString() + " ";
                    //            }

                    //            {
                    //                if (!material.HasProperty("_Color"))
                    //                {
                    //                    Log.Normal("Material: " + material.name + " : " + material.shader.name + " : " + textures + " : " + "No Color");
                    //                }
                    //                else
                    //                {
                    //                    Log.Normal("Material: " + material.name + " : " + material.shader.name + " : " + textures + " : " + material.color.ToString());
                    //                }
                    //                continue;
                    //            }
                    //        }
                    //        catch { }
                    //    }
                    //}


                    // Register new GasColor Module
                    bool hasGrasMaterial = false;
                    Material grassMaterial = null;
                    foreach (Renderer renderer in model.prefab.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (Material material in renderer.materials.Where(mat => mat.shader.name == "KSP/Scenery/Diffuse Ground KSC"))
                        {
                            //Log.Normal("Found GrassColor on: " + model.name);
                            grassMaterial = material;
                            hasGrasMaterial = true;
                            break;
                        }
                    }
                    if (hasGrasMaterial)
                    {
                        //Log.Normal("Adding GrasColor to: " + model.name);
                        StaticModule module = new StaticModule();
                        module.moduleNamespace = "KerbalKonstructs";
                        module.moduleClassname = "GrassColor2";
                        module.moduleFields.Add("DefaultBlendMaskTexture", "BUILTIN:/" + grassMaterial.GetTexture("_BlendMaskTexture").name);
                        module.moduleFields.Add("DefaultNearGrassTexture", "BUILTIN:/" + grassMaterial.GetTexture("_NearGrassTexture").name);
                        module.moduleFields.Add("DefaultFarGrassTexture", "BUILTIN:/" + grassMaterial.GetTexture("_FarGrassTexture").name);
                        module.moduleFields.Add("DefaultTarmacTexture", "BUILTIN:/" + grassMaterial.GetTexture("_TarmacTexture").name);
                        //module.moduleFields.Add("DefaultNearGrassTiling", grassMaterial.GetFloat("_NearGrassTiling").ToString());

                        //Log.Normal("Instance: " + model.name + " uses Near Tiling: " + grassMaterial.GetFloat("_NearGrassTiling").ToString());
                        //Log.Normal("Instance: " + model.name + " uses Far  Tiling: " + grassMaterial.GetFloat("_FarGrassTiling").ToString());
                        //module.moduleFields.Add("DefaultFarGrassTiling", grassMaterial.GetFloat("_FarGrassTiling").ToString());

                        //module.moduleFields.Add("DefaultFarGrassBlendDistance", grassMaterial.GetFloat("_FarGrassBlendDistance").ToString());
                        //Log.Normal("Instance: " + model.name + " uses Far Distance: " + grassMaterial.GetFloat("_FarGrassBlendDistance").ToString());
                        model.modules = new List<StaticModule>();
                        model.modules.Add(module);

                        TexturePreset preset = new TexturePreset();
                        preset.usage = TextureUsage.BlendMask;
                        preset.texturePath = "BUILTIN:/" + grassMaterial.GetTexture("_BlendMaskTexture").name;
                        if (TextureSelector.additionalTextures.Where(x => x.texturePath == preset.texturePath).Count() == 0)
                        {
                            TextureSelector.additionalTextures.Add(preset);
                        }

                    }

                    StaticDatabase.RegisterModel(model, modelName);

                    if (modelName == "KSC_SpaceplaneHangar_level_3")
                    {
                        CreateModelFromGameObject(model.prefab.transform.FindRecursive("Tank").gameObject, "KSC_FuelTank", "Tanks");
                        CreateModelFromGameObject(model.prefab.transform.FindRecursive("ksp_pad_cylTank").gameObject, "KSC_FuelTanks", "Tanks");
                        CreateModelFromGameObject(model.prefab.transform.FindRecursive("ksp_pad_waterTower").gameObject, "KSC_WaterTower", "Tanks");
                    }

                    // try to extract the wrecks from the facilities
                    var transforms = model.prefab.transform.GetComponentsInChildren<Transform>(true);
                    int wreckCount = 0;
                    foreach (var transform in transforms)
                    {

                        if (transform.name.Equals("wreck", StringComparison.InvariantCultureIgnoreCase))
                        {
                            wreckCount++;
                            StaticModel wreck = new StaticModel();
                            string wreckName = modelName + "_wreck_" + wreckCount.ToString();
                            wreck.name = wreckName;

                            // Fill in FakeNews errr values
                            wreck.path = "KerbalKonstructs/" + wreckName;
                            wreck.configPath = wreck.path + ".cfg";
                            wreck.keepConvex = true;
                            wreck.title = modelTitle + " wreck " + wreckCount.ToString();
                            wreck.mesh = wreckName;
                            wreck.category = "Squad KSC wreck";
                            wreck.author = "Squad";
                            wreck.manufacturer = "Squad";
                            wreck.description = "Squad original " + wreck.title;

                            wreck.isSquad = true;
                            wreck.prefab = GameObject.Instantiate(transform.gameObject);
                            GameObject.DontDestroyOnLoad(wreck.prefab);
                            wreck.prefab.SetActive(false);

                            //wreck.prefab.GetComponent<Transform>().parent = null;
                            StaticDatabase.RegisterModel(wreck, wreck.name);

                        }
                    }
                }
            }

        }

        private static void CreateModelFromGameObject(GameObject prefab, string modelName, string category)
        {
            StaticModel model = new StaticModel();
            model.isSquad = true;
            model.name = modelName;
            model.prefab = GameObject.Instantiate(prefab);
            GameObject.DontDestroyOnLoad(model.prefab);
            model.prefab.SetActive(false);
            // Fill in FakeNews errr values
            model.path = "KerbalKonstructs/" + modelName;
            model.configPath = model.path + ".cfg";
            model.keepConvex = true;
            model.title = modelName;
            model.mesh = modelName;
            model.category = category;
            model.author = "Squad";
            model.manufacturer = "Squad";
            model.description = "Squad original " + modelName;
            StaticDatabase.RegisterModel(model, modelName);
        }


        /// <summary>
        /// Loads all non KSC models into the ModelDatabase
        /// </summary>
        public static void LoadSquadAnomalies()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name == "KSC" || pqs.gameObject.name == "KSC2" || pqs.gameObject.name == "Pyramids" || pqs.gameObject.name == "Pyramid" || pqs.gameObject.name == "CommNetDish")
                {
                    continue;
                }

                string modelName = "SQUAD_" + pqs.gameObject.name;
                string modelTitle = "Squad " + pqs.gameObject.name;

                // don't double register the models a second time (they will do this) 
                // maybe with a "without green flag" and filter that our later at spawn in mangle
                if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                {
                    continue;
                }

                StaticModel model = new StaticModel();
                model.name = modelName;

                // Fill in FakeNews errr values
                model.path = "KerbalKonstructs/" + modelName;
                model.configPath = model.path + ".cfg";
                model.keepConvex = true;
                model.title = modelTitle;
                model.mesh = modelName;
                model.category = "Squad Anomalies";
                model.author = "Squad";
                model.manufacturer = "Squad";
                model.description = "Squad original " + modelTitle;

                model.isSquad = true;

                // we reference only the original prefab, as we cannot instantiate an instance for some reason
                //model.prefab = pqs.gameObject;
                model.prefab = GameObject.Instantiate(pqs.gameObject);
                GameObject.DontDestroyOnLoad(model.prefab);
                //model.prefab.SetActive(false);
                // activate them
                foreach (Transform child in model.prefab.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.SetActive(true);
                }
                model.prefab.SetActive(false);

                StaticDatabase.RegisterModel(model, modelName);
            }

            foreach (PQSCity2 pqs2 in Resources.FindObjectsOfTypeAll<PQSCity2>())
            {
                if (pqs2.gameObject.name == "Desert_Airfield")
                {
                    continue;
                }

                string modelName = "SQUAD_" + pqs2.gameObject.name;
                string modelTitle = "Squad " + pqs2.gameObject.name;

                if (modelName.Contains("Clone"))
                {
                    continue;
                }

                // don't double register the models a second time (they will do this) 
                // maybe with a "without green flag" and filter that our later at spawn in mangle
                if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                {
                    continue;
                }

                StaticModel model = new StaticModel();
                model.name = modelName;

                // Fill in FakeNews errr values
                model.path = "KerbalKonstructs/" + modelName;
                model.configPath = model.path + ".cfg";
                model.keepConvex = true;
                model.title = modelTitle;
                model.mesh = modelName;
                model.category = "Squad Anomalies";
                model.author = "Squad";
                model.manufacturer = "Squad";
                model.description = "Squad original " + modelTitle;

                model.isSquad = true;

                if (modelName == "SQUAD_MobileLaunchPad" || modelName == "SQUAD_Desert_Airfield")
                {
                    model.category = "Squad LaunchSite";
                    model.DefaultLaunchPadTransform = "SpawnPoint";
                }


                // we reference only the original prefab, as we cannot instantiate an instance for some reason
                model.prefab = pqs2.gameObject;
                //model.prefab = GameObject.Instantiate(pqs2.gameObject);
                //GameObject.DontDestroyOnLoad(model.prefab);
                //model.prefab.SetActive(false);
                StaticDatabase.RegisterModel(model, modelName);
            }
        }

        /// <summary>
        /// Loads the statics of the KSC2
        /// </summary>
        public static void LoadSquadAnomaliesLevel2()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name != "KSC2" && pqs.gameObject.name != "Pyramids" && pqs.gameObject.name != "CommNetDish")
                {
                    continue;
                }

                GameObject baseGameObject = pqs.gameObject;
                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject)
                    {
                        continue;
                    }

                    string modelName = "SQUAD_" + pqs.gameObject.name + "_" + child.gameObject.name;
                    string modelTitle = "Squad " + pqs.gameObject.name + " " + child.gameObject.name;

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                    {
                        continue;
                    }

                    // filter out some unneded stuff
                    if (modelName.Contains("ollider") || modelName.Contains("onolit"))
                    {
                        continue;
                    }

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad Anomalies";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;

                    if (model.name.Equals("SQUAD_KSC2_launchpad", StringComparison.CurrentCultureIgnoreCase))
                    {
                        model.DefaultLaunchPadTransform = "PlatformPlane";
                    }

                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = child.gameObject;
                    //model.prefab = GameObject.Instantiate(child.gameObject);
                    //GameObject.DontDestroyOnLoad(model.prefab);
                    //model.prefab.SetActive(false);

                    StaticDatabase.RegisterModel(model, modelName);
                }
            }
        }

        /// <summary>
        /// Loads the individual components of the desert Airfield into the static database
        /// </summary>
        public static void LoadDesertSiteAssets()
        {

            List<string> allStatics = new List<string> { "Model", "" };


            foreach (PQSCity2 pqs2 in Resources.FindObjectsOfTypeAll<PQSCity2>())
            {

                if (pqs2.gameObject == null)
                    continue;

                if (pqs2.gameObject.name != "Desert_Airfield")
                {
                    continue;
                }

                Log.Normal("found PQS2 " + pqs2.gameObject.name);

                GameObject baseGameObject = pqs2.gameObject;
                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    if (child.parent == null)
                    {
                        continue;
                    }

                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject)
                    {
                        continue;
                    }

                    Log.Normal("found child " + child.gameObject.name);

                    if (child.gameObject.name == "Model")
                    {
                        string modelName = "SQUAD_Desert_Runway";
                        string modelTitle = "Squad Desert Runway";

                        // don't double register the models a second time (they will do this) 
                        // maybe with a "without green flag" and filter that our later at spawn in mangle
                        if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                            continue;


                        StaticModel model = new StaticModel();
                        model.name = modelName;

                        // Fill in FakeNews errr values
                        model.path = "KerbalKonstructs/" + modelName;
                        model.configPath = model.path + ".cfg";
                        model.keepConvex = true;
                        model.title = modelTitle;
                        model.mesh = modelName;
                        model.category = "Squad Desert";
                        model.author = "Squad";
                        model.manufacturer = "Squad";
                        model.description = "Squad original " + modelTitle;

                        model.isSquad = true;

                        model.DefaultLaunchPadTransform = "End09/SpawnPoint";

                        // we reference only the original prefab, as we cannot instantiate an instance for some reason
                        model.prefab = GameObject.Instantiate(child.gameObject);
                        GameObject.DontDestroyOnLoad(model.prefab);
                        model.prefab.SetActive(false);

                        StaticDatabase.RegisterModel(model, modelName);
                    }

                    if (child.gameObject.name == "GroundObjects")
                    {
                        GameObject baseObject = child.gameObject;

                        foreach (var child2 in baseGameObject.GetComponentsInChildren<Transform>(true))
                        {
                            if (child2.parent == null)
                            {
                                continue;
                            }

                            // we only want to be one level down.
                            if (child2.parent.gameObject != baseObject)
                            {
                                continue;
                            }

                            //Filter out multiple instances or boken stuff
                            if (child2.gameObject.name.Contains("(") || child2.gameObject.name.Contains("windmill"))
                            {
                                continue;
                            }


                            //Log.Normal("found child2 " + child2.gameObject.name);

                            string modelName = "SQUAD_" + pqs2.gameObject.name + "_" + child2.gameObject.name;
                            string modelTitle = "Squad " + pqs2.gameObject.name + " " + child2.gameObject.name;

                            // don't double register the models a second time (they will do this) 
                            // maybe with a "without green flag" and filter that our later at spawn in mangle
                            if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                            {
                                continue;
                            }

                            StaticModel model = new StaticModel();
                            model.name = modelName;

                            // Fill in FakeNews errr values
                            model.path = "KerbalKonstructs/" + modelName;
                            model.configPath = model.path + ".cfg";
                            model.keepConvex = true;
                            model.title = modelTitle;
                            model.mesh = modelName;
                            model.category = "Squad Desert";
                            model.author = "Squad";
                            model.manufacturer = "Squad";
                            model.description = "Squad original " + modelTitle;

                            model.isSquad = true;

                            // we reference only the original prefab, as we cannot instantiate an instance for some reason
                            model.prefab = GameObject.Instantiate(child2.gameObject);
                            GameObject.DontDestroyOnLoad(model.prefab);
                            model.prefab.SetActive(false);

                            StaticDatabase.RegisterModel(model, modelName);

                        }
                    }
                }
            }
        }


        public static void MangleTrackingDishes()
        {
            // StrackingStation LV1
            //
            StaticModel model = StaticDatabase.GetModelByName("KSC_TrackingStation_level_1");
            List<DishController.Dish> dishes = new List<DishController.Dish>();

            DishController controller = model.prefab.AddComponent<DishController>();

            controller.fakeTimeWarp = 1f;
            controller.maxSpeed = 10f;
            controller.maxElevation = 20f;
            controller.minElevation = -70f;

            foreach (Transform dishTransform in model.prefab.transform.FindAllRecursive("TS_dish"))
            {
                Log.Normal("Dish: Found Dish");
                DishController.Dish dish = new DishController.Dish();

                dish.elevationTransform = dishTransform.FindRecursive("dish_antenna");
                //dish.elevationInit = new Quaternion();
                dish.rotationTransform = dishTransform.FindRecursive("dish_support");

                dish.elevationTransform.parent = dish.rotationTransform;

                dishes.Add(dish);

            }
            controller.dishes = dishes.ToArray();
            controller.enabled = true;


            // StrackingStation LV2
            //
            model = StaticDatabase.GetModelByName("KSC_TrackingStation_level_2");
            dishes.Clear();

            controller = model.prefab.AddComponent<DishController>();

            controller.fakeTimeWarp = 1f;
            controller.maxSpeed = 10f;
            controller.maxElevation = 20f;
            controller.minElevation = -70f;

            foreach (Transform dishTransform in model.prefab.transform.FindAllRecursive("TS_dish"))
            {
                Log.Normal("Dish: Found Dish");
                DishController.Dish dish = new DishController.Dish();

                dish.elevationTransform = dishTransform.FindRecursive("dish_antenna");
                //dish.elevationInit = new Quaternion();
                dish.rotationTransform = dishTransform.FindRecursive("dish_support");

                dish.elevationTransform.parent = dish.rotationTransform;

                dishes.Add(dish);
            }
            controller.dishes = dishes.ToArray();
            controller.enabled = true;


            // Extract LV3 Dish as Extra Dish
            model = StaticDatabase.GetModelByName("KSC_TrackingStation_level_3");

            Transform dishNorth = model.prefab.transform.FindRecursive("dish_north");
            string modelName = "SQUAD_LV3_Tracking_Dish";
            StaticModel dishModel = new StaticModel();
            dishModel.prefab = dishNorth.gameObject;
            // dishModel.prefab.transform.parent = null;
            dishModel.name = modelName;
            dishModel.path = "KerbalKonstructs/" + modelName;
            dishModel.configPath = model.path + ".cfg";
            dishModel.keepConvex = true;
            dishModel.title = "TrackingStation Lv3 Dish";
            dishModel.mesh = modelName;
            dishModel.category = "Dish";
            dishModel.author = "Squad";
            dishModel.manufacturer = "Squad";
            dishModel.description = "Squad original TrackingStation Lv3 Dish";

            dishModel.isSquad = true;

            StaticDatabase.RegisterModel(dishModel, dishModel.name);
        }

        /// <summary>
        /// used for loading the pyramid parts
        /// </summary>
        public static void LoadSquadAnomaliesLevel3()
        {

            foreach (PQSCity pqs in Resources.FindObjectsOfTypeAll<PQSCity>())
            {
                if (pqs.gameObject.name != "Pyramids")
                {
                    continue;
                }


                // find the lv2 parent
                GameObject baseGameObject = pqs.gameObject;
                GameObject baseGameObject2 = null;

                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject)
                    {
                        continue;
                    }
                    else
                    {
                        baseGameObject2 = child.gameObject;
                    }
                }


                foreach (var child in baseGameObject.GetComponentsInChildren<Transform>(true))
                {
                    // we only want to be one level down.
                    if (child.parent.gameObject != baseGameObject2)
                    {
                        continue;
                    }

                    string modelName = "SQUAD_" + pqs.gameObject.name + "_" + child.gameObject.name;
                    string modelTitle = "Squad " + pqs.gameObject.name + " " + child.gameObject.name;

                    // don't double register the models a second time (they will do this) 
                    // maybe with a "without green flag" and filter that our later at spawn in mangle
                    if (StaticDatabase.allStaticModels.Select(x => x.name).Contains(modelName))
                        continue;

                    StaticModel model = new StaticModel();
                    model.name = modelName;

                    // Fill in FakeNews errr values
                    model.path = "KerbalKonstructs/" + modelName;
                    model.configPath = model.path + ".cfg";
                    model.keepConvex = true;
                    model.title = modelTitle;
                    model.mesh = modelName;
                    model.category = "Squad Anomalies";
                    model.author = "Squad";
                    model.manufacturer = "Squad";
                    model.description = "Squad original " + modelTitle;

                    model.isSquad = true;


                    // we reference only the original prefab, as we cannot instantiate an instance for some reason
                    model.prefab = child.gameObject;


                    StaticDatabase.RegisterModel(model, modelName);
                }
            }
        }

        internal static void ExtractDesertLights()
        {
            StaticModel model = StaticDatabase.GetModelByName("SQUAD_Desert_Runway");

            Transform lights1 = model.prefab.transform.FindRecursive("Section1_lights");
            string modelName = "SQUAD_Runway_Lights";
            StaticModel lightsModel = new StaticModel();
            lightsModel.prefab = lights1.gameObject;
            // dishModel.prefab.transform.parent = null;
            lightsModel.name = modelName;
            lightsModel.path = "KerbalKonstructs/" + modelName;
            lightsModel.configPath = model.path + ".cfg";
            lightsModel.keepConvex = true;
            lightsModel.title = "Desert Runway Lights";
            lightsModel.mesh = modelName;
            lightsModel.category = "Runway";
            lightsModel.author = "Squad";
            lightsModel.manufacturer = "Squad";
            lightsModel.description = "Squad original Desert Runway Lights";

            lightsModel.isSquad = true;

            StaticDatabase.RegisterModel(lightsModel, lightsModel.name);
        }



        internal static void PimpLv2Runway(GameObject modelPrefab, bool state = false)
        {
            StaticModel modelLights = StaticDatabase.GetModelByName("SQUAD_Runway_Lights");
            //Log.Normal("Prefab name: " + modelPrefab.name);
            int count = 0;
            foreach (Transform target in modelPrefab.transform.FindAllRecursive("fxTarget"))
            {
                GameObject light = GameObject.Instantiate(modelLights.prefab);
                light.SetActive(state);
                //Log.Normal("found target: " + target.parent.name);
                light.transform.parent = target.parent.FindRecursiveContains("runway");
                light.transform.localScale *= 0.6f;

                light.transform.rotation = modelPrefab.transform.rotation;

                light.transform.localPosition = new Vector3(6.5f, 0.85f, -1050f + count * 330);

                light.name = light.transform.parent.name + "_lights";

                count++;
            }
        }
    }
}
