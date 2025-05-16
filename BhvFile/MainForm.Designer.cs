namespace BHVEditor

{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnOpenBHV;
        private System.Windows.Forms.Button btnExportJSON;
        private System.Windows.Forms.Button btnImportJSON;
        private System.Windows.Forms.Button btnSaveBHV;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListView listViewFields;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.OpenFileDialog openJsonFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveBhvFileDialog;

        private void InitializeComponent()
        {
            this.btnOpenBHV = new System.Windows.Forms.Button();
            this.btnExportJSON = new System.Windows.Forms.Button();
            this.btnImportJSON = new System.Windows.Forms.Button();
            this.btnSaveBHV = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.listViewFields = new System.Windows.Forms.ListView();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderValue = new System.Windows.Forms.ColumnHeader();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openJsonFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.saveBhvFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // btnOpenBHV
            // 
            this.btnOpenBHV.Location = new System.Drawing.Point(12, 12);
            this.btnOpenBHV.Size = new System.Drawing.Size(90, 30);
            this.btnOpenBHV.Text = "Open BHV";
            this.btnOpenBHV.Click += new System.EventHandler(this.btnOpenBHV_Click);
            // 
            // btnExportJSON
            // 
            this.btnExportJSON.Location = new System.Drawing.Point(108, 12);
            this.btnExportJSON.Size = new System.Drawing.Size(90, 30);
            this.btnExportJSON.Text = "Export JSON";
            this.btnExportJSON.Click += new System.EventHandler(this.btnExportJSON_Click);
            // 
            // btnImportJSON
            // 
            this.btnImportJSON.Location = new System.Drawing.Point(204, 12);
            this.btnImportJSON.Size = new System.Drawing.Size(90, 30);
            this.btnImportJSON.Text = "Import JSON";
            this.btnImportJSON.Click += new System.EventHandler(this.btnImportJSON_Click);
            // 
            // btnSaveBHV
            // 
            this.btnSaveBHV.Location = new System.Drawing.Point(300, 12);
            this.btnSaveBHV.Size = new System.Drawing.Size(90, 30);
            this.btnSaveBHV.Text = "Save BHV";
            this.btnSaveBHV.Click += new System.EventHandler(this.btnSaveBHV_Click);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(12, 50);
            this.treeView1.Size = new System.Drawing.Size(300, 400);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // listViewFields
            // 
            this.listViewFields.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderValue});
            this.listViewFields.Location = new System.Drawing.Point(318, 50);
            this.listViewFields.Size = new System.Drawing.Size(470, 400);
            this.listViewFields.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Field";
            this.columnHeaderName.Width = 200;
            // 
            // columnHeaderValue
            // 
            this.columnHeaderValue.Text = "Value";
            this.columnHeaderValue.Width = 260;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "BHV Files (*.bhv)|*.bhv|All Files|*.*";
            // 
            // openJsonFileDialog
            // 
            this.openJsonFileDialog.Filter = "JSON Files (*.json)|*.json|All Files|*.*";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "JSON Files (*.json)|*.json|All Files|*.*";
            // 
            // saveBhvFileDialog
            // 
            this.saveBhvFileDialog.Filter = "BHV Files (*.bhv)|*.bhv|All Files|*.*";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 470);
            this.Controls.Add(this.btnOpenBHV);
            this.Controls.Add(this.btnExportJSON);
            this.Controls.Add(this.btnImportJSON);
            this.Controls.Add(this.btnSaveBHV);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.listViewFields);
            this.Text = "BHV Editor";
            this.ResumeLayout(false);
        }
    }
}
