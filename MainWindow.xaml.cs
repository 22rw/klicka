using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.Win32;
using Windows.Win32.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Klicka
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        const int startHotkeyID = 1, stopHotkeyID = 2;

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

            ContentFrame.SourcePageType = typeof(ConfigPage);

            this.AppWindow.ResizeClient(new() { Width = 550, Height = 425 });

            var hook = new WindowMessageHook(this);
            this.Closed += (s, e) => hook.Dispose(); // unhook on close
            hook.Message += (s, e) =>
            {
                const int WM_HOTKEY = 0x312;
                if (e.Message == WM_HOTKEY)
                {
                    switch (e.WParam)
                    {
                        case startHotkeyID:
                            StartScheduler();
                            break;
                        case stopHotkeyID:
                            StopScheduler();
                            break;
                    }
                }
            };

            var hwnd = Win32Interop.GetWindowFromWindowId(this.AppWindow.Id);

            PInvoke.RegisterHotKey(
                (HWND)hwnd,
                startHotkeyID,
                Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS.MOD_ALT,
                (uint)VirtualKey.F7
            );

            PInvoke.RegisterHotKey(
                (HWND)hwnd,
                stopHotkeyID,
                Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS.MOD_ALT,
                (uint)VirtualKey.F8
            );

            this.Closed += (s, e) =>
            {
                PInvoke.UnregisterHotKey((HWND)hwnd, startHotkeyID);
                PInvoke.UnregisterHotKey((HWND)hwnd, stopHotkeyID);
            };

            this.Activated += (s, e) => 
            {
                if(e.WindowActivationState == WindowActivationState.Deactivated)
                {
                    toggleIcon.Foreground = App.Current.Resources["TextFillColorDisabledBrush"] as SolidColorBrush;
                } else
                {
                    toggleIcon.Foreground = App.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
                }
            };
        }

        private void StartScheduler()
        {
            ConfigPage page = ContentFrame.Content as ConfigPage;
            var @params = new Parameters()
            {
                button = page!.SelectedButton,
                isStaticTarget = page.UseStaticLocation,
                target = page.TargetLocation,
                sleepMillis = page.IntervalMillis,
                isRandomInterval = page.UseRandomInterval,
                randomRange = page.IntervalRange,
            };
            var random = 8;
            titleBarTextBlock.Text = "Klicka - Running";
            Scheduler.Start(@params);
        }

        private void StopScheduler()
        {
            titleBarTextBlock.Text = "Klicka";
            Scheduler.Stop();
        }

        private void compactOverlayToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if(this.AppWindow.Presenter.GetType() != typeof(CompactOverlayPresenter))
            {
                var presenter = CompactOverlayPresenter.Create();
                presenter.InitialSize = CompactOverlaySize.Large;

                this.AppWindow.SetPresenter(presenter);
                toggleIcon.Glyph = "\uEE47";
            }
            else
            {
                this.AppWindow.SetPresenter(AppWindowPresenterKind.Default);
                toggleIcon.Glyph = "\uEE49";
            }
            SetRegionsForCustomTitleBar();
        }

        private void SetRegionsForCustomTitleBar()
        {
            // Specify the interactive regions of the title bar.

            double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

            RightPaddingColumn.Width = new GridLength(this.AppWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(this.AppWindow.TitleBar.LeftInset / scaleAdjustment);

            GeneralTransform transform = compactOverlayToggleBtn.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                             compactOverlayToggleBtn.ActualWidth,
                                                             compactOverlayToggleBtn.ActualHeight));
            Windows.Graphics.RectInt32 overlayToggleButtonRect = GetRect(bounds, scaleAdjustment);

            var rectArray = new Windows.Graphics.RectInt32[] { overlayToggleButtonRect };

            InputNonClientPointerSource nonClientInputSrc =
                InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            SetRegionsForCustomTitleBar();
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetRegionsForCustomTitleBar();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopScheduler();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            StartScheduler();
        }

        private Windows.Graphics.RectInt32 GetRect(Rect bounds, double scale)
        {
            return new Windows.Graphics.RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }
    }
}
