using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class GrasColor: StaticModule
    {

        public string GrasMeshName;

        public void Awake()
        {
            //Log.Normal("FlagDeclal: Awake called");
            //setTexture();
        }

        public override void StaticObjectUpdate()
        {
            //Log.Normal("FlagDeclal: Set Texture Called");
            setTexture();
        }

        internal Color GetColor()
        {

            
            Color underGroundColor = Color.clear;
            if (staticInstance.GrasColor == Color.clear)
            {
                underGroundColor = GrasColorCam.instance.getCameraColor(staticInstance);
                staticInstance.GrasColor = underGroundColor;
            }
            else
            {
                underGroundColor = staticInstance.GrasColor;
            }
            // Make the color a bit lighter
            //underGroundColor.r += 0.1f;
            //underGroundColor.g += 0.1f;
            //underGroundColor.b += 0.1f;

            Log.Normal("underGroundColor: " + underGroundColor.ToString());

                return underGroundColor;

        }


        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void setTexture()
        {
            //Log.Normal("FlagDeclal: setTexture called");

            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrasMeshName).ToArray();


            foreach (var transform in allTransforms)
            {
                Renderer flagRenderer = transform.GetComponent<Renderer>();
                flagRenderer.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                flagRenderer.material.mainTexture = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/Textures/BW_grass_1", false);

                flagRenderer.material.SetColor("_Color", GetColor());
            }
        }


    }
}