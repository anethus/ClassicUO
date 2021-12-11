using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Controls
{

    internal class FontSelectorCombobox : Control
    {
        private readonly Label _label;
        private readonly int _maxHeight;
        private byte _selectedIndex;
        private byte _maxFonts;

        public FontSelectorCombobox
        (
            int x,
            int y,
            int width,
            byte font,
            int maxHeight = 200,
            bool showArrow = true,
            byte maxFonts = 7
        )
        {
            X = x;
            Y = y;
            Width = width;
            Height = 25;
            SelectedIndex = font;
            _maxHeight = maxHeight;
            _maxFonts = maxFonts;

            Add
            (
                new ResizePic(0x0BB8)
                {
                    Width = width, Height = Height
                }
            );

            _label = new Label(
                "This Font!",
                true,
                0,
                width,
                font,
                FontStyle.None,
                TEXT_ALIGN_TYPE.TS_CENTER)
            {
                X = 2, Y = 5
            };

            Add(_label);

            if (showArrow)
            {
                Add(new GumpPic(width - 18, 2, 0x00FC, 0));
            }
        }


        public byte SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;

                if (_label != null && FontsLoader.Instance.UnicodeFontExists((byte)_selectedIndex))
                {
                    _label.Font = (byte)_selectedIndex;

                    OnOptionSelected?.Invoke(this, value);
                }
            }
        }


        public event EventHandler<int> OnOptionSelected;


        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (batcher.ClipBegin(x, y, Width, Height))
            {
                base.Draw(batcher, x, y);
                batcher.ClipEnd();
            }

            return true;
        }


        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button != MouseButtonType.Left)
            {
                return;
            }

            int comboY = ScreenCoordinateY + Offset.Y;

            if (comboY < 0)
            {
                comboY = 0;
            }
            else if (comboY + _maxHeight > Client.Game.Window.ClientBounds.Height)
            {
                comboY = Client.Game.Window.ClientBounds.Height - _maxHeight;
            }

            UIManager.Add
            (
                new FontSelectorGump
                (
                    ScreenCoordinateX,
                    comboY,
                    Width,
                    _maxHeight,
                    _maxFonts,
                    this
                )
            );

            base.OnMouseUp(x, y, button);
        }

        private class FontSelectorGump : Gump
        {
            private const int ELEMENT_HEIGHT = 20;


            private readonly FontSelectorCombobox _combobox;

            public FontSelectorGump
            (
                int x,
                int y,
                int width,
                int maxHeight,
                byte maxFonts,
                FontSelectorCombobox combobox
            ) : base(0, 0)
            {
                CanMove = false;
                AcceptMouseInput = true;
                X = x;
                Y = y;

                IsModal = true;
                LayerOrder = UILayer.Over;
                ModalClickOutsideAreaClosesThisControl = true;

                _combobox = combobox;

                ResizePic background;
                Add(background = new ResizePic(0x0BB8));
                background.AcceptMouseInput = false;

                List<HoveredLabel> labels = new List<HoveredLabel>();

                for (byte i = 0; i < maxFonts; i++)
                {
                    if (!FontsLoader.Instance.UnicodeFontExists(i)) break;

                    HoveredLabel label = new HoveredLabel
                    (
                        "This Font!",
                        true,
                        0,
                        0,
                        0,
                        width,
                        i,
                        FontStyle.None,
                        TEXT_ALIGN_TYPE.TS_CENTER
                    )
                    {
                        X = 2,
                        Y = i * ELEMENT_HEIGHT,
                        DrawBackgroundCurrentIndex = true,
                        Tag = i,
                        Height = ELEMENT_HEIGHT,
                    };
                    label.MouseUp += LabelOnMouseUp;
                    labels.Add(label);
                }

                int totalHeight = Math.Min(maxHeight, labels.Max(o => o.Y + o.Height));
                int maxWidth = Math.Max(width, labels.Max(o => o.X + o.Width));

                ScrollArea area = new ScrollArea
                (
                    0,
                    0,
                    maxWidth + 15,
                    totalHeight,
                    true
                );

                foreach (HoveredLabel label in labels)
                {
                    label.Width = maxWidth;
                    area.Add(label);
                }

                Add(area);

                background.Width = maxWidth;
                background.Height = totalHeight;
            }


            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                if (batcher.ClipBegin(x, y, Width, Height))
                {
                    base.Draw(batcher, x, y);

                    batcher.ClipEnd();
                }

                return true;
            }

            private void LabelOnMouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtonType.Left)
                {
                    _combobox.SelectedIndex = (byte)((Label)sender).Tag;

                    Dispose();
                }
            }
        }
    }

}
