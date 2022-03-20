using Molten.Samples;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.ShowDialog();
    }
}
