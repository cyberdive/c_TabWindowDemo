using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TabWindow
{
    public interface ITabWindow
    {
        void AddTabItem(string tabHeader, Control content);
        ItemCollection TabItems { get; }
        void RemoveTabItem(TabItem tabItem);
        string TabHeaderSelected { get; set; }
    }
}
