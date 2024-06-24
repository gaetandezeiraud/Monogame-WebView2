using System;

namespace MonogameWebView2
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WindowsWebView webView = new WindowsWebView();

            using var game = new Game1(webView);
            webView.Initialize(game.Window);

            game.Run();
        }
    }
}