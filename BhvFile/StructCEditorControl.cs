// StructCEditorControl.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BHVEditor
{
    public class StructCEditorControl : UserControl
    {
        private readonly DataGridView grid;
        private List<List<byte>> structCs;
        private int sizeC;

        public StructCEditorControl()
        {
            // 初始化 DataGridView
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                Font = new Font("Consolas", 9),
            };
            this.Controls.Add(grid);

            // 事件
            grid.CellValueChanged += Grid_CellValueChanged;
            grid.DataError += (s, e) =>
            {
                // 如果用户输入非法，恢复旧值
                e.Cancel = false;
                int r = e.RowIndex, c = e.ColumnIndex;
                if (r >= 0 && c >= 1 && structCs != null && r < structCs.Count && c - 1 < sizeC)
                    grid.Rows[r].Cells[c].Value = structCs[r][c - 1];
            };
        }

        /// <summary>
        /// 用 BHVFile.StructCs 填充表格
        /// </summary>
        public void LoadStructCs(List<List<byte>> list)
        {
            structCs = list ?? new List<List<byte>>();
            grid.Columns.Clear();
            grid.Rows.Clear();

            // 第一列：Index
            var colIndex = new DataGridViewTextBoxColumn
            {
                Name = "Index",
                HeaderText = "Index",
                ReadOnly = true,
                Width = 50
            };
            grid.Columns.Add(colIndex);

            if (structCs.Count == 0)
                return;

            sizeC = structCs[0].Count;
            // 按照 sizeC 动态添加列
            for (int i = 0; i < sizeC; i++)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    Name = $"B{i:X2}",
                    HeaderText = i.ToString("X2"),
                    Width = 30,
                    ValueType = typeof(byte)
                };
                grid.Columns.Add(col);
            }

            // 填行
            for (int row = 0; row < structCs.Count; row++)
            {
                var values = new object[sizeC + 1];
                values[0] = row;
                for (int j = 0; j < sizeC; j++)
                    values[j + 1] = structCs[row][j];
                grid.Rows.Add(values);
            }
        }

        /// <summary>
        /// 用户修改单元格后，将值写回 structCs
        /// </summary>
        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int r = e.RowIndex, c = e.ColumnIndex;
            if (r < 0 || c < 1) return;
            var cell = grid.Rows[r].Cells[c];
            if (byte.TryParse(cell.Value?.ToString(), out byte v))
            {
                structCs[r][c - 1] = v;
            }
            else
            {
                // 恢复旧值
                cell.Value = structCs[r][c - 1];
            }
        }
    }
}
