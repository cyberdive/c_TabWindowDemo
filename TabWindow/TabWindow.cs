using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Input;

namespace TabWindow
{
    public class TabWindow : Window, ITabWindow, IDragDropToTabWindow
    {
        #region Member Variables

        DragSupportTabControl _tabCtrl;
        DropOverlayWindow _overlayWindow = null;

        #endregion

        #region Constructor

        public TabWindow():base()
        {
            Grid gridContent = new Grid();
            this.Content = gridContent;

            _tabCtrl = new DragSupportTabControl();
            gridContent.Children.Add(_tabCtrl);
            _tabCtrl.Margin = new Thickness(0);
            _tabCtrl.SelectionChanged += new SelectionChangedEventHandler(_tabCtrl_SelectionChanged);

            DragWindowManager.Instance.Register(this);
            this.SourceInitialized += new EventHandler(TabWindow_SourceInitialized);
        }

        #endregion

        #region Events

        void TabWindow_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        void _tabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabCtrl = sender as TabControl;
            if (tabCtrl != null)
            {
                TabItem item = tabCtrl.SelectedItem as TabItem;
                if (item != null)
                {
                    this.Title = item.Header.ToString();
                }
            }
        }

        #endregion

        #region ITabWindow Members

        public void AddTabItem(string tabHeader, Control content)
        {
            CloseEnabledTabItem item = new CloseEnabledTabItem();
            item.Header = tabHeader;
            item.Content = content;
            _tabCtrl.Items.Add(item);
            item.TabHeaderDoubleClick += new RoutedEventHandler(tabItem_TabHeaderDoubleClick);
        }

        public ItemCollection TabItems
        {
            get
            {
                return _tabCtrl.Items;
            }
        }

        public void RemoveTabItem(TabItem tabItem)
        {
            if (_tabCtrl.Items.Contains(tabItem))
            {
                ((CloseEnabledTabItem)tabItem).TabHeaderDoubleClick -= new RoutedEventHandler(tabItem_TabHeaderDoubleClick);
                _tabCtrl.Items.Remove(tabItem);
            }
        }

        public string TabHeaderSelected
        {
            get
            {
                var tab = _tabCtrl.SelectedItem as CloseEnabledTabItem;
                if (tab != null)
                    return tab.Header.ToString();
                return string.Empty;
            }
            set
            {
                SelectTabItem(value);
            }
        }

        void tabItem_TabHeaderDoubleClick(object sender, RoutedEventArgs e)
        {
            CloseEnabledTabItem tabItem = e.Source as CloseEnabledTabItem;
            if (tabItem != null)
            {
                Point mousePos = this.PointToScreen(Mouse.GetPosition(tabItem));
                TabWindow tabWin = TabWindow.CreateTabWindow(mousePos.X, mousePos.Y, this.ActualWidth, this.ActualHeight, tabItem);
                _tabCtrl.RemoveTabItem(tabItem);
                tabWin.Activate();
                tabWin.Focus();
            }
        }

        #endregion

        #region IDropToTabWindow Members

        public void OnDragEnter()
        {
            if (_overlayWindow == null)
                _overlayWindow = new DropOverlayWindow();
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                _overlayWindow.Left = 0;
                _overlayWindow.Top = 0;
            }
            else
            {
                _overlayWindow.Left = this.Left;
                _overlayWindow.Top = this.Top;
            }
            _overlayWindow.Width = this.ActualWidth;
            _overlayWindow.Height = this.ActualHeight;
            _overlayWindow.Topmost = true;
            _overlayWindow.Show();
        }

        public void OnDrageLeave()
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Close();
                _overlayWindow = null;
            }
        }

        public bool IsDragMouseOver(Point mousePosition)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                return false;

            double left, top;
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                left = 0;
                top = 0;
            }
            else
            {
                left = this.Left;
                top = this.Top;
            }
            bool isMouseOver = (mousePosition.X > left && mousePosition.X < (left + this.ActualWidth) && mousePosition.Y > top && mousePosition.Y < (top + this.ActualHeight));
            //bool isMouseOver = (mousePosition.X > this.Left && mousePosition.X < (this.Left + this.Width) && mousePosition.Y > this.Top && mousePosition.Y < (this.Top + this.Height));
            return isMouseOver;
        }

        public bool IsDragMouseOverTabZone(Point mousePosition)
        {
            if (_overlayWindow == null) return false;

            return _overlayWindow.IsMouseOverTabTarget(mousePosition);
        }

        #endregion

        bool _hasFocus = false;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_ENTERSIZEMOVE)
                _hasFocus = true;
            else if (msg == Win32Helper.WM_EXITSIZEMOVE)
            {
                _hasFocus = false;
                DragWindowManager.Instance.DragEnd(this);
            }
            else if (msg == Win32Helper.WM_MOVE)
            {
                if (_hasFocus)
                    DragWindowManager.Instance.DragMove(this);
            }

            handled = false;
            return IntPtr.Zero;
        }

        public static TabWindow CreateTabWindow(double left, double top, double width, double height, TabItem tabItem)
        {
            TabWindow tabWin = new TabWindow();
            tabWin.Width = width;
            tabWin.Height = height;
            tabWin.Left = left;
            tabWin.Top = top;
            tabWin.WindowStartupLocation = WindowStartupLocation.Manual;
            Control tabContent = tabItem.Content as Control;
            if (tabContent == null)
            {
                tabContent = new ContentControl();
                ((ContentControl)tabContent).Content = tabItem.Content;
            }
            ((ITabWindow)tabWin).AddTabItem(tabItem.Header.ToString(), tabContent);
            tabWin.Show();
            tabWin.Activate();
            tabWin.Focus();
            return tabWin;
        }

        private void SelectTabItem(string tabHeader)
        {
            CloseEnabledTabItem selectedTab = null;
            foreach(CloseEnabledTabItem item in _tabCtrl.Items)
                if (item.Header.ToString() == tabHeader)
                {
                    selectedTab = item;
                    break;
                }
            if (selectedTab != null)
                _tabCtrl.SelectedItem = selectedTab;
        }
    }
}
