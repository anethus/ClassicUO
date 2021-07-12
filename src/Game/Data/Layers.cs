#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using ClassicUO.Game.GameObjects;
using System.Collections.Generic;

namespace ClassicUO.Game.Data
{
    internal enum Layer : byte
    {
        Invalid = 0x00,
        OneHanded = 0x01,
        TwoHanded = 0x02,
        Shoes = 0x03,
        Pants = 0x04,
        Shirt = 0x05,
        Helmet = 0x06,
        Gloves = 0x07,
        Ring = 0x08,
        Talisman = 0x09,
        Necklace = 0x0A,
        Hair = 0x0B,
        Waist = 0x0C,
        Torso = 0x0D,
        Bracelet = 0x0E,
        Face = 0x0F,
        Beard = 0x10,
        Tunic = 0x11,
        Earrings = 0x12,
        Arms = 0x13,
        Cloak = 0x14,
        Backpack = 0x15,
        Robe = 0x16,
        Skirt = 0x17,
        Legs = 0x18,
        Mount = 0x19,
        ShopBuyRestock = 0x1A,
        ShopBuy = 0x1B,
        ShopSell = 0x1C,
        Bank = 0x1D
    }

    internal static class Layers
    {
        private static Layer[][] LayerSortByDirection { get; } = new Layer[8][]
        {
            new Layer[]
            {
                // 0
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Helmet,
                Layer.TwoHanded, Layer.Cloak
            },
            new Layer[]
            {
                // 1
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new Layer[]
            {
                // 2
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new Layer[]
            {
                // 3
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe, Layer.Waist,
                Layer.Necklace, Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded
            },
            new Layer[]
            {
                // 4
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new Layer[]
            {
                // 5
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new Layer[]
            {
                // 6
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new Layer[]
            {
                // 7
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            }
        };

        private enum PaperDollSortType
        {
            STANDARD = 0,
            QUIVER = 1,
            PLATEARMS = 2,
            PLATEARMS_AND_QUIVER = 3
        };

        private static Layer[][] LayerSortPaperdoll { get; } = new Layer[4][]
        {
            new Layer[]
            {
                // Standard sort
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new Layer[]
            {
                // Quiver
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Cloak, Layer.Waist,
                Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new Layer[]
            {
                // Plate Arms (Torso and Arms Swapped)
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Arms , Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new Layer[]
            {
                // Quiver and Plate Arms
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Arms, Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Cloak, Layer.Waist,
                Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
        };

        public static IEnumerable<Item> GetItemsOnMap(Entity entity, byte dir)
        {
            Layer[] layers = LayerSortByDirection[dir];

            for (int i = 0; i < layers.Length; i++)
            {
                Item item = entity.FindItemByLayer(layers[i]);

                if (item == null)
                {
                    continue;
                }

                yield return item;
            }
        }

        public static IEnumerable<Item> GetItemsOnPaperdoll(Mobile mobile)
        {
            PaperDollSortType sortType = PaperDollSortType.STANDARD;

            ushort graphic = 0;

            Item arms = mobile.FindItemByLayer(Layer.Arms);
            if (arms != null)
            {
                graphic = arms.Graphic;
            }
            else if (ItemHold.Enabled && !ItemHold.IsFixedPosition && (byte)Layer.Arms == ItemHold.ItemData.Layer)
            {
                graphic = ItemHold.Graphic;
            }

            if (graphic == 0x1410 || graphic == 0x1417)
            {
                sortType = PaperDollSortType.PLATEARMS;
            }

            Item cloak = mobile.FindItemByLayer(Layer.Cloak);

            if (cloak != null)
            {
                if (cloak.ItemData.IsContainer)
                {
                    sortType = sortType + 1;
                }
            }
            else if (ItemHold.Enabled && !ItemHold.IsFixedPosition && (byte)Layer.Cloak == ItemHold.ItemData.Layer)
            {
                if (ItemHold.ItemData.IsContainer)
                {
                    sortType = sortType + 1;
                }
            }

            Layer[] layers = LayerSortPaperdoll[(int)sortType];

            for (int i = 0; i < layers.Length; i++)
            {
                yield return mobile.FindItemByLayer(layers[i]);
            }
        }
    }
}