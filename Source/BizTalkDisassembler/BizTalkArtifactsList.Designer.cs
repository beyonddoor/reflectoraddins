namespace BizTalkDisassembler
{
    partial class BizTalkArtifactsList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BizTalkArtifactsList));
            this.btsViewerLabel = new System.Windows.Forms.Label();
            this.decompileButton = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.artifactsListView = new System.Windows.Forms.ListView();
            this.selectColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.statusColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.kindColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.nameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.namespaceColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // btsViewerLabel
            // 
            resources.ApplyResources(this.btsViewerLabel, "btsViewerLabel");
            this.btsViewerLabel.Name = "btsViewerLabel";
            // 
            // decompileButton
            // 
            resources.ApplyResources(this.decompileButton, "decompileButton");
            this.decompileButton.Name = "decompileButton";
            this.decompileButton.UseVisualStyleBackColor = true;
            this.decompileButton.Click += new System.EventHandler(this.decompileButton_Click);
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            // 
            // artifactsListView
            // 
            this.artifactsListView.AllowColumnReorder = true;
            resources.ApplyResources(this.artifactsListView, "artifactsListView");
            this.artifactsListView.CheckBoxes = true;
            this.artifactsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.selectColumnHeader,
            this.statusColumnHeader,
            this.kindColumnHeader,
            this.nameColumnHeader,
            this.namespaceColumnHeader});
            this.artifactsListView.FullRowSelect = true;
            this.artifactsListView.Name = "artifactsListView";
            this.artifactsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.artifactsListView.UseCompatibleStateImageBehavior = false;
            this.artifactsListView.View = System.Windows.Forms.View.Details;
            this.artifactsListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.artifactsListView_ItemChecked);
            // 
            // selectColumnHeader
            // 
            resources.ApplyResources(this.selectColumnHeader, "selectColumnHeader");
            // 
            // statusColumnHeader
            // 
            resources.ApplyResources(this.statusColumnHeader, "statusColumnHeader");
            // 
            // kindColumnHeader
            // 
            resources.ApplyResources(this.kindColumnHeader, "kindColumnHeader");
            // 
            // nameColumnHeader
            // 
            resources.ApplyResources(this.nameColumnHeader, "nameColumnHeader");
            // 
            // namespaceColumnHeader
            // 
            resources.ApplyResources(this.namespaceColumnHeader, "namespaceColumnHeader");
            // 
            // BizTalkArtifactsList
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.artifactsListView);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.decompileButton);
            this.Controls.Add(this.btsViewerLabel);
            this.Name = "BizTalkArtifactsList";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label btsViewerLabel;
        private System.Windows.Forms.Button decompileButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.ListView artifactsListView;
        private System.Windows.Forms.ColumnHeader kindColumnHeader;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader namespaceColumnHeader;
        private System.Windows.Forms.ColumnHeader selectColumnHeader;
        private System.Windows.Forms.ColumnHeader statusColumnHeader;
    }
}
