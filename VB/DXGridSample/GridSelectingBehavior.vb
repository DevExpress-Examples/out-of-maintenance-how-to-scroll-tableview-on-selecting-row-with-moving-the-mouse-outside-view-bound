Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports DevExpress.Xpf.Grid
Imports System.Windows.Interactivity
Imports System.Windows.Input
Imports System.Windows
Imports System.Windows.Media
Imports DevExpress.Xpf.Core.Native
Imports System.Windows.Controls.Primitives

Namespace DXGrid_AssignComboBoxToColumn
	Public Class SelectionBehaviorSettings
		Public DragArea As Integer = 4
		Public StartScrollArea As Integer = 25
		Public UnselectedRowCountWhileScrolling As Integer = 0

		Public Function IsDragging(ByVal pt1 As Point, ByVal pt2 As Point) As Boolean
			Return Math.Abs(pt1.X - pt2.X) > DragArea OrElse Math.Abs(pt1.Y - pt2.Y) > DragArea
		End Function
		Public Function IsMouseOverTopScrollingArea(ByVal pt As Point) As Boolean
			Return pt.Y < StartScrollArea
		End Function
		Public Function IsMouseOverBottomScrollingArea(ByVal pt As Point, ByVal maxHeight As Double) As Boolean
			Return pt.Y > maxHeight - StartScrollArea
		End Function
		Public Function IsMouseOverLeftScrollingArea(ByVal pt As Point) As Boolean
			Return pt.X < StartScrollArea
		End Function
		Public Function IsMouseOverRightScrollingArea(ByVal pt As Point, ByVal maxWidth As Double) As Boolean
			Return pt.X > maxWidth - StartScrollArea
		End Function
	End Class
	Public Class SelectionInfo
		Public Property MousePoint() As Point
		Public Property RowHandle() As Integer
		Public Property Column() As GridColumn
		Public Overridable Sub Clear()
			MousePoint = InvalidPoint
			RowHandle = GridControl.InvalidRowHandle
			Column = Nothing
		End Sub
		Public Overridable Function IsEmptyInfo() As Boolean
			Return MousePoint = InvalidPoint
		End Function

		Private Shared ReadOnly InvalidPoint As New Point(-10000, 10000)
	End Class
	Public Class StartSelectionInfo
		Inherits SelectionInfo
		Public Property IsSelectionStarted() As Boolean
		Public ReadOnly Property IsLeftMouseButtonPressed() As Boolean
			Get
				Return Mouse.LeftButton = MouseButtonState.Pressed
			End Get
		End Property
		Public Overrides Sub Clear()
			MyBase.Clear()
			IsSelectionStarted = False
		End Sub
	End Class
	Public Class GridSelectingBehavior
		Inherits Behavior(Of GridControl)
		Public Property Settings() As SelectionBehaviorSettings
		Public ReadOnly Property Grid() As GridControl
			Get
				Return AssociatedObject
			End Get
		End Property
		Public ReadOnly Property View() As TableView
			Get
				Return CType(Grid.View, TableView)
			End Get
		End Property
		Protected Friend Property ScrollElement() As IScrollInfo
		Protected Friend Property DataArea() As FrameworkElement
		Protected Property StartSelectionInfo() As StartSelectionInfo
		Protected Property CurrentSelectionInfo() As SelectionInfo
		Protected Property ScrollController() As ScrollController

		Public Sub New()
			Settings = New SelectionBehaviorSettings()
			StartSelectionInfo = New StartSelectionInfo()
			CurrentSelectionInfo = New SelectionInfo()
			ScrollController = New ScrollController(Me)
			AddHandler ScrollController.ScrollDown, AddressOf OnScrollControllerScrollDown
			AddHandler ScrollController.ScrollUp, AddressOf OnScrollControllerScrollUp
			AddHandler ScrollController.ScrollLeft, AddressOf OnScrollControllerScrollLeft
			AddHandler ScrollController.ScrollRight, AddressOf OnScrollControllerScrollRight
		End Sub

		#Region " INITIALIZATION"
		Protected Overrides Sub OnAttached()
			MyBase.OnAttached()
			AddHandler Grid.Loaded, AddressOf OnGridLoaded
		End Sub
		Protected Overrides Sub OnDetaching()
			RemoveHandler Grid.PreviewMouseMove, AddressOf OnGridPreviewMouseMove
			RemoveHandler Grid.PreviewMouseDown, AddressOf OnGridPreviewMouseDown
			RemoveHandler Grid.PreviewMouseUp, AddressOf OnGridPreviewMouseUp
			RemoveHandler Grid.Loaded, AddressOf OnGridLoaded
			MyBase.OnDetaching()
		End Sub
		Private Sub OnGridLoaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			AddHandler View.LayoutUpdated, AddressOf OnViewLayoutUpdated
		End Sub
		Private Sub OnViewLayoutUpdated(ByVal sender As Object, ByVal e As EventArgs)
			If Grid.SelectionMode = MultiSelectMode.None Then
				Throw New Exception("GridSelectingBehavior does not allow using the MultiSelectMode.None.")
			End If

			DataArea = LayoutHelper.FindElementByName(View, "PART_ScrollContentPresenter")
			If DataArea Is Nothing Then
				Return
			End If
            'ScrollElement = CType(LayoutHelper.FindElement(DataArea, Function(TypeOf el) el Is DataPresenter), DataPresenter)
            ScrollElement = CType(LayoutHelper.FindElement(DataArea, Function(el) (TypeOf el Is DataPresenter)), DataPresenter)
			If ScrollElement Is Nothing Then
				Return
			End If

			RemoveHandler View.LayoutUpdated, AddressOf OnViewLayoutUpdated

			AddHandler Grid.PreviewMouseMove, AddressOf OnGridPreviewMouseMove
			AddHandler Grid.PreviewMouseDown, AddressOf OnGridPreviewMouseDown
			AddHandler Grid.PreviewMouseUp, AddressOf OnGridPreviewMouseUp
		End Sub
		#End Region

		Private Sub OnGridPreviewMouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			If (Not StartSelectionInfo.IsSelectionStarted) Then
				Return
			End If
			Mouse.Capture(Nothing)
			ScrollController.StopHorizontalScrolling()
			ScrollController.StopVerticalScrolling()
			StartSelectionInfo.Clear()
			CurrentSelectionInfo.Clear()
			e.Handled = True
		End Sub
		Private Sub OnGridPreviewMouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			StartSelectionInfo.Clear()
			Dim hitInfo As TableViewHitInfo = View.CalcHitInfo(TryCast(e.OriginalSource, DependencyObject))
			If (Not hitInfo.InRow) AndAlso (Not hitInfo.InRowCell) Then
				Return
			End If
			StartSelectionInfo.MousePoint = e.GetPosition(DataArea)
			StartSelectionInfo.RowHandle = hitInfo.RowHandle
			StartSelectionInfo.Column = hitInfo.Column
		End Sub
		Private Sub OnGridPreviewMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
			If (Not StartSelectionInfo.IsLeftMouseButtonPressed) OrElse StartSelectionInfo.IsEmptyInfo() Then
				Return
			End If
			If (Not StartSelectionInfo.IsSelectionStarted) Then
				If (Not Settings.IsDragging(StartSelectionInfo.MousePoint, e.GetPosition(DataArea))) Then
					Return
				End If
				Mouse.Capture(DataArea, CaptureMode.SubTree)
				StartSelectionInfo.IsSelectionStarted = True
			End If

			UpdateCurrentSelectionInfo()

			UpdateVerticalScrolling(CurrentSelectionInfo.MousePoint)
			UpdateHorizontalScrolling(CurrentSelectionInfo.MousePoint)

			If (Not ScrollController.IsScrollWorking) Then
				UpdateSelection()
			End If

			ScrollController.UpdateVerticalScrollTimerInterval(DataArea.ActualHeight, e.GetPosition(DataArea).Y)
			ScrollController.UpdateHorizontalScrollTimerInterval(DataArea.ActualWidth, e.GetPosition(DataArea).X)
			e.Handled = True
		End Sub

		'TODO: improve this method
		Private Sub UpdateCurrentSelectionInfo()
			CurrentSelectionInfo.MousePoint = Mouse.GetPosition(DataArea)
			Dim pt As Point = CurrentSelectionInfo.MousePoint
			'Inside DataArea
			If pt.X > 0 AndAlso pt.Y > 0 AndAlso pt.X < DataArea.ActualWidth AndAlso pt.Y < DataArea.ActualHeight Then
				Dim result As HitTestResult = VisualTreeHelper.HitTest(DataArea, CurrentSelectionInfo.MousePoint)
				If result IsNot Nothing Then
					Dim hittedObject As DependencyObject = result.VisualHit
					Dim hitInfo As TableViewHitInfo = View.CalcHitInfo(hittedObject)
					CurrentSelectionInfo.RowHandle = hitInfo.RowHandle
					CurrentSelectionInfo.Column = hitInfo.Column
				End If
				Return
			End If
			'Outside DataArea
			CurrentSelectionInfo.RowHandle = GetRowhandle(CurrentSelectionInfo.MousePoint.Y)
			'Select to right
			Dim rightColumn, rightColumnWhenUnselect As GridColumn
			GetRightVisibleColumn(rightColumn, rightColumnWhenUnselect)
			If pt.X >= DataArea.ActualWidth AndAlso StartSelectionInfo.Column.VisibleIndex <= CurrentSelectionInfo.Column.VisibleIndex Then
				If rightColumn.VisibleIndex > CurrentSelectionInfo.Column.VisibleIndex Then
					CurrentSelectionInfo.Column = rightColumn
				End If
				Return
			End If
			'Unselecting to right
			If pt.X >= DataArea.ActualWidth AndAlso StartSelectionInfo.Column.VisibleIndex >= CurrentSelectionInfo.Column.VisibleIndex Then
				If rightColumnWhenUnselect.VisibleIndex >= CurrentSelectionInfo.Column.VisibleIndex Then
					CurrentSelectionInfo.Column = rightColumnWhenUnselect
				End If
				Return
			End If
			'Select to left
			If pt.X <= 0 Then
				Dim col As GridColumn = GetLeftVisibleColumn()
					CurrentSelectionInfo.Column = col
				Return
			End If
		End Sub
		Private Sub GetRightVisibleColumn(<System.Runtime.InteropServices.Out()> ByRef rightColumn As GridColumn, <System.Runtime.InteropServices.Out()> ByRef rightColumnWhenUnselect As GridColumn)
			rightColumn = Nothing
			rightColumnWhenUnselect = Nothing
			Dim maxWidth As Double = ScrollElement.HorizontalOffset + ScrollElement.ViewportWidth
			Dim columnsWidth As Double = 0R
			rightColumn = FindVisibleColumn(Function(w) w > maxWidth, columnsWidth)
			rightColumnWhenUnselect = FindVisibleColumn(Function(w) w >= maxWidth, columnsWidth)
			If rightColumnWhenUnselect Is Nothing Then
				rightColumnWhenUnselect = View.VisibleColumns.Last()
			End If
			If rightColumnWhenUnselect.VisibleIndex - 1 <= 0 Then
				rightColumnWhenUnselect = View.VisibleColumns.First()
			End If

			If rightColumn Is Nothing Then
				rightColumn = View.VisibleColumns.Last()
			End If

			columnsWidth = maxWidth - (columnsWidth - rightColumn.Width)
			If columnsWidth <= rightColumn.Width / 2 Then
				If rightColumn.VisibleIndex - 1 > 0 Then
					rightColumn = View.VisibleColumns(rightColumn.VisibleIndex - 1)
				Else
					rightColumn = View.VisibleColumns.First()
				End If
			End If
		End Sub
		Private Function GetLeftVisibleColumn() As GridColumn
			If ScrollElement.HorizontalOffset < View.VisibleColumns.First().Width / 2 Then
				Return View.VisibleColumns.First()
			End If
			Dim maxWidth As Double = ScrollElement.HorizontalOffset
			Dim columnsWidth As Double = 0R
			Dim res As GridColumn = FindVisibleColumn(Function(w) w > maxWidth, columnsWidth)
			columnsWidth = columnsWidth - res.Width
			If maxWidth < columnsWidth + res.Width / 2 Then
				Return res
			Else
				Return View.VisibleColumns(res.VisibleIndex + 1)
			End If
		End Function
		Private Function FindVisibleColumn(ByVal cond As Func(Of Double, Boolean), ByRef columnsWidth As Double) As GridColumn
			columnsWidth = 0R
			Dim res As GridColumn = Nothing
			For Each gc As GridColumn In View.VisibleColumns
				columnsWidth += gc.Width
				If cond(columnsWidth) Then
					res = gc
					Exit For
				End If
			Next gc
			Return res
		End Function


		Private Function GetRowhandle(ByVal mouseYPosition As Double) As Integer
			Dim avgRowHeight As Double = DataArea.ActualHeight / ScrollElement.ViewportHeight
			Dim currentRowIndex As Integer = 0
			Dim summaryHeight As Double = 0
			Do While summaryHeight < mouseYPosition
				summaryHeight += avgRowHeight
				currentRowIndex += 1
			Loop
			Return Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + currentRowIndex)
		End Function

		Private Sub UpdateSelection()
			Grid.BeginSelection()
			Grid.UnselectAll()
			If Grid.SelectionMode = MultiSelectMode.Row Then
				Grid.SelectRange(StartSelectionInfo.RowHandle, CurrentSelectionInfo.RowHandle)
			Else
				View.SelectCells(StartSelectionInfo.RowHandle, StartSelectionInfo.Column, CurrentSelectionInfo.RowHandle, CurrentSelectionInfo.Column)
			End If
			Grid.EndSelection()
		End Sub

		Private Sub UpdateVerticalScrolling(ByVal pt As Point)
			If Settings.IsMouseOverTopScrollingArea(pt) AndAlso ScrollController.CanScrollUp Then
				ScrollController.StartScrollUp()
			ElseIf Settings.IsMouseOverBottomScrollingArea(pt, DataArea.ActualHeight) AndAlso ScrollController.CanScrollDown Then
				ScrollController.StartScrollDown()
			Else
				ScrollController.StopVerticalScrolling()
			End If
		End Sub
		Private Sub UpdateHorizontalScrolling(ByVal pt As Point)
			If Settings.IsMouseOverLeftScrollingArea(pt) AndAlso ScrollController.CanScrollLeft Then
				ScrollController.StartScrollLeft()
			ElseIf Settings.IsMouseOverRightScrollingArea(pt, DataArea.ActualWidth) AndAlso ScrollController.CanScrollRight Then
				ScrollController.StartScrollRight()
			Else
				ScrollController.StopHorizontalScrolling()
			End If
		End Sub

		Private Sub OnScrollControllerScrollUp(ByVal sender As Object, ByVal e As EventArgs)
			If ScrollController.CanScrollUp Then
				CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + Settings.UnselectedRowCountWhileScrolling)
			Else
				CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex)
			End If
			UpdateSelection()
		End Sub
		Private Sub OnScrollControllerScrollDown(ByVal sender As Object, ByVal e As EventArgs)
			If ScrollController.CanScrollDown Then
				CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + ScrollController.VisibleRowCount - Settings.UnselectedRowCountWhileScrolling)
			Else
				CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(Grid.VisibleRowCount - 1)
			End If
			UpdateSelection()
		End Sub
		Private Sub OnScrollControllerScrollLeft(ByVal sender As Object, ByVal e As EventArgs)
			If ScrollController.CanScrollLeft Then
				UpdateCurrentSelectionInfo()
				UpdateSelection()
			End If
		End Sub
		Private Sub OnScrollControllerScrollRight(ByVal sender As Object, ByVal e As EventArgs)
			If ScrollController.CanScrollRight Then
				UpdateCurrentSelectionInfo()
			End If
				UpdateSelection()
		End Sub
	End Class

End Namespace


