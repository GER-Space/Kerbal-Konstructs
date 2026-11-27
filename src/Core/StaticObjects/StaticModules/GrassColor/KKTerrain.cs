using KerbalKonstructs.Core;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs
{
    /// <summary>
    /// Ground meshes drawn like the terrain underneath (shader KK/Terrain). It replaces the GrasColor and GrassColor2
    /// modules (KerbalKonstructs.LoadModels, SquadStatics) and keeps GrassColor2's settings and config: tarmac, blend mask,
    /// third and fourth texture.
    /// The grass is the planet's own low/mid/high, steep and bump textures, with the terrain color at the renderer's
    /// position (the PQS vertex color, sampled once per renderer and again only when it moved). With overwrite_color the
    /// color set in the grass editor (GrassColor of the instance config) is used instead of the terrain color.
    /// The grass texture settings of GrassColor2 are not used by KK/Terrain. With an older shader bundle without
    /// KK/Terrain, it works like GrassColor2.
    /// </summary>
    public class KKTerrain : GrassColor2
    {
        /// <summary>
        /// Model setting (and saved per instance): true uses the given grass color instead of the terrain color
        /// </summary>
        public bool overwrite_color = false;

        /// <summary>
        /// The terrain color at the first grass mesh (shown in the grass editor while overwrite_color is off)
        /// </summary>
        internal Color terrainColor = defaultColor;

        private bool overwriteRead = false;

        private class TerrainSample
        {
            internal Vector3d position;
            internal Color color;
        }
        private readonly Dictionary<Renderer, TerrainSample> samples = new Dictionary<Renderer, TerrainSample>();

        protected override Shader GroundShader
        {
            get
            {
                Shader terrainShader = KKTerrainShader.TerrainShader;
                return terrainShader != null ? terrainShader : base.GroundShader;
            }
        }

        // the color is the terrain's, unless the instance overwrites it
        protected override bool ImportsGroundColor => false;

        /// <summary>
        /// True when the grass meshes use KK/Terrain (not the old shader of an older bundle)
        /// </summary>
        internal bool UsesTerrainShader => grassMaterials.Count > 0 && grassMaterials[0] != null && grassMaterials[0].HasProperty("_KKTerrainMode");


        internal override void ApplySettings()
        {
            base.ApplySettings();
            ReadOverwrite();
            ApplyTerrain();
        }


        internal override void OnUserColorChange()
        {
            overwrite_color = true;
        }


        /// <summary>
        /// Switches between the terrain color and the given color. Turning the given color on starts it at the terrain
        /// color, so the editor's color picker begins with what is shown.
        /// </summary>
        internal void SetOverwriteColor(bool overwrite)
        {
            if (overwrite && !overwrite_color)
            {
                grassColor = new Color(terrainColor.r, terrainColor.g, terrainColor.b, 1f);
            }
            overwrite_color = overwrite;
            ApplySettings();
        }


        public override ConfigNode GiveConfig()
        {
            ConfigNode node = base.GiveConfig();
            node.AddValue("overwrite_color", overwrite_color);
            return node;
        }


        /// <summary>
        /// The instance's saved overwrite_color, once after ReadConfig() (the model setting stays without one)
        /// </summary>
        private void ReadOverwrite()
        {
            if (overwriteRead)
            {
                return;
            }
            overwriteRead = true;
            bool overwrite;
            if (instanceConfig != null && instanceConfig.HasValue("overwrite_color") && bool.TryParse(instanceConfig.GetValue("overwrite_color"), out overwrite))
            {
                overwrite_color = overwrite;
            }
        }


        // diagnostics: the first renderers' values go to the log
        private static int loggedRenderers = 0;
        private const int maxLoggedRenderers = 12;
        private bool loggedNoPosition = false;

        private void ApplyTerrain()
        {
            if (staticInstance == null)
            {
                return;
            }
            if (!UsesTerrainShader)
            {
                if (loggedRenderers < maxLoggedRenderers)
                {
                    loggedRenderers++;
                    Log.Normal("KKTerrain " + staticInstance.UUID + ": not on KK/Terrain (" + (grassMaterials.Count > 0 && grassMaterials[0] != null ? grassMaterials[0].shader.name : "no grass material") + ")");
                }
                return;
            }
            CelestialBody body = staticInstance.CelestialBody;
            if (body == null || body.pqsController == null)
            {
                return;
            }
            Transform planet = body.pqsController.transform;
            List<Renderer> renderers = GrassRenderers;
            for (int i = 0; i < grassMaterials.Count && i < renderers.Count; i++)
            {
                Renderer renderer = renderers[i];
                Vector3d position;
                if (renderer == null || !KKTerrainShader.TryGetPlanetFramePosition(renderer.transform, planet, out position))
                {
                    if (!loggedNoPosition)
                    {
                        loggedNoPosition = true;
                        Log.Normal("KKTerrain " + staticInstance.model.name + " " + staticInstance.UUID + ": grass mesh not under the planet's PQS transform (yet): " +
                            (renderer != null ? Path(renderer.transform) : "no renderer"));
                    }
                    continue;
                }
                Color sample = GetTerrainSample(body, renderer, position);
                if (i == 0)
                {
                    terrainColor = sample;
                }
                Color color = overwrite_color ? grassColor : sample;
                KKTerrainShader.SetTerrainProperties(grassMaterials[i], body, position, sample, color, thirdTextureTiling, fourthTextureTiling);
                if (loggedRenderers < maxLoggedRenderers)
                {
                    loggedRenderers++;
                    Log.Normal("KKTerrain " + staticInstance.model.name + " " + staticInstance.UUID + " mesh " + renderer.name + ": distance " + position.magnitude.ToString("F1") +
                        " sample " + sample + (overwrite_color ? " overwritten by " + grassColor : "") + " -> " + KKTerrainShader.DescribeRendererMaterial(grassMaterials[i]));
                }
            }
        }


        private static string Path(Transform transform)
        {
            string path = transform.name;
            for (Transform t = transform.parent; t != null; t = t.parent)
            {
                path = t.name + "/" + path;
            }
            return path;
        }


        /// <summary>
        /// The terrain color at the renderer: sampled once, and again when the renderer moved (in the editor)
        /// </summary>
        private Color GetTerrainSample(CelestialBody body, Renderer renderer, Vector3d position)
        {
            TerrainSample sample;
            if (samples.TryGetValue(renderer, out sample) && (sample.position - position).magnitude < 1d)
            {
                return sample.color;
            }
            Color color;
            if (!KKTerrainShader.TrySampleTerrain(body, position, out color))
            {
                color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
            }
            samples[renderer] = new TerrainSample { position = position, color = color };
            return color;
        }
    }
}
