using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TabWindow
{
    public interface IDragDropToTabWindow
    {
        void OnDragEnter();
        void OnDrageLeave();
        bool IsDragMouseOver(Point mousePosition);
        bool IsDragMouseOverTabZone(Point mousePosition);
    }
}
