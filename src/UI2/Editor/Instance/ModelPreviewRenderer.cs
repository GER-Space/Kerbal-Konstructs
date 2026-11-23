using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Renders spinning previews of static models into small RenderTextures (one per slot),
    /// like the part icons in the KSP vessel editor.
    /// The previews are mesh-only copies of the model prefabs (no scripts, no colliders) on their
    /// own layer, seen only by this renderer's camera. They are only active while being rendered.
    /// The look of AdvancedTextures and GrassColor modules is applied to them (ModelPreviewModules).
    /// </summary>
    internal class ModelPreviewRenderer : MonoBehaviour
    {
        // KSP's "PartsList_Icons" layer: used by the vessel editor's part icons, not used in flight
        private const int previewLayer = 8;
        private const float fieldOfView = 30f;
        // camera looks down at the model at this angle
        private const float cameraPitch = 20f;
        // degrees per second
        private const float spinSpeed = 30f;

        private static readonly Color backgroundColor = new Color(0.16f, 0.18f, 0.2f, 1f);

        private Camera previewCamera;
        private Light previewLight;
        private RenderTexture[] textures;
        private Preview[] previews;
        private float angle = 0f;

        // the previews stop turning while this is false
        internal bool spinning = true;

        private class Preview
        {
            internal GameObject root;
            internal Transform pivot;
            internal float radius;
            // material copies made for this preview (modules), destroyed with it
            internal List<Material> materials = new List<Material>();
        }


        private static void DestroyPreview(Preview preview)
        {
            if (preview == null)
            {
                return;
            }
            if (preview.root != null)
            {
                Destroy(preview.root);
            }
            foreach (Material material in preview.materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            preview.materials.Clear();
        }


        /// <summary>
        /// Creates a renderer with the given number of slots and texture size (pixels).
        /// </summary>
        internal static ModelPreviewRenderer Create(int slots, int size)
        {
            GameObject rendererObject = new GameObject("KKModelPreviewRenderer");
            // far away from the scene; the previews are inactive outside of Render() anyway
            rendererObject.transform.position = new Vector3(0f, 100000f, 0f);
            ModelPreviewRenderer renderer = rendererObject.AddComponent<ModelPreviewRenderer>();
            renderer.Setup(slots, size);
            return renderer;
        }


        private void Setup(int slots, int size)
        {
            textures = new RenderTexture[slots];
            previews = new Preview[slots];
            for (int i = 0; i < slots; i++)
            {
                textures[i] = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
                textures[i].antiAliasing = 2;
                textures[i].Create();
            }

            GameObject cameraObject = new GameObject("KKModelPreviewCamera");
            cameraObject.transform.SetParent(transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.enabled = false;
            previewCamera.cullingMask = 1 << previewLayer;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = backgroundColor;
            previewCamera.fieldOfView = fieldOfView;
            previewCamera.allowHDR = false;

            GameObject lightObject = new GameObject("KKModelPreviewLight");
            lightObject.transform.SetParent(transform, false);
            // from the upper left, behind the camera
            lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            previewLight = lightObject.AddComponent<Light>();
            previewLight.type = LightType.Directional;
            previewLight.intensity = 1.1f;
            previewLight.color = Color.white;
            previewLight.cullingMask = 1 << previewLayer;
            previewLight.shadows = LightShadows.None;
        }


        internal RenderTexture GetTexture(int slot)
        {
            if (textures == null || slot < 0 || slot >= textures.Length)
            {
                return null;
            }
            return textures[slot];
        }


        /// <summary>
        /// Shows these models in the slots (in order); slots without a model are cleared.
        /// </summary>
        internal void SetModels(IList<StaticModel> models)
        {
            for (int i = 0; i < previews.Length; i++)
            {
                if (previews[i] != null)
                {
                    DestroyPreview(previews[i]);
                    previews[i] = null;
                }
                if (models != null && i < models.Count && models[i] != null)
                {
                    previews[i] = CreatePreview(models[i]);
                }
                ClearTexture(i);
            }
        }


        private void ClearTexture(int slot)
        {
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = textures[slot];
            GL.Clear(true, true, backgroundColor);
            RenderTexture.active = active;
        }


        /// <summary>
        /// A mesh-only copy of the model's prefab, centered on a pivot that spins.
        /// </summary>
        private Preview CreatePreview(StaticModel model)
        {
            GameObject prefab = GetPrefab(model);
            if (prefab == null)
            {
                return null;
            }

            GameObject root = new GameObject("KKPreview_" + model.name);
            root.layer = previewLayer;
            root.transform.SetParent(transform, false);
            GameObject pivotObject = new GameObject("Pivot");
            pivotObject.layer = previewLayer;
            pivotObject.transform.SetParent(root.transform, false);
            GameObject meshes = new GameObject("Meshes");
            meshes.layer = previewLayer;
            meshes.transform.SetParent(pivotObject.transform, false);

            Bounds bounds = new Bounds();
            bool hasBounds = false;
            Matrix4x4 toPrefab = prefab.transform.worldToLocalMatrix;

            foreach (Renderer source in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (!(source is MeshRenderer) && !(source is SkinnedMeshRenderer))
                {
                    continue;
                }
                if (!source.enabled || !IsActiveBelow(source.transform, prefab.transform))
                {
                    continue;
                }
                Mesh mesh = null;
                if (source is SkinnedMeshRenderer)
                {
                    mesh = ((SkinnedMeshRenderer)source).sharedMesh;
                }
                else
                {
                    MeshFilter filter = source.GetComponent<MeshFilter>();
                    mesh = (filter != null) ? filter.sharedMesh : null;
                }
                if (mesh == null)
                {
                    continue;
                }

                // same place relative to the prefab root
                Matrix4x4 matrix = toPrefab * source.transform.localToWorldMatrix;
                GameObject copy = new GameObject(source.name);
                copy.layer = previewLayer;
                copy.transform.SetParent(meshes.transform, false);
                copy.transform.localPosition = matrix.GetColumn(3);
                Vector3 forward = matrix.GetColumn(2);
                Vector3 up = matrix.GetColumn(1);
                if (forward.sqrMagnitude > 0f && up.sqrMagnitude > 0f)
                {
                    copy.transform.localRotation = Quaternion.LookRotation(forward, up);
                }
                copy.transform.localScale = new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);

                copy.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer renderer = copy.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = source.sharedMaterials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                // bounds in the root's space
                Bounds meshBounds = TransformBounds(mesh.bounds, matrix);
                if (hasBounds)
                {
                    bounds.Encapsulate(meshBounds);
                }
                else
                {
                    bounds = meshBounds;
                    hasBounds = true;
                }
            }

            if (!hasBounds)
            {
                Destroy(root);
                return null;
            }

            // spin around the center of the model
            meshes.transform.localPosition = -bounds.center;
            root.SetActive(false);

            Preview preview = new Preview();
            // the look of the model's AdvancedTextures / GrassColor modules
            ModelPreviewModules.Apply(model, meshes, preview.materials);
            preview.root = root;
            preview.pivot = pivotObject.transform;
            preview.radius = Mathf.Max(bounds.extents.magnitude, 0.1f);
            return preview;
        }


        private static GameObject GetPrefab(StaticModel model)
        {
            if (model.prefab != null)
            {
                return model.prefab;
            }
            if (model.hasVariants)
            {
                foreach (ModelVariant variant in model.variants.Values)
                {
                    if (variant.prefab != null)
                    {
                        return variant.prefab;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// True if the transform and its parents up to (not including) root are active.
        /// The prefabs themselves are inactive, so activeInHierarchy can't be used.
        /// </summary>
        private static bool IsActiveBelow(Transform transform, Transform root)
        {
            while (transform != null && transform != root)
            {
                if (!transform.gameObject.activeSelf)
                {
                    return false;
                }
                transform = transform.parent;
            }
            return true;
        }


        private static Bounds TransformBounds(Bounds local, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(local.center);
            Vector3 extents = local.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0, 0));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0, extents.y, 0));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0, 0, extents.z));
            Vector3 newExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, newExtents * 2f);
        }


        private void LateUpdate()
        {
            if (previews == null)
            {
                return;
            }
            if (spinning)
            {
                angle = (angle + spinSpeed * Time.unscaledDeltaTime) % 360f;
            }

            for (int i = 0; i < previews.Length; i++)
            {
                Preview preview = previews[i];
                if (preview == null || preview.root == null)
                {
                    continue;
                }
                RenderPreview(preview, textures[i]);
            }
        }


        private void RenderPreview(Preview preview, RenderTexture target)
        {
            preview.pivot.localRotation = Quaternion.Euler(0f, angle, 0f);

            // frame the whole model: distance so its bounding sphere fits the view
            float distance = preview.radius / Mathf.Sin(fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.05f;
            Quaternion look = Quaternion.Euler(cameraPitch, 180f, 0f);
            Vector3 center = preview.root.transform.position;
            previewCamera.transform.position = center - look * Vector3.forward * distance;
            previewCamera.transform.rotation = look;
            previewCamera.nearClipPlane = Mathf.Max(0.05f, distance - preview.radius * 1.5f);
            previewCamera.farClipPlane = distance + preview.radius * 1.5f;
            previewLight.transform.rotation = look * Quaternion.Euler(30f, -35f, 0f);

            // only this preview is active, and only while it is rendered
            preview.root.SetActive(true);
            previewCamera.targetTexture = target;
            try
            {
                previewCamera.Render();
            }
            catch (Exception e)
            {
                Log.Normal("ModelPreviewRenderer: render failed: " + e.Message);
            }
            previewCamera.targetTexture = null;
            preview.root.SetActive(false);
        }


        private void OnDestroy()
        {
            if (previews != null)
            {
                foreach (Preview preview in previews)
                {
                    DestroyPreview(preview);
                }
            }
            if (textures != null)
            {
                foreach (RenderTexture texture in textures)
                {
                    if (texture != null)
                    {
                        texture.Release();
                        Destroy(texture);
                    }
                }
            }
        }
    }
}
