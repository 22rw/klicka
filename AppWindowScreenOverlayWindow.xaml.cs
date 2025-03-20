using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Klicka
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppWindowScreenOverlayWindow : Window
    {
        SUBCLASSPROC wndProcHandler;
        SemaphoreSlim semaphoreSlim;

        public event EventHandler<System.Drawing.Point>? Clicked;

        public AppWindowScreenOverlayWindow()
        {
            this.InitializeComponent();

            semaphoreSlim = new(1);

            this.ExtendsContentIntoTitleBar = true;

            var presenter = (OverlappedPresenter)this.AppWindow.Presenter;
            //presenter.IsAlwaysOnTop = true;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);

            var handle = Win32Interop.GetWindowFromWindowId(this.AppWindow.Id);

            var rgn = PInvoke.CreateRectRgn(-2, -2, -1, -1);
            PInvoke.DwmEnableBlurBehindWindow(new HWND(handle), new DWM_BLURBEHIND()
            {
                dwFlags = 0x00000001 | 0x00000002,
                fEnable = true,
                hRgnBlur = rgn,
            });
            TransparentHelper.SetTransparent(this, true);

            wndProcHandler = new SUBCLASSPROC(WndProc);
            PInvoke.SetWindowSubclass(new HWND(handle), wndProcHandler, 1, 0);

            presenter.Maximize();
            PInvoke.DeleteObject(rgn);

            isLoaded = true;
            new Thread(CursorPosWatcher).Start();

            new Thread(() => 
            {
                Thread.Sleep(100);
                isActivated = true;
            }).Start();
        }

        public IntPtr GetHandle()
        {
            return new HWND(Win32Interop.GetWindowFromWindowId(this.AppWindow.Id));
        }

        private unsafe LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
        {
            if (uMsg == PInvoke.WM_PAINT)
            {
                var hdc = PInvoke.BeginPaint(hWnd, out var ps);
                if (hdc.IsNull) return new LRESULT(0);

                var brush = PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH);
                PInvoke.FillRect(hdc, ps.rcPaint, new Handle((nint)brush));
                return new LRESULT(1);
            }

            return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }

        private bool isLoaded = false;

        private void CursorPosWatcher()
        {
            while (isLoaded)
            {
                var pos = new System.Drawing.Point();
                Windows.Win32.PInvoke.GetCursorPos(out pos);
                DispatcherQueue?.TryEnqueue(() => { cursorPosTextX.Text = $"{pos.X}"; cursorPostTextY.Text = $"{pos.Y}"; });
                DispatcherQueue?.TryEnqueue(() => 
                { 
                    Canvas.SetLeft(cursorPosLabel, pos.X + 4); 
                    Canvas.SetTop(cursorPosLabel, pos.Y + 8); 
                });
                if ((PInvoke.GetKeyState((int)VirtualKey.LeftButton) & 0x01) != 0)
                {
                    if(isActivated)
                    {
                        System.Drawing.Point p = new();
                        PInvoke.GetCursorPos(out p);
                        new Thread(WrappedInvoke).Start(p);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void WrappedInvoke(object? parameter)
        {
            if (semaphoreSlim.CurrentCount == 0) return;
            semaphoreSlim.Wait();
            Clicked?.Invoke(this, (System.Drawing.Point)parameter!);
            semaphoreSlim.Release();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            isLoaded = false;
        }

        private bool isActivated = false;
    }
}
