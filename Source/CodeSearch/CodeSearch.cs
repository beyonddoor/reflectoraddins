[assembly: System.Reflection.AssemblyTitle("Reflector.CodeSearch")]
[assembly: System.Reflection.AssemblyCompany("mailto:hchapalain@hotmail.com")]
[assembly: System.Reflection.AssemblyProduct("Reflector.CodeSearch")]
[assembly: System.Reflection.AssemblyCopyright("Copyright © Herve Chapalain 2005-2007")]
[assembly: System.Reflection.AssemblyVersion("5.0.0.*")]

namespace Reflector.CodeSearch
{
	using System;
	using System.Drawing;
	using System.IO;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Windows.Forms;
	using Reflector;
	using Reflector.CodeModel;

	public class CodeSearchWindow : UserControl, IPackage
	{
		#region Member's variables

		private IWindowManager windowManager = null;
		private ICommandBarManager commandbarManager = null;
		private EventHandler evtSearch = null;
		private IServiceProvider serviceProvider = null;
		private ICommandBarButton toolBarButton = null;
		private ICommandBarSeparator toolBarSeparator = null;
		private ICommandBarButton menuBarButton = null;
		private ICommandBarSeparator menuBarSeparator = null;
		private ComboBox txtQuery;
		private ListView lstResults;
		private ToggleButton btnSearch;
		private ToggleButton btnCancel;
		private ProgressBar pgBar;
		private ColumnHeader colHeadPath;
		private ColumnHeader colHeadHit;
		private System.ComponentModel.IContainer components = null;
		private IAssemblyBrowser assemblyBrowser = null;
		private ILanguageManager languageManager = null;
		private ITranslatorManager translatorManager = null;
		private IVisibilityConfiguration visibilityConfiguration = null;

		bool bCountAtModuleLevel = false;
		bool bCountAtNSLevel = false;
		bool bCountAtTypeLevel = false;

		bool bIsCanceledRequired = false;
		bool bIsSearchPending = false;

		#endregion

		#region Constructor and Destructor
		public CodeSearchWindow()
		{
			InitializeComponent();
		}
		~CodeSearchWindow()
		{

			this.SizeChanged -= new System.EventHandler(this.OnSizeChanged);
			this.btnCancel.Click -= new System.EventHandler(this.btnCancel_Click);
			this.btnSearch.Click -= new System.EventHandler(this.btnSearch_Click);
			this.txtQuery.KeyPress -= new System.Windows.Forms.KeyPressEventHandler(this.txtQuery_KeyPress);
		}
		#endregion

		#region IPackage Members

		void IPackage.Load(IServiceProvider serviceProvider)
		{

			this.serviceProvider = serviceProvider;
			this.windowManager = (IWindowManager)this.serviceProvider.GetService(typeof(IWindowManager));
			this.commandbarManager = (ICommandBarManager)this.serviceProvider.GetService(typeof(ICommandBarManager));

			// manage the search window
			this.windowManager.Windows.Add("SearchCodeWindow", this, "Code Search " + this.GetType().Assembly.GetName().Version.ToString());

			// load the resources
			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			Stream myStream = myAssembly.GetManifestResourceStream("Reflector.CodeSearch.Search.png");
			Bitmap img = null;

			if (myStream != null)
			{
				img = new Bitmap(myStream);
			}

			// add the toolbar button
			evtSearch = new EventHandler(this.CodeSearchButton_Click);

			this.toolBarSeparator = commandbarManager.CommandBars["ToolBar"].Items.AddSeparator();
			this.toolBarButton = commandbarManager.CommandBars["ToolBar"].Items.AddButton("Code Search", img, evtSearch);

			this.menuBarSeparator = commandbarManager.CommandBars["Tools"].Items.AddSeparator();
			this.menuBarButton = commandbarManager.CommandBars["Tools"].Items.AddButton("Code Search", img, evtSearch);

			// init members
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.languageManager = (ILanguageManager)serviceProvider.GetService(typeof(ILanguageManager));
			this.translatorManager = (ITranslatorManager)serviceProvider.GetService(typeof(ITranslatorManager));
			this.visibilityConfiguration = (IVisibilityConfiguration)serviceProvider.GetService(typeof(IVisibilityConfiguration));
		}

