using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TabWindowDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TabWindow.TabWindow tw = new TabWindow.TabWindow();
            tw.Width = 500;
            tw.Height = 400;
            for (int i = 0; i < 5; i++)
            {
                string title = "Test " + i;
                Control tw1Ctrl = CreateTestControl(title);
                tw.AddTabItem(title, tw1Ctrl);
            }
            tw.Show();
        }

        private Control CreateTestControl(string content)
        {
            TextBox tx = new TextBox();
            tx.Text = content;
            return tx;
        }
    }
}
