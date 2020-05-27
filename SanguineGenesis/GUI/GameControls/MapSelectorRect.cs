using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanguineGenesis.GameControls
{
    /// <summary>
    /// Represents rectangle for selecting units.
    /// </summary>
    class MapSelectorRect:IRectangle
    {   
        /// <summary>
        /// The starting point of this rectangle. In map coordinates.
        /// </summary>
        private Vector2 StartPoint { get; }
        private Vector2 endPoint;
        /// <summary>
        /// The last selected point of this rectangle. In map coordinates.
        /// </summary>
        public Vector2 EndPoint { get => endPoint; set { endPoint = value; Update(); } }
        
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Bottom { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Left { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Right { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Top { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Width => Right - Left;
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Height => Top - Bottom;
        
        public MapSelectorRect(Vector2 originalPoint)
        {
            StartPoint = originalPoint;
            EndPoint = originalPoint;
        }

        /// <summary>
        /// Update the rectangle coordinates.
        /// </summary>
        private void Update()
        {
            Left = Math.Min(StartPoint.X, EndPoint.X);
            Right = Math.Max(StartPoint.X, EndPoint.X);
            Bottom = Math.Min(StartPoint.Y, EndPoint.Y);
            Top = Math.Max(StartPoint.Y, EndPoint.Y);
        }

        /// <summary>
        /// Returns all entities of current player colliding with this MapSelectorRect.
        /// </summary>
        public IEnumerable<Entity> GetSelectedEntities(Game game)
        {
            return GameQuerying.SelectEntitiesInArea(game,((IRectangle)this).GetRect())
                .Where((entity)=>entity.Faction.FactionID==game.CurrentPlayer.FactionID);
        }
    }
}
