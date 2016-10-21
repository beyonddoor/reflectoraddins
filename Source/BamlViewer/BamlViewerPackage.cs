namespace Reflector.BamlViewer
{
	using System;
	using System.Windows.Forms;

	public class BamlViewerPackage : IPackage
    {
		private IWindowManager windowManager = null;
		private ICommandBarManager commandBarManager = null;
		private ICommandBarSeparator separator = null;
		private ICommandBarButton button = null;

		public void Load(IServiceProvider serviceProvider)
		{
			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));

			BamlViewerWindow window = new BamlViewerWindow(serviceProvider);
			this.windowManager.Windows.Add("BamlViewerWindow", window, "BAML Viewer");

			this.separator = commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.button = commandBarManager.CommandBars["Tools"].Items.AddButton("&BAML Viewer", new EventHandler(this.Button_Click));
		}

		public void Unload()
		{
			this.windowManager.Windows.Remove("BamlViewerWindow");

			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.button);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.separator);
		}

		private void Button_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["BamlViewerWindow"].Visible = true;
		}
    }
}
