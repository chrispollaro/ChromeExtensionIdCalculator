using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

using ChromeExtensionsIdCalculator;

namespace ChromeExtensionIdUtility
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

        private void PEMBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                PEMLocation.Content = openFileDialog.FileName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var extCalculator = new ExtensionIdentifier();
            string path = PEMLocation.Content as string;
            var id = extCalculator.CalculateId(path);
            ExtensionId.Text = id;
        }
    }
}
