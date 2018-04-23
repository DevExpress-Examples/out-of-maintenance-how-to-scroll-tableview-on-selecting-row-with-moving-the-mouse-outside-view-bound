// Developer Express Code Central Example:
// How to scroll TableView on selecting row/cell with moving the mouse outside view bounds
// 
// This example illustrates how to add the DXGrid behavior which provides the
// capability to select rows and cells by simply moving the mouse over them with
// the mouse button pressed and auto-scrolling the view in a necessary
// direction.
// This functionality was implemented via attached behavior for DXGrid
// which encapsulates all the selection functionality. The scrolling functionality
// was implemented in a separate class named ScrollController.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E2725

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
            DataContext = new SampleSource1();
        }
    }
}
