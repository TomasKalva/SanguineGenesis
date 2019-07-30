using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    class Physics
    {
        private float terrainAcc=5000000f;

        /// <summary>
        /// Pushing maps for the current map. If the map changes they are updated by the method UpdatePushingMaps.
        /// </summary>
        public Dictionary<Movement, FlowMap> PushingMaps { get; }

        public static Physics GetPhysics() => new Physics();
        private Physics()
        {
            PushingMaps = new Dictionary<Movement, FlowMap>();
            PushingMaps.Add(Movement.GROUND, null);
            PushingMaps.Add(Movement.WATER, null);
            PushingMaps.Add(Movement.GROUND_WATER, null);
        }

        /// <summary>
        /// Updates PushingMaps. Should be called only when the map changes.
        /// </summary>
        /// <param name="map"></param>
        public void UpdatePushingMaps(Map map)
        {
            
            ObstacleMap groundObst = map.ObstacleMaps[Movement.GROUND];
            ObstacleMap waterObst = map.ObstacleMaps[Movement.WATER];
            ObstacleMap bothObst = map.ObstacleMaps[Movement.GROUND_WATER];
            FlowMap gPMap = PushingMapGenerator.GeneratePushingMap(groundObst);
            FlowMap wPMap = PushingMapGenerator.GeneratePushingMap(waterObst);
            FlowMap gwPMap = PushingMapGenerator.GeneratePushingMap(bothObst);
            PushingMaps[Movement.GROUND] = gPMap;
            PushingMaps[Movement.WATER] = wPMap;
            PushingMaps[Movement.GROUND_WATER]= gwPMap;
        }

        public void PushAway(Map map, List<Unit> units, float deltaT)
        {
            foreach (Unit u1 in units)
                foreach (Unit u2 in units)
                {
                    //accelerate only distinct units and calculate acceleration for each
                    //pair of units only once
                    if (u1.GetHashCode() < u2.GetHashCode())
                    {

                        float dist = (u1.Center - u2.Center).Length;
                        //if two units get stuck on top of each other, move them apart
                        if (dist == 0)
                        {
                            Vector2 epsilon = new Vector2(0.1f, 0.1f);
                            u1.Position = u1.Center + epsilon;
                            dist = epsilon.Length;
                        }
                        float totalR = u1.Range + u2.Range;
                        //check if the units u1 and u2 are in collision
                        if (dist < totalR && dist != 0)
                        {
                            //push centres of the units from each other
                            Vector2 dir12 = u1.Center.UnitDirectionTo(u2.Center);
                            Vector2 pushVec = (totalR - dist) / 2 * dir12;
                            if (u1.Player != u2.Player)
                            {
                                if (u1.WantsToMove && !u2.WantsToMove)
                                {
                                    u1.Position = u1.Center - 2 * pushVec;

                                }
                                else if (u2.WantsToMove && !u1.WantsToMove)
                                {
                                    u2.Position = u2.Center + 2 * pushVec;

                                }
                                else
                                {
                                    u1.Position = u1.Center - pushVec;
                                    u2.Position = u2.Center + pushVec;

                                }
                            }
                            else
                            {
                                if(u1.CanBeMoved && !u2.CanBeMoved)
                                {
                                    u1.Position = u1.Center - 2 * pushVec;

                                }
                                else if (u2.CanBeMoved && !u1.CanBeMoved)
                                {
                                    u2.Position = u2.Center + 2 * pushVec;
                                }
                                else
                                {
                                    u1.Position = u1.Center - pushVec;
                                    u2.Position = u2.Center + pushVec;
                                }
                                //u1.Accelerate((-unitAcc * deltaT) * dir12);
                                //u2.Accelerate((unitAcc * deltaT) * dir12);
                                //u1.IsInCollision = true;
                                //u2.IsInCollision = true;}
                            }
                        }
                    }
                }
        }

        public void PushOutsideOfObstacles(Map map, List<Unit> units, float deltaT)
        {

            ObstacleMap gOMap = map.ObstacleMaps[Movement.GROUND];
            ObstacleMap wOMap = map.ObstacleMaps[Movement.WATER];
            ObstacleMap gwOMap = map.ObstacleMaps[Movement.GROUND_WATER];
            if (map.MapWasChanged
                || PushingMaps[Movement.GROUND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.GROUND_WATER] == null)
                UpdatePushingMaps(map);
            FlowMap gPMap = PushingMaps[Movement.GROUND];
            FlowMap wPMap = PushingMaps[Movement.WATER];
            FlowMap gwPMap = PushingMaps[Movement.GROUND_WATER];
            foreach (Unit u in units)
            {
                switch (u.Movement)
                {
                    case Movement.GROUND:
                        u.Accelerate(
                            deltaT * gPMap.GetIntensity(u.Center, terrainAcc)
                            );
                        if (gOMap.CollidingWithObstacle(u.Center))
                            u.IsInCollision = true;
                        break;
                    case Movement.WATER:
                        u.Accelerate(
                            deltaT * wPMap.GetIntensity(u.Center, terrainAcc)
                            );
                        if (wOMap.CollidingWithObstacle(u.Center))
                            u.IsInCollision = true;
                        break;
                    case Movement.GROUND_WATER:
                        u.Accelerate(
                            deltaT * gwPMap.GetIntensity(u.Center, terrainAcc)
                            );
                        if (gwOMap.CollidingWithObstacle(u.Center))
                            u.IsInCollision = true;
                        break;
                }
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
