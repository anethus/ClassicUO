using ClassicUO.Input;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls
{
    internal class Html5Control : Control
    {
        public Html5Control(string url, uint width, uint height)
        {
            Width = (int)width;
            Height = (int)height;
            UltralightWrpper.LoadUrl(url);
            UltralightWrpper.ResizeView((int)width, (int)height);
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

                UltralightWrpper.Draw(batcher, x + 8, y + 8);

                batcher.ClipEnd();
            }

            return true;
        }

        protected override void OnMouseDown(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Left)
            {
                UltralightWrpper.MouseClick(x, y);
            }
        }

        public override void Dispose()
        {
            UltralightWrpper.Clear();
        }
    }
}
