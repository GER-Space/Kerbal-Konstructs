using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class FlagDecal: StaticModule
    {

        public string textureQuadName;

        public void Awake()
        {
            Log.Normal("FlagDeclal: Awake called");
            //setTexture();
        }

        public override void StaticObjectUpdate()
        {
            Log.Normal("FlagDeclal: Set Texture Called");
            setTexture();
        }
        internal void setTexture ()
        {
            Transform transform = gameObject.transform.FindChild(textureQuadName);
            Renderer flagRenderer = transform.GetComponent<Renderer>();
            flagRenderer.material.mainTexture = GameDatabase.Instance.GetTexture(HighLogic.CurrentGame.flagURL, false);
        }



    }
}
