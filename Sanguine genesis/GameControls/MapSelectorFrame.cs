using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    /// <summary>
    /// Represents rectangle for selecting units.
    /// </summary>
    public class MapSelectorFrame:IRectangle
    {   
        /// <summary>
        /// The starting point of this frame. In map coordinates.
        /// </summary>
        public Vector2 OriginalPoint { get; }
        /// <summary>
        /// The last selected point of this frame. In map coordinates.
        /// </summary>
        public Vector2 EndPoint { get; private set; }
        
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
        
        public MapSelectorFrame(Vector2 originalPoint)
        {
            OriginalPoint = originalPoint;
            EndPoint = originalPoint;
        }

        /// <summary>
        /// Sets EndPoint.
        /// </summary>
        public void SetEndPoint(Vector2 endPoint)
        {
            EndPoint = endPoint;
        }

        /// <summary>
        /// Update the rectangle coordinates.
        /// </summary>
        public void Update()
        {
            Left = Math.Min(OriginalPoint.X, EndPoint.X);
            Right = Math.Max(OriginalPoint.X, EndPoint.X);
            Bottom = Math.Min(OriginalPoint.Y, EndPoint.Y);
            Top = Math.Max(OriginalPoint.Y, EndPoint.Y);
        }

        /// <summary>
        /// Returns all entities colliding with this MapSelectorFrame.
        /// </summary>
        public IEnumerable<Entity> GetSelectedUnits(Game game)
        {
            return game.GameQuerying.SelectRectEntities(game,((IRectangle)this).GetRect(),
                (unit)=>unit.Player.PlayerID==game.CurrentPlayer.PlayerID);
        }
    }
}
