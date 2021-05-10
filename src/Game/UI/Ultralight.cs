using ClassicUO.Renderer;
using ImpromptuNinjas.UltralightSharp;
using ImpromptuNinjas.UltralightSharp.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using String = ImpromptuNinjas.UltralightSharp.String;

namespace ClassicUO.Game
{
    /// <summary>
    /// Wrapper for Ultralight
    /// </summary>
    internal unsafe class UltralightWrpper
    {
        /// <summary>
        /// Config for Ultraligh
        /// </summary>
        private static Config* cfg = null;
        /// <summary>
        /// Renderer
        /// </summary>
        private static ImpromptuNinjas.UltralightSharp.Renderer* renderer = null;
        /// <summary>
        /// SessionName
        /// </summary>
        private static String* sessionName;
        /// <summary>
        /// Session
        /// </summary>
        private static Session* session;
        /// <summary>
        /// View
        /// </summary>
        private static View* view;
        /// <summary>
        /// Surface
        /// </summary>
        private static Surface* surface;
        /// <summary>
        /// Bitmap
        /// </summary>
        private static Bitmap* bitmap;
        /// <summary>
        /// Texture
        /// </summary>
        private static Texture2D texture;
        /// <summary>
        /// Indictor when page is lodaed
        /// </summary>
        private static bool isLoaded = false;

        /// <summary>
        /// Initialize Ultralight
        /// </summary>
        public static unsafe void Init()
        {
            LoggerLogMessageCallback cb = LoggerCallback;
            Ultralight.SetLogger(new Logger { LogMessage = cb });
            
            // Get assemly dir 
            var asmPath = new Uri(typeof(GameController).Assembly.CodeBase).LocalPath;
            var asmDir = Path.GetDirectoryName(asmPath);
            // Get temp dir
            var tempDir = Path.GetTempPath();
            // find a place to stash instance storage
            string storagePath;
            do
            {
                storagePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
            } while (Directory.Exists(storagePath) || File.Exists(storagePath));

            // Set config
            cfg = Config.Create();
            {
                var cachePath = String.Create(Path.Combine(storagePath, "Cache"));
                cfg->SetCachePath(cachePath);
                cachePath->Destroy();
                cfg->SetUseGpuRenderer(false);
                cfg->SetEnableImages(true);
                cfg->SetEnableJavaScript(true);
            }

            // Load resources
            {
                var resourcePath = String.Create(Path.Combine(asmDir, "resources"));
                cfg->SetResourcePath(resourcePath);
                resourcePath->Destroy();
            }
            
            // Set Font Loader
            AppCore.EnablePlatformFontLoader();

            {
                var assetsPath = String.Create(Path.Combine(asmDir, "assets"));
                AppCore.EnablePlatformFileSystem(assetsPath);
                assetsPath->Destroy();
            }

            // Create Renderer
            renderer = ImpromptuNinjas.UltralightSharp.Renderer.Create(cfg);
            sessionName = String.Create("ClassicUO");
            session = Session.Create(renderer, false, sessionName);

            // Create View
            view = View.Create(renderer, 640, 480, false, session);

            // Set finishing Loading Callback
            view->SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) => {
                Console.WriteLine($"Loading Finished, URL: 0x{(ulong)url:X8}  {url->Read()}");

                isLoaded = true;
            }, null);
            
            isLoaded = false;

            // Load default page
            view->LoadUrl(String.Create("https://tenor.com/view/blink-blinking-glasses-stare-gif-5182689"));
            view->Focus();

            ReloadRenderer();
        }

        /// <summary>
        /// Draw Loaded Page
        /// </summary>
        /// <param name="batcher">Ultima 2D Batcher</param>
        public static void Draw(UltimaBatcher2D batcher)
        {
            ReloadView();

            DrawFrame(batcher);
        }

        /// <summary>
        /// Create Texture2D from Bmp
        /// </summary>
        /// <param name="pixels">Pointer to pixels array</param>
        /// <param name="w"> Picture Width</param>
        /// <param name="h"> Picture Height</param>
        /// <returns></returns>
        private static Texture2D GetTextureFromBmp(void* pixels, int w, int h)
        {
            var tempTexture = new Texture2D(Client.Game.GraphicsDevice, w, h);
            uint[] pixelsArray = new uint[w * h];
            var pPixels = (byte*)pixels;
                
            Color[] data = new Color[w * h];

            for (int i = 0; i < data.Length; i++)
            {
                // Go through every color and reverse red and blue channels
                data[i] = new Color(pPixels[2], pPixels[1], pPixels[0], pPixels[3]);
                pPixels += 4;
            }

            tempTexture.SetData(pixelsArray);
            return tempTexture;
        }

        /// <summary>
        /// Load Url
        /// </summary>
        /// <param name="url">Url string</param>
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

        /// <summary>
        /// Is Page Loaded
        /// </summary>
        /// <returns>Indictor is page loaded</returns>
        public static bool IsLoaded()
        {
            return isLoaded;
        }

        /// <summary>
        /// Draw Single Frame using UltimaBatcher2D
        /// </summary>
        /// <param name="batcher">Ultima Batcher 2D</param>
        public static void DrawFrame(UltimaBatcher2D batcher)
        {
            batcher.Begin();
            var hue = new Vector3(0, 0, 0);
            batcher.Draw2D(texture, 10, 10, ref hue);
            batcher.End();
        }

        /// <summary>
        /// Reload Single Frame
        /// </summary>
        public static void ReloadView()
        {
            renderer->Update();
            renderer->Render();

            {
                surface = view->GetSurface();
                bitmap = surface->GetBitmap();
                var pixels = bitmap->LockPixels();
                texture = GetTextureFromBmp(pixels, (int)bitmap->GetWidth(), (int)bitmap->GetHeight());
                bitmap->UnlockPixels();
            }
        }

        /// <summary>
        /// Reload Renderer
        /// </summary>
        public static void ReloadRenderer()
        {
            while (!isLoaded)
            {
                renderer->Update();
                renderer->Render();
            }
        }

        /// <summary>
        /// Logger Callback Method (also set isLoaded after finish)
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="msg">Message</param>
        private static unsafe void LoggerCallback(LogLevel logLevel, String* msg)
            => Console.WriteLine($"{logLevel}: {msg->Read()}");
    }
}
