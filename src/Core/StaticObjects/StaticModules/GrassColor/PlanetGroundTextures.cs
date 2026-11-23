using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// The ground texture and colour the planet's own terrain shader shows at a static's position.
    /// Used by GrassColor2 when "UsePlanetTextures" is on, so a static's grass matches the terrain around it.
    /// </summary>
    internal struct PlanetGround
    {
        internal Texture2D texture;
        internal Color color;
        internal string bandName;
    }

    /// <summary>
    /// Reads the ground textures from the body's PQS surface material (stock, or what Kopernicus configured).
    /// Works with the stock "PQS Main" shader family (low/mid/high textures).
    /// The 1.9+ "Ultra" shader uses texture arrays chosen per terrain vertex, which can't be put on a static,
    /// so on Ultra (or any other unknown shader) TryGet returns false and KK's own grass textures are used.
    /// </summary>
    internal static class PlanetGroundTextures
    {
        // Re-sample the PQS only when the static moved further than this (metres), the body changed,
        // or the surface material was replaced (the terrain shader quality was changed).
        private const float resampleDistance = 10f;

        private class CacheEntry
        {
            internal CelestialBody body;
            internal Material surfaceMaterial;
            internal Vector3 bodyPosition;
            internal bool valid;
            internal PlanetGround ground;
        }

        private static readonly Dictionary<StaticInstance, CacheEntry> cache = new Dictionary<StaticInstance, CacheEntry>();

        private static readonly int lowTexID = Shader.PropertyToID("_lowTex");
        private static readonly int midTexID = Shader.PropertyToID("_midTex");
        private static readonly int highTexID = Shader.PropertyToID("_highTex");
        private static readonly int lowStartID = Shader.PropertyToID("_lowStart");
        private static readonly int lowEndID = Shader.PropertyToID("_lowEnd");
        private static readonly int highStartID = Shader.PropertyToID("_highStart");
        private static readonly int highEndID = Shader.PropertyToID("_highEnd");

        internal static bool TryGet(StaticInstance instance, out PlanetGround ground)
        {
            ground = default;

            if (instance == null || instance.gameObject == null)
            {
                return false;
            }

            CelestialBody body = instance.CelestialBody;
            if (body == null || body.pqsController == null)
            {
                return false;
            }

            Material surfaceMaterial = body.pqsController.surfaceMaterial;
            if (surfaceMaterial == null)
            {
                return false;
            }

            Vector3 bodyPosition = body.transform.InverseTransformPoint(instance.gameObject.transform.position);

            if (cache.TryGetValue(instance, out CacheEntry entry)
                && entry.body == body
                && entry.surfaceMaterial == surfaceMaterial
                && (entry.bodyPosition - bodyPosition).sqrMagnitude < resampleDistance * resampleDistance)
            {
                ground = entry.ground;
                return entry.valid;
            }

            if (entry == null)
            {
                entry = new CacheEntry();
                cache[instance] = entry;
            }
            entry.body = body;
            entry.surfaceMaterial = surfaceMaterial;
            entry.bodyPosition = bodyPosition;
            entry.valid = Sample(instance, body, surfaceMaterial, out entry.ground);

            ground = entry.ground;
            return entry.valid;
        }

        internal static void Forget(StaticInstance instance)
        {
            if (instance != null)
            {
                cache.Remove(instance);
            }
        }

        private static bool Sample(StaticInstance instance, CelestialBody body, Material surfaceMaterial, out PlanetGround ground)
        {
            ground = default;

            Texture2D midTexture = GetTexture2D(surfaceMaterial, midTexID);
            if (midTexture == null)
            {
                // Ultra (texture arrays), the fallback shader or a custom shader: nothing we can use
                return false;
            }

            Vector2d latLon = body.GetLatitudeAndLongitude(instance.gameObject.transform.position);

            Color vertexColor;
            try
            {
                vertexColor = GrassColorUtils.SampleSurfaceVertexColorPQS(body, latLon.x, latLon.y);
            }
            catch (System.Exception e)
            {
                Log.UserWarning("PlanetGroundTextures: could not sample the PQS of " + body.name + ": " + e.Message);
                return false;
            }

            // The PQS Main shaders blend low -> mid between _lowStart and _lowEnd and mid -> high between
            // _highStart and _highEnd, using the vertex colour's alpha. A static gets the texture that dominates.
            float altitudeAlpha = vertexColor.a;
            ground.texture = midTexture;
            ground.bandName = "mid";

            if (surfaceMaterial.HasProperty(lowStartID) && surfaceMaterial.HasProperty(lowEndID)
                && altitudeAlpha < (surfaceMaterial.GetFloat(lowStartID) + surfaceMaterial.GetFloat(lowEndID)) * 0.5f)
            {
                Texture2D lowTexture = GetTexture2D(surfaceMaterial, lowTexID);
                if (lowTexture != null)
                {
                    ground.texture = lowTexture;
                    ground.bandName = "low";
                }
            }
            else if (surfaceMaterial.HasProperty(highStartID) && surfaceMaterial.HasProperty(highEndID)
                && altitudeAlpha > (surfaceMaterial.GetFloat(highStartID) + surfaceMaterial.GetFloat(highEndID)) * 0.5f)
            {
                Texture2D highTexture = GetTexture2D(surfaceMaterial, highTexID);
                if (highTexture != null)
                {
                    ground.texture = highTexture;
                    ground.bandName = "high";
                }
            }

            // Same small lift KK applies to its other PQS colours; the terrain shader multiplies the texture by this colour too
            Color color = vertexColor + new Color(0.01f, 0.01f, 0.02f);
            color.a = 1f;
            ground.color = color;

            return true;
        }

        private static Texture2D GetTexture2D(Material material, int propertyID)
        {
            if (!material.HasProperty(propertyID))
            {
                return null;
            }
            return material.GetTexture(propertyID) as Texture2D;
        }
    }
}
