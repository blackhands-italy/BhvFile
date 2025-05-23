using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BHVEditor
{
    /// <summary>可视化状态机编辑控件。</summary>
    public class StateEditorControl : UserControl
    {
        private BHVFile currentFile;
        private BHVFile originalFile;
        private List<Node> nodes = new List<Node>();
        private List<BHVFile> history = new List<BHVFile>();
        private int historyIndex = -1;

        private readonly Size nodeSize = new Size(100, 60);
        private readonly Pen arrowPen;
        private readonly Brush nodeFill = Brushes.LightBlue;
        private readonly Brush nodeBorder = Brushes.Black;
        private readonly Font nodeFont = new Font("Arial", 9);
        private readonly StringFormat textFmt;

        // 缩放和平移
        private float scale = 1.0f;
        private PointF panOffset = new PointF(0, 0);
        private bool panning = false;
        private Point lastPanPoint;

        private Node draggingNode;
        private Point dragOffset;
        private Node pendingTransitionSource;

        private ContextMenuStrip ctxMenu;
        private ToolStripMenuItem menuUndo, menuRedo, menuReset;

        public StateEditorControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;

            arrowPen = new Pen(Color.DarkBlue, 2);
            arrowPen.CustomEndCap = new AdjustableArrowCap(6, 6, true);

            textFmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // 事件订阅
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            MouseDoubleClick += OnMouseDoubleClick;
            MouseWheel += OnMouseWheel;
            KeyDown += OnKeyDown;
            PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                    e.IsInputKey = true;
            };
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            InitContextMenu();
        }

        #region 公共 API

        public void LoadBHVFile(string path)
        {
            var file = BHVFile.Load(path);
            SetCurrentFile(file);
        }

        public void ImportJSON(string path)
        {
            var txt = System.IO.File.ReadAllText(path);
            var file = JsonConvert.DeserializeObject<BHVFile>(txt);
            SetCurrentFile(file);
        }

        public void ExportJSON(string path)
        {
            var txt = JsonConvert.SerializeObject(currentFile, Formatting.Indented);
            System.IO.File.WriteAllText(path, txt, System.Text.Encoding.UTF8);
        }

        public void ResetToOriginal()
        {
            if (originalFile == null) return;
            currentFile = CloneBhv(originalFile);
            BuildNodes();
            history.Clear();
            historyIndex = -1;
            PushHistory();
            Invalidate();
        }

        public void Undo()
        {
            if (historyIndex > 0)
            {
                historyIndex--;
                currentFile = CloneBhv(history[historyIndex]);
                BuildNodes();
                Invalidate();
                UpdateUndoRedoMenu();
            }
        }

        public void Redo()
        {
            if (historyIndex < history.Count - 1)
            {
                historyIndex++;
                currentFile = CloneBhv(history[historyIndex]);
                BuildNodes();
                Invalidate();
                UpdateUndoRedoMenu();
            }
        }

        #endregion

        #region 私有方法

        private void SetCurrentFile(BHVFile file)
        {
            currentFile = file;
            originalFile = CloneBhv(file);
            BuildNodes();
            history.Clear();
            historyIndex = -1;
            PushHistory();
            Invalidate();
        }

        private BHVFile CloneBhv(BHVFile src)
        {
            var txt = JsonConvert.SerializeObject(src);
            return JsonConvert.DeserializeObject<BHVFile>(txt);
        }

        private void BuildNodes()
        {
            nodes.Clear();
            if (currentFile?.States == null) return;
            int n = currentFile.States.Count;
            int cols = (int)Math.Ceiling(Math.Sqrt(n));
            int spacingX = nodeSize.Width + 50;
            int spacingY = nodeSize.Height + 50;
            for (int i = 0; i < n; i++)
            {
                var st = currentFile.States[i];
                int col = i % cols, row = i / cols;
                var pos = new Point(50 + col * spacingX, 50 + row * spacingY);
                // 获取对应 StructB.Unk00
                string label = $"S{st.Index}\nU00={GetStructUnk00(st)}";
                nodes.Add(new Node(st, label, pos));
            }
            // 设置滚动区域，保证所有节点可见
            int maxX = nodes.Max(n2 => n2.Position.X) + nodeSize.Width + 50;
            int maxY = nodes.Max(n2 => n2.Position.Y) + nodeSize.Height + 50;
            AutoScrollMinSize = new Size(maxX, maxY);
        }
        private string GetStructUnk00(State st)
        {
            int idx = st.StructBid;
            if (idx >= 0 && idx < currentFile.StructBs.Count)
                return currentFile.StructBs[idx].Unk00.ToString();
            return "-";
        }

        private void PushHistory()
        {
            if (historyIndex < history.Count - 1)
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
            history.Add(CloneBhv(currentFile));
            historyIndex = history.Count - 1;
            UpdateUndoRedoMenu();
        }

        private void InitContextMenu()
        {
            ctxMenu = new ContextMenuStrip();
            ctxMenu.Opening += (s, e) =>
            {
                var clientPt = PointToClient(Control.MousePosition);
                var worldPt = ScreenToWorld(clientPt);
                ctxMenu.Items.Clear();

                var node = FindNodeAt(worldPt);
                var trHit = FindTransitionAt(worldPt);
                if (trHit != null)
                {
                    ctxMenu.Items.Add("删除转换", null, (sender, args) => DeleteTransition(worldPt));
                    ctxMenu.Items.Add("属性...", null, (sender, args) => ShowTransitionProps(worldPt));
                }
                else if (node != null)
                {
                    ctxMenu.Items.Add("添加转换...", null, (sender, args) => StartAddTransition(node));
                    ctxMenu.Items.Add("删除状态", null, (sender, args) => DeleteState(node));
                    ctxMenu.Items.Add("重命名状态...", null, (sender, args) => RenameState(node));
                    ctxMenu.Items.Add("属性...", null, (sender, args) => ShowStateProps(node));
                }
                else
                {
                    ctxMenu.Items.Add("添加新状态", null, (sender, args) => AddState());
                    ctxMenu.Items.Add(new ToolStripSeparator());
                    ctxMenu.Items.Add("导入 JSON...", null, (sender, args) => ImportJsonDialog());
                    ctxMenu.Items.Add("导出 JSON...", null, (sender, args) => ExportJsonDialog());
                    ctxMenu.Items.Add("编辑 JSON...", null, (sender, args) => EditJsonDialog());
                    ctxMenu.Items.Add(new ToolStripSeparator());

                    menuUndo = new ToolStripMenuItem("撤销", null, (s2, e2) => Undo());
                    menuRedo = new ToolStripMenuItem("重做", null, (s2, e2) => Redo());
                    menuReset = new ToolStripMenuItem("重置到初始", null, (s2, e2) => ResetToOriginal());

                    ctxMenu.Items.Add(menuUndo);
                    ctxMenu.Items.Add(menuRedo);
                    ctxMenu.Items.Add(menuReset);
                    UpdateUndoRedoMenu();
                }
            };
            ContextMenuStrip = ctxMenu;
        }
        private void UpdateUndoRedoMenu()
        {
            if (menuUndo != null) menuUndo.Enabled = historyIndex > 0;
            if (menuRedo != null) menuRedo.Enabled = historyIndex < history.Count - 1;
        }

        private void ImportJsonDialog()
        {
            using var dlg = new OpenFileDialog { Filter = "JSON 文件|*.json" };
            if (dlg.ShowDialog() == DialogResult.OK) ImportJSON(dlg.FileName);
        }

        private void ExportJsonDialog()
        {
            using var dlg = new SaveFileDialog { Filter = "JSON 文件|*.json" };
            if (dlg.ShowDialog() == DialogResult.OK) ExportJSON(dlg.FileName);
        }

        private void EditJsonDialog()
        {
            if (currentFile == null) return;
            string json = JsonConvert.SerializeObject(currentFile, Formatting.Indented);
            using var form = new Form { Text = "编辑 JSON", Size = new Size(600, 400) };
            var txt = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Font = new Font(FontFamily.GenericMonospace, 9), Text = json };
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var btnOK = new Button { Text = "应用", DialogResult = DialogResult.OK, Left = 10, Width = 80 };
            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Left = 100, Width = 80 };
            panel.Controls.AddRange(new Control[] { btnOK, btnCancel });
            form.Controls.Add(txt);
            form.Controls.Add(panel);
            form.AcceptButton = btnOK;
            form.CancelButton = btnCancel;
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var nf = JsonConvert.DeserializeObject<BHVFile>(txt.Text);
                    if (nf != null) SetCurrentFile(nf);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("JSON 解析失败:\n" + ex.Message);
                }
            }
        }

        #endregion

        #region 交互事件

        private void OnMouseDown(object s, MouseEventArgs e)
        {
            Focus();
            if (e.Button == MouseButtons.Middle)
            {
                panning = true;
                lastPanPoint = e.Location;
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                var wp = ScreenToWorld(e.Location);
                var nd = FindNodeAt(wp);
                if (nd != null)
                {
                    draggingNode = nd;
                    dragOffset = new Point(wp.X - nd.Position.X, wp.Y - nd.Position.Y);
                }
            }
        }

        private void OnMouseMove(object s, MouseEventArgs e)
        {
            if (panning && e.Button == MouseButtons.Middle)
            {
                panOffset.X += e.X - lastPanPoint.X;
                panOffset.Y += e.Y - lastPanPoint.Y;
                lastPanPoint = e.Location;
                Invalidate();  // 整面重绘
                return;
            }
            if (draggingNode != null && e.Button == MouseButtons.Left)
            {
                // 拖节点
                var wp = ScreenToWorld(e.Location);
                draggingNode.Position = new Point(
                    wp.X - dragOffset.X,
                    wp.Y - dragOffset.Y
                );
                Invalidate();
            }
        }

        private void OnMouseUp(object s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                panning = false;
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                var wp = ScreenToWorld(e.Location);
                if (draggingNode != null)
                {
                    draggingNode = null;
                }
                else if (pendingTransitionSource != null)
                {
                    var tgt = FindNodeAt(wp);
                    if (tgt != null && tgt != pendingTransitionSource)
                        AddTransition(pendingTransitionSource, tgt);
                    pendingTransitionSource = null;
                    PushHistory();
                    Invalidate();
                }
            }
        }

        private void OnMouseDoubleClick(object s, MouseEventArgs e)
        {
            var wp = ScreenToWorld(e.Location);
            var nd = FindNodeAt(wp);
            if (nd != null) ShowStateProps(nd);
            else if (FindTransitionAt(wp) != null) ShowTransitionProps(wp);
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            var before = ScreenToWorld(e.Location);
            scale *= e.Delta > 0 ? 1.1f : 0.9f;
            var after = ScreenToWorld(e.Location);

            // 保持鼠标位置处不动
            panOffset.X += (after.X - before.X) * scale;
            panOffset.Y += (after.Y - before.Y) * scale;
            Invalidate();
        }
        private void OnKeyDown(object s, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.SuppressKeyPress = true; }
        }
        private Point ScreenToWorld(Point screenPt)
        {
            return new Point(
                (int)((screenPt.X - panOffset.X) / scale),
                (int)((screenPt.Y - panOffset.Y) / scale)
            );
        }
        #endregion

        #region 节点/转换 查找 & 操作

        private Node FindNodeAt(Point p)
            => nodes.FirstOrDefault(n =>
                p.X >= n.Position.X && p.X <= n.Position.X + nodeSize.Width &&
                p.Y >= n.Position.Y && p.Y <= n.Position.Y + nodeSize.Height);

        private Tuple<State, Transition> FindTransitionAt(Point p)
        {
            const double tol = 6.0;
            foreach (var st in currentFile.States)
            {
                var nd = nodes.FirstOrDefault(x => x.State == st);
                if (nd == null) continue;
                var p1 = nd.Center(nodeSize);
                foreach (var tr in st.Transitions)
                {
                    var nd2 = nodes.FirstOrDefault(x => x.State.Index == tr.StateIndex);
                    if (nd2 == null) continue;
                    var p2 = nd2.Center(nodeSize);
                    if (DistancePointToSegment(p, p1, p2) <= tol)
                        return Tuple.Create(st, tr);
                }
            }
            return null;
        }

        private double DistancePointToSegment(Point p, Point a, Point b)
        {
            float vx = b.X - a.X, vy = b.Y - a.Y;
            float wx = p.X - a.X, wy = p.Y - a.Y;
            float c1 = vx * wx + vy * wy;
            if (c1 <= 0) return Math.Sqrt(wx * wx + wy * wy);
            float c2 = vx * vx + vy * vy;
            if (c2 <= c1) return Math.Sqrt((p.X - b.X) * (p.X - b.X) + (p.Y - b.Y) * (p.Y - b.Y));
            float t = c1 / c2;
            float projX = a.X + t * vx, projY = a.Y + t * vy;
            float dx = p.X - projX, dy = p.Y - projY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void StartAddTransition(Node src) => pendingTransitionSource = src;

        private void AddTransition(Node src, Node dst)
        {
            var st = src.State;
            var tr = new Transition
            {
                StateIndex = dst.State.Index
            };
            st.Transitions.Add(tr);
            st.TransitionCount = st.Transitions.Count;
            PushHistory();
            Invalidate();
        }

        private void DeleteTransition(Point worldPt)
        {
            var hit = FindTransitionAt(worldPt);
            if (hit != null)
            {
                hit.Item1.Transitions.Remove(hit.Item2);
                hit.Item1.TransitionCount = hit.Item1.Transitions.Count;
            }
            PushHistory();
            Invalidate();
        }

        private void AddState()
        {
            var st = new State { Index = currentFile.States.Count };
            currentFile.States.Add(st);
            var sb = new StructB();
            currentFile.StructBs.Add(sb);
            st.StructBid = (short)(currentFile.StructBs.Count - 1);
            currentFile.Header.StateCount = currentFile.States.Count;
            currentFile.Header.CountB = currentFile.StructBs.Count;
            BuildNodes();
            PushHistory();
            Invalidate();
        }

        private void DeleteState(Node nd)
        {
            var st = nd.State;
            int idx = st.Index;
            currentFile.States.Remove(st);
            foreach (var s in currentFile.States)
            {
                s.Transitions.RemoveAll(tr => tr.StateIndex == idx);
                foreach (var tr in s.Transitions)
                    if (tr.StateIndex > idx) tr.StateIndex--;
            }
            for (int i = 0; i < currentFile.States.Count; i++)
                currentFile.States[i].Index = i;
            if (st.StructBid >= 0 && st.StructBid < currentFile.StructBs.Count)
            {
                int bi = st.StructBid;
                bool used = currentFile.States.Any(s => s.StructBid == bi);
                if (!used)
                {
                    currentFile.StructBs.RemoveAt(bi);
                    foreach (var s in currentFile.States)
                        if (s.StructBid > bi) s.StructBid--;
                }
            }
            currentFile.Header.StateCount = currentFile.States.Count;
            currentFile.Header.CountB = currentFile.StructBs.Count;
            BuildNodes();
            PushHistory();
            Invalidate();
        }

        private void RenameState(Node nd)
        {
            // 使用 PromptDialog 替代 Interaction.InputBox
            string newName = PromptDialog.Show(
                title: "重命名状态",
                promptText: "请输入状态名称：",
                defaultValue: nd.Name
            );
            if (!string.IsNullOrEmpty(newName))
            {
                nd.Name = newName;
                PushHistory();
                Invalidate();
            }
        }
        private void ShowStateProps(Node nd)
        {
            using var f = new Form { Text = $"状态 {nd.State.Index} 属性", Size = new Size(400, 400) };
            var pg = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = nd.State };
            f.Controls.Add(pg);
            f.ShowDialog();
            PushHistory();
        }

        private void ShowTransitionProps(Point worldPt)
        {
            var hit = FindTransitionAt(worldPt);
            if (hit != null) ShowTransitionProps(hit.Item1, hit.Item2);
        }
        private void ShowTransitionProps(State st, Transition tr)
        {
            using var f = new Form { Text = $"转换 {st.Index}→{tr.StateIndex}", Size = new Size(400, 400) };
            var pg = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = tr };
            f.Controls.Add(pg);
            if (f.ShowDialog() == DialogResult.OK)
            {
                RecalcAllCountsAndOffsets();
                PushHistory();
                Invalidate();
            }
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            g.ResetTransform();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(panOffset.X, panOffset.Y);
            g.ScaleTransform(scale, scale);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 先画所有连线
            if (currentFile != null)
            {
                foreach (var st in currentFile.States)
                {
                    var nd1 = nodes.FirstOrDefault(n => n.State == st);
                    if (nd1 == null) continue;
                    var c1 = nd1.Center(nodeSize);
                    foreach (var tr in st.Transitions)
                    {
                        var nd2 = nodes.FirstOrDefault(n => n.State.Index == tr.StateIndex);
                        if (nd2 == null) continue;
                        var c2 = nd2.Center(nodeSize);
                        var p1 = GetEdgePoint(c1, c2);
                        var p2 = GetEdgePoint(c2, c1);
                        g.DrawLine(arrowPen, p1, p2);
                    }
                }
            }

            // 再画节点
            foreach (var nd in nodes)
            {
                var rect = new Rectangle(nd.Position, nodeSize);
                g.FillRectangle(nodeFill, rect);
                g.DrawRectangle(new Pen(nodeBorder, 2), rect);
                g.DrawString(nd.Name, nodeFont, Brushes.Black, rect, textFmt);
            }
        }

        private Point GetEdgePoint(Point center, Point toward)
        {
            float halfW = nodeSize.Width / 2f, halfH = nodeSize.Height / 2f;
            float dx = toward.X - center.X, dy = toward.Y - center.Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 1e-6) return center;
            float ux = (float)(dx / d), uy = (float)(dy / d);
            float tX = ux == 0 ? float.MaxValue : halfW / Math.Abs(ux);
            float tY = uy == 0 ? float.MaxValue : halfH / Math.Abs(uy);
            float t = Math.Min(tX, tY);
            return new Point(
                center.X + (int)(ux * t),
                center.Y + (int)(uy * t)
            );
        }
        public void SaveBHV(string path)
        {
            // 先把所有 count／offset 重新根据 List<> 同步一遍
            RecalcAllCountsAndOffsets();

            // 真正写文件
            currentFile.Save(path);
        }

        private void RecalcAllCountsAndOffsets()
        {
            // 1) 头部的各个段数量
            currentFile.Header.StateCount = currentFile.States.Count;
            currentFile.Header.CountB = currentFile.StructBs.Count;
            currentFile.Header.CountC = (short)currentFile.StructCs.Count;
            currentFile.Header.CountD = currentFile.StructDs.Count;

            // 2) 每个 State 里的 TransitionCount
            foreach (var st in currentFile.States)
            {
                st.TransitionCount = st.Transitions?.Count ?? 0;
                // 3) 每个 Transition 里的 ConditionCount
                foreach (var tr in st.Transitions)
                    tr.ConditionCount = tr.Conditions?.Count ?? 0;
            }
            // 其它 offset（Offset04、TransitionsOffset 等）都在 BHVFile.Write 里会根据这些 Count 自动计算
        }
        #endregion
    }
}
