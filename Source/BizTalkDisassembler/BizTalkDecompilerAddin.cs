using System;
using System.Collections.Generic;

using Reflector;
using Reflector.CodeModel;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Main Reflector Addin class.
    /// </summary>
    public class BizTalkDecompilerAddin : IPackage
    {
        private IServiceProvider svcProvider;
        private IAssemblyBrowser assemblyBrowser;

        private ICommandBarButton showBTSArtifactsButton;
        private BizTalkArtifactsList btsArtifactsControl;


        /// <summary>
        /// Gets or sets the ServiceProvider offered by Reflector.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return svcProvider;  }
            set { svcProvider = value; }
        }

        /// <summary>
        /// Gets or sets the Reflector Assembly Browser.
        /// </summary>
        private IAssemblyBrowser AssemblyBrowser
        {
            get { return assemblyBrowser; }
            set
            {
                if (assemblyBrowser != null)
                {
                    assemblyBrowser.ActiveItemChanged -= new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }

                assemblyBrowser = value;

                if (assemblyBrowser != null)
                {
                    assemblyBrowser.ActiveItemChanged += new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }
            }
        }

        /// <summary>
        /// Gets or sets the main command ("Show BizTalk Server 2006 Artifacts").
        /// </summary>
        private ICommandBarButton MainCommand
        {
            get { return showBTSArtifactsButton;  }
            set { showBTSArtifactsButton = value; }
        }

        /// <summary>
        /// Gets the control used to display the list of BizTalk artifacts.
        /// </summary>
        private BizTalkArtifactsList BizTalkArtifactsListControl
        {
            get
            {
                if (btsArtifactsControl == null)
                {
                    btsArtifactsControl = new BizTalkArtifactsList();
                }
                return btsArtifactsControl;
            }
            set
            {
                if (btsArtifactsControl != null)
                {
                    btsArtifactsControl.Dispose();
                }
                btsArtifactsControl = value;
            }
        }

        /// <summary>
        /// Load the addin.
        /// </summary>
        /// <param name="serviceProvider">Reflector service provider.</param>
        public void Load(IServiceProvider serviceProvider)
        {
            // Remember various Reflector Services
            ServiceProvider = serviceProvider;
            AssemblyBrowser = ServiceProvider.GetService(typeof(IAssemblyBrowser)) as IAssemblyBrowser;

            // Register our main window
            RegisterArtifactsWindow();
        }

        /// <summary>
        /// Unload the addin.
        /// </summary>
        public void Unload()
        {
            // Unregister our commands, if any
            UnRegisterCommands();

            // Unregister our main windosw
            UnRegisterArtifactsWindow();

            // Release all our references
            BizTalkArtifactsListControl = null;
            AssemblyBrowser = null;
            ServiceProvider = null;
        }

        /// <summary>
        /// Registers our commands with Reflector.
        /// </summary>
        private void RegisterCommands()
        {
            ICommandBarManager commandBarMgr = ServiceProvider.GetService(typeof(ICommandBarManager)) as ICommandBarManager;
            if (commandBarMgr != null)
            {
                ICommandBar defaultBrowserContextMenu = commandBarMgr.CommandBars["Browser.Assembly"];
                MainCommand = defaultBrowserContextMenu.Items.AddButton(Resources.ShowBizTalkArtifacts, new EventHandler(ShowBizTalkArtifacts_ButtonClick));
            }
        }

        /// <summary>
        /// Un registers our commands with reflector.
        /// </summary>
        private void UnRegisterCommands()
        {
            ICommandBarManager commandBarMgr = ServiceProvider.GetService(typeof(ICommandBarManager)) as ICommandBarManager;
            if (commandBarMgr != null)
            {
                ICommandBar defaultBrowserContextMenu = commandBarMgr.CommandBars["Browser.Assembly"];
                if (defaultBrowserContextMenu.Items.Contains(MainCommand))
                {
                    defaultBrowserContextMenu.Items.Remove(MainCommand);
                }
                
                MainCommand = null;
            }
        }

        /// <summary>
        /// Registers the artifacts window.
        /// </summary>
        private void RegisterArtifactsWindow()
        {
            IWindowManager windowMgr = ServiceProvider.GetService(typeof(IWindowManager)) as IWindowManager;
            if (windowMgr != null)
            {
                windowMgr.Windows.Add(Constants.BizTalkArtifactWindowID, BizTalkArtifactsListControl, Resources.BizTalkArtifactsWindow);
            }
        }

        /// <summary>
        /// Un registers the artifacts window.
        /// </summary>
        private void UnRegisterArtifactsWindow()
        {
            IWindowManager windowMgr = ServiceProvider.GetService(typeof(IWindowManager)) as IWindowManager;
            if (windowMgr != null)
            {
                windowMgr.Windows.Remove(Constants.BizTalkArtifactWindowID);
            }
        }

        /// <summary>
        /// Handles the ActievItemChanged in the Reflector AssemblyBrowser.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="e">Argument associated with this event.</param>
        private void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
            IAssembly assembly = AssemblyBrowser.ActiveItem as IAssembly;
            if (ArtifactsCache.IsBizTalkAssembly(assembly))
            {
                // The currently selected assembly is a BizTalk assembly: show our commands
                RegisterCommands();
                UpdateDisplay(assembly, false);
            }
            else
            {
                // The currently selected assembly is not a BizTalk assembly: hide our commands
                UnRegisterCommands();
                UpdateDisplay(null, false);
            }
        }

        /// <summary>
        /// Displays the BizTalk Server Artifacts Window.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="args">Arguments associated with this event.</param>
        private void ShowBizTalkArtifacts_ButtonClick(object sender, EventArgs args)
        {
            // Show the BizTalk Artifact Window
            UpdateDisplay(AssemblyBrowser.ActiveItem as IAssembly, true);
        }

        /// <summary>
        /// Update the BizTalk Artifacts window display.
        /// </summary>
        /// <param name="assembly">Assembly for which artifacts should be displayed.</param>
        /// <param name="forceVisible">true if the artifact window show be made visible, false otherwise.</param>
        /// <remarks>The assembly can be null. This means that the selected assembly is not a BizTalk assembly.</remarks>
        private void UpdateDisplay(IAssembly assembly, bool forceVisible)
        {
            IWindowManager windowMgr = ServiceProvider.GetService(typeof(IWindowManager)) as IWindowManager;
            if (windowMgr != null)
            {
                // If we are asked to force the window to appear, do it now
                if (forceVisible)
                {
                    windowMgr.Windows[Constants.BizTalkArtifactWindowID].Visible = true;
                }

                // Update the window with the new list of artifacts
                if (windowMgr.Windows[Constants.BizTalkArtifactWindowID].Visible)
                {
                    List<DecompiledArtifact> artifacts = ArtifactsCache.GetBizTalkArtifactsForAssembly(assembly);
                    BizTalkArtifactsListControl.Display(artifacts);
                }
            }
        }
    }
}
