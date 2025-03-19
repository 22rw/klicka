using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Klicka
{
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2,
    }

    class InputSynthesizer
    {
        public static int Click(MouseButton btn, Point pos, bool move = true)
        {
            INPUT inputDataDown = new INPUT();
            inputDataDown.type = INPUT_TYPE.INPUT_MOUSE;
            inputDataDown.Anonymous.mi = CreateInputData(btn, pos, false, move);

            INPUT inputDataUp = new INPUT();
            inputDataUp.type = INPUT_TYPE.INPUT_MOUSE;
            inputDataUp.Anonymous.mi = CreateInputData(btn, pos, true, move);

            return (int)PInvoke.SendInput(new ReadOnlySpan<INPUT>([inputDataDown, inputDataUp]), Marshal.SizeOf(typeof(INPUT)));
        }

        private static MOUSEINPUT CreateInputData(MouseButton btn, Point pos, bool release, bool move = true)
        {
            MOUSE_EVENT_FLAGS inputFlagsPress = 0;
            switch (btn)
            {
                case MouseButton.Left: inputFlagsPress = MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN; break;
                case MouseButton.Right: inputFlagsPress = MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN; break;
                case MouseButton.Middle: inputFlagsPress = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN; break;
            }

            MOUSE_EVENT_FLAGS inputFlagsRelease = 0;
            switch (btn)
            {
                case MouseButton.Left: inputFlagsRelease = MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP; break;
                case MouseButton.Right: inputFlagsRelease = MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP; break;
                case MouseButton.Middle: inputFlagsRelease = MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP; break;
            }

            MOUSEINPUT mInput = new MOUSEINPUT()
            {
                dx = (pos.X * 65536) / PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
                dy = (pos.Y * 65536) / PInvoke.GetSystemMetrics(Windows.Win32.UI.WindowsAndMessaging.SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN),
                time = 0,
                dwFlags = MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE | (move ? MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE : 0) | MOUSE_EVENT_FLAGS.MOUSEEVENTF_VIRTUALDESK | (release ? inputFlagsRelease : inputFlagsPress),
                mouseData = 0,
                dwExtraInfo = 0,
            };

            return mInput;
        }

    }
}
