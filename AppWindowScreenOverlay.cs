using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;

namespace Klicka
{
    class AppWindowScreenOverlay: IScreenOverlayProvider
    {
        public event EventHandler<Point>? OverlayClicked;

        private IntPtr windowHandle;

        private AppWindowScreenOverlayWindow? wnd;

        public void Configure(HWND hwnd) { }

        public void Show()
        {
            if (wnd != null) return;

            wnd = new AppWindowScreenOverlayWindow();
            windowHandle = wnd.GetHandle();
            wnd.Clicked += (s, p) =>
            {
                OverlayClicked?.Invoke(this, p);
            };
            wnd.Activate();
        }

        public void Close()
        {
            PInvoke.SendMessage(new HWND(windowHandle), 0x0112, 0xF060, 0);
        }
    }
}
