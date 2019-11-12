using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TabWindow
{
    public class DragWindowManager
    {
        #region singleton

        private static readonly DragWindowManager _instance = new DragWindowManager();
        public static DragWindowManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private DragWindowManager() 
        {
            _allWindows = new List<IDragDropToTabWindow>();
            _dragEnteredWindows = new List<IDragDropToTabWindow>();
        }

        #endregion

        private List<IDragDropToTabWindow> _allWindows;
        private List<IDragDropToTabWindow> _dragEnteredWindows;

        public void Register(IDragDropToTabWindow win)
        {
            if (!_allWindows.Contains(win))
            {
                _allWindows.Add(win);
                ((Window)win).Closed +=new EventHandler(DragWindowManager_Closed);
            }
        }

        public void Unregister(IDragDropToTabWindow win)
        {
            if (_allWindows.Contains(win))
                _allWindows.Remove(win);
        }

        void  DragWindowManager_Closed(object sender, EventArgs e)
        {
 	        Window win = sender as Window;
            if(win != null)
                Unregister((IDragDropToTabWindow)win);
        }

        public void DragMove(IDragDropToTabWindow dragWin)
        {
            if (dragWin == null) return;

            Win32Helper.Win32Point p = new Win32Helper.Win32Point();
            if (!Win32Helper.GetCursorPos(ref p)) return;

            Point dragWinPosition = new Point(p.X, p.Y);
            foreach (IDragDropToTabWindow existWin in _allWindows)
            {
                if (dragWin.Equals(existWin)) continue;

                if (existWin.IsDragMouseOver(dragWinPosition))
                {
                    if (!_dragEnteredWindows.Contains(existWin))
                        _dragEnteredWindows.Add(existWin);
                }
                else
                {
                    if (_dragEnteredWindows.Contains(existWin))
                    {
                        _dragEnteredWindows.Remove(existWin);
                        existWin.OnDrageLeave();
                    }
                }
            }
            if (_dragEnteredWindows.Count > 0)
            {
                IntPtr dragWinHwnd = new System.Windows.Interop.WindowInteropHelper((Window)dragWin).Handle;
                IntPtr dragBelowhwnd = Win32Helper.GetWindow(dragWinHwnd, Win32Helper.GW_HWNDNEXT);
                IDragDropToTabWindow nextTopWin = null;
                bool foundTabTarget = false;
                for (IntPtr hWind = dragBelowhwnd; hWind != IntPtr.Zero; hWind = Win32Helper.GetWindow(hWind, Win32Helper.GW_HWNDNEXT))
                {
                    foreach (Window enteredWin in _dragEnteredWindows)
                    {
                        IntPtr enterWinHwnd = new System.Windows.Interop.WindowInteropHelper(enteredWin).Handle;
                        if (hWind == enterWinHwnd)
                        {
                            nextTopWin = (IDragDropToTabWindow)enteredWin;
                            ((IDragDropToTabWindow)enteredWin).OnDragEnter();
                            foundTabTarget = true;
                            break;
                        }

                    }
                    if (foundTabTarget) break;
                }
                if (nextTopWin != null)
                {
                    foreach (Window enteredWin in _dragEnteredWindows)
                    {
                        if (!nextTopWin.Equals(enteredWin))
                            ((IDragDropToTabWindow)enteredWin).OnDrageLeave();
                    }

                    if (nextTopWin.IsDragMouseOverTabZone(dragWinPosition))
                        ((Window)dragWin).Hide();
                    else
                        ((Window)dragWin).Show();
                }
            }
            else
            {
                if (!((Window)dragWin).IsVisible)
                    ((Window)dragWin).Show();
            }
        }

        public void DragEnd(IDragDropToTabWindow dragWin)
        {
            if (dragWin == null) return;

            Win32Helper.Win32Point p = new Win32Helper.Win32Point();
            if (!Win32Helper.GetCursorPos(ref p)) return;

            Point dragWinPosition = new Point(p.X, p.Y);
            foreach (IDragDropToTabWindow targetWin in _dragEnteredWindows)
            {
                if (targetWin.IsDragMouseOverTabZone(dragWinPosition))
                {
                    System.Windows.Controls.ItemCollection items = ((ITabWindow)dragWin).TabItems;
                    for (int i = 0; i < items.Count; i++)
                    {
                        System.Windows.Controls.TabItem item = items[i] as System.Windows.Controls.TabItem;
                        if (item != null)
                            ((ITabWindow)targetWin).AddTabItem(item.Header.ToString(), (System.Windows.Controls.Control)item.Content);
                    }
                    for (int i = items.Count; i > 0; i--)
                    {
                        System.Windows.Controls.TabItem item = items[i - 1] as System.Windows.Controls.TabItem;
                        if (item != null)
                            ((ITabWindow)dragWin).RemoveTabItem(item);
                    }
                }
                targetWin.OnDrageLeave();
            }
            if (_dragEnteredWindows.Count > 0 && ((ITabWindow)dragWin).TabItems.Count == 0)
            {
                ((Window)dragWin).Close();
            }

            _dragEnteredWindows.Clear();
        }

    }
}
