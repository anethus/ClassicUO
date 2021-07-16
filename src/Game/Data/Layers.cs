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
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassicUO.Game.Data
{
    internal static class Layer
    {
        public const byte Invalid = 0x00;
        public const byte OneHanded = 0x01;
        public const byte TwoHanded = 0x02;
        public const byte Shoes = 0x03;
        public const byte Pants = 0x04;
        public const byte Shirt = 0x05;
        public const byte Helmet = 0x06;
        public const byte Gloves = 0x07;
        public const byte Ring = 0x08;
        public const byte Talisman = 0x09;
        public const byte Necklace = 0x0A;
        public const byte Hair = 0x0B;
        public const byte Waist = 0x0C;
        public const byte Torso = 0x0D;
        public const byte Bracelet = 0x0E;
        public const byte Face = 0x0F;
        public const byte Beard = 0x10;
        public const byte Tunic = 0x11;
        public const byte Earrings = 0x12;
        public const byte Arms = 0x13;
        public const byte Cloak = 0x14;
        public const byte Backpack = 0x15;
        public const byte Robe = 0x16;
        public const byte Skirt = 0x17;
        public const byte Legs = 0x18;
        public const byte Mount = 0x19;
        public const byte ShopBuyRestock = 0x1A;
        public const byte ShopBuy = 0x1B;
        public const byte ShopSell = 0x1C;
        public const byte Bank = 0x1D;
    }

    internal static class Layers
    {
        public static void Load()
        {
            string path = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string layersFile = Path.Combine(path, "layers.txt");
            Dictionary<string, byte> layers = new Dictionary<string, byte>();

            if (File.Exists(layersFile))
            {
                TextFileParser parser = new TextFileParser(File.ReadAllText(layersFile), new[] { ' ', '\t', ',' }, new[] { '#', ';' }, new[] { '"', '"' });

                while (!parser.IsEOF())
                {
                    var tokens = parser.ReadTokens();

                    if (tokens == null)
                    {
                        continue;
                    }

                    byte layer = byte.Parse(tokens[0]);
                    string fullName = tokens[1];
                    string shortName = tokens[2];

                    if (layers.TryGetValue(shortName, out var num))
                    {
                        Log.Error($"Duplicate layer in layers.txt: {shortName}");
                        continue;
                    }

                    layers[shortName] = layer;
                }
            }

            string layerOrderFile = Path.Combine(path, "layerOrder.txt");

            if (File.Exists(layerOrderFile))
            {
                // 8 directions + paperdoll
                List<byte>[] layerSet = new List<byte>[9];

                TextFileParser parser = new TextFileParser(File.ReadAllText(layerOrderFile), new[] { ' ', '\t', ',' }, new[] { '#', ';' }, new[] { '"', '"' });

                int idx = 0;
                while (!parser.IsEOF() && idx < 9)
                {
                    var tokens = parser.ReadTokens();

                    if (tokens == null)
                    {
                        continue;
                    }

                    List<byte> ordering = new List<byte>();

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        ordering.Add(layers[tokens[i]]);
                    }

                    layerSet[idx] = ordering;

                    idx++;
                }

                for (int i = 0; i < 8; i++)
                {
                    LayerSortByDirection[i] = layerSet[i].ToArray();
                }

                LayerSortPaperdoll[0] = layerSet[8].ToArray();

                // Quiver version
                var quiverOrdering = new List<byte>(layerSet[8]);
                quiverOrdering.Remove((byte)Layer.Cloak);
                quiverOrdering.Insert(0, (byte)Layer.Cloak);
                LayerSortPaperdoll[1] = quiverOrdering.ToArray();


                // Plate arms
                var plateArmsOrdering = new List<byte>(layerSet[8]);
                for (int i = 0; i < plateArmsOrdering.Count; i++)
                {
                    if (plateArmsOrdering[i] == (byte)Layer.Arms)
                    {
                        plateArmsOrdering[i] = (byte)Layer.Torso;
                    }
                    else if (plateArmsOrdering[i] == (byte)Layer.Torso)
                    {
                        plateArmsOrdering[i] = (byte)Layer.Arms;
                    }
                }
                LayerSortPaperdoll[2] = plateArmsOrdering.ToArray();

                // Quiver and plate arms
                plateArmsOrdering.Remove((byte)Layer.Cloak);
                plateArmsOrdering.Insert(0, (byte)Layer.Cloak);
                LayerSortPaperdoll[3] = plateArmsOrdering.ToArray();
            }
        }

        private static byte[][] LayerSortByDirection { get; } = new byte[8][]
        {
            new byte[]
            {
                // 0
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Helmet,
                Layer.TwoHanded, Layer.Cloak
            },
            new byte[]
            {
                // 1
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new byte[]
            {
                // 2
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new byte[]
            {
                // 3
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe, Layer.Waist,
                Layer.Necklace, Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded
            },
            new byte[]
            {
                // 4
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new byte[]
            {
                // 5
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new byte[]
            {
                // 6
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Ring, Layer.Talisman,
                Layer.Bracelet, Layer.Face, Layer.Arms, Layer.Gloves, Layer.Skirt, Layer.Tunic, Layer.Robe,
                Layer.Necklace, Layer.Hair, Layer.Waist, Layer.Beard, Layer.Earrings, Layer.OneHanded, Layer.Cloak,
                Layer.Helmet, Layer.TwoHanded
            },
            new byte[]
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

        private static byte[][] LayerSortPaperdoll { get; } = new byte[4][]
        {
            new byte[]
            {
                // Standard sort
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new byte[]
            {
                // Quiver
                Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Cloak, Layer.Waist,
                Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new byte[]
            {
                // Plate Arms (Torso and Arms Swapped)
                Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Torso, Layer.Arms , Layer.Tunic,
                Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
                Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
            },
            new byte[]
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
            byte[] layers = LayerSortByDirection[dir];

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
            else if (ItemHold.Enabled && !ItemHold.IsFixedPosition && Layer.Arms == ItemHold.ItemData.Layer)
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
            else if (ItemHold.Enabled && !ItemHold.IsFixedPosition && Layer.Cloak == ItemHold.ItemData.Layer)
            {
                if (ItemHold.ItemData.IsContainer)
                {
                    sortType = sortType + 1;
                }
            }

            byte[] layers = LayerSortPaperdoll[(int)sortType];

            for (int i = 0; i < layers.Length; i++)
            {
                Item item = mobile.FindItemByLayer(layers[i]);

                if (item != null)
                {
                    yield return item;
                }
                else if (ItemHold.Enabled && !ItemHold.IsFixedPosition && layers[i] == ItemHold.ItemData.Layer && ItemHold.ItemData.AnimID != 0)
                {
                    yield return null;
                }
            }
        }

        public static IEnumerable<Item> GetItemsOnVendor(Mobile mobile)
        {
            Item item;

            item = mobile.FindItemByLayer(Layer.ShopBuyRestock);

            if (item != null)
            {
                yield return item;
            }

            item = mobile.FindItemByLayer(Layer.ShopBuy);

            if (item != null)
            {
                yield return item;
            }

            item = mobile.FindItemByLayer(Layer.ShopSell);

            if (item != null)
            {
                yield return item;
            }
        }

        public static bool IsHiddenLayer(byte layer)
        {
            // Note that this doesn't include the backpack layer, which technically is
            // visible on the paperdoll
            switch (layer)
            {
                case Layer.Invalid:
                case Layer.Mount:
                case Layer.ShopBuyRestock:
                case Layer.ShopBuy:
                case Layer.ShopSell:
                case Layer.Bank:
                    return true;
            }

            return false;
        }
    }
}