namespace BizTalkDisassembler
{
    partial class ProgressDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
            this.decompileButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusRichTextBox = new System.Windows.Forms.RichTextBox();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.exportToLabel = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // decompileButton
            // 
            resources.ApplyResources(this.decompileButton, "decompileButton");
            this.decompileButton.Name = "decompileButton";
            this.decompileButton.UseVisualStyleBackColor = true;
            this.decompileButton.Click += new System.EventHandler(this.decompileButton_Click);
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            // 
            // statusRichTextBox
            // 
            this.statusRichTextBox.AcceptsTab = true;
            resources.ApplyResources(this.statusRichTextBox, "statusRichTextBox");
            this.statusRichTextBox.Name = "statusRichTextBox";
            this.statusRichTextBox.ReadOnly = true;
            // 
            // pathTextBox
            // 
            resources.ApplyResources(this.pathTextBox, "pathTextBox");
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.TextChanged += new System.EventHandler(this.pathTextBox_TextChanged);
            // 
            // exportToLabel
            // 
            resources.ApplyResources(this.exportToLabel, "exportToLabel");
            this.exportToLabel.Name = "exportToLabel";
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ProgressDialog
            // 
            this.AcceptButton = this.decompileButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.exportToLabel);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.statusRichTextBox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.decompileButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ProgressDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button decompileButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox statusRichTextBox;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Label exportToLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button cancelButton;
    }
}