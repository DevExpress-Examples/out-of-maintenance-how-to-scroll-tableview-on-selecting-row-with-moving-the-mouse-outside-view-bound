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
Imports System.Collections.ObjectModel
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
			DataContext = New MyViewModel()
		End Sub


	End Class


	Public Class MyViewModel
		Public Sub New()
			CreateList()
		End Sub

		Public Property PersonList() As ObservableCollection(Of Person)
		Private Sub CreateList()
			PersonList = New ObservableCollection(Of Person)()
			For i As Integer = 0 To 19
				Dim p As New Person(i)
				PersonList.Add(p)
			Next i
		End Sub
	End Class
	Public Class Person
		Public Sub New(ByVal i As Integer)
			FirstName = "FirstName" & i
			LastName = "LastName" & i
			Age = i
		End Sub

		Public Property FirstName() As String

		Public Property LastName() As String

		Public Property Age() As Integer
	End Class
End Namespace
