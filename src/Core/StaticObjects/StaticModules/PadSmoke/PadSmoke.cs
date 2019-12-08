using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace KerbalKonstructs
{
    public class PadSmoke : StaticModule
    {

        public string smokeReceiverName = "";
        public string smokeEmittersNames = "";
        //public string smokeName = "PadSmokeLvl2";
        public string smokeName = "KKPadSmoke1";

        private List<string> emitterTransforms = new List<string>();
        private string[] seperators = new string[] { ",", ";" };
        

        GameObject baseObject;


        public void Start()
        {
            //Log.Normal("PadSmoke Start");
            baseObject = gameObject;
            var tmpList = smokeEmittersNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string value in tmpList)
            {
                emitterTransforms.Add(value.Trim());
            }

            Transform receiverTransform = gameObject.transform.FindRecursive(smokeReceiverName);
            Collider receiverCollider = receiverTransform.gameObject.GetComponent<Collider>();

            if (receiverCollider != null)
            {
                receiverCollider.tag = "LaunchpadFX";
                //Log.Normal("Collider Tag: " + receiverCollider.tag);
                receiverCollider.gameObject.layer = 15;
                KKPadFX padfx = receiverCollider.gameObject.AddOrGetComponent<KKPadFX>();
                padfx.Setup(emitterTransforms, gameObject, smokeName);
            }
            else
            {
                Log.Warning("PadFX: Collider not found " + smokeReceiverName);
            }
        }
    }

    public class KKPadFX : LaunchPadFX
    {

        internal static bool isInitialized = false;

        private static List<string> stockNames = new List<string> { "PadSmokeLvl2", "PadSmokeLvl3" };

        private FieldInfo totalFXField;
        private FieldInfo amoutfield ;

        internal  struct KKEmitter
        {
            internal List<ParticleSystem> pSystems ;
            internal List<KSPParticleEmitter> kspEmitters;
        }

        internal static Dictionary<string,KKEmitter> particleSystems = new Dictionary<string, KKEmitter>();
        KSPParticleEmitter[] kspPS;

        /// <summary>
        /// Attaches the LaunchPadFX module and 
        /// </summary>
        /// <param name="emitterTransformNames"></param>
        /// <param name="baseObject"></param>
        public void Setup(List<string> emitterTransformNames, GameObject baseObject, string smokeName)
        {
            InitializePSystems();
            List<ParticleSystem> unityEmitters = new List<ParticleSystem>();
            List<KSPParticleEmitter> kspEmitters = new List<KSPParticleEmitter>();

            totalFXField = typeof(LaunchPadFX).GetField("totalFX", BindingFlags.NonPublic | BindingFlags.Instance);
            amoutfield = typeof(KSPParticleEmitter).GetField("amountPerSec", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (string emName in emitterTransformNames)
            {
                foreach (Transform emTransform in baseObject.transform.FindAllRecursive(emName))
                {

                    if (particleSystems.ContainsKey(smokeName))
                    {
                        if (particleSystems[smokeName].pSystems != null)
                        {
                            foreach (ParticleSystem pSystem in particleSystems[smokeName].pSystems)
                            {
                                //Log.Normal("adding PSystem: " + pSystem.name);
                                ParticleSystem emPsystem = Instantiate(pSystem, emTransform.position, emTransform.rotation, emTransform);
                                emPsystem.gameObject.SetActive(true);
                                unityEmitters.Add(emPsystem);
                                FloatingOrigin.RegisterParticleSystem(emPsystem);
                            }
                        }
                        if (particleSystems[smokeName].kspEmitters != null)
                        {
                            foreach (KSPParticleEmitter pSystem in particleSystems[smokeName].kspEmitters)
                            {
                                //Log.Normal("adding PSystem: " + pSystem.name);
                                KSPParticleEmitter emPsystem = Instantiate(pSystem, emTransform.position, emTransform.rotation, emTransform);
                                emPsystem.gameObject.SetActive(true);
                                kspEmitters.Add(emPsystem);
                            }
                        }


                        } 
                    else
                    {
                        Log.UserError("Cannot find a LaunchPad Smoke with name: " + smokeName);
                    }


                }
            }
            // assign the emitters to the underlying component
            ps = unityEmitters.ToArray();
            kspPS = kspEmitters.ToArray();
        }


        private void LateUpdate()
        {

            foreach (ParticleSystem pSystem in ps)
            {
                if (fxScale > 0f)
                {
                    ParticleSystem.MainModule main = pSystem.main;
                    if (!pSystem.isPlaying)
                    {
                        pSystem.Play();
                    }
                    if (!pSystem.emission.enabled)
                    {
                        ParticleSystem.EmissionModule emission = pSystem.emission;
                        emission.enabled = true;
                    }
                    main.startColor = main.startColor.color.SetAlpha(Mathf.Lerp(0f, 0.5f, fxScale));
                }
                if (fxScale == 0f)
                {
                    ParticleSystem.EmissionModule emission = pSystem.emission;
                    emission.enabled = false;
                }
            }

            foreach (KSPParticleEmitter kspSystem in kspPS)
            {
                if (fxScale > 0f)
                {
                    kspSystem.emit = true;

                    kspSystem.doesAnimateColor = false;
                    kspSystem.color = kspSystem.color.SetAlpha(Mathf.Lerp(0f, 0.5f, fxScale));
                }


                if (fxScale == 0f)
                {
                    kspSystem.doesAnimateColor = true;
                    kspSystem.emit = false;
                }

            }
            // reset the emissions

            fxScale = 0f;
            totalFXField.SetValue(this, 0f);
        }



        //static functions below

        /// <summary>
        /// Load the assets into memory
        /// </summary>
        internal static void InitializePSystems()
        {
            if (!isInitialized)
            {
                GetSquadPsystem();
                CustomSmoke();
                isInitialized = true;
            }
        }



        internal static void GetSquadPsystem()
        {
            ParticleSystem pSystem;

            foreach (string psName in stockNames)
            {
                Log.Normal("searching for: " + psName);
                pSystem = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where(ps => ps.name == psName).FirstOrDefault();

                if (pSystem != null)
                {
                    Log.Normal("found: " + psName);


                    KKEmitter emitter = new KKEmitter { pSystems = new List<ParticleSystem> { pSystem } };

                    particleSystems.Add(psName, emitter);
                }
            }
        }


        internal static void CustomSmoke()
        {
            foreach (UrlDir.UrlConfig smokeConfig in GameDatabase.Instance.GetConfigs("KKLaunchPadSmoke"))
            {
                ConfigNode smokeNode = smokeConfig.config;
                string path = Path.GetDirectoryName(Path.GetDirectoryName(smokeConfig.url)).Replace("\\", "/");

                if (!smokeNode.HasValue("Name"))
                {
                    continue;
                }
                
                string name = smokeNode.GetValue("Name");
                if (particleSystems.ContainsKey(name))
                {
                    continue;
                }

                float intensity = 1;
                if (smokeNode.HasValue("Intensity"))
                {
                    intensity = float.Parse(smokeNode.GetValue("Intensity"));
                }

                List<ParticleSystem> psEmitters = new List<ParticleSystem>();
                List<KSPParticleEmitter> kspEmitters = new List<KSPParticleEmitter>();

                foreach (string meshname in smokeNode.GetValues("EmitterPath"))
                {
                    GameObject modelObject = GameDatabase.Instance.GetModelPrefab(path + "/" + meshname);

                    if (modelObject == null)
                    {
                        modelObject = GameDatabase.Instance.GetModelPrefab(meshname);
                    }
                    if (modelObject == null)
                    {
                        Log.UserError("could not load smoke asset: " + name + " :  " + meshname);
                        Log.UserError("last error from file: " + smokeConfig.url);
                        continue;
                    }

                    ParticleSystem ps = null;

                    KSPParticleEmitter kspEmitter = modelObject.GetComponent<KSPParticleEmitter>();
                    if (kspEmitter != null)
                    {
                        kspEmitters.Add(kspEmitter);
                    } 
                    else
                    {
                        ps = modelObject.GetComponent<ParticleSystem>();
                    }

                    if (ps == null)
                    {            
                        continue;
                    }

                    var main = ps.main;

                    main.loop = true;
                    main.playOnAwake = false;
                    if (kspEmitter == null)
                    {
                        psEmitters.Add(ps);
                    }
                }

                KKEmitter kKEmitter = new KKEmitter { kspEmitters = kspEmitters, pSystems = psEmitters };
                particleSystems.Add(name, kKEmitter);

            }
        }

        //private static ParticleSystem ParseKSPEmitter(KSPParticleEmitter kspEmitter)
        //{
        //    kspEmitter.SetDirty();
        //    kspEmitter.SetupProperties();

        //    ParticleSystem ps = kspEmitter.ps;
        //    var emSystem = ps.emission;
        //    emSystem.rateOverTime = new ParticleSystem.MinMaxCurve(kspEmitter.minEmission, kspEmitter.maxEmission);

        //    var main = ps.main;
        //    main.simulationSpace = ParticleSystemSimulationSpace.Local;

        //    main.startLifetime = new ParticleSystem.MinMaxCurve(kspEmitter.minEnergy, kspEmitter.maxEnergy);
        //    main.startSize = new ParticleSystem.MinMaxCurve(kspEmitter.minSize, kspEmitter.maxSize);
        //    float minSpeed = kspEmitter.localVelocity.magnitude;
        //    float maxSpeed = (kspEmitter.localVelocity + kspEmitter.rndVelocity).magnitude;
        //    main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);

        //    main.loop = true;
        //    main.playOnAwake = false;
        //    GameObject.DestroyImmediate(kspEmitter);
        //    return ps;
        //}

    }
}
