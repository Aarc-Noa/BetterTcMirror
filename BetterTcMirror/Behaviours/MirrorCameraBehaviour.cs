using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.XR;

namespace BetterTcMirror.Behaviours
{
    public class MirrorCameraBehaviour : MonoBehaviour
    {
        // References to TC GameObjects and Components
        private Camera _mainCamera;
        
        private IEnumerable<Canvas> _canvases;
        
        // Modded in stuff
        private Vector3 _cameraOffset;

        private GameObject _cameraTarget;
        private Camera _cameraClone;
        private RenderTexture _renderTexture;
        private ScreenCameraBehaviour _screenCamera; // Needed to draw the RenderTexture onscreen. Why? Beats me.
        
        private GameObject _canvasTarget;
        
        private int _prevScreenWidth;
        private int _prevScreenHeight;

        public virtual void Init(GameObject mainCameraObject, float fov = 70f, Vector3 cameraOffset = default)
        {
            // Hide native mirror
            XRSettings.showDeviceView = false;
            
            _cameraOffset = cameraOffset;
            
            // Destroy unneeded children
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // Prepare canvases
            _canvases = new[]
            {
                GameObject.Find("SceneFadeCanvas").GetComponent<Canvas>(),
                GameObject.Find("ImportantCanvas").GetComponent<Canvas>()
            };

            foreach (var canvas in _canvases)
            {
                // Workaround to allow canvases to be visible in both cameras.
                // The increased scale allows the canvas to extend past the boundaries of the VR viewport to fit with
                // the (optional) increased FOV of the mirror. 
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.localRotation = Quaternion.Euler(0, 0, 0);
                canvas.transform.localScale = new Vector3(0.3f, 0.3f, 0.001f); // Original: {0.001, 0.001, 0.001}
            }
            
            // Prepare canvas target
            _canvasTarget = new GameObject("CanvasTarget");
            _canvasTarget.transform.parent = mainCameraObject.transform;
            _canvasTarget.transform.localPosition = new Vector3(0, 0, 1); // Matches planeDistance of 1
            _canvasTarget.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _canvasTarget.transform.localScale = Vector3.one;

            // Prepare camera target
            _cameraTarget = new GameObject("CameraTarget");
            _cameraTarget.transform.parent = mainCameraObject.transform;
            _cameraTarget.transform.localPosition = _cameraOffset;
            _cameraTarget.transform.localRotation = Quaternion.Euler(0, 0, 0);
            _cameraTarget.transform.localScale = Vector3.one;

            _mainCamera = mainCameraObject.GetComponent<Camera>();
            
            // Preparing the camera clone
            _cameraClone = gameObject.GetComponent<Camera>();
            _cameraClone.cullingMask = _mainCamera.cullingMask;
            _cameraClone.stereoTargetEye = StereoTargetEyeMask.None;
            _cameraClone.fieldOfView = fov;

            gameObject.SetActive(true);
            
            _screenCamera = new GameObject("Screen Camera").AddComponent<ScreenCameraBehaviour>();
            CreateScreenRenderTexture();
        }

        private void LateUpdate()
        {
            // Update mirror texture if screen resolution changed.
            if (Screen.width != _prevScreenWidth || Screen.height != _prevScreenHeight)
            {
                CreateScreenRenderTexture();
            }
            
            // Move canvas to target
            foreach (var canvas in _canvases)
            {
                var canvasTransform = canvas.transform;
                canvasTransform.position = _canvasTarget.transform.position;
                canvasTransform.rotation = _canvasTarget.transform.rotation;
            }

            // As the camera isn't recreated often, background color changes need to be propagated
            _cameraClone.backgroundColor = _mainCamera.backgroundColor;
            // Some scenes fade to black using something else than the canvas. Need to forward culling mask to match
            _cameraClone.cullingMask = _mainCamera.cullingMask;
            
            // Update camera position
            transform.position = _cameraTarget.transform.position;
            transform.rotation = _cameraTarget.transform.rotation;
        }

        private void CreateScreenRenderTexture()
        {
            // Hold reference to previous texture... 
            var prev = _renderTexture;
            
            _prevScreenWidth = Screen.width;
            _prevScreenHeight = Screen.height;
            
            _renderTexture = new RenderTexture(_prevScreenWidth, _prevScreenHeight, 24);
            _renderTexture.antiAliasing = 4;
            
            _cameraClone.targetTexture = _renderTexture;
            _screenCamera.SetRenderTexture(_renderTexture);
            
            // ...and dispose of the previous texture
            Destroy(prev);
        }
        
        private void OnDestroy()
        {
            // Avoid lingering objects.
            Destroy(_screenCamera.gameObject);
            Destroy(_canvasTarget);
            Destroy(_cameraTarget);
        }
    }
}