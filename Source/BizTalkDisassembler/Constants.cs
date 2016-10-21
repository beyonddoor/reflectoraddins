using System;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Various constants.
    /// </summary>
    internal static class Constants
    {
        //==========================================================================================
        //                                    Reflector IDs
        //==========================================================================================

        /// <summary>
        /// ID of our BizTalk Artifact window as given to Reflector.
        /// </summary>
        public const string BizTalkArtifactWindowID = "BizTalkArtifactsWindow";


        //==========================================================================================
        //                              BizTalk Assemblies / Types
        //==========================================================================================

        /// <summary>
        /// Fully qualified name of Microsoft.XALNGs.BaseTypes as shipped in BizTalk Server 2006.
        /// </summary>
        public const string  XLANGBaseTypesFullyQualifiedAssemblyName = "Microsoft.XLANGs.BaseTypes, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        /// <summary>
        /// Fully qualified name of Microsoft.XLANGs.BizTalk.Engine as shipped in BizTalk Server 2006.
        /// </summary>
        public const string XLANGEngineFullyQualifiedAssemblyName = "Microsoft.XLANGs.BizTalk.Engine, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        /// <summary>
        /// Fully qualified name of Microsoft.BizTalk.Pipeline as shipped in BizTalk Server 2006.
        /// </summary>
        public const string BizTalkPipelineFullyQualifiedAssemblyName  = "Microsoft.BizTalk.Pipeline, Version=3.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        /// <summary>
        /// Name of attribute used to mark BizTalk Server 2006 assemblies.
        /// </summary>
        public const string BizTalkAssemblyAttribute = "BizTalkAssemblyAttribute";



        //==========================================================================================
        //                           .NET TypeName <--> BizTalk Decompiler mapping
        //==========================================================================================

        /// <summary>
        /// Enumeration of all BizTalk artifacts we can extract.
        /// </summary>
        public enum BizTalkArtifactType
        {
            None = 0,
            Map = 1,
            Schema = 2,
            Orchestration = 3,
            SendPipeline = 4,
            ReceivePipeline = 5
        }

        /// <summary>
        /// Maps a .NET typename to a DecompiledArtifact class.
        /// </summary>
        internal struct RecognizedBizTalkArtifact
        {
            public string ExpectedType;
            public Type DecompiledType;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="typeName">Name of the base class.</param>
            /// <param name="factoryType">DecompiledArtifact falvor to map the given type to.</param>
            public RecognizedBizTalkArtifact(string typeName, Type factoryType)
            {
                ExpectedType = typeName;
                DecompiledType = factoryType;
            }
        }

        /// <summary>
        /// List of all BizTalk types we can decompile associated with the type of the class which handles decompilation.
        /// </summary>
        public static RecognizedBizTalkArtifact[] RecognizedArtifacts = new RecognizedBizTalkArtifact[] {
                new RecognizedBizTalkArtifact("TransformBase, " + XLANGBaseTypesFullyQualifiedAssemblyName, typeof(DecompiledBTSMap)),
                new RecognizedBizTalkArtifact("SchemaBase, " + XLANGBaseTypesFullyQualifiedAssemblyName, typeof(DecompiledBTSSchema)),
                new RecognizedBizTalkArtifact("ReceivePipeline, " + BizTalkPipelineFullyQualifiedAssemblyName, typeof(DecompiledReceivePipeline)),
                new RecognizedBizTalkArtifact("SendPipeline, " + BizTalkPipelineFullyQualifiedAssemblyName, typeof(DecompiledSendPipeline)),
                new RecognizedBizTalkArtifact("BTXService, " + XLANGEngineFullyQualifiedAssemblyName, typeof(DecompiledOrchestration))
        };
    }
}
