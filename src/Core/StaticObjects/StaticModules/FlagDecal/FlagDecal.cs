using KerbalKonstructs.Core;
using System.Linq;
using UnityEngine;


namespace KerbalKonstructs
{
    public class FlagDecal : StaticModule
    {

        public string textureQuadName;

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

        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void setTexture()
        {
            //Log.Normal("FlagDeclal: setTexture called");
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == textureQuadName).ToArray();

            foreach (var transform in allTransforms)
            {
                Renderer flagRenderer = transform.GetComponent<Renderer>();
                flagRenderer.material.mainTexture = GameDatabase.Instance.GetTexture(HighLogic.CurrentGame.flagURL, false);
            }
        }


    }
}