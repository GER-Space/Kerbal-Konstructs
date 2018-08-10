using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{
    static class API
    {

        public static string SpawnObject(string modelName)
        {
            StaticModel model = StaticDatabase.GetModelByName(modelName);
            if (model != null)
            {
                return CareerEditor.instance.SpawnInstance(model, 3f, KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position));
            }
            else
            {
                Log.UserError("API:SpawnObject: Could not find selected KK-Model named: " + modelName);
                return null;
            }
        }

        public static void RemoveStatic(string uuid)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                StaticDatabase.DeleteStatic(StaticDatabase.instancedByUUID[uuid]);
            }
            else
            {
                Log.UserWarning("API:RemoveObject: Can´t find a static with the UUID: " + uuid);
            }
        }

        public static void HighLightStatic(string uuid, Color color)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                StaticDatabase.instancedByUUID[uuid].HighlightObject(color);
            }
            else
            {
                Log.UserWarning("API:Highlight: Can´t find a static with the UUID: " + uuid);
            }
        }

        public static string GetModelTitel(string uuid)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                return StaticDatabase.instancedByUUID[uuid].model.title;
            }
            else
            {
                Log.UserWarning("API:GetModelTitel: Can´t find a static with the UUID: " + uuid);
                return null;
            }
        }

        public static void SetEditorRange(float newRange)
        {
            CareerEditor.maxEditorRange = newRange;
        }


    }
}
