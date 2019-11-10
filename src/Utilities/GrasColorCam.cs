using System;
using UnityEngine;

//
// This class is copied from KSPWheel by ShadowMage
// https://github.com/shadowmage45/KSPWheel
// License of this is GPLv3 
//
namespace KerbalKonstructs.Core
{
    //[KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class GrasColorCam : MonoBehaviour
    {
        private static float frameWidth = 6f;
        private static float frameHeight = 6f;
        private static int cameraMask = 32784;

        private static GrasColorCam _instance = null;

        private GameObject cameraObject;
        private Camera grasCamera;
        private Texture2D cameraTexture;
        private RenderTexture cameraRenderTexture;

        private static GameObject myGameObject;

        private bool cameraInitialized = false;

        public static GrasColorCam instance
        {
            get
            {
                if (_instance == null)
                {
                    myGameObject = new GameObject();
                    _instance = myGameObject.AddComponent<GrasColorCam>();
                }
                return _instance;
            }
        }

        //public void Awake()
        //{
        //    DontDestroyOnLoad(this);
        //    _instance = this;
        //}

        //public void Start()
        //{
        //    DontDestroyOnLoad(this);
        //    _instance = this;
        //}

        public void OnDestroy()
        {
            GameObject.Destroy(grasCamera);
            GameObject.Destroy(cameraObject);
            GameObject.Destroy(cameraTexture);
            GameObject.Destroy(cameraRenderTexture);
            _instance = null;
        }

        private void InitializeCamera()
        {
            cameraInitialized = true;
            cameraObject = new GameObject("GrasColorCamera");
            Log.Normal("Cam created");
            if (cameraObject == null)
            {
                Log.UserError("No CamObject was created");
            }
            cameraObject.transform.parent = this.gameObject.transform;
            grasCamera = cameraObject.AddComponent<Camera>();
            grasCamera.targetTexture = cameraRenderTexture;
            grasCamera.cullingMask = cameraMask;
            grasCamera.enabled = false;
            cameraRenderTexture = new RenderTexture(Convert.ToInt32(frameWidth), Convert.ToInt32(frameHeight), 24);
            cameraTexture = new Texture2D(Convert.ToInt32(frameWidth), Convert.ToInt32(frameHeight), TextureFormat.RGB24, false);
        }

        /// <summary>
        /// places the camera above the terrain of the staticObject
        /// </summary>
        /// <param name="instance"></param>
        private void SetupCameraForVessel(StaticInstance instance)
        {
            if (!cameraInitialized)
            {
                InitializeCamera();
            }
            //cameraObject.transform.position = instance.gameObject.transform.position;
            // set the position 3 meters above the ground
            cameraObject.transform.position = instance.CelestialBody.GetWorldSurfacePosition(instance.RefLatitude, instance.RefLongitude, (instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.pqsController.radius + 10d));
            cameraObject.transform.LookAt(instance.CelestialBody.transform.position);
            // cameraObject.transform.Translate(0, 0, -10f);//translate negative z, as it is pointed at the ground this will leave it 10m above the ground at the vessels position, with the ground fully in the camera view box
        }

        /// <summary>
        /// Returns the Color for the Instance Position
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Color GetCameraColor(StaticInstance instance)
        {
            SetupCameraForVessel(instance);
            // Move the current object out of the cams view
            InstanceUtil.SetLayerRecursively(instance, 0);
            grasCamera.targetTexture = cameraRenderTexture;
            grasCamera.enabled = true;
            //Light light = grasCamera.gameObject.AddOrGetComponent<Light>();
            //light.type = LightType.Point;
            //light.intensity = 1;
            //light.color = Color.white;
            //light.enabled = true;
            grasCamera.Render();
            //light.enabled = false;

            Ray myRay = new Ray(cameraObject.transform.position, instance.CelestialBody.transform.position);
            RaycastHit castHit = new RaycastHit();
            if (!Physics.Raycast(myRay, out castHit, float.PositiveInfinity, 1 << 15))
            {
                Log.Normal("NO raycast hit");
            }
            else
            {

                Renderer rend = castHit.transform.GetComponent<Renderer>();

                if (rend != null)
                {
                    if (rend.materials.Length != 0)
                    {
                        foreach (string name in rend.material.GetTexturePropertyNames())
                        {
                            Texture2D lowTex = (Texture2D) rend.material.GetTexture("_lowTex");
                        }
                    }
                    else
                    {
                        Log.Normal("No Raycast material found");
                    }
                }
                else
                {
                    Log.Normal("No renderer found");
                }
            }


            // bring it back to the normal scenery
            InstanceUtil.SetLayerRecursively(instance, 15);

            Color newColor = GrassColorUtils.AverageColor(cameraRenderTexture.ToTexture2D().GetPixels());
            newColor.a = 1;

            return newColor;
        }


        public Texture2D GetCameraTexture(StaticInstance instance)
        {
            Texture2D MidTex2 = null;
            SetupCameraForVessel(instance);

            Ray myRay = new Ray(cameraObject.transform.position, instance.CelestialBody.transform.position);
            RaycastHit castHit = new RaycastHit();
            if (!Physics.Raycast(myRay, out castHit, float.PositiveInfinity, 1 << 15))
            {
                Log.Normal("NO raycast hit");
            }
            else
            {

                Renderer rend = castHit.transform.GetComponent<Renderer>();

                if (rend != null)
                {
                    if (rend.materials.Length != 0)
                    {
                        Texture midTex = rend.material.GetTexture("_lowTex");
                        if (midTex != null)
                        {
                            MidTex2 = midTex.ToTexture2D(64);
                            //rend.material.GetTexture("_midTex").ToTexture2D().WritePNG("Test/MidTex");
                            //rend.material.GetTexture("_highTex").ToTexture2D().WritePNG("Test/HighTex");
                        }
                        else
                        {
                            Log.Normal("No LowTex found");
                        }

                    }
                    else
                    {
                        Log.Normal("No Raycast material found");
                    }
                }
                else
                {
                    Log.Normal("No renderer found");
                }
            }

            InstanceUtil.SetLayerRecursively(instance, 15);

            return MidTex2;
        }


    }

}
