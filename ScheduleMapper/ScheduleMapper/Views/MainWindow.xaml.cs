using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ScheduleMapper.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WaypointButton_Click(object sender, RoutedEventArgs e)
        {
            int DestIndex = MainStack.Children.IndexOf(DestinationStack);
            StackPanel newPanel = new StackPanel();
            newPanel.Orientation = Orientation.Horizontal;
            Label l = new Label();
            l.Content = "Waypoint " + DestIndex + ":";
            ComboBox cb = new ComboBox();
            cb.DisplayMemberPath = "Name";
            cb.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding("Coordinates"));
            cb.SetBinding(
                Selector.SelectedIndexProperty,
                new Binding("WaypointIndex[" + (DestIndex - 1) + "]"));
            newPanel.Children.Add(l);
            newPanel.Children.Add(cb);
            MainStack.Children.Insert(DestIndex, newPanel);
            ((dynamic)DataContext).WaypointIndex.Add(0);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int imageCount = ((dynamic)DataContext).Images.Count;
            ((dynamic)DataContext).Run();
            while (MainStack.Children.IndexOf(DestinationStack) > 1)
                MainStack.Children.RemoveAt(1);
            ((dynamic)DataContext).WaypointIndex.Clear();
            new MapDisplay(imageCount) { DataContext = DataContext }.Show();
        }
    }
}
