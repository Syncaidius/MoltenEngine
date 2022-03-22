using System.Runtime.Serialization;
using Molten.Graphics;

namespace Molten
{
    [DataContract]
    public class GraphicsSettings : SettingBank
    {
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
            VSync = AddSetting<bool>("vsync", true);
            MSAA = AddSetting<AntiAliasLevel>("msaa", 0);
            BackBufferSize = AddSetting<uint>("back_buffer_size", 1);
            EnableDebugLayer = AddSetting<bool>("renderer_debug");
        }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayAdapter"/> that was last used.</summary>
        [DataMember]
        public SettingValue<int> GraphicsAdapterID { get; }

        /// <summary>Gets or sets the UID of the <see cref="IDisplayOutput"/> that was last used on the last used <see cref="GraphicsAdapterID"/>.</summary>
        [DataMember]
        public SettingValueList<int> DisplayOutputIds { get; }

        /// <summary>Gets or sets wether Vsync is enabled.</summary>
        [DataMember]
        public SettingValue<bool> VSync { get; }

        /// <summary>Gets or sets the multi-sampled anti-aliasing (MSAA) level.</summary>
        [DataMember]
        public SettingValue<AntiAliasLevel> MSAA { get; }

        /// <summary>Gets or sets the number of back-buffer surfaces. More tend to increase performance, but also consumes more video memory.</summary>
        public SettingValue<uint> BackBufferSize { get; }

        /// <summary>Gets or sets whether to enable a renderer's debug layer, if available.</summary>
        [DataMember]
        public SettingValue<bool> EnableDebugLayer { get; }
    }
}
