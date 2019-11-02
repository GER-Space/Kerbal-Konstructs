//The MIT License(MIT)

//Copyright(c) 2014 sarbian

//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.




using UnityEngine;

namespace KerbalKonstructs.UI
{
    class DrawObject
    {
        private static bool transforms = false;
        private static bool colliders = true;
        private static bool bounds = false;

        internal static void DrawObjects(GameObject go)
        {


            if (transforms)
            {

                DrawTools.DrawTransform(go.transform, 0.3f);

            }

            if (colliders)
            {

                Collider[] comp = go.GetComponents<Collider>();
                for (int i = 0; i < comp.Length; i++)
                {
                    Collider baseCol = comp[i];

                    if (baseCol is BoxCollider)
                    {

                        BoxCollider box = baseCol as BoxCollider;
                        DrawTools.DrawLocalCube(box.transform, box.size, Color.yellow, box.center);

                    }

                    if (baseCol is SphereCollider)
                    {

                        SphereCollider sphere = baseCol as SphereCollider;
                        DrawTools.DrawSphere(sphere.transform.TransformPoint(sphere.center), Color.red, sphere.radius);

                    }

                    if (baseCol is CapsuleCollider)
                    {

                        CapsuleCollider caps = baseCol as CapsuleCollider;
                        Vector3 dir = new Vector3(caps.direction == 0 ? 1 : 0, caps.direction == 1 ? 1 : 0, caps.direction == 2 ? 1 : 0);
                        Vector3 top = caps.transform.TransformPoint(caps.center + caps.height * 0.5f * dir);
                        Vector3 bottom = caps.transform.TransformPoint(caps.center - caps.height * 0.5f * dir);
                        DrawTools.DrawCapsule(top, bottom, Color.green, caps.radius);

                    }

                    if (baseCol is MeshCollider)
                    {
                        continue;


                    }
                }
            }

            if (bounds)
            {


                //DrawTools.DrawBounds(go.GetRendererBounds(), XKCDColors.Pink);

                //Renderer[] renderers = go.GetComponents<Renderer>();
                //for (int i = 0; i < renderers.Length; i++)
                //{
                //    Bounds bound = renderers[i].bounds;
                //    DrawTools.DrawLocalCube(renderers[i].transform, bound.size, XKCDColors.Pink, bound.center);
                //}

                MeshFilter[] mesh = go.GetComponents<MeshFilter>();
                for (int i = 0; i < mesh.Length; i++)
                {
                    DrawTools.DrawLocalCube(mesh[i].transform, mesh[i].mesh.bounds.size, XKCDColors.Pink, mesh[i].mesh.bounds.center);
                }

            }



            int count = go.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject child = go.transform.GetChild(i).gameObject;

                if (!child.GetComponent<Part>() && child.name != "main camera pivot")
                    DrawObjects(child);
            }

        }
    }
}
