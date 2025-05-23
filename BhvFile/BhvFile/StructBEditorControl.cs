using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace BHVEditor
{
    public partial class StructBEditorControl : UserControl
    {
        private BindingList<StructB> structBs;

        // 当 StructB 列表发生变化时触发，便于外部刷新显示
        public event EventHandler StructBsChanged;

        public StructBEditorControl()
        {
            InitializeComponent();
            // 监听属性修改事件，以便刷新
            pgStructB.PropertyValueChanged += PgStructB_PropertyValueChanged;
        }

        public void LoadStructBs(BindingList<StructB> list)
        {
            structBs = list;
            lstStructB.DataSource = structBs;
            lstStructB.DisplayMember = "Unk00"; // 显示第一个字段
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            StructB sb;
            if (lstStructB.SelectedItem is StructB prev)
            {
                // 克隆前一行的所有字段
                sb = (StructB)prev.GetType()
                    .GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(prev, null);
            }
            else
            {
                sb = new StructB();
            }
            structBs.Add(sb);
            lstStructB.SelectedItem = sb;
            OnStructBsChanged();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstStructB.SelectedItem is StructB sb)
            {
                structBs.Remove(sb);
                OnStructBsChanged();
            }
        }

        private void lstStructB_SelectedIndexChanged(object sender, EventArgs e)
        {
            pgStructB.SelectedObject = lstStructB.SelectedItem;
        }

        private void PgStructB_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            OnStructBsChanged();
        }

        protected virtual void OnStructBsChanged()
        {
            StructBsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    partial class StructBEditorControl
    {
        private ListBox lstStructB;
        private PropertyGrid pgStructB;
        private Button btnAdd;
        private Button btnRemove;

        private void InitializeComponent()
        {
            this.lstStructB = new System.Windows.Forms.ListBox();
            this.pgStructB = new System.Windows.Forms.PropertyGrid();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstStructB
            // 
            this.lstStructB.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstStructB.Width = 150;
            this.lstStructB.SelectedIndexChanged += new System.EventHandler(this.lstStructB_SelectedIndexChanged);
            // 
            // pgStructB
            // 
            this.pgStructB.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // btnAdd
            // 
            this.btnAdd.Text = "添加";
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnAdd.Height = 30;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Text = "删除";
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnRemove.Height = 30;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // StructBEditorControl
            // 
            this.Controls.Add(this.pgStructB);
            this.Controls.Add(this.lstStructB);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.ResumeLayout(false);
        }
    }
}
