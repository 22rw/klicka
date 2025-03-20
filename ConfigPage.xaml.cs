using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Klicka
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigPage : Page
    {
        public MouseButton SelectedButton => (MouseButton)btnComboBox.SelectedIndex;
        public bool UseStaticLocation => locationChoice.SelectedIndex == 1;
        public Point TargetLocation => new Point((int)targetX.Value, (int)targetY.Value);
        public bool UseRandomInterval => intervalChoice.SelectedIndex == 1;
        public int IntervalMillis => (int)intervalInputBox.Value;
        public (int, int) IntervalRange => ((int)randIntervalMinInputBox.Value, (int)randIntervalMaxInputBox.Value);
        public bool UseFixedDuration => durationChoice.SelectedIndex == 1;
        public int FixedClickAmount => (int)fixedClickCountInputBox.Value;

        public ConfigPage()
        {
            this.InitializeComponent();
        }

        private void getCursorLocBtn_Click(object sender, RoutedEventArgs e)
        {
            locationChoice.SelectedIndex = 1;

            var window = (Application.Current as App).Window;
            var hwnd = Win32Interop.GetWindowFromWindowId(window.AppWindow.Id);
            IScreenOverlayProvider wnd = new AppWindowScreenOverlay();

            wnd.Configure(new HWND(hwnd));
            wnd.OverlayClicked += (s, p) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    targetX.Value = p.X;
                    targetY.Value = p.Y;
                });
                wnd.Close();
            };
            wnd.Show();
        }
    }
}
