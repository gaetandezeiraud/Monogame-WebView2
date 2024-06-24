using Microsoft.Web.WebView2.Core;
using Microsoft.Xna.Framework;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MonogameWebView2
{
    public class WindowsWebView
    {
        internal const uint WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE = PInvoke.WM_USER + 1;

        private static CoreWebView2Controller _controller;
        private static UiThreadSynchronizationContext _uiThreadSyncCtx;
        private GameWindow _gameWindow;

        public WindowsWebView()
        {
            Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "00FFFFFF");
        }

        public void Initialize(GameWindow window)
        {
            _gameWindow = window;
            window.ClientSizeChanged += Window_ClientSizeChanged;

            _uiThreadSyncCtx = new UiThreadSynchronizationContext((HWND)window.Handle);
            SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);

            _ = CreateCoreWebView2Async(this, (HWND)window.Handle);
        }

        public void Update(Game g)
        {
            _uiThreadSyncCtx.RunAvailableWorkOnCurrentThread();
        }

        public void CustomEvent(string name)
        {
            string jsCode = "(function() { window.dispatchEvent(new CustomEvent('" + name + "')); })();";
            _controller.CoreWebView2.ExecuteScriptAsync(jsCode);
        }

        public void CustomEvent(string name, object detail)
        {
            string jsCode = "(function() { window.dispatchEvent(new CustomEvent('" + name + "', { detail: " + JsonSerializer.Serialize(detail) + "})); })();";
            _controller.CoreWebView2.ExecuteScriptAsync(jsCode);
        }

        // Private methods
        private static async Task CreateCoreWebView2Async(WindowsWebView view, HWND hwnd)
        {
            try
            {
                Console.WriteLine("Initializing WebView2...");
                var environment = await CoreWebView2Environment.CreateAsync(null, null, null);
                _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd);

                _controller.DefaultBackgroundColor = System.Drawing.Color.Transparent; // avoids flash of white when page first renders
                PInvoke.GetClientRect(hwnd, out var hwndRect);
                _controller.Bounds = new System.Drawing.Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
                _controller.AllowExternalDrop = false;

                // Load HTML
                _controller.CoreWebView2.NavigateToString("<h1 style='color: red'>Hello world!</h1>");


                _controller.IsVisible = true;

                Console.WriteLine("WebView2 initialization succeeded.");
            }
            catch (WebView2RuntimeNotFoundException)
            {
                var result = PInvoke.MessageBox(hwnd, "WebView2 runtime not installed.", "Error", MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR);

                if (result == MESSAGEBOX_RESULT.IDYES)
                {
                    // TODO: download WV2 bootstrapper from https://go.microsoft.com/fwlink/p/?LinkId=2124703 and run it
                }

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                PInvoke.MessageBox(hwnd, $"Failed to initialize WebView2:{Environment.NewLine}{ex}", "Error", MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR);
                Environment.Exit(1);
            }
        }

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            _controller.Bounds = new System.Drawing.Rectangle(0, 0, _gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height);
        }
    }
}
