using System;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BHVEditor
{
    public partial class MainForm : Form
    {
        private BHVFile currentFile;

        public MainForm()
        {
            InitializeComponent();

            // 将自定义编辑器控件 StateEditorControl 加入面板（可选）
            var editor = new StateEditorControl { Dock = DockStyle.Fill };
            panelContainer.Controls.Add(editor);

            // 将按钮事件与编辑器方法关联
            btnOpenBHV.Click += (s, e) =>
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                try
                {
                    currentFile = BHVFile.Load(openFileDialog1.FileName);
                    editor.LoadBHVFile(openFileDialog1.FileName);
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
                using (var dlg = new SaveFileDialog { Filter = "JSON 文件 (*.json)|*.json" })
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    try
                    {
                        editor.ExportJSON(dlg.FileName);
                        MessageBox.Show("导出 JSON 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            btnImportJSON.Click += (s, e) =>
            {
                using (var dlg = new OpenFileDialog { Filter = "JSON 文件 (*.json)|*.json" })
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    try
                    {
                        currentFile = JsonConvert.DeserializeObject<BHVFile>(
                            System.IO.File.ReadAllText(dlg.FileName, Encoding.UTF8)
                        );
                        editor.ImportJSON(dlg.FileName);
                        MessageBox.Show("JSON 导入完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            btnSaveBHV.Click += (s, e) =>
            {
                if (currentFile == null)
                {
                    MessageBox.Show("请先加载或导入数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var dlg = new SaveFileDialog { Filter = "BHV 文件 (*.bhv)|*.bhv" })
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    try
                    {
                        currentFile.Save(dlg.FileName);
                        MessageBox.Show("保存 BHV 成功。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
        }
    }
}
