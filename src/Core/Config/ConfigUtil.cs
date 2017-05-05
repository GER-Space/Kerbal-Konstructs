using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KerbalKonstructs.Core;
using UnityEngine;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    internal static class ConfigUtil
    {
        internal static bool initialized = false;
        // Model Settings
        private static List<string> stringAttributesModel = new List<string>();
        private static List<string> intAttributesModel = new List<string>();
        private static List<string> floatAttributesModel = new List<string>();
        private static List<string> doubleAttributesModel = new List<string>();
        private static List<string> vector3AttributesModel = new List<string>();
        private static List<string> boolAttributesModel = new List<string>();

        // Instance Settings
        private static List<string> stringAttributesInstance = new List<string>();
        private static List<string> intAttributesInstance = new List<string>();
        private static List<string> floatAttributesInstance = new List<string>();
        private static List<string> doubleAttributesInstance = new List<string>();
        private static List<string> vector3AttributesInstance = new List<string>();
        private static List<string> boolAttributesInstance = new List<string>();

        // combined settings
        private static List<string> stringAttributes;
        private static List<string> intAttributes;
        private static List<string> floatAttributes;
        private static List<string> doubleAttributes;
        private static List<string> vector3Attributes;
        private static List<string> boolAttributes;

        internal static Dictionary<string, Type> modelTypes = new Dictionary<string, Type>();
        internal static Dictionary<string, FieldInfo> modelFields = new Dictionary<string, FieldInfo>();


        internal static Dictionary<string, Type> instanceTypes = new Dictionary<string, Type>();
        internal static Dictionary<string, FieldInfo> instanceFields = new Dictionary<string, FieldInfo>();

        internal static void InitTypes()
        {
            foreach (FieldInfo field in typeof(StaticModel).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                modelTypes.Add(field.Name, field.FieldType);
                modelFields.Add(field.Name, field);

                Log.Normal("Parser Model:" + field.FieldType.ToString());

                if (field.FieldType == typeof(string))
                {
                    stringAttributesModel.Add(field.Name);
                }
                if (field.FieldType == typeof(int))
                {
                    intAttributesModel.Add(field.Name);
                }
                if (field.FieldType == typeof(float))
                {
                    floatAttributesModel.Add(field.Name);
                }
                if (field.FieldType == typeof(double))
                {
                    doubleAttributesModel.Add(field.Name);
                }
                if (field.FieldType == typeof(Vector3))
                {
                    vector3AttributesModel.Add(field.Name);
                }
                if (field.FieldType == typeof(bool))
                {
                    boolAttributesModel.Add(field.Name);
                }
            }


            foreach (FieldInfo field in typeof(StaticObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                instanceTypes.Add(field.Name, field.GetType());
                instanceFields.Add(field.Name, field);
                Log.Normal("Parser Instance: " + field.GetType().ToString());

                if (field.FieldType == typeof(string))
                {
                    stringAttributesInstance.Add(field.Name);
                }
                if (field.FieldType == typeof(int))
                {
                    intAttributesInstance.Add(field.Name);
                }
                if (field.FieldType == typeof(float))
                {
                    floatAttributesInstance.Add(field.Name);
                }
                if (field.FieldType == typeof(double))
                {
                    doubleAttributesInstance.Add(field.Name);
                }
                if (field.FieldType == typeof(Vector3))
                {
                    vector3AttributesInstance.Add(field.Name);
                }
                if (field.FieldType == typeof(bool))
                {
                    boolAttributesInstance.Add(field.Name);
                }
            }

            stringAttributes = stringAttributesModel;
            stringAttributes.AddRange(stringAttributesInstance);
            intAttributes = intAttributesModel;
            intAttributes.AddRange(intAttributesInstance);
            floatAttributes = floatAttributesModel;
            floatAttributes.AddRange(floatAttributesInstance);
            doubleAttributes = doubleAttributesModel;
            doubleAttributes.AddRange(doubleAttributesInstance);
            vector3Attributes = vector3AttributesModel;
            vector3Attributes.AddRange(vector3AttributesInstance);
            boolAttributes = boolAttributesModel;
            boolAttributes.AddRange(boolAttributesInstance);

            initialized = true;

            Log.Normal("Parser Counts: " + stringAttributesModel.Count + " " + stringAttributesInstance.Count);
        }



        /// <summary>
        /// returns a list with the necessarry parameters for a LaunchSite
        /// </summary>
        /// <returns></returns>
        internal static List<string> ParametersForLaunchSite()
        {
            List<string> paramList = new List<string> { "openclosestate", "favouritesite", "missioncount", "missionlog" };
            return paramList;
        }


        internal static bool IsString(string parameter)
        {
            if (!initialized)
                InitTypes();

            return stringAttributes.Contains(parameter);
        }

        internal static bool IsInt(string parameter)
        {
            if (!initialized)
                InitTypes();

            return intAttributes.Contains(parameter);
        }

        internal static bool IsFloat(string parameter)
        {
            if (!initialized)
                InitTypes();

            return floatAttributes.Contains(parameter);
        }

        internal static bool IsDouble(string parameter)
        {
            if (!initialized)
                InitTypes();

            return doubleAttributes.Contains(parameter);
        }

        internal static bool IsVector3(string parameter)
        {
            if (!initialized)
                InitTypes();

            return vector3Attributes.Contains(parameter);
        }

        internal static bool IsBool(string parameter)
        {
            if (!initialized)
                InitTypes();

            return boolAttributes.Contains(parameter);
        }

        internal static string KeyFromString(string orginalString)
        {
            return Regex.Replace(orginalString, @"\s+", "");
        }

        internal static string LSKeyFromName(string name)
        {
            return name.Replace(' ', '_').ToUpperInvariant();
        }

    }
}
