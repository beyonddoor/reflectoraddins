using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Displays a list of BizTalk artifacts in the Reflector window.
    /// </summary>
    internal partial class BizTalkArtifactsList : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public BizTalkArtifactsList()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Displays the given list of artifacts in the BizTalk artifact list view.
        /// </summary>
        /// <param name="artifacts">List of artifacts to display.</param>
        /// <remarks>The list of artifacts to display may be null indicating no artifact.</remarks>
        public void Display(List<DecompiledArtifact> artifacts)
        {
            artifactsListView.BeginUpdate();

            // Clear the list
            artifactsListView.Items.Clear();
            if (artifacts != null)
            {
                // Insert all artifacts into the list
                foreach (DecompiledArtifact artifact in artifacts)
                {
                    // Prepare the ListViewItem for this artifact
                    ListViewItem lvi = new ListViewItem();

                    // Artifact "Status" (aka can we decompile without loss or not?)
                    // Currently, we cannot decompile maps - The original mapping is not in the assembly
                    ListViewItem.ListViewSubItem statusSubItem = new ListViewItem.ListViewSubItem();
                    statusSubItem.Text = artifact.ArtifactKind == Constants.BizTalkArtifactType.Map ? Resources.DecompileAsXSLT : Resources.DecompileAsSource;
                    lvi.SubItems.Add(statusSubItem);

                    // Artifact Kind
                    ListViewItem.ListViewSubItem kindSubItem = new ListViewItem.ListViewSubItem();
                    kindSubItem.Text = artifact.ArtifactKind.ToString();
                    lvi.SubItems.Add(kindSubItem);

                    // Artifact Name
                    ListViewItem.ListViewSubItem nameSubItem = new ListViewItem.ListViewSubItem();
                    nameSubItem.Text = artifact.Name;
                    lvi.SubItems.Add(nameSubItem);

                    // Artifact Namespace
                    ListViewItem.ListViewSubItem nsSubItem = new ListViewItem.ListViewSubItem();
                    nsSubItem.Text = artifact.Namespace;
                    lvi.SubItems.Add(nsSubItem);

                    // Bind the item to the internal artifact object
                    lvi.Tag = artifact;
                    lvi.Checked = true;

                    // Insert a new item into the list view
                    artifactsListView.Items.Add(lvi);
                }
            }

            artifactsListView.EndUpdate();
        }

        /// <summary>
        /// Handles the ItemChecked event fired by the artifacts list view.
        /// </summary>
        /// <param name="sender">Object sending this message.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void artifactsListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListView listView = sender as ListView;
            decompileButton.Enabled = listView.CheckedItems.Count > 0;
        }

        /// <summary>
        /// Handles the Click event fired by the "Decompile..." button.
        /// </summary>
        /// <param name="sender">Object sending this message.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void decompileButton_Click(object sender, EventArgs e)
        {
            // Compile the list of artifacts to export ()
            List<DecompiledArtifact> artifactsToDecompile = new List<DecompiledArtifact>();
            foreach (ListViewItem lvi in artifactsListView.CheckedItems)
            {
                DecompiledArtifact artifact = lvi.Tag as DecompiledArtifact;
                if (artifact != null)
                {
                    artifactsToDecompile.Add(artifact);
                }
            }

            // Delegate the work to a popup dialog
            if (artifactsToDecompile.Count > 0)
            {
                using (ProgressDialog dialog = new ProgressDialog(artifactsToDecompile))
                {
                    dialog.ShowDialog();
                }
            }
        }
    }
}
