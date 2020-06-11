using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace blazor_light_app
{
    public class WebView
    {
        public delegate void UpdateViewHandler(IntPtr user_data);
        public event UpdateViewHandler UpdateView;
        public event Action CloseWindow;

        static uint kWindowFlags_Borderless = 1 << 0;
        static uint kWindowFlags_Titled = 1 << 1;
        static uint kWindowFlags_Resizable = 1 << 2;
        static uint kWindowFlags_Maximizable = 1 << 3;

        /// Various globals
        IntPtr app = IntPtr.Zero;
        IntPtr window = IntPtr.Zero;
        IntPtr overlay = IntPtr.Zero;
        IntPtr view = IntPtr.Zero;

        //parameter
        uint WindowWight;
        uint WindowHeight;
        string WindowTitle;
        string StartUrl;

        public WebView(uint windowWight, uint windowHeight, string title, string startUrl)
        {
            WindowWight = windowWight;
            WindowHeight = windowHeight;
            WindowTitle = title;
            StartUrl = startUrl;
        }
       
       
        public void Init()
        {
            
            /// Create default settings/config
            IntPtr settings = NativeAppCore.ulCreateSettings();
            IntPtr config = NativeAppCore.ulCreateConfig();

           
            /// Create our App
            app = NativeAppCore.ulCreateApp(settings, config);
            //NativeAppCore.ulConfigSetDeviceScale(config, 1.0);

            /// Register a callback to handle app update logic.
            NativeAppCore.ulAppSetUpdateCallback(app, OnUpdate, IntPtr.Zero);

            Debug.WriteLine("App Create");


            /// Done using settings/config, make sure to destroy anything we create
            NativeAppCore.ulDestroySettings(settings);
            NativeAppCore.ulDestroyConfig(config);

            /// Create our window, make it 500x500 with a titlebar and resize handles.
            window = NativeAppCore.ulCreateWindow(NativeAppCore.ulAppGetMainMonitor(app), WindowWight, WindowHeight, false, kWindowFlags_Resizable | kWindowFlags_Maximizable );
            NativeAppCore.ulWindowSetCloseCallback(window, OnCloseWindow, IntPtr.Zero);



            Debug.WriteLine("Window Create");
            

            /// Set our window title.            
            NativeAppCore.ulWindowSetTitle(window, WindowTitle);

            /// Register a callback to handle window resize.
            NativeAppCore.ulWindowSetResizeCallback(window, OnResize, IntPtr.Zero);

           

            IntPtr pathRoot = NativeAppCore.ulCreateString(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot"));
            NativeAppCore.ulSettingsSetFileSystemPath(settings, pathRoot);
            NativeAppCore.ulDestroyString(pathRoot);
            
            /// Tell our app to use this window as the main window.
            NativeAppCore.ulAppSetWindow(app, window);
            
            /// Create a overlay inside our window at 0,0 (top-left) origin.
            /// Overlays also create an HTML view for us to display content in.            
            overlay = NativeAppCore.ulCreateOverlay(window, WindowWight, WindowWight, 0, 0);

            Debug.WriteLine("Overlay Create");
            /// Get the overlay's view.
            view = NativeAppCore.ulOverlayGetView(overlay);

            Debug.WriteLine("View Create");
            
            /// Load a file from the FileSystem.
            ///  **IMPORTANT**: Make sure `file:///` has three (3) forward slashes.    
            Uri uri = new Uri(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "wwwroot"));
            string _baseUri = uri.AbsoluteUri;
            
            IntPtr url = NativeAppCore.ulCreateString($"{_baseUri}/{StartUrl}");
            NativeAppCore.ulViewLoadURL(view, url);
            NativeAppCore.ulDestroyString(url);

            NativeAppCore.ulOverlayResize(overlay, WindowWight, WindowHeight);

            Debug.WriteLine("App Run");
            // Run app loop
            NativeAppCore.ulAppRun(app);
            
        }

        private void OnCloseWindow(IntPtr user_data)
        {
            CloseWindow?.Invoke();
        }


        private void OnResize(IntPtr user_data, uint width, uint height)
        {            
            NativeAppCore.ulOverlayResize(overlay, width, height);
        }

        private void OnUpdate(IntPtr user_data)
        {
            UpdateView?.Invoke(user_data);
        }

       

        class NativeAppCore
        {

#if _WINDOWS_
            const string appcore = "AppCore.dll";
            const string ultralight = "Ultralight.dll";
            const string webcore = "WebCore.dll";

#elif _OSX_
            const string appcore = "libAppCore.dylib";
            const string ultralight = "libUltralight.dylib";
            const string webcore = "libWebCore.dylib";

#elif _LINUX_
            const string appcore = "libAppCore.so";
            const string ultralight = "libUltralight.so";
            const string webcore = "libWebCore.so";
#endif



            [DllImport(appcore/*, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl*/)]
            internal static extern IntPtr ulCreateSettings();

            [DllImport(appcore)]
            internal static extern void ulDestroySettings(IntPtr settings);


            [DllImport(ultralight/*, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl*/)]
            internal static extern IntPtr ulCreateConfig();

            [DllImport(ultralight)]
            internal static extern void ulDestroyConfig(IntPtr config);

            [DllImport(ultralight)]
            internal static extern void ulConfigSetDeviceScale(IntPtr config, double value);


            [DllImport(appcore)]
            internal static extern IntPtr ulCreateApp(IntPtr settings, IntPtr config);

            /// <summary>
            /// Get the main monitor 
            /// </summary>
            /// <param name="app"></param>
            /// <returns></returns>
            [DllImport(appcore)]
            internal static extern IntPtr ulAppGetMainMonitor(IntPtr app);


            /// <summary>
            /// Create a new Window.
            /// </summary>
            /// <param name="monitor">The monitor to create the Window on.</param>         
            /// <param name="width">The width (in device coordinates).</param>
            /// <param name="height">The height (in device coordinates).</param>
            /// <param name="fullscreen">Whether or not the window is fullscreen.</param>
            /// <param name="window_flags">Various window flags</param>
            /// <returns></returns>
            [DllImport(appcore)]
            internal static extern IntPtr ulCreateWindow(IntPtr monitor, uint width, uint height, bool fullscreen, uint window_flags);


            [DllImport(appcore)]
            internal static extern void ulWindowSetTitle(IntPtr window, string title);

            [DllImport(appcore)]
            internal static extern void ulAppSetWindow(IntPtr app, IntPtr window);

            [DllImport(appcore)]
            internal static extern double ulWindowGetScale(IntPtr window);

            [DllImport(appcore)]
            internal static extern void ulAppSetUpdateCallback(IntPtr app, ULUpdateCallback callback, IntPtr user_data);

            /// <summary>
            /// Run the main loop, make sure to call ulAppSetWindow before calling this.
            /// </summary>
            /// <param name="app"></param>
            [DllImport(appcore)]
            internal static extern void ulAppRun(IntPtr app);


            /// <summary>
            /// Create a new Overlay.
            /// </summary>
            /// <param name="window">The window to create the Overlay in</param>
            /// <param name="width">The width in device coordinates.</param>
            /// <param name="height">The height in device coordinates.</param>
            /// <param name="x">The x-position (offset from the left of the Window)</param>
            /// <param name="y">The y-position (offset from the top of the Window)</param>
            /// <returns></returns>
            [DllImport(appcore)]
            internal static extern IntPtr ulCreateOverlay(IntPtr window, uint width, uint height, int x, int y);

            [DllImport(appcore)]
            internal static extern void ulOverlayResize(IntPtr overlay, uint width, uint height);

            [DllImport(ultralight)]
            internal static extern IntPtr ulCreateRenderer(IntPtr config);

            [DllImport(ultralight)]
            internal static extern void ulUpdate(IntPtr renderer);

            [DllImport(ultralight)]
            internal static extern void ulRender(IntPtr renderer);

            /// <summary>
            /// Create a View with certain size (in device coordinates).
            /// </summary>      
            [DllImport(ultralight)]
            internal static extern IntPtr ulCreateView(IntPtr renderer, uint width, uint height, bool transparent);


            [DllImport(appcore)]
            internal static extern IntPtr ulOverlayGetView(IntPtr overlay);


            [DllImport(appcore)]
            internal static extern void ulSettingsSetFileSystemPath(IntPtr settings, IntPtr path);

            /// <summary>
            /// Load a URL into main frame.
            /// </summary>       
            [DllImport(ultralight)]
            internal static extern void ulViewLoadURL(IntPtr view, IntPtr url_string);

            [DllImport(ultralight)]
            internal static extern IntPtr ulViewLoadHTML(IntPtr view, string html_string);


            [DllImport(ultralight)]
            internal static extern IntPtr ulViewGetJSContext(IntPtr view);

            [DllImport(webcore)]
            internal static extern IntPtr JSStringCreateWithUTF8CString(string _string);

            [DllImport(webcore)]
            internal static extern IntPtr JSValueMakeString(IntPtr ctx, IntPtr _string);

            [DllImport(webcore)]
            internal static extern void JSStringRelease(IntPtr _string);

            [DllImport(webcore)]
            internal static extern void JSObjectSetProperty(IntPtr ctx, IntPtr _object, IntPtr propertyName, IntPtr value, IntPtr attributes, IntPtr exception);

            [DllImport(webcore)]
            internal static extern IntPtr JSContextGetGlobalObject(IntPtr ctx);

            [DllImport(appcore)]
            internal static extern void ulWindowSetResizeCallback(IntPtr window, ULResizeCallback callback, IntPtr user_data);

            [DllImport(ultralight)]
            internal static extern void ulViewSetChangeURLCallback(IntPtr view, ULChangeURLCallback callback, IntPtr user_data);

            [DllImport(ultralight)]
            internal static extern void ulViewSetDOMReadyCallback(IntPtr view, ULDOMReadyCallback callback, IntPtr user_data);

            [DllImport(webcore)]
            internal static extern IntPtr JSObjectMakeFunctionWithCallback(IntPtr ctx, IntPtr name, JSObjectCallAsFunctionCallback callAsFunction);

            [DllImport(appcore)]
            internal static extern void ulWindowSetCloseCallback(IntPtr window, ULCloseCallback callback, IntPtr user_data);



            [DllImport(ultralight)]
            internal static extern IntPtr ulCreateString(string str);

            [DllImport(ultralight)]
            internal static extern void ulDestroyString(IntPtr str);
            // (IntPtr user_data, IntPtr caller, string url)
        }
    }

    delegate void ULChangeURLCallback(IntPtr user_data, IntPtr caller, string url);
    delegate void ULUpdateCallback(IntPtr user_data);
    delegate void ULResizeCallback(IntPtr user_data, uint width, uint height);
    delegate void ULDOMReadyCallback (IntPtr user_data, IntPtr caller);
    delegate IntPtr JSObjectCallAsFunctionCallback(IntPtr ctx, IntPtr function, IntPtr thisObject, int argumentCount, IntPtr[] arguments, IntPtr exception);
    delegate void ULCloseCallback (IntPtr user_data);
}