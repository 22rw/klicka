using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace Klicka
{
    interface IScreenOverlayProvider
    {
        public event EventHandler<System.Drawing.Point> OverlayClicked;
        public void Configure(HWND parent);
        public void Show();
        public void Close();
    }
}
