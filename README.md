# Molten Engine
Molten is a engine project I've been working on in my spare time as a means to eventually produce my own games without using a third party engine or graphics framework such as [MonoGame](http://monogame.net) or [Unity3D](https://unity3d.com/).

In the past I've used MonoGame, XNA and Unity3D to produce a game but never felt quite satisfied with the fact that I didn't actually create the framework or foundation myself, as well as feeling frustrated at times by the limitations of third-party frameworks.

Despite sounding like a classic case of reinventing the wheel, I'm treating it more as a learning experience in the short to mid-term, with the end-goal of producing at least one game with it in the long-term.

# Why Open-Source?
I've previously attempted a game engine back in 2015 using SharpDX and while it inevitably ended in failure, I learned a great deal in terms of game engine architecture and the way a GPU works under the hood, as well as things like multi-threading, tasking and other high-performance system design.

This is my second attempt at such an engine, so I would like to extend the learning experience further by it opening up to everyone. More importantly, it allows others to learn from my code (and mistakes) and perhaps even use the engine as a base for their own games and engine variants.

# Current State
The engine is pretty rough around the edges at this point in time. There is no audio system, AI, cross-platform, collision/physics, deferred rendering or post-processing. Simply put, it's a giant ball of molten rock which is probably no use to anyone yet.

Here's a list of completed or work-in-progress (WIP) features:
  * A basic renderer with:
    * [WIP] Entity-component-system (ECS)
    * Sprite/primitive batcher
    * [WIP] Material system
	* Support for popular texture formats - DDS, PNG and JEG
	* Compressed texture support
	* [WIP] Sprite font system
  * Content manager
  * [WIP] Modular abstraction layer - remove/replace parts of the engine without touching the core
  * Complete set of math types. The majority are imported from [SharpDX](https://github.com/sharpdx/SharpDX) with a many additions and changes.
  * [WIP] Mouse, keyboard and game-pad input
  * Example projects
  
  
# Long-term Goals
In no particular order:
  * Develop core engine (renderer, audio, input, I/O, etc) to a usable state.
  * Add linux support later in time via Mono. This also means supporting OpenGL via [OpenTK](https://opentk.github.io/) or [OpenGL.NET](https://github.com/luca-piccioni/OpenGL.Net)
  * DX12/Vulkan support - DX12 will likely be first due to already being available through SharpDX
  * Mobile support via Xamarin or whatever the best cross-platform framework is at that time this is put in to action.
  * An example game or two to demonstrate how things work

# Licensing
MIT - Basically, do what you want with it. Fork it, chop it up and consume it for dinner. You get the idea!