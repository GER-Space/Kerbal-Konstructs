using System;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// Inputs of the KK terrain shader (KK/Terrain, GrassColor2), which draws the KK ground like the stock PQS terrain it
    /// continues. The old ground shader (KK/Ground_Multi_NoUV) stays in the bundle; GrassColor2 uses it only with an older
    /// bundle that has no KK/Terrain.
    ///
    /// Globals of the current planet, set every frame (UpdateGlobals):
    ///  _KKWorldToPlanet / _KKPlanetToWorld  rotation between world space and the planet frame (the PQS transform)
    ///  _KKSunDirection   world direction from the planet to the sun
    ///  _KKAerialEnabled, _KKGlobalDensity, _KKFogColorRamp   the aerial perspective (haze) of the planet's PQS material.
    ///                    KSP itself sets _heightDensityAtViewer (PQSMod_AerialPerspectiveMaterial).
    ///  _KKSteepEnabled, _KKSteepTex, _KKSteepBumpMap, _KKSteepPower, _KKSteepTexStart, _KKSteepTexEnd,
    ///  _KKGroundTexStart, _KKGroundTexEnd   the steep texture of the planet's PQS material and its blend values
    ///
    /// Per renderer (SetRendererProperties): the textures are mapped in the planet frame. A float position on a planet
    /// is too imprecise for texture coordinates, so the shader adds the small distance of the pixel from the renderer's
    /// origin to an offset that is computed here in double precision: (planet frame position of the origin * tiling),
    /// reduced by offsetPeriod. Neighboring statics then continue each other's textures.
    ///
    /// With Scatterer installed the haze stays off: Scatterer replaces the stock terrain haze with its own atmosphere,
    /// which it also applies to the statics, so a stock-like haze on top of it would be doubled.
    /// </summary>
    internal static class KKTerrainShader
    {
        /// <summary>
        /// The texture coordinate offsets are reduced modulo this. It is an integer (the texture repeats every 1) and
        /// nearly a multiple of the row spacing (sqrt 3) of the hex grid of the stochastic sampling: 1351 = 780 * 1.7320508
        /// - 0.0004. So the random pattern of the stochastic tiling also continues between statics.
        /// </summary>
        internal const double offsetPeriod = 1351d;

        internal const string terrainShaderName = "KK/Terrain";
        internal const string oldGroundShaderName = "KK/Ground_Multi_NoUV";

        private static Shader groundShader = null;

        /// <summary>
        /// The shader for GrassColor2: KK/Terrain, or the old KK/Ground_Multi_NoUV with an older shader bundle
        /// </summary>
        internal static Shader GroundShader
        {
            get
            {
                if (groundShader == null)
                {
                    if (KKGraphics.HasShader(terrainShaderName))
                    {
                        groundShader = KKGraphics.GetShader(terrainShaderName);
                    }
                    else
                    {
                        Log.UserWarning(terrainShaderName + " is not in the shader bundle (an older bundle?): GrassColor2 uses " + oldGroundShaderName);
                        groundShader = KKGraphics.GetShader(oldGroundShaderName);
                    }
                }
                return groundShader;
            }
        }

        private static readonly int worldToPlanetID = Shader.PropertyToID("_KKWorldToPlanet");
        private static readonly int planetToWorldID = Shader.PropertyToID("_KKPlanetToWorld");
        private static readonly int sunID = Shader.PropertyToID("_KKSunDirection");
        // used only by the haze of the old KK/Ground_Multi_NoUV
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
        private static readonly int anchorUpID = Shader.PropertyToID("_KKAnchorUp");
        private static readonly int nearOffsetID = Shader.PropertyToID("_KKNearOffset");
        private static readonly int farOffsetID = Shader.PropertyToID("_KKFarOffset");
        private static readonly int thirdOffsetID = Shader.PropertyToID("_KKThirdOffset");
        private static readonly int fourthOffsetID = Shader.PropertyToID("_KKFourthOffset");
        private static readonly int steepNearOffsetID = Shader.PropertyToID("_KKSteepNearOffset");
        private static readonly int steepFarOffsetID = Shader.PropertyToID("_KKSteepFarOffset");
        private static readonly int steepNearTilingID = Shader.PropertyToID("_KKSteepNearTiling");
        private static readonly int steepFarTilingID = Shader.PropertyToID("_KKSteepFarTiling");

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

            // steep texture
            bool hasSteep = false;
            if (surfaceMaterial != null && surfaceMaterial.HasProperty(pqsSteepTexID) && surfaceMaterial.GetTexture(pqsSteepTexID) != null)
            {
                Shader.SetGlobalTexture(steepTexID, surfaceMaterial.GetTexture(pqsSteepTexID));
                Texture steepBump = surfaceMaterial.HasProperty(pqsSteepBumpID) ? surfaceMaterial.GetTexture(pqsSteepBumpID) : null;
                Shader.SetGlobalTexture(steepBumpID, steepBump != null ? steepBump : FlatNormalMap);
                Shader.SetGlobalFloat(steepPowerID, GetFloat(surfaceMaterial, pqsSteepPowerID, 1f));
                Shader.SetGlobalFloat(steepStartID, GetFloat(surfaceMaterial, pqsSteepStartID, 20000f));
                Shader.SetGlobalFloat(steepEndID, GetFloat(surfaceMaterial, pqsSteepEndID, 30000f));
                Shader.SetGlobalFloat(groundStartID, GetFloat(surfaceMaterial, pqsGroundStartID, 2000f));
                Shader.SetGlobalFloat(groundEndID, GetFloat(surfaceMaterial, pqsGroundEndID, 10000f));
                hasSteep = true;
            }
            Shader.SetGlobalFloat(steepEnabledID, hasSteep ? 1f : 0f);

            Log.Normal("KK terrain shader on " + body.name + " (" + (surfaceMaterial != null ? surfaceMaterial.shader.name : "no PQS material") +
                "): haze " + (hasHaze ? "on" : "off") + ", steep texture " + (hasSteep ? "on" : "off"));
        }


        /// <summary>
        /// Sets the planet anchored texture coordinates of one renderer's ground material
        /// </summary>
        internal static void SetRendererProperties(Material material, Renderer renderer, CelestialBody body,
            float nearTiling, float farTiling, float thirdTiling, float fourthTiling)
        {
            if (material == null || renderer == null || !material.HasProperty(terrainModeID))
            {
                return;
            }
            if (body == null || body.pqsController == null)
            {
                return;
            }
            Vector3d position;
            if (!TryGetPlanetFramePosition(renderer.transform, body.pqsController.transform, out position) || position.magnitude < 1d)
            {
                // not placed on the planet (e.g. a preview): the shader's defaults
                return;
            }
            Vector3d up = position.normalized;
            material.SetVector(anchorUpID, new Vector4((float)up.x, (float)up.y, (float)up.z, 0f));
            material.SetVector(nearOffsetID, Offset(position, nearTiling));
            material.SetVector(farOffsetID, Offset(position, farTiling));
            material.SetVector(thirdOffsetID, Offset(position, thirdTiling));
            material.SetVector(fourthOffsetID, Offset(position, fourthTiling));

            // the PQS shaders map the steep texture by the direction from the planet's center times the tiling:
            // per meter at the renderer that is tiling / distance from the center
            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (surfaceMaterial != null)
            {
                double steepFarTiling = GetFloat(surfaceMaterial, pqsSteepTilingID, 1f);
                double steepNearTiling = GetFloat(surfaceMaterial, pqsSteepNearTilingID, (float)steepFarTiling);
                double distance = position.magnitude;
                material.SetVector(steepNearOffsetID, Offset(up, steepNearTiling));
                material.SetVector(steepFarOffsetID, Offset(up, steepFarTiling));
                material.SetFloat(steepNearTilingID, (float)(steepNearTiling / distance));
                material.SetFloat(steepFarTilingID, (float)(steepFarTiling / distance));
            }
        }


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

        private static float GetFloat(Material material, int id, float fallback)
        {
            return material.HasProperty(id) ? material.GetFloat(id) : fallback;
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
        /// A flat normal map, for planets without a steep bump map
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
    }
}
