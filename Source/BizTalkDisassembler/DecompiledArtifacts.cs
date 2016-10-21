using System;
using System.Collections.Generic;

using Reflector.CodeModel;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Base class for all decompiled artifacts.
    /// </summary>
    internal abstract class DecompiledArtifact
    {
        private string declaringNS;
        private string name;
        private string artifactValue;

        /// <summary>
        /// Gets or sets the namespace of the artifact.
        /// </summary>
        public string Namespace
        {
                    get { return declaringNS;  }
            private set { declaringNS = value; }
        }

        /// <summary>
        /// Gets or sets the name of the artifact.
        /// </summary>
        public string Name
        {
                    get { return name;  }
            private set { name = value; }
        }

        /// <summary>
        /// Gets or sets the artifacts's value (as XML).
        /// </summary>
        public string ArtifactValue
        {
                      get { return artifactValue;  }
            protected set { artifactValue = value; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public abstract Constants.BizTalkArtifactType ArtifactKind
        {
            get;
        }

        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected abstract string FieldName
        {
            get;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to decompile into a BizTalk artifact.</param>
        public DecompiledArtifact(ITypeDeclaration typeDeclaration)
        {
            Namespace = typeDeclaration.Namespace;
            Name = typeDeclaration.Name;

            ArtifactValue = ExtractArtifact(typeDeclaration);
        }

        /// <summary>
        /// Extract the initializer for the given TypeDeclaration.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to extarct the initializer from.</param>
        /// <returns>Value of the initializer as a string.</returns>
        protected virtual string ExtractArtifact(ITypeDeclaration typeDeclaration)
        {
            string value = null;

            // Locate the field for this artifact and extract its initializer
            string expectedFieldName = FieldName;
            foreach (IFieldDeclaration fieldDeclaration in typeDeclaration.Fields)
            {
                // BizTalk 2006 compiles textual representations as private const string fields
                if (fieldDeclaration.Visibility == FieldVisibility.Private)
                {
                    // Loacte the field holding the textual representation of the artifact
                    if (String.CompareOrdinal(expectedFieldName, fieldDeclaration.Name) == 0)
                    {
                        ILiteralExpression litteralExpression = fieldDeclaration.Initializer as ILiteralExpression;
                        if (litteralExpression != null)
                        {
                            value = litteralExpression.Value as string;
                            break;
                        }
                    }
                }
            }

            return value;
        }
    }

    /// <summary>
    /// A decompiled BizTalk schema.
    /// </summary>
    internal class DecompiledBTSSchema : DecompiledArtifact
    {
        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected override string FieldName
        {
            get { return "_strSchema"; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public override Constants.BizTalkArtifactType ArtifactKind
        {
            get { return Constants.BizTalkArtifactType.Schema; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to use when decompiling.</param>
        public DecompiledBTSSchema(ITypeDeclaration typeDeclaration)
            : base(typeDeclaration)
        {
        }
    }

    /// <summary>
    /// A decompiled BizTalk map.
    /// </summary>
    internal class DecompiledBTSMap : DecompiledArtifact
    {
        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected override string FieldName
        {
            get { return "_strMap"; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public override Constants.BizTalkArtifactType ArtifactKind
        {
            get { return Constants.BizTalkArtifactType.Map; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to use when decompiling.</param>
        public DecompiledBTSMap(ITypeDeclaration typeDeclaration)
            : base(typeDeclaration)
        {

        }
    }

    /// <summary>
    /// A decompiled BizTalk orchestration.
    /// </summary>
    internal class DecompiledOrchestration : DecompiledArtifact
    {
        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected override string FieldName
        {
            get { return "_symODXML"; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public override Constants.BizTalkArtifactType ArtifactKind
        {
            get { return Constants.BizTalkArtifactType.Orchestration; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to use when decompiling.</param>
        public DecompiledOrchestration(ITypeDeclaration typeDeclaration)
            : base(typeDeclaration)
        {
            // Patch up the artifact: for some reason the XLANG compiler adds a carioage return at the beginning of ODX XML
            if (!String.IsNullOrEmpty(ArtifactValue))
            {
                ArtifactValue = ArtifactValue.Trim();
            }
        }
    }

    /// <summary>
    /// A decompiled BizTalk send pipeline.
    /// </summary>
    internal class DecompiledSendPipeline : DecompiledArtifact
    {
        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected override string FieldName
        {
            get { return "_strPipeline"; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public override Constants.BizTalkArtifactType ArtifactKind
        {
            get { return Constants.BizTalkArtifactType.SendPipeline; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to use when decompiling.</param>
        public DecompiledSendPipeline(ITypeDeclaration typeDeclaration)
            : base(typeDeclaration)
        {

        }
    }

    /// <summary>
    /// A decompiled BizTalk receive pipeline.
    /// </summary>
    internal class DecompiledReceivePipeline : DecompiledArtifact
    {
        /// <summary>
        /// Gets the field name that should be used during reflection to extract the artifact's value.
        /// </summary>
        protected override string FieldName
        {
            get { return "_strPipeline"; }
        }

        /// <summary>
        /// Gets the type of artifact.
        /// </summary>
        public override Constants.BizTalkArtifactType ArtifactKind
        {
            get { return Constants.BizTalkArtifactType.ReceivePipeline; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="typeDeclaration">TypeDeclaration to use when decompiling.</param>
        public DecompiledReceivePipeline(ITypeDeclaration typeDeclaration)
            : base(typeDeclaration)
        {

        }
    }
}
