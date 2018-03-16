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

        public GraphicsSettings()
        {
            GraphicsAdapterID = AddSetting<int>("adapter_id", -1);
            DisplayOutputIds = AddSettingList<int>("display_id");
            RendererLibrary = AddSetting<string>("renderer", RENDERER_DX11);
            VSync = AddSetting<bool>("vsync", true);
            MSAA = AddSetting<int>("msaa", 0);
            BackBufferSize = AddSetting<int>("back_buffer_size", 1);
            EnableDebugLayer = AddSetting<bool>("renderer_debug");
        }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayAdapter"/> that was last used.</summary>
        [DataMember]
        public SettingValue<int> GraphicsAdapterID { get; private set; }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayOutput"/> that was last used on the last used <see cref="GraphicsAdapterID"/>.</summary>
        [DataMember]
        public SettingValueList<int> DisplayOutputIds { get; private set; }

        /// <summary>Gets the renderer library to use with the engine. This can be changed to any library containing one or more implementations of <see cref="IRenderer"/>.</summary>
        [DataMember]
        public SettingValue<string> RendererLibrary { get; private set; }

        /// <summary>Gets or sets wether Vsync is enabled.</summary>
        public SettingValue<bool> VSync { get; private set; }

        /// <summary>Gets or sets the multi-sampled anti-aliasing (MSAA) level. A level of 0 will disable MSAA.</summary>
        public SettingValue<int> MSAA { get; private set; }

        /// <summary>Gets or sets the number of back-buffer surfaces. More tend to increase performance, but also consumes more video memory.</summary>
        public SettingValue<int> BackBufferSize { get; private set; }

        /// <summary>Gets or sets whether to enable a renderer's debug layer, if available.</summary>
        public SettingValue<bool> EnableDebugLayer { get; private set; }
    }
}
