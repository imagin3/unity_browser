using MessageLibrary;
using Xilium.CefGlue;

namespace SharedPluginServer
{
    class WorkerCefClient : CefClient
    {
#if DEBUG
        private static readonly log4net.ILog log =
  log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif

        //required:
        //lifespan+,
        //display? - status,console, etc
        //render+,
        //load+,
        //dialog +

        private readonly WorkerCefLoadHandler _loadHandler;
        private readonly WorkerCefRenderHandler _renderHandler;
        private readonly WorkerCefLifespanHandler _lifespanHandler;
        private readonly WorkerCefWebRequestHandler _requestHandler;
        private readonly WorkerCefJSDialogHandler _jsDialogHandler;
        private readonly WorkerCefContextMenuHandler _contextMenuHandler;

        private CefWorker _mainWorker;

        public delegate void LoadFinished(int StatusCode);

        public event LoadFinished OnLoadFinished;

        public WorkerCefClient(int windowWidth, int windowHeight,CefWorker mainCefWorker)
        {
            _mainWorker = mainCefWorker;
            _renderHandler = new WorkerCefRenderHandler(windowWidth, windowHeight);
            _loadHandler = new WorkerCefLoadHandler(_mainWorker);
            _lifespanHandler=new WorkerCefLifespanHandler(_mainWorker);
            _requestHandler=new WorkerCefWebRequestHandler(_mainWorker);
            _jsDialogHandler=new WorkerCefJSDialogHandler(_mainWorker);
            _contextMenuHandler=new WorkerCefContextMenuHandler();

        }

        public void ContinueDialog(bool res, string input)
        {
            _jsDialogHandler.Continue(res,input);
        }

        protected override CefRequestHandler GetRequestHandler()
        {
            return _requestHandler;
        }

        protected override CefContextMenuHandler GetContextMenuHandler()
        {
            return _contextMenuHandler;
        }

        protected override CefJSDialogHandler GetJSDialogHandler()
        {
            return _jsDialogHandler;
        }



        public void SetMemServer(SharedTextureWriter memServer)
        {
            _renderHandler._memServer = memServer;
        }



        protected override CefRenderHandler GetRenderHandler()
        {
            return _renderHandler;
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            var handled = _mainWorker.BrowserMessageRouter.OnProcessMessageReceived(browser, frame, sourceProcess, message);
            if (handled) return true;

            return false;
        }


        protected override CefLoadHandler GetLoadHandler()
        {
            return _loadHandler;
        }

        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return _lifespanHandler;
        }

        public int GetWidth()
        {
            return _renderHandler.CurrentWidth;
        }

        public int GetHeight()
        {
            return _renderHandler.CurrentHeight;
        }

        

#region Events
        public void MouseEvent(int x,int y,bool updown,MouseButton button)
        {
            //_lifespanHandler.MainBrowserHost.SendFocusEvent(true);
            //_lifespanHandler.MainBrowser.GetHost().SendFocusEvent(true);
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X =x,
                Y =y,
            };
            CefEventFlags modifiers = new CefEventFlags();
            CefMouseButtonType mouse=CefMouseButtonType.Left;
            if (button == MouseButton.Left)
            {
                modifiers |= CefEventFlags.LeftMouseButton;
                mouse=CefMouseButtonType.Left;
            }
            if (button == MouseButton.Right)
            {
                mouse = CefMouseButtonType.Right;
                modifiers |= CefEventFlags.RightMouseButton;
            }
            if (button == MouseButton.Middle)
            {
                mouse = CefMouseButtonType.Middle;
                modifiers |= CefEventFlags.MiddleMouseButton;
            }
            mouseEvent.Modifiers = modifiers;
          // log.Info("CLICK:" + x + "," + y);
          

