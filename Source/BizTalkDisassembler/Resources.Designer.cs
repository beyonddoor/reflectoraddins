﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BizTalkDisassembler {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BizTalkDisassembler.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BizTalk Server 2006 Artifacts.
        /// </summary>
        internal static string BizTalkArtifactsWindow {
            get {
                return ResourceManager.GetString("BizTalkArtifactsWindow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Full source.
        /// </summary>
        internal static string DecompileAsSource {
            get {
                return ResourceManager.GetString("DecompileAsSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XSLT only.
        /// </summary>
        internal static string DecompileAsXSLT {
            get {
                return ResourceManager.GetString("DecompileAsXSLT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error exporting {0} &quot;{1}&quot;: {2}..
        /// </summary>
        internal static string ErrorExportedArtifact {
            get {
                return ResourceManager.GetString("ErrorExportedArtifact", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disassembly canceled by user..
        /// </summary>
        internal static string ExportCanceled {
            get {
                return ResourceManager.GetString("ExportCanceled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disassembly done..
        /// </summary>
        internal static string ExportDone {
            get {
                return ResourceManager.GetString("ExportDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exported {0} &quot;{1}&quot; to {2}..
        /// </summary>
        internal static string ExportedArtifact {
            get {
                return ResourceManager.GetString("ExportedArtifact", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting disassembly..
        /// </summary>
        internal static string ExportStarted {
            get {
                return ResourceManager.GetString("ExportStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select the directory where BizTalk Server 2006 Artifacts will be disassembled..
        /// </summary>
        internal static string SelectExportDestinationPath {
            get {
                return ResourceManager.GetString("SelectExportDestinationPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BizTalk Server 2006 Artifacts.
        /// </summary>
        internal static string ShowBizTalkArtifacts {
            get {
                return ResourceManager.GetString("ShowBizTalkArtifacts", resourceCulture);
            }
        }
    }
}