namespace BHVEditor
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnOpenBHV;
        private System.Windows.Forms.Button btnExportJSON;
        private System.Windows.Forms.Button btnImportJSON;
        private System.Windows.Forms.Button btnSaveBHV;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Panel panelContainer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnOpenBHV = new System.Windows.Forms.Button();
            this.btnExportJSON = new System.Windows.Forms.Button();
            this.btnImportJSON = new System.Windows.Forms.Button();
            this.btnSaveBHV = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panelContainer = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // btnOpenBHV
            // 
            this.btnOpenBHV.Location = new System.Drawing.Point(12, 12);
            this.btnOpenBHV.Size = new System.Drawing.Size(90, 30);
            this.btnOpenBHV.Text = "Open BHV";
            // 
            // btnExportJSON
            // 
            this.btnExportJSON.Location = new System.Drawing.Point(108, 12);
            this.btnExportJSON.Size = new System.Drawing.Size(90, 30);
            this.btnExportJSON.Text = "Export JSON";
            // 
            // btnImportJSON
            // 
            this.btnImportJSON.Location = new System.Drawing.Point(204, 12);
            this.btnImportJSON.Size = new System.Drawing.Size(90, 30);
            this.btnImportJSON.Text = "Import JSON";
            // 
            // btnSaveBHV
            // 
            this.btnSaveBHV.Location = new System.Drawing.Point(300, 12);
            this.btnSaveBHV.Size = new System.Drawing.Size(90, 30);
            this.btnSaveBHV.Text = "Save BHV";
            // 
            // panelContainer
            // 
            this.panelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)
                ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContainer.Location = new System.Drawing.Point(12, 50);
            this.panelContainer.Size = new System.Drawing.Size(760, 500);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.btnOpenBHV);
            this.Controls.Add(this.btnExportJSON);
            this.Controls.Add(this.btnImportJSON);
            this.Controls.Add(this.btnSaveBHV);
            this.Controls.Add(this.panelContainer);
            this.Text = "BHV Editor";
            this.ResumeLayout(false);

        }
    }
}