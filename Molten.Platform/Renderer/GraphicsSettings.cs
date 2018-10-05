using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [DataContract]
    public class GraphicsSettings : SettingBank
    {
        /// <summary>The name of the default DX11 renderer library.</summary>
        public const string RENDERER_DX11 = "Molten.DX11.dll; Molten.Graphics.RendererDX11";

        /// <summary>The name of the default DX12 renderer library.</summary>
        public const string RENDERER_DX12 = "Molten.DX12.dll; Molten.Graphics.RendererDX12";

        /// <summary>
        /// The name of the default OpenGL renderer library.
        /// </summary>
        public const string RENDERER_OPENGL = "Molten.OpenGL.dll; Molten.Graphics.RendererOpenGL";

        /// <summary>
        /// The minimum tessellation factor for batched lights.
        /// </summary>
        public const float MIN_LIGHT_TESS_FACTOR = 4.0f;

        /// <summary>
        /// The maximum tessellation factor for batched lights.
        /// </summary>
        public const float MAX_LIGHT_TESS_FACTOR = 8.0f;

        public const float LIGHT_TESS_FACTOR_RANGE = MAX_LIGHT_TESS_FACTOR - MIN_LIGHT_TESS_FACTOR;

        public GraphicsSettings()
        {
            GraphicsAdapterID = AddSetting<int>("adapter_id", -1);
            DisplayOutputIds = AddSettingList<int>("display_id");
            RendererLibrary = AddSetting<string>("renderer", RENDERER_DX11);
            VSync = AddSetting<bool>("vsync", true);
            MSAA = AddSetting<AntiAliasMode>("msaa", 0);
            BackBufferSize = AddSetting<int>("back_buffer_size", 1);
            EnableDebugLayer = AddSetting<bool>("renderer_debug");
        }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayAdapter"/> that was last used.</summary>
        [DataMember]
        public SettingValue<int> GraphicsAdapterID { get; private set; }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayOutput"/> that was last used on the last used <see cref="GraphicsAdapterID"/>.</summary>
        [DataMember]
        public SettingValueList<int> DisplayOutputIds { get; private set; }

        /// <summary>Gets the renderer library to use with the engine. This can be changed to any library containing one or more implementations of <see cref="MoltenRenderer"/>.</summary>
        [DataMember]
        public SettingValue<string> RendererLibrary { get; private set; }

        /// <summary>Gets or sets wether Vsync is enabled.</summary>
        [DataMember]
        public SettingValue<bool> VSync { get; private set; }

        /// <summary>Gets or sets the multi-sampled anti-aliasing (MSAA) level.</summary>
        [DataMember]
        public SettingValue<AntiAliasMode> MSAA { get; private set; }

        /// <summary>Gets or sets the number of back-buffer surfaces. More tend to increase performance, but also consumes more video memory.</summary>
        public SettingValue<int> BackBufferSize { get; private set; }

        /// <summary>Gets or sets whether to enable a renderer's debug layer, if available.</summary>
        [DataMember]
        public SettingValue<bool> EnableDebugLayer { get; private set; }
    }
}
