using Molten.Samples;
using System;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.ShowDialog();
    }
}
