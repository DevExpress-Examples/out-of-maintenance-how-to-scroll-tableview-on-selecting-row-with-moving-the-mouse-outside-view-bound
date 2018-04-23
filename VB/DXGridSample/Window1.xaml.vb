' Developer Express Code Central Example:
' How to scroll TableView on selecting row/cell with moving the mouse outside view bounds
' 
' This example illustrates how to add the DXGrid behavior which provides the
' capability to select rows and cells by simply moving the mouse over them with
' the mouse button pressed and auto-scrolling the view in a necessary
' direction.
' This functionality was implemented via attached behavior for DXGrid
' which encapsulates all the selection functionality. The scrolling functionality
' was implemented in a separate class named ScrollController.
' 
' You can find sample updates and versions for different programming languages here:
' http://www.devexpress.com/example=E2725

' Developer Express Code Central Example:
' How to Select Rows via the mouse
' 
' This example demonstrates how to select rows by simply moving the mouse over
' them with the mouse button pressed
' 
' You can find sample updates and versions for different programming languages here:
' http://www.devexpress.com/example=E2725


Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes

Namespace DXGrid_AssignComboBoxToColumn
	Partial Public Class Window1
		Inherits Window
		Public Sub New()
			InitializeComponent()
			DataContext = New SampleSource1()
		End Sub
	End Class
End Namespace
