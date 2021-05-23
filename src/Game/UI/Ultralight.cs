using ClassicUO.Renderer;
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
    internal unsafe class UltralightWrapper
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
        private View* _view;
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

        }

        /// <summary>
        /// Draw Loaded Page
        /// </summary>
        /// <param name="batcher">Ultima 2D Batcher</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public void Draw(UltimaBatcher2D batcher, int x, int y)
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
        public void DrawDynamic(UltimaBatcher2D batcher, int x, int y)
        {
            ReloadDynamicView();
            DrawFrame(batcher, x, y);
        }

        /// <summary>
        /// Resize view
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void ResizeView(int width, int height)
        {
            _view->Resize((uint)width, (uint)height);
        }

        /// <summary>
        /// Load Url
        /// </summary>
        /// <param name="url">Url string</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public void LoadUrl(string url, int w, int h)
        {
            // Create View
            _view = View.Create(_renderer, (uint)w, (uint)h, false, _session);
            {
                var urlString = String.Create(url);
                _view->LoadUrl(urlString);
                urlString->Destroy();
            }
        }

        /// <summary>
        /// Load Local HTML File
        /// </summary>
        /// <param name="url">File Name</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public void LoadLocalUrl(string fileName, int w, int h)
        {
            _view = View.Create(_renderer, (uint)w, (uint)h, false, _session);

            {
                var urlString = String.Create($"file:///{fileName}");
                _view->LoadUrl(urlString);
                urlString->Destroy();
            }
        }

        /// <summary>
        /// Mouse Click - It's contain mouse down and up event
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public void MouseClick(int x, int y)
        {
            // Mouse down
            MouseEvent* evt = MouseEvent.Create(
                MouseEventType.MouseDown,
                x,
                y,
                MouseButton.Left);
            _view->FireMouseEvent(evt);

            // Mouse up
            evt = MouseEvent.Create(
                MouseEventType.MouseUp,
                x,
                y,
                MouseButton.Left);
            _view->FireMouseEvent(evt);
        }

        /// <summary>
        /// Dispatch
        /// </summary>
        public void Clear()
        {
            _view->Destroy();
            _renderer->PurgeMemory();
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
        private void ReloadView()
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
                GenerateTexture();
                _surface->ClearDirtyBounds();
            }
        }

        /// <summary>
        /// Reload Dynamic View
        /// Since GetDirtyBounds for dynamic views (like gifs) is never empty there is no need to check that flag
        /// </summary>
        private void ReloadDynamicView()
        {
            _renderer->Update();
            _renderer->Render();

            {
                _surface = _view->GetSurface();
                _bitmap = _surface->GetBitmap();
                GenerateTexture();
            }
        }

        /// <summary>
        /// Generate Texture form BMP
        /// </summary>
        private static void GenerateTexture()
        {
            int w = (int)_bitmap->GetWidth();
            int h = (int)_bitmap->GetHeight();
            int bpp = (int)_bitmap->GetBpp();
            var pixels = (byte*)_bitmap->LockPixels();

            int arrSize = w * h * bpp;

            using (var ms = new UnmanagedMemoryStream(pixels, arrSize))
            {
                byte[] buffer = new byte[arrSize];

                ms.Read(buffer, 0, arrSize);

                for (int i = 0; i < buffer.Length - 2; i += 4)
                {
                    byte r = buffer[i];
                    buffer[i] = buffer[i + 2];
                    buffer[i + 2] = r;
                }

                var tex = new Texture2D(Client.Game.GraphicsDevice, w, h);
                tex.SetData(buffer, 0, arrSize);

                _bitmap->UnlockPixels();
                _texture = tex;
            }
        }

    }
}
