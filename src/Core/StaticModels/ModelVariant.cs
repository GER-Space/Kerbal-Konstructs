using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class ModelVariant
    {

        public string name = "Default";
        public string newMeshName = "";
        public string deactivateTransforms = "";
        //public string activateTransforms = "";


        internal StaticModel model;

        internal GameObject prefab;

        // private List<string> transforms2Activate = new List<string>();
        private List<string> transforms2Deactivate = new List<string>();

        internal List<TextureSet> textureSets = new List<TextureSet>();

        private string[] seperators = new string[] { " ", ",", ";" };

        private ConfigNode origVariantNode;


        internal ModelVariant(StaticModel baseModel, ConfigNode variantNode)
        {
            origVariantNode = variantNode;
            model = baseModel;

            variantNode.TryGetValue("name", ref name);

            variantNode.TryGetValue("newMeshName", ref newMeshName);
            variantNode.TryGetValue("deactivateTransforms", ref deactivateTransforms);
            //            variantNode.TryGetValue("activateTransforms", ref activateTransforms);

            //            transforms2Activate = actiateTransforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            transforms2Deactivate = deactivateTransforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();


            model.hasVariants = true;

            foreach (ConfigNode tsNode in variantNode.GetNodes("TextureSet"))
            {
                //Log.Normal("found TextureSet:");
                if (!tsNode.HasValue("transforms"))
                {
                    continue;
                }
                TextureSet set = new TextureSet();
                tsNode.TryGetValue("color", ref set.newColor);
                tsNode.TryGetValue("texture", ref set.newTexture);
                set.targetTransforms = tsNode.GetValue("transforms").Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
                textureSets.Add(set);
            }
        }

        internal ConfigNode GetConfig()
        {
            return origVariantNode;
        }



        internal static GameObject SpawnVariant(StaticInstance instance)
        {
            if (instance.model.hasVariants)
            {
                if (!String.IsNullOrEmpty(instance.VariantName))
                {

                    if (instance.model.variants.ContainsKey(instance.VariantName))
                    {

                        ModelVariant variant = instance.model.variants[instance.VariantName];
                        if (String.IsNullOrEmpty(variant.newMeshName))
                        {
                            variant.newMeshName = instance.model.mesh;
                            variant.prefab = instance.model.prefab;
                        }
                        else
                        {
                            variant.prefab = GameDatabase.Instance.GetModelPrefab(instance.model.path + "/" + variant.newMeshName);
                            if (variant.prefab == null)
                            {
                                Log.UserError("Could not find " + instance.model.path + "/" + variant.newMeshName + ".mu! Did the modder forget to include it or did you actually install it?");
                            }
                        }

                        return GameObject.Instantiate(variant.prefab);
                    }
                    else
                    {
                        Log.UserError("Cannot find variant: " + instance.VariantName + "on model " + instance.model.name);
                        return null;
                    }
                }
                else
                {
                    // Default is the first one
                    instance.VariantName = instance.model.variants.Keys.ToArray()[0];
                    return SpawnVariant(instance);
                }
            }
            else
            {
                return GameObject.Instantiate(instance.model.prefab);
            }
        }

        internal static void ApplyVariant(StaticInstance instance)
        {
            if (!instance.model.hasVariants || String.IsNullOrEmpty(instance.VariantName))
            {
                return;
            }

            if (!(instance.model.variants.ContainsKey(instance.VariantName)))
            {
                Log.UserError("Cannot find variant: " + instance.VariantName + "on model " + instance.model.name);
                return;
            }

            ModelVariant variant = instance.model.variants[instance.VariantName];

            // apply Textures before altering them later
            foreach (AdvancedTextures advTexture in instance.gameObject.GetComponentsInChildren<AdvancedTextures>(true))
            {
                advTexture.Start();
            }


            variant.AlterTexture(instance.mesh);

            variant.DeactivateGameObjects(instance.mesh);

            if (instance == UI.EditorGUI.selectedInstance)
            {
                instance.gameObject.BroadcastMessage("StaticObjectUpdate");
            }

        }



        internal void DeactivateGameObjects(GameObject gameObject)
        {
            foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (transforms2Deactivate.Contains(transform.name))
                {
                    transform.gameObject.SetActive(false);
                }
            }

        }

        internal void ActivateGameObjects(GameObject gameObject)
        {


        }

        internal void AlterTexture(GameObject gameObject)
        {
            //Log.Normal("called");

            foreach (TextureSet set in textureSets)
            {

                //Log.Normal("Apply TextureSet");
                foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (!set.targetTransforms.Contains("Any") && !set.targetTransforms.Contains(renderer.transform.name))
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(set.newTexture))
                    {
                        renderer.material.mainTexture = KKGraphics.GetTexture(set.newTexture);
                    }

                    if (!String.IsNullOrEmpty(set.newColor))
                    {
                        Color color = ConfigNode.ParseColor(set.newColor);

                        renderer.material.color = color;
                    }
                }
            }
        }


        internal class TextureSet
        {
            internal string newTexture = "";
            internal string newColor = "";
            internal List<string> targetTransforms = new List<string>();
        }

    }
}
