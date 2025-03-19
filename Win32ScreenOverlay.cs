using ABI.Windows.Foundation;
using Microsoft.UI.Xaml.Documents;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Klicka
{
    internal class Win32ScreenOverlay: IScreenOverlayProvider
    {
        public bool Open => _open;
        private bool _open = false;

        private HWND _hwnd;
        public HWND Handle => _hwnd;

        public event EventHandler<System.Drawing.Point>? OverlayClicked;

        private HWND _parent;

        private const string className = "Klicka.Win32ScreenOverlay";

        public void Configure(HWND parent)
        {
            _parent = parent;
        }

        private Rectangle GetVScreenArea()
        {
            System.Drawing.Rectangle windowRect = new()
            {
                X = PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
                Y = PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
                Width = PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
                Height = PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN)
            };
            return windowRect;
        }

        private HMODULE GetHInstance(HWND parent)
        {
            var hInst = PInvoke.GetWindowLong(parent, WINDOW_LONG_PTR_INDEX.GWL_HINSTANCE);
            return new HMODULE(hInst);
        }

        private unsafe char* StringToPointer(string str)
        {
            fixed (char* cn = str)
            {
                return cn;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private unsafe bool RegisterWndClass()
        {
            WNDCLASSEXW wcex = new();
            wcex.cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>();
            wcex.lpszClassName = StringToPointer(className);
            wcex.style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW;
            wcex.hInstance = GetHInstance(_parent);
            wcex.lpfnWndProc = WNDPROC;
            wcex.hCursor = (HCURSOR)LoadCursor(IntPtr.Zero, 32512);
            return PInvoke.RegisterClassEx(in wcex) != 0;
        }

        private unsafe void WinMain()
        {
            RegisterWndClass();
            var rect = GetVScreenArea();
            _hwnd = PInvoke.CreateWindowEx(
                    WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TOPMOST,
                    "Klicka.Win32ScreenOverlay",
                    "ScreenOverlay",
                    WINDOW_STYLE.WS_POPUPWINDOW,
                    rect.X,
                    rect.Y,
                    rect.Width,
                    rect.Height,
                    _parent,
                    null,
                    new Handle((int)GetHInstance(_parent)),
                    (void*)0
            );

            PInvoke.SetLayeredWindowAttributes(_hwnd, new COLORREF(0x0000FFFF), 20, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

            PInvoke.ShowWindow(_hwnd, SHOW_WINDOW_CMD.SW_MAXIMIZE);
            PInvoke.UpdateWindow(_hwnd);

            MSG msg;
            BOOL bRet;
            while ((bRet = PInvoke.GetMessage(out msg, new HWND(0), 0, 0)) != 0)
            {
                if (bRet == -1) break;

                PInvoke.TranslateMessage(in msg);
                PInvoke.DispatchMessage(in msg);
            }
            PInvoke.DestroyWindow(_hwnd);
            PInvoke.UnregisterClass(StringToPointer(className), GetHInstance(_parent));
        }

        private LRESULT WNDPROC(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam)
        {
            switch(Msg)
            {
                case PInvoke.WM_DESTROY:
                    {
                        PInvoke.PostQuitMessage(0);
                        break;
                    }
                case PInvoke.WM_LBUTTONDOWN:
                    {
                        System.Drawing.Point cursor = new();
                        PInvoke.GetCursorPos(out cursor);
                        OverlayClicked?.Invoke(this, cursor);
                        return new LRESULT(1);
                    }
            }
            return PInvoke.DefWindowProc(hWnd, Msg, wParam, lParam);
        }

        public void Show()
        {
            if(!_open)
            {
                _open = true;
                new Thread(WinMain).Start();
            }
        }

        public void Close()
        {
            _open = false;
            PInvoke.PostMessage(_hwnd, PInvoke.WM_QUIT, 0, 0);
        }
    }

    class Handle : SafeHandle
    {
        public override bool IsInvalid => handle >= 0;

        public Handle(IntPtr handle): base(handle, false)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }

        public static explicit operator Handle(IntPtr handle) => new Handle(handle);
        public static implicit operator IntPtr(Handle handle) => handle.handle; 
    }
}
