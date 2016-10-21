namespace Reflector.CodeMetrics
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;
	using Reflector.CodeModel;

	public sealed class CodeMetricPackage : IPackage
    {
		private IWindowManager windowManager;
		private IAssemblyManager assemblyManager;
		private IAssemblyCache assemblyCache;
		private ICommandBarManager commandBarManager;
		private ICommandBarSeparator separator;
		private ICommandBarButton button;

		public void Load(IServiceProvider serviceProvider)
		{
			CodeMetricWindow codeMetricWindow = new CodeMetricWindow(serviceProvider);

			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.windowManager.Load += new EventHandler(this.WindowManager_Load);
			this.windowManager.Windows.Add("CodeMetricWindow", codeMetricWindow, "Code Metrics");

			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
			this.assemblyCache = (IAssemblyCache)serviceProvider.GetService(typeof(IAssemblyCache));

			this.commandBarManager = (ICommandBarManager) serviceProvider.GetService(typeof(ICommandBarManager));
			this.separator = this.commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.button = this.commandBarManager.CommandBars["Tools"].Items.AddButton("&Code Metrics", new EventHandler(this.Button_Click), Keys.Control | Keys.E);
		}

		public void Unload()
		{
			this.windowManager.Windows.Remove("CodeMetricsWindow");
			this.windowManager.Load -= new EventHandler(this.WindowManager_Load);

			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.separator);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.button);
		}

		private void Button_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["CodeMetricWindow"].Visible = true;
		}

		private void WindowManager_Load(object sender, EventArgs e)
		{
			CommandLineProcessor commandLineProcessor = new CommandLineProcessor(this.windowManager, this.assemblyManager, this.assemblyCache);
			if (commandLineProcessor.CommandLineMode)
			{
				commandLineProcessor.Run();
				this.windowManager.Close();
			}
		}
	}
}
