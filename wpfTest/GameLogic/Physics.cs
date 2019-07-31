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
            PushingMaps.Add(Movement.LAND, null);
            PushingMaps.Add(Movement.WATER, null);
            PushingMaps.Add(Movement.LAND_WATER, null);
        }

        /// <summary>
        /// Updates PushingMaps. Should be called only when the map changes.
        /// </summary>
        /// <param name="map"></param>
        public void UpdatePushingMaps(Map map)
        {
            
            ObstacleMap groundObst = map.ObstacleMaps[Movement.LAND];
            ObstacleMap waterObst = map.ObstacleMaps[Movement.WATER];
            ObstacleMap bothObst = map.ObstacleMaps[Movement.LAND_WATER];
            FlowMap gPMap = PushingMapGenerator.GeneratePushingMap(groundObst);
            FlowMap wPMap = PushingMapGenerator.GeneratePushingMap(waterObst);
            FlowMap gwPMap = PushingMapGenerator.GeneratePushingMap(bothObst);
            PushingMaps[Movement.LAND] = gPMap;
            PushingMaps[Movement.WATER] = wPMap;
            PushingMaps[Movement.LAND_WATER]= gwPMap;
        }

        public void PushAway(Map map, List<Unit> units, List<Entity> entities, float deltaT)
        {
            foreach (Unit u in units)
                foreach (Entity e in entities)
                {
                    //calculate collisions for each pair of unit and entity only once
                    if (u.GetHashCode() < e.GetHashCode() || e.GetType()==typeof(Building))
                    {

                        float dist = (u.Center - e.Center).Length;
                        //if two units get stuck on top of each other, move them apart
                        if (dist == 0)
                        {
                            Vector2 epsilon = new Vector2(0.1f, 0.1f);
                            u.Position = u.Center + epsilon;
                            dist = epsilon.Length;
                        }
                        float totalR = u.Range + e.Range;
                        //check if u and e are in collision
                        if (dist < totalR && dist != 0)
                        {
                            //push centres of the units from each other
                            Vector2 dir12 = u.Center.UnitDirectionTo(e.Center);
                            Vector2 pushVec = (totalR - dist) / 2 * dir12;
                            if (e.GetType() == typeof(Building))
                            {
                                //buildings can't be pushed
                                u.Position = u.Center - 2 * pushVec;
                            }
                            else
                            {
                                Unit u1 = (Unit)e;
                                if (u.Player != u1.Player)
                                {
                                    if (u.WantsToMove && !u1.WantsToMove)
                                    {
                                        u.Position = u.Center - 2 * pushVec;

                                    }
                                    else if (u1.WantsToMove && !u.WantsToMove)
                                    {
                                        u1.Position = u1.Center + 2 * pushVec;

                                    }
                                    else
                                    {
                                        u.Position = u.Center - pushVec;
                                        u1.Position = u1.Center + pushVec;

                                    }
                                }
                                else
                                {
                                    if (u.CanBeMoved && !u1.CanBeMoved)
                                    {
                                        u.Position = u.Center - 2 * pushVec;

                                    }
                                    else if (u1.CanBeMoved && !u.CanBeMoved)
                                    {
                                        u1.Position = u1.Center + 2 * pushVec;
                                    }
                                    else
                                    {
                                        u.Position = u.Center - pushVec;
                                        u1.Position = u1.Center + pushVec;
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
        }

        public void PushOutsideOfObstacles(Map map, List<Unit> units, float deltaT)
        {

            ObstacleMap gOMap = map.ObstacleMaps[Movement.LAND];
            ObstacleMap wOMap = map.ObstacleMaps[Movement.WATER];
            ObstacleMap gwOMap = map.ObstacleMaps[Movement.LAND_WATER];
            if (map.MapWasChanged
                || PushingMaps[Movement.LAND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.LAND_WATER] == null)
                UpdatePushingMaps(map);
            FlowMap gPMap = PushingMaps[Movement.LAND];
            FlowMap wPMap = PushingMaps[Movement.WATER];
            FlowMap gwPMap = PushingMaps[Movement.LAND_WATER];
            foreach (Unit u in units)
            {
                switch (u.Movement)
                {
                    case Movement.LAND:
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
                    case Movement.LAND_WATER:
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
