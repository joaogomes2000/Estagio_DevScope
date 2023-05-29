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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Create columns
            DataGridTextColumn column1 = new DataGridTextColumn();
            column1.Header = "ID";
            column1.Binding = new System.Windows.Data.Binding("PropertyName1");

            DataGridTextColumn column2 = new DataGridTextColumn();
            column2.Header = "Name";
            column2.Binding = new System.Windows.Data.Binding("PropertyName2");

            // Add columns to the DataGrid
            data.Columns.Insert(0, column1);
            data.Columns.Insert(1, column2);
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
        }
    }
}
