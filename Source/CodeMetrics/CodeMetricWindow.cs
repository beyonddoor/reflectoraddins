namespace Reflector.CodeMetrics
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Windows.Forms;
	using Reflector.CodeModel;

	internal sealed class CodeMetricWindow : Control
	{
		private CommandBarManager commandBarManager;
		private CommandBar commandBar;

		private CommandBarButton startButton;
		private CommandBarButton copyButton;
		private CommandBarButton saveButton;
		private CommandBarComboBox codeMetricComboBox;

		private IServiceProvider serviceProvider;

		private CodeMetricManager codeMetricManager;

		private Control contentHost;
		private AssemblySelectorControl assemblySelector;
		private StatusView statusView;

		private Thread workerThread;

		public CodeMetricWindow(IServiceProvider serviceProvider)
		{
			this.Visible = false;
			
			this.TabStop = false;
			this.Dock = DockStyle.Fill;

			this.serviceProvider = serviceProvider;

			this.codeMetricManager = new CodeMetricManager();
			this.codeMetricManager.Register(new TypeCodeMetric());
			this.codeMetricManager.Register(new MethodCodeMetric());
			this.codeMetricManager.Register(new ModuleCodeMetric());

			this.codeMetricManager.BeginRun += new EventHandler(this.CodeMetricManager_BeginRun);
			this.codeMetricManager.BeginRunMetric += new CodeMetricEventHandler(this.CodeMetricManager_BeginRunMetric);
			this.codeMetricManager.EndRun += new EventHandler(this.CodeMetricManager_EndRun);
			this.codeMetricManager.EndRunMetric += new CodeMetricEventHandler(this.CodeMetricManager_EndRunMetric);

			this.contentHost = new Control();
			this.contentHost.TabStop = false;
			this.contentHost.Dock = DockStyle.Fill;
			this.Controls.Add(this.contentHost);

			this.commandBarManager = new CommandBarManager();
			this.commandBar = new CommandBar(this.commandBarManager, CommandBarStyle.ToolBar);

			this.startButton = this.commandBar.Items.AddButton(CommandBarImages.Refresh, "Start Analysis", new EventHandler(this.StartButton_Click), Keys.Control | Keys.E);
			this.commandBar.Items.AddSeparator();
			this.saveButton = this.commandBar.Items.AddButton(CommandBarImages.Save, "Save", new EventHandler(this.SaveButton_Click), Keys.Control | Keys.S);
			this.copyButton = this.commandBar.Items.AddButton(CommandBarImages.Copy, "Copy", new EventHandler(this.CopyButton_Click), Keys.Control | Keys.C);
			this.codeMetricComboBox = this.commandBar.Items.AddComboBox("Code Metric Selector", new ComboBox());
			this.codeMetricComboBox.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.codeMetricComboBox.ComboBox.SelectedIndexChanged += new EventHandler(this.CodeMetricComboBox_SelectedIndexChanged);

			this.commandBarManager.CommandBars.Add(this.commandBar);
			this.Controls.Add(this.commandBarManager);

			this.assemblySelector = new AssemblySelectorControl(serviceProvider, this.codeMetricManager);

			this.NewAnalysis();
		}

		public CodeMetricManager CodeMetricManager
		{
			get
			{
				return this.codeMetricManager;
			}
		}

		public void NewAnalysis()
		{
			this.startButton.Enabled = false;
			this.saveButton.Enabled = false;
			this.copyButton.Enabled = false;
			this.codeMetricComboBox.ComboBox.Enabled = false;
			this.codeMetricComboBox.ComboBox.Items.Clear();

			this.contentHost.Controls.Add(this.assemblySelector);
			this.assemblySelector.Focus();

			for (int i = this.contentHost.Controls.Count - 2; i >= 0; i--)
			{
				this.contentHost.Controls.RemoveAt(i);
			}

			this.startButton.Image = CommandBarImages.Refresh;
			this.startButton.Text = "Start Analysis";
			this.startButton.Enabled = true;
		}

		public void StartAnalysis()
		{
			this.startButton.Enabled = false;

			ThreadStart threadStart = new ThreadStart(this.codeMetricManager.Analyze);
			this.workerThread = new Thread(threadStart);
			this.workerThread.Priority = ThreadPriority.BelowNormal;
			this.workerThread.IsBackground = true;
			this.workerThread.Start();
		}

		public bool IsAnalyzing()
		{
			return ((this.workerThread != null) && (this.workerThread.IsAlive));
		}

		public void ClearAssemblyList()
		{
			this.assemblySelector.ClearAssemblyList();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.workerThread.Join();
				this.workerThread = null;

				if (this.codeMetricManager != null)
				{
					this.codeMetricManager.BeginRun -= new EventHandler(this.CodeMetricManager_BeginRun);
					this.codeMetricManager.BeginRunMetric -= new CodeMetricEventHandler(this.CodeMetricManager_BeginRunMetric);
					this.codeMetricManager.EndRun -= new EventHandler(this.CodeMetricManager_EndRun);
					this.codeMetricManager.EndRunMetric -= new CodeMetricEventHandler(this.CodeMetricManager_EndRunMetric);
					this.codeMetricManager = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnParentChanged(EventArgs e)
		{
 			base.OnParentChanged(e);

			this.Visible = (this.Parent != null);

			if (this.contentHost.Controls.Count == 0)
			{
				this.NewAnalysis();
			}
		}

		private void StartButton_Click(object sender, EventArgs e)
		{
			if (this.startButton.Image == CommandBarImages.New)
			{
				this.NewAnalysis();
			}
			else if (this.startButton.Image == CommandBarImages.Refresh)
			{
				this.StartAnalysis();
			}
			else if (this.startButton.Image == CommandBarImages.Stop)
			{
				this.startButton.Enabled = false;
				this.codeMetricManager.Abort();
			}
		}

		private void CopyButton_Click(object sender, EventArgs e)
		{
			if (this.contentHost.Controls.Count == 1)
			{
				CodeMetricView view = this.contentHost.Controls[0] as CodeMetricView;
				if (view != null)
				{
					view.Copy();
				}
			}
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Save Report";
			dialog.DefaultExt = "xml";
			dialog.AddExtension = true;
			dialog.Filter = "XML files (*.xml)|*.xml";

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				using (StreamWriter writer = new StreamWriter(dialog.FileName))
				{
					this.codeMetricManager.Save(writer);
				}
			}
		}

		private void CodeMetricManager_BeginRun(object sender, EventArgs e)
		{
			this.Invoke(new BeginRunUpdateCallback(this.BeginRunUpdate), new object[0]);
		}

		private void CodeMetricManager_EndRun(object sender, EventArgs e)
		{
			this.workerThread = null;
			this.Invoke(new EndRunUpdateCallback(this.EndRunUpdate), new object[0]);
		}

		private void CodeMetricManager_BeginRunMetric(object sender, CodeMetricEventArgs e)
		{
			e.CodeMetric.Progress += new ComputationProgressEventHandler(this.CodeMetric_Progress);
		}

		private void CodeMetricManager_EndRunMetric(object sender, CodeMetricEventArgs e)
		{
			e.CodeMetric.Progress -= new ComputationProgressEventHandler(this.CodeMetric_Progress);
			this.Invoke(new StatusBarUpdateCallback(this.StatusBarUpdate), new object[] { 100 });
		}

		private void CodeMetric_Progress(Object sender, ComputationProgressEventArgs e)
		{
			CodeMetric codeMetric = sender as CodeMetric;

			int index = this.codeMetricManager.CodeMetrics.IndexOf(codeMetric);
			int count = this.codeMetricManager.CodeMetrics.Count;

			int percentComplete = ((index * 100) + (e.PercentComplete)) / count;
			if (percentComplete > 100)
			{
				percentComplete = 100;
			}

			this.Invoke(new StatusBarUpdateCallback(this.StatusBarUpdate), new object[] { percentComplete });
		}

		private delegate void BeginRunUpdateCallback();

		private void BeginRunUpdate()
		{
			this.startButton.Image = CommandBarImages.Stop;
			this.startButton.Text = "Abort Analysis";
			this.startButton.Enabled = true;

			this.saveButton.Enabled = false;
			this.copyButton.Enabled = false;

			this.statusView = new StatusView();

			this.contentHost.Controls.Clear();
			this.contentHost.Controls.Add(this.statusView);

			this.codeMetricComboBox.ComboBox.Enabled = false;
			this.codeMetricComboBox.ComboBox.Items.Clear();

			System.Windows.Forms.Application.DoEvents();
		}

		private delegate void EndRunUpdateCallback();

		private void EndRunUpdate()
		{
			this.contentHost.Controls.Clear();
			this.codeMetricComboBox.ComboBox.Items.Clear();

			if (this.codeMetricManager.IsAbortPending())
			{
				this.NewAnalysis();
			}
			else if (this.codeMetricManager.ErrorException != null)
			{
				throw this.codeMetricManager.ErrorException;
			}
			else
			{
				this.startButton.Image = CommandBarImages.New;
				this.startButton.Text = "New Analysis";
				this.startButton.Enabled = true;

				this.saveButton.Enabled = true;
				this.copyButton.Enabled = true;

				CodeMetric activeCodeMetric = null;

				foreach (CodeMetric codeMetric in this.codeMetricManager.CodeMetrics)
				{
					this.codeMetricComboBox.ComboBox.Items.Add(codeMetric);

					if (activeCodeMetric == null)
					{
						activeCodeMetric = codeMetric;
					}
				}

				if (this.codeMetricComboBox.ComboBox.SelectedItem == null)
				{
					this.codeMetricComboBox.ComboBox.SelectedItem = activeCodeMetric;
				}

				this.codeMetricComboBox.ComboBox.Enabled = true;
			}

			System.Windows.Forms.Application.DoEvents();
		}

		private delegate void StatusBarUpdateCallback(int percentComplete);

		private void StatusBarUpdate(int percentComplete)
		{
			this.statusView.UpdateStatus(percentComplete);
			System.Windows.Forms.Application.DoEvents();
		}

		private void CodeMetricComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor currentCursor = Cursor.Current;
			try
			{
				Cursor.Current = Cursors.WaitCursor;

				CodeMetric codeMetric = this.codeMetricComboBox.ComboBox.SelectedItem as CodeMetric;
				if (codeMetric != null)
				{
					CodeMetricView codeMetricView = new CodeMetricView(codeMetric, this.serviceProvider);

					this.contentHost.Controls.Add(codeMetricView);
					codeMetricView.Activate();

					for (int i = this.contentHost.Controls.Count - 2; i >= 0; i--)
					{
						this.contentHost.Controls.RemoveAt(i);
					}
				}
			}
			finally
			{
				Cursor.Current = currentCursor;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Send keys to local CommandBarManager before they bubble to the main window.
			if ((this.commandBarManager != null) && (this.commandBarManager.PreProcessMessage(ref msg)))
			{
				return true;
			}

			if (base.ProcessCmdKey(ref msg, keyData))
			{
				return true;
			}

			return false;
		}

		private class AssemblySelectorControl : ListView
		{
			private IAssemblyManager assemblyManager;
			private IAssemblyBrowser assemblyBrowser;
			private CodeMetricManager codeMetricManager;

			public AssemblySelectorControl(IServiceProvider serviceProvider, CodeMetricManager codeMetricManager)
			{
				this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
				this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
				this.codeMetricManager = codeMetricManager;

				ColumnHeader[] columnHeaders = new ColumnHeader[3];
				columnHeaders[0] = new ColumnHeader();
				columnHeaders[0].Text = "Name";
				columnHeaders[0].Width = 250;
				columnHeaders[1] = new ColumnHeader();
				columnHeaders[1].Text = "Version";
				columnHeaders[1].Width = 80;
				columnHeaders[2] = new ColumnHeader();
				columnHeaders[2].Text = "Location";
				columnHeaders[2].Width = 400;

				this.Dock = DockStyle.Fill;
				this.Columns.AddRange(columnHeaders);
				this.FullRowSelect = true;
				this.TabIndex = 0;
				this.CheckBoxes = true;
				this.View = View.Details;
			}

			public void Populate()
			{
				IntPtr handle = this.Handle;

				this.Items.Clear();

				bool focused = false;

				for (int i = 0; i < this.assemblyManager.Assemblies.Count; ++i)
				{
					IAssembly assembly = this.assemblyManager.Assemblies[i];

					ListViewItem item = new ListViewItem();
					item.Tag = assembly;
					item.Text = assembly.Name;
					item.Checked = Array.IndexOf(this.codeMetricManager.Assemblies, assembly) != -1;

					ListViewItem.ListViewSubItem subItem1 = new ListViewItem.ListViewSubItem();

					if ((assembly.Status == null) || (assembly.Status.Length == 0))
					{
						subItem1.Text = assembly.Version.ToString();
					}

					item.SubItems.Add(subItem1);

					ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem();
					subItem2.Text = assembly.Location;
					item.SubItems.Add(subItem2);

					this.Items.Add(item);

					if ((item.Checked) && (!focused))
					{
						item.Focused = true;
						item.Selected = true;

						focused = false;
					}
				}
			}

			public void ClearAssemblyList()
			{
				foreach (ListViewItem item in this.Items)
				{
					item.Checked = false;
				}
			}

			protected override void OnParentChanged(EventArgs e)
			{
 				 base.OnParentChanged(e);

				 if (this.Parent != null)
				 {
					 this.assemblyManager.AssemblyLoaded += new EventHandler(AssemblyManager_AssemblyLoaded);
					 this.assemblyManager.AssemblyUnloaded += new EventHandler(AssemblyManager_AssemblyLoaded);
					 this.assemblyBrowser.ActiveItemChanged += new EventHandler(AssemblyBrowser_ActiveItemChanged);

					 this.UpdateActiveItem();
					 this.Populate();
				 }
				 else
				 {
					 this.assemblyManager.AssemblyLoaded -= new EventHandler(AssemblyManager_AssemblyLoaded);
					 this.assemblyManager.AssemblyUnloaded -= new EventHandler(AssemblyManager_AssemblyLoaded);
					 this.assemblyBrowser.ActiveItemChanged -= new EventHandler(AssemblyBrowser_ActiveItemChanged);
				 }
			}

			protected override void OnItemCheck(ItemCheckEventArgs e)
			{
				base.OnItemCheck(e);

				if (e.Index < this.Items.Count)
				{
					ListViewItem item = this.Items[e.Index] as ListViewItem;
					if (item != null)
					{
						IAssembly assembly = item.Tag as IAssembly;
						if (assembly != null)
						{
							if (e.NewValue == CheckState.Checked)
							{
								this.codeMetricManager.AddAssembly(assembly);
							}
							else
							{
								this.codeMetricManager.RemoveAssembly(assembly);
							}
						}
					}
				}
			}

			private void AssemblyManager_AssemblyLoaded(object sender, EventArgs e)
			{
				if (this.Visible)
				{
					this.UpdateActiveItem();
					this.Populate();
				}
			}

			private void UpdateActiveItem()
			{
				// get active IAssembly
				IAssembly currentAssembly = this.GetCurrentAssembly();
				if (currentAssembly != null)
				{
					IAssembly[] assemblies = this.codeMetricManager.Assemblies;
					foreach (IAssembly assembly in assemblies)
					{
						this.codeMetricManager.RemoveAssembly(assembly);
					}

					this.codeMetricManager.AddAssembly(currentAssembly);
				}
			}

			private void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
			{
				if (this.Enabled)
				{
					this.UpdateActiveItem();
					this.Populate();
				}
			}

			private IAssembly GetCurrentAssembly()
			{
				IAssembly assembly = this.assemblyBrowser.ActiveItem as IAssembly;
				if (assembly != null)
				{
					return assembly;
				}

				IModule module = this.assemblyBrowser.ActiveItem as IModule;
				if (module != null)
				{
					return module.Assembly;
				}

				ITypeReference typeReference = this.assemblyBrowser.ActiveItem as ITypeReference;
				if (typeReference != null)
				{
					IModule declaringModule = GetModule(typeReference);
					if (declaringModule != null)
					{
						return declaringModule.Assembly;
					}
				}

				IMemberDeclaration memberDeclaration = this.assemblyBrowser.ActiveItem as IMemberDeclaration;
				if (memberDeclaration != null)
				{
					ITypeReference declaringType = memberDeclaration.DeclaringType as ITypeReference;
					if (declaringType != null)
					{
						IModule declaringModule = GetModule(declaringType);
						if (declaringModule != null)
						{
							return declaringModule.Assembly;
						}
					}
				}

				return null;
			}

			private IModule GetModule(ITypeReference type)
			{
				ITypeReference current = type;
				while (current != null)
				{
					IModule owner = current.Owner as IModule;
					if (owner != null)
					{
						return owner;
					}
					current = current.Owner as ITypeReference;
				}
				return null;
			}
		}

		private class StatusView : Control
		{
			private ProgressBar progressBar;

			public StatusView()
			{
				this.Dock = DockStyle.Fill;
				this.TabStop = false;

				this.progressBar = new ProgressBar();
				this.progressBar.Size = new Size(100, 16);
				this.Controls.Add(this.progressBar);

				this.OnSizeChanged(EventArgs.Empty);
			}

			protected override void OnSizeChanged(EventArgs e)
			{
				base.OnSizeChanged(e);
				this.progressBar.Location = new Point((this.Width / 2) - (this.progressBar.Width / 2), (this.Height / 2) - (this.progressBar.Height / 2));
			}

			public void UpdateStatus(int percentComplete)
			{
				if (percentComplete != this.progressBar.Value)
				{
					this.progressBar.Value = percentComplete;
				}
			}
		}
	}
}
