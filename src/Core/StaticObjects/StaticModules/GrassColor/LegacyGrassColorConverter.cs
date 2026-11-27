using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    /// <summary>
    /// Converts the grass color of the legacy GrasColor config (instance values GrasColor / GrasTexture) into a color
    /// for the GrassColor2 shader, so that the grass keeps the same average color on screen.
    ///
    /// Both grass shaders show  color.rgb * texture.rgb ; the alpha channel of the color is not used by them.
    /// The alpha only has a meaning in old configs, written for the first KK grass shader, which showed
    ///     lerp(color.rgb, texture.rgb, color.a)
    /// So a legacy color is read like this:
    ///  - 0 &lt; alpha &lt; 1 : an old "blend" color. On screen (on average): lerp(color, average(blend texture), alpha).
    ///                    The blend texture of the stock grass texture is KK's legacyGrassColors.
    ///  - alpha 0 or 1  : already a multiplier (also what older KK versions wrote back after converting).
    ///                    On screen (on average): color * average(shown texture).
    /// The new color is that average divided by the average of the new texture(s), as the GrassColor2 shader shows them
    /// (grayscale included), limited to 0..maxMultiplier, with alpha 1.
    ///
    /// The conversion is a pure function of the config values: it does not change the instance, and it gives the same
    /// result whichever grass module reads the config first, and however often it is run. Texture averages are cached.
    /// </summary>
    internal static class LegacyGrassColorConverter
    {
        internal const string legacyDefaultTexture = "BUILTIN:/terrain_grass00_new";
        internal const string legacyBlendTexture = "KerbalKonstructs/Assets/Colors/legacyGrassColors";
        internal const string newDefaultTexture = "BUILTIN:/terrain_grass00_new_detail";

        // the old conversion limited the multiplier to this, keep it
        internal const float maxMultiplier = 6f;

        // average colors by texture name: [0] = as is, [1] = grayscale as the GrassColor2 shader makes it
        private static Dictionary<string, Color[]> averageCache = new Dictionary<string, Color[]>();


        /// <summary>
        /// True for an old blend color (alpha strictly between 0 and 1)
        /// </summary>
        internal static bool IsBlendColor(Color legacyColor)
        {
            return (legacyColor.a > 0f && legacyColor.a < 1f);
        }


        /// <summary>
        /// The texture the legacy GrasColor module shows: the instance's GrasTexture, or the module's default
        /// </summary>
        internal static string ShownLegacyTexture(string legacyTexture, string moduleDefaultTexture = null)
        {
            if (!string.IsNullOrEmpty(legacyTexture))
            {
                return legacyTexture;
            }
            return string.IsNullOrEmpty(moduleDefaultTexture) ? legacyDefaultTexture : moduleDefaultTexture;
        }


        /// <summary>
        /// The texture a GrassColor2 module (that was not converted from GrasColor) gets when it imports a legacy config:
        /// the instance's own texture, else the stock detail texture
        /// </summary>
        internal static string GrassColor2Texture(string legacyTexture)
        {
            return string.IsNullOrEmpty(legacyTexture) ? newDefaultTexture : legacyTexture;
        }


        /// <summary>
        /// The average color the legacy config shows on screen (alpha 1)
        /// </summary>
        internal static Color DisplayedColor(Color legacyColor, string legacyTexture, string moduleDefaultTexture = null)
        {
            Color displayed;
            if (IsBlendColor(legacyColor))
            {
                string blendTexture = ShownLegacyTexture(legacyTexture, moduleDefaultTexture);
                if (blendTexture == legacyDefaultTexture)
                {
                    blendTexture = legacyBlendTexture;
                }
                Color textureAverage = Average(blendTexture, false);
                displayed = new Color(
                    Mathf.Lerp(legacyColor.r, textureAverage.r, legacyColor.a),
                    Mathf.Lerp(legacyColor.g, textureAverage.g, legacyColor.a),
                    Mathf.Lerp(legacyColor.b, textureAverage.b, legacyColor.a),
                    1f);
            }
            else
            {
                Color textureAverage = Average(ShownLegacyTexture(legacyTexture, moduleDefaultTexture), false);
                displayed = new Color(legacyColor.r * textureAverage.r, legacyColor.g * textureAverage.g, legacyColor.b * textureAverage.b, 1f);
            }
            return displayed;
        }


        /// <summary>
        /// The _GrassColor that gives the grass the average color of the ground underneath
        /// (groundColor: the PQS / camera color of the terrain)
        /// </summary>
        internal static Color FromGroundColor(Color groundColor, string nearTexture, bool nearGray, string farTexture, bool farGray)
        {
            Color ground = new Color(groundColor.r, groundColor.g, groundColor.b, 1f);
            Color newAverage = (Average(nearTexture, nearGray) + Average(farTexture, farGray)) * 0.5f;
            return Divide(ground, newAverage);
        }


        /// <summary>
        /// The _GrassColor for GrassColor2 (KK/Ground_Multi_NoUV), with the near and far grass textures it will use.
        /// </summary>
        internal static Color ToGrassColor2(Color legacyColor, string legacyTexture, string nearTexture, bool nearGray, string farTexture, bool farGray, string moduleDefaultTexture = null)
        {
            // same texture, shown the same way, and already a multiplier: nothing to convert
            string shownTexture = ShownLegacyTexture(legacyTexture, moduleDefaultTexture);
            if (!IsBlendColor(legacyColor) && nearTexture == shownTexture && farTexture == shownTexture && !nearGray && !farGray)
            {
                return Opaque(legacyColor);
            }

            Color displayed = DisplayedColor(legacyColor, legacyTexture, moduleDefaultTexture);

            // the shader blends near and far texture by distance: use the mean of both
            Color nearAverage = Average(nearTexture, nearGray);
            Color farAverage = Average(farTexture, farGray);
            Color newAverage = (nearAverage + farAverage) * 0.5f;

            return Divide(displayed, newAverage);
        }


        // the GrassColor2 settings of a converted legacy module
        internal const float convertedTiling = 1f;
        internal const float convertedBlendDistance = 200f;


        /// <summary>
        /// For a legacy GrasColor MODULE of a model config: the fields of the GrassColor2 MODULE that LoadModels()
        /// adds instead. The legacy texture (GrasTextureImage) becomes the near and far grass texture, with tiling 1
        /// and blend distance 200. False for any other module, or a GrasColor module without GrasMeshName.
        /// The instance's legacy GrasColor / GrasTexture values are converted when GrassColor2 reads its config.
        /// </summary>
        internal static bool TryGetGrassColor2Fields(StaticModule module, string modelName, out Dictionary<string, string> grassColor2Fields)
        {
            grassColor2Fields = null;
            if (ReferenceEquals(module, null) || module.moduleClassname != "GrasColor" || (!string.IsNullOrEmpty(module.moduleNamespace) && module.moduleNamespace != "KerbalKonstructs"))
            {
                return false;
            }

            Dictionary<string, string> fields = module.moduleFields;
            string meshName;
            if (!fields.TryGetValue("GrasMeshName", out meshName) || string.IsNullOrEmpty(meshName))
            {
                Log.UserWarning("GrasColor module of " + modelName + " has no GrasMeshName, not replaced by GrassColor2");
                return false;
            }

            string textureImage;
            if (!fields.TryGetValue("GrasTextureImage", out textureImage) || string.IsNullOrEmpty(textureImage))
            {
                textureImage = legacyDefaultTexture;
            }

            string tiling = convertedTiling.ToString(System.Globalization.CultureInfo.InvariantCulture);
            grassColor2Fields = new Dictionary<string, string>
            {
                { "GrassMeshName", meshName },
                { "LegacyGrasTextureImage", textureImage },
                { "DefaultNearGrassTexture", textureImage },
                { "DefaultFarGrassTexture", textureImage },
                { "DefaultNearGrassTiling", tiling },
                { "DefaultFarGrassTiling", tiling },
                { "DefaultFarGrassBlendDistance", convertedBlendDistance.ToString(System.Globalization.CultureInfo.InvariantCulture) }
            };
            return true;
        }


        /// <summary>
        /// One line for the log: what was converted into what
        /// </summary>
        internal static string Describe(Color legacyColor, string legacyTexture, Color newColor, string newTexture)
        {
            return "legacy grass color " + ConfigNode.WriteColor(legacyColor) + (IsBlendColor(legacyColor) ? " (blend, alpha " + legacyColor.a + ")" : " (multiplier)") +
                " on " + (string.IsNullOrEmpty(legacyTexture) ? "<default texture>" : legacyTexture) +
                " -> GrassColor2 color " + ConfigNode.WriteColor(newColor) + " on " + newTexture;
        }


        #region helpers

        private static Color Opaque(Color color)
        {
            return new Color(Mathf.Clamp(color.r, 0f, maxMultiplier), Mathf.Clamp(color.g, 0f, maxMultiplier), Mathf.Clamp(color.b, 0f, maxMultiplier), 1f);
        }


        private static Color Divide(Color value, Color divisor)
        {
            return new Color(
                Mathf.Clamp(value.r / Math.Max(divisor.r, 0.00001f), 0f, maxMultiplier),
                Mathf.Clamp(value.g / Math.Max(divisor.g, 0.00001f), 0f, maxMultiplier),
                Mathf.Clamp(value.b / Math.Max(divisor.b, 0.00001f), 0f, maxMultiplier),
                1f);
        }


        /// <summary>
        /// The average color of a texture (alpha 1), optionally after the GrassColor2 shader's grayscale
        /// </summary>
        internal static Color Average(string textureName, bool grayScale)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                return Color.grey;
            }

            Color[] averages;
            if (!averageCache.TryGetValue(textureName, out averages))
            {
                averages = ComputeAverages(textureName);
                averageCache[textureName] = averages;
            }
            return averages[grayScale ? 1 : 0];
        }


        private static Color[] ComputeAverages(string textureName)
        {
            Texture2D texture = KKGraphics.GetTexture(textureName);
            if (texture == null)
            {
                Log.UserWarning("LegacyGrassColorConverter: texture not found: " + textureName + ", using grey as its average");
                return new Color[] { Color.grey, Color.grey };
            }

            Texture2D small = texture.BlitTexture(64);
            Color[] pixels = small.GetPixels();
            UnityEngine.Object.Destroy(small);

            if (pixels.Length == 0)
            {
                return new Color[] { Color.grey, Color.grey };
            }

            double r = 0, g = 0, b = 0, gray = 0;
            foreach (Color pixel in pixels)
            {
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                gray += GrayScale(pixel);
            }
            float count = pixels.Length;
            Color average = new Color((float)(r / count), (float)(g / count), (float)(b / count), 1f);
            float grayAverage = (float)(gray / count);
            return new Color[] { average, new Color(grayAverage, grayAverage, grayAverage, 1f) };
        }


        /// <summary>
        /// Same as GrayScale() in the GrassColor2 shader (Ground_Multi_NoUV)
        /// </summary>
        private static float GrayScale(Color color)
        {
            float g = 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;
            float l = (Math.Max(Math.Max(color.r, color.g), color.b) + Math.Min(Math.Min(color.r, color.g), color.b)) * 0.5f;
            return (g + l) / 2f;
        }

        #endregion
    }
}
