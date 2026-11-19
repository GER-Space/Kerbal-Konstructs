using KerbalKonstructs.Core;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace KerbalKonstructs.UI2
{
    internal static class WindowManager2
    {
        private static readonly string storagePath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbalKonstructs/PluginData");
        private static readonly string storageFile = Path.Combine(storagePath, "WindowPositions.cfg");
        // older versions wrote the file next to PluginData because of a missing path separator
        private static readonly string oldStorageFile = storagePath + "WindowPositions.cfg";

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
            return GetPosition(name, new Vector2(0.5f, 0.5f));
        }


        internal static Vector2 GetPosition(string name, Vector2 defaultPosition)
        {

            if (lastPositions.ContainsKey(name))
            {
                return lastPositions[name];
            }
            else
            {
                Log.Normal("Window not found: " + name);
                return defaultPosition;
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
            Log.Normal("UI2.WindowManager2 initialized");
        }


        internal static void SavePresets()
        {
            ConfigNode positionsNode = new ConfigNode("WindowPositions");

            //Log.Normal("");
            foreach (var pos in lastPositions)
            {
                Vector2 position = pos.Value;
                position.x = Mathf.Clamp01(position.x);
                position.y = Mathf.Clamp01(position.y);
                ConfigNode node = positionsNode.AddNode("Position");
                node.AddValue("name", pos.Key);
                node.AddValue("position", position);
                Log.Normal("Saving: " + pos.Key + " : " + pos.Value);
            }

            if (!System.IO.Directory.Exists(storagePath))
            {
                Log.Normal("Creating Directory: " + storagePath);
                System.IO.Directory.CreateDirectory(storagePath);

            }
            ConfigNode masterNode = new ConfigNode("Master");
            masterNode.AddNode(positionsNode);
            masterNode.Save(storageFile);
        }


        internal static void LoadPresets()
        {
            lastPositions.Clear();
            string fileToLoad = storageFile;
            if (!System.IO.File.Exists(fileToLoad))
            {
                // fall back to the file written by older versions
                fileToLoad = oldStorageFile;
            }
            if (!System.IO.File.Exists(fileToLoad))
            {
                Log.Normal("No file found");
                return;
            }

            ConfigNode loadedNode = ConfigNode.Load(fileToLoad);
            ConfigNode positionsNode = (loadedNode != null) ? loadedNode.GetNode("WindowPositions") : null;
            if (positionsNode == null)
            {
                Log.Normal("No Node Found");
                return;
            }
            foreach (ConfigNode node in positionsNode.GetNodes("Position"))
            {
                string name = node.GetValue("name");
                Vector2 position = ConfigNode.ParseVector2(node.GetValue("position"));
                position.x = Mathf.Clamp01(position.x);
                position.y = Mathf.Clamp01(position.y);
                Log.Normal("loading: " + name + " : " + position.ToString());
                lastPositions.Add(name, position);
            }

        }


    }
}
