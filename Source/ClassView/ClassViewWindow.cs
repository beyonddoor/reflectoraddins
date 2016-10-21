namespace Reflector
{
	using System;
	using System.Collections;	
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using Reflector.CodeModel;

  	internal class ClassViewWindow : RichTextBox
	{
		private IAssemblyBrowser assemblyBrowser;
		private ILanguageManager languageManager;
		private ITranslatorManager translatorManager;
		private IVisibilityConfiguration visibilityConfiguration;

		public ClassViewWindow(IAssemblyBrowser assemblyBrowser, ILanguageManager languageManager, ITranslatorManager translatorManager, IVisibilityConfiguration visibilityConfiguration)
		{
			this.Dock = DockStyle.Fill;
			this.Multiline = true;
			this.WordWrap = false;
			this.ReadOnly = true;
			this.Font = new Font("Courier New", 9f);
			this.ScrollBars = RichTextBoxScrollBars.Both;
			this.BorderStyle = BorderStyle.None;

			this.ContextMenu = new ContextMenu();
			this.ContextMenu.MenuItems.Add("Select &All", new EventHandler(this.SelectAll_Click)).Shortcut = Shortcut.CtrlA;
			this.ContextMenu.MenuItems.Add("-");
			this.ContextMenu.MenuItems.Add("&Copy", new EventHandler(this.Copy_Click)).Shortcut = Shortcut.CtrlC;

			this.assemblyBrowser = assemblyBrowser;

			if (this.assemblyBrowser != null)
			{
				this.assemblyBrowser.ActiveItemChanged += new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
			}

			this.languageManager = languageManager;

			if (this.languageManager != null)
			{
				this.languageManager.ActiveLanguageChanged += new EventHandler(this.LanguageManager_ActiveLanguageChanged);
			}

			this.translatorManager = translatorManager;
			this.visibilityConfiguration = visibilityConfiguration;
		}

		~ClassViewWindow()
		{
			if (this.assemblyBrowser != null)
			{
				this.assemblyBrowser.ActiveItemChanged -= new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
				this.assemblyBrowser = null;
			}

			if (this.languageManager != null)
			{
				this.languageManager.ActiveLanguageChanged -= new EventHandler(this.LanguageManager_ActiveLanguageChanged);
				this.languageManager = null;
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			this.Translate();
		}

		private void LanguageManager_ActiveLanguageChanged(object sender, EventArgs e)
		{
			this.Translate();	
		}

		private void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
		{
			this.Translate();
		}

		private void Translate()
		{
			if (this.Parent != null)
			{
				RichTextFormatter formatter = new RichTextFormatter();

				ILanguage language = this.languageManager.ActiveLanguage;

				LanguageWriterConfiguration configuration = new LanguageWriterConfiguration();
				configuration.Visibility = this.visibilityConfiguration;
				configuration["ShowCustomAttributes"] = "true";
				configuration["ShowNamespaceImports"] = "true";
				configuration["ShowNamespaceBody"] = "true";
				configuration["ShowTypeDeclarationBody"] = "true";
				configuration["ShowMethodDeclarationBody"] = "false";

				ILanguageWriter writer = language.GetWriter(formatter, configuration);

				object value = this.assemblyBrowser.ActiveItem;
	
				ITypeDeclaration typeDeclaration = value as ITypeDeclaration;
				if (typeDeclaration != null)
				{
					configuration["ShowMethodDeclarationBody"] = "false";

					if (language.Translate)
					{
						typeDeclaration = this.translatorManager.CreateDisassembler(null, null).TranslateTypeDeclaration(typeDeclaration, true, false);
					}

					writer.WriteTypeDeclaration(typeDeclaration);
				}

				IFieldDeclaration fieldDeclaration = value as IFieldDeclaration;
				if (fieldDeclaration != null)
				{
					configuration["ShowMethodDeclarationBody"] = "true";
					
					if (language.Translate)
					{
						fieldDeclaration = this.translatorManager.CreateDisassembler(null, null).TranslateFieldDeclaration(fieldDeclaration);
					}

					writer.WriteFieldDeclaration(fieldDeclaration);
				}
	
				IMethodDeclaration methodDeclaration = value as IMethodDeclaration;
				if (methodDeclaration != null)
				{
					configuration["ShowMethodDeclarationBody"] = "true";

					if (language.Translate)
					{
						methodDeclaration = this.translatorManager.CreateDisassembler(null, null).TranslateMethodDeclaration(methodDeclaration);
					}

					writer.WriteMethodDeclaration(methodDeclaration);
				}

				IPropertyDeclaration propertyDeclaration = value as IPropertyDeclaration;
				if (propertyDeclaration != null)
				{
					configuration["ShowMethodDeclarationBody"] = "true";

					if (language.Translate)
					{
						propertyDeclaration = this.translatorManager.CreateDisassembler(null, null).TranslatePropertyDeclaration(propertyDeclaration);
					}

					writer.WritePropertyDeclaration(propertyDeclaration);
				}

				IEventDeclaration eventDeclaration = value as IEventDeclaration;
				if (eventDeclaration != null)
				{
					configuration["ShowMethodDeclarationBody"] = "true";

					if (language.Translate)
					{
						eventDeclaration = this.translatorManager.CreateDisassembler(null, null).TranslateEventDeclaration(eventDeclaration);
					}

					writer.WriteEventDeclaration(eventDeclaration);
				}

				this.Rtf = formatter.ToString();
			}
		}

		private void Copy_Click(object sender, EventArgs e)
		{
			this.Copy();
		}

		private void SelectAll_Click(object sender, EventArgs e)
		{
			this.SelectAll();
		}

		private class LanguageWriterConfiguration : ILanguageWriterConfiguration
		{
			private IVisibilityConfiguration visibility;
			private IDictionary table = new Hashtable();
	
			public IVisibilityConfiguration Visibility
			{
				get
				{
					return this.visibility;
				}
	
				set
				{
					this.visibility = value;
				}
			}
	
			public string this[string name]
			{
				get
				{
					return (string) this.table[name];
				}
	
				set
				{
					this.table[name] = value;
				}
			}
		}
	}
}