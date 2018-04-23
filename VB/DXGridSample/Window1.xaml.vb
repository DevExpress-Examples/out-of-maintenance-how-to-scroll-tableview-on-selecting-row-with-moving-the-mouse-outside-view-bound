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

		Private StartRowHandle As Integer = -1
		Private CurrentRowHandle As Integer = -1



		Private Sub TableView_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			StartRowHandle = GetRowAt(TryCast(e.OriginalSource, DependencyObject))
		End Sub

		Private Sub TableView_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
			Dim newRowHandle As Integer = GetRowAt(TryCast(e.OriginalSource, DependencyObject))
			If CurrentRowHandle <> newRowHandle Then
				CurrentRowHandle = newRowHandle
				SelectRows(StartRowHandle, CurrentRowHandle)
			End If
		End Sub

		Private Sub TableView_MouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			StartRowHandle = -1
			CurrentRowHandle = -1
		End Sub
		Private Function GetRowAt(ByVal sourceObj As DependencyObject) As Integer
			Return myTableView.CalcHitInfo(sourceObj).RowHandle
		End Function
		Private Sub SelectRows(ByVal startRow As Integer, ByVal endRow As Integer)
			If startRow > -1 AndAlso endRow > -1 Then
				myTableView.BeginSelection()
				myTableView.ClearSelection()
				myTableView.SelectRange(startRow, endRow)
				myTableView.EndSelection()
			End If
		End Sub

	End Class
	Public Class NwindData
		Public Shared ReadOnly Property Data() As nwindDataSet.ProductsDataTable
			Get
				Return New nwindDataSetTableAdapters.ProductsTableAdapter().GetData()
			End Get
		End Property
	End Class
End Namespace
