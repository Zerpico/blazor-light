//using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace blazor_light_app
{
    class Program
    {
    	static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        static CancellationToken token;

        static void Main(string[] args)
        {
        	//run blazor
            token = cancelTokenSource.Token;
            RunBlazor(args);

            //run view
            WebView view = new WebView(1000,600, "BlazorSample", "app.html");            
            view.UpdateView += View_UpdateView;
            view.Init();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Debug.WriteLine("WebView Start!");

            
        }

        private static async void RunBlazor(string[] args)
        {
            await BlazorServer.Program.CreateHostBuilder(args).Build().RunAsync(token);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            cancelTokenSource.Cancel();
        }

        private static void View_UpdateView(IntPtr user_data)
        {

        }
    }
}
