using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BizTalkDisassembler
{
    /// <summary>
    /// Export artifacts to disk and provides a progress dialog.
    /// </summary>
    internal partial class ProgressDialog : Form
    {
        #region Class ExportWorkItem

        private class ExportWorkItem
        {
            public ProgressDialog ParentDialog;
            public string BasePath;
            public List<DecompiledArtifact> Artifacts;

            public ExportWorkItem(ProgressDialog parentDialog, string basePath, List<DecompiledArtifact> artifacts)
            {
                ParentDialog = parentDialog;
                BasePath = basePath;
                Artifacts = artifacts;
            }
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Various delegate declaration so the worker thread can update the User Interface.
        /// </summary>
        private delegate void OnNotifyArtifactExportBegin(List<DecompiledArtifact> artifacts);
        private delegate void OnNotifyArtifactExported(string path, DecompiledArtifact artifact);
        private delegate void OnNotifyExportError(Exception e, DecompiledArtifact artifact);
        private delegate void OnNotifyArtifactExportEnd(bool canceled);

        #endregion

        /// <summary>
        /// List of artifacts to decompile.
        /// </summary>
        private List<DecompiledArtifact> artifacts;

        /// <summary>
        /// True if we are currently exporting, false otherwise.
        /// </summary>
        private bool isExporting;

        /// <summary>
        /// Event used to trigger the "Cancel" during export.
        /// </summary>
        private static AutoResetEvent CancelEvent = new AutoResetEvent(false);


        /// <summary>
        /// Gets or sets the list of artifacts to export.
        /// </summary>
        private List<DecompiledArtifact> ArtifactsToExport
        {
            get { return artifacts;  }
            set { artifacts = value; }
        }

        /// <summary>
        /// Gets or sets the status of export.
        /// </summary>
        private bool IsExporting
        {
            get { return isExporting;  }
            set { isExporting = value; }
        }



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="artifacts">List of artifacts to export.</param>
        public ProgressDialog(List<DecompiledArtifact> artifacts)
        {
            ArtifactsToExport = artifacts;
            InitializeComponent();
        }

        /// <summary>
        /// Handle the Click event on the "Browse..." button.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void browseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog browserDialog = new FolderBrowserDialog())
            {
                // Configure the Folder Browser dialog
                browserDialog.Description = Resources.SelectExportDestinationPath;
                browserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                browserDialog.ShowNewFolderButton = true;

                // Show the dialog
                DialogResult result = browserDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Extract the path and copy it into the text box
                    pathTextBox.Text = browserDialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Handle the TextChanged event on the "Path" text box.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void pathTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox pathTextBox = sender as TextBox;
            if (pathTextBox != null)
            {
                // Perform a very basic validation check on the path
                string currentPath = pathTextBox.Text.Trim();
                bool valid = Path.IsPathRooted(currentPath) &&
                             !String.IsNullOrEmpty(currentPath) &&
                             (currentPath.IndexOfAny(Path.GetInvalidPathChars()) == -1);

                // Enable the decompile button as appropriate
                decompileButton.Enabled = valid;
            }
        }

        /// <summary>
        /// Handle the Click event on the "Decompile" button.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void decompileButton_Click(object sender, EventArgs e)
        {
            // Get the path where we should export all files
            string basePath = pathTextBox.Text.Trim();
            basePath = Path.GetFullPath(basePath);

            // Use the thread pool to run the export
            WaitCallback exporterCallback = new WaitCallback(WorkerThreadStart);
            ExportWorkItem workItem = new ExportWorkItem(this, basePath, ArtifactsToExport);
            ThreadPool.QueueUserWorkItem(exporterCallback, workItem);
        }

        /// <summary>
        /// Handle the Click event on the "Cancel" button.
        /// </summary>
        /// <param name="sender">Object sending this event.</param>
        /// <param name="e">Parameter associated with this event.</param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            // If we are exporting, signal our Cancel event
            if (IsExporting)
            {
                // Send the cancel signal to our thread
                CancelEvent.Set();
            }
            else
            {
                // Regular "Cancel" (aka not during export operation)
                Close();
            }
        }

        /// <summary>
        /// Notify the user that we started to export artifacts.
        /// </summary>
        /// <param name="artifacts">List of artifacts being exported.</param>
        private void NotifyArtifactExportBegin(List<DecompiledArtifact> artifacts)
        {
            // Disable adequate controls in UI
            decompileButton.Enabled = false;
            browseButton.Enabled = false;
            pathTextBox.Enabled = false;
            statusRichTextBox.Enabled = false;

            // Clear the feedback area
            ClearFeedbackArea();

            // Configure the progress bar
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Maximum = artifacts.Count;
            progressBar.Minimum = 0;
            progressBar.Step = 1;
            progressBar.Value = 0;

            // Emit a startup text to the feedback area
            AppendMessageToFeedbackArea(Resources.ExportStarted, System.Diagnostics.TraceLevel.Info);
        }

        /// <summary>
        /// Notify the feedback area that we exported an artifact.
        /// </summary>
        /// <param name="outPath">Destination to where the artifact was exported.</param>
        /// <param name="artifact">Artifact exported.</param>
        private void NotifyArtifactExported(string outPath, DecompiledArtifact artifact)
        {
            // Update the progress bar
            progressBar.Increment(1);

            // Emit a message to the feedback area
            string message = String.Format(CultureInfo.CurrentCulture,
                                           Resources.ExportedArtifact,
                                           new object[] {
                                               artifact.ArtifactKind,
                                               artifact.Namespace + "." + artifact.Name,
                                               outPath
                                           });
            AppendMessageToFeedbackArea(message, System.Diagnostics.TraceLevel.Info);
        }

        /// <summary>
        /// Notify the feedback area of an error during the export process.
        /// </summary>
        /// <param name="e">Exception.</param>
        /// <param name="artifact">Artifact being exported.</param>
        private void NotifyExportError(Exception e, DecompiledArtifact artifact)
        {
            // Update the progress bar
            progressBar.Increment(1);

            // Emit an error message to the feedback area
            string message = String.Format(CultureInfo.CurrentCulture,
                                           Resources.ErrorExportedArtifact,
                                           new object[] {
                                               artifact.ArtifactKind,
                                               artifact.Namespace + "." + artifact.Name,
                                               e.ToString()
                                           });
            AppendMessageToFeedbackArea(message, System.Diagnostics.TraceLevel.Error);
        }

        /// <summary>
        /// Notify the user that we finished exporting artifacts.
        /// </summary>
        /// <param name="canceled">true if the export was canceled, false otherwise.</param>
        private void NotifyArtifactExportEnd(bool canceled)
        {
            // Emit an end text to the feedback area
            AppendMessageToFeedbackArea(canceled ? Resources.ExportCanceled : Resources.ExportDone, System.Diagnostics.TraceLevel.Info);

            // Enable adequate controls in the UI
            decompileButton.Enabled = true;
            browseButton.Enabled = true;
            pathTextBox.Enabled = true;
            statusRichTextBox.Enabled = true;
        }

        /// <summary>
        /// Clears the content of the feedback area.
        /// </summary>
        private void ClearFeedbackArea()
        {
            statusRichTextBox.Text = String.Empty;
        }

        /// <summary>
        /// Appends a message with the right formatting to the feedbacka area.
        /// </summary>
        /// <param name="message">Message to emit.</param>
        /// <param name="style">Style for the message.</param>
        private void AppendMessageToFeedbackArea(string message, System.Diagnostics.TraceLevel style)
        {
            // Format the text we just added depending on the reporting level
            switch (style)
            {
                case System.Diagnostics.TraceLevel.Error:
                    statusRichTextBox.SelectionColor = System.Drawing.Color.Red;
                    break;

                default:
                    break;
            }

            // Emit the text
            statusRichTextBox.SelectedText = message + "\n";
        }

        /// <summary>
        /// Export worker thread main entry point.
        /// </summary>
        /// <param name="parameters">Parameters passed to the worker thread (as an ExportWorkItem).</param>
        private static void WorkerThreadStart(object parameters)
        {
            bool wasCancelled = false;

            // Extract arguments passed to the thread
            ExportWorkItem workItem = parameters as ExportWorkItem;

            // We are exporting
            workItem.ParentDialog.IsExporting = true;

            try
            {
                // We start exporting artifacts
                workItem.ParentDialog.Invoke(new OnNotifyArtifactExportBegin(workItem.ParentDialog.NotifyArtifactExportBegin), new object[] { workItem.Artifacts });

                // Export all artifacts
                foreach (DecompiledArtifact artifact in workItem.Artifacts)
                {
                    // Are we asked to cancel?
                    if (CancelEvent.WaitOne(0, false))
                    {
                        wasCancelled = true;
                        break;
                    }

                    try
                    {
                        // Ensure the directory hierachy exists and export the artifact, formatting it
                        string path = ComputeAndEnsureOutputPathHierachy(workItem.BasePath, artifact);
                        ExportFormattedXml(path, artifact);

                        System.Threading.Thread.Sleep(2000);

                        // Provide visual feedback to user
                        workItem.ParentDialog.Invoke(new OnNotifyArtifactExported(workItem.ParentDialog.NotifyArtifactExported), new object[] { path, artifact });
                    }
                    catch (Exception exp)
                    {
                        workItem.ParentDialog.Invoke(new OnNotifyExportError(workItem.ParentDialog.NotifyExportError), new object[] { exp, artifact });
                    }
                }

                // We are done exporting artifacts
                workItem.ParentDialog.Invoke(new OnNotifyArtifactExportEnd(workItem.ParentDialog.NotifyArtifactExportEnd), new object[] { wasCancelled });
            }
            finally
            {
                // We are not exporting anymore
                workItem.ParentDialog.IsExporting = false;
            }
        }

        /// <summary>
        /// Get the file extension associated with a given artifact.
        /// </summary>
        /// <param name="artifact">Artifact to consider when computing the extension.</param>
        /// <returns>Extension, as a string.</returns>
        private static string GetExtension(DecompiledArtifact artifact)
        {
            switch (artifact.ArtifactKind)
            {
                case Constants.BizTalkArtifactType.Map:
                    return "xslt";
                case Constants.BizTalkArtifactType.Orchestration:
                    return "odx";
                case Constants.BizTalkArtifactType.ReceivePipeline:
                case Constants.BizTalkArtifactType.SendPipeline:
                    return "btp";
                case Constants.BizTalkArtifactType.Schema:
                    return "xsd";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Compute the destination path (a set of nested directories to mimic namespaces).
        /// The hierachy is also created on disk if it does not exist.
        /// </summary>
        /// <param name="basePath">Base path to start from.</param>
        /// <param name="artifact">Artifact to compute the export path for.</param>
        /// <returns>Path to the directory the artifact should be exported.</returns>
        private static string ComputeAndEnsureOutputPathHierachy(string basePath, DecompiledArtifact artifact)
        {
            // Compute the destiantion path (we start with the base path)
            string destinationPath = basePath;

            // Make sure the base path is available
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            // Create directories as needed to mimic the namespace structure
            if (!String.IsNullOrEmpty(artifact.Namespace))
            {
                string[] namespaces = artifact.Namespace.Split(new char[] { ',' });
                if (namespaces != null)
                {
                    foreach (string segment in namespaces)
                    {
                        destinationPath = Path.Combine(destinationPath, segment);
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                    }
                }
            }

            return destinationPath;
        }

        /// <summary>
        /// Export the given artifact to a file under the given path.
        /// </summary>
        /// <param name="path">Path to export the artifact to.</param>
        /// <param name="artifact">Artifact to export.</param>
        private static void ExportFormattedXml(string path, DecompiledArtifact artifact)
        {
            // Construct the output file path
            string filePath = Path.ChangeExtension(Path.Combine(path, artifact.Name), GetExtension(artifact));

            // Delete the file if it already exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Write the Xml and format it at the same time
            using (XmlTextWriter xmlWriter = new XmlTextWriter(filePath, System.Text.Encoding.UTF8))
            {
                XmlDocument artifactDOM = new XmlDocument();
                artifactDOM.LoadXml(artifact.ArtifactValue);

                // Configure the writer so it formats output
                xmlWriter.Formatting = Formatting.Indented;

                // Write to the output file
                artifactDOM.WriteTo(xmlWriter);
            }
        }
    }
}