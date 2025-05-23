using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;

namespace BHVEditor
{
    public partial class MainForm : Form
    {
        private BHVFile currentFile;
        private StateEditorControl stateEditor;
        private StructBEditorControl structBEditor;

        public MainForm()
        {
            InitializeComponent();

            // 创建选项卡，用于在 StateEditor 和 StructBEditor 之间切换
            var tab = new TabControl { Dock = DockStyle.Fill };
            var tabPageState = new TabPage("状态机编辑");
            stateEditor = new StateEditorControl { Dock = DockStyle.Fill };
            tabPageState.Controls.Add(stateEditor);

            var tabPageStructB = new TabPage("StructB 编辑");
            structBEditor = new StructBEditorControl { Dock = DockStyle.Fill };
            tabPageStructB.Controls.Add(structBEditor);

            tab.TabPages.Add(tabPageState);
            tab.TabPages.Add(tabPageStructB);

            // 清空原有容器，添加 TabControl
            panelContainer.Controls.Clear();
            panelContainer.Controls.Add(tab);

            // 按钮事件
            btnOpenBHV.Click += (s, e) =>
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                try
                {
                    currentFile = BHVFile.Load(openFileDialog1.FileName);
                    stateEditor.LoadBHVFile(openFileDialog1.FileName);
                    // 传递 StructB 列表给编辑控件
                    structBEditor.LoadStructBs(new BindingList<StructB>(currentFile.StructBs));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnExportJSON.Click += (s, e) =>
            {
                if (currentFile == null)
                {
                    MessageBox.Show("请先加载一个 BHV 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var dlg = new SaveFileDialog { Filter = "JSON 文件 (*.json)|*.json" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    // JSON 导出只针对 State 数据
                    stateEditor.ExportJSON(dlg.FileName);
                    MessageBox.Show("导出 JSON 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnImportJSON.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = "JSON 文件 (*.json)|*.json" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    // JSON 导入替换整个模型
                    currentFile = JsonConvert.DeserializeObject<BHVFile>(File.ReadAllText(dlg.FileName, Encoding.UTF8));
                    stateEditor.ImportJSON(dlg.FileName);
                    // 更新 StructB 编辑器
                    structBEditor.LoadStructBs(new BindingList<StructB>(currentFile.StructBs));
                    MessageBox.Show("JSON 导入完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnSaveBHV.Click += (s, e) =>
            {
                if (currentFile == null)
                {
                    MessageBox.Show("请先加载或导入数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var dlg = new SaveFileDialog { Filter = "BHV 文件 (*.bhv)|*.bhv" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    // 保存前，确保 StructB 编辑同步到 currentFile
                    // BindingList 已自动同步原始列表
                    currentFile.Save(dlg.FileName);
                    MessageBox.Show("保存 BHV 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
    }
}
