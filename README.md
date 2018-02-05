# Molten Engine
Molten is a engine project I've been working on in my spare time as a means to eventually produce my own games without using a third party engine or framework.

In the past I've used MonoGame, XNA and Unity3D to produce a game but never felt quite satisfied with the fact that I didn't actually create the framework or foundation myself, as well as feeling frustrated at times by the limitations of third-party frameworks.

Despite it sounding like a classic case of reinventing the wheel, I intend to treat it more as a learning experience in the short to mid-term, with high ambitions of actually producing at least one game with it in the long-term. Whether those ambitions are reached remains to be seen!

# Why Open-Source?
I've previously attempted a game engine back in 2015 using SharpDX and while it inevitably ended in failure, I learned a great deal in terms of game engine architecture and the way a GPU works under the hood, as well as things like multi-threading, tasking and other high-performance system design.

This is my second attempt at such an engine, so I would like to extend the learning experience further by it opening up to everyone. More importantly, it allows others to learn from my mistakes, code and other bits and pieces!

# Current State
Right now the engine is extremely rough around the edges. Here's a list of the current features:
  * An extremely basic renderer with:
    * Entity-component-system (ECS) scene system
    * Sprite-batcher
    * Material system
    * Abstraction layer for swapping out the renderer without touching the engine itself
  * Content manager
  * Complete set of math types. The majority are imported from [SharpDX](https://github.com/sharpdx/SharpDX) with a few additions and changes.
  * Crude test app (written in WPF)
  

Currently, there is no audio system. No input (WIP), no collision/physics, no deferred rendering or post-processing.
Simply put, it's a giant ball of molten rock which is no use to anyone yet. ;)
  
  
# Long-term Goals
  1. Develop Molten engine and at least one game, running on Windows 7/8/10 via [SharpDX](http://sharpdx.org)
  2. Add linux support later in time via Mono. This also means supporting OpenGL via [OpenTK](https://opentk.github.io/) or [OpenGL4Net](https://sourceforge.net/projects/ogl4net/)
  3. DX12/Vulkan support
  4. Depending how far the engine has progressed, mobile support may come much later via Xamarin or whatever the best cross-platform framework is at that time.

Any commerially viable games I produce with this engine will obviously not be open-source, but I'll definitely be making some example games to go alongside it, as well as documentation at some point.

# Licensing
MIT - Basically, you can do what you want with it. Fork it, add a knife, consume it for dinner (*just kidding*). You get the idea!
