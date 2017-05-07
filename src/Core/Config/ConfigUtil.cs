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
    /// <summary>
    /// We use dictionarys for the lookup of the parameter types, because they are way faster then making reflection lookups.
    /// We use reflektion calls to scan for the internal datatypes of SaticModule and StaticObject - (this might be changed to attributes) - 
    /// We have for each cfgfile-setting a same named field in the classes, so we don't need a translation table.
    /// </summary>
    internal static class ConfigUtil
    {
        internal static bool initialized = false;
        // Model Settings
        private static List<string> stringAttributesModel = new List<string>();
        private static List<string> intAttributesModel = new List<string>();
        private static List<string> floatAttributesModel = new List<string>();
        private static List<string> doubleAttributesModel = new List<string>();
        private static List<string> vector3AttributesModel = new List<string>();
        private static List<string> vector3dAttributesModel = new List<string>();
        private static List<string> boolAttributesModel = new List<string>();
        private static List<string> CelBodyAttributesModel = new List<string>();

        // Instance Settings
        private static List<string> stringAttributesInstance = new List<string>();
        private static List<string> intAttributesInstance = new List<string>();
        private static List<string> floatAttributesInstance = new List<string>();
        private static List<string> doubleAttributesInstance = new List<string>();
        private static List<string> vector3AttributesInstance = new List<string>();
        private static List<string> vector3dAttributesInstance = new List<string>();
        private static List<string> boolAttributesInstance = new List<string>();
        private static List<string> CelBodyAttributesInstance = new List<string>();

        // combined settings
        private static List<string> stringAttributes;
        private static List<string> intAttributes;
        private static List<string> floatAttributes;
        private static List<string> doubleAttributes;
        private static List<string> vector3Attributes;
        private static List<string> vector3dAttributes;
        private static List<string> boolAttributes;
        private static List<string> CelBodyAttributes;

        internal static Dictionary<string, Type> modelTypes = new Dictionary<string, Type>();
        internal static Dictionary<string, FieldInfo> modelFields = new Dictionary<string, FieldInfo>();


        internal static Dictionary<string, Type> instanceTypes = new Dictionary<string, Type>();
        internal static Dictionary<string, FieldInfo> instanceFields = new Dictionary<string, FieldInfo>();


        /// <summary>
        /// Fills up the lookup tables for the parser. 
        /// </summary>
        internal static void InitTypes()
        {
            foreach (FieldInfo field in typeof(StaticModel).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                modelTypes.Add(field.Name, field.FieldType);
                modelFields.Add(field.Name, field);

                Log.Normal("Parser Model:" + field.FieldType.ToString());

                switch (field.FieldType.ToString())
                {
                    case "System.String":
                        stringAttributesModel.Add(field.Name);
                        break;
                    case "System.Int32":
                        intAttributesModel.Add(field.Name);
                        break;
                    case "System.Single":
                        floatAttributesModel.Add(field.Name);
                        break;
                    case "System.Double":
                        doubleAttributesModel.Add(field.Name);
                        break;
                    case "UnityEngine.Vector3":
                        vector3AttributesModel.Add(field.Name);
                        break;
                    case "UnityEngine.Vector3d":
                        vector3dAttributesModel.Add(field.Name);
                        break;
                    case "CelestialBody":
                        CelBodyAttributesModel.Add(field.Name);
                        break;
                    case "System.Boolean":
                        boolAttributesModel.Add(field.Name);
                        break;
                }

            }
            foreach (FieldInfo field in typeof(StaticObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                instanceTypes.Add(field.Name, field.GetType());
                instanceFields.Add(field.Name, field);
                Log.Normal("Parser Instance: " + field.FieldType.ToString());


                switch (field.FieldType.ToString())
                {
                    case "System.String":
                        stringAttributesInstance.Add(field.Name);
                        break;
                    case "System.Int32":
                        intAttributesInstance.Add(field.Name);
                        break;
                    case "System.Single":
                        floatAttributesInstance.Add(field.Name);
                        break;
                    case "System.Double":
                        doubleAttributesInstance.Add(field.Name);
                        break;
                    case "UnityEngine.Vector3":
                        vector3AttributesInstance.Add(field.Name);
                        break;
                    case "UnityEngine.Vector3d":
                        vector3dAttributesInstance.Add(field.Name);
                        break;
                    case "CelestialBody":
                        CelBodyAttributesInstance.Add(field.Name);
                        break;
                    case "System.Boolean":
                        boolAttributesInstance.Add(field.Name);
                        break;
                }
                //if (field.FieldType == typeof(string))
                //{
                //    stringAttributesInstance.Add(field.Name);
                //}
                //if (field.FieldType == typeof(int))
                //{
                //    intAttributesInstance.Add(field.Name);
                //}
                //if (field.FieldType == typeof(float))
                //{
                //    floatAttributesInstance.Add(field.Name);
                //}
                //if (field.FieldType == typeof(double))
                //{
                //    doubleAttributesInstance.Add(field.Name);
                //}
                //if (field.FieldType == typeof(Vector3))
                //{
                //    vector3AttributesInstance.Add(field.Name);
                //}
                //if (field.FieldType == typeof(bool))
                //{
                //    boolAttributesInstance.Add(field.Name);
                //}
            }

            stringAttributes = stringAttributesModel.ToList();
            stringAttributes.AddRange(stringAttributesInstance.ToList());
            intAttributes = intAttributesModel.ToList();
            intAttributes.AddRange(intAttributesInstance.ToList());
            floatAttributes = floatAttributesModel.ToList();
            floatAttributes.AddRange(floatAttributesInstance.ToList());
            doubleAttributes = doubleAttributesModel.ToList();
            doubleAttributes.AddRange(doubleAttributesInstance.ToList());
            vector3Attributes = vector3AttributesModel.ToList();
            vector3Attributes.AddRange(vector3AttributesInstance.ToList());
            vector3dAttributes = vector3AttributesModel.ToList();
            vector3dAttributes.AddRange(vector3AttributesInstance.ToList());
            boolAttributes = boolAttributesModel.ToList();
            boolAttributes.AddRange(boolAttributesInstance.ToList());
            CelBodyAttributes = CelBodyAttributesModel.ToList();
            CelBodyAttributes.AddRange(CelBodyAttributesInstance.ToList());

            initialized = true;
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

        internal static bool IsVector3d(string parameter)
        {
            if (!initialized)
                InitTypes();

            return vector3dAttributes.Contains(parameter);
        }

        internal static bool IsBool(string parameter)
        {
            if (!initialized)
                InitTypes();

            return boolAttributes.Contains(parameter);
        }

        internal static bool IsCelBody(string parameter)
        {
            if (!initialized)
                InitTypes();

            return CelBodyAttributes.Contains(parameter);
        }


    }
}
