# Molten Engine
Molten is a engine project I've been working on in my spare time as a means to eventually produce my own games without using a third party engine or graphics framework such as [MonoGame](http://monogame.net) or [Unity3D](https://unity3d.com/).

In the past I've used MonoGame, XNA and Unity3D to produce a game but never felt quite satisfied with the fact that I didn't actually create the framework or foundation myself, as well as feeling frustrated at times by the limitations of third-party frameworks.

Despite it sounding like a classic case of reinventing the wheel, I intend to treat it more as a learning experience in the short to mid-term, with high ambitions of actually producing at least one game with it in the long-term. Whether those ambitions are reached remains to be seen!

# Why Open-Source?
I've previously attempted a game engine back in 2015 using SharpDX and while it inevitably ended in failure, I learned a great deal in terms of game engine architecture and the way a GPU works under the hood, as well as things like multi-threading, tasking and other high-performance system design.

This is my second attempt at such an engine, so I would like to extend the learning experience further by it opening up to everyone. More importantly, it allows others to learn from my code (and mistakes) and perhaps even use the engine as a base for their own games and engine variants.

# Current State
Right now the engine is extremely rough around the edges. Here's a list of the current features:
  * A basic renderer with:
    * [WIP] Entity-component-system (ECS)
    * Sprite-batcher
    * [WIP] Material system
    * Abstraction layer for swapping out renderer without touching the engine itself
	* Support for texture formats (DDS, PNG, JPEG, etc)
	* Compressed texture support
	* [WIP] Sprite font system
  * Content manager
  * Complete set of math types. The majority are imported from [SharpDX](https://github.com/sharpdx/SharpDX) with a few additions and changes.
  * [WIP] Mouse, keyboard and game-pad input
  * Samples
  
There is no audio system, AI, cross-platform, collision/physics, deferred rendering or post-processing.
Simply put, it's a giant ball of molten rock which is no use to anyone yet. The whole thing is a work-in-progress. ;)
  
  
# Long-term Goals
  1. Develop Molten engine and at least one game, running on Windows 7/8/10 via [SharpDX](http://sharpdx.org)
  2. Add linux support later in time via Mono. This also means supporting OpenGL via [OpenTK](https://opentk.github.io/) or [OpenGL.NET](https://github.com/luca-piccioni/OpenGL.Net)
  3. DX12/Vulkan support
  4. Depending how far the engine has progressed, mobile support may come much later via Xamarin or whatever the best cross-platform framework is at that time.

Any commerially viable games I produce with this engine will obviously not be open-source, but I'll definitely be making some example games to go alongside it, as well as documentation at some point.

# Licensing
MIT - Basically, you can do what you want with it. Fork it, add a knife, consume it for dinner (*just kidding*). You get the idea!
