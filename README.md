# Molten Engine
[![Build Status](https://dev.azure.com/jyarwood/MoltenEngine/_apis/build/status/MoltenEngine-.NET%20Desktop-CI)](https://dev.azure.com/jyarwood/MoltenEngine/_build/latest?definitionId=2)  

Molten is a engine project I'm working on in my spare time as a means to eventually produce my own games without using a third party engine or framework such as [Unity3D](https://unity3d.com/) or [MonoGame](http://monogame.net).

In the past I've used MonoGame, XNA and Unity3D to produce a game but never felt quite satisfied with the fact that I didn't actually create the foundations myself, as well as feeling frustrated at times by the limitations of third-party frameworks.

While parts of this project will inevitably be a classic case of re-inventing the wheel, I'm teating it as a chance to gain more experience in the area of game engine development. 

# Why Open-Source?
I've previously attempted a game engine back in 2015 using SharpDX and while it inevitably ended in failure, I learned a great deal in terms of game engine architecture and the way a GPU works under the hood, as well as other high-performance elements, such as multi-threading.

This is my second attempt at such an engine, so being able to share the code for others to learn from and perhaps even contribute to, seems like the right thing to do. Hopefully you'll find something useful around here!

# Current Status
See the following table for the per-platform support and status of each feature.

✔️ Functional/Complete\
🚧 Work in progres\
Blank - Not Started\
🚫 Not-applicable

| Feature                | Windows    | Android  | Linux  | Mac  |
| :---                   | :-------:  | :-----:  | :---:  | :-:  |
| Renderer - OpenGL      | 🚧         | 🚫      | 🚧     | 🚧  |
| Renderer - OpenES      |            |         | 🚫     | 🚫  |
| Renderer - Vulkan      |            |          |        |      |
| Renderer - DirectX 11  | 🚧        | 🚫       | 🚫    | 🚫   |
| Renderer - DirectX 12  |            | 🚫       | 🚫    | 🚫   |
| Audio - OpenAL         |            |          |        |      |
| Audio - XAudio 2       |            | 🚫       | 🚫    | 🚫   |
| Keyboard support       | ✔️          |          |        |      |
| Mouse support          | ✔️          |          |        |      |
| Touch support          |            | ✔️       |        |      |
| Game pad support       | ✔️         |          |        |      |
| Entity component system| ✔️         | ✔️       | ✔️     | ✔️    |
| Example projects       | ✔️         |          |        |      |
| Networking             | 🚧         |          |        |      |
| 2D Physics engine      |            |          |        |      |
| 3D Physics engine      |            |          |        |      |
| Google Analytics       |            |          |        |      |
| Google AdMob           |            |          |        |      |
| Google Firebase        |            |          |        |      |
| Tool - Content editor  |            |          |        |      |


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
| Tessellation           | ✔️         |         |        |             |       |
| Hull & Domain Shaders  | ✔️         |         |        |             |       |
| Compute Shaders        | ✔️         |         |        |             |       |
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
| Render into WinForms   | ✔️         |         | 🚫    | 🚫          | 🚫    |
| Render into WPF        |            |         | 🚫    | 🚫          | 🚫    |
| Render into UWP        |            |         | 🚫    | 🚫          | 🚫    |
|[Render into MAUI](https://docs.microsoft.com/en-us/dotnet/maui/what-is-maui)|            |         |       |             |       |
| Render into Android UI | 🚫         | 🚫      |       | 🚫          | 🚫   |
| Raytracing             | 🚫         |         | 🚫    | 🚫          |       |


# Third-Party Libraries
A special thanks to each of these great libraries, without which Molten wouldn't exist in it's current form:
  * [SharpDX](https://github.com/sharpdx/SharpDX) - DirectX bindings for C#
  * [Magick.NET](https://github.com/dlemstra/Magick.NET) - Extensive image library for .NET
  * [JSON.NET](https://www.newtonsoft.com/json) - JSON serialization
  * [OpenTK](https://opentk.net/) - OpenGL, OpenAL and input bindings for C#

# Licensing
[MIT](LICENSE) - You can do what you want with Molten. Fork it, chop it up and consume it for dinner. You get the idea!
