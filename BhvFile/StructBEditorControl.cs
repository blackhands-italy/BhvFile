using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace BHVEditor
{
    /// <summary>
    /// StructB 编辑器控件：显示、增删改 StructB 列表，并自动维护序号列。
    /// </summary>
    public class StructBEditorControl : UserControl
    {
        private DataGridView dataGridView;
        private Panel toolPanel;
        private Button btnAdd;
        private Button btnDelete;
        private BindingList<StructB> bindingList;

        public StructBEditorControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 工具栏
            toolPanel = new Panel { Dock = DockStyle.Top, Height = 30 };
            btnAdd = new Button { Text = "添加", Dock = DockStyle.Left, Width = 60 };
            btnDelete = new Button { Text = "删除", Dock = DockStyle.Left, Width = 60 };
            toolPanel.Controls.AddRange(new Control[] { btnAdd, btnDelete });

            // DataGridView
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
            };

            // 添加序号列
            var indexCol = new DataGridViewTextBoxColumn
            {
                Name = "Index",
                HeaderText = "序号",
                ReadOnly = true,
                Width = 50,
            };
            dataGridView.Columns.Add(indexCol);

            // 使用反射为 StructB 的每个属性添加列
            var props = typeof(StructB)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.Name);
            foreach (var prop in props)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = prop.Name,
                    Name = prop.Name,
                    HeaderText = prop.Name,
                    ReadOnly = false,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                };
                dataGridView.Columns.Add(col);
            }

            // 添加到控件
            this.Controls.Add(dataGridView);
            this.Controls.Add(toolPanel);

            // 事件连接
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        /// <summary>
        /// 加载 BindingList<StructB> 并绑定到表格
        /// </summary>
        public void LoadStructBs(BindingList<StructB> list)
        {
            bindingList = list;
            dataGridView.DataSource = bindingList;
            // 每次数据更新后刷新序号
            bindingList.ListChanged += (s, e) => RefreshIndexColumn();
            dataGridView.DataBindingComplete += (s, e) => RefreshIndexColumn();
            RefreshIndexColumn();
        }

        /// <summary>
        /// 刷新序号列
        /// </summary>
        private void RefreshIndexColumn()
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Cells["Index"].Value = i;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            StructB newItem;
            if (bindingList != null && bindingList.Count > 0)
            {
                // 复制上一行属性
                var last = bindingList[bindingList.Count - 1];
                newItem = new StructB();
                foreach (var prop in typeof(StructB).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var val = prop.GetValue(last);
                    prop.SetValue(newItem, val);
                }
            }
            else
            {
                newItem = new StructB();
            }
            bindingList?.Add(newItem);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (bindingList == null) return;
            var row = dataGridView.CurrentRow;
            if (row != null && row.Index < bindingList.Count)
            {
                bindingList.RemoveAt(row.Index);
            }
        }
    }
}
