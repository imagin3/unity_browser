using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefWebRequestHandler : CefRequestHandler
    {
        private readonly CefWorker _mainWorker;

        public WorkerCefWebRequestHandler(CefWorker mainCefWorker)
        {
            _mainWorker = mainCefWorker;
        }

        protected override CefResourceRequestHandler GetResourceRequestHandler(CefBrowser browser, CefFrame frame, CefRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            /*//NOTE: In most cases you examine the request.Url and only handle requests you are interested in
            if (request.Url.ToLower().StartsWith("https://cefsharp.example")
                || request.Url.ToLower().StartsWith("mailto:"))
            {
                // For example
                // https://github.com/cefsharp/CefSharp/blob/a12023a449df524e61b4782738c0c3071c0acb40/CefSharp.Example/Handlers/ExampleResourceRequestHandler.cs#L21
                return new ExampleResourceRequestHandler();
            }*/
            return null;
        }

        protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            _mainWorker.BrowserMessageRouter.OnBeforeBrowse(browser, frame);
            return base.OnBeforeBrowse(browser, frame, request, userGesture, isRedirect);
        }

        protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
        {
            _mainWorker.BrowserMessageRouter.OnRenderProcessTerminated(browser);
        }
    }
}