using System;
using System.Drawing;
using System.Windows.Forms;

namespace BHVEditor
{
    /// <summary>
    /// 一个简单的通用输入对话框，用于替代 Microsoft.VisualBasic.Interaction.InputBox。
    /// Show 方法会弹出一个模态窗体，提示用户输入文本，并返回用户输入的字符串或 null（取消时）。
    /// </summary>
    public static class PromptDialog
    {
        /// <summary>
        /// 弹出一个对话框，有标题、提示文本和默认值，返回输入结果或 null。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="promptText">提示文字</param>
        /// <param name="defaultValue">默认输入</param>
        /// <returns>用户输入的文本，或 null 表示取消</returns>
        public static string Show(string title, string promptText, string defaultValue = "")
        {
            using (var form = new Form())
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ClientSize = new Size(400, 130);
                form.Text = title;

                // 提示标签
                var lbl = new Label()
                {
                    AutoSize = true,
                    Location = new Point(12, 15),
                    Text = promptText
                };
                form.Controls.Add(lbl);

                // 文本输入框
                var txt = new TextBox()
                {
                    Location = new Point(12, lbl.Bottom + 8),
                    Width = form.ClientSize.Width - 24,
                    Text = defaultValue
                };
                form.Controls.Add(txt);

                // OK 按钮
                var btnOk = new Button()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(form.ClientSize.Width - 180, form.ClientSize.Height - 40),
                    Size = new Size(80, 25)
                };
                form.Controls.Add(btnOk);

                // Cancel 按钮
                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(form.ClientSize.Width - 90, form.ClientSize.Height - 40),
                    Size = new Size(80, 25)
                };
                form.Controls.Add(btnCancel);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                // 显示对话框
                var result = form.ShowDialog();
                return result == DialogResult.OK ? txt.Text : null;
            }
        }
    }
}
