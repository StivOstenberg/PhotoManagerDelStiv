using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoManagerDelStiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SourceRoot = @"D:\Pictures2Process";
        public string DestRoot = @"D:\Camera";
        
        public string Destination1 = @"C:\_OneDrive\OneDrive\Mobile Pictures\Scenic & Travel\Gypsies";
        public string Dest1Name = @"OneNote Gypsies";

        public string Destination2 = @"C:\Users\stiv\Desktop\2blog";
        public string Dest2Name = "To Blog";

        public string Destination3 = "";

        public string Destination4 = "";
        public string Destroot = "";


        public MainWindow()
        {
            InitializeComponent();
            // Put settings load here eventually
            SetButtons();

        }

        private void SetButtons()
        {
            Dest1Button.Content = Dest1Name;
            Dest1Button.ToolTip = Destination1;
            Dest2Button.Content = Dest2Name;
            Dest2Button.ToolTip = Destination2;

        }

        private void SelectTargetRoot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
