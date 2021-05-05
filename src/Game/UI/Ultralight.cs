using ImpromptuNinjas.UltralightSharp;
using ImpromptuNinjas.UltralightSharp.Enums;
using System;
using System.IO;
using Renderer = ImpromptuNinjas.UltralightSharp.Renderer;
using String = ImpromptuNinjas.UltralightSharp.String;

namespace ClassicUO.Game
{
    public unsafe class UltralightWrpper
    {
        private static Config* cfg = null;
        private static ImpromptuNinjas.UltralightSharp.Renderer* renderer = null;
        private static String* sessionName;
        private static Session* session;
        private static View* view;

        private static bool isLoaded = false;

        public static unsafe void Init()
        {
            LoggerLogMessageCallback cb = LoggerCallback;
            Ultralight.SetLogger(new Logger { LogMessage = cb });

            var asmPath = new Uri(typeof(GameController).Assembly.CodeBase).LocalPath;
            var asmDir = Path.GetDirectoryName(asmPath);
            var tempDir = Path.GetTempPath();
            // find a place to stash instance storage
            string storagePath;
            do
            {
                storagePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
            } while (Directory.Exists(storagePath) || File.Exists(storagePath));

            cfg = Config.Create();

            {
                var cachePath = String.Create(Path.Combine(storagePath, "Cache"));
                cfg->SetCachePath(cachePath);
                cachePath->Destroy();
            }

            {
                var resourcePath = String.Create(Path.Combine(asmDir, "resources"));
                cfg->SetResourcePath(resourcePath);
                resourcePath->Destroy();
            }

            cfg->SetUseGpuRenderer(false);
            cfg->SetEnableImages(true);
            cfg->SetEnableJavaScript(false);

            AppCore.EnablePlatformFontLoader();

            {
                var assetsPath = String.Create(Path.Combine(asmDir, "assets"));
                AppCore.EnablePlatformFileSystem(assetsPath);
                assetsPath->Destroy();
            }

            renderer = ImpromptuNinjas.UltralightSharp.Renderer.Create(cfg);
            sessionName = String.Create("Demo");
            session = Session.Create(renderer, false, sessionName);

            view = View.Create(renderer, 640, 480, false, session);

            {
                var htmlString = String.Create("<i>Loading...</i>");
                Console.WriteLine($"Loading HTML: {htmlString->Read()}");
                view->LoadHtml(htmlString);
                htmlString->Destroy();
            }

            view->SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) => {
                Console.WriteLine($"Loading Finished, URL: 0x{(ulong)url:X8}  {url->Read()}");

                isLoaded = true;
            }, null);

            while (!isLoaded)
            {
                Ultralight.Update(renderer);
                Ultralight.Render(renderer);
            }

            view->LoadUrl(String.Create("http://google.com"));
            view->Focus();
        }

        public static void LoadUrl(string url)
        {
            isLoaded = false;

            {
                var urlString = String.Create(url);
                Console.WriteLine($"Loading URL: {urlString->Read()}");
                view->LoadUrl(urlString);
                urlString->Destroy();
            }
        }

        public static void Reload()
        {
            while (!isLoaded)
            {
                //Ultralight.Update(renderer);
                renderer->Update();
                //Ultralight.Render(renderer);
                renderer->Render();
            }
        }

        public static void GetUrl()
        {
            {
                var urlStrPtr = view->GetUrl();
                Console.WriteLine($"After Loaded View GetURL: 0x{(ulong)urlStrPtr:X8} {urlStrPtr->Read()}");
            }
        }

        public static void LoadSurface()
        {
            {
                var surface = view->GetSurface();
                var bitmap = surface->GetBitmap();
                var pixels = bitmap->LockPixels();
                //RenderAnsi<Bgra32>(pixels, bitmap->GetWidth(), bitmap->GetHeight());
                bitmap->UnlockPixels();
                bitmap->SwapRedBlueChannels();
            }
        }


        private static unsafe void LoggerCallback(LogLevel logLevel, String* msg)
            => Console.WriteLine($"{logLevel.ToString()}: {msg->Read()}");
    }
}
