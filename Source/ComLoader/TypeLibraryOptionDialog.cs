namespace Reflector.ComLoader
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;

    internal class TypeLibraryOptionDialog : Form
    {
		private Button keyPairBrowseButton = new Button();
		private Label publicKeyLabel = new Label();
		private TextBox keyPairTextBox = new TextBox();
		private Button acceptButton = new Button();
		private Label namespaceLabel = new Label();
		private Label versionLabel = new Label();
		private Label keyPairLabel = new Label();
		private TextBox publicKeyTextBox = new TextBox();
		private TextBox namespaceTextBox = new TextBox();
		private Button publicKeyBrowseButton = new Button();
		private CheckBox unsafeInterfacesCheckBox = new CheckBox();
		private CheckBox safeArrayAsSystemArrayCheckBox = new CheckBox();
		private CheckBox primaryInteropAssemblyCheckBox = new CheckBox();
		private TextBox versionTextBox = new TextBox();

		private Version version = new Version(0,0,0,0);

		public TypeLibraryOptionDialog()
		{
			this.Text = "Type Library Conversion Options";
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

			this.ClientSize = new Size(408, 253);

			this.namespaceLabel.AutoSize = true;
			this.namespaceLabel.FlatStyle = FlatStyle.System;
			this.namespaceLabel.Location = new Point(12, 15);
			this.namespaceLabel.Size = new Size(66, 13);
			this.namespaceLabel.TabIndex = 0;
			this.namespaceLabel.Text = "&Namespace:";

			this.namespaceTextBox.Location = new Point(92, 12);
			this.namespaceTextBox.Size = new Size(223, 21);
			this.namespaceTextBox.TabIndex = 1;
			
			this.versionLabel.AutoSize = true;
			this.versionLabel.FlatStyle = FlatStyle.System;
			this.versionLabel.Location = new Point(12, 42);
			this.versionLabel.Size = new Size(46, 13);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "&Version:";

			this.versionTextBox.Location = new Point(92, 39);
			this.versionTextBox.Size = new Size(111, 21);
			this.versionTextBox.TabIndex = 3;
			this.versionTextBox.Text = this.version.ToString();
			this.versionTextBox.CausesValidation = true;
			this.versionTextBox.LostFocus += new EventHandler(this.VersionTextBox_LostFocus);

			this.publicKeyLabel.AutoSize = true;
			this.publicKeyLabel.FlatStyle = FlatStyle.System;
			this.publicKeyLabel.Location = new Point(12, 69);
			this.publicKeyLabel.Size = new Size(59, 13);
			this.publicKeyLabel.TabIndex = 4;
			this.publicKeyLabel.Text = "&Public Key:";

			this.publicKeyTextBox.Location = new Point(92, 66);
			this.publicKeyTextBox.Size = new Size(223, 21);
			this.publicKeyTextBox.TabIndex = 5;

			this.publicKeyBrowseButton.FlatStyle = FlatStyle.System;
			this.publicKeyBrowseButton.Location = new Point(321, 64);
			this.publicKeyBrowseButton.Size = new Size(75, 22);
			this.publicKeyBrowseButton.TabIndex = 6;
			this.publicKeyBrowseButton.Text = "Browse...";
			this.publicKeyBrowseButton.Click += new EventHandler(this.PublicKeyBrowseButton_Click);

			this.keyPairLabel.AutoSize = true;
			this.keyPairLabel.FlatStyle = FlatStyle.System;
			this.keyPairLabel.Location = new Point(12, 96);
			this.keyPairLabel.Size = new Size(50, 13);
			this.keyPairLabel.TabIndex = 7;
			this.keyPairLabel.Text = "&Key Pair:";

			this.keyPairTextBox.Location = new Point(92, 93);
			this.keyPairTextBox.Size = new Size(223, 21);
			this.keyPairTextBox.TabIndex = 8;

			this.keyPairBrowseButton.FlatStyle = FlatStyle.System;
			this.keyPairBrowseButton.Location = new Point(321, 91);
			this.keyPairBrowseButton.Size = new Size(75, 22);
			this.keyPairBrowseButton.TabIndex = 9;
			this.keyPairBrowseButton.Text = "Browse...";
			this.keyPairBrowseButton.Click += new EventHandler(this.KeyPairBrowseButton_Click);

			this.unsafeInterfacesCheckBox.Location = new Point(92, 130);
			this.unsafeInterfacesCheckBox.Size = new Size(300, 17);
			this.unsafeInterfacesCheckBox.TabIndex = 10;
			this.unsafeInterfacesCheckBox.Text = "Produce &interfaces without runtime security checks.";
			this.unsafeInterfacesCheckBox.FlatStyle = FlatStyle.System;

			this.safeArrayAsSystemArrayCheckBox.Location = new Point(92, 130 + 23);
			this.safeArrayAsSystemArrayCheckBox.Size = new Size(225, 17);
			this.safeArrayAsSystemArrayCheckBox.TabIndex = 11;
			this.safeArrayAsSystemArrayCheckBox.Text = "Import &SAFEARRAY as System.Array.";
			this.safeArrayAsSystemArrayCheckBox.FlatStyle = FlatStyle.System;

			this.primaryInteropAssemblyCheckBox.Location = new Point(92, 130 + 23 + 23);
			this.primaryInteropAssemblyCheckBox.Size = new Size(225, 17);
			this.primaryInteropAssemblyCheckBox.TabIndex = 12;
			this.primaryInteropAssemblyCheckBox.Text = "Produce a primary interop &assembly.";
			this.primaryInteropAssemblyCheckBox.FlatStyle = FlatStyle.System;

			this.acceptButton.FlatStyle = FlatStyle.System;
			this.acceptButton.Location = new Point(321, 218);
			this.acceptButton.Size = new Size(75, 23);
			this.acceptButton.TabIndex = 50;
			this.acceptButton.Text = "&Close";
			this.acceptButton.DialogResult = DialogResult.OK;
			this.AcceptButton = this.acceptButton;
			this.CancelButton = this.acceptButton;

			this.Controls.Add(this.versionTextBox);
			this.Controls.Add(this.unsafeInterfacesCheckBox);
			this.Controls.Add(this.primaryInteropAssemblyCheckBox);
			this.Controls.Add(this.safeArrayAsSystemArrayCheckBox);
			this.Controls.Add(this.publicKeyBrowseButton);
			this.Controls.Add(this.namespaceTextBox);
			this.Controls.Add(this.publicKeyTextBox);
			this.Controls.Add(this.keyPairLabel);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.namespaceLabel);
			this.Controls.Add(this.keyPairTextBox);
			this.Controls.Add(this.publicKeyLabel);
			this.Controls.Add(this.keyPairBrowseButton);
			this.Controls.Add(this.acceptButton);
		}

		public string PublicKey
		{
			get { return this.publicKeyTextBox.Text; }
		}

		public string KeyPair
		{
			get { return this.keyPairTextBox.Text; }
		}

		public Version AssemblyVersion
		{
			get { return this.version; }
		}

		public string AssemblyNamespace
		{
			get { return this.namespaceTextBox.Text; }
		}

		public bool UnsafeInterfaces
		{
			get { return this.unsafeInterfacesCheckBox.Checked; }
		}

		public bool SafeArrayAsSystemArray
		{
			get { return this.safeArrayAsSystemArrayCheckBox.Checked; }
		}

		public bool PrimaryInteropAssembly
		{
			get { return this.primaryInteropAssemblyCheckBox.Checked; }
		}

		private void KeyPairBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Open";
			dialog.Multiselect = false;
			dialog.Filter = "Strong Name Key files (*.snk)|*.snk|All files (*.*)|*.*";
			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				this.keyPairTextBox.Text = dialog.FileName;
			}
		}

		private void PublicKeyBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Title = "Open";
			dialog.Multiselect = false;
			dialog.Filter = "Strong Name Key files (*.snk)|*.snk|All files (*.*)|*.*";
			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				this.publicKeyTextBox.Text = dialog.FileName;
			}
		}

		private void VersionTextBox_LostFocus(object sender, EventArgs e)
		{
			try
			{
				this.version = new Version(this.versionTextBox.Text);
			}
			catch
			{
				this.versionTextBox.Text = this.version.ToString();
			}
		}
	}
}