            _lifespanHandler.MainBrowser.GetHost().SendMouseClickEvent(mouseEvent,mouse, updown,1);
            
        }

        public void MouseMoveEvent(int x, int y,MouseButton button)
        {
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X = x,
                Y = y,
            };
            CefEventFlags modifiers = new CefEventFlags();
            if (button == MouseButton.Left)
                modifiers |= CefEventFlags.LeftMouseButton;
            if (button == MouseButton.Right)
                modifiers |= CefEventFlags.RightMouseButton;
            if (button == MouseButton.Middle)
                modifiers |= CefEventFlags.MiddleMouseButton;
            mouseEvent.Modifiers = modifiers;
            _lifespanHandler.MainBrowser.GetHost().SendMouseMoveEvent(mouseEvent,false);
        }

        public void TouchEvent(int id,  float x, float y, float radX, float radY, float rotAngle, float pressure, TouchEventType type, PointerType pointerType)
        {
            CefTouchEvent touchEvent = new CefTouchEvent()
            {
                Id = id,
                X = x,
                Y = y,

                RadiusX = radX,
                RadiusY = radY,
                RotationAngle = rotAngle,

                Pressure = pressure,
            };

            //CefEventFlags modifiers = new CefEventFlags();

            CefPointerType cefpointerType = CefPointerType.Unknown;
            if (pointerType == PointerType.Eraser)
                cefpointerType = CefPointerType.Eraser;
            else if (pointerType == PointerType.Mouse)
                cefpointerType = CefPointerType.Mouse;
            else if (pointerType == PointerType.Pen)
                cefpointerType = CefPointerType.Pen;
            else if (pointerType == PointerType.Touch)
                cefpointerType = CefPointerType.Touch;
            touchEvent.PointerType = cefpointerType;

            CefTouchEventType cefType = new CefTouchEventType();
            if (type == TouchEventType.Cancelled)
                cefType = CefTouchEventType.Cancelled;
            if (type == TouchEventType.Moved)
                cefType = CefTouchEventType.Moved;
            if (type == TouchEventType.Pressed)
                cefType = CefTouchEventType.Pressed;
            if (type == TouchEventType.Released)
                cefType = CefTouchEventType.Released;
            touchEvent.Type = cefType;

            _lifespanHandler.MainBrowser.GetHost().SendTouchEvent(touchEvent);
        }

        public void KeyboardEvent(int character,KeyboardEventType type)
        {
            CefKeyEvent keyEvent = new CefKeyEvent()
            {
                EventType = CefKeyEventType.Char,
                WindowsKeyCode = character
            };
            if(type==KeyboardEventType.Down)
                keyEvent.EventType=CefKeyEventType.KeyDown;
            if(type==KeyboardEventType.Up)
                keyEvent.EventType=CefKeyEventType.KeyUp;

            _lifespanHandler.MainBrowser.GetHost().SendKeyEvent(keyEvent);
        }

        public void FocusEvent(int focus)
        {
            if (focus == 0)
                _lifespanHandler.MainBrowser.GetHost().SetFocus(false);
            else
            _lifespanHandler.MainBrowser.GetHost().SetFocus(true);
        }

        public void MouseLeaveEvent()
        {
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X = 0,
                Y = 0
            };
            _lifespanHandler.MainBrowser.GetHost().SendMouseMoveEvent(mouseEvent, false);

        }

        public void MouseWheelEvent(int x, int y, int delta)
        {
            CefMouseEvent mouseEvent = new CefMouseEvent()
            {
                X = x,
                Y = y
            };
            _lifespanHandler.MainBrowser.GetHost().SendMouseWheelEvent(mouseEvent,0,delta);
        }

        #endregion

        #region Controls

        public void Navigate(string url)
        {
            _lifespanHandler.MainBrowser.GetMainFrame().LoadUrl(url);
        }

        public void GoBack()
        {
            if(_lifespanHandler.MainBrowser.CanGoBack)
            _lifespanHandler.MainBrowser.GoBack();
        }

        public void GoForward()
        {
            if (_lifespanHandler.MainBrowser.CanGoForward)
                _lifespanHandler.MainBrowser.GoForward();
        }


        #endregion


        #region javascript

        public void ExecuteJavaScript(string jscode)
        {
            CefFrame frame = _lifespanHandler.MainBrowser.GetMainFrame();
            frame.ExecuteJavaScript(jscode,frame.Url,0);
        }

        #endregion
        public void Shutdown()
        {
            _lifespanHandler.MainBrowser.GetHost().CloseBrowser();
            _lifespanHandler.MainBrowser.Dispose();


        }
    }
}