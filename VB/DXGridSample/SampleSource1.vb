Imports Microsoft.VisualBasic
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Namespace DXGrid_AssignComboBoxToColumn
	Public Class SampleSource1
		Implements INotifyPropertyChanged
		Private currentItem_Renamed As SampleItem
		Private privateItems As ObservableCollection(Of SampleItem)
		Public Property Items() As ObservableCollection(Of SampleItem)
			Get
				Return privateItems
			End Get
			Set(ByVal value As ObservableCollection(Of SampleItem))
				privateItems = value
			End Set
		End Property
		Public Property CurrentItem() As SampleItem
			Get
				Return currentItem_Renamed
			End Get
			Set(ByVal value As SampleItem)
				If currentItem_Renamed Is value Then
					Return
				End If
				currentItem_Renamed = value
				RaisePropertyChanged("CurrentItem")
			End Set
		End Property
		Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
		Public Sub New()
			Items = New ObservableCollection(Of SampleItem)()
			InitItems()
		End Sub
		Private Sub InitItems()
			For i As Integer = 0 To 99
				Dim item As New SampleItem() With {.Id = i, .Name = "item " & i.ToString()}
				Items.Add(item)
			Next i
		End Sub
		Private Sub RaisePropertyChanged(ByVal propertyName As String)
			If PropertyChangedEvent Is Nothing Then
				Return
			End If
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
		End Sub
	End Class

	Public Class SampleItem
		Implements INotifyPropertyChanged
		Private id_Renamed As Integer
		Private name_Renamed As String
		Public Property Id() As Integer
			Get
				Return id_Renamed
			End Get
			Set(ByVal value As Integer)
				If id_Renamed = value Then
					Return
				End If
				id_Renamed = value
				RaisePropertyChanged("Id")
			End Set
		End Property
		Public Property Name() As String
			Get
				Return name_Renamed
			End Get
			Set(ByVal value As String)
				If name_Renamed = value Then
					Return
				End If
				name_Renamed = value
				RaisePropertyChanged("Name")
			End Set
		End Property
		Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
		Private Sub RaisePropertyChanged(ByVal fieldName As String)
			If PropertyChangedEvent Is Nothing Then
				Return
			End If
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(fieldName))
		End Sub
	End Class
End Namespace
