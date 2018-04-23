Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports DevExpress.Xpf.Grid
Imports System.Windows.Input
Imports System.Windows
Imports System.Windows.Media
Imports DevExpress.Xpf.Core.Native
Imports System.Windows.Controls.Primitives
Imports System.ComponentModel
Imports System.Windows.Controls
Imports DevExpress.Mvvm.UI.Interactivity

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
		Private privateMousePoint As Point
		Public Property MousePoint() As Point
			Get
				Return privateMousePoint
			End Get
			Set(ByVal value As Point)
				privateMousePoint = value
			End Set
		End Property
		Private privateRowHandle As Integer
		Public Property RowHandle() As Integer
			Get
				Return privateRowHandle
			End Get
			Set(ByVal value As Integer)
				privateRowHandle = value
			End Set
		End Property
		Private privateColumn As GridColumn
		Public Property Column() As GridColumn
			Get
				Return privateColumn
			End Get
			Set(ByVal value As GridColumn)
				privateColumn = value
			End Set
		End Property
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
		Private privateIsSelectionStarted As Boolean
		Public Property IsSelectionStarted() As Boolean
			Get
				Return privateIsSelectionStarted
			End Get
			Set(ByVal value As Boolean)
				privateIsSelectionStarted = value
			End Set
		End Property
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
		Private IsLogicBlocked As Boolean
		Private privateSettings As SelectionBehaviorSettings
		Public Property Settings() As SelectionBehaviorSettings
			Get
				Return privateSettings
			End Get
			Set(ByVal value As SelectionBehaviorSettings)
				privateSettings = value
			End Set
		End Property
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
		Private privateScrollElement As IScrollInfo
		Protected Friend Property ScrollElement() As IScrollInfo
			Get
				Return privateScrollElement
			End Get
			Private Set(ByVal value As IScrollInfo)
				privateScrollElement = value
			End Set
		End Property
		Private privateDataArea As FrameworkElement
		Protected Friend Property DataArea() As FrameworkElement
			Get
				Return privateDataArea
			End Get
			Private Set(ByVal value As FrameworkElement)
				privateDataArea = value
			End Set
		End Property
		Private privateStartSelectionInfo As StartSelectionInfo
		Protected Property StartSelectionInfo() As StartSelectionInfo
			Get
				Return privateStartSelectionInfo
			End Get
			Private Set(ByVal value As StartSelectionInfo)
				privateStartSelectionInfo = value
			End Set
		End Property
		Private privateCurrentSelectionInfo As SelectionInfo
		Protected Property CurrentSelectionInfo() As SelectionInfo
			Get
				Return privateCurrentSelectionInfo
			End Get
			Private Set(ByVal value As SelectionInfo)
				privateCurrentSelectionInfo = value
			End Set
		End Property
		Private privateScrollController As ScrollController
		Protected Property ScrollController() As ScrollController
			Get
				Return privateScrollController
			End Get
			Set(ByVal value As ScrollController)
				privateScrollController = value
			End Set
		End Property

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
			Dim dpd = DependencyPropertyDescriptor.FromProperty(GridControl.SelectionModeProperty, GetType(GridControl))
			dpd.AddValueChanged(Grid, AddressOf OnSelectionModeChanged)
			MyBase.OnDetaching()
		End Sub

		Private Sub OnSelectionModeChanged(ByVal sender As Object, ByVal e As EventArgs)
			If Grid.SelectionMode <> MultiSelectMode.Row OrElse Grid.SelectionMode <> MultiSelectMode.MultipleRow Then
				Grid.UnselectAll()
				IsLogicBlocked = True
			Else
				IsLogicBlocked = False
			End If
		End Sub
		Private Sub OnGridLoaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			AddHandler View.LayoutUpdated, AddressOf OnViewLayoutUpdated
		End Sub
		Private Sub OnViewLayoutUpdated(ByVal sender As Object, ByVal e As EventArgs)
			If Grid.SelectionMode = MultiSelectMode.None Then
				IsLogicBlocked = True
			End If
			DataArea = LayoutHelper.FindElement(View, AddressOf IsScrollContentPresenterAndHasRowPresenterGridAsParent)
			If DataArea Is Nothing Then
				Return
			End If
			ScrollElement = CType(LayoutHelper.FindElement(DataArea, AddressOf IsDataPresenter), DataPresenter)
			If ScrollElement Is Nothing Then
				Return
			End If

			RemoveHandler View.LayoutUpdated, AddressOf OnViewLayoutUpdated

			AddHandler Grid.PreviewMouseMove, AddressOf OnGridPreviewMouseMove
			AddHandler Grid.PreviewMouseDown, AddressOf OnGridPreviewMouseDown
			AddHandler Grid.PreviewMouseUp, AddressOf OnGridPreviewMouseUp
		End Sub

		Private Function IsScrollContentPresenter(ByVal e As FrameworkElement) As Boolean
			Return e.Name = "PART_ScrollContentPresenter"
		End Function
		Private Function IsRowPresenterGrid(ByVal e As DependencyObject) As Boolean
			Return (TypeOf e Is Grid) AndAlso ((CType(e, FrameworkElement)).Name = "rowPresenterGrid")
		End Function
		Private Function IsDataPresenter(ByVal e As FrameworkElement) As Boolean
			Return TypeOf e Is DataPresenter
		End Function
		Private Function IsScrollContentPresenterAndHasRowPresenterGridAsParent(ByVal e As FrameworkElement) As Boolean
			Return IsScrollContentPresenter(e) AndAlso LayoutHelper.FindLayoutOrVisualParentObject(e, AddressOf IsRowPresenterGrid) IsNot Nothing
		End Function
		#End Region

		Private Sub OnGridPreviewMouseUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)

			If (Not IsLogicBlocked) Then
				If (Not StartSelectionInfo.IsSelectionStarted) Then
					Return
				End If
				Mouse.Capture(Nothing)
				ScrollController.StopHorizontalScrolling()
				ScrollController.StopVerticalScrolling()
				StartSelectionInfo.Clear()
				CurrentSelectionInfo.Clear()
				e.Handled = True
			End If
		End Sub
		Private Sub OnGridPreviewMouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			If (Not IsLogicBlocked) Then
				StartSelectionInfo.Clear()
				Dim hitInfo As TableViewHitInfo = View.CalcHitInfo(TryCast(e.OriginalSource, DependencyObject))
				If (Not hitInfo.InRow) AndAlso (Not hitInfo.InRowCell) Then
					Return
				End If
				StartSelectionInfo.MousePoint = e.GetPosition(DataArea)
				StartSelectionInfo.RowHandle = hitInfo.RowHandle
				StartSelectionInfo.Column = hitInfo.Column
			End If
		End Sub
		Private Sub OnGridPreviewMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
			If (Not IsLogicBlocked) Then
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
			End If
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
					If hitInfo.RowHandle <> GridControl.InvalidRowHandle Then
						CurrentSelectionInfo.RowHandle = hitInfo.RowHandle
					End If
					If hitInfo.Column IsNot Nothing Then
						CurrentSelectionInfo.Column = hitInfo.Column
					End If
				End If
				Return
			End If
			'Outside DataArea
			CurrentSelectionInfo.RowHandle = GetRowhandle(CurrentSelectionInfo.MousePoint.Y)
			'Select to right
			Dim rightColumn, rightColumnWhenUnselect As GridColumn
			GetRightVisibleColumn(rightColumn, rightColumnWhenUnselect)
			If pt.X >= DataArea.ActualWidth AndAlso CurrentSelectionInfo.Column IsNot Nothing AndAlso StartSelectionInfo.Column.VisibleIndex <= CurrentSelectionInfo.Column.VisibleIndex Then
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
			If (Not View.AutoWidth) Then
				columnsWidth += LayoutHelper.FindElement(View, AddressOf IsFitContent).ActualWidth
			End If
			For Each gc As GridColumn In View.VisibleColumns
				columnsWidth += gc.ActualHeaderWidth
				If cond(columnsWidth) Then
					res = gc
					Exit For
				End If
			Next gc
			Return res
		End Function
		Private Function IsFitContent(ByVal e As FrameworkElement) As Boolean
			Return e.Name = "PART_FitContent"
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

