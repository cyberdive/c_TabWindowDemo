using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace TabWindow
{
    public class DragSupportTabControl : TabControl
    {
        private Point _startPoint;
        Window _dragTornWin = null;
        readonly object _synLockTabWindow = new object();

        public DragSupportTabControl() : base()
        {
            this.AllowDrop = true;
            this.MouseMove += new MouseEventHandler(DragSupportTabControl_MouseMove);
            this.Drop += new System.Windows.DragEventHandler(DragSupportTabControl_Drop);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DragSupportTabControl_PreviewMouseLeftButtonDown);
        }

        void DragSupportTabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._startPoint = e.GetPosition(null);
        }

        void DragSupportTabControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point mpos = e.GetPosition(null);
            Vector diff = this._startPoint - mpos;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var tabItem = e.Source as CloseEnabledTabItem;
                if (tabItem == null) return;

                //Console.WriteLine("DragSupportTabControl_MouseMove - {0}  startX:{1} startY:{2} mposX:{3} mposY:{4}", tabItem.Header, this._startPoint.X, this._startPoint.Y, mpos.X, mpos.Y);

                this.QueryContinueDrag += DragSupportTabControl_QueryContinueDrag;
                this.GiveFeedback += new GiveFeedbackEventHandler(DragSupportTabControl_GiveFeedback);
                DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);

                this.GiveFeedback -= new GiveFeedbackEventHandler(DragSupportTabControl_GiveFeedback);
            }
        }

        void DragSupportTabControl_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_dragTornWin != null)
            {
                Mouse.SetCursor(Cursors.Arrow);
                e.Handled = true;
            }
        }

        void DragSupportTabControl_Drop(object sender, System.Windows.DragEventArgs e)
        {
            this.QueryContinueDrag -= DragSupportTabControl_QueryContinueDrag;

            var tabItemTarget = e.Source as TabItem;
            if (tabItemTarget == null) return;

            var tabItemSource = e.Data.GetData(typeof(CloseEnabledTabItem)) as TabItem;
            if (tabItemSource == null) return;

            SwapTabItems(tabItemSource, tabItemTarget);
        }

        void DragSupportTabControl_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
            {
                Console.WriteLine("DragSupportTabControl_QueryContinueDrag - action:{0}  {1}", e.Action.ToString(), DateTime.Now.ToLongTimeString());
                Win32Helper.Win32Point p = new Win32Helper.Win32Point();
                if (Win32Helper.GetCursorPos(ref p))
                {
                    Point _tabPos = this.PointToScreen(new Point(0, 0));
                    if (!((p.X >= _tabPos.X && p.X <= (_tabPos.X + this.ActualWidth) && p.Y >= _tabPos.Y && p.Y <= (_tabPos.Y + this.ActualHeight))))
                    {
                        var item = e.Source as TabItem;
                        if (item != null)
                            UpdateWindowLocation(p.X - 50, p.Y - 10, item);
                    }
                    else
                    {
                        if (this._dragTornWin != null)
                            UpdateWindowLocation(p.X - 50, p.Y - 10, null);
                    }
                }
            }
            else if (e.KeyStates == DragDropKeyStates.None)
            {
                this.QueryContinueDrag -= DragSupportTabControl_QueryContinueDrag;
                e.Handled = true;

                if (this._dragTornWin != null)
                {
                    _dragTornWin = null;
                    var item = e.Source as TabItem;
                    if (item != null)
                        this.RemoveTabItem(item);
                }
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (this.Items.Count == 1)
            {
                ((TabItem)this.Items[0]).Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (this.Items.Count == 2)
            {
                ((TabItem)this.Items[0]).Visibility = System.Windows.Visibility.Visible;
                ((TabItem)this.Items[1]).Visibility = System.Windows.Visibility.Visible;
            }
        }

        public bool SwapTabItems(TabItem source, TabItem target)
        {
            if (source == null || target == null) return false;

            if (!target.Equals(source))
            {
                var tabControl = target.Parent as TabControl;
                int sourceIdx = tabControl.Items.IndexOf(source);
                int targetIdx = tabControl.Items.IndexOf(target);

                tabControl.Items.Remove(source);
                tabControl.Items.Insert(targetIdx, source);

                tabControl.SelectedIndex = targetIdx;
                return true;
            }
            return false;
        }

        public void RemoveTabItem(TabItem item)
        {
            if (this.Items.Contains(item))
                this.Items.Remove(item);
        }

        private void UpdateWindowLocation(double left, double top, TabItem tabItem)
        {
            if (this._dragTornWin == null)
            {
                lock (_synLockTabWindow)
                {
                    if (_dragTornWin == null)
                        _dragTornWin = TabWindow.CreateTabWindow(left, top, this.ActualWidth, this.ActualHeight, tabItem);
                }
            }
            if (this._dragTornWin != null)
            {
                this._dragTornWin.Left = left;
                this._dragTornWin.Top = top;
            }
        }

    }
}
