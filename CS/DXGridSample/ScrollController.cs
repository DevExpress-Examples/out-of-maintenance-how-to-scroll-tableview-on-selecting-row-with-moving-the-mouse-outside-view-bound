using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpf.Grid;
using System.Windows.Threading;

namespace DXGrid_AssignComboBoxToColumn {
    public class ScrollController {
        public GridSelectingBehavior Owner;
        DispatcherTimer HorizontalScrollTimer;
        DispatcherTimer VerticalScrollTimer;
        bool isScrollUp = false;
        bool isScrollRight = false;
        public int VisibleRowCount { get { return (int)Owner.ScrollElement.ViewportHeight; } }
        public bool CanScrollUp { get { return Owner.View.TopRowIndex != 0; } }
        public bool CanScrollDown { get { return Owner.View.TopRowIndex + VisibleRowCount < ((GridControl)Owner.Grid).VisibleRowCount; } }
        public bool CanScrollLeft { get { return Owner.ScrollElement.HorizontalOffset > 0; } }
        public bool CanScrollRight { get { return Owner.ScrollElement.HorizontalOffset + Owner.ScrollElement.ViewportWidth < Owner.ScrollElement.ExtentWidth; } }
        public bool IsScrollWorking { get { return HorizontalScrollTimer.IsEnabled || VerticalScrollTimer.IsEnabled; } }
        public event EventHandler ScrollUp;
        public event EventHandler ScrollDown;
        public event EventHandler ScrollRight;
        public event EventHandler ScrollLeft;

        public ScrollController(GridSelectingBehavior owner) {
            Owner = owner;
            HorizontalScrollTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            VerticalScrollTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            HorizontalScrollTimer.Tick += OnHorizontalScrollTimerTick;
            VerticalScrollTimer.Tick += OnVerticalScrollTimerTick;
        }
        void OnVerticalScrollTimerTick(object sender, EventArgs e) {
            if(isScrollUp) {
                Owner.ScrollElement.LineUp();
                if(ScrollUp != null) ScrollUp(this, EventArgs.Empty);
            } else {
                Owner.ScrollElement.LineDown();
                if(ScrollDown != null) ScrollDown(this, EventArgs.Empty);
            }
        }
        void OnHorizontalScrollTimerTick(object sender, EventArgs e) {
            if(isScrollRight) {
                Owner.ScrollElement.LineRight();
                if(ScrollRight != null) ScrollRight(this, EventArgs.Empty);
            } else {
                Owner.ScrollElement.LineLeft();
                if(ScrollLeft != null) ScrollLeft(this, EventArgs.Empty);
            }
        }

        public void StartScrollUp() {
            if(isScrollUp && VerticalScrollTimer.IsEnabled) return;
            StopVerticalScrolling();
            isScrollUp = true;
            VerticalScrollTimer.Start();
        }
        public void StartScrollDown() {
            if(!isScrollUp && VerticalScrollTimer.IsEnabled) return;
            StopVerticalScrolling();
            isScrollUp = false;
            VerticalScrollTimer.Start();
        }

        public void StartScrollLeft() {
            if(!isScrollRight && HorizontalScrollTimer.IsEnabled) return;
            StopHorizontalScrolling();
            isScrollRight = false;
            HorizontalScrollTimer.Start();
        }
        public void StartScrollRight() {
            if(isScrollRight && HorizontalScrollTimer.IsEnabled) return;
            StopHorizontalScrolling();
            isScrollRight = true;
            HorizontalScrollTimer.Start();
        }

        public void StopVerticalScrolling() {
            VerticalScrollTimer.Stop();
        }
        public void StopHorizontalScrolling() {
            HorizontalScrollTimer.Stop();
        }
        public void UpdateVerticalScrollTimerInterval(double actualHeight, double mousePositionY) {
            TimeSpan res = UpdateScrollTimerInterval(actualHeight, mousePositionY);
            if(res != TimeSpan.Zero)
                VerticalScrollTimer.Interval = res;
        }
        public void UpdateHorizontalScrollTimerInterval(double actualWidth, double mousePositionX) {
            TimeSpan res = UpdateScrollTimerInterval(actualWidth, mousePositionX);
            if(res != TimeSpan.Zero)
                HorizontalScrollTimer.Interval = res;
        }
        protected virtual TimeSpan UpdateScrollTimerInterval(double size, double mousePos) {
            double multiplier = 0;
            if(mousePos > size)
                multiplier = Math.Abs((mousePos - size) / size);
            else if(mousePos < 0)
                multiplier = Math.Abs(mousePos / size);
            if(multiplier < 1) {
                double milliseconds = (1 - multiplier) * 100;
                if(milliseconds > 20 && milliseconds < 100)
                    return TimeSpan.FromMilliseconds(milliseconds);
            }
            return TimeSpan.Zero;
        }
    }
}
