using System;
using System.Collections.Generic;
using System.Linq;

namespace KerbalKonstructs.Core
{

    enum TextureUsage
    {
        Unused,
        Texture,
        BlendMask
    }

    class TexturePreset
    {
        internal string texturePath = "";
        internal TextureUsage usage = TextureUsage.Unused;
    }


    /// <summary>
    /// The textures offered by the texture picker of the grass editors: the KK_TexturePreset config nodes
    /// plus the textures found on the stock KSC models (additionalTextures, filled by SquadStatics).
    /// (the window part of the old TextureSelector is now UI2.TexturePicker)
    /// </summary>
    static class TexturePresets
    {
        private static bool isInitialized = false;
        private static List<TexturePreset> textureList = new List<TexturePreset>();

        internal static List<TexturePreset> additionalTextures = new List<TexturePreset>();


        private static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            textureList = new List<TexturePreset>(additionalTextures);
            foreach (ConfigNode colorNode in GameDatabase.Instance.GetConfigNodes("KK_TexturePreset"))
            {
                if (colorNode.HasValue("TextureFile") && colorNode.HasValue("Usage"))
                {
                    TexturePreset preset = new TexturePreset();
                    preset.texturePath = colorNode.GetValue("TextureFile");

                    if (!Enum.TryParse(colorNode.GetValue("Usage"), true, out preset.usage))
                    {
                        preset.usage = TextureUsage.Unused;
                    }

                    if (textureList.Any(x => x.texturePath == preset.texturePath && x.usage == preset.usage))
                    {
                        continue;
                    }
                    textureList.Add(preset);
                }
            }
            textureList.Sort(delegate (TexturePreset first, TexturePreset second)
            {
                return first.texturePath.CompareTo(second.texturePath);
            });
        }


        /// <summary>
        /// The texture paths for one usage, sorted by path
        /// </summary>
        internal static List<string> GetTextures(TextureUsage usage)
        {
            Initialize();
            return textureList.Where(preset => preset.usage == usage).Select(preset => preset.texturePath).Distinct().ToList();
        }
    }
}
