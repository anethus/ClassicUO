using ClassicUO.Input;
using ClassicUO.Renderer;
using SDL2;

namespace ClassicUO.Game.UI.Controls
{
    internal class Html5Control : Control
    {
        private readonly UltralightWrapper ulWrapp;
        public Html5Control(string url, uint width, uint height)
        {
            Width = (int)width;
            Height = (int)height;
            ulWrapp = new UltralightWrapper();
            ulWrapp.LoadLocalUrl("index.html", (int)width, (int)height);
            //ulWrapp.LoadUrl("http://discord.com", (int)width, (int)height);
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (batcher.ClipBegin(x, y, Width, Height))
            {
                base.Draw(batcher, x, y);

                ulWrapp.Draw(batcher, x + 8, y + 8);

                batcher.ClipEnd();
            }

            return true;
        }

        protected override void OnMouseDown(int x, int y, MouseButtonType button)
        {
            AcceptKeyboardInput = true;
            IsEditable = true;

            SetKeyboardFocus();
            if (button == MouseButtonType.Left)
            {
                ulWrapp.MouseClick(x, y);
            }
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_LSHIFT || key == SDL.SDL_Keycode.SDLK_RSHIFT)
                return;

            var lShift = mod & SDL.SDL_Keymod.KMOD_LSHIFT;
            var rShift = mod & SDL.SDL_Keymod.KMOD_RSHIFT;

            if(lShift == SDL.SDL_Keymod.KMOD_LSHIFT || rShift == SDL.SDL_Keymod.KMOD_RSHIFT)
            {
                if (key >= SDL.SDL_Keycode.SDLK_a && key <= SDL.SDL_Keycode.SDLK_z)
                    ulWrapp.KeyboardChar((char)(key - 32), (uint)lShift);
                else if (key == SDL.SDL_Keycode.SDLK_2)
                    ulWrapp.KeyboardChar('@', (uint)lShift);
                return;
            }
            ulWrapp.KeyboardChar((char)key, (uint)mod);
        }

        public override void Dispose()
        {
            ulWrapp.Clear();


            base.Dispose();
        }
    }
}
