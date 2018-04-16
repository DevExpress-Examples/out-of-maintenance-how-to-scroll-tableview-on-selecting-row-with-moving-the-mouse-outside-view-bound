// Developer Express Code Central Example:
// How to Select Rows via the mouse
// 
// This example demonstrates how to select rows by simply moving the mouse over
// them with the mouse button pressed
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E2725

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DXGrid_AssignComboBoxToColumn {
    public partial class Window1 : Window {
        public Window1() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid.ItemsSource = NwindData.Data;
        }
    }
    public class NwindData {
        public static nwindDataSet.ProductsDataTable Data {
            get { return new nwindDataSetTableAdapters.ProductsTableAdapter().GetData(); }
        }
    }
}
