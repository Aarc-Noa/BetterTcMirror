using System;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BetterTcMirror.Behaviours;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace BetterTcMirror
{
    [BepInPlugin("org.aarcnoa.tc.bettermirror", "Better TC Mirror", "1.0.1")]
    [BepInProcess("Chronos.exe")]
    [BepInProcess("ChronosDemo.exe")]
    public class MirrorPlugin : BaseUnityPlugin
    {
        protected string[] CameraObjectPaths
        {
            get
            {
                return Paths.ProcessName switch
                {
                    "Chronos" => new [] {"OpenVRCameraRig(Clone)/Camera"}, // Tokyo Chronos Steam v1.0.4
                    "ChronosDemo" => new [] {"OpenVRCameraRig/Camera (eye)"},
                    _ => throw new InvalidOperationException($"This is not supposed to happen - process name was {Paths.ProcessName}")
                };
            }
        }
        
        private ConfigEntry<float> fieldOfView;
        private ConfigEntry<Vector3> cameraOffset;

        private WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
        
        private GameObject _currentMirror;

        protected MirrorPlugin()
        {
            // Configuration
            fieldOfView = Config.Bind("General", "FieldOfView", 70f, "The field of view of the mirror camera.");
            cameraOffset = Config.Bind("General", "Offset", Vector3.zero,
                "How much to move the camera relative to the player's head.");
            
            
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                #if DEBUG
                Logger.LogInfo(scene.name + $" loaded ({mode})");
                #endif

                if (scene.name == "002InGame" || // Entering a save file. 
                    scene.name == "000Permanent") // Starting the game.
                {
                    StartCoroutine(SetupCamera(scene));
                }
            };
            SceneManager.sceneUnloaded += (scene) =>
            {
                #if DEBUG
                Logger.LogInfo(scene.name + " unloaded");
                #endif
                
                // This scene gets unloaded on the way to the title screen
                if (scene.name == "002InGame")
                    StartCoroutine(SetupCamera(SceneManager.GetSceneByName("000Permanent")));
            };
        }

        private IEnumerator SetupCamera(Scene targetScene)
        {
            Logger.LogInfo($"Setting up camera for {targetScene.name}");
            
            yield return _waitForSecondsRealtime;

            if (_currentMirror != null) Destroy(_currentMirror);

            // Preserving the current active scene...
            var tmp = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(targetScene);

            GameObject cameraObject = null;

            foreach (var path in CameraObjectPaths)
            {
                cameraObject = GameObject.Find(path);
                if (cameraObject != null)
                    break;
            }

            if (cameraObject == null)
            {
                Logger.LogError("Could not find the expected camera object. " +
                                "Please report this issue at https://github.com/Jeremiidesu/BetterTcMirror with the " +
                                "version of the game you are using! (Store, version number)");
                yield break;
            }

            _currentMirror = Instantiate(cameraObject);
            _currentMirror.name = "BetterMirrorManager";
            var mcb = _currentMirror.AddComponent<MirrorCameraBehaviour>();
            mcb.Init(cameraObject, fieldOfView.Value, cameraOffset.Value);

            // ...and restoring it because I'd rather be too careful than not enough
            SceneManager.SetActiveScene(tmp);
        }
    }
}