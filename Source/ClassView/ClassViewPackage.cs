namespace Reflector
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Windows.Forms;
	using Reflector.CodeModel;

	internal class ClassViewPackage : IPackage
	{
		private ClassViewWindow window = null;
		private IWindowManager windowManager = null;
		private ICommandBarManager commandBarManager = null;
		private ICommandBarSeparator separator = null;
		private ICommandBarButton button = null;
		
		public void Load(IServiceProvider serviceProvider)
		{	
			IAssemblyBrowser assemblyBrowser = (IAssemblyBrowser) serviceProvider.GetService(typeof(IAssemblyBrowser));
			ILanguageManager languageManager = (ILanguageManager) serviceProvider.GetService(typeof(ILanguageManager));
			ITranslatorManager translatorManager = (ITranslatorManager) serviceProvider.GetService(typeof(ITranslatorManager));
			IVisibilityConfiguration visibilityConfiguration = (IVisibilityConfiguration) serviceProvider.GetService(typeof(IVisibilityConfiguration));

			this.window = new ClassViewWindow(assemblyBrowser, languageManager, translatorManager, visibilityConfiguration);

			this.windowManager = (IWindowManager) serviceProvider.GetService(typeof(IWindowManager));
			this.windowManager.Windows.Add("ClassViewWindow", this.window, "Class View");

			this.commandBarManager = (ICommandBarManager) serviceProvider.GetService(typeof(ICommandBarManager));

			this.separator = commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.button = commandBarManager.CommandBars["Tools"].Items.AddButton("Class &View", new EventHandler(this.Button_Click));
		}

		public void Unload()
		{
			this.windowManager.Windows.Remove("ClassViewWindow");

			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.button);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.separator);
		}
		
		private void Button_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["ClassViewWindow"].Visible = true;
		}
	}
}
