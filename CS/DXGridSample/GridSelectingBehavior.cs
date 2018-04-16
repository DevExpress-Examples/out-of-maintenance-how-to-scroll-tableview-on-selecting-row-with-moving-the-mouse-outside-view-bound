using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpf.Grid;
using System.Windows.Interactivity;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Core.Native;
using System.Windows.Controls.Primitives;

namespace DXGrid_AssignComboBoxToColumn {
    public class SelectionBehaviorSettings {
        public int DragArea = 4;
        public int StartScrollArea = 25;
        public int UnselectedRowCountWhileScrolling = 0;

        public bool IsDragging(Point pt1, Point pt2) {
            return Math.Abs(pt1.X - pt2.X) > DragArea || Math.Abs(pt1.Y - pt2.Y) > DragArea;
        }
        public bool IsMouseOverTopScrollingArea(Point pt) {
            return pt.Y < StartScrollArea;
        }
        public bool IsMouseOverBottomScrollingArea(Point pt, double maxHeight) {
            return pt.Y > maxHeight - StartScrollArea;
        }
        public bool IsMouseOverLeftScrollingArea(Point pt) {
            return pt.X < StartScrollArea;
        }
        public bool IsMouseOverRightScrollingArea(Point pt, double maxWidth) {
            return pt.X > maxWidth - StartScrollArea;
        }
    }
    public class SelectionInfo {
        public Point MousePoint { get; set; }
        public int RowHandle { get; set; }
        public GridColumn Column { get; set; }
        public virtual void Clear() {
            MousePoint = InvalidPoint;
            RowHandle = GridControl.InvalidRowHandle;
            Column = null;
        }
        public virtual bool IsEmptyInfo() {
            return MousePoint == InvalidPoint;
        }

        static readonly Point InvalidPoint = new Point(-10000, 10000);
    }
    public class StartSelectionInfo : SelectionInfo {
        public bool IsSelectionStarted { get; set; }
        public bool IsLeftMouseButtonPressed { get { return Mouse.LeftButton == MouseButtonState.Pressed; } }
        public override void Clear() {
            base.Clear();
            IsSelectionStarted = false;
        }
    }
    public class GridSelectingBehavior : Behavior<GridControl> {
        public SelectionBehaviorSettings Settings { get; set; }
        public GridControl Grid { get { return AssociatedObject; } }
        public TableView View { get { return (TableView)Grid.View; } }
        protected internal IScrollInfo ScrollElement { get; private set; }
        protected internal FrameworkElement DataArea { get; private set; }
        protected StartSelectionInfo StartSelectionInfo { get; private set; }
        protected SelectionInfo CurrentSelectionInfo { get; private set; }
        protected ScrollController ScrollController { get; set; }

        public GridSelectingBehavior() {
            Settings = new SelectionBehaviorSettings();
            StartSelectionInfo = new StartSelectionInfo();
            CurrentSelectionInfo = new SelectionInfo();
            ScrollController = new ScrollController(this);
            ScrollController.ScrollDown += OnScrollControllerScrollDown;
            ScrollController.ScrollUp += OnScrollControllerScrollUp;
            ScrollController.ScrollLeft += OnScrollControllerScrollLeft;
            ScrollController.ScrollRight += OnScrollControllerScrollRight;
        }

        #region  INITIALIZATION
        protected override void OnAttached() {
            base.OnAttached();
            Grid.Loaded += OnGridLoaded;
        }
        protected override void OnDetaching() {
            Grid.PreviewMouseMove -= OnGridPreviewMouseMove;
            Grid.PreviewMouseDown -= OnGridPreviewMouseDown;
            Grid.PreviewMouseUp -= OnGridPreviewMouseUp;
            Grid.Loaded -= OnGridLoaded;
            base.OnDetaching();
        }
        private void OnGridLoaded(object sender, RoutedEventArgs e) {
            View.LayoutUpdated += OnViewLayoutUpdated;
        }
        void OnViewLayoutUpdated(object sender, EventArgs e) {
            if (View.MultiSelectMode == TableViewSelectMode.None)
                throw new Exception("GridSelectingBehavior does not allow using the MultiSelectMode.None.");

            DataArea = LayoutHelper.FindElementByName(View, "PART_ScrollContentPresenter");
            if (DataArea == null) return;
            ScrollElement = (DataPresenter)LayoutHelper.FindElement(DataArea, (el) => el is DataPresenter);
            if (ScrollElement == null) return;

            View.LayoutUpdated -= OnViewLayoutUpdated;

            Grid.PreviewMouseMove += OnGridPreviewMouseMove;
            Grid.PreviewMouseDown += OnGridPreviewMouseDown;
            Grid.PreviewMouseUp += OnGridPreviewMouseUp;
        }
        #endregion

