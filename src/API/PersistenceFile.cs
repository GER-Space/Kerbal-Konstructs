using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KerbalKonstructs.API
{
    class PersistenceFile<T>
    {
        public static void Load(T obj, string nodeName, string filename)
        {
            ConfigNode cfg = ConfigNode.Load(GetFullFilename(filename));
            if (cfg == null) return;
            ConfigNode node = cfg.GetNode(nodeName);
            if (node == null) return;
            LoadObj(obj, node);
        }

        public static void Save(T obj, string nodeName, string filename)
        {
            ConfigNode cfg = ConfigNode.Load(GetFullFilename(filename)) ?? new ConfigNode();
            ConfigNode node = cfg.GetNode(nodeName) ?? cfg.AddNode(nodeName);
            SaveObj(obj, node);
            cfg.Save(string.Format(GetFullFilename(filename)));
        }

        public static void LoadList(List<T> objs, string nodeName, string filename)
        {
            ConfigNode cfg = ConfigNode.Load(GetFullFilename(filename));
            if (cfg == null) return;
            ConfigNode node = cfg.GetNode(nodeName);
            if (node == null) return;
            foreach (var obj in objs)
            {
                var keyFI = obj.GetType().GetFields().First<FieldInfo>((FieldInfo) => Attribute.IsDefined(FieldInfo, typeof(PersistentKey)));
                var objNodeName = GetNodeName(keyFI.GetValue(obj).ToString());
                if (keyFI == null || !node.HasNode(objNodeName)) continue;
                LoadObj(obj, node.GetNode(objNodeName));
            }
        }

        public static void SaveList(List<T> objs, string nodeName, string filename)
        {
            ConfigNode cfg = ConfigNode.Load(GetFullFilename(filename)) ?? new ConfigNode();
            ConfigNode node = cfg.GetNode(nodeName) ?? cfg.AddNode(nodeName);
            foreach (var obj in objs)
            {
                var keyFI = obj.GetType().GetFields().First<FieldInfo>((FieldInfo) => Attribute.IsDefined(FieldInfo, typeof(PersistentKey)));
                if (keyFI == null) continue;
                
                var objNodeName = GetNodeName(keyFI.GetValue(obj).ToString());
                SaveObj(obj, node.GetNode(objNodeName) ?? node.AddNode(objNodeName));
            }
            cfg.Save(string.Format(GetFullFilename(filename)));
        }

        private static string GetFullFilename(string filename)
        {
            return string.Format("{0}saves/{1}/{2}.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder, filename);
        }

        private static string GetNodeName(string name)
        {
            return name.Replace(' ', '_').ToUpperInvariant();
        }

        private static void LoadObj(T obj, ConfigNode node)
        {
            foreach (var fieldInfo in obj.GetType().GetFields().Where<FieldInfo>((FieldInfo fieldInfo) => Attribute.IsDefined(fieldInfo, typeof(PersistentField)) && node.HasValue(fieldInfo.Name)))
                fieldInfo.SetValue(obj, Convert.ChangeType(node.GetValue(fieldInfo.Name), fieldInfo.FieldType));
        }

        private static void SaveObj(T obj, ConfigNode node)
        {
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields().Where<FieldInfo>((FieldInfo fieldInfo) => Attribute.IsDefined(fieldInfo, typeof(PersistentField))))
            {
                if (node.HasValue(fieldInfo.Name))
                    node.SetValue(fieldInfo.Name, fieldInfo.GetValue(obj).ToString());
                else
                    node.AddValue(fieldInfo.Name, fieldInfo.GetValue(obj));
            }
        }

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PersistentKey : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PersistentField : Attribute
    {
    }

}
