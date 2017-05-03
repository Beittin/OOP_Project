using ScheduleMapper.ViewModels;
using System.Windows;

namespace ScheduleMapper.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow w = new MainWindow();
            MainWindow_VM vm = new MainWindow_VM();
            MainWindow = w;
            w.DataContext = vm;
            w.Show();
        }
    }
}
