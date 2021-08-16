using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class AtlasGump: Gump
    {
        private const ushort HUE_FONT = 0xFFFF;
        private const ushort BACKGROUND_COLOR = 999;
        private const ushort GUMP_WIDTH = 300;
        private const ushort GUMP_HEIGHT = 450;

        private readonly int _gumpPosX = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125;
        private readonly int _gumpPosY = 100;

        private Combobox _itemSelected;
        private Combobox _hueSelected;

        private Label _itemNameLabel;
        private Label _sourceLabel;

        private Label _hueNameLabel;
        private Label _hueSourceLabel;

        private GumpPic _showItem;

        private List<AtlasItem> _mockItems = new List<AtlasItem>
        {
            new AtlasItem(0x13B7, "long sword", "Carpenter", 1),
            new AtlasItem(0x13B8, "long sword", "Dunno", 1),
            new AtlasItem(0x13B9, "viking sword", "Dunno", 1),
            new AtlasItem(0x13BA, "viking sword", "Dunno", 1),
            new AtlasItem(0x13BB, "chainmail coif", "Dunno", 1),
            new AtlasItem(0x13BC, "chainmail leggins", "Dunno", 1),
        };

        private List<AtlasHue> _mockHue = new List<AtlasHue>
        {
            new AtlasHue(0, "no hue", ""),
            new AtlasHue(1483, "Shimmer Oxide", "Launch exclusive"),
            new AtlasHue(1476, "Shimmer Cardinal", "Winter holidays"),
            new AtlasHue(1788, "Water Aspect", "Water aspect"),
            new AtlasHue(1782, "Command Aspect", "Command Aspect"),

        };
        
        string[] layerList = {
            "OneHanded", "TwoHanded", "Shoes", "Pants"
        };

        public AtlasGump() : base(0, 0)
        {
            CanMove = true;
            AcceptMouseInput = true;

            Add
            (
                new AlphaBlendControl(0.05f)
                {
                    X = _gumpPosX,
                    Y = _gumpPosY,
                    Width = GUMP_WIDTH,
                    Height = GUMP_HEIGHT,
                    Hue = BACKGROUND_COLOR,
                    AcceptMouseInput = true,
                    CanMove = true,
                    CanCloseWithRightClick = true,
                }
            );

            Add(new Combobox(_gumpPosX + 10, _gumpPosY + 10, 200, layerList, 0));

            _itemSelected = new Combobox(_gumpPosX + 10, _gumpPosY + 40, 200, _mockItems.Select(x => x.Name).ToArray(), 0);
            _itemSelected.OnOptionSelected += _itemSelected_OnOptionSelected;
            Add(_itemSelected);

            _hueSelected = new Combobox(_gumpPosX + 10, _gumpPosY + 70, 200, _mockHue.Select(x => $"{x.Name} [{x.HueId}]").ToArray(), 0);
            _hueSelected.OnOptionSelected += _itemSelected_OnOptionSelected;
            Add(_hueSelected);

            Add(new Label("Item info", true, HUE_FONT) { X = _gumpPosX + 10, Y = _gumpPosY + 280 });

            Add(new Label("Hue info", true, HUE_FONT) { X = _gumpPosX + 10, Y = _gumpPosY + 360 });
            SetInScreen();
        }

        private void DrawPic(ushort graphicId, ushort hue, string name, string source, string hName, string hSource)
        {
            Remove(_showItem);
            Remove(_itemNameLabel);
            Remove(_sourceLabel);
            Remove(_hueNameLabel);
            Remove(_hueSourceLabel);

            _showItem = new GumpPic(_gumpPosX + 30, _gumpPosY + 100, graphicId, hue);

            _itemNameLabel = new Label($"Name: {name}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 300 };
            _sourceLabel = new Label($"Source: {source}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 320 };

            _hueNameLabel = new Label($"Name: {hName}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 380 };
            _hueSourceLabel = new Label($"Source: {hSource}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 400 };

            Add(_showItem);
            Add(_itemNameLabel);
            Add(_sourceLabel);
            Add(_hueNameLabel);
            Add(_hueSourceLabel);
        }

        private void _itemSelected_OnOptionSelected(object sender, int e)
        {
            var selectedGraphicId = _mockItems[_itemSelected.SelectedIndex];
            var selectedHue = _mockHue[_hueSelected.SelectedIndex];
            DrawPic(
                (ushort)selectedGraphicId.GraphicId,
                (ushort)selectedHue.HueId,
                selectedGraphicId.Name,
                selectedGraphicId.Location,
                selectedHue.Name,
                selectedHue.Location);
        }
        
        private class AtlasItem
        {
            public int GraphicId { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            private ushort Layer { get; set; }

            public AtlasItem(int graphicId, string name, string location, ushort layer)
            {
                GraphicId = graphicId;
                Name = name;
                Location = location;
                Layer = layer;
            }
        }

        private class AtlasHue
        {
            public int HueId { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }

            public AtlasHue(int hueId, string name, string location)
            {
                HueId = hueId;
                Name = name;
                Location = location;
            }
        }
    }
}
