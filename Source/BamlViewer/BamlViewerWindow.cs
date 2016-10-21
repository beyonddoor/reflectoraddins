namespace Reflector.BamlViewer
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using Reflector.CodeModel;

	internal class BamlViewerWindow : Control
	{
		private IAssemblyManager assemblyManager;
		private BrowserTreeView browserTreeView;
		private Splitter splitter;
		private RichTextBox textBox;
		private Label messageLabel;

		public BamlViewerWindow(IServiceProvider serviceProvider)
		{
			this.TabStop = false;

			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));

			this.messageLabel = new Label();
			this.messageLabel.FlatStyle = FlatStyle.System;
			this.messageLabel.AutoSize = true;
			this.messageLabel.Text = "Please load assemblies containing BAML resources into Reflector.";
			this.Controls.Add(this.messageLabel);

			this.browserTreeView = new BrowserTreeView();
			this.browserTreeView.TabIndex = 1;
			this.browserTreeView.Dock = DockStyle.Top;
			this.browserTreeView.Height = 200;
			this.browserTreeView.AfterSelect += new TreeViewEventHandler(this.BrowserTreeView_AfterSelect);

			this.splitter = new Splitter();
			this.splitter.Dock = DockStyle.Top;

			this.textBox = new RichTextBox();
			this.textBox.TabIndex = 2;
			this.textBox.Font = new Font("Courier New", SystemInformation.MenuFont.SizeInPoints);
			this.textBox.Dock = DockStyle.Fill;
			this.textBox.Multiline = true;
			this.textBox.WordWrap = false;
			this.textBox.Select(0, 0);
			this.textBox.ContextMenu = new ContextMenu(new MenuItem[] { 
				new MenuItem("Select All", new EventHandler(this.TextBox_SelectAll)),
				new MenuItem("Copy", new EventHandler(this.TextBox_Copy))});
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.messageLabel.Location = new Point((this.Width - this.messageLabel.Width) / 2, (this.Height - this.messageLabel.Height) / 2);
		}

		private void BrowserTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			this.textBox.Text = string.Empty;

			if (e.Node != null)
			{
				MemoryStream stream = e.Node.Tag as MemoryStream;
				if (stream != null)
				{
					stream.Position = 0;

					try
					{
						BamlTranslator translator = new BamlTranslator(stream);
			
						this.textBox.Text = translator.ToString();
						this.textBox.Select(0, 0);
					}
					catch (Exception exception)
					{
						MessageBox.Show(null, exception.Message, "BAML Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void TextBox_SelectAll(object sender, EventArgs args)
		{
			this.textBox.SelectAll();
		}

		private void TextBox_Copy(object sender, EventArgs args)
		{
			this.textBox.Copy();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.assemblyManager.AssemblyLoaded += new EventHandler(this.AssemblyManager_AssemblyLoaded);
				this.assemblyManager.AssemblyUnloaded += new EventHandler(this.AssemblyManager_AssemblyUnloaded);

				this.UpdateBrowserTreeView(this.assemblyManager.Assemblies);
			}
			else
			{
				this.assemblyManager.AssemblyLoaded -= new EventHandler(this.AssemblyManager_AssemblyLoaded);
				this.assemblyManager.AssemblyUnloaded -= new EventHandler(this.AssemblyManager_AssemblyUnloaded);
				this.browserTreeView.Clear();
			}
		}

		private void AssemblyManager_AssemblyLoaded(object sender, EventArgs e)
		{
			this.UpdateBrowserTreeView(this.assemblyManager.Assemblies);
		}

		private void AssemblyManager_AssemblyUnloaded(object sender, EventArgs e)
		{
			this.UpdateBrowserTreeView(this.assemblyManager.Assemblies);
		}

		private void UpdateBrowserTreeView(ICollection assemblies)
		{
			this.browserTreeView.Clear();
			this.browserTreeView.AddRange(assemblies);

			if (this.browserTreeView.Nodes.Count > 0)
			{
				if (this.Controls.Count != 3)
				{
					this.Controls.Clear();
					this.Controls.Add(this.textBox);
					this.Controls.Add(this.splitter);
					this.Controls.Add(this.browserTreeView);
				}
			}
			else
			{
				if (this.Controls.Count != 1)
				{
					this.Controls.Clear();
					this.Controls.Add(this.messageLabel);
					this.OnSizeChanged(EventArgs.Empty);
				}
			}
		}

		private class BrowserTreeView : TreeView
		{

			private ContextMenu bamlNodeContextMenu;

			public BrowserTreeView()
			{
				this.ShowLines = false;
				this.HotTracking = true;
				this.HideSelection = false;

				this.ImageList = new ImageList();
				this.ImageList.Images.AddStrip(new Bitmap(this.GetType().Assembly.GetManifestResourceStream("Reflector.BamlViewer.Icon.png")));
				this.ImageList.ColorDepth = ColorDepth.Depth32Bit;
				this.ImageList.TransparentColor = Color.FromArgb(255, 0, 128, 0);

				// The Context Menu to show on BamlNodes
				bamlNodeContextMenu = new ContextMenu(new MenuItem[] {	new MenuItem("Save As...", new EventHandler(this.BrowserTreeView_SaveAs))  });

				// Event Handler to test for the BamlNodes and show the ContextMenu
				this.MouseUp += new MouseEventHandler(BrowserTreeView_MouseUp); 
			}

			public void Clear()
			{
				this.SelectedNode = null;
				this.Nodes.Clear();
			}

			public void AddRange(ICollection assemblies)
			{
				this.BeginUpdate();
				try
				{
					foreach (IAssembly assembly in assemblies)
					{
						AssemblyNode assemblyNode = new AssemblyNode(assembly);
						if (assemblyNode.IsValid())
						{
							this.Nodes.Add(assemblyNode);

							if (this.SelectedNode == null)
							{
								this.SelectedNode = assemblyNode;
							}
						}
					}
				}
				finally
				{
					this.EndUpdate();
				}
			}

			protected override void WndProc(ref Message message)
			{
				// Fix WM_ERASEBKGND behavior
				if (message.Msg != 0x14)
				{
					base.WndProc(ref message);
				}
			}

			void BrowserTreeView_MouseUp(object sender, MouseEventArgs e)
			{
				// Only show the Context Menu on Right Click
				if (e.Button == MouseButtons.Right)
				{

					Point pt = new Point(e.X, e.Y);

					// Get the node under the mouse
					TreeNode node = GetNodeAt(e.X, e.Y);

					// Show the appropriate Menu
					if (node is BamlNode)
					{
						// Right clicking doesn't select the node, so
						// Force the selection before you show the 
						// ContextMenu so the MenuItem handler(s)
						// can use the current selection
						SelectedNode = node;

						// Show the ContextMenu
						bamlNodeContextMenu.Show(this, pt);
					}
				}
			}

			void BrowserTreeView_SaveAs(object sender, EventArgs args)
			{
				try
				{
					BamlNode node = SelectedNode as BamlNode;
					if (node != null)
					{
						// Construct the file name
						string[] fileNameParts = node.Text.Split('/', '\\');
						string fileName = fileNameParts[fileNameParts.Length - 1].ToLower().Replace(".baml", ".xaml");

						// Setup and show the Save File Dialog
						SaveFileDialog dialog = new SaveFileDialog();
						dialog.Title = "Save As";
						dialog.OverwritePrompt = true;
						dialog.CheckPathExists = true;
						dialog.Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*";
						dialog.FilterIndex = 0;
						dialog.AddExtension = true;
						dialog.FileName = fileName;
						if (dialog.ShowDialog() == DialogResult.OK)
						{
							using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
							using (StreamWriter writer = new StreamWriter(stream))
							{
								//
								// NOTE: Should probably refactor this code to re-use
								//       between AfterSelect in TreeView and this
								//       code but for simplicity this is a copy of 
								//       some of that code.
								//       (Shawn Wildermuth - 3/24/2007)
								//

								MemoryStream memoryStream = node.Tag as MemoryStream;
								if (memoryStream != null)
								{
									memoryStream.Position = 0;

									BamlTranslator translator = new BamlTranslator(memoryStream);

									writer.Write(translator.ToString());
								}
							}
						}
					}
				}
				catch (Exception exception)
				{
					MessageBox.Show(null, exception.Message, "BAML Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			private class AssemblyNode : TreeNode
			{
				public AssemblyNode(IAssembly assembly)
				{
					this.Text = assembly.Name;
					this.ImageIndex = this.SelectedImageIndex = 0;

					foreach (IResource resource in assembly.Resources)
					{
						if (Helper.GetFileExtension(resource.Name).ToLower() == ".resources")
						{
							ResourceNode resourceNode = new ResourceNode(resource);
							if (resourceNode.IsValid())
							{
								this.Nodes.Add(resourceNode);
							}
						}
					}
				}

				public bool IsValid()
				{
					return (this.Nodes.Count > 0);
				}
			}

			private class ResourceNode : TreeNode
			{
				public ResourceNode(IResource value)
				{
					this.Text = value.Name;
					this.ImageIndex = this.SelectedImageIndex = 1;

					byte[] buffer = this.GetResourceValue(value);
					if (buffer != null)
					{
						MemoryStream stream = new MemoryStream(buffer);
						ResourceReader reader = new ResourceReader(stream);

						foreach (Resource resource in reader)
						{
							if ((Helper.GetFileExtension(resource.Name).ToLower() == ".baml") && (resource.Value is MemoryStream))
							{
								BamlNode bamlNode = new BamlNode(resource);
								this.Nodes.Add(bamlNode);
							}
						}
					}
				}

				private byte[] GetResourceValue(IResource resource)
				{
					byte[] buffer = null;

					IEmbeddedResource embeddedResource = resource as IEmbeddedResource;
					if (embeddedResource != null)
					{
						buffer = embeddedResource.Value;
					}

					IFileResource fileResource = resource as IFileResource;
					if (fileResource != null)
					{
						string location = Path.Combine(Path.GetDirectoryName(fileResource.Module.Location), fileResource.Location);
						location = Environment.ExpandEnvironmentVariables(location);
						if (File.Exists(location))
						{
							using (Stream stream = new FileStream(location, FileMode.Open, FileAccess.Read))
							{
								if (fileResource.Offset == 0)
								{
									buffer = new byte[stream.Length];
									stream.Read(buffer, 0, buffer.Length);
								}
								else
								{
									BinaryReader reader = new BinaryReader(stream);
									int size = reader.ReadInt32();
									buffer = new byte[size];
									stream.Read(buffer, 0, size);
								}
							}
						}
					}

					return buffer;
				}

				public bool IsValid()
				{
					return (this.Nodes.Count > 0);
				}
			}

			private class BamlNode : TreeNode
			{
				public BamlNode(Resource resource)
				{
					this.ImageIndex = this.SelectedImageIndex = 2;
					this.Text = resource.Name;
					this.Tag = resource.Value;
				}
			}
		}

		private class Helper
		{
			public static string GetFileExtension(string resourceName)
			{
				string fileExtension = string.Empty;
				int index = resourceName.LastIndexOf('.');
				if (index != -1)
				{
					fileExtension = resourceName.Substring(index);
				}

				return fileExtension;
			}
		}
	}
}
