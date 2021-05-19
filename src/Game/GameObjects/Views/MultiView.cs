﻿#region license

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

using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using System.Collections.Generic;

namespace ClassicUO.Game.GameObjects
{
    internal partial class Multi
    {
        private int _canBeTransparent;
        public bool IsHousePreview;

        public override bool TransparentTest(int z)
        {
            bool r = true;

            if (Z <= z - ItemData.Height)
            {
                r = false;
            }
            else if (z < Z && (_canBeTransparent & 0xFF) == 0)
            {
                r = false;
            }

            return r;
        }

        public override bool Draw(UltimaBatcher2D batcher, int posX, int posY)
        {
            if (!AllowedToDraw || IsDestroyed)
            {
                return false;
            }

            ResetHueVector();

            ushort hue = Hue;

            if (State != 0)
            {
                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER) != 0)
                {
                    return false;
                }

                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0)
                {
                    hue = 0x002B;
                }

                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT) != 0)
                {
                    if (AlphaHue >= 192)
                    {
                        AlphaHue = 0xFF;
                    }
                    else
                    {
                        ProcessAlpha(192);
                    }
                }
            }

            ResetHueVector();

            ushort graphic = Graphic;

            if (CUOEnviroment.IsOutlands && ProfileManager.CurrentProfile.ReplaceSailsOption == 1 && _betterSailsGraph.ContainsKey(Graphic))
            {
                graphic = _betterSailsGraph[Graphic];
            }
            else if (CUOEnviroment.IsOutlands && ProfileManager.CurrentProfile.ReplaceSailsOption == 2 && _hiddenSailsGraph.ContainsKey(Graphic))
            {
                graphic = _hiddenSailsGraph[Graphic];
            }

            bool partial = ItemData.IsPartialHue;

            Profile currentProfile = ProfileManager.CurrentProfile;

            if (currentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
            {
                hue = Constants.HIGHLIGHT_CURRENT_OBJECT_HUE;
                partial = false;
            }
            else if (currentProfile.NoColorObjectsOutOfRange && Distance > World.ClientViewRange)
            {
                hue = Constants.OUT_RANGE_COLOR;
                partial = false;
            }
            else if (World.Player.IsDead && currentProfile.EnableBlackWhiteEffect)
            {
                hue = Constants.DEAD_RANGE_COLOR;
                partial = false;
            }

            ShaderHueTranslator.GetHueVector(ref HueVector, hue, partial, 0);

            //Engine.DebugInfo.MultiRendered++;

            if (IsHousePreview)
            {
                HueVector.Z = 0.5f;
            }

            posX += (int) Offset.X;
            posY += (int) (Offset.Y + Offset.Z);

            if (AlphaHue != 255)
            {
                HueVector.Z = 1f - AlphaHue / 255f;
            }

            DrawStaticAnimated
            (
                batcher,
                graphic,
                posX,
                posY,
                ref HueVector,
                ref DrawTransparent,
                false
            );

            if (ItemData.IsLight)
            {
                Client.Game.GetScene<GameScene>().AddLight(this, this, posX + 22, posY + 22);
            }

            if (!(SelectedObject.Object == this || IsHousePreview || FoliageIndex != -1 && Client.Game.GetScene<GameScene>().FoliageIndex == FoliageIndex))
            {
                if (State != 0)
                {
                    if ((State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW)) != 0)
                    {
                        return true;
                    }
                }

                if (DrawTransparent)
                {
                    return true;
                }

                ref UOFileIndex index = ref ArtLoader.Instance.GetValidRefEntry(graphic + 0x4000);

                posX -= index.Width;
                posY -= index.Height;

                if (SelectedObject.IsPointInStatic(ArtLoader.Instance.GetTexture(graphic), posX, posY))
                {
                    SelectedObject.Object = this;
                }
            }

            return true;
        }

        private static readonly Dictionary<ushort, ushort> _hiddenSailsGraph = new Dictionary<ushort, ushort>
        {
            { 15959, 50235 },
            { 15962, 50236 },
            { 15980, 50237 },
            { 15986, 50238 },
            { 16072, 50239 },
            { 16075, 50240 },
            { 16093, 50241 },
            { 16098, 50242 },
        };

        private static readonly Dictionary<ushort, ushort> _betterSailsGraph = new Dictionary<ushort, ushort>
        {
            { 15935, 50200 },
            { 15936, 50201 },
            { 15937, 50202 },
            { 15938, 50203 },
            { 15959, 50204 },
            { 15960, 50205 },
            { 15961, 50206 },
            { 15962, 50207 },
            { 15963, 50208 },
            { 15964, 50209 },
            { 15978, 50210 },
            { 15979, 50211 },
            { 15980, 50212 },
            { 15981, 50213 },
            { 15982, 50214 },
            { 15984, 50215 },
            { 15985, 50216 },
            { 15986, 50217 },
            { 15987, 50218 },
            { 15988, 50219 },
            { 16072, 50220 },
            { 16073, 50221 },
            { 16074, 50222 },
            { 16075, 50223 },
            { 16076, 50224 },
            { 16092, 50225 },
            { 16093, 50226 },
            { 16094, 50227 },
            { 16095, 50228 },
            { 16096, 50229 },
            { 16097, 50230 },
            { 16098, 50231 },
            { 16099, 50232 },
            { 16100, 50233 },
            { 16101, 50234 },
        };
    }
}