		void IPackage.Unload()
		{
			this.windowManager.Windows.Remove("SearchCodeWindow");

			this.commandbarManager.CommandBars["ToolBar"].Items.Remove(this.toolBarButton);
			this.commandbarManager.CommandBars["ToolBar"].Items.Remove(this.toolBarSeparator);

			this.commandbarManager.CommandBars["Tools"].Items.Remove(this.toolBarButton);
			this.commandbarManager.CommandBars["Tools"].Items.Remove(this.toolBarSeparator);
		}

		#endregion

		#region UI Stuffs here
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CodeSearchWindow));
			this.txtQuery = new System.Windows.Forms.ComboBox();
			this.lstResults = new System.Windows.Forms.ListView();
			this.colHeadPath = new System.Windows.Forms.ColumnHeader();
			this.colHeadHit = new System.Windows.Forms.ColumnHeader();
			this.btnSearch = new ToggleButton();
			this.btnCancel = new ToggleButton();
			this.pgBar = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();

			// required to see the Caption bar
			this.Dock = DockStyle.Fill;
			this.BackColor = System.Drawing.SystemColors.Control;

			this.txtQuery.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQuery_KeyPress);

			// 
			// lstResults
			// 

			this.lstResults.AutoArrange = false;
			this.lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.colHeadPath, this.colHeadHit });
			this.lstResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstResults.Location = new System.Drawing.Point(0, 22);
			this.lstResults.MultiSelect = false;
			this.lstResults.Name = "lstResults";
			this.lstResults.Size = new System.Drawing.Size(10, 10);
			this.lstResults.TabIndex = 2;
			this.lstResults.View = System.Windows.Forms.View.Details;

			this.lstResults.DoubleClick += new System.EventHandler(this.lstResults_DoubleClick);
			this.lstResults.Click += new System.EventHandler(this.lstResults_Click);

			// 
			// columnHeader1
			// 
			this.colHeadPath.Text = "Path";
			this.colHeadPath.Width = 340;
			// 
			// columnHeader2
			// 
			this.colHeadHit.Text = "Hit Count";

			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right));
			this.btnSearch.Text = "Search";

			System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			Stream myStream = myAssembly.GetManifestResourceStream("Reflector.CodeSearch.Search.png");
			if (myStream != null)
			{
				Bitmap img = new Bitmap(myStream);
				if (img != null)
					this.btnSearch.Image = img;
			}
			this.btnSearch.Location = new System.Drawing.Point(350, 0);
			this.btnSearch.ForeColor = System.Drawing.SystemColors.Control;
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(21, 21);
			this.btnSearch.TabIndex = 1;
			this.btnSearch.TabStop = false;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);

			//
			// btnCancel
			//
			this.btnCancel.Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right));
			this.btnCancel.Text = "Cancel";

			System.Reflection.Assembly myAssembly2 = System.Reflection.Assembly.GetExecutingAssembly();
			Stream myStream2 = myAssembly2.GetManifestResourceStream("Reflector.CodeSearch.Cancel.png");
			if (myStream2 != null)
			{
				Bitmap img = new Bitmap(myStream2);
				if (img != null)
					this.btnCancel.Image = img;
			}
			this.btnCancel.Location = new System.Drawing.Point(371, 0);
			this.btnCancel.ForeColor = System.Drawing.SystemColors.Control;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(21, 21);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.TabStop = false;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);


			// 
			// pgBar
			// 
			// this.pgBar.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Left);
			// this.pgBar.Location = new System.Drawing.Point(0, 248);
			// this.pgBar.Name = "pgBar";
			// this.pgBar.Size = new System.Drawing.Size(363, 16);

			// 
			// CodeSearchWindow
			//

			// this.Controls.Add(this.pgBar);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.lstResults);
			this.Controls.Add(this.txtQuery);
			this.Name = "CodeSearchWindow";
			this.Size = new System.Drawing.Size(393, 266);


			this.SizeChanged += new System.EventHandler(this.OnSizeChanged);

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.windowManager.StatusBar.Controls.Add(this.pgBar);
			}
			else
			{
				this.windowManager.StatusBar.Controls.Remove(this.pgBar);
			}
		}

		void OnSizeChanged(object sender, System.EventArgs e)
		{
			try
			{
				this.txtQuery.Size = new Size(this.Size.Width - this.btnSearch.Width - this.btnCancel.Width - 4, 20);
				this.lstResults.Size = new Size(this.Size.Width, this.Size.Height - 1 - this.txtQuery.Size.Height);
				// this.pgBar.Size = new Size(this.Size.Width, 16);
			}
			catch (Exception except)
			{
				string s = except.Message;
			}
		}

		private void CodeSearchButton_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["SearchCodeWindow"].Visible = true;
		}

		void DoSearchAsynchronously()
		{

			bIsSearchPending = true;

			bIsCanceledRequired = false;
			if (txtQuery.Text == "")
				return;

			bool bAlreadyExist = false;
			foreach (string s in this.txtQuery.Items)
			{
				if (s == txtQuery.Text)
				{
					bAlreadyExist = true;
					break;
				}
			}
			if (!bAlreadyExist)
				this.txtQuery.Items.Add(txtQuery.Text);

			try
			{
				lstResults.Focus();
				this.Cursor = Cursors.WaitCursor;
				SearchInTheCode(txtQuery.Text);
				this.Cursor = Cursors.Default;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Exception");
			}
			finally
			{
				btnSearch.Invalidate();
				btnSearch.Update();
				this.Cursor = Cursors.Default;
				bIsSearchPending = false;
			}
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{

			this.txtQuery.Focus();
			btnSearch.Invalidate();
			btnSearch.Update();
			System.Windows.Forms.Application.DoEvents();

			if (bIsSearchPending == true)
				return;

			Thread wt = new System.Threading.Thread(new ThreadStart(this.DoSearchAsynchronously));
			wt.Start();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.bIsCanceledRequired = true;
			this.txtQuery.Focus();
			btnSearch.Invalidate();
			btnSearch.Update();
			System.Windows.Forms.Application.DoEvents();
		}

		private void txtQuery_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13)
				btnSearch_Click(null, null);
		}

		private void lstResults_Click(object sender, EventArgs e)
		{
			try
			{
				this.assemblyBrowser.ActiveItem = (object)lstResults.SelectedItems[0].Tag;
				lstResults.Select();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Exception");
			}
		}

		private void lstResults_DoubleClick(object sender, EventArgs e)
		{
			if (this.windowManager.Windows["DisassemblerWindow"] != null)
				this.windowManager.Windows["DisassemblerWindow"].Visible = true;
		}

		#endregion

		#region Search routines
		private void SearchInTheCode(string sQuery)
		{
			lstResults.Items.Clear();
			object value = this.assemblyBrowser.ActiveItem;

			TextFormatter formatter = new TextFormatter();
			ILanguage language = this.languageManager.ActiveLanguage;
			if ((language == null) || (formatter == null))
				return;

			// is selected item an assembly
			IAssembly assembly = value as IAssembly;
			if (assembly != null)
			{
				try
				{
					bCountAtModuleLevel = true;
					pgBar.Maximum = 0;
					foreach (IModule m in assembly.Modules)
					{
						pgBar.Maximum = pgBar.Maximum + m.Types.Count;
					}

					SearchInAssembly(value);
				}
				finally
				{
					pgBar.Value = 0;
					bCountAtModuleLevel = false;
				}
				return;
			}

			if (this.bIsCanceledRequired)
				return;

			// is selected item a module
			IModule module = value as IModule;
			if (module != null)
			{
				try
				{
					bCountAtModuleLevel = true;
					pgBar.Maximum = module.Types.Count;
					SearchInModule(value);
				}
				finally
				{
					pgBar.Value = 0;
					bCountAtModuleLevel = false;
				}
				return;
			}

			if (this.bIsCanceledRequired)
				return;

			// is selected item a namespace
			INamespace namespaceDeclaration = value as INamespace;
			if (namespaceDeclaration != null)
			{
				try
				{
					bCountAtNSLevel = true;
					pgBar.Maximum = namespaceDeclaration.Types.Count;
					SearchInNameSpace(value);
				}
				finally
				{
					pgBar.Value = 0;
					bCountAtNSLevel = false;
				}
				return;
			}

			if (this.bIsCanceledRequired)
				return;

			// is selected item a type
			ITypeDeclaration typeDeclaration = value as ITypeDeclaration;
			if (typeDeclaration != null)
			{
				try
				{
					bCountAtTypeLevel = true;
					pgBar.Maximum = typeDeclaration.Methods.Count;
					SearchInType(value);
				}
				finally
				{
					pgBar.Value = 0;
					bCountAtTypeLevel = false;
				}
				return;
			}

			if (this.bIsCanceledRequired)
				return;

			// is selected item a method
			IMethodDeclaration methodDeclaration = value as IMethodDeclaration;
			if (methodDeclaration != null)
			{
				SearchInMethod(value);
				return;
			}
		}

		private void SearchInAssembly(object assembly)
		{
			foreach (IModule module in ((IAssembly)assembly).Modules)
			{
				SearchInModule(module);
				if (this.bIsCanceledRequired)
					break;
			}
		}

		private void SearchInModule(object module)
		{
			foreach (IType t in ((IModule)module).Types)
			{
				if (bCountAtModuleLevel)
					pgBar.Value++;
				ITypeDeclaration td = (ITypeDeclaration)t;
				if (td != null)
					SearchInType(td);
				if (this.bIsCanceledRequired)
					return;
			}
		}

		private void SearchInNameSpace(object ns)
		{
			foreach (ITypeDeclaration m in ((INamespace)ns).Types)
			{
				if (bCountAtNSLevel)
					pgBar.Value++;
				SearchInType(m);
				if (this.bIsCanceledRequired)
					return;
			}
		}

		private void SearchInType(object type)
		{
			foreach (IMethodDeclaration m in ((ITypeDeclaration)type).Methods)
			{
				if (bCountAtTypeLevel)
					pgBar.Value++;
				SearchInMethod(m);
				System.Windows.Forms.Application.DoEvents();
				if (this.bIsCanceledRequired)
					return;
			}
		}

		private void SearchInMethod(object method)
		{
			string source = GetSourceCode(method);
			if (source != null)
			{
				try
				{
					MatchCollection mc;
					Regex r = new Regex(txtQuery.Text);
					mc = r.Matches(source);
					if (mc.Count > 0)
					{
						ListViewItem item = lstResults.Items.Add(method.ToString());
						item.Tag = (object)method;
						item.SubItems.Add(mc.Count.ToString());
					}
				}
				catch (Exception e)
				{
					string sEx = e.ToString();
				}
			}
		}

		private string GetSourceCode(object method)
		{
			TextFormatter formatter = new TextFormatter();
			ILanguage language = this.languageManager.ActiveLanguage;
			if ((language != null) && (formatter != null))
			{
				LanguageWriterConfiguration configuration = new LanguageWriterConfiguration();
				configuration.Visibility = this.visibilityConfiguration;
				ILanguageWriter writer = language.GetWriter(formatter, configuration);
				if (writer != null)
				{
					try
					{
						IMethodDeclaration md = null;
						if (language.Translate)
						{
							ITranslator translator = this.translatorManager.CreateDisassembler(null, null);
							if (translator != null)
							{
								md = translator.TranslateMethodDeclaration((IMethodDeclaration)method);
							}
						}
						else
						{
							md = (IMethodDeclaration)method;
						}

						writer.WriteMethodDeclaration(md);

						return formatter.ToString();
					}
					catch (Exception ex)
					{
						string sEx = ex.ToString();
					}
				}
			}
			return null;
		}
		#endregion

		private class LanguageWriterConfiguration : ILanguageWriterConfiguration
		{
			private IVisibilityConfiguration visibility;
			public IVisibilityConfiguration Visibility
			{
				get { return this.visibility; }
				set { this.visibility = value; }
			}
			public string this[string name]
			{
				get
				{
					switch (name)
					{
						case "ShowMethodDeclarationBody":
						case "ShowCustomAttributes":
							return "true";
						default:
							return "false";
					}
				}
			}
		}

		private class ToggleButton : UserControl
		{
			public event EventHandler CheckedChanged;

			private bool isChecked = false;
			private bool isHover = false;
			private bool isPressed = false;
			private Image image = null;
			private ToolTip toolTip = null;
			private Keys shortcut = Keys.None;

			public ToggleButton()
			{
				this.TabStop = false;
			}

			public Image Image
			{
				set
				{
					this.image = value;
					this.Invalidate();
				}
				get
				{
					return this.image;
				}
			}

			public bool Checked
			{
				set
				{
					this.Text = this.Text;
					this.isChecked = value;
					this.Invalidate();
					this.OnCheckedChanged(EventArgs.Empty);
				}
				get
				{
					return this.isChecked;
				}
			}

			public override string Text
			{
				get
				{
					return base.Text;
				}

				set
				{
					if (toolTip != null)
					{
						this.toolTip.RemoveAll();
						this.toolTip.Dispose();
					}

					base.Text = value;
					this.toolTip = new ToolTip();
					this.toolTip.InitialDelay = 0;
					this.toolTip.ShowAlways = true;
					this.toolTip.SetToolTip(this, value);
				}
			}

			public Keys Shortcut
			{
				set
				{
					this.shortcut = value;
				}

				get
				{
					return this.shortcut;
				}
			}

			protected override void OnMouseEnter(EventArgs e)
			{
				this.isHover = true;
				this.Invalidate();
				this.Update();
				base.OnMouseEnter(e);
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				this.isHover = false;
				this.Invalidate();
				this.Update();
				base.OnMouseLeave(e);
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				if ((e.Button == MouseButtons.Left) && (e.Clicks == 1) && (!this.Checked))
				{
					this.isPressed = true;
					this.Invalidate();
					this.Update();
				}
				base.OnMouseDown(e);
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				if ((e.Button == MouseButtons.Left) && (e.Clicks == 1) && (!this.Checked))
				{
					if (this.isPressed)
					{
						this.isPressed = false;
						this.Invalidate();
						this.Update();
						//this.Toggle();
					}
				}
				base.OnMouseUp(e);
			}

			protected override bool ProcessKeyPreview(ref Message message)
			{
				Keys keyData = (Keys)(int)message.WParam | ModifierKeys;
				if ((keyData == this.shortcut) && (!this.Checked))
				{
					//this.Toggle();
					return true;
				}
				return base.ProcessKeyPreview(ref message);
			}

			protected override void OnPaintBackground(PaintEventArgs e)
			{
				if (this.isPressed)
				{
					e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(170, 190, 220)), 0, 0, this.Width - 1, this.Height - 1);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(127, 157, 185), 1.0f), 0, 0, this.Width - 1, this.Height - 1);
				}
				else if (this.Checked)
				{
					e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 250)), 0, 0, this.Width - 1, this.Height - 1);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(127, 157, 185), 1.0f), 0, 0, this.Width - 1, this.Height - 1);
				}
				else if (this.isHover)
				{
					base.OnPaintBackground(e);
					e.Graphics.DrawRectangle(new Pen(Color.FromArgb(127, 157, 185), 1.0f), 0, 0, this.Width - 1, this.Height - 1);
				}
				else
				{
					base.OnPaintBackground(e);
				}
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				if (this.image != null)
				{
					Size size = this.image.Size;

					Point point = new Point((this.Width - size.Width) / 2, (this.Height - size.Height) / 2);
					e.Graphics.DrawImage(this.image, point.X, point.Y, size.Width, size.Height);
				}
			}

			protected virtual void OnCheckedChanged(EventArgs e)
			{
				if (this.CheckedChanged != null)
				{
					this.CheckedChanged(this, e);
				}
			}

			private void Toggle()
			{
				this.Checked = !this.Checked;
				this.OnClick(EventArgs.Empty);
			}
		}

		private class TextFormatter : IFormatter
		{
			private StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			private bool allowProperties = false;
			private bool newLine = false;
			private int indent = 0;

			public override string ToString()
			{
				return this.writer.ToString();
			}

			public void Write(string text)
			{
				this.ApplyIndent();
				this.writer.Write(text);
			}

			public void WriteDeclaration(string text)
			{
				this.WriteBold(text);
			}
			public void WriteDeclaration(string text, object target)
			{
				this.WriteBold(text);
			}

			public void WriteComment(string text)
			{
				this.WriteColor(text, (int)0x808080);
			}

			public void WriteLiteral(string text)
			{
				this.WriteColor(text, (int)0x800000);
			}

			public void WriteKeyword(string text)
			{
				this.WriteColor(text, (int)0x000080);
			}

			public void WriteIndent()
			{
				this.indent++;
			}

			public void WriteLine()
			{
				this.writer.WriteLine();
				this.newLine = true;
			}

			public void WriteOutdent()
			{
				this.indent--;
			}

			public void WriteReference(string text, string toolTip, Object reference)
			{
				this.ApplyIndent();
				this.writer.Write(text);
			}

			public void WriteProperty(string propertyName, string propertyValue)
			{
				if (this.allowProperties)
				{
					throw new NotSupportedException();
				}
			}

			public bool AllowProperties
			{
				set
				{
					this.allowProperties = value;
				}

				get
				{
					return this.allowProperties;
				}
			}

			private void WriteBold(string text)
			{
				this.ApplyIndent();
				this.writer.Write(text);
			}

			private void WriteColor(string text, int color)
			{
				this.ApplyIndent();
				this.writer.Write(text);
			}

			private void ApplyIndent()
			{
				if (this.newLine)
				{
					for (int i = 0; i < this.indent; i++)
					{
						this.writer.Write("    ");
					}

					this.newLine = false;
				}
			}
		}
	}
}
