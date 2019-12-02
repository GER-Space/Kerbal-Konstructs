using KerbalKonstructs.Core;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    internal static class WindowManager
    {

        private static Dictionary<string, Vector2> lastPositions = new Dictionary<string, Vector2>();


        internal static void SavePosition(PopupDialog dialog)
        {
            if (dialog == null)
            {
                return;
            }

            string name = dialog.dialogToDisplay.name;
            if (lastPositions.ContainsKey(name))
            {
                lastPositions[name] = ConvertPosition(dialog.RTrf.position);
            }
            else
            {
                lastPositions.Add(name, ConvertPosition(dialog.RTrf.position));
            }
        }


        internal static Vector2 GetPosition(string name)
        {

            if (lastPositions.ContainsKey(name))
            {
                return lastPositions[name];
            }
            else
            {
                Log.Normal("Window not found: " + name);
                return new Vector2(0.5f, 0.5f);
            }
        }


        internal static Vector2 ConvertPosition(Vector3 rawPos)
        {
            float x = rawPos.x;
            float y = rawPos.y;

            x = (x + Screen.width / 2) / Screen.width;
            y = (y + Screen.height / 2) / Screen.height;

            return new Vector2(x, y);
        }

        internal static void Initialize()
        {
            KKStyle.Init();
            LoadPresets();
            Log.Normal("UI2.WindowManager initialized");
        }


        internal static void SavePresets()
        {
            ConfigNode positionsNode = new ConfigNode("WindowPositions");

            Log.Normal("");
            foreach (var pos in lastPositions)
            {
                ConfigNode node = positionsNode.AddNode("Position");
                node.AddValue("name", pos.Key);
                node.AddValue("position", pos.Value);
                Log.Normal("Saving: " + pos.Key + " : " + pos.Value);
            }

            if (!System.IO.Directory.Exists(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/"))
            {
                Log.Normal("Creating Directory: " + KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/");
                System.IO.Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/");

            }
            ConfigNode masterNode = new ConfigNode("Master");
            masterNode.AddNode(positionsNode);
            masterNode.Save(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/" + "WindowPositions.txt");
        }


        internal static void LoadPresets()
        {
            lastPositions.Clear();
            if (!System.IO.Directory.Exists(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/"))
            {
                Log.Normal("No Directory found");
                return;
            }
            if (!System.IO.File.Exists(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/" + "WindowPositions.txt"))
            {
                Log.Normal("No file found");
                return;
            }

            ConfigNode positionsNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "PluginData/KerbalKonstructs/" + "WindowPositions.txt").GetNode("WindowPositions");
            if (positionsNode == null)
            {
                Log.Normal("No Node Found");
                return;
            }
            foreach (ConfigNode node in positionsNode.GetNodes("Position"))
            {
                string name = node.GetValue("name");
                Vector2 pos = ConfigNode.ParseVector2(node.GetValue("position"));
                Log.Normal("loading: " + name + " : " + pos.ToString());
                lastPositions.Add(name, pos);
            }

        }


    }
}
