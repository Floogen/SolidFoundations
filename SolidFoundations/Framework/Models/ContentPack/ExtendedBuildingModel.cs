using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SolidFoundations.Framework.Models.Backport;
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
        // TODO: When using SDV v1.6, likely flag this as obsolete in favor of StardewValley.GameData.BuildingData.BuildCondition?
        public bool IsLocked { get; set; }

        internal string Owner { get; set; }
        internal string PackName { get; set; }

        [ContentSerializer(Optional = true)]
        public new List<ExtendedBuildingDrawLayer> DrawLayers;

        [ContentSerializer(Optional = true)]
        public List<BuildingActionTiles> EventTiles = new List<BuildingActionTiles>();

        protected Dictionary<Point, string> _eventTiles;

        public string GetEventAtTile(int relative_x, int relative_y)
        {
            Point key = new Point(relative_x, relative_y);
            if (_eventTiles == null)
            {
                _eventTiles = new Dictionary<Point, string>();
                foreach (BuildingActionTiles eventTile in EventTiles)
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
                value = DefaultAction;
            }
            return value;
        }
    }
}
