using ClassicUO.Input;
using ClassicUO.Renderer;

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
            ulWrapp.LoadUrl(url, (int)width, (int)height);
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
            if (button == MouseButtonType.Left)
            {
                ulWrapp.MouseClick(x, y);
            }
        }

        public override void Dispose()
        {
            ulWrapp.Clear();
        }
    }
}
