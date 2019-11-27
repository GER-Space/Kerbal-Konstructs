using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs
{
    public class PadSmoke : StaticModule
    {

        public string smokeReceiverName = "";
        public string smokeEmittersNames = "";


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
                padfx.Setup(emitterTransforms, gameObject);
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
        internal static ParticleSystem pSystem = null;



        public void Setup(List<string> emitterTransformNames, GameObject baseObject)
        {
            GetSquadPsystem();

            List<ParticleSystem> emitters = new List<ParticleSystem>();

            foreach (string emName in emitterTransformNames)
            {
                foreach (Transform emTransform in baseObject.transform.FindAllRecursive(emName))
                {
                    ParticleSystem emPsystem = Instantiate(pSystem, emTransform.position, emTransform.rotation, emTransform);

                    emitters.Add(emPsystem);
                    FloatingOrigin.RegisterParticleSystem(emPsystem);
                }

            }

            ps = emitters.ToArray();

        }

        internal static void GetSquadPsystem()
        {
            if (!isInitialized)
            {
                pSystem = Resources.FindObjectsOfTypeAll<ParticleSystem>().Where(ps => ps.name == "PadSmokeLvl2").First();

                if (pSystem == null )
                {
                    Log.UserError("Failed to Setup Particle Systems");
                    return;
                }
                else
                {
                    isInitialized = true;
                }
            }

        }


    }


}
