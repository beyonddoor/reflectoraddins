namespace Reflector.AssemblyListEx
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;
	using Reflector.CodeModel;
    using System.Diagnostics;
    using System.Collections;
    using Reflector.CodeModel.Memory;

    public sealed class AssemblyListEx : IPackage
    {
		private IWindowManager windowManager;
		private IAssemblyManager assemblyManager;
        private IConfigurationManager configurationManager;

		public void Load(IServiceProvider serviceProvider)
		{
            this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
            this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
            this.configurationManager = (IConfigurationManager)serviceProvider.GetService(typeof(IConfigurationManager));
            this.windowManager.Load += new EventHandler(windowManager_Load);
		}

        private void windowManager_Load(object sender, EventArgs e)
        {
            ProcessCommandLineArguments();
        }

        private void ProcessCommandLineArguments()
        {
            CommandLineManager commandLineManger = new CommandLineManager();

            if (commandLineManger.IsArgumentPresent("assemblyListFile"))
            {
                string fileName = commandLineManger.GetArgument("assemblyListFile");
                if (false == File.Exists(fileName))
                {
                    MessageBox.Show(String.Format("Could not find file {0}", fileName));
                    return;
                }

                AssemblyListFile file = new AssemblyListFile(fileName);
                string[] assembliesToLoad = file.Assemblies;

                if (file.RemoveExisitingAssemblies)
                {
                    // Remove all exisiting assemblies
                    for (int i = assemblyManager.Assemblies.Count - 1; i >= 0; i--)
                    {
                        assemblyManager.Unload(assemblyManager.Assemblies[i]);
                    }
                }

                foreach (string assemblyFile in assembliesToLoad)
                {
                    assemblyManager.LoadFile(assemblyFile);
                }

                if (file.AutomaticallyResolveReferences)
                {
                    ResolveAllReferences(file.SupressResolveUI);                    
                }
            }

            if (commandLineManger.IsArgumentPresent("RegisterAssemblyListFileType"))
            {
                DoRegisterFileTypeAssociation();
            }

            if (commandLineManger.IsArgumentPresent("UnregisterAssemblyListFileType"))
            {
                DoUnregisterFileTypeAssociation();
            }


        }

        private void ResolveAllReferences(bool supressResolveUI)
        {
            IConfiguration configuration = this.configurationManager["AssemblyBrowser"];
            bool autoResolve = configuration.GetProperty("AutoResolve") == "true";
            if (supressResolveUI && autoResolve == false)
            {
                configuration.SetProperty("AutoResolve", "true");
            }

            int oldAssemblyCount = 0;
            ArrayList assembliesToAdd = new ArrayList();

            while (assemblyManager.Assemblies.Count != oldAssemblyCount)
            {
                oldAssemblyCount = assemblyManager.Assemblies.Count;
                IAssemblyCollection collection = new AssemblyCollection();

                foreach (IAssembly assembly in assemblyManager.Assemblies)
                    collection.Add(assembly);

                foreach (IAssembly assembly in collection)
                {
                    foreach (IModule module in assembly.Modules)
                    {
                        foreach (IAssemblyReference assemblyReference in module.AssemblyReferences)
                        {
                            IAssembly newassembly = assemblyReference.Resolve();

                            if (assemblyManager.Assemblies.Contains(newassembly) == false)
                            {
                                assembliesToAdd.Add(newassembly);
                            }
                        }
                    }
                }

                foreach (IAssembly assemblyToAdd in assembliesToAdd)
                {
                    assemblyManager.Load(assemblyToAdd, assemblyToAdd.Location);
                }
            }

            if (supressResolveUI && autoResolve == false)
            {
                configuration.SetProperty("AutoResolve", "false");
            }

        }

        private void DoRegisterFileTypeAssociation()
        {
            Hashtable commands = new Hashtable();

            commands.Add("open", String.Format("{0} /assemblyListFile:%1", System.Reflection.Assembly.GetEntryAssembly().Location));

            FileTypeAssociation.Create(".ref", /* Extension */
                                       "assemblylist/xml", /* Content type */
                                       "Reflector Assembly List", /* Full Name */
                                       "Reflector Assembly List", /* Proper Name */
                                       String.Format("{0},0", System.Reflection.Assembly.GetEntryAssembly().Location), /* Icon path */
                                       commands);
                                       
        }

        private void DoUnregisterFileTypeAssociation()
        {
            FileTypeAssociation.Remove(".ref", /* Extension */
                                       "Reflector Assembly List"); /* Proper Name */
        }


		public void Unload()
		{
            // We don't register anything so we don't need to unload anything
		}
	}
}
