using System;
using System.Collections.Generic;

using Reflector.CodeModel;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Extract information from assemblies and maintains a cache for future use. 
    /// </summary>
    internal static class ArtifactsCache
    {
        /// <summary>
        /// Tests is the given Assembly is a BizTalk assembly or not.
        /// </summary>
        /// <param name="assembly">Assembly to test.</param>
        /// <returns>true if the assembly is a BizTalk assembly, false otherwise.</returns>
        public static bool IsBizTalkAssembly(IAssembly assembly)
        {
            bool btsAssembly = false;
            if (assembly != null)
            {
                // Analyse the assembly
                btsAssembly = TestForBizTalkAssembly(assembly);
            }
            return btsAssembly;
        }

        /// <summary>
        /// Gets the list of BizTalk artifacts for the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to get the list of artifacts from.</param>
        /// <returns>null if the assembly is not a BizTalk assembly or an list of decompiled artifacts.</returns>
        public static List<DecompiledArtifact> GetBizTalkArtifactsForAssembly(IAssembly assembly)
        {
            List<DecompiledArtifact> artifacts = null;

            // Test for BizTalk assembly
            if (IsBizTalkAssembly(assembly))
            {
                // Compute the list of BizTalk assemblies
                artifacts = EnumerateAssembly(assembly);
            }

            return artifacts;
        }

        /// <summary>
        /// Enumerates all types in the given assembly hunting for BizTalk types.
        /// </summary>
        /// <param name="assembly">Assembly to enumerate.</param>
        /// <returns>List of decompiled BizTalk artifacts.</returns>
        private static List<DecompiledArtifact> EnumerateAssembly(IAssembly assembly)
        {
            List<DecompiledArtifact> artifacts = new List<DecompiledArtifact>();

            // Look at all types declared in this assembly and extract the BizTalk related ones
            if ((assembly != null) && (assembly.Modules != null))
            {
                foreach (IModule module in assembly.Modules)
                {
                    if (module.Types != null)
                    {
                        foreach (ITypeDeclaration typeDeclaration in module.Types)
                        {
                            // BizTalk artifacts are in sealed, non abstract classes
                            if (!typeDeclaration.Interface && typeDeclaration.Sealed && !typeDeclaration.Abstract)
                            {
                                // Decompile the artifact
                                DecompiledArtifact artifact = DecompileArtifact(typeDeclaration);
                                if (artifact != null)
                                {
                                    artifacts.Add(artifact);
                                } 
                            }
                        }
                    }
                }
            }

            return artifacts;
        }

        /// <summary>
        /// Tests is the given Assembly is a BizTalk assembly or not.
        /// </summary>
        /// <param name="assembly">Assembly to test.</param>
        /// <returns>true if this is a BizTalk assembly, false otherwise.</returns>
        private static bool TestForBizTalkAssembly(IAssembly assembly)
        {
            bool isBTSAssembly = false;
            if (assembly != null)
            {
                // A BizTalk assembly is a Library
                if (assembly.Type == AssemblyType.Library)
                {
                    // A BizTalk assembly has the "BizTalkAssemblyAttribute" set
                    foreach (ICustomAttribute attribute in assembly.Attributes)
                    {
                        // Get the constructor MethodReference for this attribute
                        IMethodReference ctorReference = attribute.Constructor as IMethodReference;
                        if (ctorReference != null)
                        {
                            // Get the declaring type for the constructor
                            ITypeReference ctorDeclaringType = ctorReference.DeclaringType as ITypeReference;
                            if (ctorDeclaringType != null)
                            {
                                isBTSAssembly = (String.CompareOrdinal(ctorDeclaringType.Owner.ToString(), Constants.XLANGBaseTypesFullyQualifiedAssemblyName) == 0) &&
                                                (String.CompareOrdinal(Constants.BizTalkAssemblyAttribute, ctorDeclaringType.ToString()) == 0);

                                // If we found the attribute we expect, we can declare it is a BizTalk assembly
                                if (isBTSAssembly)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return isBTSAssembly;
        }

        /// <summary>
        /// Decompiles a BizTalk type knowing its Reflector TypeDeclaration.
        /// </summary>
        /// <param name="typeDeclaration">Type declaration to decompile.</param>
        /// <returns>Decompiled Artifact.</returns>
        private static DecompiledArtifact DecompileArtifact(ITypeDeclaration typeDeclaration)
        {
            DecompiledArtifact artifact = null;

            // Build the fully qualified name of the base class for this type
            ITypeReference baseType = typeDeclaration.BaseType as ITypeReference;
            string fullyQualifiedTypeName = baseType.Name + ", " + baseType.Owner.ToString();

            // Locate the class which will handle this artifact
            foreach (Constants.RecognizedBizTalkArtifact recognizedArtifact in Constants.RecognizedArtifacts)
            {
                // If we found a matching handler, create a new instance of the handler and initialize it
                if (String.CompareOrdinal(recognizedArtifact.ExpectedType, fullyQualifiedTypeName) == 0)
                {
                    // Extract the XML representation of the artifact
                    artifact = Activator.CreateInstance(recognizedArtifact.DecompiledType, new object[] { typeDeclaration }) as DecompiledArtifact;
                    break;
                }
            }

            return artifact;
        }
    }
}
