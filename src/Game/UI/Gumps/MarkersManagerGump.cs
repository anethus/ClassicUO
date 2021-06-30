using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Resources;
using static ClassicUO.Game.UI.Gumps.WorldMapGump;

namespace ClassicUO.Game.UI.Gumps
{
    internal sealed class MarkersManagerGump : Gump
    {
        private const int WIDTH = 700;
        private const int HEIGHT = 500;
        private const ushort HUE_FONT = 0xFFFF;

        private bool _isMarkerListModified;

        private ScrollArea _scrollArea;

        private static List<WMapMarker> _markers = new List<WMapMarker>();

        private readonly string _userMarkersFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", $"{USER_MARKERS_FILE}.usr");

        internal MarkersManagerGump(): base(0, 0)
        {
            CanMove = true;

            LoadMarkers();

            Add
            (
                new AlphaBlendControl(0.05f)
                {
                    X = 1,
                    Y = 1,
                    Width = WIDTH - 2,
                    Height = HEIGHT - 2,
                    Hue = 999,
                    AcceptMouseInput = true,
                    CanCloseWithRightClick = true,
                    CanMove = true,
                }
            );

            #region Boarder
            Add
            (
                new Line
                (
                    0,
                    0,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    0,
                    0,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    0,
                    HEIGHT,
                    WIDTH,
                    1,
                    Color.Gray.PackedValue
                )
            );

            Add
            (
                new Line
                (
                    WIDTH,
                    0,
                    1,
                    HEIGHT,
                    Color.Gray.PackedValue
                )
            );
            #endregion

            var initY = 10;

            #region Legend

            Add(new Label(ResGumps.MarkerName, true, HUE_FONT, 185, 255, Renderer.FontStyle.BlackBorder) { X = 20, Y = initY });

            Add(new Label(ResGumps.MarkerX, true, HUE_FONT, 35, 255, Renderer.FontStyle.BlackBorder) { X = 295, Y = initY });

            Add(new Label(ResGumps.MarkerY, true, HUE_FONT, 35, 255, Renderer.FontStyle.BlackBorder) { X = 340, Y = initY });

            Add(new Label(ResGumps.MarkerColor, true, HUE_FONT, 35, 255, Renderer.FontStyle.BlackBorder) { X = 390, Y = initY });

            Add(new Label(ResGumps.Edit, true, HUE_FONT, 35, 255, Renderer.FontStyle.BlackBorder) { X = 460, Y = initY });

            Add(new Label(ResGumps.Remove, true, HUE_FONT, 40, 255, Renderer.FontStyle.BlackBorder) { X = 490, Y = initY });

            #endregion

            Add
            (
                new Line
                (
                    0,
                    initY + 20,
                    WIDTH - 10,
                    1,
                    Color.Gray.PackedValue
                )
            );

            DrawArea();

            SetInScreen();
        }

        private void DrawArea()
        {
            _scrollArea = new ScrollArea
            (
                10,
                40,
                WIDTH - 50,
                420,
                true
            );

            int i = 0;

            foreach (var marker in _markers.Select((value, idx) => new { idx, value }))
            {
                var newElement = new MakerManagerControl(marker.value, i, marker.idx);
                newElement.RemoveMarkerEvent += MarkerRemoveEventHandler;
                newElement.EditMarkerEvent += MarkerEditEventHandler;

                _scrollArea.Add(newElement);
                i += 25;
            }
            Add(_scrollArea);
        }

        private void MarkerRemoveEventHandler(object sender, EventArgs e)
        {
            if(sender is int idx)
            {
                _markers.RemoveAt(idx);
                // Clear area
                Remove(_scrollArea);
                //Redraw List
                DrawArea();
                //Mark list as Modified
                _isMarkerListModified = true;
            }
        }

        private void MarkerEditEventHandler(object sender, EventArgs e)
        {
            _isMarkerListModified = true;
        }

        public override void Dispose()
        {
            if (_isMarkerListModified)
            {
                using (StreamWriter writer = new StreamWriter(_userMarkersFilePath, false))
                {
                    foreach (var marker in _markers)
                    {
                        var newLine = $"{marker.X},{marker.Y},{marker.MapId},{marker.Name},lol.png,{marker.ColorName},4";

                        writer.WriteLine(newLine);
                    }
                }
                _isMarkerListModified = false;
                ReloadUserMarkers();
            }
            base.Dispose();
        }

        private void LoadMarkers()
        {
            if (!File.Exists(_userMarkersFilePath))
            {
                return;
            }

            _markers = LoadUserMarkers();
        }
        
        private sealed class MakerManagerControl : Control
        {
            private readonly WMapMarker _marker;
            private readonly int _y;
            private readonly int _idx;

            private Label _labelName;
            private Label _labelX;
            private Label _labelY;
            private Label _labelColor;

            public event EventHandler RemoveMarkerEvent;
            public event EventHandler EditMarkerEvent;

            private enum ButtonsOption
            {
                EDIT_MARKER_BTN,
                REMOVE_MARKER_BTN,
            }

            public MakerManagerControl(WMapMarker marker, int y, int idx)
            {
                CanMove = true;

                _idx = idx;
                _marker = marker;
                _y = y;

                DrawData();
            }

            private void DrawData()
            {
                _labelName = new Label($"{_marker.Name}", true, HUE_FONT, 280) { X = 10, Y = _y };
                Add(_labelName);

                _labelX = new Label($"{_marker.X}", true, HUE_FONT, 35) { X = 285, Y = _y };
                Add(_labelX);

                _labelY = new Label($"{_marker.Y}", true, HUE_FONT, 35) { X = 330, Y = _y };
                Add(_labelY);

                _labelColor = new Label($"{_marker.ColorName}", true, HUE_FONT, 35) { X = 390, Y = _y };
                Add(_labelColor);

                Add(
                    new Button((int)ButtonsOption.EDIT_MARKER_BTN, 0xFAB, 0xFAC)
                    {
                        X = 450,
                        Y = _y,
                        ButtonAction = ButtonAction.Activate,
                    }
                );

                Add(
                    new Button((int)ButtonsOption.REMOVE_MARKER_BTN, 0xFB1, 0xFB2)
                    {
                        X = 485,
                        Y = _y,
                        ButtonAction = ButtonAction.Activate,
                    }
                );
            }

            private void OnEditEnd(object sender, EventArgs e)
            {
                if(sender is WMapMarker editedMarker)
                {
                    _labelName.Text = editedMarker.Name;
                    _labelColor.Text = editedMarker.ColorName;
                    _labelX.Text = editedMarker.X.ToString();
                    _labelY.Text = editedMarker.Y.ToString();

                    EditMarkerEvent.Raise();
                }
            }

            public override void OnButtonClick(int buttonId)
            {
                switch (buttonId)
                {
                    case (int)ButtonsOption.EDIT_MARKER_BTN:
                        UserMarkersGump existingGump = UIManager.GetGump<UserMarkersGump>();

                        existingGump?.Dispose();

                        var editUserMarkerGump = new UserMarkersGump(_marker.X, _marker.Y, _markers, _marker.ColorName, true, _idx);
                        editUserMarkerGump.EditEnd += OnEditEnd;

                        UIManager.Add(editUserMarkerGump);
                            
                        break;
                    case (int)ButtonsOption.REMOVE_MARKER_BTN:
                        RemoveMarkerEvent.Raise(_idx);
                        break;
                }
            }
        }
    }

}
