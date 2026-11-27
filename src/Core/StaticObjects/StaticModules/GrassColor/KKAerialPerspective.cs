using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// Feeds the aerial perspective (atmospheric haze) of KK/Ground_Multi_NoUV, so KK grass gets the same haze as the
    /// PQS terrain around it. KSP sets the haze inputs of the terrain shaders as globals (PQSMod_AerialPerspectiveMaterial:
    /// _heightFallOff, _atmosphereDepth, _cameraAltitude, _heightDensityAtViewer); the density and the color ramp are
    /// material values of the planet's PQS surface material (fogColorRamp / globalDensity in Kopernicus configs).
    /// This sets, once per frame, the rest as globals for the KK shader:
    ///  _KKAerialEnabled  1 on a body with an atmosphere and a PQS haze, else 0
    ///  _KKPlanetOrigin   world position of the body's center (floating origin moves it)
    ///  _KKPlanetRadius   radius of the body
    ///  _KKSunDirection   world direction from the body to the sun
    ///  _KKGlobalDensity  haze density of the body's PQS material
    ///  _KKFogColorRamp   haze color ramp of the body's PQS material
    /// The shader ignores all of it when it is an older version without the haze.
    /// </summary>
    internal static class KKAerialPerspective
    {
        private static readonly int enabledID = Shader.PropertyToID("_KKAerialEnabled");
        private static readonly int originID = Shader.PropertyToID("_KKPlanetOrigin");
        private static readonly int radiusID = Shader.PropertyToID("_KKPlanetRadius");
        private static readonly int sunID = Shader.PropertyToID("_KKSunDirection");
        private static readonly int densityID = Shader.PropertyToID("_KKGlobalDensity");
        private static readonly int rampID = Shader.PropertyToID("_KKFogColorRamp");

        private static readonly int pqsDensityID = Shader.PropertyToID("_globalDensity");
        private static readonly int pqsRampID = Shader.PropertyToID("_fogColorRamp");

        private static CelestialBody lastBody = null;
        private static Material lastMaterial = null;
        private static bool hasHaze = false;


        /// <summary>
        /// Called every frame (KerbalKonstructs.LateUpdate) in the scenes that show statics
        /// </summary>
        internal static void UpdateGlobals(CelestialBody body)
        {
            if (body == null || body.pqsController == null)
            {
                Shader.SetGlobalFloat(enabledID, 0f);
                return;
            }

            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (body != lastBody || surfaceMaterial != lastMaterial)
            {
                // a new body, or the terrain shader quality was changed
                lastBody = body;
                lastMaterial = surfaceMaterial;
                hasHaze = false;

                if (body.atmosphere && surfaceMaterial != null && surfaceMaterial.HasProperty(pqsRampID))
                {
                    Texture ramp = surfaceMaterial.GetTexture(pqsRampID);
                    float density = surfaceMaterial.HasProperty(pqsDensityID) ? surfaceMaterial.GetFloat(pqsDensityID) : Shader.GetGlobalFloat(pqsDensityID);
                    if (ramp != null && density != 0f)
                    {
                        Shader.SetGlobalTexture(rampID, ramp);
                        Shader.SetGlobalFloat(densityID, density);
                        hasHaze = true;
                    }
                }
                Log.Normal("Aerial perspective for KK grass on " + body.name + ": " + (hasHaze ? "on" : "off"));
            }

            Shader.SetGlobalFloat(enabledID, hasHaze ? 1f : 0f);
            if (!hasHaze)
            {
                return;
            }

            Vector3 origin = body.position;
            Shader.SetGlobalVector(originID, origin);
            Shader.SetGlobalFloat(radiusID, (float)body.Radius);

            CelestialBody sun = Planetarium.fetch != null ? Planetarium.fetch.Sun : null;
            if (sun != null)
            {
                Vector3 toSun = ((Vector3)(sun.position - body.position)).normalized;
                Shader.SetGlobalVector(sunID, toSun);
            }
        }
    }
}
