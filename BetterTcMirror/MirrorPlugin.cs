using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using BetterTcMirror.Behaviours;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace BetterTcMirror
{
    [BepInPlugin("org.aarcnoa.tc.bettermirror", "Better TC Mirror", "1.0.0")]
    [BepInProcess("Chronos.exe")]
    [BepInProcess("ChronosDemo.exe")]
    public class MirrorPlugin : BaseUnityPlugin
    {
        private ConfigEntry<float> fieldOfView;
        private ConfigEntry<Vector3> cameraOffset;

        private WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
        
        private GameObject _currentMirror;

        private MirrorPlugin()
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

            var camObj = GameObject.Find("OpenVRCameraRig(Clone)/Camera");

            _currentMirror = Instantiate(camObj);
            _currentMirror.name = "BetterMirrorManager";
            var mcb = _currentMirror.AddComponent<MirrorCameraBehaviour>();
            mcb.Init(camObj, fieldOfView.Value, cameraOffset.Value);

            // ...and restoring it because I'd rather be too careful than not enough
            SceneManager.SetActiveScene(tmp);
        }
    }
}