using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class AtlasGump: Gump
    {
        private const ushort HUE_FONT = 0xFFFF;
        private const ushort BACKGROUND_COLOR = 999;
        private const ushort GUMP_WIDTH = 600;
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
        private Item _itemToAdd = new Item();
        private FittingRoomControl _fittingRoom;

        private List<AtlasItem> _mockItems = new List<AtlasItem>
        {
            new AtlasItem(0x13B7, "long sword", "Carpenter", Layer.OneHanded),
            new AtlasItem(0x13B8, "long sword", "Dunno", Layer.OneHanded),
            new AtlasItem(0x13B9, "viking sword", "Dunno", Layer.OneHanded),
            new AtlasItem(0x13BA, "viking sword", "Dunno", Layer.OneHanded),
            new AtlasItem(0x13BB, "chainmail coif", "Dunno", Layer.Helmet),
            new AtlasItem(0x13BC, "chainmail leggins", "Dunno", Layer.Pants)
        };

        private List<AtlasHue> _mockHue = new List<AtlasHue>
        {
            new AtlasHue(0, "no hue", ""),
            new AtlasHue(1483, "Shimmer Oxide", "Launch exclusive"),
            new AtlasHue(1476, "Shimmer Cardinal", "Winter holidays"),
            new AtlasHue(1788, "Water Aspect", "Water aspect"),
            new AtlasHue(1782, "Command Aspect", "Command Aspect")

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

            Add(new Label("Hue info", true, HUE_FONT) { X = _gumpPosX + 10, Y = _gumpPosY + 320 });

            _fittingRoom = new FittingRoomControl { X = _gumpPosX + 300, Y = _gumpPosY };
            Add(_fittingRoom);

            Add
            (
                new NiceButton
                (
                    _gumpPosX + 40, _gumpPosY + 400, 60, 30,
                    ButtonAction.Activate, "Wear"
                )
                {
                    ButtonParameter = 1,
                }
            );
            SetInScreen();
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case 1:
                    _fittingRoom.AddItemToLayer(_itemToAdd);
                    break;
            }
        }

        private void DrawPic(ushort graphicId, ushort hue, string name, string source, string hName, string hSource, byte layer)
        {
            Remove(_showItem);
            Remove(_itemNameLabel);
            //Remove(_sourceLabel);
            Remove(_hueNameLabel);
            //Remove(_hueSourceLabel);
            ref StaticTiles data = ref TileDataLoader.Instance.StaticData[graphicId];

            var gumpGraphicId = PaperDollInteractable.GetAnimID(graphicId, data.AnimID, false);
            _showItem = new GumpPic(_gumpPosX + 30, _gumpPosY + 100, gumpGraphicId, hue)
            {
                IsFromServer = false,
                IsPartialHue = data.IsPartialHue,
            };

            _itemNameLabel = new Label($"Name: {name}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 300 };
            //_sourceLabel = new Label($"Source: {source}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 320 };

            _hueNameLabel = new Label($"Name: {hName}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 340 };
            //_hueSourceLabel = new Label($"Source: {hSource}", true, HUE_FONT) { X = _gumpPosX + 20, Y = _gumpPosY + 400 };

            _itemToAdd.Graphic = graphicId;
            _itemToAdd.ItemData = data;
            _itemToAdd.Hue = hue;
            _itemToAdd.Layer = layer;

            Add(_showItem);
            Add(_itemNameLabel);
            //Add(_sourceLabel);
            Add(_hueNameLabel);
            //Add(_hueSourceLabel);
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
                selectedHue.Location,
                selectedGraphicId.Layer);
        }
        
        private class AtlasItem
        {
            public int GraphicId { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public byte Layer { get; set; }

            public AtlasItem(int graphicId, string name, string location, byte layer)
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

        internal sealed class FittingRoomControl : Control
        {
            private bool needRefresh = false;

            private List<Item> ItemList = new List<Item>
            {
                new Item { Graphic = 0, Layer = Layer.OneHanded },
                new Item { Graphic = 0, Layer = Layer.TwoHanded },
                new Item { Graphic = 0, Layer = Layer.Shoes },
                new Item { Graphic = 0, Layer = Layer.Pants },
                new Item { Graphic = 0, Layer = Layer.Shirt },
                new Item { Graphic = 0, Layer = Layer.Helmet },
                new Item { Graphic = 0, Layer = Layer.Gloves },
                new Item { Graphic = 0, Layer = Layer.Ring },
                new Item { Graphic = 0, Layer = Layer.Talisman },
                new Item { Graphic = 0, Layer = Layer.Necklace },
                new Item { Graphic = 0, Layer = Layer.Hair },
                new Item { Graphic = 0, Layer = Layer.Waist },
                new Item { Graphic = 0, Layer = Layer.Torso },
                new Item { Graphic = 0, Layer = Layer.Bracelet },
                new Item { Graphic = 0, Layer = Layer.Face },
                new Item { Graphic = 0, Layer = Layer.Beard },
                new Item { Graphic = 0, Layer = Layer.Tunic },
                new Item { Graphic = 0, Layer = Layer.Earrings },
                new Item { Graphic = 0, Layer = Layer.Arms },
                new Item { Graphic = 0, Layer = Layer.Cloak },
                new Item { Graphic = 0, Layer = Layer.Backpack },
                new Item { Graphic = 0, Layer = Layer.Robe },
                new Item { Graphic = 0, Layer = Layer.Skirt },
                new Item { Graphic = 0, Layer = Layer.Legs },
            };

            public FittingRoomControl()
            {
                Add(new GumpPic(0, 0, 0x0D, 0));
            }

            public void AddItemToLayer(Item item)
            {
                var elementIdx = ItemList.FindIndex(x => x.Layer == item.Layer);

                if (elementIdx == -1)
                {
                    return;
                }

                needRefresh = true;
                ItemList[elementIdx] = item;
            }

            public override void Update(double totalTime, double frameTime)
            {
                base.Update(totalTime, frameTime);

                if (needRefresh)
                {
                    UpdateUI();
                    needRefresh = false;
                }
            }

            private void UpdateUI()
            {
                if (IsDisposed)
                    return;

                foreach (var item in ItemList.Where(x => x.Graphic != 0))
                {
                    ushort id = PaperDollInteractable.GetAnimID(item.Graphic, item.ItemData.AnimID, true);

                    Add(new GumpPic(0, 0, id, item.Hue) { IsPartialHue = item.ItemData.IsPartialHue });
                }
            }
        }
    }
}
