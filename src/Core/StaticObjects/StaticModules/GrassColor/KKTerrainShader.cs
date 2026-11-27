using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// Inputs of the KK terrain shader (KK/Terrain, used by the KKTerrain module), which draws KK ground meshes with the
    /// stock PQS terrain formula so they look like the terrain underneath.
    ///
    /// Globals of the current planet, set every frame (UpdateGlobals):
    ///  _KKWorldToPlanet / _KKPlanetToWorld  rotation between world space and the planet frame (the PQS transform)
    ///  _KKSunDirection   world direction from the planet to the sun
    ///  _KKAerialEnabled, _KKGlobalDensity, _KKFogColorRamp   the aerial perspective (haze) of the planet's PQS material.
    ///                    KSP itself sets _heightDensityAtViewer (PQSMod_AerialPerspectiveMaterial).
    ///  _KKSteepEnabled, _KKSteepTex, _KKSteepBumpMap, _KKSteepPower, _KKSteepTexStart, _KKSteepTexEnd,
    ///  _KKGroundTexStart, _KKGroundTexEnd   the steep texture and the near/far distances of the planet's PQS material
    ///  _KKPlanetOrigin, _KKPlanetRadius   only for the haze of the old KK/Ground_Multi_NoUV
    ///
    /// Per renderer (SetTerrainProperties): the terrain color at the renderer (SampleTerrain, the PQS vertex color: once
    /// per renderer, not per frame), two of the planet's low/mid/high layers chosen by the altitude there, and the texture
    /// coordinates. Those are mapped in the planet frame: a float position on a planet is too imprecise for texture
    /// coordinates, so the shader adds the small distance of the pixel from the renderer's origin to an offset that is
    /// computed here in double precision (planet frame position * tiling, reduced by offsetPeriod).
    ///
    /// With Scatterer installed the haze stays off: Scatterer replaces the stock terrain haze with its own atmosphere,
    /// which it also applies to the statics, so a stock-like haze on top of it would be doubled.
    /// </summary>
    internal static class KKTerrainShader
    {
        /// <summary>
        /// The texture coordinate offsets are reduced modulo this. It is an integer (the textures repeat every 1) and
        /// nearly a multiple of the row spacing (sqrt 3) of the hex grid of the stochastic sampling: 1351 = 780 * 1.7320508
        /// - 0.0004.
        /// </summary>
        internal const double offsetPeriod = 1351d;

        internal const string terrainShaderName = "KK/Terrain";
        internal const string oldGroundShaderName = "KK/Ground_Multi_NoUV";

        private static bool? hasTerrainShader = null;

        /// <summary>
        /// KK/Terrain, or null with an older shader bundle that doesn't have it
        /// </summary>
        internal static Shader TerrainShader
        {
            get
            {
                if (!hasTerrainShader.HasValue)
                {
                    hasTerrainShader = KKGraphics.HasShader(terrainShaderName);
                    if (!hasTerrainShader.Value)
                    {
                        Log.UserWarning(terrainShaderName + " is not in the shader bundle (an older bundle?): KKTerrain uses " + oldGroundShaderName);
                    }
                }
                return hasTerrainShader.Value ? KKGraphics.GetShader(terrainShaderName) : null;
            }
        }

        /// <summary>
        /// The old ground shader of GrassColor2
        /// </summary>
        internal static Shader OldGroundShader => KKGraphics.GetShader(oldGroundShaderName);

        // globals
        private static readonly int worldToPlanetID = Shader.PropertyToID("_KKWorldToPlanet");
        private static readonly int planetToWorldID = Shader.PropertyToID("_KKPlanetToWorld");
        private static readonly int sunID = Shader.PropertyToID("_KKSunDirection");
        private static readonly int originID = Shader.PropertyToID("_KKPlanetOrigin");
        private static readonly int radiusID = Shader.PropertyToID("_KKPlanetRadius");
        private static readonly int aerialEnabledID = Shader.PropertyToID("_KKAerialEnabled");
        private static readonly int densityID = Shader.PropertyToID("_KKGlobalDensity");
        private static readonly int rampID = Shader.PropertyToID("_KKFogColorRamp");
        private static readonly int steepEnabledID = Shader.PropertyToID("_KKSteepEnabled");
        private static readonly int steepTexID = Shader.PropertyToID("_KKSteepTex");
        private static readonly int steepBumpID = Shader.PropertyToID("_KKSteepBumpMap");
        private static readonly int steepPowerID = Shader.PropertyToID("_KKSteepPower");
        private static readonly int steepStartID = Shader.PropertyToID("_KKSteepTexStart");
        private static readonly int steepEndID = Shader.PropertyToID("_KKSteepTexEnd");
        private static readonly int groundStartID = Shader.PropertyToID("_KKGroundTexStart");
        private static readonly int groundEndID = Shader.PropertyToID("_KKGroundTexEnd");

        // per renderer
        private static readonly int terrainModeID = Shader.PropertyToID("_KKTerrainMode");
        private static readonly int terrainColorID = Shader.PropertyToID("_KKTerrainColor");
        private static readonly int anchorUpID = Shader.PropertyToID("_KKAnchorUp");
        private static readonly int triWeightsID = Shader.PropertyToID("_KKTriWeights");
        private static readonly int powerNearID = Shader.PropertyToID("_KKPowerNear");
        private static readonly int powerFarID = Shader.PropertyToID("_KKPowerFar");
        private static readonly int steepNearOffsetID = Shader.PropertyToID("_KKSteepNearOffset");
        private static readonly int steepFarOffsetID = Shader.PropertyToID("_KKSteepFarOffset");
        private static readonly int steepNearTilingID = Shader.PropertyToID("_KKSteepNearTiling");
        private static readonly int steepFarTilingID = Shader.PropertyToID("_KKSteepFarTiling");
        private static readonly int thirdOffsetID = Shader.PropertyToID("_KKThirdOffset");
        private static readonly int fourthOffsetID = Shader.PropertyToID("_KKFourthOffset");

        /// <summary>
        /// Shader properties of one of the two terrain layers (A, B) of KK/Terrain
        /// </summary>
        private class LayerSlot
        {
            internal readonly int tex, bump, weight, tilings, nearOffset, farOffset, bumpNearOffset, bumpFarOffset;

            internal LayerSlot(string name)
            {
                string prefix = "_KKLayer" + name;
                tex = Shader.PropertyToID(prefix + "Tex");
                bump = Shader.PropertyToID(prefix + "Bump");
                weight = Shader.PropertyToID(prefix + "Weight");
                tilings = Shader.PropertyToID(prefix + "Tilings");
                nearOffset = Shader.PropertyToID(prefix + "NearOffset");
                farOffset = Shader.PropertyToID(prefix + "FarOffset");
                bumpNearOffset = Shader.PropertyToID(prefix + "BumpNearOffset");
                bumpFarOffset = Shader.PropertyToID(prefix + "BumpFarOffset");
            }
        }
        private static readonly LayerSlot[] layerSlots = new LayerSlot[] { new LayerSlot("A"), new LayerSlot("B") };

        // the low / mid / high layer property names of the stock PQS terrain materials
        private static readonly string[] layerNames = new string[] { "low", "mid", "high" };

        // properties of the stock PQS terrain materials
        private static readonly int pqsDensityID = Shader.PropertyToID("_globalDensity");
        private static readonly int pqsRampID = Shader.PropertyToID("_fogColorRamp");
        private static readonly int pqsSteepTexID = Shader.PropertyToID("_steepTex");
        private static readonly int pqsSteepBumpID = Shader.PropertyToID("_steepBumpMap");
        private static readonly int pqsSteepPowerID = Shader.PropertyToID("_steepPower");
        private static readonly int pqsSteepStartID = Shader.PropertyToID("_steepTexStart");
        private static readonly int pqsSteepEndID = Shader.PropertyToID("_steepTexEnd");
        private static readonly int pqsSteepNearTilingID = Shader.PropertyToID("_steepNearTiling");
        private static readonly int pqsSteepTilingID = Shader.PropertyToID("_steepTiling");
        private static readonly int pqsGroundStartID = Shader.PropertyToID("_groundTexStart");
        private static readonly int pqsGroundEndID = Shader.PropertyToID("_groundTexEnd");
        private static readonly int pqsPowerNearID = Shader.PropertyToID("_powerNear");
        private static readonly int pqsPowerFarID = Shader.PropertyToID("_powerFar");
        private static readonly int pqsLowStartID = Shader.PropertyToID("_lowStart");
        private static readonly int pqsLowEndID = Shader.PropertyToID("_lowEnd");
        private static readonly int pqsHighStartID = Shader.PropertyToID("_highStart");
        private static readonly int pqsHighEndID = Shader.PropertyToID("_highEnd");

        private static bool? scattererLoaded = null;

        /// <summary>
        /// True when Scatterer is installed (its assembly is named "scatterer")
        /// </summary>
        internal static bool ScattererLoaded
        {
            get
            {
                if (!scattererLoaded.HasValue)
                {
                    scattererLoaded = AssemblyLoader.loadedAssemblies.Any(a => string.Equals(a.name, "scatterer", StringComparison.OrdinalIgnoreCase));
                    if (scattererLoaded.Value)
                    {
                        Log.Normal("Scatterer found: no stock-like haze on the KK ground (Scatterer applies its own)");
                    }
                }
                return scattererLoaded.Value;
            }
        }

        private static CelestialBody lastBody = null;
        private static Material lastMaterial = null;


        #region globals

        /// <summary>
        /// Called every frame (KerbalKonstructs.LateUpdate) in the scenes that show statics
        /// </summary>
        internal static void UpdateGlobals(CelestialBody body)
        {
            if (body == null || body.pqsController == null)
            {
                Shader.SetGlobalFloat(aerialEnabledID, 0f);
                Shader.SetGlobalFloat(steepEnabledID, 0f);
                lastBody = null;
                lastMaterial = null;
                return;
            }

            // the planet rotates in world space above the inverse rotation altitude: every frame
            Quaternion planetRotation = body.pqsController.transform.rotation;
            Shader.SetGlobalMatrix(worldToPlanetID, Matrix4x4.Rotate(Quaternion.Inverse(planetRotation)));
            Shader.SetGlobalMatrix(planetToWorldID, Matrix4x4.Rotate(planetRotation));
            Shader.SetGlobalVector(originID, (Vector3)body.position);
            Shader.SetGlobalFloat(radiusID, (float)body.Radius);

            CelestialBody sun = Planetarium.fetch != null ? Planetarium.fetch.Sun : null;
            if (sun != null)
            {
                Shader.SetGlobalVector(sunID, ((Vector3)(sun.position - body.position)).normalized);
            }

            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (body == lastBody && surfaceMaterial == lastMaterial)
            {
                return;
            }
            // a new body, or the terrain shader quality was changed
            lastBody = body;
            lastMaterial = surfaceMaterial;

            // aerial perspective
            bool hasHaze = false;
            if (!ScattererLoaded && body.atmosphere && surfaceMaterial != null && surfaceMaterial.HasProperty(pqsRampID)
                && AerialKeywordOn(surfaceMaterial))
            {
                Texture ramp = surfaceMaterial.GetTexture(pqsRampID);
                float density = surfaceMaterial.HasProperty(pqsDensityID) ? surfaceMaterial.GetFloat(pqsDensityID) : Shader.GetGlobalFloat(pqsDensityID);
                if (ramp != null && density != 0f)
                {
                    Shader.SetGlobalTexture(rampID, ramp);
                    // as the PQS shaders use it: fog = 1 - exp(depth * density * heightDensityAtViewer)
                    Shader.SetGlobalFloat(densityID, density);
                    hasHaze = true;
                }
            }
            Shader.SetGlobalFloat(aerialEnabledID, hasHaze ? 1f : 0f);

            // near / far distances of the terrain textures
            Shader.SetGlobalFloat(groundStartID, GetFloat(surfaceMaterial, pqsGroundStartID, 2000f));
            Shader.SetGlobalFloat(groundEndID, GetFloat(surfaceMaterial, pqsGroundEndID, 10000f));

            // steep texture
            bool hasSteep = false;
            if (surfaceMaterial != null && surfaceMaterial.HasProperty(pqsSteepTexID) && surfaceMaterial.GetTexture(pqsSteepTexID) != null)
            {
                Shader.SetGlobalTexture(steepTexID, surfaceMaterial.GetTexture(pqsSteepTexID));
                Shader.SetGlobalTexture(steepBumpID, GetBumpMap(surfaceMaterial, pqsSteepBumpID));
                Shader.SetGlobalFloat(steepPowerID, GetFloat(surfaceMaterial, pqsSteepPowerID, 1f));
                Shader.SetGlobalFloat(steepStartID, GetFloat(surfaceMaterial, pqsSteepStartID, 20000f));
                Shader.SetGlobalFloat(steepEndID, GetFloat(surfaceMaterial, pqsSteepEndID, 30000f));
                hasSteep = true;
            }
            Shader.SetGlobalFloat(steepEnabledID, hasSteep ? 1f : 0f);

            Log.Normal("KK terrain shader on " + body.name + " (" + (surfaceMaterial != null ? surfaceMaterial.shader.name : "no PQS material") +
                "): haze " + (hasHaze ? "on" : "off") + ", steep texture " + (hasSteep ? "on" : "off"));
        }

        #endregion


        #region per renderer

        /// <summary>
        /// Sets the KK/Terrain properties of one renderer's material.
        /// position: planet frame position of the renderer's origin (TryGetPlanetFramePosition);
        /// terrainSample: the PQS vertex color there (SampleTerrain), its alpha chooses the low/mid/high layers;
        /// color: the color the terrain formula uses (the sample, or the color set in the grass editor)
        /// </summary>
        internal static void SetTerrainProperties(Material material, CelestialBody body, Vector3d position, Color terrainSample,
            Color color, float thirdTiling, float fourthTiling)
        {
            if (material == null || !material.HasProperty(terrainModeID) || body == null || body.pqsController == null)
            {
                return;
            }
            double distance = position.magnitude;
            if (distance < 1d)
            {
                return;
            }
            Vector3d up = position / distance;
            material.SetVector(anchorUpID, new Vector4((float)up.x, (float)up.y, (float)up.z, 0f));
            // the stock triplanar weights: abs(direction from the planet's center), not normalized
            material.SetVector(triWeightsID, new Vector4((float)Math.Abs(up.x), (float)Math.Abs(up.y), (float)Math.Abs(up.z), 0f));
            material.SetColor(terrainColorID, new Color(color.r, color.g, color.b, 1f));
            material.SetVector(thirdOffsetID, Offset(position, thirdTiling));
            material.SetVector(fourthOffsetID, Offset(position, fourthTiling));

            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (surfaceMaterial == null)
            {
                return;
            }
            material.SetFloat(powerNearID, GetFloat(surfaceMaterial, pqsPowerNearID, 0.5f));
            material.SetFloat(powerFarID, GetFloat(surfaceMaterial, pqsPowerFarID, 0.5f));

            // the PQS shaders map the steep texture by the direction from the planet's center times the tiling:
            // per meter at the renderer that is tiling / distance from the center
            double steepFarTiling = GetFloat(surfaceMaterial, pqsSteepTilingID, 1f);
            double steepNearTiling = GetFloat(surfaceMaterial, pqsSteepNearTilingID, (float)steepFarTiling);
            material.SetVector(steepNearOffsetID, Offset(up, steepNearTiling));
            material.SetVector(steepFarOffsetID, Offset(up, steepFarTiling));
            material.SetFloat(steepNearTilingID, (float)(steepNearTiling / distance));
            material.SetFloat(steepFarTilingID, (float)(steepFarTiling / distance));

            // low / mid / high weights by the altitude value of the vertex color, as in the PQS shaders
            float height = terrainSample.a;
            float lowBlend = SmoothStep(GetFloat(surfaceMaterial, pqsLowStartID, 0f), GetFloat(surfaceMaterial, pqsLowEndID, 0.3f), height);
            float highBlend = SmoothStep(GetFloat(surfaceMaterial, pqsHighStartID, 0.8f), GetFloat(surfaceMaterial, pqsHighEndID, 1f), height);
            float[] weights = new float[] { 1f - lowBlend, lowBlend - highBlend, highBlend };

            // the two strongest layers (a third one only has weight where the low and high transitions overlap)
            int[] order = new int[] { 0, 1, 2 };
            Array.Sort(order, (a, b) => Math.Abs(weights[b]).CompareTo(Math.Abs(weights[a])));
            for (int slot = 0; slot < 2; slot++)
            {
                SetLayer(material, layerSlots[slot], surfaceMaterial, layerNames[order[slot]], weights[order[slot]], position, up, distance);
            }
        }


        private static void SetLayer(Material material, LayerSlot slot, Material surfaceMaterial, string layer, float weight,
            Vector3d position, Vector3d up, double distance)
        {
            Texture texture = GetTexture(surfaceMaterial, "_" + layer + "Tex");
            if (texture == null || Math.Abs(weight) < 0.0001f)
            {
                material.SetFloat(slot.weight, 0f);
                return;
            }
            // near: the PQS shaders use (position * 0.00001 * tiling), far: (direction from the center * tiling)
            double nearTiling = GetFloat(surfaceMaterial, "_" + layer + "NearTiling", GetFloat(surfaceMaterial, "_" + layer + "Tiling", 1000f)) * 0.00001;
            double farTiling = GetFloat(surfaceMaterial, "_" + layer + "MultiFactor", 10f) / distance;
            double bumpNearTiling = GetFloat(surfaceMaterial, "_" + layer + "BumpNearTiling", GetFloat(surfaceMaterial, "_" + layer + "BumpTiling", 1f)) * 0.00001;
            double bumpFarTiling = GetFloat(surfaceMaterial, "_" + layer + "BumpFarTiling", 1f) / distance;

            material.SetTexture(slot.tex, texture);
            material.SetTexture(slot.bump, GetBumpMap(surfaceMaterial, Shader.PropertyToID("_" + layer + "BumpMap")));
            material.SetFloat(slot.weight, weight);
            material.SetVector(slot.tilings, new Vector4((float)nearTiling, (float)farTiling, (float)bumpNearTiling, (float)bumpFarTiling));
            material.SetVector(slot.nearOffset, Offset(position, nearTiling));
            material.SetVector(slot.bumpNearOffset, Offset(position, bumpNearTiling));
            // the far coordinates continue the terrain's own
            material.SetVector(slot.farOffset, Offset(up, farTiling * distance));
            material.SetVector(slot.bumpFarOffset, Offset(up, bumpFarTiling * distance));
        }

        #endregion


        #region terrain color

        private delegate void VertexBuildHeight(PQS.VertexBuildData data, bool overrideQuadBuildCheck);
        private delegate void VertexBuild(PQS.VertexBuildData data);

        private static readonly Dictionary<PQS, VertexBuildHeight> buildHeight = new Dictionary<PQS, VertexBuildHeight>();
        private static readonly Dictionary<PQS, VertexBuild> build = new Dictionary<PQS, VertexBuild>();

        /// <summary>
        /// The PQS vertex color at a planet frame position, like the PQS builds it for the terrain mesh (the stock terrain
        /// shaders use it as the terrain color, its alpha as altitude value for the low/mid/high textures). Runs the PQS mods
        /// once (PQS.Mod_OnVertexBuildHeight / Mod_OnVertexBuild, as Kopernicus' texture exporter does), without changing
        /// the terrain.
        /// </summary>
        internal static bool TrySampleTerrain(CelestialBody body, Vector3d position, out Color color)
        {
            color = Color.clear;
            PQS pqs = body != null ? body.pqsController : null;
            if (pqs == null || position.magnitude < 1d)
            {
                return false;
            }
            VertexBuildHeight onBuildHeight;
            VertexBuild onBuild;
            if (!GetBuildMethods(pqs, out onBuildHeight, out onBuild))
            {
                return false;
            }

            bool wasBuildingMaps = pqs.isBuildingMaps;
            bool wasFakeBuild = pqs.isFakeBuild;
            try
            {
                // our vertex must not end up in the terrain
                pqs.isBuildingMaps = true;
                pqs.isFakeBuild = true;

                PQS.VertexBuildData data = new PQS.VertexBuildData();
                data.directionFromCenter = position.normalized;
                data.vertHeight = pqs.radius;
                onBuildHeight(data, true);
                onBuild(data);
                color = data.vertColor;
                return true;
            }
            catch (Exception e)
            {
                Log.UserWarning("KK terrain: could not sample the terrain of " + body.name + ": " + e.Message);
                return false;
            }
            finally
            {
                pqs.isBuildingMaps = wasBuildingMaps;
                pqs.isFakeBuild = wasFakeBuild;
            }
        }


        private static bool GetBuildMethods(PQS pqs, out VertexBuildHeight onBuildHeight, out VertexBuild onBuild)
        {
            if (!buildHeight.TryGetValue(pqs, out onBuildHeight) || !build.TryGetValue(pqs, out onBuild))
            {
                onBuildHeight = null;
                onBuild = null;
                try
                {
                    MethodInfo heightMethod = typeof(PQS).GetMethod("Mod_OnVertexBuildHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo buildMethod = typeof(PQS).GetMethod("Mod_OnVertexBuild", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (heightMethod != null && buildMethod != null)
                    {
                        onBuildHeight = (VertexBuildHeight)Delegate.CreateDelegate(typeof(VertexBuildHeight), pqs, heightMethod);
                        onBuild = (VertexBuild)Delegate.CreateDelegate(typeof(VertexBuild), pqs, buildMethod);
                    }
                }
                catch (Exception e)
                {
                    Log.UserWarning("KK terrain: PQS vertex build methods not found: " + e.Message);
                }
                buildHeight[pqs] = onBuildHeight;
                build[pqs] = onBuild;
            }
            return onBuildHeight != null && onBuild != null;
        }

        #endregion


        #region helpers

        /// <summary>
        /// The position of the transform's origin in the frame of the planet's PQS transform, in double precision.
        /// Walks up the parents: the rotations and scales only touch the small offsets below the top, the large position
        /// of the group center on the planet is added last.
        /// </summary>
        internal static bool TryGetPlanetFramePosition(Transform transform, Transform planet, out Vector3d position)
        {
            Vector3d result = Vector3d.zero;
            Transform current = transform;
            while (current != null && current != planet)
            {
                Vector3 scaled = Vector3.Scale(current.localScale, (Vector3)result);
                result = (Vector3d)(current.localRotation * scaled) + (Vector3d)current.localPosition;
                current = current.parent;
            }
            position = result;
            return current == planet;
        }


        private static Vector4 Offset(Vector3d position, double tiling)
        {
            return new Vector4(Reduce(position.x * tiling), Reduce(position.y * tiling), Reduce(position.z * tiling), 0f);
        }

        private static float Reduce(double value)
        {
            return (float)(value - Math.Floor(value / offsetPeriod) * offsetPeriod);
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = (edge1 == edge0) ? (x >= edge1 ? 1f : 0f) : Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static float GetFloat(Material material, int id, float fallback)
        {
            return (material != null && material.HasProperty(id)) ? material.GetFloat(id) : fallback;
        }

        private static float GetFloat(Material material, string name, float fallback)
        {
            return (material != null && material.HasProperty(name)) ? material.GetFloat(name) : fallback;
        }

        private static Texture GetTexture(Material material, string name)
        {
            return (material != null && material.HasProperty(name)) ? material.GetTexture(name) : null;
        }

        private static Texture GetBumpMap(Material material, int id)
        {
            Texture bump = (material != null && material.HasProperty(id)) ? material.GetTexture(id) : null;
            return bump != null ? bump : FlatNormalMap;
        }

        /// <summary>
        /// The PQS shaders have AERIAL_ON / AERIAL_OFF variants, chosen by the material's keywords
        /// </summary>
        private static bool AerialKeywordOn(Material material)
        {
            return material.IsKeywordEnabled("AERIAL_ON") || !material.IsKeywordEnabled("AERIAL_OFF");
        }

        private static Texture2D flatNormalMap = null;

        /// <summary>
        /// A flat normal map, for planets without a bump map
        /// </summary>
        private static Texture2D FlatNormalMap
        {
            get
            {
                if (flatNormalMap == null)
                {
                    flatNormalMap = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                    flatNormalMap.name = "KKFlatNormalMap";
                    // UnpackNormal reads x as r * a, y as g: (0.5, 0.5) is flat
                    flatNormalMap.SetPixels32(new Color32[] { new Color32(128, 128, 255, 255) });
                    flatNormalMap.Apply(false, true);
                }
                return flatNormalMap;
            }
        }

        #endregion
    }
}
