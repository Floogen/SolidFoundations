using Microsoft.Xna.Framework;
using Netcode;
using SolidFoundations.Framework.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class GenericBuilding : Building
    {
        [XmlIgnore]
        public ExtendedBuildingModel Model { get; set; }
        public string Id { get { return base.modData.ContainsKey(ModDataKeys.GENERIC_BUILDING_ID) ? base.modData[ModDataKeys.GENERIC_BUILDING_ID] : String.Empty; } set { base.modData[ModDataKeys.GENERIC_BUILDING_ID] = value; } }
        public string LocationName { get; set; }

        private List<LightSource> LightSources { get; set; } = new List<LightSource>();

        // Start of backported properties
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingLocation
        [XmlIgnore]
        public NetLocationRef buildingLocation = new NetLocationRef();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingChests
        public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.animalDoorOpenAmount
        public readonly NetFloat animalDoorOpenAmount = new NetFloat();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._buildingMetadata
        [XmlIgnore]
        protected Dictionary<string, string> _buildingMetadata = new Dictionary<string, string>();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._lastHouseUpgradeLevel
        protected int _lastHouseUpgradeLevel = -1;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._chimneyPosition
        protected Vector2 _chimneyPosition = Vector2.Zero;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building._hasChimney
        protected bool? _hasChimney;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.chimneyTimer
        protected int chimneyTimer = 500;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.skinID
        public NetString skinID = new NetString();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.hasLoaded
        public bool hasLoaded;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.upgradeName
        public readonly NetString upgradeName = new NetString();

        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.nonInstancedIndoors
        public readonly NetLocationRef nonInstancedIndoors = new NetLocationRef();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.IndoorOrInstancedIndoor
        public GameLocation IndoorOrInstancedIndoor
        {
            get
            {
                if (this.indoors.Value != null)
                {
                    return this.indoors.Value;
                }
                return this.nonInstancedIndoors.Value;
            }
        }

        public GenericBuilding() : base()
        {

        }
    }
}
