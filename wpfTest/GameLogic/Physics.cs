using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public class Physics
    {
        private float terrainAcc=500000000f;

        /// <summary>
        /// Pushing maps for the current map. If the map changes they are updated by the method UpdatePushingMaps.
        /// </summary>
        public Dictionary<Movement, PushingMap> PushingMaps { get; }

        public static Physics GetPhysics() => new Physics();
        private Physics()
        {
            PushingMaps = new Dictionary<Movement, PushingMap>();
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
            PushingMap gPMap = PushingMapGenerator.GeneratePushingMap(groundObst);
            PushingMap wPMap = PushingMapGenerator.GeneratePushingMap(waterObst);
            PushingMap gwPMap = PushingMapGenerator.GeneratePushingMap(bothObst);
            PushingMaps[Movement.LAND] = gPMap;
            PushingMaps[Movement.WATER] = wPMap;
            PushingMaps[Movement.LAND_WATER]= gwPMap;
        }

        public void PushAway(Map map, List<Animal> animals, List<Entity> entities, float deltaT)
        {
            foreach (Animal u in animals)
            {
                //animals that aren't physical don't need to check for collisions
                if (!u.Physical)
                    continue;

                foreach (Entity e in entities)
                {
                    //entities that aren't physical don't need to check for collisions
                    if (!e.Physical)
                        continue;

                    //calculate collisions for each pair of unit and entity only once
                    if (u.GetHashCode() < e.GetHashCode() || e is Building)
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
                            if (e is Building)
                            {
                                //buildings can't be pushed
                                u.Position = u.Center - 2 * pushVec;
                            }
                            else if (e is Corpse)
                            {
                                Corpse c = (Corpse)e;
                                u.Position = u.Center - pushVec;
                                c.Position = c.Center + pushVec;
                            }
                            else
                            {
                                Animal u1 = (Animal)e;
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
                                }
                            }
                        }
                    }
                }
            }
        }

        public void PushOutsideOfObstacles(Map map, List<Animal> animals, float deltaT)
        {

            ObstacleMap gOMap = map.ObstacleMaps[Movement.LAND];
            ObstacleMap wOMap = map.ObstacleMaps[Movement.WATER];
            ObstacleMap gwOMap = map.ObstacleMaps[Movement.LAND_WATER];
            if (map.MapWasChanged
                || PushingMaps[Movement.LAND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.LAND_WATER] == null)
                UpdatePushingMaps(map);
            PushingMap gPMap = PushingMaps[Movement.LAND];
            PushingMap wPMap = PushingMaps[Movement.WATER];
            PushingMap gwPMap = PushingMaps[Movement.LAND_WATER];
            foreach (Animal u in animals)
            {
                //non-physical animals don't need to check for collisions
                if (!u.Physical)
                    continue;

                PushingMap pushingMap;
                ObstacleMap obstacleMap;
                switch (u.Movement)
                {
                    case Movement.LAND:
                        pushingMap = gPMap;
                        obstacleMap = gOMap;
                        break;
                    case Movement.WATER:
                        pushingMap = wPMap;
                        obstacleMap = wOMap;
                        break;
                    default://Movement.LAND_WATER
                        pushingMap = gwPMap;
                        obstacleMap = gwOMap;
                        break;
                }
                u.Accelerate(
                            deltaT * pushingMap.GetIntensity(u.Center, terrainAcc), map
                            );
                /*Vector2 intensity = pushingMap.GetIntensity(u.Center, 1f);
                float t = (float)Math.Min((Math.Ceiling(u.Position.X) - u.Position.X) / intensity.X,
                    (Math.Ceiling(u.Position.Y) - u.Position.Y) / intensity.Y);
                if(!float.IsNaN(t) && !float.IsInfinity(t))
                    u.Position += t * intensity;*/
                if (obstacleMap.CollidingWithObstacle(u.Center))
                    u.IsInCollision = true;
            }
        }

        public void ResetCollision(List<Animal> units)
        {
            foreach (Animal u in units)
            {
                if (!u.WantsToMove && !u.IsInCollision)
                    u.Velocity = new Vector2(0f, 0f);
                u.IsInCollision = false;
            }
        }

        public void Step(Map map, List<Animal> units, float deltaT)
        {
            foreach(Animal u in units)
            {
                u.Move(map,deltaT);
            }
        }
    }
}
