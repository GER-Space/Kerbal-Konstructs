using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Applies the look of a model's AdvancedTextures, GrassColor2 and GrasColor modules to a
    /// preview copy of the model (see ModelPreviewRenderer).
    /// The real modules need a placed static (its body, position and group), so they can't run on a
    /// preview. This does the same material changes from the module settings in the model config,
    /// with the default grass color. Tiling that depends on the static's scale is left out.
    /// All materials it changes are copies; they are returned in createdMaterials so the caller can
    /// destroy them with the preview.
    /// </summary>
    internal static class ModelPreviewModules
    {
        private static readonly string[] seperators = new string[] { ",", ";" };


        internal static void Apply(StaticModel model, GameObject previewMeshes, List<Material> createdMaterials)
        {
            if (model == null || model.modules == null || previewMeshes == null)
            {
                return;
            }
            // renderers whose materials were already copied for this preview
            HashSet<Renderer> ownMaterials = new HashSet<Renderer>();

            foreach (StaticModule module in model.modules)
            {
                // The model's module entries are created with "new StaticModule()". A MonoBehaviour made
                // with new has no Unity object behind it, so Unity's == operator says it is null even though
                // the C# object and its fields are fine. Check the C# reference instead.
                if (ReferenceEquals(module, null))
                {
                    continue;
                }
                try
                {
                    switch (module.moduleClassname)
                    {
                        case "AdvancedTextures":
                            ApplyAdvancedTextures(module.moduleFields, previewMeshes, ownMaterials, createdMaterials);
                            break;
                        case "GrassColor2":
                            ApplyGrassColor2(model, module.moduleFields, previewMeshes, ownMaterials, createdMaterials);
                            break;
                        case "GrasColor":
                            ApplyGrasColor(module.moduleFields, previewMeshes, ownMaterials, createdMaterials);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Normal("ModelPreview: could not apply " + module.moduleClassname + " to " + model.name + ": " + e.Message);
                }
            }
        }


        #region helpers

        private static string GetField(Dictionary<string, string> fields, string name, string defaultValue)
        {
            string value;
            if (fields != null && fields.TryGetValue(name, out value) && value != null)
            {
                return value;
            }
            return defaultValue;
        }

        private static float GetFloat(Dictionary<string, string> fields, string name, float defaultValue)
        {
            float value;
            if (float.TryParse(GetField(fields, name, ""), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            return defaultValue;
        }

        private static bool GetBool(Dictionary<string, string> fields, string name, bool defaultValue)
        {
            bool value;
            if (bool.TryParse(GetField(fields, name, ""), out value))
            {
                return value;
            }
            return defaultValue;
        }

        private static Color GetColor(Dictionary<string, string> fields, string name, Color defaultValue)
        {
            string text = GetField(fields, name, "");
            if (string.IsNullOrEmpty(text))
            {
                return defaultValue;
            }
            try
            {
                return ConfigNode.ParseColor(text);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static Vector2 GetVector2(Dictionary<string, string> fields, string name, Vector2 defaultValue)
        {
            string text = GetField(fields, name, "");
            if (string.IsNullOrEmpty(text))
            {
                return defaultValue;
            }
            try
            {
                return ConfigNode.ParseVector2(text);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// The renderer's materials, copied once per preview so the shared model materials stay untouched.
        /// </summary>
        private static Material[] GetOwnMaterials(Renderer renderer, HashSet<Renderer> ownMaterials, List<Material> createdMaterials)
        {
            if (!ownMaterials.Contains(renderer))
            {
                Material[] copies = renderer.sharedMaterials.Select(material => (material != null) ? new Material(material) : null).ToArray();
                createdMaterials.AddRange(copies.Where(material => material != null));
                renderer.sharedMaterials = copies;
                ownMaterials.Add(renderer);
            }
            return renderer.sharedMaterials;
        }

        private static IEnumerable<Renderer> RenderersNamed(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.transform.name == name);
        }

        #endregion


        /// <summary>
        /// (like AdvancedTextures.Start, without scale tiling)
        /// </summary>
        private static void ApplyAdvancedTextures(Dictionary<string, string> fields, GameObject root, HashSet<Renderer> ownMaterials, List<Material> createdMaterials)
        {
            string transforms = GetField(fields, "transforms", "Any");
            string newMaterial = GetField(fields, "newMaterial", "");
            string newShader = GetField(fields, "newShader", null);
            int textureIndex = 0;
            int.TryParse(GetField(fields, "BuiltinIndex", "0"), out textureIndex);
            string colorText = GetField(fields, "_Color", "");

            bool anyTransform = transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase);
            List<string> targetTransforms = transforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).Select(value => value.Trim()).ToList();

            Material foundMaterial = string.IsNullOrEmpty(newMaterial) ? null : KKGraphics.GetMaterial(newMaterial);
            Shader foundShader = string.IsNullOrEmpty(newShader) ? null : KKGraphics.GetShader(newShader);

            string[] textureProperties = { "_MainTex", "_ParallaxMap", "_Emissive", "_EmissionMap", "_MetallicGlossMap", "_OcclusionMap", "_SpecGlossMap", "_BumpMap" };

            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!anyTransform && !targetTransforms.Contains(renderer.transform.name))
                {
                    continue;
                }

                Material[] materials = GetOwnMaterials(renderer, ownMaterials, createdMaterials);
                if (materials.Length == 0 || materials[0] == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(newMaterial))
                {
                    // AdvancedTextures replaces the (first) material and does nothing else
                    if (foundMaterial != null)
                    {
                        Material replacement = new Material(foundMaterial);
                        createdMaterials.Add(replacement);
                        materials[0] = replacement;
                        renderer.sharedMaterials = materials;
                    }
                    continue;
                }

                Material material = materials[0];
                if (foundShader != null)
                {
                    material.shader = foundShader;
                }
                if (!string.IsNullOrEmpty(colorText))
                {
                    material.SetColor("_Color", GetColor(fields, "_Color", Color.white));
                }
                foreach (string property in textureProperties)
                {
                    string textureName = GetField(fields, property, null);
                    if (string.IsNullOrEmpty(textureName))
                    {
                        continue;
                    }
                    Texture2D texture = KKGraphics.GetTexture(textureName, property == "_BumpMap", textureIndex);
                    if (texture != null)
                    {
                        material.SetTexture(property, texture);
                    }
                }
            }
        }


        /// <summary>
        /// (like GrassColor2 with its default settings and the default grass color)
        /// </summary>
        private static void ApplyGrassColor2(StaticModel model, Dictionary<string, string> fields, GameObject root, HashSet<Renderer> ownMaterials, List<Material> createdMaterials)
        {
            string grassMeshName = GetField(fields, "GrassMeshName", "Nix");
            Shader groundShader = KKTerrainShader.GroundShader;
            if (groundShader == null)
            {
                return;
            }

            List<Material> grassMaterials = new List<Material>();
            foreach (Renderer renderer in RenderersNamed(root, grassMeshName))
            {
                Material[] materials = GetOwnMaterials(renderer, ownMaterials, createdMaterials);
                if (model.isSquad)
                {
                    // stock KSC models: only the ground materials
                    foreach (Material material in materials.Where(material => material != null && material.shader.name.StartsWith("KSP/Scenery/Diffuse Ground KSC")))
                    {
                        material.shader = groundShader;
                        grassMaterials.Add(material);
                    }
                }
                else if (materials.Length > 0 && materials[0] != null)
                {
                    materials[0].shader = groundShader;
                    grassMaterials.Add(materials[0]);
                }
            }

            Texture2D nearGrass = KKGraphics.GetTexture(GetField(fields, "DefaultNearGrassTexture", "BUILTIN:/terrain_grass00_new"));
            Texture2D farGrass = KKGraphics.GetTexture(GetField(fields, "DefaultFarGrassTexture", "BUILTIN:/terrain_grass00_new_detail"));
            Texture2D tarmac = KKGraphics.GetTexture(GetField(fields, "DefaultTarmacTexture", "BUILTIN:/ksc_exterior_terrain_asphalt"));
            Texture2D blendMask = KKGraphics.GetTexture(GetField(fields, "DefaultBlendMaskTexture", "BUILTIN:/blackSquare"));
            string thirdName = GetField(fields, "DefaultThirdTexture", "");
            string fourthName = GetField(fields, "DefaultFourthTexture", "");
            Texture2D third = string.IsNullOrEmpty(thirdName) ? null : KKGraphics.GetTexture(thirdName);
            Texture2D fourth = string.IsNullOrEmpty(fourthName) ? null : KKGraphics.GetTexture(fourthName);
            Vector2 tarmacTiling = GetVector2(fields, "DefaultTarmacTiling", new Vector2(10, 10));

            foreach (Material material in grassMaterials)
            {
                GrassColor2.SetFlatNormals(material);
                material.SetColor("_GrassColor", GrassColor2.defaultColor);
                material.SetColor("_TarmacColor", GetColor(fields, "DefaultTarmacColor", Color.white));
                material.SetFloat("_NearGrassTiling", GetFloat(fields, "DefaultNearGrassTiling", 0.03f));
                material.SetFloat("_FarGrassTiling", GetFloat(fields, "DefaultFarGrassTiling", 0.01f));
                material.SetFloat("_FarGrassBlendDistance", GetFloat(fields, "DefaultFarGrassBlendDistance", 100f));
                material.SetTexture("_NearGrassTexture", nearGrass);
                material.SetTexture("_FarGrassTexture", farGrass);
                material.SetTexture("_TarmacTexture", tarmac);
                if (tarmac != null)
                {
                    material.SetTextureScale("_TarmacTexture", tarmacTiling);
                }
                material.SetFloat("_TarmacTileRandom", (tarmac != null && GetBool(fields, "DefaultTarmacTileRandom", false)) ? 1f : 0f);
                material.SetTexture("_BlendMaskTexture", blendMask);

                material.SetTexture("_ThirdTexture", third);
                material.SetFloat("_ThirdTextureTileRandom", (third != null && GetBool(fields, "DefaultThirdTextureTileRandom", false)) ? 1f : 0f);
                material.SetColor("_ThirdTextureColor", GetColor(fields, "DefaultThirdTextureColor", Color.clear));
                material.SetFloat("_ThirdTextureTiling", GetFloat(fields, "DefaultThirdTextureTiling", 1f));

                material.SetTexture("_FourthTexture", fourth);
                material.SetFloat("_FourthTextureTileRandom", (fourth != null && GetBool(fields, "DefaultFourthTextureTileRandom", false)) ? 1f : 0f);
                material.SetColor("_FourthTextureColor", GetColor(fields, "DefaultFourthTextureColor", Color.clear));
                material.SetFloat("_FourthTextureTiling", GetFloat(fields, "DefaultFourthTextureTiling", 1f));

                material.SetFloat("_NearGrassGrayScale", GetBool(fields, "DefaultNearGrassTextureGray", false) ? 1f : 0f);
                material.SetFloat("_FarGrassGrayScale", GetBool(fields, "DefaultFarGrassTextureGray", false) ? 1f : 0f);
                material.SetFloat("_TarmacGrayScale", GetBool(fields, "DefaultTarmacTextureGray", false) ? 1f : 0f);
                material.SetFloat("_ThirdTextureGrayScale", GetBool(fields, "DefaultThirdTextureGray", false) ? 1f : 0f);
                material.SetFloat("_FourthTextureGrayScale", GetBool(fields, "DefaultFourthTextureGray", false) ? 1f : 0f);
            }
        }


        /// <summary>
        /// (like the legacy GrasColor module, with the default grass color)
        /// </summary>
        private static void ApplyGrasColor(Dictionary<string, string> fields, GameObject root, HashSet<Renderer> ownMaterials, List<Material> createdMaterials)
        {
            string grasMeshName = GetField(fields, "GrasMeshName", null);
            if (string.IsNullOrEmpty(grasMeshName))
            {
                return;
            }
            Shader shader = KKGraphics.GetShader("KK/Diffuse_Multiply_Random");
            Texture2D texture = KKGraphics.GetTexture(GetField(fields, "GrasTextureImage", "BUILTIN:/terrain_grass00_new"), false, 0, true);
            string normalMapName = GetField(fields, "GrasTextureNormalMap", null);
            Texture2D normalMap = string.IsNullOrEmpty(normalMapName) ? null : KKGraphics.GetTexture(normalMapName, true, 0, true);

            foreach (Renderer renderer in RenderersNamed(root, grasMeshName))
            {
                Material[] materials = GetOwnMaterials(renderer, ownMaterials, createdMaterials);
                if (materials.Length == 0 || materials[0] == null)
                {
                    continue;
                }
                Material material = materials[0];
                if (shader != null)
                {
                    material.shader = shader;
                }
                material.SetColor("_Color", GrassColor2.defaultColor);
                if (texture != null)
                {
                    material.mainTexture = texture;
                    if (normalMap != null)
                    {
                        material.SetTexture("_BumpMap", normalMap);
                    }
                }
            }
        }
    }
}
