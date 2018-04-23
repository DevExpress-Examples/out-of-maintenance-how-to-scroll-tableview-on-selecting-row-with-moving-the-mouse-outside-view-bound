Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports DevExpress.Xpf.Grid
Imports System.Windows.Threading

Namespace DXGrid_AssignComboBoxToColumn
	Public Class ScrollController
		Public Owner As GridSelectingBehavior
		Private HorizontalScrollTimer As DispatcherTimer
		Private VerticalScrollTimer As DispatcherTimer
		Private isScrollUp As Boolean = False
		Private isScrollRight As Boolean = False
		Public ReadOnly Property VisibleRowCount() As Integer
			Get
				Return CInt(Fix(Owner.ScrollElement.ViewportHeight))
			End Get
		End Property
		Public ReadOnly Property CanScrollUp() As Boolean
			Get
				Return Owner.View.TopRowIndex <> 0
			End Get
		End Property
		Public ReadOnly Property CanScrollDown() As Boolean
			Get
				Return Owner.View.TopRowIndex + VisibleRowCount < (CType(Owner.Grid, GridControl)).VisibleRowCount
			End Get
		End Property
		Public ReadOnly Property CanScrollLeft() As Boolean
			Get
				Return Owner.ScrollElement.HorizontalOffset > 0
			End Get
		End Property
		Public ReadOnly Property CanScrollRight() As Boolean
			Get
				Return Owner.ScrollElement.HorizontalOffset + Owner.ScrollElement.ViewportWidth < Owner.ScrollElement.ExtentWidth
			End Get
		End Property
		Public ReadOnly Property IsScrollWorking() As Boolean
			Get
				Return HorizontalScrollTimer.IsEnabled OrElse VerticalScrollTimer.IsEnabled
			End Get
		End Property
		Public Event ScrollUp As EventHandler
		Public Event ScrollDown As EventHandler
		Public Event ScrollRight As EventHandler
		Public Event ScrollLeft As EventHandler

		Public Sub New(ByVal owner As GridSelectingBehavior)
			Me.Owner = owner
			HorizontalScrollTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(100)}
			VerticalScrollTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(100)}
			AddHandler HorizontalScrollTimer.Tick, AddressOf OnHorizontalScrollTimerTick
			AddHandler VerticalScrollTimer.Tick, AddressOf OnVerticalScrollTimerTick
		End Sub
		Private Sub OnVerticalScrollTimerTick(ByVal sender As Object, ByVal e As EventArgs)
			If isScrollUp Then
				Owner.ScrollElement.LineUp()
				RaiseEvent ScrollUp(Me, EventArgs.Empty)
			Else
				Owner.ScrollElement.LineDown()
				RaiseEvent ScrollDown(Me, EventArgs.Empty)
			End If
		End Sub
		Private Sub OnHorizontalScrollTimerTick(ByVal sender As Object, ByVal e As EventArgs)
			If isScrollRight Then
				Owner.ScrollElement.LineRight()
				RaiseEvent ScrollRight(Me, EventArgs.Empty)
			Else
				Owner.ScrollElement.LineLeft()
				RaiseEvent ScrollLeft(Me, EventArgs.Empty)
			End If
		End Sub

		Public Sub StartScrollUp()
			If isScrollUp AndAlso VerticalScrollTimer.IsEnabled Then
				Return
			End If
			StopVerticalScrolling()
			isScrollUp = True
			VerticalScrollTimer.Start()
		End Sub
		Public Sub StartScrollDown()
			If (Not isScrollUp) AndAlso VerticalScrollTimer.IsEnabled Then
				Return
			End If
			StopVerticalScrolling()
			isScrollUp = False
			VerticalScrollTimer.Start()
		End Sub

		Public Sub StartScrollLeft()
			If (Not isScrollRight) AndAlso HorizontalScrollTimer.IsEnabled Then
				Return
			End If
			StopHorizontalScrolling()
			isScrollRight = False
			HorizontalScrollTimer.Start()
		End Sub
		Public Sub StartScrollRight()
			If isScrollRight AndAlso HorizontalScrollTimer.IsEnabled Then
				Return
			End If
			StopHorizontalScrolling()
			isScrollRight = True
			HorizontalScrollTimer.Start()
		End Sub

		Public Sub StopVerticalScrolling()
			VerticalScrollTimer.Stop()
		End Sub
		Public Sub StopHorizontalScrolling()
			HorizontalScrollTimer.Stop()
		End Sub
		Public Sub UpdateVerticalScrollTimerInterval(ByVal actualHeight As Double, ByVal mousePositionY As Double)
			Dim res As TimeSpan = UpdateScrollTimerInterval(actualHeight, mousePositionY)
			If res <> TimeSpan.Zero Then
				VerticalScrollTimer.Interval = res
			End If
		End Sub
		Public Sub UpdateHorizontalScrollTimerInterval(ByVal actualWidth As Double, ByVal mousePositionX As Double)
			Dim res As TimeSpan = UpdateScrollTimerInterval(actualWidth, mousePositionX)
			If res <> TimeSpan.Zero Then
				HorizontalScrollTimer.Interval = res
			End If
		End Sub
		Protected Overridable Function UpdateScrollTimerInterval(ByVal size As Double, ByVal mousePos As Double) As TimeSpan
			Dim multiplier As Double = 0
			If mousePos > size Then
				multiplier = Math.Abs((mousePos - size) / size)
			ElseIf mousePos < 0 Then
				multiplier = Math.Abs(mousePos / size)
			End If
			If multiplier < 1 Then
				Dim milliseconds As Double = (1 - multiplier) * 100
				If milliseconds > 20 AndAlso milliseconds < 100 Then
					Return TimeSpan.FromMilliseconds(milliseconds)
				End If
			End If
			Return TimeSpan.Zero
		End Function
	End Class
End Namespace
