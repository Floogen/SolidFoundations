using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidFoundations.Framework.Interfaces.Internal
{
    public interface IApi
    {
        public class BroadcastEventArgs : EventArgs
        {
            public string BuildingId { get; set; }
            public Building Building { get; set; }
            public Farmer Farmer { get; set; }
            public Point TriggerTile { get; set; }
            public string Message { get; set; }
        }

        public event EventHandler<BroadcastEventArgs> BroadcastSpecialActionTriggered;
    }

    public class Api : IApi
    {
        public event EventHandler<IApi.BroadcastEventArgs> BroadcastSpecialActionTriggered;

        internal void OnSpecialActionTriggered(IApi.BroadcastEventArgs e)
        {
            EventHandler<IApi.BroadcastEventArgs> handler = BroadcastSpecialActionTriggered;
            if (handler is not null)
            {
                handler(this, e);
            }
        }
    }
}
