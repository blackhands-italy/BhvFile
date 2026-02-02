using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace BHVEditor
{
    public partial class MainForm : Form
    {
        private StateEditorControl stateEditor;
        private StructBEditorControl structBEditor;
        private StructCEditorControl structCEditor;
        private StringsEditorControl stringsEditor;

        public MainForm()
        {
            InitializeComponent();

            // 重新排列所有按钮的位置，为新的"载入调试JSON"按钮腾出空间
            int buttonWidth = 90;
            int buttonSpacing = 8;
            int startX = 12;
            int y = 12;

            // 设置现有按钮位置
            btnOpenBHV.Location = new Point(startX, y);
            btnOpenBHV.Size = new Size(buttonWidth, 30);

            btnExportJSON.Location = new Point(startX + buttonWidth + buttonSpacing, y);
            btnExportJSON.Size = new Size(buttonWidth, 30);

            btnImportJSON.Location = new Point(startX + (buttonWidth + buttonSpacing) * 2, y);
            btnImportJSON.Size = new Size(buttonWidth, 30);

            btnSaveBHV.Location = new Point(startX + (buttonWidth + buttonSpacing) * 3, y);
            btnSaveBHV.Size = new Size(buttonWidth, 30);

            // 添加"载入调试JSON"按钮
            Button btnLoadDebug = new Button
            {
                Text = "载入调试 JSON",
                Size = new Size(120, 30),
                Location = new Point(startX + (buttonWidth + buttonSpacing) * 4, y),
                FlatStyle = FlatStyle.Flat
            };

            this.Controls.Add(btnLoadDebug);

            // 按钮事件处理
            // 修改 btnLoadDebug.Click 事件处理程序，添加调试信息同步功能
            btnLoadDebug.Click += (s, e) => {
                using (OpenFileDialog ofd = new OpenFileDialog { Filter = "JSON Files|*.json" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        BhvDebugManager.LoadIntegratedJson(ofd.FileName);
                        // 同步调试信息到当前加载的文件
                        if (stateEditor.CurrentFile != null)
                        {
                            stateEditor.SyncDebugInfo();
                        }
                        // 刷新界面
                        if (stateEditor.CurrentFile != null)
                        {
                            var method = stateEditor.GetType().GetMethod("BuildNodes",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            method?.Invoke(stateEditor, null);
                            stateEditor.Invalidate();
                        }
                    }
                }
            };

            // 创建选项卡，用于在 StateEditor 和 StructBEditor 之间切换
            var tab = new TabControl { Dock = DockStyle.Fill };
            var tabPageState = new TabPage("状态机编辑");
            stateEditor = new StateEditorControl { Dock = DockStyle.Fill };
            tabPageState.Controls.Add(stateEditor);

            var tabPageStructB = new TabPage("StructB 编辑");
            structBEditor = new StructBEditorControl { Dock = DockStyle.Fill };
            tabPageStructB.Controls.Add(structBEditor);

            var tabPageStructC = new TabPage("StructC 编辑");
            structCEditor = new StructCEditorControl { Dock = DockStyle.Fill };
            tabPageStructC.Controls.Add(structCEditor);

            // ★ 新增：Strings 编辑
            var tabPageStrings = new TabPage("Strings 编辑");
            stringsEditor = new StringsEditorControl { Dock = DockStyle.Fill };
            tabPageStrings.Controls.Add(stringsEditor);

            tab.TabPages.Add(tabPageState);
            tab.TabPages.Add(tabPageStructB);
            tab.TabPages.Add(tabPageStructC);
            tab.TabPages.Add(tabPageStrings);

            // 清空原有容器，添加 TabControl
            panelContainer.Controls.Clear();
            panelContainer.Controls.Add(tab);

            // 按钮事件
            btnOpenBHV.Click += (s, e) =>
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                try
                {
                    stateEditor.LoadBHVFile(openFileDialog1.FileName);
                    // 传递 StructB 列表给编辑控件
                    structBEditor.LoadStructBs(new BindingList<StructB>(stateEditor.CurrentFile.StructBs));
                    structCEditor.LoadStructCs(stateEditor.CurrentFile.StructCs);
                    // ★ 同步 Strings
                    stringsEditor.LoadStrings(stateEditor.CurrentFile.Strings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnExportJSON.Click += (s, e) =>
            {
                if (stateEditor.CurrentFile == null)
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
                    stateEditor.ImportJSON(dlg.FileName);
                    // 更新 StructB 编辑器
                    structBEditor.LoadStructBs(new BindingList<StructB>(stateEditor.CurrentFile.StructBs));
                    // 更新 StructC 编辑器
                    structCEditor.LoadStructCs(stateEditor.CurrentFile.StructCs);
                    // ★ 同步 Strings
                    stringsEditor.LoadStrings(stateEditor.CurrentFile.Strings);
                    MessageBox.Show("JSON 导入完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnSaveBHV.Click += (s, e) =>
            {
                if (stateEditor.CurrentFile == null)
                {
                    MessageBox.Show("请先加载或导入数据。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var dlg = new SaveFileDialog { Filter = "BHV 文件 (*.bhv)|*.bhv" };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    // 保存前，确保 StructB 和 StructC 编辑同步到 currentFile
                    stateEditor.CurrentFile.Save(dlg.FileName);
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