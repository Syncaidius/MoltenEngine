# Molten Engine
[Documentation (WIP)](https://syncaidius.github.io/MoltenEngine/docs/Molten.html)

Molten is a engine project I'm working on in my spare time as a means to eventually produce my own games without using a third party engine or framework such as [Unity3D](https://unity3d.com/) or [MonoGame](http://monogame.net).

In the past I've used MonoGame, XNA and Unity3D to produce a game but never felt quite satisfied with the fact that I didn't actually create the foundations myself, as well as feeling frustrated at times by the limitations of third-party frameworks.

While parts of this project will inevitably be a classic case of re-inventing the wheel, I'm teating it as a chance to gain more experience in the area of game engine development. 

# Why Open-Source?
I've previously attempted a game engine back in 2015 using SharpDX and while it inevitably ended in failure, I learned a great deal in terms of game engine architecture and the way a GPU works under the hood, as well as things like multi-threading, tasking and other high-performance system design elements.

This is my second attempt at such an engine, so being able to share the code for others to learn from and perhaps even contribute to, seems like the right thing to do. Hopefully you'll find something useful around here!

# Current State
The engine is rough around the edges at this point in time. There is no audio system, AI, cross-platform, collision/physics, deferred rendering or post-processing. Simply put, it's a giant ball of molten rock which is probably no use for producing a game yet, but it may be a good starting point for something.

Here's a list of completed or work-in-progress (WIP) features:
  * A basic renderer with:
    * [WIP] Entity-component-system (ECS)
    * Sprite/primitive batcher
    * [WIP] Material system
	* Compressed texture support
	* [WIP] Sprite font system
  * Content manager
  * [WIP] Modular abstraction layer - remove/replace parts of the engine without touching the core
  * Complete set of math types. The majority are imported from [SharpDX](https://github.com/sharpdx/SharpDX) with many additions and changes.
  * [WIP] Mouse, keyboard and game-pad input
  * Example projects
  
  
# Long-term Goals
In no particular order:
  * Develop core engine (renderer, audio, input, I/O, etc) to a usable state.
  * Add linux support later in time via Mono. This also means supporting OpenGL via [OpenTK](https://opentk.github.io/)
  * DX12/Vulkan support - DX12 will likely be first due to already being available through SharpDX
  * Mobile support via Xamarin or whatever the best cross-platform framework is at that time this is put in to action.
  * An example game or two to demonstrate how things work

# Third-Party Libraries
A special thanks to each of these great libraries, without which Molten wouldn't exist in it's current form:
  * [SharpDX](https://github.com/sharpdx/SharpDX) - DirectX bindings for C#
  * [Magick.NET](https://github.com/dlemstra/Magick.NET) - Extensive image library for .NET
  * [JSON.NET](https://www.newtonsoft.com/json) - JSON serialization
  * [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net) - OpenGL bindings for C#

# Licensing
MIT - Basically, it means you can do what you want with Molten. Fork it, chop it up and consume it for dinner. You get the idea!