        void OnGridPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if (!StartSelectionInfo.IsSelectionStarted) return;
            Mouse.Capture(null);
            ScrollController.StopHorizontalScrolling();
            ScrollController.StopVerticalScrolling();
            StartSelectionInfo.Clear();
            CurrentSelectionInfo.Clear();
            e.Handled = true;
        }
        void OnGridPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            StartSelectionInfo.Clear();
            TableViewHitInfo hitInfo = View.CalcHitInfo(e.OriginalSource as DependencyObject);
            if (!hitInfo.InRow && !hitInfo.InRowCell) return;
            StartSelectionInfo.MousePoint = e.GetPosition(DataArea);
            StartSelectionInfo.RowHandle = hitInfo.RowHandle;
            StartSelectionInfo.Column = hitInfo.Column;
        }
        void OnGridPreviewMouseMove(object sender, MouseEventArgs e) {
            if (!StartSelectionInfo.IsLeftMouseButtonPressed || StartSelectionInfo.IsEmptyInfo()) return;
            if (!StartSelectionInfo.IsSelectionStarted) {
                if (!Settings.IsDragging(StartSelectionInfo.MousePoint, e.GetPosition(DataArea))) return;
                Mouse.Capture(DataArea, CaptureMode.SubTree);
                StartSelectionInfo.IsSelectionStarted = true;
            }

            UpdateCurrentSelectionInfo();

            UpdateVerticalScrolling(CurrentSelectionInfo.MousePoint);
            UpdateHorizontalScrolling(CurrentSelectionInfo.MousePoint);

            if (!ScrollController.IsScrollWorking) UpdateSelection();

            ScrollController.UpdateVerticalScrollTimerInterval(DataArea.ActualHeight, e.GetPosition(DataArea).Y);
            ScrollController.UpdateHorizontalScrollTimerInterval(DataArea.ActualWidth, e.GetPosition(DataArea).X);
            e.Handled = true;
        }

        //TODO: improve this method
        void UpdateCurrentSelectionInfo() {
            CurrentSelectionInfo.MousePoint = Mouse.GetPosition(DataArea);
            Point pt = CurrentSelectionInfo.MousePoint;
            //Inside DataArea
            if (pt.X > 0 && pt.Y > 0 && pt.X < DataArea.ActualWidth && pt.Y < DataArea.ActualHeight) {
                HitTestResult result = VisualTreeHelper.HitTest(DataArea, CurrentSelectionInfo.MousePoint);
                if (result != null) {
                    DependencyObject hittedObject = result.VisualHit;
                    TableViewHitInfo hitInfo = View.CalcHitInfo(hittedObject);
                    CurrentSelectionInfo.RowHandle = hitInfo.RowHandle;
                    CurrentSelectionInfo.Column = hitInfo.Column;
                }
                return;
            }
            //Outside DataArea
            CurrentSelectionInfo.RowHandle = GetRowhandle(CurrentSelectionInfo.MousePoint.Y);
            //Select to right
            GridColumn rightColumn, rightColumnWhenUnselect;
            GetRightVisibleColumn(out rightColumn, out rightColumnWhenUnselect);
            if (pt.X >= DataArea.ActualWidth && StartSelectionInfo.Column.VisibleIndex <= CurrentSelectionInfo.Column.VisibleIndex) {
                if (rightColumn.VisibleIndex > CurrentSelectionInfo.Column.VisibleIndex)
                    CurrentSelectionInfo.Column = rightColumn;
                return;
            }
            //Unselecting to right
            if (pt.X >= DataArea.ActualWidth && StartSelectionInfo.Column.VisibleIndex >= CurrentSelectionInfo.Column.VisibleIndex) {
                if (rightColumnWhenUnselect.VisibleIndex >= CurrentSelectionInfo.Column.VisibleIndex)
                    CurrentSelectionInfo.Column = rightColumnWhenUnselect;
                return;
            }
            //Select to left
            if (pt.X <= 0) {
                GridColumn col = GetLeftVisibleColumn();
                    CurrentSelectionInfo.Column = col;
                return;
            }
        }
        void GetRightVisibleColumn(out GridColumn rightColumn, out GridColumn rightColumnWhenUnselect) {
            rightColumn = null;
            rightColumnWhenUnselect = null;
            double maxWidth = ScrollElement.HorizontalOffset + ScrollElement.ViewportWidth;
            double columnsWidth = 0d;
            rightColumn = FindVisibleColumn((w) => w > maxWidth, ref columnsWidth);
            rightColumnWhenUnselect = FindVisibleColumn((w) => w >= maxWidth, ref columnsWidth);

            if (rightColumnWhenUnselect.VisibleIndex - 1 <= 0)
                rightColumnWhenUnselect = View.VisibleColumns.First();

            if (rightColumn == null) rightColumn = View.VisibleColumns.Last();

            columnsWidth = maxWidth - (columnsWidth - rightColumn.Width);
            if (columnsWidth <= rightColumn.Width / 2) {
                if (rightColumn.VisibleIndex - 1 > 0)
                    rightColumn = View.VisibleColumns[rightColumn.VisibleIndex - 1];
                else rightColumn = View.VisibleColumns.First();
            }
        }
        GridColumn GetLeftVisibleColumn() {
            if (ScrollElement.HorizontalOffset < View.VisibleColumns.First().Width / 2) return View.VisibleColumns.First();
            double maxWidth = ScrollElement.HorizontalOffset;
            double columnsWidth = 0d;
            GridColumn res = FindVisibleColumn((w) => w > maxWidth, ref columnsWidth);
            columnsWidth = columnsWidth - res.Width;
            if (maxWidth < columnsWidth + res.Width / 2)
                return res;
            else
                return View.VisibleColumns[res.VisibleIndex + 1];
        }
        GridColumn FindVisibleColumn(Func<double, bool> cond, ref double columnsWidth) {
            columnsWidth = 0d;
            GridColumn res = null;
            foreach (GridColumn gc in View.VisibleColumns) {
                columnsWidth += gc.Width;
                if (cond(columnsWidth)) {
                    res = gc;
                    break;
                }
            }
            return res;
        }


        int GetRowhandle(double mouseYPosition) {
            double avgRowHeight = DataArea.ActualHeight / ScrollElement.ViewportHeight;
            int currentRowIndex = 0;
            double summaryHeight = 0;
            while (summaryHeight < mouseYPosition) {
                summaryHeight += avgRowHeight;
                currentRowIndex++;
            }
            return Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + currentRowIndex);
        }

        void UpdateSelection() {
            View.BeginSelection();
            View.ClearSelection();
            if (View.MultiSelectMode == TableViewSelectMode.Row)
                View.SelectRange(StartSelectionInfo.RowHandle, CurrentSelectionInfo.RowHandle);
            else
                View.SelectCells(StartSelectionInfo.RowHandle, StartSelectionInfo.Column, CurrentSelectionInfo.RowHandle, CurrentSelectionInfo.Column);
            View.EndSelection();
        }

        void UpdateVerticalScrolling(Point pt) {
            if (Settings.IsMouseOverTopScrollingArea(pt) && ScrollController.CanScrollUp)
                ScrollController.StartScrollUp();
            else if (Settings.IsMouseOverBottomScrollingArea(pt, DataArea.ActualHeight) && ScrollController.CanScrollDown)
                ScrollController.StartScrollDown();
            else
                ScrollController.StopVerticalScrolling();
        }
        void UpdateHorizontalScrolling(Point pt) {
            if (Settings.IsMouseOverLeftScrollingArea(pt) && ScrollController.CanScrollLeft)
                ScrollController.StartScrollLeft();
            else if (Settings.IsMouseOverRightScrollingArea(pt, DataArea.ActualWidth) && ScrollController.CanScrollRight)
                ScrollController.StartScrollRight();
            else ScrollController.StopHorizontalScrolling();
        }
        
        void OnScrollControllerScrollUp(object sender, EventArgs e) {
            if (ScrollController.CanScrollUp)
                CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + Settings.UnselectedRowCountWhileScrolling);
            else CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex);
            UpdateSelection();
        }
        void OnScrollControllerScrollDown(object sender, EventArgs e) {
            if (ScrollController.CanScrollDown)
                CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(View.TopRowIndex + ScrollController.VisibleRowCount - Settings.UnselectedRowCountWhileScrolling);
            else CurrentSelectionInfo.RowHandle = Grid.GetRowHandleByVisibleIndex(Grid.VisibleRowCount - 1);
            UpdateSelection();
        }
        void OnScrollControllerScrollLeft(object sender, EventArgs e) {
            if (ScrollController.CanScrollLeft) {
                UpdateCurrentSelectionInfo();
                UpdateSelection();
            }
        }
        void OnScrollControllerScrollRight(object sender, EventArgs e) {
            if (ScrollController.CanScrollRight)
                UpdateCurrentSelectionInfo();
                UpdateSelection();
        }
    }

}


