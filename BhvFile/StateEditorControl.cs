using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BHVEditor
{
    public class StateEditorControl : UserControl
    {
        // Expose current file
        public BHVFile CurrentFile => currentFile;

        private BHVFile currentFile;
        private BHVFile originalFile;
        private List<Node> nodes = new List<Node>();
        private List<BHVFile> history = new List<BHVFile>();
        private int historyIndex = -1;

        private readonly Size nodeSize = new Size(100, 60);
        private readonly Pen arrowPen;
        private readonly Pen previewPen;
        private readonly Brush nodeFill = Brushes.LightBlue;
        private readonly Brush nodeBorder = Brushes.Black;
        private readonly Font nodeFont = new Font("Arial", 9);
        private readonly StringFormat textFmt;

        // Box selection
        private bool isBoxSelecting = false;
        private Point boxStart;
        private Rectangle selectionRect = Rectangle.Empty;
        private List<Node> selectedNodes = new List<Node>();

        // Interaction state
        private Node draggingNode;
        private Point dragOffset;
        private Node pendingTransitionSource;
        private Point currentMouseWorld;
        private bool isPanning;
        private Point panStart;
        private Point viewOffset = Point.Empty;
        private float zoom = 1.0f;
        // 过滤框
        // 在类成员区，新增字段：
        private TextBox txtFilterAnim1;
        private TextBox txtFilterAnim2;
        private int? filterAnim1 = null;
        private int? filterAnim2 = null;

        private ContextMenuStrip ctxMenu;
        private ToolStripMenuItem menuUndo, menuRedo, menuReset;

        // Quick transition trigger area size
        private const int QuickTriggerSize = 12;

        public StateEditorControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;

            arrowPen = new Pen(Color.DarkBlue, 2)
            {
                CustomEndCap = new AdjustableArrowCap(6, 6, true)
            };
            previewPen = new Pen(Color.OrangeRed, 2)
            {
                CustomEndCap = new AdjustableArrowCap(6, 6, true)
            };
            textFmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            // —— 新增过滤框 —— 
            txtFilterAnim1 = new TextBox
            {
                Location = new Point(10, 10),
                Width = 80,
                Tag = "Anim1 ID"
            };
            txtFilterAnim1.TextChanged += (s, e) =>
            {
                if (int.TryParse(txtFilterAnim1.Text, out var v)) filterAnim1 = v;
                else filterAnim1 = null;
                BuildNodes();
                Invalidate();
            };
            Controls.Add(txtFilterAnim1);

            txtFilterAnim2 = new TextBox
            {
                Location = new Point(100, 10),
                Width = 80,
                Tag = "Anim2 ID"
            };
            txtFilterAnim2.TextChanged += (s, e) =>
            {
                if (int.TryParse(txtFilterAnim2.Text, out var v)) filterAnim2 = v;
                else filterAnim2 = null;
                BuildNodes();
                Invalidate();
            };
            Controls.Add(txtFilterAnim2);

            // 调整原有 BuildNodes 调用的位置到这里，确保过滤框先初始化
            SetCurrentFile(null);
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseDoubleClick += OnMouseDoubleClick;
            MouseWheel += OnMouseWheel;
            KeyDown += OnKeyDown;
            PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                    e.IsInputKey = true;
            };

            InitContextMenu();
            UpdateUndoRedoMenu();
        }

        #region Public API
        public void LoadBHVFile(string path)
        {
            var file = BHVFile.Load(path);
            NormalizeFile(file);
            SetCurrentFile(file);
        }

        public void ImportJSON(string path)
        {
            var txt = System.IO.File.ReadAllText(path);
            var file = JsonConvert.DeserializeObject<BHVFile>(txt);
            NormalizeFile(file);
            SetCurrentFile(file);
        }

        public void ExportJSON(string path)
        {
            var txt = JsonConvert.SerializeObject(currentFile, Formatting.Indented);
            System.IO.File.WriteAllText(path, txt, System.Text.Encoding.UTF8);
        }

        public void Undo()
        {
            if (historyIndex > 0)
            {
                historyIndex--;
                currentFile = CloneBhv(history[historyIndex]);
                BuildNodes(); Invalidate();
                UpdateUndoRedoMenu();
            }
        }

        public void Redo()
        {
            if (historyIndex < history.Count - 1)
            {
                historyIndex++;
                currentFile = CloneBhv(history[historyIndex]);
                BuildNodes(); Invalidate();
                UpdateUndoRedoMenu();
            }
        }

        public void ResetToOriginal()
        {
            if (originalFile == null) return;
            currentFile = CloneBhv(originalFile);
            BuildNodes();
            history.Clear(); historyIndex = -1;
            PushHistory(); Invalidate();
        }
        #endregion

        #region Initialization & History
        private void NormalizeFile(BHVFile file)
        {
            if (file.States == null) file.States = new List<State>();
            if (file.StructBs == null) file.StructBs = new List<StructB>();
            if (file.StructCs == null) file.StructCs = new List<List<byte>>();
            if (file.StructDs == null) file.StructDs = new List<StructD>();
            if (file.Strings == null) file.Strings = new List<string>();
        }

        private void SetCurrentFile(BHVFile file)
        {
            currentFile = file;
            originalFile = CloneBhv(file);
            BuildNodes();
            history.Clear(); historyIndex = -1;
            PushHistory(); Invalidate();
        }

        private BHVFile CloneBhv(BHVFile src)
        {
            var json = JsonConvert.SerializeObject(src);
            return JsonConvert.DeserializeObject<BHVFile>(json);
        }

        private void BuildNodes()
        {
            nodes.Clear();
            if (currentFile?.States == null) return;

            // 先根据动画 ID 过滤状态
var states = currentFile.States.Where(st => {
    var sb1 = currentFile.StructBs.ElementAtOrDefault(st.StructBid);
    var sb2 = currentFile.StructBs.ElementAtOrDefault(st.StructBsid2);
    if (filterAnim1.HasValue && (sb1 == null || sb1.Unk00 != filterAnim1.Value)) return false;
    if (filterAnim2.HasValue && (sb2 == null || sb2.Unk00 != filterAnim2.Value)) return false;
    return true;
}).ToList();


            int n = states.Count;
            if (n == 0) return;
            int cols = (int)Math.Ceiling(Math.Sqrt(n));
            int spacingX = nodeSize.Width + 50;
            int spacingY = nodeSize.Height + 50;
            for (int i = 0; i < n; i++)
            {
                var st = states[i];
                int col = i % cols, row = i / cols;
                var pos = new Point(50 + col * spacingX, 50 + row * spacingY);
                // 节点名称里继续包含 structBid 和 structB2 的动画 ID
                var sb1 = currentFile.StructBs.ElementAtOrDefault(st.StructBid);
                var sb2 = currentFile.StructBs.ElementAtOrDefault(st.StructBsid2);
                string name = $"S{st.Index} A1:{sb1?.Unk00 ?? -1} A2:{sb2?.Unk00 ?? -1}";
                nodes.Add(new Node(st, name, pos));
            }
        }

        private void PushHistory()
        {
            if (historyIndex < history.Count - 1)
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
            history.Add(CloneBhv(currentFile));
            historyIndex = history.Count - 1;
            UpdateUndoRedoMenu();
        }

        private void UpdateUndoRedoMenu()
        {
            if (menuUndo != null) menuUndo.Enabled = historyIndex > 0;
            if (menuRedo != null) menuRedo.Enabled = historyIndex < history.Count - 1;
        }
        #endregion

        #region Input Handling
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Focus();
            var world = ScreenToWorld(e.Location);
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = true; panStart = e.Location;
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Quick transition trigger region
                foreach (var nd in nodes)
                {
                    var rect = new Rectangle(nd.Position, nodeSize);
                    var trigger = new Rectangle(rect.Right - QuickTriggerSize, rect.Top,
                                                 QuickTriggerSize, QuickTriggerSize);
                    if (trigger.Contains(world))
                    {
                        pendingTransitionSource = nd;
                        return;
                    }
                }
                // Drag node
                var hit = FindNodeAt(world);
                if (hit != null)
                {
                    draggingNode = hit;
                    dragOffset = new Point(world.X - hit.Position.X, world.Y - hit.Position.Y);
                    return;
                }
                // Blank area: start box selection
                pendingTransitionSource = null;
                isBoxSelecting = true;
                boxStart = world;
                selectionRect = Rectangle.Empty;
                selectedNodes.Clear();
                Invalidate();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var world = ScreenToWorld(e.Location);
            currentMouseWorld = world;
            if (isPanning)
            {
                viewOffset = new Point(viewOffset.X + (e.X - panStart.X), viewOffset.Y + (e.Y - panStart.Y));
                panStart = e.Location; Invalidate();
            }
            else if (draggingNode != null && e.Button == MouseButtons.Left)
            {
                draggingNode.Position = new Point(world.X - dragOffset.X, world.Y - dragOffset.Y);
                Invalidate();
            }
            else if (pendingTransitionSource != null)
            {
                Invalidate();
            }
            else if (isBoxSelecting && e.Button == MouseButtons.Left)
            {
                int x = Math.Min(boxStart.X, world.X);
                int y = Math.Min(boxStart.Y, world.Y);
                int w = Math.Abs(world.X - boxStart.X);
                int h = Math.Abs(world.Y - boxStart.Y);
                selectionRect = new Rectangle(x, y, w, h);
                Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            var world = ScreenToWorld(e.Location);
            if (e.Button == MouseButtons.Middle)
            {
                isPanning = false; return;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (draggingNode != null)
                {
                    draggingNode = null;
                }
                else if (pendingTransitionSource != null)
                {
                    var dst = FindNodeAt(world);
                    if (dst != null && dst != pendingTransitionSource)
                    {
                        AddTransition(pendingTransitionSource, dst);
                        PushHistory();
                    }
                    pendingTransitionSource = null;
                    Invalidate();
                }
                else if (isBoxSelecting)
                {
                    isBoxSelecting = false;
                    selectedNodes = nodes.Where(n =>
                        selectionRect.Contains(n.Position) &&
                        selectionRect.Contains(new Point(n.Position.X + nodeSize.Width,
                                                         n.Position.Y + nodeSize.Height)))
                        .ToList();
                    Invalidate();
                }
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var world = ScreenToWorld(e.Location);
            var nd = FindNodeAt(world);
            if (nd != null) ShowStateProps(nd);
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            zoom *= e.Delta > 0 ? 1.1f : 0.9f;
            Invalidate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.D && selectedNodes.Count > 0)
            {
                DuplicateSelectedNodes();
                PushHistory(); Invalidate();
            }
        }
        #endregion

        #region Transform & Hit
        private Point ScreenToWorld(Point p)
        {
            return new Point(
                (int)((p.X - viewOffset.X) / zoom),
                (int)((p.Y - viewOffset.Y) / zoom));
        }

        private Node FindNodeAt(Point p) =>
            nodes.FirstOrDefault(n => new Rectangle(n.Position, nodeSize).Contains(p));
        #endregion

        #region Draw
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (currentFile == null || currentFile.States == null) return;

            e.Graphics.TranslateTransform(viewOffset.X, viewOffset.Y);
            e.Graphics.ScaleTransform(zoom, zoom);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // draw transitions
            foreach (var st in currentFile.States)
            {
                var n1 = nodes.FirstOrDefault(n => n.State == st);
                if (n1 == null || st.Transitions == null) continue;
                var c1 = n1.Center(nodeSize);
                foreach (var tr in st.Transitions)
                {
                    var n2 = nodes.FirstOrDefault(n => n.State.Index == tr.StateIndex);
                    if (n2 == null) continue;
                    var c2 = n2.Center(nodeSize);
                    var p1 = GetEdgePoint(c1, c2);
                    var p2 = GetEdgePoint(c2, c1);
                    e.Graphics.DrawLine(arrowPen, p1, p2);
                }
            }

            // draw nodes & quick trigger
            foreach (var nd in nodes)
            {
                var rect = new Rectangle(nd.Position, nodeSize);
                e.Graphics.FillRectangle(nodeFill, rect);
                e.Graphics.DrawRectangle(new Pen(nodeBorder, 2), rect);
                e.Graphics.DrawString(nd.Name, nodeFont, Brushes.Black, rect, textFmt);
                // trigger
                var trig = new Point(rect.Right - QuickTriggerSize, rect.Top);
                e.Graphics.FillEllipse(Brushes.Orange, new Rectangle(trig, new Size(QuickTriggerSize, QuickTriggerSize)));
            }

            // draw box selection rectangle
            if (isBoxSelecting)
            {
                using var brush = new SolidBrush(Color.FromArgb(60, Color.LightSkyBlue));
                e.Graphics.FillRectangle(brush, selectionRect);
                e.Graphics.DrawRectangle(Pens.DodgerBlue, selectionRect);
            }

            // highlight selected nodes
            foreach (var nd in selectedNodes)
            {
                var rect = new Rectangle(nd.Position, nodeSize);
                e.Graphics.DrawRectangle(new Pen(Color.Red, 2), rect);
            }

            // draw preview arrow
            if (pendingTransitionSource != null)
            {
                var srcCenter = pendingTransitionSource.Center(nodeSize);
                e.Graphics.DrawLine(previewPen, srcCenter, currentMouseWorld);
            }
        }

        private Point GetEdgePoint(Point center, Point toward)
        {
            double halfW = nodeSize.Width / 2.0;
            double halfH = nodeSize.Height / 2.0;
            double dx = toward.X - center.X;
            double dy = toward.Y - center.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1e-6) return center;
            double ux = dx / dist, uy = dy / dist;
            return new Point(
                (int)(center.X + ux * halfW),
                (int)(center.Y + uy * halfH));
        }
        #endregion

        #region Context Menu & Helpers
        private void InitContextMenu()
        {
            ctxMenu = new ContextMenuStrip();
            ctxMenu.Opening += (s, e) =>
            {
                ctxMenu.Items.Clear();
                if (selectedNodes.Count > 1)
                {
                    ctxMenu.Items.Add("复制选中节点", null, (s2, e2) =>
                    {
                        DuplicateSelectedNodes();
                        // clear selection after copying
                        selectedNodes.Clear();
                        selectionRect = Rectangle.Empty;
                        PushHistory();
                        Invalidate();
                    });
                }
                var pt = ScreenToWorld(PointToClient(Cursor.Position));
                var nd = FindNodeAt(pt);
                if (nd != null)
                {
                    ctxMenu.Items.Add("复制节点", null, (s2, e2) => { DuplicateNode(nd); PushHistory(); Invalidate(); });
                    ctxMenu.Items.Add("删除状态", null, (s2, e2) => { DeleteState(nd); PushHistory(); Invalidate(); });
                }
                ctxMenu.Items.Add("编辑 Mystery Block…", null, (s, e) => ShowMysteryDialog());

                ctxMenu.Items.Add(new ToolStripSeparator());
                menuUndo = new ToolStripMenuItem("撤销", null, (s2, e2) => Undo());
                menuRedo = new ToolStripMenuItem("重做", null, (s2, e2) => Redo());
                menuReset = new ToolStripMenuItem("重置到初始", null, (s2, e2) => ResetToOriginal());
                ctxMenu.Items.Add(menuUndo);
                ctxMenu.Items.Add(menuRedo);
                ctxMenu.Items.Add(menuReset);
                UpdateUndoRedoMenu();
            };
            ContextMenuStrip = ctxMenu;
        }
        private void ShowMysteryDialog()
        {
            if (currentFile == null) return;

            // 将当前的 byte[] 按 little-endian 解析成 short[]
            var bytes = currentFile.Header.MysteryBlock ?? Array.Empty<byte>();
            int shortCount = bytes.Length / 2;
            var shorts = new short[shortCount];
            for (int i = 0; i < shortCount; i++)
            {
                shorts[i] = BitConverter.ToInt16(bytes, i * 2);
            }
            string defaultText = string.Join(", ", shorts.Select(s => s.ToString()));

            using var form = new Form
            {
                Text = "编辑 Mystery Block（逗号分隔的 short）",
                Size = new Size(500, 300),
                StartPosition = FormStartPosition.CenterParent
            };
            var txt = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 10),
                Text = defaultText
            };
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var btnOK = new Button
            {
                Text = "应用",
                DialogResult = DialogResult.OK,
                Left = 10,
                Width = 80,
                Top = 8
            };
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Left = 100,
                Width = 80,
                Top = 8
            };
            panel.Controls.AddRange(new Control[] { btnOK, btnCancel });
            form.Controls.Add(txt);
            form.Controls.Add(panel);
            form.AcceptButton = btnOK;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 解析用户输入的 short 列表
                    var parts = txt.Text
                        .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var newShorts = parts.Select(p =>
                    {
                        if (!short.TryParse(p.Trim(), out var v))
                            throw new FormatException($"“{p}” 不是有效的 short 值");
                        return v;
                    }).ToArray();

                    // 把 short[] 转成 byte[]（little-endian）
                    var newBytes = new List<byte>(newShorts.Length * 2);
                    foreach (var s in newShorts)
                    {
                        newBytes.AddRange(BitConverter.GetBytes(s));
                    }

                    currentFile.Header.MysteryBlock = newBytes.ToArray();
                    // 同步更新 JSON 中的 MysteryBlockHex （保留原逻辑）
                    currentFile.Header.MysteryBlockHex =
                        BitConverter.ToString(currentFile.Header.MysteryBlock).Replace("-", " ");

                    // 把此操作记录到历史，以支持 Undo
                    PushHistory();
                    Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("解析失败：\n" + ex.Message,
                                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddTransition(Node src, Node dst)
        {
            var st = src.State;
            var tr = new Transition { StateIndex = dst.State.Index };
            tr.StructAbb.Unk01 = 1; // ensure correct type
            st.Transitions.Add(tr);
            st.TransitionCount = st.Transitions.Count;
        }

        private void DuplicateNode(Node nd)
        {
            var cloneSt = JsonConvert.DeserializeObject<State>(
                JsonConvert.SerializeObject(nd.State));
            cloneSt.Index = currentFile.States.Count;
            currentFile.States.Add(cloneSt);
            BuildNodes();
        }

        private void DuplicateSelectedNodes()
        {
            var clones = new List<State>();
            foreach (var nd in selectedNodes)
            {
                var st = nd.State;
                var clone = JsonConvert.DeserializeObject<State>(
                    JsonConvert.SerializeObject(st));
                clones.Add(clone);
            }
            foreach (var c in clones)
            {
                c.Index = currentFile.States.Count;
                currentFile.States.Add(c);
            }
            BuildNodes();
        }

        private void DeleteState(Node nd)
        {
            var st = nd.State;
            currentFile.States.Remove(st);
            // reindex
            for (int i = 0; i < currentFile.States.Count; i++)
                currentFile.States[i].Index = i;
            BuildNodes();
        }

        private void ShowStateProps(Node nd)
        {
            using var f = new Form { Text = $"状态 {nd.State.Index} 属性", Size = new Size(400, 400) };
            var pg = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = nd.State };
            f.Controls.Add(pg);
            f.ShowDialog();
            PushHistory(); Invalidate();
        }
        #endregion
    }
}
