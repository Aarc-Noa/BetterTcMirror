# BetterTcMirror

A simple mod to provide a better mirror view for the VR visual novel Tokyo Chronos.

## Using

### Requirements

- Tokyo Chronos (Steam, v1.0.4)
- BepInEx 5.4.11

### Installation

1. Install BepInEx to Tokyo Chronos
2. Move `BetterTcMirror.dll` to `TOKYO CHRONOS\BepInEx\plugins\BetterTcMirror\BetterTcMirror.dll`
   (Subfolders are not required but recommended.)
3. Start the game!

### Configuration

A configuration file will be created at `TOKYO CHRONOS\BepInEx\config\com.aarcnoa.tc.bettermirror.cfg`

Available options:

- `FieldOfView` - vertical field of view of the mirror camera.
    - Type: float
    - Default value: 70°
    - Recommended range: 50-90°
    
- `CameraOffset` - how much to offset the camera for better view of close objects
    - Type: Vector3
    - Default value: `{ "x": 0.0, "y": 0.0, "z": 0.0 }`
    - Recommended: offsetting the Z axis up to -.15 
  
### Known issues

#### Recentering while looking around 90° to the side shows behind the dark fade

This is linked to the way I currently update the `Canvas`es that handle the fades. The render mode MyDearest used
is `ScreenSpaceCamera`, which is the right choice given that the canvas gets "stuck" to the camera. However, this
has additional side effect of only being rendered to one camera, meaning that if I were to stick to one `Canvas`
using `ScreenSpaceCamera`, either the VR player or the mirror would see the fades, but not both. This is also an 
issue while regularly moving the head around, but the canvas has been scaled up to compensate for regular head
movement.

To work around this issue, I set the canvas to render in `WorldSpace`, then move the canvas in front of the camera
on each `Update`. Unfortunately, it seems that there's a delay of one (1) singular frame in doing so, and therefore
what's behind the fade screen becomes visible for a single frame.

## Build

### Requirements

- Tokyo Chronos (Steam, v1.0.4) with BepInEx 5.4.11
- .NET Framework SDK that can target 3.5
- Build Tools for Visual Studio

### Instructions

Next to `BetterTcMirror\BetterTcMirror.csproj`, create `BetterTcMirror.csproj.user` and paste this content:

```xml
<Project>
  <PropertyGroup>
    <TCDir>C:\Program Files (x86)\Steam\steamapps\common\TOKYO CHRONOS</TCDir>
  </PropertyGroup>
</Project>
```

Replace the path in <TCDir> with the path to your game installation.

## License

The source code in this repository is provided under the MIT License. See details in [LICENSE](LICENSE).

Additionally, this project reuses code from: 

- [CameraPlus](https://github.com/xyonico/CameraPlus) by **xyonico**, used under the MIT license