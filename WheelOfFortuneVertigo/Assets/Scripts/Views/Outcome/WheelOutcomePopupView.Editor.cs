#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed partial class WheelOutcomePopupView
    {
        private Coroutine _rewardBurstCaptureRoutine;
        private static int _rewardBurstCaptureIndex;

        public void RefreshResponsiveLayoutForCapture()
        {
        }

        private void CaptureRewardBurstDebug(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_rewardBurstCaptureRoutine != null)
            {
                StopCoroutine(_rewardBurstCaptureRoutine);
            }

            _rewardBurstCaptureRoutine = StartCoroutine(CaptureRewardBurstDebugRoutine(binding, snapshot));
        }

        private IEnumerator CaptureRewardBurstDebugRoutine(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            yield return new WaitForSecondsRealtime(0.14f);
            yield return new WaitForEndOfFrame();

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/UIParticleBurstCaptures"));
            Directory.CreateDirectory(folder);
            int captureId = ++_rewardBurstCaptureIndex;
            string captureLabel = captureId < 10 ? string.Concat("0", captureId) : string.Concat(captureId);
            string prefix = Path.Combine(folder, string.Concat("burst_", captureLabel, "_", snapshot.RewardId));
            string screenPath = string.Concat(prefix, "_game.png");
            string renderTexturePath = string.Concat(prefix, "_render_texture.png");
            string revealPath = string.Concat(prefix, "_reveal_hold_game.png");
            string lingerPath = string.Concat(prefix, "_linger_game.png");

            UnityEngine.ScreenCapture.CaptureScreenshot(screenPath);
            CaptureRenderTexture(binding.RewardBurstCamera, renderTexturePath);

            RectTransform displayRect = binding.RewardBurstDisplay != null ? binding.RewardBurstDisplay.rectTransform : null;
            RectTransform iconRect = binding.IconImage != null ? binding.IconImage.rectTransform : null;
            Vector2 displayPosition = displayRect != null ? displayRect.anchoredPosition : Vector2.zero;
            Vector2 displaySize = displayRect != null ? displayRect.sizeDelta : Vector2.zero;
            Vector2 iconPosition = iconRect != null ? iconRect.anchoredPosition : Vector2.zero;
            Debug.Log(
                string.Concat(
                    "UIParticle burst capture saved: ",
                    screenPath,
                    " | ",
                    renderTexturePath,
                    " | icon=",
                    iconPosition,
                    " display=",
                    displayPosition,
                    " size=",
                    displaySize));

            yield return new WaitForSecondsRealtime(0.96f);
            yield return new WaitForEndOfFrame();
            UnityEngine.ScreenCapture.CaptureScreenshot(revealPath);

            yield return new WaitForSecondsRealtime(1.08f);
            yield return new WaitForEndOfFrame();
            UnityEngine.ScreenCapture.CaptureScreenshot(lingerPath);
            _rewardBurstCaptureRoutine = null;
        }

        private static void CaptureRenderTexture(Camera camera, string path)
        {
            if (camera == null || camera.targetTexture == null)
            {
                return;
            }

            RenderTexture renderTexture = camera.targetTexture;
            RenderTexture active = RenderTexture.active;
            camera.Render();
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply(false);
            File.WriteAllBytes(path, UnityEngine.ImageConversion.EncodeToPNG(texture));
            RenderTexture.active = active;
        }
    }
}
#endif
