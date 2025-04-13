using System;
using System.Windows.Forms;
using TreeOfLifeVisualization.Views;

namespace TreeOfLifeVisualization
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Views.View());
        }
    }
}
