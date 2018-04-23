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
            grid.DataSource = NwindData.Data;
        }

        private int StartRowHandle = -1;
        private int CurrentRowHandle = -1;



        private void TableView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartRowHandle = GetRowAt(e.OriginalSource as DependencyObject);
        }

        private void TableView_MouseMove(object sender, MouseEventArgs e)
        {
            int newRowHandle = GetRowAt(e.OriginalSource as DependencyObject);
            if (CurrentRowHandle != newRowHandle)
            {
                CurrentRowHandle = newRowHandle;
                SelectRows(StartRowHandle, CurrentRowHandle);
            }
        }

        private void TableView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartRowHandle = -1;
            CurrentRowHandle = -1;
        }
        private int GetRowAt(DependencyObject sourceObj)
        {
            return myTableView.CalcHitInfo(sourceObj).RowHandle;
        }
        private void SelectRows(int startRow, int endRow)
        {
            if (startRow > -1 && endRow > -1)
            {
                myTableView.BeginSelection();
                myTableView.ClearSelection();
                myTableView.SelectRange(startRow, endRow);
                myTableView.EndSelection();
            }
        }

    }
    public class NwindData {
        public static nwindDataSet.ProductsDataTable Data {
            get { return new nwindDataSetTableAdapters.ProductsTableAdapter().GetData(); }
        }
    }
}
