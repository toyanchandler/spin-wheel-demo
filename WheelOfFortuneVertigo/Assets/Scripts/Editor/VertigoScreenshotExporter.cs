#if UNITY_EDITOR
using System.IO;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoScreenshotExporter
    {
        private const string OutputRoot = "/Users/bengisucay/ComputerUse/vertigo-case/screenshots/landscape";

        private static readonly CaptureSize[] Sizes =
        {
            new CaptureSize("16x9_1920x1080", 1920, 1080),
            new CaptureSize("20x9_2400x1080", 2400, 1080),
            new CaptureSize("4x3_2048x1536", 2048, 1536)
        };

        [MenuItem("Vertigo Case/Export Landscape Screenshots")]
        public static void ExportLandscapeScreenshots()
        {
            Directory.CreateDirectory(OutputRoot);
            EditorSceneManager.OpenScene(VertigoWheelPaths.ScenePath);

            Camera camera = Object.FindObjectOfType<Camera>();
            WheelRuntimeCompositionRoot runtime = Object.FindObjectOfType<WheelRuntimeCompositionRoot>();
            WheelGameState state = Object.FindObjectOfType<WheelGameState>();
            WheelStatePublisher publisher = Object.FindObjectOfType<WheelStatePublisher>();
            RectTransform wheelRoot = GameObject.Find("ui_wheel_animator_root").GetComponent<RectTransform>();
            GameObject debugCanvas = GameObject.Find("debug_canvas");

            bool debugCanvasWasActive = debugCanvas != null && debugCanvas.activeSelf;
            if (debugCanvas != null)
            {
                debugCanvas.SetActive(false);
            }

            runtime.StartRuntime();

            CaptureState("01_start", camera, runtime, state, publisher, wheelRoot, PrepareStart);
            CaptureState("02_spinning", camera, runtime, state, publisher, wheelRoot, PrepareSpinning);
            CaptureState("03_reward_active", camera, runtime, state, publisher, wheelRoot, PrepareReward);
            CaptureState("04_bomb_fail", camera, runtime, state, publisher, wheelRoot, PrepareBomb);

            runtime.StopRuntime();
            if (debugCanvas != null)
            {
                debugCanvas.SetActive(debugCanvasWasActive);
            }

            AssetDatabase.Refresh();
            Debug.Log("Landscape screenshots exported to " + OutputRoot);
        }

        private static void CaptureState(
            string stateName,
            Camera camera,
            WheelRuntimeCompositionRoot runtime,
            WheelGameState state,
            WheelStatePublisher publisher,
            RectTransform wheelRoot,
            System.Action<WheelRuntimeCompositionRoot, WheelGameState, WheelStatePublisher, RectTransform> prepare)
        {
            prepare(runtime, state, publisher, wheelRoot);
            DOTween.CompleteAll(false);
            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < Sizes.Length; i++)
            {
                CaptureSize size = Sizes[i];
                string path = Path.Combine(OutputRoot, size.Label + "_" + stateName + ".png");
                Capture(camera, size.Width, size.Height, path);
            }
        }

        private static void PrepareStart(WheelRuntimeCompositionRoot runtime, WheelGameState state, WheelStatePublisher publisher, RectTransform wheelRoot)
        {
            wheelRoot.localEulerAngles = Vector3.zero;
            state.Restart();
            publisher.PublishAll();
        }

        private static void PrepareSpinning(WheelRuntimeCompositionRoot runtime, WheelGameState state, WheelStatePublisher publisher, RectTransform wheelRoot)
        {
            state.Restart();
            publisher.PublishAll();
            state.BeginSpin();
            wheelRoot.localEulerAngles = new Vector3(0f, 0f, 148f);
            publisher.PublishHud();
        }

        private static void PrepareReward(WheelRuntimeCompositionRoot runtime, WheelGameState state, WheelStatePublisher publisher, RectTransform wheelRoot)
        {
            state.Restart();
            int index = WheelSliceQueries.FindFirst(state.Slices, state.SliceCount, false);
            index = index < 0 ? 0 : index;
            wheelRoot.localEulerAngles = Vector3.zero;
            state.Resolve(new WheelSpinResult(index, state.Slices[index]));
            publisher.PublishAll();
            publisher.PublishOutcome(state.LastResult, true);
        }

        private static void PrepareBomb(WheelRuntimeCompositionRoot runtime, WheelGameState state, WheelStatePublisher publisher, RectTransform wheelRoot)
        {
            state.Restart();
            int index = WheelSliceQueries.FindFirst(state.Slices, state.SliceCount, true);
            index = index < 0 ? 0 : index;
            wheelRoot.localEulerAngles = Vector3.zero;
            state.Resolve(new WheelSpinResult(index, state.Slices[index]));
            publisher.PublishHud();
            publisher.PublishOutcome(state.LastResult, true);
        }

        private static void ConfigureCanvases(Camera camera, int width, int height)
        {
            const float pixelsPerUnit = 100f;
            camera.orthographic = true;
            camera.orthographicSize = height / (pixelsPerUnit * 2f);
            camera.aspect = width / (float)height;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;

            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].renderMode = RenderMode.WorldSpace;
                canvases[i].worldCamera = camera;
                canvases[i].overrideSorting = true;
                RectTransform rectTransform = canvases[i].GetComponent<RectTransform>();
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.position = Vector3.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one / pixelsPerUnit;
            }
        }

        private static void Capture(Camera camera, int width, int height, string path)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousCameraTarget = camera.targetTexture;

            ConfigureCanvases(camera, width, height);
            Canvas.ForceUpdateCanvases();
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply(false);
            File.WriteAllBytes(path, texture.EncodeToPNG());

            camera.targetTexture = previousCameraTarget;
            RenderTexture.active = previousActive;
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(texture);
        }

        private readonly struct CaptureSize
        {
            public readonly string Label;
            public readonly int Width;
            public readonly int Height;

            public CaptureSize(string label, int width, int height)
            {
                Label = label;
                Width = width;
                Height = height;
            }
        }
    }
}
#endif
