// Reused almost entirely as is from
// https://github.com/xyonico/CameraPlus/blob/master/CameraPlus/ScreenCameraBehaviour.cs
// under the MIT License

using UnityEngine;

namespace BetterTcMirror.Behaviours
{
    public class ScreenCameraBehaviour : MonoBehaviour
    {
        private Camera _cam;
        private RenderTexture _renderTexture;
		
        public void SetRenderTexture(RenderTexture renderTexture)
        {
            _renderTexture = renderTexture;
        }

        private void Awake()
        {
            _cam = gameObject.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.Nothing;
            _cam.cullingMask = 57143;
            _cam.depth = -1000;
            _cam.stereoTargetEye = StereoTargetEyeMask.None;
        }
		
        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_renderTexture == null) return;
            Graphics.Blit(_renderTexture, dest);
        }
    }
}