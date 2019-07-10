using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    class Pathfinding
    {
        private static Pathfinding pathfinding;
        public static Pathfinding GetPathfinding => pathfinding;

        static Pathfinding()
        {
            pathfinding = new Pathfinding();
        }

        private Pathfinding() { }

        public FlowMap GenerateFlowMap(ObstacleMap obst, List<Unit> units, Vector2 targetLocation)
        {
            FlowMap flMap = new FlowMap(obst.Width, obst.Height);

            throw new NotImplementedException();
        }
    }
}
