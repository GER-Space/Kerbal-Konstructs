using KerbalKonstructs.Core;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs
{
    /// <summary>
    /// Ground meshes drawn like the terrain underneath (shader KK/Terrain). It replaces the GrasColor and GrassColor2
    /// modules (KerbalKonstructs.LoadModels, SquadStatics) and keeps GrassColor2's settings and config: tarmac, blend mask,
    /// third and fourth texture.
    /// The grass is the planet's own low/mid/high, steep and bump textures with a terrain color:
    ///  - models converted from GrasColor: the terrain color at the static's position (the PQS vertex color)
    ///  - the other models: their grass color setting (GrassColor of the instance config)
    /// overwrite_color decides it: empty (default) chooses as above, true uses the grass color, false the terrain color.
    /// The grass editor sets it per instance.
    /// The PQS is sampled once per instance at the static's position (its alpha also chooses the low/mid/high textures):
    /// the sample is kept in the instance's GrassColor2 config node (TerrainSample, TerrainSamplePosition), saved with
    /// the instance, and only taken again when the static was moved. A scene switch (Activate) doesn't apply the settings
    /// again unless the static moved.
    /// The grass texture settings of GrassColor2 are not used by KK/Terrain. With an older shader bundle without
    /// KK/Terrain, it works like GrassColor2.
    /// </summary>
    public class KKTerrain : GrassColor2
    {
        /// <summary>
        /// Model setting (and saved per instance): "true" uses the grass color, "false" the terrain color, empty chooses
        /// by the model (converted from GrasColor: terrain color, else the grass color)
        /// </summary>
        public string overwrite_color = "";

        /// <summary>
        /// Model setting, set by ReplaceGrassModule: this module replaces a GrasColor module
        /// </summary>
        public bool ConvertedFromGrasColor = false;

        /// <summary>
        /// The terrain color at the static (shown in the grass editor while the terrain color is used)
        /// </summary>
        internal Color terrainColor = defaultColor;

        /// <summary>
        /// The static moves less than this (meters) between two samples
        /// </summary>
        private const double resampleDistance = 1d;

        private bool instanceConfigRead = false;
        private bool hasTerrainSample = false;
        private Color terrainSample = Color.clear;
        private Vector3d terrainSamplePosition = Vector3d.zero;

        // the planet frame position of the static when the settings were applied last
        private bool applied = false;
        private Vector3d appliedPosition = Vector3d.zero;
        private Quaternion appliedRotation = Quaternion.identity;
        private Vector3 appliedScale = Vector3.one;

        protected override Shader GroundShader
        {
            get
            {
                Shader terrainShader = KKTerrainShader.TerrainShader;
                return terrainShader != null ? terrainShader : base.GroundShader;
            }
        }

        /// <summary>
        /// True when the grass meshes use KK/Terrain (not the old shader of an older bundle)
        /// </summary>
        internal bool UsesTerrainShader => grassMaterials.Count > 0 && grassMaterials[0] != null && grassMaterials[0].HasProperty("_KKTerrainMode");

        /// <summary>
        /// True: the grass color is used, false: the terrain color at the static
        /// </summary>
        internal bool OverwriteColor
        {
            get
            {
                bool value;
                if (bool.TryParse(overwrite_color, out value))
                {
                    return value;
                }
                return !ConvertedFromGrasColor;
            }
        }


        /// <summary>
        /// Value of the instance's GrassColor2 config node: true (default) draws the grass mesh with KKTerrain (terrain
        /// shader), false with GrassColor2 (KK/Ground_Multi_NoUV and the grass editor's textures). Switched in the grass
        /// editor, which respawns the instance: StaticInstance.Spawn attaches the module of the saved choice.
        /// </summary>
        internal const string terrainShaderKey = "TerrainShader";

        /// <summary>
        /// The KKTerrain values a GrassColor2 keeps in its config, so switching back to the terrain shader finds them
        /// </summary>
        internal static readonly string[] terrainKeys = new string[] { "overwrite_color", "TerrainSample", "TerrainSamplePosition" };


        /// <summary>
        /// For StaticInstance.Spawn: false when the instance's config node of this grass module chose GrassColor2
        /// </summary>
        internal static bool TerrainShaderSelected(StaticInstance instance, StaticModule description)
        {
            string meshName;
            if (instance == null || !description.moduleFields.TryGetValue("GrassMeshName", out meshName))
            {
                return true;
            }
            string materialOffset;
            if (!description.moduleFields.TryGetValue("MaterialOffset", out materialOffset))
            {
                materialOffset = "0";
            }
            foreach (ConfigNode node in instance.grassColor2Configs)
            {
                if (node.GetValue("GrassMeshName") != meshName || (node.HasValue("MaterialOffset") && node.GetValue("MaterialOffset") != materialOffset))
                {
                    continue;
                }
                bool terrainShader;
                if (node.HasValue(terrainShaderKey) && bool.TryParse(node.GetValue(terrainShaderKey), out terrainShader))
                {
                    return terrainShader;
                }
                return true;
            }
            return true;
        }


        /// <summary>
        /// For KerbalKonstructs.LoadModels: makes a GrasColor or GrassColor2 module description a KKTerrain one. A GrasColor
        /// module keeps only its grass mesh name (its color and texture settings are not converted).
        /// </summary>
        internal static bool ReplaceGrassModule(StaticModule module, string modelName)
        {
            if (ReferenceEquals(module, null) || (!string.IsNullOrEmpty(module.moduleNamespace) && module.moduleNamespace != "KerbalKonstructs"))
            {
                return false;
            }
            if (module.moduleClassname == "GrassColor2")
            {
                module.moduleNamespace = "KerbalKonstructs";
                module.moduleClassname = "KKTerrain";
                return true;
            }
            if (module.moduleClassname != "GrasColor")
            {
                return false;
            }
            string meshName;
            if (!module.moduleFields.TryGetValue("GrasMeshName", out meshName) || string.IsNullOrEmpty(meshName))
            {
                Log.UserWarning("GrasColor module of " + modelName + " has no GrasMeshName, not replaced by KKTerrain");
                return false;
            }
            module.moduleNamespace = "KerbalKonstructs";
            module.moduleClassname = "KKTerrain";
            module.moduleFields = new Dictionary<string, string>
            {
                { "GrassMeshName", meshName },
                { "ConvertedFromGrasColor", "True" }
            };
            return true;
        }


        /// <summary>
        /// Scene switches activate the statics again: nothing to do unless the static was moved, turned or scaled
        /// </summary>
        public override void StaticObjectUpdate()
        {
            Vector3d position;
            if (applied && GetStaticPosition(out position) && (position - appliedPosition).magnitude < resampleDistance
                && Quaternion.Angle(PlanetRotation(), appliedRotation) < 0.01f && staticInstance.gameObject.transform.lossyScale == appliedScale)
            {
                return;
            }
            ApplySettings();
        }


        /// <summary>
        /// The rotation of the static in the planet frame
        /// </summary>
        private Quaternion PlanetRotation()
        {
            return Quaternion.Inverse(staticInstance.CelestialBody.pqsController.transform.rotation) * staticInstance.gameObject.transform.rotation;
        }


        internal override void ApplySettings()
        {
            base.ApplySettings();
            ReadInstanceConfig();
            ApplyTerrain();
        }


        internal override void OnUserColorChange()
        {
            overwrite_color = "true";
        }


        /// <summary>
        /// Switches between the terrain color and the given color. Turning the given color on starts it at the terrain
        /// color, so the editor's color picker begins with what is shown.
        /// </summary>
        internal void SetOverwriteColor(bool overwrite)
        {
            if (overwrite && !OverwriteColor)
            {
                grassColor = new Color(terrainColor.r, terrainColor.g, terrainColor.b, 1f);
            }
            overwrite_color = overwrite ? "true" : "false";
            ApplySettings();
        }


        public override ConfigNode GiveConfig()
        {
            ConfigNode node = base.GiveConfig();
            if (!string.IsNullOrEmpty(overwrite_color))
            {
                node.AddValue("overwrite_color", overwrite_color);
            }
            if (hasTerrainSample)
            {
                node.AddValue("TerrainSample", ConfigNode.WriteColor(terrainSample));
                node.AddValue("TerrainSamplePosition", WritePosition(terrainSamplePosition));
            }
            return node;
        }


        /// <summary>
        /// The instance's saved overwrite_color and terrain sample, once after ReadConfig() (the model setting stays
        /// without one)
        /// </summary>
        private void ReadInstanceConfig()
        {
            if (instanceConfigRead)
            {
                return;
            }
            instanceConfigRead = true;
            if (instanceConfig == null)
            {
                return;
            }
            bool overwrite;
            if (instanceConfig.HasValue("overwrite_color") && bool.TryParse(instanceConfig.GetValue("overwrite_color"), out overwrite))
            {
                overwrite_color = overwrite ? "true" : "false";
            }
            Vector3d position;
            if (instanceConfig.HasValue("TerrainSample") && instanceConfig.HasValue("TerrainSamplePosition")
                && TryParsePosition(instanceConfig.GetValue("TerrainSamplePosition"), out position))
            {
                terrainSample = ConfigNode.ParseColor(instanceConfig.GetValue("TerrainSample"));
                terrainSamplePosition = position;
                hasTerrainSample = true;
            }
        }


        private void ApplyTerrain()
        {
            if (staticInstance == null)
            {
                return;
            }
            if (!UsesTerrainShader)
            {
                return;
            }
            CelestialBody body = staticInstance.CelestialBody;
            Vector3d staticPosition;
            if (body == null || body.pqsController == null || !GetStaticPosition(out staticPosition))
            {
                return;
            }

            // once per instance (and again after a move): the terrain color and altitude value at the static
            Color sample = GetTerrainSample(body, staticPosition);
            terrainColor = sample;
            Color color = OverwriteColor ? grassColor : sample;

            Transform planet = body.pqsController.transform;
            List<Renderer> renderers = GrassRenderers;
            for (int i = 0; i < grassMaterials.Count && i < renderers.Count; i++)
            {
                Renderer renderer = renderers[i];
                Vector3d position;
                if (renderer == null || !KKTerrainShader.TryGetPlanetFramePosition(renderer.transform, planet, out position))
                {
                    continue;
                }
                KKTerrainShader.SetTerrainProperties(grassMaterials[i], body, position, sample, color, thirdTextureTiling, fourthTextureTiling);
            }
            applied = true;
            appliedPosition = staticPosition;
            appliedRotation = PlanetRotation();
            appliedScale = staticInstance.gameObject.transform.lossyScale;
        }


        /// <summary>
        /// The planet frame position of the static
        /// </summary>
        private bool GetStaticPosition(out Vector3d position)
        {
            position = Vector3d.zero;
            if (staticInstance == null || staticInstance.CelestialBody == null || staticInstance.CelestialBody.pqsController == null)
            {
                return false;
            }
            return KKTerrainShader.TryGetPlanetFramePosition(staticInstance.gameObject.transform, staticInstance.CelestialBody.pqsController.transform, out position);
        }


        /// <summary>
        /// The terrain color at the static: the saved sample, or a new one when there is none or the static moved
        /// </summary>
        private Color GetTerrainSample(CelestialBody body, Vector3d position)
        {
            if (hasTerrainSample && (terrainSamplePosition - position).magnitude < resampleDistance)
            {
                return terrainSample;
            }
            Color color;
            if (!KKTerrainShader.TrySampleTerrain(body, position, out color))
            {
                // not kept: tried again next time
                return new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
            }
            terrainSample = color;
            terrainSamplePosition = position;
            hasTerrainSample = true;
            UpdateInstanceConfig();
            return color;
        }


        /// <summary>
        /// Keeps the sample in the instance's GrassColor2 config node too. KK writes the instance files from GiveConfig()
        /// when it saves the instances.
        /// </summary>
        private void UpdateInstanceConfig()
        {
            if (instanceConfig == null)
            {
                return;
            }
            instanceConfig.RemoveValue("TerrainSample");
            instanceConfig.RemoveValue("TerrainSamplePosition");
            instanceConfig.AddValue("TerrainSample", ConfigNode.WriteColor(terrainSample));
            instanceConfig.AddValue("TerrainSamplePosition", WritePosition(terrainSamplePosition));
        }


        private static string WritePosition(Vector3d position)
        {
            return position.x.ToString("R", CultureInfo.InvariantCulture) + "," + position.y.ToString("R", CultureInfo.InvariantCulture) + "," +
                position.z.ToString("R", CultureInfo.InvariantCulture);
        }

        private static bool TryParsePosition(string text, out Vector3d position)
        {
            position = Vector3d.zero;
            string[] parts = text.Split(',');
            double x, y, z;
            if (parts.Length == 3 && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
            {
                position = new Vector3d(x, y, z);
                return true;
            }
            return false;
        }
    }
}
