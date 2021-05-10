using ClassicUO.Renderer;
using ClassicUO.Utility.Logging;
using ImpromptuNinjas.UltralightSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using ImpromptuNinjas.UltralightSharp.Enums;
using String = ImpromptuNinjas.UltralightSharp.String;

namespace ClassicUO.Game.UI
{
    /// <summary>
    /// Wrapper for Ultralight
    /// </summary>
    internal unsafe class UltralightWrpper
    {
        /// <summary>
        /// Config for Ultralight
        /// </summary>
        private static Config* _cfg = null;
        /// <summary>
        /// Renderer
        /// </summary>
        private static ImpromptuNinjas.UltralightSharp.Renderer* _renderer = null;
        /// <summary>
        /// SessionName
        /// </summary>
        private static String* _sessionName;
        /// <summary>
        /// Session
        /// </summary>
        private static Session* _session;
        /// <summary>
        /// View
        /// </summary>
        private static View* _view;
        /// <summary>
        /// Surface
        /// </summary>
        private static Surface* _surface;
        /// <summary>
        /// Bitmap
        /// </summary>
        private static Bitmap* _bitmap;
        /// <summary>
        /// Texture
        /// </summary>
        private static Texture2D _texture;

        /// <summary>
        /// Initialize Ultralight
        /// </summary>
        public static void Init()
        {
            // Get assembly dir 
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
            _cfg = Config.Create();
            {
                var cachePath = String.Create(Path.Combine(storagePath, "Cache"));
                _cfg->SetCachePath(cachePath);
                cachePath->Destroy();
                _cfg->SetUseGpuRenderer(false);
                _cfg->SetEnableImages(true);
                _cfg->SetEnableJavaScript(true);
                _cfg->SetUserAgent(String.Create("Mozilla/5.0"));
            }

            // Load resources
            {
                if (asmDir != null)
                {
                    var resourcePath = String.Create(Path.Combine(asmDir, "resources"));
                    _cfg->SetResourcePath(resourcePath);
                    resourcePath->Destroy();
                }
            }

            // Set Font Loader
            AppCore.EnablePlatformFontLoader();

            {
                if (asmDir != null)
                {
                    var assetsPath = String.Create(Path.Combine(asmDir, "assets"));
                    AppCore.EnablePlatformFileSystem(assetsPath);
                    assetsPath->Destroy();
                }
            }

            // Create Renderer
            _renderer = ImpromptuNinjas.UltralightSharp.Renderer.Create(_cfg);
            _sessionName = String.Create("ClassicUO");
            _session = Session.Create(_renderer, false, _sessionName);

            // Create View
            _view = View.Create(_renderer, 640, 480, false, _session);

            // Set finishing Loading Callback
            _view->SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) =>
            {
                Log.Info($"Loading Finished, URL: 0x{(ulong)url:X8}  {url->Read()}");
            }, null);

        }

        /// <summary>
        /// Draw Loaded Page
        /// </summary>
        /// <param name="batcher">Ultima 2D Batcher</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public static void Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ReloadView();
            DrawFrame(batcher, x, y);
        }

        /// <summary>
        /// Draw Dynamic Loaded Page
        /// </summary>
        /// <param name="batcher">Ultima 2D Batcher</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public static void DrawDynamic(UltimaBatcher2D batcher, int x, int y)
        {
            ReloadDynamicView();
            DrawFrame(batcher, x, y);
        }

        /// <summary>
        /// Resize view
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void ResizeView(int width, int height)
        {
            _view->Resize((uint)width, (uint)height);
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
            var pPixels = (byte*)pixels;

            Color[] data = new Color[w * h];

            for (int i = 0; i < data.Length; i++)
            {
                // Go through every color and reverse red and blue channels
                data[i] = new Color(pPixels[2], pPixels[1], pPixels[0], pPixels[3]);
                pPixels += 4;
            }

            tempTexture.SetData(data);
            return tempTexture;
        }

        /// <summary>
        /// Load Url
        /// </summary>
        /// <param name="url">Url string</param>
        public static void LoadUrl(string url)
        {
            {
                var urlString = String.Create(url);
                _view->LoadUrl(urlString);
                urlString->Destroy();
            }
        }

        /// <summary>
        /// Draw Single Frame using UltimaBatcher2D
        /// </summary>
        /// <param name="batcher">Ultima Batcher 2D</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        private static void DrawFrame(UltimaBatcher2D batcher, int x, int y)
        {
            var hue = new Vector3(0, 0, 0);
            batcher.Draw2D(_texture, x, y, ref hue);
        }

        /// <summary>
        /// Reload Single Frame
        /// </summary>
        private static void ReloadView()
        {
            _renderer->Update();
            _renderer->Render();

            {
                _surface = _view->GetSurface();

                if (_surface->GetDirtyBounds().IsEmpty())
                {
                    return;
                }

                _bitmap = _surface->GetBitmap();
                var pixels = _bitmap->LockPixels();
                _texture = GetTextureFromBmp(pixels, (int)_bitmap->GetWidth(), (int)_bitmap->GetHeight());
                _bitmap->UnlockPixels();
                _surface->ClearDirtyBounds();
            }
        }

        /// <summary>
        /// Reload Dynamic View
        /// Since GetDirtyBounds for dynamic views (like gifs) is never empty there is no need to check that flag
        /// </summary>
        private static void ReloadDynamicView()
        {
            _renderer->Update();
            _renderer->Render();

            {
                _surface = _view->GetSurface();
                _bitmap = _surface->GetBitmap();
                var pixels = _bitmap->LockPixels();
                _texture = GetTextureFromBmp(pixels, (int)_bitmap->GetWidth(), (int)_bitmap->GetHeight());
                _bitmap->UnlockPixels();
            }
        }

        public static void MouseClick(int x, int y)
        {
            MouseEvent* evt = MouseEvent.Create(
                MouseEventType.MouseDown,
                x,
                y,
                MouseButton.Left);
            _view->FireMouseEvent(evt);
        }

        /// <summary>
        /// Dispatch
        /// </summary>
        public static void Clear()
        {
            _view->Unfocus();
            _renderer->PurgeMemory();
        }

    }
}
