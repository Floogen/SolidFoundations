﻿using StardewValley.GameData.Buildings;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class ExtendedBuildingChest : BuildingChest
    {
        // Required for handling SDV v1.5 SF packs
        public string Name { get { return string.IsNullOrEmpty(base.Id) ? _name : base.Id; } set { base.Id = value; } }
        public string _name;

        public int Capacity { get { return _capacity <= 0 ? 1 : _capacity; } set { _capacity = value; } }
        protected int _capacity = 36;
    }
}
