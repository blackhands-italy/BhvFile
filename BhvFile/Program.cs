using BhvFile;
using System;
using System.Windows.Forms;

namespace BHVEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // 启动 WinForms 环境
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 运行主窗口
            Application.Run(new MainForm());
        }
    }
}
