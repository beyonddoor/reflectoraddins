namespace Reflector.ComLoader
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;
	using Microsoft.Win32;
	using Reflector;

    internal class TypeLibraryDialog : Form
    {
		private ComboBox typeLibraryComboBox = new ComboBox();
		private Button browseButton = new Button();
		private Label typeLibraryLabel = new Label();
		private Label assemblyLabel = new Label();
		private TextBox assemblyTextBox = new TextBox();
		private Button acceptButton = new Button();
		private Button cancelButton = new Button();
		private Button optionsButton = new Button();

		private TextBox logTextBox = new TextBox();
		private TypeLibraryConverter typeLibraryConverter;
		private delegate void MessageCallback(string value);
		private delegate void CompleteCallback();

		private TypeLibraryOptionDialog optionDialog = new TypeLibraryOptionDialog();

		public TypeLibraryDialog()
		{
			this.Text = "Open Type Library";
			this.Icon = null;
			this.Font = new Font("Tahoma", 8.25f);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.ControlBox = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.StartPosition = FormStartPosition.CenterParent;
			this.AutoScale = true;
			this.AutoScaleBaseSize = new Size(5, 14);

			this.ClientSize = new Size(408, 153);

			this.typeLibraryLabel.AutoSize = true;
			this.typeLibraryLabel.FlatStyle = FlatStyle.System;
			this.typeLibraryLabel.Location = new Point(12, 12);
			this.typeLibraryLabel.Size = new Size(97, 13);
			this.typeLibraryLabel.TabIndex = 0;
			this.typeLibraryLabel.Text = "COM/OLE &Type Library:";

			this.typeLibraryComboBox.DropDownWidth = 339;
			this.typeLibraryComboBox.Location = new Point(12, 28);
			this.typeLibraryComboBox.Size = new Size(303, 21);
			this.typeLibraryComboBox.TabIndex = 1;
			this.typeLibraryComboBox.DropDown += new EventHandler(this.TypeLibraryComboBox_DropDown);
			this.typeLibraryComboBox.TextChanged += new EventHandler(this.TypeLibraryComboBox_TextChanged);
			this.typeLibraryComboBox.SelectedIndexChanged += new EventHandler(this.TypeLibraryComboBox_SelectedIndexChanged);

			this.browseButton.FlatStyle = FlatStyle.System;
			this.browseButton.Location = new Point(321, 28);
			this.browseButton.Size = new Size(75, 22);
			this.browseButton.TabIndex = 2;
			this.browseButton.Text = "&Browse...";
			this.browseButton.Click += new EventHandler(this.BrowseButton_Click);

			this.assemblyLabel.AutoSize = true;
			this.assemblyLabel.FlatStyle = FlatStyle.System;
			this.assemblyLabel.Location = new Point(12, 63);
			this.assemblyLabel.Size = new Size(82, 13);
			this.assemblyLabel.TabIndex = 3;
			this.assemblyLabel.Text = ".NET Interop &Assembly:";

			this.assemblyTextBox.Location = new Point(12, 79);
			this.assemblyTextBox.Size = new Size(408 - 12 - 12, 21);
			this.assemblyTextBox.TabIndex = 4;
			this.assemblyTextBox.TextChanged += new EventHandler(this.AssemblyTextBox_TextChanged);

			this.optionsButton.FlatStyle = FlatStyle.System;
			this.optionsButton.Location = new Point(12, 119);
			this.optionsButton.Size = new Size(75, 22);
			this.optionsButton.TabIndex = 5;
			this.optionsButton.Text = "&Options...";
			this.optionsButton.Click += new EventHandler(this.OptionsButton_Click);

			this.acceptButton.FlatStyle = FlatStyle.System;
			this.acceptButton.Location = new Point(240, 118);
			this.acceptButton.Size = new Size(75, 23);
			this.acceptButton.TabIndex = 6;
			this.acceptButton.Text = "Import";
			this.acceptButton.Click += new EventHandler(this.ImportButton_Click);
			this.AcceptButton = this.acceptButton;

			this.cancelButton.FlatStyle = FlatStyle.System;
			this.cancelButton.Location = new Point(321, 118);
			this.cancelButton.Size = new Size(75, 23);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.CancelButton = this.cancelButton;

			this.Controls.Add(this.optionsButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.acceptButton);
			this.Controls.Add(this.assemblyTextBox);
			this.Controls.Add(this.assemblyLabel);
			this.Controls.Add(this.typeLibraryLabel);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.typeLibraryComboBox);

			this.UpdateButtonState();
		}

		public string TypeLibraryLocation
		{
			get
			{
				if (this.typeLibraryComboBox.SelectedItem == null)
				{
					return this.typeLibraryComboBox.Text;
				}

				TypeLibraryItem item = (TypeLibraryItem)this.typeLibraryComboBox.SelectedItem;
				return item.Location;
			}
		}

		public string AssemblyLocation
		{
			get
			{
				return this.assemblyTextBox.Text;
			}
		}

		private void OptionsButton_Click(object sender, EventArgs e)
		{
			this.optionDialog.ShowDialog(this);
		}

		private void ImportButton_Click(object sender, EventArgs e)
		{
			string typeLibraryLocation = Environment.ExpandEnvironmentVariables(this.TypeLibraryLocation);
			string assemblyLocation = Environment.ExpandEnvironmentVariables(this.AssemblyLocation);

			if (File.Exists(assemblyLocation))
			{
				if (MessageBox.Show(string.Format("File '{0}' exists. Do you want to overwrite?", assemblyLocation), "Silverlight Loader Add-In", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					return;
				}

				File.Delete(assemblyLocation);
			}

			this.acceptButton.Click -= new EventHandler(this.ImportButton_Click);
			this.acceptButton.Enabled = false;
			this.cancelButton.Enabled = false;

			this.Controls.Remove(this.optionsButton);
			this.Controls.Remove(this.assemblyTextBox);
			this.Controls.Remove(this.assemblyLabel);
			this.Controls.Remove(this.typeLibraryLabel);
			this.Controls.Remove(this.browseButton);
			this.Controls.Remove(this.typeLibraryComboBox);

			this.logTextBox.Location = new Point(12, 12);
			this.logTextBox.Size = new Size(408 - 12 - 12, 90);
			this.logTextBox.ReadOnly = true;
			this.logTextBox.Multiline = true;
			this.logTextBox.MaxLength = 1024 * 1024 * 8;
			this.logTextBox.ScrollBars = ScrollBars.Vertical;
			this.logTextBox.WordWrap = false;
			this.Controls.Add(this.logTextBox);

			this.typeLibraryConverter = new TypeLibraryConverter();
			this.typeLibraryConverter.AssemblyLocation = assemblyLocation;
			this.typeLibraryConverter.TypeLibraryLocation = typeLibraryLocation;
			this.typeLibraryConverter.AssemblyNamespace = this.optionDialog.AssemblyNamespace;
			this.typeLibraryConverter.AssemblyVersion = this.optionDialog.AssemblyVersion;
			this.typeLibraryConverter.PrimaryInteropAssembly = this.optionDialog.PrimaryInteropAssembly;
			this.typeLibraryConverter.UnsafeInterfaces = this.optionDialog.UnsafeInterfaces;
			this.typeLibraryConverter.SafeArrayAsSystemArray = this.optionDialog.SafeArrayAsSystemArray;

			// Read Public Key
			if (File.Exists(this.optionDialog.PublicKey))
			{
				using (FileStream stream = File.OpenRead(this.optionDialog.PublicKey))
				{
					byte[] buffer = new byte[(int) stream.Length];
					stream.Read(buffer, 0, buffer.Length);
					this.typeLibraryConverter.PublicKey = buffer;
				}
			}

			// Read Key Pair
			if (File.Exists(this.optionDialog.KeyPair))
			{
				using (FileStream stream = File.OpenRead(this.optionDialog.KeyPair))
				{
					this.typeLibraryConverter.KeyPair = new StrongNameKeyPair(stream);
				}
			}

			this.typeLibraryConverter.Message += new TextEventHandler(this.TypeLibraryConverter_Message);

			MethodInvoker invoker = new MethodInvoker(this.AsyncConvert);
			invoker.BeginInvoke(null, null);
		}

		private void AsyncConvert()
		{
			try
			{
				this.typeLibraryConverter.Start();
				this.BeginInvoke(new CompleteCallback(this.AsyncComplete), new object[0]);
			}
			catch (Exception exception)
			{
				this.BeginInvoke(new MessageCallback(this.AsyncMessage), new object[] { exception.Message });
			}
		}

		private void TypeLibraryConverter_Message(object sender, TextEventArgs e)
		{
			this.BeginInvoke(new MessageCallback(this.AsyncMessage), new object[] { e.Text });
		}

		private void AsyncMessage(string value)
		{
			this.logTextBox.AppendText(value + "\r\n");
            this.logTextBox.ScrollToCaret();
            System.Windows.Forms.Application.DoEvents();
		}

		private void AsyncComplete()
		{
			this.cancelButton.Enabled = true;

			if (File.Exists(Environment.ExpandEnvironmentVariables(this.AssemblyLocation)))
			{
				this.acceptButton.Text = "&Load";
				this.acceptButton.Click += new EventHandler(this.LoadButton_Click);
				this.acceptButton.Enabled = true;
			}
		}

		private void LoadButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void TypeLibraryComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.UpdateAssemblyLocation();
			this.UpdateButtonState();
		}

		private void TypeLibraryComboBox_TextChanged(object sender, EventArgs e)
		{
			this.UpdateAssemblyLocation();
			this.UpdateButtonState();
		}

		private void AssemblyTextBox_TextChanged(object sender, EventArgs e)
		{
			this.UpdateButtonState();
		}

		private void TypeLibraryComboBox_DropDown(object sender, EventArgs e)
		{
			if (this.typeLibraryComboBox.Items.Count == 0)
			{
				this.PopulateTypeLibraryList();
			}
		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Open";
			dialog.Filter = "Type Library files (*.exe,*.dll,*.tlb)|*.dll;*.exe;*.tlb|All files (*.*)|*.*";

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				this.typeLibraryComboBox.Text = dialog.FileName;
			}
		}

		private void PopulateTypeLibraryList()
		{
			using (RegistryKey listKey = Registry.ClassesRoot.OpenSubKey("TypeLib"))
			{
				foreach (string guid in listKey.GetSubKeyNames())
				{
					using (RegistryKey libraryKey = listKey.OpenSubKey(guid))
					{
						foreach (string version in libraryKey.GetSubKeyNames())
						{
							using (RegistryKey versionKey = libraryKey.OpenSubKey(version))
							{
								string name = (string)versionKey.GetValue(null);
								if ((name != null) && (name.Length > 0))
								{
									foreach (string sv in versionKey.GetSubKeyNames())
									{
										if ((sv != "FLAGS") && (sv != "HELPDIR"))
										{
											using (RegistryKey locationKey = versionKey.OpenSubKey(sv + @"\win32"))
											{
												if (locationKey != null)
												{
													string location = (string)locationKey.GetValue(null);

													TypeLibraryItem item = new TypeLibraryItem(name, version, location);
													this.typeLibraryComboBox.Items.Add(item);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			ArrayList list = new ArrayList();
			list.AddRange(this.typeLibraryComboBox.Items);
			list.Sort();
			this.typeLibraryComboBox.Items.Clear();
			this.typeLibraryComboBox.Items.AddRange(list.ToArray());
		}

		private ToolTip toolTip = new ToolTip();

		private void UpdateAssemblyLocation()
		{
			this.toolTip.RemoveAll();

			TypeLibraryConverter converter = new TypeLibraryConverter();

			if (this.typeLibraryComboBox.SelectedItem != null)
			{
				TypeLibraryItem item = (TypeLibraryItem)this.typeLibraryComboBox.SelectedItem;
				toolTip.SetToolTip(this.typeLibraryComboBox, item.Location);

				converter.TypeLibraryLocation = item.Location;
			}
			else
			{
				converter.TypeLibraryLocation = this.typeLibraryComboBox.Text;
			}

			string fileName = "Interop." + converter.AssemblyLocation;

			string outputPath = null;

			if (this.assemblyTextBox.Text.Length > 0)
			{
				outputPath = Path.GetDirectoryName(this.assemblyTextBox.Text);
			}
			else
			{
				outputPath = this.OutputPath;
			}

			this.assemblyTextBox.Text = Path.Combine(outputPath, fileName);
		}

		private void UpdateButtonState()
		{
			this.acceptButton.Enabled = ((this.TypeLibraryLocation.Length > 0) && (this.AssemblyLocation.Length > 0));
		}

		private string OutputPath
		{
			get
			{
				string userProfile = Environment.ExpandEnvironmentVariables("%UserProfile%");
				string documentRoot = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

				if (documentRoot.StartsWith(userProfile))
				{
					documentRoot = "%UserProfile%" + documentRoot.Substring(userProfile.Length);
				}

				string outputPath = documentRoot;
				outputPath = Path.Combine(outputPath, "Reflector");
				outputPath = Path.Combine(outputPath, "Interop");
				return outputPath;
			}
		}

		internal class TypeLibraryItem : IComparable
		{
			private string name;
			private string version;
			private string location;

			public TypeLibraryItem(string name, string version, string location)
			{
				this.name = name;
				this.version = version;
				this.location = location;
			}

			public string Name
			{
				get { return this.name; }
			}

			public string Version
			{
				get { return this.version; }
			}

			public string Location
			{
				get { return this.location; }
			}

			public int CompareTo(object value)
			{
				return this.ToString().CompareTo(value.ToString());
			}

			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.Name, this.Version);
			}
		}
	}
}
