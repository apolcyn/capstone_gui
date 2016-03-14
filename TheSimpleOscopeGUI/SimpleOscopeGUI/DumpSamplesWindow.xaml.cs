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
using System.Windows.Shapes;

namespace SimpleOscope
{
    /// <summary>
    /// Interaction logic for DumpSamplesWindow.xaml
    /// </summary>
    public partial class DumpSamplesWindow : Window
    {
        string fileName;

        public DumpSamplesWindow(string initialFileName)
        {
            InitializeComponent();
            this.file_name_textbox.Text = initialFileName;
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            fileName = this.file_name_textbox.Text;
        }

        public string Answer()
        {
            return fileName;
        }
    }
}
