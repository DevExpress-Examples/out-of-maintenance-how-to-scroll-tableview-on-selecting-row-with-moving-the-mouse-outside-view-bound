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
		End Sub

		Private Sub Window_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			grid.ItemsSource = NwindData.Data
		End Sub

		'private int StartRowHandle = -1;
		'private int CurrentRowHandle = -1;

		'private void TableView_MouseMove(object sender, MouseEventArgs e)
		'{
		'    int newRowHandle = GetRowAt(e.OriginalSource as DependencyObject);
		'    if (CurrentRowHandle != newRowHandle)
		'    {
		'        CurrentRowHandle = newRowHandle;
		'        SelectRows(StartRowHandle, CurrentRowHandle);
		'    }
		'}

		'private void TableView_MouseUp(object sender, MouseButtonEventArgs e)
		'{
		'    StartRowHandle = -1;
		'    CurrentRowHandle = -1;
		'}
		'private int GetRowAt(DependencyObject sourceObj)
		'{
		'    return myTableView.CalcHitInfo(sourceObj).RowHandle;
		'}
		'private void SelectRows(int startRow, int endRow)
		'{
		'    if (startRow > -1 && endRow > -1)
		'    {
		'        myTableView.BeginSelection();
		'        myTableView.ClearSelection();
		'        myTableView.SelectRange(startRow, endRow);
		'        myTableView.EndSelection();
		'    }
		'}

		'private void grid_MouseDown(object sender, MouseButtonEventArgs e) {
		'    StartRowHandle = GetRowAt(e.OriginalSource as DependencyObject);
		'}

		'private void grid_MouseLeave(object sender, MouseEventArgs e) {
		'    grid.View.ScrollIntoView(grid.View.FocusedRowHandle++); 
		'}

		'private void grid_KeyDown(object sender, KeyEventArgs e) {

		'        //grid.View.TopRowIndex = 0;
		'        ////
		'}

		'private void myTableView_KeyDown(object sender, KeyEventArgs e){
		'    //if (e.Key == Key.F)
		'    //    grid.View.ScrollIntoView(grid.View.FocusedRowHandle++);
		'}
	End Class
	Public Class NwindData
		Public Shared ReadOnly Property Data() As nwindDataSet.ProductsDataTable
			Get
				Return New nwindDataSetTableAdapters.ProductsTableAdapter().GetData()
			End Get
		End Property
	End Class
End Namespace
