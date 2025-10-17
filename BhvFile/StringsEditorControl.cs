using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BHVEditor
{
    public class StringsEditorControl : UserControl
    {
        private List<string> strings = new List<string>();

        private ListBox lst;
        private TextBox txt;
        private Button btnAdd, btnDel, btnUp, btnDown;
        private Label lblStats;

        public StringsEditorControl()
        {
            Dock = DockStyle.Fill;

            lst = new ListBox { Dock = DockStyle.Left, Width = 260, IntegralHeight = false };
            txt = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(6, 4, 6, 4)
            };
            btnAdd = new Button { Text = "添加", Width = 60 };
            btnDel = new Button { Text = "删除", Width = 60 };
            btnUp = new Button { Text = "上移", Width = 60 };
            btnDown = new Button { Text = "下移", Width = 60 };
            tools.Controls.AddRange(new Control[] { btnAdd, btnDel, btnUp, btnDown });

            lblStats = new Label { Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleRight };

            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(txt);
            right.Controls.Add(tools);
            right.Controls.Add(lblStats);

            Controls.Add(right);
            Controls.Add(lst);

            // 事件
            lst.SelectedIndexChanged += (s, e) =>
            {
                int i = lst.SelectedIndex;
                if (i >= 0 && i < strings.Count) txt.Text = strings[i];
                else txt.Text = "";
            };
            txt.TextChanged += (s, e) =>
            {
                int i = lst.SelectedIndex;
                if (i >= 0 && i < strings.Count)
                {
                    strings[i] = txt.Text;
                    lst.Items[i] = RenderItem(i, strings[i]);
                    UpdateStats();
                }
            };
            btnAdd.Click += (s, e) =>
            {
                int insertAt = Math.Max(lst.SelectedIndex, 0);
                strings.Insert(insertAt + 1, "");
                RebuildList();
                lst.SelectedIndex = insertAt + 1;
            };
            btnDel.Click += (s, e) =>
            {
                int i = lst.SelectedIndex;
                if (i >= 0 && i < strings.Count)
                {
                    strings.RemoveAt(i);
                    RebuildList();
                    lst.SelectedIndex = Math.Min(i, strings.Count - 1);
                }
            };
            btnUp.Click += (s, e) =>
            {
                int i = lst.SelectedIndex;
                if (i > 0)
                {
                    (strings[i - 1], strings[i]) = (strings[i], strings[i - 1]);
                    RebuildList();
                    lst.SelectedIndex = i - 1;
                }
            };
            btnDown.Click += (s, e) =>
            {
                int i = lst.SelectedIndex;
                if (i >= 0 && i < strings.Count - 1)
                {
                    (strings[i + 1], strings[i]) = (strings[i], strings[i + 1]);
                    RebuildList();
                    lst.SelectedIndex = i + 1;
                }
            };
        }

        private string RenderItem(int index, string s)
        {
            string preview = s?.Replace("\r", "").Replace("\n", "⏎");
            if (preview != null && preview.Length > 24) preview = preview.Substring(0, 24) + "…";
            return $"{index:D3}  {preview}";
        }

        private void RebuildList()
        {
            lst.BeginUpdate();
            lst.Items.Clear();
            for (int i = 0; i < strings.Count; i++)
                lst.Items.Add(RenderItem(i, strings[i]));
            lst.EndUpdate();
            UpdateStats();
        }

        private void UpdateStats()
        {
            // 预计写入大小：2(个数) + 2*N(偏移表) + Σ(len(UTF8)+1)
            int n = strings.Count;
            long sum = 2 + 2L * n + strings.Sum(s => Encoding.UTF8.GetByteCount(s ?? "") + 1);
            // 任一字符串的偏移不能大于 0xFFFF（写入逻辑用的是 uint16 偏移）
            bool warn = sum - (2 + 2L * n) > 0xFFFF;
            lblStats.Text = warn
                ? $"预计内容区 {sum - (2 + 2L * n)} bytes > 65535，可能越界！"
                : $"预计内容区 {sum - (2 + 2L * n)} bytes（OK）";
            lblStats.ForeColor = warn ? Color.OrangeRed : Color.DimGray;
        }

        // —— 对外 API ——
        public void LoadStrings(List<string> list)
        {
            strings = list ?? new List<string>();
            RebuildList();
            if (lst.Items.Count > 0) lst.SelectedIndex = 0;
        }
    }
}
