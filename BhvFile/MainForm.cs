using System;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BHVEditor

{
    public partial class MainForm : Form
    {
        private BHVFile currentFile;

        public MainForm() => InitializeComponent();

        private void btnOpenBHV_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            try
            {
                currentFile = BHVFile.Load(openFileDialog1.FileName);
                PopulateTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportJSON_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("请先加载一个 BHV 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            try
            {
                var json = JsonConvert.SerializeObject(currentFile, Formatting.Indented);
                var bomUtf8 = new UTF8Encoding(true);
                System.IO.File.WriteAllText(saveFileDialog1.FileName, json, bomUtf8);
                MessageBox.Show("导出 JSON 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportJSON_Click(object sender, EventArgs e)
        {
            if (openJsonFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var json = System.IO.File.ReadAllText(openJsonFileDialog.FileName, Encoding.UTF8);
                currentFile = JsonConvert.DeserializeObject<BHVFile>(json);
                PopulateTree();
                MessageBox.Show("JSON 导入完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveBHV_Click(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                MessageBox.Show("请先加载或导入数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (saveBhvFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                currentFile.Save(saveBhvFileDialog.FileName);
                MessageBox.Show("保存 BHV 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 将 currentFile 的内容填充到 treeView1
        private void PopulateTree()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (currentFile == null)
            {
                treeView1.EndUpdate();
                listViewFields.Items.Clear();
                return;
            }

            // Header
            var nodeHdr = new TreeNode("Header") { Tag = currentFile.Header };
            treeView1.Nodes.Add(nodeHdr);

            // States
            var nodeStates = new TreeNode($"States ({currentFile.States.Count})");
            foreach (var st in currentFile.States)
                nodeStates.Nodes.Add(new TreeNode($"State {st.Index}") { Tag = st });
            treeView1.Nodes.Add(nodeStates);

            // StructB
            var nodeB = new TreeNode($"StructB ({currentFile.StructBs.Count})");
            for (int i = 0; i < currentFile.StructBs.Count; i++)
                nodeB.Nodes.Add(new TreeNode($"B[{i}]") { Tag = currentFile.StructBs[i] });
            treeView1.Nodes.Add(nodeB);

            // StructC
            var nodeC = new TreeNode($"StructC ({currentFile.StructCs.Count})");
            for (int i = 0; i < currentFile.StructCs.Count; i++)
                nodeC.Nodes.Add(new TreeNode($"C[{i}]") { Tag = currentFile.StructCs[i] });
            treeView1.Nodes.Add(nodeC);

            // StructD
            var nodeD = new TreeNode($"StructD ({currentFile.StructDs.Count})");
            for (int i = 0; i < currentFile.StructDs.Count; i++)
                nodeD.Nodes.Add(new TreeNode($"D[{i}]") { Tag = currentFile.StructDs[i] });
            treeView1.Nodes.Add(nodeD);

            // Strings
            if (currentFile.Strings != null)
            {
                var nodeS = new TreeNode($"Strings ({currentFile.Strings.Count})");
                foreach (var s in currentFile.Strings)
                    nodeS.Nodes.Add(new TreeNode(s));
                treeView1.Nodes.Add(nodeS);
            }

            treeView1.ExpandAll();
            treeView1.EndUpdate();
            listViewFields.Items.Clear();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listViewFields.Items.Clear();
            if (e.Node?.Tag == null) return;
            var obj = e.Node.Tag;
            var props = obj.GetType().GetProperties();
            foreach (var p in props)
            {
                if (!p.CanRead) continue;
                var val = p.GetValue(obj);
                if (val == null) continue;
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(val.ToString());
                listViewFields.Items.Add(item);
            }
        }
    }
}
