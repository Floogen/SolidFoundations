using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SolidFoundations.Framework.Models.ContentPack
{
    public class GenericBuilding
    {
        [XmlIgnore]
        public ExtendedBuildingModel Model { get; set; }
        public string Id { get; set; }
        public string LocationName { get; set; }

        private List<LightSource> LightSources { get; set; } = new List<LightSource>();

        // Start of backported properties
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingLocation
        [XmlIgnore]
        public string buildingLocation;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.buildingChests

        [XmlElement("buildingChests")]
        public new List<Chest> buildingChests = new List<Chest>();
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.animalDoorOpenAmount
        public new readonly float animalDoorOpenAmount;
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
        protected new int chimneyTimer = 500;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.skinID
        public NetString skinID;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.hasLoaded
        public new bool hasLoaded;
        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.upgradeName
        public new readonly string upgradeName;

        public GameLocation? indoors { get; set; }
        public int tileX { get; set; }
        public int tileY { get; set; }

        // TODO: When updated to SDV v1.6, this property should be deleted in favor of using the native StardewValley.Buildings.Building.nonInstancedIndoors
        public readonly string nonInstancedIndoors;

        public GenericBuilding() : base()
        {

        }
    }
}
