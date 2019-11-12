using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TabWindow
{
    /// <summary>
    /// Interaction logic for DropOverlayWindow.xaml
    /// </summary>
    public partial class DropOverlayWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public DropOverlayWindow()
        {
            InitializeComponent();
        }

        public bool IsMouseOverTabTarget(Point mousePos)
        {
            Point buttonPosToScreen = this.btnDropTarget.PointToScreen(new Point(0, 0));
            PresentationSource source = PresentationSource.FromVisual(this);
            Point targetPos = source.CompositionTarget.TransformFromDevice.Transform(buttonPosToScreen);

            bool isMouseOver = (mousePos.X > targetPos.X && mousePos.X < (targetPos.X + btnDropTarget.Width) && mousePos.Y > targetPos.Y && mousePos.Y < (targetPos.Y + btnDropTarget.Height));
            IsTabTargetOver = isMouseOver;

            return isMouseOver;
        }

        bool _isTabTargetOver = false;
        public bool IsTabTargetOver
        {
            get { return _isTabTargetOver; }
            set
            {
                if (_isTabTargetOver != value)
                {
                    _isTabTargetOver = value;
                    RaisePropertyChange("IsTabTargetOver");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChange(string name)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
