using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Xna.Framework;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        private WebView2 webView2;

        public WindowsWebView()
        {
            Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "00FFFFFF");
        }

        public async void Initialize(GameWindow window)
        {
            _gameWindow = window;
            window.ClientSizeChanged += Window_ClientSizeChanged;

            _uiThreadSyncCtx = new UiThreadSynchronizationContext((HWND)window.Handle);
            SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);


            IntPtr winHandle = (HWND)window.Handle;
            Form winForm = Control.FromHandle(winHandle) as Form;

            webView2 = new WebView2();
            webView2.Dock = DockStyle.Fill;
            webView2.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            winForm.Controls.Add(webView2);
            //webView2.Source = new Uri("https://www.google.com");

            var environment = await CoreWebView2Environment.CreateAsync(null, null, null);
            await webView2.EnsureCoreWebView2Async(environment);

            webView2.NavigateToString("<h1 style='color: red'>Hello world!</h1>");
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
        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            //_controller.Bounds = new System.Drawing.Rectangle(0, 0, _gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height);
        }
    }
}
