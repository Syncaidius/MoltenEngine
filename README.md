![Molten Engine](Images/logo_light_transparent.png)
[![Build Status](https://dev.azure.com/jyarwood/MoltenEngine/_apis/build/status/MoltenEngine-.NET%20Desktop-CI)](https://dev.azure.com/jyarwood/MoltenEngine/_build/latest?definitionId=2)  [![Build Android](https://github.com/Syncaidius/MoltenEngine/actions/workflows/build-android.yml/badge.svg)](https://github.com/Syncaidius/MoltenEngine/actions/workflows/build-android.yml)  [![Build Windows](https://github.com/Syncaidius/MoltenEngine/actions/workflows/build-windows.yml/badge.svg)](https://github.com/Syncaidius/MoltenEngine/actions/workflows/build-windows.yml)  
[![GitHub](https://img.shields.io/github/license/Syncaidius/MoltenEngine)](LICENSE)  [![GitHub last commit](https://img.shields.io/github/last-commit/Syncaidius/MoltenEngine)](https://github.com/Syncaidius/MoltenEngine/commits/)

Molten started as spare-time engine project, which I eventually intend to use to develop my own games without relying on other engines. Hopefully you'll find a use for it too!

While parts of this project will inevitably be a classic case of re-inventing the wheel, I'm treating it as a chance to gain more experience in the area of game engine development. 

# Current Status
See the following table for the per-platform support and status of each feature.

✔️ Functional/Complete\
🚧 Work in progres\
Blank - Not Started\
🚫 Not-applicable

| Feature                    | Windows    | Android  | Linux  | Mac  |
| :---                       | :-------:  | :-----:  | :---:  | :-:  |
| Renderer - OpenGL          |            | 🚫       |        |      |
| Renderer - OpenES          |            |          | 🚫     | 🚫   |
| Renderer - Vulkan          |            |          |        |      |
| Renderer - DirectX 11      | 🚧         | 🚫       | 🚫    | 🚫   |
| Renderer - DirectX 12      |            | 🚫       | 🚫    | 🚫   |
| Audio - OpenAL             | 🚧         |          |        |      |
| Audio - XAudio 2           |            | 🚫       | 🚫    | 🚫   |
| Keyboard support           | ✔️          |          |        |      |
| Mouse support              | ✔️          |          |        |      |
| Touch support              |            | 🚧       |        |      |
| Game pad support           | ✔️         |          |        |      |
| Entity component system    | ✔️         | ✔️       | ✔️     | ✔️    |
| Example projects           | 🚧         |          |        |      |
| Networking                 |            |          |        |      |
| 2D Physics engine          |            |          |        |      |
| 3D Physics engine          |            |          |        |      |
| Google Analytics           |            |          |        |      |
| Google AdMob               |            |          |        |      |
| Google Firebase            |            |          |        |      |
| Tool - Content editor      |            |          |        |      |
| Tool - Scene editor        |            |          |        |      |
| Project templates          |            |          |        |      |


# Renderer Feature Support
See the following table for the per-renderer support and status of each feature.

✔️ Functional/Complete\
🚧 Work in progres\
Blank - Not Started\
🚫 Not-supported

| Feature                | DX11       | DX12    | OpenGL  | OpenGL ES  | Vulkan |
| :---                   | :-------:  | :-----: | :---:   | :-:        | :-:    |
| Deferrend rendering    | 🚧         |         |        |             |       |
| Geometry Shaders       | ✔️         |         |        |             |       |
| Tessellation           | 🚧         |         |        |             |       |
| Hull & Domain Shaders  | ✔️         |         |        |             |       |
| Compute Shaders        | 🚧         |         |        |             |       |
| 2D Spite batching      | ✔️         |         |        |             |       |
| 2D Primitive batching  | ✔️         |         |        |             |       |
| Compressed textures    | ✔️         |         |        |             |       |
| [Hardware instancing](https://en.wikipedia.org/wiki/Geometry_instancing)    |            |         |        |             |       |
| [Occlusion culling](https://en.wikipedia.org/wiki/Hidden-surface_determination#Occlusion_culling)      |            |         |        |             |       |
| [Frustrum culling](https://en.wikipedia.org/wiki/Hidden-surface_determination#Viewing-frustum_culling)       |            |         |        |             |       |
| [Portal culling](https://docs.panda3d.org/1.10/python/programming/render-attributes/occlusion-culling/portal-culling)         |            |         |        |             |       |
| [Level of Detail (LoD)](https://en.wikipedia.org/wiki/Level_of_detail_(computer_graphics))  |            |         |        |             |       |
| HDR support            |            |         |        |             |       |
| 2D Texture arrays      | ✔️         |         |        |             |       |
| 3D Texture arrays      | ✔️         |         |        |             |       |
| 3D/volume textures     | ✔️         |         |        |             |       |
| static skyboxes        | ✔️         |         |        |             |       |
| real-time skyboxes     |            |         |        |             |       |
| multi-window support   | ✔️         |         |        | 🚫          |       |
| Render into WinForms   | ✔️         |         |        | 🚫          | 🚫    |
| Render into WPF        |            |         |        | 🚫          | 🚫    |
| Render into UWP        |            |         | 🚫    | 🚫          | 🚫    |
|[Render into MAUI](https://docs.microsoft.com/en-us/dotnet/maui/what-is-maui) |            |         |       |             |       |
| Render into Android UI | 🚫         | 🚫      |       |             |       |
| Raytracing             | 🚫         |         | 🚫    | 🚫          |       |
| HLSL Shader Compiler   | 🚧		  | 🚫      | 🚫    | 🚫          | 🚫   |
| GLSL Shader Compiler   |  		  |         |        |             |      |
| SPIR-V Shader Compiler |  		  |         |        |             |      |

# Android
Molten has recently been upgraded to .NET 6. To build for Android you will need to install the Android workloads by running  
```dotnet workload install android``` in command prompt or powershell.

# Sponsors
A massive thank you to some of our sponsors:
 * [IceReaper](https://github.com/IceReaper)


# Third-Party Libraries
A special thanks to each of these great libraries, without which Molten wouldn't exist in it's current form:
  * [Silk.NET](https://github.com/dotnet/Silk.NET) - C# Bindings for DirectX 11/12, OpenGL, Vulkan, Assimp, OpenCL, OpenAL and OpenXR
  * [SharpDX](https://github.com/sharpdx/SharpDX) - Source for some Molten.Math types
  * [Magick.NET](https://github.com/dlemstra/Magick.NET) - Extensive image library for .NET
  * [JSON.NET](https://www.newtonsoft.com/json) - JSON serialization

# Licensing
[MIT](LICENSE) - You can do what you want with Molten. Fork it, chop it up and consume it for dinner. You get the idea!
