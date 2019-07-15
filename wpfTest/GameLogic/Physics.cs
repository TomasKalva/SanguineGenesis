using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class Physics
    {
        private float terrainAcc=5000000f;
        private float unitAcc = 20f;

        public static Physics GetPhysics() => new Physics();

        private Physics() { }

        public void Repulse(Map map, List<Unit> units, float deltaT)
        {
            foreach (Unit u1 in units)
                foreach(Unit u2 in units)
                {
                    //accelerate only distinct units and calculate acceleration for each
                    //pair of units only once
                    if (u1.GetHashCode() < u2.GetHashCode())
                    {

                        float dist = map.Distance(u1, u2);
                        //if two units get stuck on top of each other, move them apart
                        if (dist == 0)
                        {
                            Vector2 epsilon = new Vector2(0.1f, 0.1f);
                            u1.Pos = u1.Pos + epsilon;
                            dist = epsilon.Length;
                        }
                        float totalR = u1.Range + u2.Range;
                        //check if the units u1 and u2 are in collision
                        if (dist < totalR && dist != 0)
                        {
                            //push centres of the units from each other
                            Vector2 dir12 = u1.Pos.UnitDirectionTo(u2.Pos);
                            u1.Accelerate((-unitAcc * deltaT) * dir12);
                            u2.Accelerate((unitAcc * deltaT) * dir12);
                            u1.IsInCollision = true;
                            u2.IsInCollision = true;
                        }
                    }
                }

        }

        public void PushOutsideOfObstacles(Map map, List<Unit> units, float deltaT)
        {
            ObstacleMap obstacleMap = map.GetObstacleMap();
            FlowMap pMap=PushingMapGenerator.GeneratePushingMap(obstacleMap);
            foreach (Unit u in units)
            {
                u.Accelerate(
                    deltaT * pMap.GetIntensity(u.Pos, terrainAcc)
                    );
                if (obstacleMap.CollidingWithObstacle(u.Pos))
                    u.IsInCollision = true;
            }
        }

        public void ResetCollision(List<Unit> units)
        {
            foreach (Unit u in units)
            {
                if (!u.WantsToMove && !u.IsInCollision)
                    u.Vel = new Vector2(0f, 0f);
                u.IsInCollision = false;
            }
        }

        public void Step(Map map, List<Unit> units, float deltaT)
        {
            foreach(Unit u in units)
            {
                u.Move(map,deltaT);
            }
        }
    }
}
