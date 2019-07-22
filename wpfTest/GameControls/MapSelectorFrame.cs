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
    public class MapSelectorFrame:IEntity
    {   
        public Vector2 OriginalPoint { get; }
        public Vector2 EndPoint { get; private set; }

        //These values are relative to the size of one node.
        public float Bottom { get; private set; }
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Width => Right - Left;
        public float Height => Top - Bottom;


        public MapSelectorFrame(Vector2 originalPoint)
        {
            OriginalPoint = originalPoint;
            EndPoint = originalPoint;
        }

        public void SetEndPoint(Vector2 endPoint)
        {
            EndPoint = endPoint;
        }

        public void Update()
        {
            Left = Math.Min(OriginalPoint.X, EndPoint.X);
            Right = Math.Max(OriginalPoint.X, EndPoint.X);
            Bottom = Math.Min(OriginalPoint.Y, EndPoint.Y);
            Top = Math.Max(OriginalPoint.Y, EndPoint.Y);
        }

        public List<Unit> GetSelectedUnits(Game game)
        {
            return game.GameQuerying.SelectRectUnits(game,((IEntity)this).GetRect(),(unit)=>unit.Owner==game.CurrentPlayer.PlayerID);
        }
    }
}
