﻿#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;

using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls
{
    internal class PaperDollInteractable : Control
    {
        private static readonly Layer[] _layerOrder =
        {
            Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
        };
        private Mobile _mobile;

        private readonly PaperDollGump _paperDollGump;
        private bool _updateUI;

        public PaperDollInteractable(int x, int y, Mobile mobile, PaperDollGump paperDollGump)
        {
            X = x;
            Y = y;
            _paperDollGump = paperDollGump;
            Mobile = mobile;
            AcceptMouseInput = false;

            _updateUI = true;
        }

        public Mobile Mobile
        {
            get => _mobile;
            set
            {
                if (value != _mobile)
                {
                    _mobile = value;
                    _updateUI = true;
                }
            }
        }

        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);

            if (_updateUI)
            {
                UpdateUI();

                _updateUI = false;
            }

            if (_mobile == null || _mobile.IsDestroyed)
                Dispose();
        }


        private bool _hasFakeItem;
        public void SetFakeItem(bool value)
        {
            _updateUI = (_hasFakeItem && !value) || (!_hasFakeItem && value);
            _hasFakeItem = value;
        }

        public bool HasFakeItem => _hasFakeItem;

        private void UpdateUI()
        {
            if (Mobile == null || Mobile.IsDestroyed)
            {
                Dispose();

                return;
            }

            Clear();

            // Add the base gump - the semi-naked paper doll.
            ushort body;

            if (Mobile.Graphic == 0x0191 || Mobile.Graphic == 0x0193)
                body = 0x000D;
            else if (Mobile.Graphic == 0x025D)
                body = 0x000E;
            else if (Mobile.Graphic == 0x025E)
                body = 0x000F;
            else if (Mobile.Graphic == 0x029A || Mobile.Graphic == 0x02B6)
                body = 0x029A;
            else if (Mobile.Graphic == 0x029B || Mobile.Graphic == 0x02B7)
                body = 0x0299;
            else if (Mobile.Graphic == 0x03DB)
            {
                body = 0x000C;
                Add(new GumpPic(0, 0, body, 0x03EA)
                {
                    AcceptMouseInput = true,
                    IsPartialHue = true
                });

                Add(new GumpPic(0, 0, 0xC72B, 0)
                {
                    AcceptMouseInput = true,
                    IsPartialHue = true
                });
            }
            else
                body = 0x000C;


            // body
            Add(new GumpPic(0, 0, body, _mobile.Hue));

            // equipment
            Item equipItem;
            for (int i = 0; i < _layerOrder.Length; i++)
            {
                Layer layer = _layerOrder[i];

                equipItem = _mobile.Equipment[(int) layer];

                if (equipItem != null)
                {
                    if (Mobile.IsCovered(_mobile, layer))
                        continue;

                    ushort id = GetAnimID(_mobile.Graphic, equipItem.ItemData.AnimID, !_mobile.IsMale);

                    Add(new GumpPicEquipment(_mobile.Serial, equipItem.Serial, 0, 0, id, (ushort) (equipItem.Hue & 0x3FFF), layer)
                    {
                        AcceptMouseInput = true,
                        IsPartialHue = equipItem.ItemData.IsPartialHue,
                        CanLift = World.InGame && 
                                  !World.Player.IsDead && layer != Layer.Beard && layer != Layer.Hair && 
                                  (_paperDollGump.CanLift || LocalSerial == World.Player)
                    });
                }
                else if (HasFakeItem && ItemHold.Enabled && (byte) layer == ItemHold.ItemData.Layer && ItemHold.ItemData.AnimID != 0)
                {
                    ushort id = GetAnimID(_mobile.Graphic, ItemHold.ItemData.AnimID, !_mobile.IsMale);
                    Add(new GumpPicEquipment(_mobile.Serial, 0, 0, 0, id, (ushort) (ItemHold.Hue & 0x3FFF), ItemHold.Layer)
                    {
                        AcceptMouseInput = true,
                        IsPartialHue = ItemHold.IsPartialHue,
                        Alpha = 0.7f
                    });
                }
            }


            equipItem = _mobile.Equipment[(int) Layer.Backpack];

            if (equipItem != null && equipItem.ItemData.AnimID != 0)
            {
                ushort backpackGraphic = (ushort) (equipItem.ItemData.AnimID + 50000);

                int bx = 0;

                if (World.ClientFeatures.PaperdollBooks)
                {
                    bx = 6;
                }

                Add(new GumpPicEquipment(_mobile.Serial, equipItem.Serial ,- bx, 0, backpackGraphic, (ushort) (equipItem.Hue & 0x3FFF), Layer.Backpack)
                {
                    AcceptMouseInput = true,
                });
            }
        }

        public void Update() => _updateUI = true;



        private static ushort GetAnimID(ushort graphic, ushort animID, bool isfemale)
        {  
            const int MALE_OFFSET = 50000;
            const int FEMALE_OFFSET = 60000;

            int offset = isfemale ? FEMALE_OFFSET : MALE_OFFSET;

            if (Client.Version >= ClientVersion.CV_7000 &&
                animID == 0x03CA       // graphic for dead shroud
                && (graphic == 0x02B7 || graphic == 0x02B6)) // dead gargoyle graphics
            {
                animID = 0x0223;
            }

            AnimationsLoader.Instance.ConvertBodyIfNeeded(ref graphic);

            if (AnimationsLoader.Instance.EquipConversions.TryGetValue(graphic, out var dict))
            {
                if (dict.TryGetValue(animID, out EquipConvData data))
                {
                    if (data.Gump > MALE_OFFSET)
                        animID = (ushort) (data.Gump >= FEMALE_OFFSET ? data.Gump - FEMALE_OFFSET : data.Gump - MALE_OFFSET);
                    else
                        animID = data.Gump;
                }
            }

            animID = (ushort) (animID + offset);

            if (isfemale && GumpsLoader.Instance.GetTexture(animID) == null)
                animID = (ushort) (animID + MALE_OFFSET);

            return animID;
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);
        }

        private class GumpPicEquipment : GumpPic
        {
            private readonly Layer _layer;
            private readonly uint _parentSerial;

            public GumpPicEquipment(uint parent, uint serial, int x, int y, ushort graphic, ushort hue, Layer layer) : base(x, y, graphic, hue)
            {
                _parentSerial = parent;
                LocalSerial = serial;
                CanMove = false;
                _layer = layer;

                if (SerialHelper.IsValid(serial))
                    SetTooltip(serial);
            }

            public bool CanLift { get; set; }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                GameActions.DoubleClick(LocalSerial);

                return true;
            }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                Mobile container = World.Mobiles.Get(_parentSerial);

                if (MouseIsOver)
                {
                    if (ItemHold.Enabled || LocalSerial == 0)
                    {
                        bool canwear = !(_layer != Layer.Backpack &&
                                         _parentSerial != World.Player &&
                                         container != null && container.Distance >= Constants.DRAG_ITEMS_DISTANCE);

                        if (canwear && container != null)
                        {
                            GameScene scene = Client.Game.GetScene<GameScene>();
                            if (scene == null)
                                return;

                            if (_layer == Layer.Backpack)
                            {
                                Item equipment = container.FindItemByLayer(_layer);

                                if (equipment != null)
                                {
                                    if (_layer == Layer.Backpack)
                                    {
                                        scene.DropHeldItemToContainer(LocalSerial != World.Player.Serial ? container.Serial : equipment.Serial);
                                    }

                                    Mouse.CancelDoubleClick = true;
                                    Mouse.LastLeftButtonClickTime = 0;
                                    return;
                                }
                            }
                            else if (ItemHold.ItemData.IsWearable)
                            {
                                Item equipment = container.FindItemByLayer(ItemHold.Layer);

                                if (equipment == null)
                                {
                                    scene.WearHeldItem(_parentSerial != World.Player ? container : World.Player);
                                    Mouse.CancelDoubleClick = true;
                                    Mouse.LastLeftButtonClickTime = 0;
                                    return;
                                }

                            }
                        }
                        else
                        {
                            Client.Game.Scene.Audio.PlayMusic(0x0051);
                        }
                    }
                }

                if (!ItemHold.Enabled && container != null)
                {
                    Item equipment = container.FindItemByLayer(_layer);

                    if (equipment != null)
                    {
                        if (TargetManager.IsTargeting)
                        {
                            TargetManager.Target(equipment.Serial);
                            Mouse.CancelDoubleClick = true;
                            Mouse.LastLeftButtonClickTime = 0;
                        }
                        else
                        {
                            if (!DelayedObjectClickManager.IsEnabled)
                            {
                                DelayedObjectClickManager.Set(LocalSerial,
                                                              Mouse.Position.X - ScreenCoordinateX,
                                                              Mouse.Position.Y - ScreenCoordinateY,
                                                              Time.Ticks + Mouse.MOUSE_DELAY_DOUBLE_CLICK);
                            }
                        }
                    }
                }
            }

            protected override void OnDragBegin(int x, int y)
            {
                if (Mouse.LButtonPressed && Mouse.IsDragging && CanLift && !ItemHold.Enabled)
                {
                    Point offset = Mouse.LDroppedOffset;

                    if (offset != Point.Zero)
                    {
                        Rectangle bounds = ArtLoader.Instance.GetTexture(Graphic)?.Bounds ?? Rectangle.Empty;
                        int centerX = bounds.Width >> 1;
                        int centerY = bounds.Height >> 1;
                        GameActions.PickUp(LocalSerial, centerX, centerY);
                    }
                }
            }

        }
    }
}