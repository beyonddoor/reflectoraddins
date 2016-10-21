namespace Reflector.ComLoader
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Reflector;
	using Reflector.CodeModel;

    public sealed class ComLoaderPackage : IPackage
    {
		private ICommandBarManager commandBarManager = null;
		private ICommandBarButton openTypeLibraryButton = null;
		private IAssemblyManager assemblyManager = null;
		private IAssemblyBrowser assemblyBrowser = null;

		public void Load(IServiceProvider serviceProvider)
		{
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));
			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.openTypeLibraryButton = this.commandBarManager.CommandBars["File"].Items.InsertButton(2, "Open &Type Library...", new EventHandler(this.OpenTypeLibraryButton_Click), Keys.T | Keys.Control);
		}

		public void Unload()
		{
			this.commandBarManager.CommandBars["File"].Items.Remove(this.openTypeLibraryButton);
		}

		private void OpenTypeLibraryButton_Click(object sender, EventArgs e)
		{
			TypeLibraryDialog dialog = new TypeLibraryDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if (File.Exists(Environment.ExpandEnvironmentVariables(dialog.AssemblyLocation)))
				{
					this.assemblyBrowser.ActiveItem = this.assemblyManager.LoadFile(dialog.AssemblyLocation);
				}
			}
		}
	}
}
