using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// Inputs of the KK terrain shader (KK/Terrain, used by the KKTerrain module), which draws KK ground meshes like the
    /// stock PQS terrain underneath. KK/Terrain follows "Terrain/PQS/PQS Main Shader - Triplanar Zoom Rotation" (the
    /// terrain shader of the higher terrain qualities); for planets with another PQS shader the parameters below make it
    /// work like those (no zoom, no specular, brightness 1, their near tiling and texture strength).
    ///
    /// Globals of the current planet (UpdateGlobals; the rotation and sun every frame, the rest when the planet or its
    /// terrain material changes):
    ///  _KKWorldToPlanet / _KKPlanetToWorld  rotation between world space and the planet frame (the PQS transform)
    ///  _KKSunDirection   world direction from the planet to the sun
    ///  _KKZoomFactor, _KKZoomBlendWidth, _KKZoomRotation   the zoom levels (_factor, _factorBlendWidth, _factorRotation)
    ///  _KKAlbedoBrightness, _KKSpecularColor, _KKSmoothnessScale   the lighting
    ///  _KKAerialEnabled, _KKGlobalDensity, _KKFogColorRamp, _KKFogRampV   the haze. KSP itself sets
    ///                    _heightDensityAtViewer and _floatingOriginOffset.
    ///  _KKSteepEnabled, _KKSteepTex, _KKSteepBumpMap, _KKSteepPower, _KKSteepTiling, _KKSteepBumpTiling,
    ///  _KKSteepTexStart, _KKSteepTexEnd   the steep texture
    ///  _KKPlanetOrigin, _KKPlanetRadius   only for the haze of the old KK/Ground_Multi_NoUV
    ///
    /// Per renderer (SetTerrainProperties): the terrain color at the renderer (SampleTerrain, the PQS vertex color: once
    /// per renderer, not per frame) with the material's saturation, contrast and tint applied as the stock vertex shader
    /// does, two of the planet's low/mid/high layers chosen by the altitude value there, and the planet frame offsets of
    /// KK's third and fourth texture (a float position on a planet is too imprecise for texture coordinates: the offsets
    /// are computed here in double precision, reduced by offsetPeriod).
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

        /// <summary>
        /// The PQS shaders scale their world coordinates by this before the tiling
        /// </summary>
        private const double pqsCoordinateScale = 0.00001;

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
        private static readonly int zoomFactorID = Shader.PropertyToID("_KKZoomFactor");
        private static readonly int zoomBlendWidthID = Shader.PropertyToID("_KKZoomBlendWidth");
        private static readonly int zoomRotationID = Shader.PropertyToID("_KKZoomRotation");
        private static readonly int albedoBrightnessID = Shader.PropertyToID("_KKAlbedoBrightness");
        private static readonly int specularColorID = Shader.PropertyToID("_KKSpecularColor");
        private static readonly int smoothnessScaleID = Shader.PropertyToID("_KKSmoothnessScale");
        private static readonly int aerialEnabledID = Shader.PropertyToID("_KKAerialEnabled");
        private static readonly int densityID = Shader.PropertyToID("_KKGlobalDensity");
        private static readonly int rampID = Shader.PropertyToID("_KKFogColorRamp");
        private static readonly int rampVID = Shader.PropertyToID("_KKFogRampV");
        private static readonly int steepEnabledID = Shader.PropertyToID("_KKSteepEnabled");
        private static readonly int steepTexID = Shader.PropertyToID("_KKSteepTex");
        private static readonly int steepBumpID = Shader.PropertyToID("_KKSteepBumpMap");
        private static readonly int steepPowerID = Shader.PropertyToID("_KKSteepPower");
        private static readonly int steepTilingID = Shader.PropertyToID("_KKSteepTiling");
        private static readonly int steepBumpTilingID = Shader.PropertyToID("_KKSteepBumpTiling");
        private static readonly int steepStartID = Shader.PropertyToID("_KKSteepTexStart");
        private static readonly int steepEndID = Shader.PropertyToID("_KKSteepTexEnd");

        // per renderer
        private static readonly int terrainModeID = Shader.PropertyToID("_KKTerrainMode");
        private static readonly int terrainColorID = Shader.PropertyToID("_KKTerrainColor");
        private static readonly int anchorUpID = Shader.PropertyToID("_KKAnchorUp");
        private static readonly int textureStrengthID = Shader.PropertyToID("_KKTextureStrength");
        private static readonly int thirdOffsetID = Shader.PropertyToID("_KKThirdOffset");
        private static readonly int fourthOffsetID = Shader.PropertyToID("_KKFourthOffset");

        /// <summary>
        /// Shader properties of one of the two terrain layers (A, B) of KK/Terrain
        /// </summary>
        private class LayerSlot
        {
            internal readonly int tex, bump, weight, tilings;

            internal LayerSlot(string name)
            {
                string prefix = "_KKLayer" + name;
                tex = Shader.PropertyToID(prefix + "Tex");
                bump = Shader.PropertyToID(prefix + "Bump");
                weight = Shader.PropertyToID(prefix + "Weight");
                tilings = Shader.PropertyToID(prefix + "Tilings");
            }
        }
        private static readonly LayerSlot[] layerSlots = new LayerSlot[] { new LayerSlot("A"), new LayerSlot("B") };

        // the low / mid / high layer property names of the stock PQS terrain materials
        private static readonly string[] layerNames = new string[] { "low", "mid", "high" };

        // properties of the stock PQS terrain materials
        private static readonly int pqsFactorID = Shader.PropertyToID("_factor");
        private static readonly int pqsFactorBlendWidthID = Shader.PropertyToID("_factorBlendWidth");
        private static readonly int pqsFactorRotationID = Shader.PropertyToID("_factorRotation");
        private static readonly int pqsAlbedoBrightnessID = Shader.PropertyToID("_albedoBrightness");
        private static readonly int pqsSpecularColorID = Shader.PropertyToID("_specularColor");
        private static readonly int pqsSaturationID = Shader.PropertyToID("_saturation");
        private static readonly int pqsContrastID = Shader.PropertyToID("_contrast");
        private static readonly int pqsTintColorID = Shader.PropertyToID("_tintColor");
        private static readonly int pqsDensityID = Shader.PropertyToID("_globalDensity");
        private static readonly int pqsRampID = Shader.PropertyToID("_fogColorRamp");
        private static readonly int pqsSteepTexID = Shader.PropertyToID("_steepTex");
        private static readonly int pqsSteepBumpID = Shader.PropertyToID("_steepBumpMap");
        private static readonly int pqsSteepPowerID = Shader.PropertyToID("_steepPower");
        private static readonly int pqsSteepStartID = Shader.PropertyToID("_steepTexStart");
        private static readonly int pqsSteepEndID = Shader.PropertyToID("_steepTexEnd");
        private static readonly int pqsSteepNearTilingID = Shader.PropertyToID("_steepNearTiling");
        private static readonly int pqsSteepTilingID = Shader.PropertyToID("_steepTiling");
        private static readonly int pqsSteepBumpTilingID = Shader.PropertyToID("_steepBumpTiling");
        private static readonly int pqsPowerNearID = Shader.PropertyToID("_powerNear");
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


        /// <summary>
        /// True for the zoom rotation terrain shaders (the higher terrain qualities): they have the _factor zoom
        /// </summary>
        private static bool IsZoomShader(Material surfaceMaterial)
        {
            return surfaceMaterial != null && surfaceMaterial.HasProperty(pqsFactorID);
        }


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
            bool zoom = IsZoomShader(surfaceMaterial);

            // zoom levels and lighting
            Shader.SetGlobalFloat(zoomFactorID, zoom ? GetFloat(surfaceMaterial, pqsFactorID, 0f) : 0f);
            Shader.SetGlobalFloat(zoomBlendWidthID, GetFloat(surfaceMaterial, pqsFactorBlendWidthID, 0.1f));
            Shader.SetGlobalFloat(zoomRotationID, GetFloat(surfaceMaterial, pqsFactorRotationID, 0f) * Mathf.Deg2Rad);
            Shader.SetGlobalFloat(albedoBrightnessID, zoom ? GetFloat(surfaceMaterial, pqsAlbedoBrightnessID, 1f) : 1f);
            bool specular = zoom && surfaceMaterial.HasProperty(pqsSpecularColorID);
            Shader.SetGlobalColor(specularColorID, specular ? surfaceMaterial.GetColor(pqsSpecularColorID) : Color.black);
            Shader.SetGlobalFloat(smoothnessScaleID, specular ? 1f : 0f);

            // aerial perspective (the zoom rotation shaders read the fog color ramp in the row 0.5, the others in 0.75)
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
            Shader.SetGlobalFloat(rampVID, zoom ? 0.5f : 0.75f);

            // steep texture: the zoom rotation shaders map it by world position (like the layers) and don't fade it,
            // the others by the direction from the planet's center times the tiling (per meter: tiling / radius)
            bool hasSteep = false;
            if (surfaceMaterial != null && surfaceMaterial.HasProperty(pqsSteepTexID) && surfaceMaterial.GetTexture(pqsSteepTexID) != null)
            {
                Shader.SetGlobalTexture(steepTexID, surfaceMaterial.GetTexture(pqsSteepTexID));
                Shader.SetGlobalTexture(steepBumpID, GetBumpMap(surfaceMaterial, pqsSteepBumpID));
                Shader.SetGlobalFloat(steepPowerID, GetFloat(surfaceMaterial, pqsSteepPowerID, 1f));
                if (zoom)
                {
                    float steepTiling = GetFloat(surfaceMaterial, pqsSteepTilingID, 100000f);
                    Shader.SetGlobalFloat(steepTilingID, (float)(steepTiling * pqsCoordinateScale));
                    Shader.SetGlobalFloat(steepBumpTilingID, (float)(GetFloat(surfaceMaterial, pqsSteepBumpTilingID, steepTiling) * pqsCoordinateScale));
                    Shader.SetGlobalFloat(steepStartID, 1e9f);
                    Shader.SetGlobalFloat(steepEndID, 2e9f);
                }
                else
                {
                    float steepTiling = GetFloat(surfaceMaterial, pqsSteepNearTilingID, GetFloat(surfaceMaterial, pqsSteepTilingID, 1f));
                    Shader.SetGlobalFloat(steepTilingID, (float)(steepTiling / body.Radius));
                    Shader.SetGlobalFloat(steepBumpTilingID, (float)(steepTiling / body.Radius));
                    Shader.SetGlobalFloat(steepStartID, GetFloat(surfaceMaterial, pqsSteepStartID, 20000f));
                    Shader.SetGlobalFloat(steepEndID, GetFloat(surfaceMaterial, pqsSteepEndID, 30000f));
                }
                hasSteep = true;
            }
            Shader.SetGlobalFloat(steepEnabledID, hasSteep ? 1f : 0f);

            Log.Normal("KK terrain shader on " + body.name + " (" + (surfaceMaterial != null ? surfaceMaterial.shader.name : "no PQS material") +
                "): " + (zoom ? "zoom rotation" : "near tiling") + ", haze " + (hasHaze ? "on" : "off") + ", steep texture " + (hasSteep ? "on" : "off") +
                ", density material " + GetFloat(surfaceMaterial, pqsDensityID, float.NaN) + " global " + Shader.GetGlobalFloat(pqsDensityID) +
                ", heightDensityAtViewer " + Shader.GetGlobalFloat("_heightDensityAtViewer") +
                ", material properties: " + DescribeTerrainMaterial(surfaceMaterial));
        }

        #endregion


        #region per renderer

        /// <summary>
        /// The terrain color as the stock zoom rotation vertex shader adjusts the PQS vertex color:
        /// contrast * (lerp(gray, color, saturation) - tint) + tint, with tint = _tintColor.rgb * _tintColor.a.
        /// The other PQS shaders use the vertex color as it is.
        /// </summary>
        internal static Color AdjustTerrainColor(CelestialBody body, Color color)
        {
            Material surfaceMaterial = (body != null && body.pqsController != null) ? body.pqsController.surfaceMaterial : null;
            if (!IsZoomShader(surfaceMaterial))
            {
                return new Color(color.r, color.g, color.b, 1f);
            }
            float saturation = GetFloat(surfaceMaterial, pqsSaturationID, 1f);
            float contrast = GetFloat(surfaceMaterial, pqsContrastID, 1f);
            Color tintColor = surfaceMaterial.HasProperty(pqsTintColorID) ? surfaceMaterial.GetColor(pqsTintColorID) : Color.clear;
            Vector3 tint = new Vector3(tintColor.r, tintColor.g, tintColor.b) * tintColor.a;
            float gray = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
            Vector3 c = new Vector3(gray, gray, gray) + saturation * (new Vector3(color.r, color.g, color.b) - new Vector3(gray, gray, gray));
            c = contrast * (c - tint) + tint;
            return new Color(c.x, c.y, c.z, 1f);
        }


        /// <summary>
        /// Sets the KK/Terrain properties of one renderer's material.
        /// position: planet frame position of the renderer's origin (TryGetPlanetFramePosition);
        /// terrainSample: the PQS vertex color there (SampleTerrain), its alpha chooses the low/mid/high layers;
        /// color: the color the terrain formula uses (the sample, or the color set in the grass editor), before the
        /// adjustment of the zoom rotation shader (AdjustTerrainColor)
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
            material.SetColor(terrainColorID, AdjustTerrainColor(body, color));
            material.SetVector(thirdOffsetID, Offset(position, thirdTiling));
            material.SetVector(fourthOffsetID, Offset(position, fourthTiling));

            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (surfaceMaterial == null)
            {
                return;
            }
            bool zoom = IsZoomShader(surfaceMaterial);
            // the zoom rotation shaders multiply the color by the textures; the others blend by the near power
            material.SetFloat(textureStrengthID, zoom ? 1f : GetFloat(surfaceMaterial, pqsPowerNearID, 0.5f));

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
                SetLayer(material, layerSlots[slot], surfaceMaterial, layerNames[order[slot]], weights[order[slot]], zoom);
            }
        }


        private static void SetLayer(Material material, LayerSlot slot, Material surfaceMaterial, string layer, float weight, bool zoom)
        {
            Texture texture = GetTexture(surfaceMaterial, "_" + layer + "Tex");
            if (texture == null || Math.Abs(weight) < 0.0001f)
            {
                material.SetFloat(slot.weight, 0f);
                return;
            }
            // the tilings per meter of the world position: zoom rotation shaders _lowTiling / _lowBumpTiling, the others
            // their near tilings, each times the PQS coordinate scale
            double tiling = zoom ? GetFloat(surfaceMaterial, "_" + layer + "Tiling", 100000f)
                : GetFloat(surfaceMaterial, "_" + layer + "NearTiling", 1000f);
            double bumpTiling = zoom ? GetFloat(surfaceMaterial, "_" + layer + "BumpTiling", (float)tiling)
                : GetFloat(surfaceMaterial, "_" + layer + "BumpNearTiling", (float)tiling);

            material.SetTexture(slot.tex, texture);
            material.SetTexture(slot.bump, GetBumpMap(surfaceMaterial, Shader.PropertyToID("_" + layer + "BumpMap")));
            material.SetFloat(slot.weight, weight);
            material.SetVector(slot.tilings, new Vector4((float)(tiling * pqsCoordinateScale), (float)(bumpTiling * pqsCoordinateScale), 0f, 0f));
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


        #region diagnostics

        private static readonly string[] terrainProperties = new string[] {
            "_lowTex", "_midTex", "_highTex", "_steepTex",
            "_lowNearTiling", "_lowMultiFactor", "_lowTiling", "_lowBumpNearTiling", "_lowBumpFarTiling", "_lowBumpTiling",
            "_midTiling", "_highTiling", "_steepNearTiling", "_steepTiling",
            "_lowStart", "_lowEnd", "_highStart", "_highEnd", "_powerNear", "_powerFar", "_groundTexStart", "_groundTexEnd",
            "_steepTexStart", "_steepTexEnd", "_factor", "_factorBlendWidth", "_factorRotation",
            "_albedoBrightness", "_saturation", "_contrast", "_tintColor", "_specularColor", "_globalDensity", "_PlanetOpacity" };

        /// <summary>
        /// The values of the planet's terrain material that KK/Terrain could use (for the log)
        /// </summary>
        internal static string DescribeTerrainMaterial(Material material)
        {
            if (material == null)
            {
                return "none";
            }
            List<string> values = new List<string>();
            foreach (string name in terrainProperties)
            {
                if (!material.HasProperty(name))
                {
                    continue;
                }
                if (name.EndsWith("Tex"))
                {
                    Texture t = material.GetTexture(name);
                    values.Add(name + "=" + (t != null ? t.name : "null"));
                }
                else if (name.EndsWith("Color"))
                {
                    values.Add(name + "=" + material.GetColor(name));
                }
                else
                {
                    values.Add(name + "=" + material.GetFloat(name));
                }
            }
            return string.Join(" ", values.ToArray());
        }

        /// <summary>
        /// The KK/Terrain values of one renderer's material (for the log)
        /// </summary>
        internal static string DescribeRendererMaterial(Material material)
        {
            if (material == null || !material.HasProperty(terrainModeID))
            {
                return "not KK/Terrain";
            }
            string result = "color " + material.GetColor(terrainColorID) + " up " + V(material.GetVector(anchorUpID)) +
                " texture strength " + material.GetFloat(textureStrengthID) +
                " | globals zoom " + Shader.GetGlobalFloat(zoomFactorID) + " brightness " + Shader.GetGlobalFloat(albedoBrightnessID) +
                " specular " + Shader.GetGlobalColor(specularColorID) + " steep tiling " + Shader.GetGlobalFloat(steepTilingID);
            foreach (LayerSlot slot in layerSlots)
            {
                Texture t = material.GetTexture(slot.tex);
                result += " | layer " + (t != null ? t.name : "null") + " weight " + material.GetFloat(slot.weight) + " tilings " + V(material.GetVector(slot.tilings));
            }
            result += " | mask " + (material.GetTexture("_BlendMaskTexture") != null ? material.GetTexture("_BlendMaskTexture").name : "null") +
                " tarmac " + material.GetColor("_TarmacColor") + " third " + material.GetColor("_ThirdTextureColor") + " fourth " + material.GetColor("_FourthTextureColor");
            return result;
        }

        private static string V(Vector4 v)
        {
            return "(" + v.x.ToString("G4") + "," + v.y.ToString("G4") + "," + v.z.ToString("G4") + "," + v.w.ToString("G4") + ")";
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
