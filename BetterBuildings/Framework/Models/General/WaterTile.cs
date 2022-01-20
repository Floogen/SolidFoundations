﻿using BetterBuildings.Framework.Models.ContentPack;
using BetterBuildings.Framework.Models.Events;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class WaterTile
    {
        public bool IsLava { get; set; }
        internal Color ActualColor { get; set; } = Microsoft.Xna.Framework.Color.White;
        public int[] Color { set { ActualColor = GetColor(value); } }
        public TileLocation Tile { get; set; }
        public Grid Grid { get; set; }

        private int GetColorIndex(int[] colorArray, int position)
        {
            if (position >= colorArray.Length)
            {
                return 255;
            }

            return colorArray[position];
        }

        private Color GetColor(int[] colorArray)
        {
            return new Color(GetColorIndex(colorArray, 0), GetColorIndex(colorArray, 1), GetColorIndex(colorArray, 2));
        }

        internal bool IsValid()
        {
            return Tile is not null || Grid is not null;
        }
    }
}