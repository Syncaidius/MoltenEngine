namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Represents a SPIR-V instruction opcode.
    /// <para>See: <see href="https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#Instructions"/></para>
    /// <para>Each instruction is made up of 32-bit unsigned integer "words".</para>
    /// <para>The first word of an instruction contains the word count in the upper 16 bits, and the opcode in the lower 16 bits.</para>
    /// </summary>
    public enum SpirvOpCode : uint
    {
        /// <summary>
        /// The SPIR-V 'OpNop' instruction opcode.
        /// <para>Opcode ID: 0</para>
        /// <para>Word Count: 1
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///     </list>
        /// </para>
        /// </summary>
        OpNop = 0,

        /// <summary>
        /// The SPIR-V 'OpUndef' instruction opcode.
        /// <para>Opcode ID: 1</para>
        /// <para>Word Count: 3
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Result Type</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Result</term>
        ///             <description>SprivResultID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpUndef = 1,

        /// <summary>
        /// The SPIR-V 'OpSourceContinued' instruction opcode.
        /// <para>Opcode ID: 2</para>
        /// <para>Word Count: 2
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Continued Source</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpSourceContinued = 2,

        /// <summary>
        /// The SPIR-V 'OpSource' instruction opcode.
        /// <para>Opcode ID: 3</para>
        /// <para>Word Count: 5
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Source Language</term>
        ///             <description>SpirvSourceLanguage</description>
        ///         </item>
        ///         <item>
        ///             <term>Version</term>
        ///             <description>SpirvVersion</description>
        ///         </item>
        ///         <item>
        ///             <term>File</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Source</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpSource = 3,

        /// <summary>
        /// The SPIR-V 'OpSourceExtension' instruction opcode.
        /// <para>Opcode ID: 4</para>
        /// <para>Word Count: 2
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Extension</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpSourceExtension = 4,

        /// <summary>
        /// The SPIR-V 'OpName' instruction opcode.
        /// <para>Opcode ID: 5</para>
        /// <para>Word Count: 3
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Target</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Name</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpName = 5,

        /// <summary>
        /// The SPIR-V 'OpMemberName' instruction opcode.
        /// <para>Opcode ID: 6</para>
        /// <para>Word Count: 4
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Type</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Member</term>
        ///             <description>SpirvLiteral<uint></description>
        ///         </item>
        ///         <item>
        ///             <term>Name</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpMemberName = 6,

        /// <summary>
        /// The SPIR-V 'OpString' instruction opcode.
        /// <para>Opcode ID: 7</para>
        /// <para>Word Count: 3
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Result</term>
        ///             <description>SprivResultID</description>
        ///         </item>
        ///         <item>
        ///             <term>Name</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpString = 7,

        /// <summary>
        /// The SPIR-V 'OpLine' instruction opcode.
        /// <para>Opcode ID: 8</para>
        /// <para>Word Count: 4
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>File</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Line</term>
        ///             <description>SpirvLiteral<uint></description>
        ///         </item>
        ///         <item>
        ///             <term>Column</term>
        ///             <description>SpirvLiteral<uint></description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpLine = 8,

        /// <summary>
        /// The SPIR-V 'OpDecorate' instruction opcode.
        /// <para>Opcode ID: 71</para>
        /// <para>Word Count: 3 + variable, based on length of 'Decoration'.
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Target</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Decoration</term>
        ///             <description>SpirvDecoration</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpDecorate = 71,

        /// <summary>
        /// The SPIR-V 'OpMemberDecorate' instruction opcode.
        /// <para>Opcode ID: 72</para>
        /// <para>Word Count: 4 + variable, based on length of 'Decoration'.
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Structure Type</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Member</term>
        ///             <description>SpirvLiteral<uint></description>
        ///         </item>
        ///         <item>
        ///             <term>Decoration</term>
        ///             <description>SpirvDecoration</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpMemberDecorate = 72,

        /// <summary>
        /// The SPIR-V 'OpDecorationGroup' instruction opcode.
        /// <para>Opcode ID: 73</para>
        /// <para>Word Count: 2
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Result</term>
        ///             <description>SprivResultID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        [Obsolete("Directly use non-group decoration instructions instead")]
        OpDecorationGroup = 73,

        /// <summary>
        /// The SPIR-V 'OpGroupDecorate' instruction opcode.
        /// <para>Opcode ID: 74</para>
        /// <para>Word Count: 2 + variable, based on length of 'Decoration Group'.
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Decoration Group</term>
        ///             <description>SpirvDecorationGroupID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        [Obsolete("Directly use non-group decoration instructions instead")]
        OpGroupDecorate = 74,

        /// <summary>
        /// The SPIR-V 'OpNoLine' instruction opcode.
        /// <para>Opcode ID: 317</para>
        /// <para>Word Count: 1
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///     </list>
        /// </para>
        /// </summary>
        OpNoLine = 317,

        /// <summary>
        /// The SPIR-V 'OpSizeOf' instruction opcode.
        /// <para>Opcode ID: 321</para>
        /// <para>Word Count: 4
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Result Type</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Result</term>
        ///             <description>SprivResultID</description>
        ///         </item>
        ///         <item>
        ///             <term>Pointer</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpSizeOf = 321,

        /// <summary>
        /// The SPIR-V 'OpModuleProcessed' instruction opcode.
        /// <para>Opcode ID: 330</para>
        /// <para>Word Count: 2
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Process</term>
        ///             <description>SpirvString</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpModuleProcessed = 330,

        /// <summary>
        /// The SPIR-V 'OpAssumeTrueKHR' instruction opcode.
        /// <para>Opcode ID: 5630</para>
        /// <para>Word Count: 2
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Condition</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpAssumeTrueKHR = 5630,

        /// <summary>
        /// The SPIR-V 'OpExpectKHR' instruction opcode.
        /// <para>Opcode ID: 5631</para>
        /// <para>Word Count: 5
        ///     <list type="number">
        ///         <listheader>
        ///             <term>Header</term>
        ///             <description>Upper 16 bits contain word count. Lower 16 bits contain opcode ID.</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Result Type</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>Result</term>
        ///             <description>SprivResultID</description>
        ///         </item>
        ///         <item>
        ///             <term>Value</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///         <item>
        ///             <term>ExpectedValue</term>
        ///             <description>SpirvID</description>
        ///         </item>
        ///     </list>
        /// </para>
        /// </summary>
        OpExpectKHR = 5631,

    }
}


