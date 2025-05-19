using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EUI = FZTools.EditorUtils.UI;
using ELayout = FZTools.EditorUtils.Layout;
using System.IO;
using System;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Experimental.Rendering;
using System.Runtime.Serialization.Formatters.Binary;

namespace FZTools
{
    public class FZPreviewRenderer
    {
        private Scene scene;
        public Scene Scene => scene;
        public Camera camera;
        public Camera Camera => camera;
        public Light light;
        public Light Light => light;
        public RenderTexture renderTexture;
        public RenderTexture RenderTexture => renderTexture;

        public FZPreviewRenderer(GameObject previewObject)
        {
            scene = EditorSceneManager.NewPreviewScene();
            previewObject.SetActive(true);
            SceneManager.MoveGameObjectToScene(previewObject, scene);
            CreatePreviewCamera();
            CreatePreviewLight();
        }

        public void SetCameraPosition(Vector3 position)
        {
            camera.transform.position = position;
        }

        public void SetCameraRotation(Quaternion rotation)
        {
            camera.transform.rotation = rotation;
        }

        public void CreatePreviewCamera()
        {
            var gameObject = new GameObject("Camera", typeof(Camera));
            gameObject.transform.position = new Vector3(0, 1, 1);
            gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            camera = gameObject.GetComponent<Camera>();
            camera.fieldOfView = 10;
            camera.nearClipPlane = .3f;
            camera.farClipPlane = 1000;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.gray;
            camera.cameraType = CameraType.Preview;
            camera.forceIntoRenderTexture = true;
            camera.scene = scene;
            camera.enabled = false;
        }

        private void CreatePreviewLight()
        {
            var existLights = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<Light>());
            if (existLights.Any())
            {
                return;
            }
            var gameObject = new GameObject("Light", typeof(Light));
            gameObject.transform.rotation = Quaternion.Euler(50, -30, 0);
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            light = gameObject.GetComponent<Light>();
            light.color = Color.white;
            light.type = LightType.Directional;
        }

        public T GetPreviewObjectComponent<T>()
        {
            return Scene.GetRootGameObjects().FirstOrDefault(rgo => rgo.GetComponent<T>() != null).GetComponent<T>();
        }

        public void RenderPreview(int width = 300, int height = 300)
        {
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(width, height, 32, GraphicsFormat.R8G8B8A8_UNorm);
            }
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = null;
        }

        public void EndPreview()
        {
            camera.targetTexture = null;

            if (renderTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(renderTexture);
                renderTexture = null;
            }

            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}