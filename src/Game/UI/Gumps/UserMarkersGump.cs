using System;
using System.Collections.Generic;
using System.IO;
using ClassicUO.Utility;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;
using static ClassicUO.Game.UI.Gumps.WorldMapGump;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class UserMarkersGump : Gump
    {
        private readonly StbTextBox _textBoxX;
        private readonly StbTextBox _textBoxY;
        private readonly StbTextBox _markerName;

        private const ushort HUE_FONT = 0xFFFF;

        private readonly Combobox _colorsCombo;
        private readonly string[] _colors;

        private readonly List<WMapMarker> _markers;
        private readonly int _markerIdx;

        private const int MAX_CORD_LEN = 10;
        private const int MAX_NAME_LEN = 25;

        private const int MAP_MIN_CORD = 0;
        private readonly int _mapMaxX = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 0];
        private readonly int _mapMaxY = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 1];

        private readonly string _userMarkersFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", $"{USER_MARKERS_FILE}.usr");

        public event EventHandler EditEnd;

        private enum ButtonsOption
        {
            ADD_BTN,
            EDIT_BTN,
            CANCEL_BTN,
        }

        internal UserMarkersGump(int x, int y, List<WMapMarker> markers, string color = "red", bool isEdit = false, int markerIdx = -1) : base(0, 0)
        {
            CanMove = true;

            _markers = markers;
            _markerIdx = markerIdx;

            _colors = new [] { "red", "green", "blue", "purple", "black", "yellow", "white", "none" };

            var markerName = _markerIdx < 0 ? ResGumps.MarkerDefName : _markers[_markerIdx].Name;

            int selectedColor = Array.IndexOf(_colors, color);

            if (selectedColor < 0)
                selectedColor = 0;

            AlphaBlendControl markersGumpBackground = new AlphaBlendControl
            {
                Width = 300,
                Height = 200,
                X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125,
                Y = 150,
                Alpha = 0.2f,
                CanMove = true,
                CanCloseWithRightClick = true,
                AcceptMouseInput = true
            };

            Add(markersGumpBackground);

            if (!isEdit)
                Add(new Label(ResGumps.AddMarker, true, HUE_FONT, 0, 255, Renderer.FontStyle.BlackBorder)
                {
                    X = markersGumpBackground.X + 100,
                    Y = markersGumpBackground.Y + 3,
                });
            else
                Add(new Label(ResGumps.EditMarker, true, HUE_FONT, 0, 255, Renderer.FontStyle.BlackBorder)
                {
                    X = markersGumpBackground.X + 100,
                    Y = markersGumpBackground.Y + 3,
                });

            // X Field
            Add(new ResizePic(0x0BB8)
            {
                X = markersGumpBackground.X + 13,
                Y = markersGumpBackground.Y + 25,
                Width = 90,
                Height = 25
            });

            _textBoxX = new StbTextBox(
                0xFF,
                MAX_CORD_LEN,
                90,
                true,
                Renderer.FontStyle.BlackBorder | Renderer.FontStyle.Fixed
            )
            {
                X = markersGumpBackground.X + 20,
                Y = markersGumpBackground.Y + 25,
                Width = 90,
                Height = 25,
                Text = x.ToString()
            };
            Add(_textBoxX);

            // Y Field
            Add(new ResizePic(0x0BB8)
            {
                X = markersGumpBackground.X + 13,
                Y = markersGumpBackground.Y + 55,
                Width = 90,
                Height = 25
            });

            _textBoxY = new StbTextBox(
                0xFF,
                MAX_CORD_LEN,
                90,
                true,
                Renderer.FontStyle.BlackBorder | Renderer.FontStyle.Fixed
            )
            {
                X = markersGumpBackground.X + 20,
                Y = markersGumpBackground.Y + 55,
                Width = 90,
                Height = 25,
                Text = y.ToString()
            };
            Add(_textBoxY);

            // Marker Name field
            Add(new ResizePic(0x0BB8)
            {
                X = markersGumpBackground.X + 13,
                Y = markersGumpBackground.Y + 90,
                Width = 250,
                Height = 25
            });

            _markerName = new StbTextBox(
                0xFF,
                MAX_NAME_LEN,
                250,
                true,
                Renderer.FontStyle.BlackBorder | Renderer.FontStyle.Fixed
            )
            {
                X = markersGumpBackground.X + 13,
                Y = markersGumpBackground.Y + 90,
                Width = 250,
                Height = 25,
                Text = markerName
            };
            Add(_markerName);

            // Color Combobox
            _colorsCombo = new Combobox
                (
                    markersGumpBackground.X + 13,
                    _markerName.Y + 30,
                    250,
                    _colors,
                    selectedColor
                );
            Add(_colorsCombo);

            // Buttons Add and Edit depend of state
            if (!isEdit)
            {
                Add
                (
                    new NiceButton
                        (
                            markersGumpBackground.X + 13,
                            markersGumpBackground.Y + markersGumpBackground.Height - 30,
                            60,
                            25,
                            ButtonAction.Activate,
                            ResGumps.CreateMarker
                        )
                    { ButtonParameter = (int)ButtonsOption.ADD_BTN, IsSelectable = false }
                );
            }
            else
            {
                Add
                (
                    new NiceButton
                        (
                            markersGumpBackground.X + 13,
                            markersGumpBackground.Y + markersGumpBackground.Height - 30,
                            60,
                            25,
                            ButtonAction.Activate,
                            ResGumps.Edit
                        )
                    { ButtonParameter = (int)ButtonsOption.EDIT_BTN, IsSelectable = false }
                );
            }

            Add
            (
                new NiceButton
                    (
                        markersGumpBackground.X + 78,
                        markersGumpBackground.Y + markersGumpBackground.Height - 30,
                        60,
                        25,
                        ButtonAction.Activate,
                        ResGumps.Cancel
                    )
                { ButtonParameter = (int)ButtonsOption.CANCEL_BTN, IsSelectable = false }
            );

            SetInScreen();
        }

        private void EditMarker()
        {
            var editedMarker = PrepareMarker();
            if (editedMarker == null)
                return;

            _markers[_markerIdx] = editedMarker;

            EditEnd.Raise(editedMarker);

            Dispose();
        }

        private void AddNewMarker()
        {
            if (!File.Exists(_userMarkersFilePath))
            {
                return;
            }

            var newMarker = PrepareMarker();
            if (newMarker == null)
            {
                return;
            }

            var newLine = $"{newMarker.X},{newMarker.Y},{newMarker.MapId},{newMarker.Name},lol.png,{newMarker.ColorName},4\r";

            File.AppendAllText(_userMarkersFilePath, newLine);

            _markers.Add(newMarker);

            Dispose();
        }

        private WMapMarker PrepareMarker()
        {
            if (!int.TryParse(_textBoxX.Text, out var x))
                return null;
            if (!int.TryParse(_textBoxY.Text, out var y))
                return null;

            // Validate User Enter Data
            if(x > _mapMaxX || x < MAP_MIN_CORD)
            {
                return null;
            }

            if (y > _mapMaxY || y < MAP_MIN_CORD)
            {
                return null;
            }

            var mapIdx = World.MapIndex;
            var markerName = _markerName.Text;
            var color = _colors[_colorsCombo.SelectedIndex];

            return new WMapMarker(color)
            {
                Name = markerName,
                X = x,
                Y = y,
                MapId = mapIdx,
                ColorName = color
            };
        }

        public override void OnButtonClick(int buttonId)
        {
            switch (buttonId)
            {
                case (int)ButtonsOption.ADD_BTN:
                    AddNewMarker();
                    break;
                case (int)ButtonsOption.EDIT_BTN:
                    EditMarker();
                    break;
                case (int)ButtonsOption.CANCEL_BTN:
                    Dispose();
                    break;
            }
        }
    }
}
