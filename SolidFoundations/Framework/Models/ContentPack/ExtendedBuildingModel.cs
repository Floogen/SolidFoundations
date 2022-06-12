using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
using SolidFoundations.Framework.Models.ContentPack.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Models.ContentPack
{
    // TODO: When using SDV v1.6, this class should inherit StardewValley.GameData.BuildingData
    public class ExtendedBuildingModel : BuildingData
    {
        internal string Owner { get; set; }
        internal string PackName { get; set; }
        internal string PaintMaskTexture;

        [ContentSerializer(Optional = true)]
        public List<PaintMaskData> PaintMasks;

        [ContentSerializer(Optional = true)]
        public new List<ExtendedBuildingSkin> Skins = new List<ExtendedBuildingSkin>();

        [ContentSerializer(Optional = true)]
        public new List<ExtendedBuildingDrawLayer> DrawLayers;

        [ContentSerializer(Optional = true)]
        public new List<ExtendedBuildingItemConversion> ItemConversions;

        [ContentSerializer(Optional = true)]
        public new List<ExtendedBuildingActionTiles> ActionTiles = new List<ExtendedBuildingActionTiles>();
        protected Dictionary<Point, SpecialAction> _specialActionTiles;

        [ContentSerializer(Optional = true)]
        public List<ChestActionTile> LoadChestTiles;
        protected Dictionary<Point, string> _loadChestTiles;

        [ContentSerializer(Optional = true)]
        public List<ChestActionTile> CollectChestTiles;
        protected Dictionary<Point, string> _collectChestTiles;

        [ContentSerializer(Optional = true)]
        public List<ExtendedBuildingActionTiles> EventTiles = new List<ExtendedBuildingActionTiles>();
        protected Dictionary<Point, string> _eventTiles;
        protected Dictionary<Point, SpecialAction> _specialEventTiles;

        [ContentSerializer(Optional = true)]
        public List<Point> TunnelDoors = new List<Point>();

        [ContentSerializer(Optional = true)]
        public List<Point> AuxiliaryHumanDoors = new List<Point>();

        [ContentSerializer(Optional = true)]
        public SpecialAction DefaultSpecialAction { get; set; }

        [ContentSerializer(Optional = true)]
        public SpecialAction DefaultSpecialEventAction { get; set; }

        [ContentSerializer(Optional = true)]
        public List<InputFilter> InputFilters;

        public new string GetActionAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_actionTiles == null)
            {
                _actionTiles = new Dictionary<Point, string>();
                foreach (ExtendedBuildingActionTiles actionTile in ActionTiles)
                {
                    _actionTiles[actionTile.Tile] = actionTile.Action;
                }
            }

            string value = null;
            if (!_actionTiles.TryGetValue(key, out value))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
                value = DefaultAction;
            }
            return value;
        }

        public SpecialAction GetSpecialActionAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_specialActionTiles == null)
            {
                _specialActionTiles = new Dictionary<Point, SpecialAction>();
                foreach (ExtendedBuildingActionTiles actionTile in ActionTiles)
                {
                    _specialActionTiles[actionTile.Tile] = actionTile.SpecialAction;
                }
            }

            SpecialAction value = null;
            if (!_specialActionTiles.TryGetValue(key, out value))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
                value = DefaultSpecialAction;
            }
            return value;
        }

        public string GetLoadChestActionAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_loadChestTiles == null)
            {
                _loadChestTiles = new Dictionary<Point, string>();
                foreach (ChestActionTile chestTile in LoadChestTiles)
                {
                    _loadChestTiles[chestTile.Tile] = chestTile.Name;
                }
            }

            string chestName = null;
            if (!_loadChestTiles.TryGetValue(key, out chestName))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
            }
            return chestName;
        }

        public string GetCollectChestActionAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_collectChestTiles == null)
            {
                _collectChestTiles = new Dictionary<Point, string>();
                foreach (ChestActionTile chestTile in CollectChestTiles)
                {
                    _collectChestTiles[chestTile.Tile] = chestTile.Name;
                }
            }

            string chestName = null;
            if (!_collectChestTiles.TryGetValue(key, out chestName))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
            }
            return chestName;
        }

        public string GetEventAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_eventTiles == null)
            {
                _eventTiles = new Dictionary<Point, string>();
                foreach (ExtendedBuildingActionTiles eventTile in EventTiles)
                {
                    _eventTiles[eventTile.Tile] = eventTile.Action;
                }
            }

            string value = null;
            if (!_eventTiles.TryGetValue(key, out value))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
            }
            return value;
        }

        public SpecialAction GetSpecialEventAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_specialEventTiles == null)
            {
                _specialEventTiles = new Dictionary<Point, SpecialAction>();
                foreach (ExtendedBuildingActionTiles eventTile in EventTiles)
                {
                    _specialEventTiles[eventTile.Tile] = eventTile.SpecialAction;
                }
            }

            SpecialAction value = null;
            if (!_specialEventTiles.TryGetValue(key, out value))
            {
                if (relative_x < 0 || relative_x >= Size.X || relative_y < 0 || relative_y >= Size.Y)
                {
                    return null;
                }
                value = DefaultSpecialEventAction;
            }
            return value;
        }
    }
}
