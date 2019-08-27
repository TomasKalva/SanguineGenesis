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
            SetPushMaps(map);



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
                                //u.Position = u.Center - 2 * pushVec;
                                u.Push(-2 * pushVec);
                            }
                            else if (e is Corpse)
                            {
                                Corpse c = (Corpse)e;
                                //u.Position = u.Center - pushVec;
                                //c.Position = c.Center + pushVec;
                                u.Push(pushVec);
                            }
                            else
                            {
                                Animal u1 = (Animal)e;
                                if (u.Player != u1.Player)
                                {
                                    if (u.WantsToMove && !u1.WantsToMove)
                                    {
                                        //u.Position = u.Center - 2 * pushVec;
                                        u.Push(-2 * pushVec);

                                    }
                                    else if (u1.WantsToMove && !u.WantsToMove)
                                    {
                                        //u1.Position = u1.Center + 2 * pushVec;
                                        u1.Push(2 * pushVec);

                                    }
                                    else
                                    {
                                        //u.Position = u.Center - pushVec;
                                        //u1.Position = u1.Center + pushVec;
                                        u.Push(-1 * pushVec);
                                        u1.Push(pushVec);

                                    }
                                }
                                else
                                {
                                    if (u.CanBeMoved && !u1.CanBeMoved)
                                    {
                                        //u.Position = u.Center - 2 * pushVec;
                                        u.Push(-2 * pushVec);

                                    }
                                    else if (u1.CanBeMoved && !u.CanBeMoved)
                                    {
                                        //u1.Position = u1.Center + 2 * pushVec;
                                        u1.Push(2 * pushVec);
                                    }
                                    else
                                    {
                                        //u.Position = u.Center - pushVec;
                                        //u1.Position = u1.Center + pushVec;
                                        u.Push(-1 * pushVec);
                                        u1.Push(pushVec);
                                    }
                                }
                                PushOutsideOfObstacles(u, deltaT);
                                PushOutsideOfObstacles(u1, deltaT);
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
            foreach (Animal a in animals)
            {
                PushOutsideOfObstacles(a, deltaT);
            }
        }

        public void SetPushMaps(Map map)
        {
            ObstacleMap gOMap = map.ObstacleMaps[Movement.LAND];
            ObstacleMap wOMap = map.ObstacleMaps[Movement.WATER];
            ObstacleMap gwOMap = map.ObstacleMaps[Movement.LAND_WATER];
            if (map.MapWasChanged
                || PushingMaps[Movement.LAND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.LAND_WATER] == null)
                UpdatePushingMaps(map);
        }

        public void PushOutsideOfObstacles(Animal animal, float deltaT)
        {
            //non-physical animals don't need to check for collisions
            if (!animal.Physical)
                return;

            PushingMap pushingMap;
            switch (animal.Movement)
            {
                case Movement.LAND:
                    pushingMap = PushingMaps[Movement.LAND];
                    break;
                case Movement.WATER:
                    pushingMap = PushingMaps[Movement.WATER];
                    break;
                default://Movement.LAND_WATER
                    pushingMap = PushingMaps[Movement.LAND_WATER];
                    break;
            }

            //move the animal outside of the blocked square in pushDirection
            Vector2 pushDirection = pushingMap.GetIntensity(animal.Center, 1f);
            float t1, t2;
            //t1 * pushDirection.X = point outside of the blocked square
            if (pushDirection.X > 0)
                t1 = ((float)Math.Ceiling(animal.Position.X) - animal.Position.X) / pushDirection.X;
            else
                t1 = ((float)Math.Floor(animal.Position.X) - animal.Position.X) / pushDirection.X;

            //t2 * pushDirection.Y = point outside of the blocked square
            if (pushDirection.Y > 0)
                t2 = ((float)Math.Ceiling(animal.Position.Y) - animal.Position.Y) / pushDirection.Y;
            else
                t2 = ((float)Math.Floor(animal.Position.Y) - animal.Position.Y) / pushDirection.Y;

            //use the smaller of t1, t2 to push animal outside of the blocked square
            float t = Math.Min(t1, t2);
            if (!float.IsNaN(t) && !float.IsInfinity(t))
                animal.Position += (t + 0.01f) * pushDirection;
        }

        public void MoveAnimals(Map map, List<Animal> units, float deltaT)
        {
            foreach(Animal u in units)
            {
                u.Move(map,deltaT);
            }
        }
    }
}
