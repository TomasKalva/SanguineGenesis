using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Used for handling collisions.
    /// </summary>
    class Collisions
    {
        /// <summary>
        /// Pushing maps for the current map. If the map changes they are updated by the method UpdatePushingMaps.
        /// </summary>
        public Dictionary<Movement, PushingMap> PushingMaps { get; }

        public static Collisions GetCollisions() => new Collisions();
        private Collisions()
        {
            PushingMaps = new Dictionary<Movement, PushingMap>
            {
                { Movement.LAND, null },
                { Movement.WATER, null },
                { Movement.LAND_WATER, null }
            };
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

        /// <summary>
        /// Handles collisions between animals and entities. If animal gets pushed to blocked node,
        /// use corresponding PushingMap to push it back.
        /// </summary>
        public void PushAway(Map map, List<Animal> physicalAnimals)
        {
            //set push maps so that colliding animals can be correctly pushed to not blocked squares
            SetPushMaps(map);
            
            foreach (Animal a in physicalAnimals)
            {
                //animals that aren't physical don't need to check for collisions
                if (!a.Physical)
                    continue;

                //resolve collisions with other animals
                foreach (Animal e in physicalAnimals)
                {
                    //calculate collisions for each pair of animals only once
                    if (a.GetHashCode() < e.GetHashCode())
                        Push(a, e, map);
                }
                //resolve collisions with nearby buildings
                int animalX = (int)a.Position.X;
                int animalY = (int)a.Position.Y;
                foreach(Building b in GameQuerying
                                        .SelectBuildingInArea(map, new Rect(
                                            animalX - 1, animalY - 1, animalX + 1, animalY + 1)))
                {
                    Push(a, b, map);
                }
            }
        }

        private void Push(Animal a, Entity e, Map map)
        {
            //entities that aren't physical don't need to check for collisions
            if (!e.Physical)
                return;

            float dist = (a.Center - e.Center).Length;
            //if two units get stuck on top of each other, move them apart
            if (dist == 0)
            {
                Vector2 epsilon = new Vector2(0.1f, 0.1f);
                a.Position = a.Center + epsilon;
                dist = epsilon.Length;
            }
            float totalR = a.Radius + e.Radius;
            //check if u and e are in collision
            if (dist < totalR && dist != 0)
            {
                //push centres of the units from each other
                Vector2 dirAtoE = a.Center.UnitDirectionTo(e.Center);
                Vector2 pushVec = (totalR - dist) / 2 * dirAtoE;
                if (e is Building)
                {
                    //buildings can't be pushed
                    a.Push(-2 * pushVec, map);
                }
                else if (e is Corpse)
                {
                    //corpse isn't pushed
                    a.Push(pushVec, map);
                }
                else
                {
                    Animal a1 = (Animal)e;
                    if (a.Faction != a1.Faction)
                    {
                        //if the players are different, push the animal that wants to move
                        //if both or none want to move, push both of them
                        if (a.WantsToMove && !a1.WantsToMove)
                        {
                            a.Push(-2 * pushVec, map);

                        }
                        else if (a1.WantsToMove && !a.WantsToMove)
                        {
                            a1.Push(2 * pushVec, map);

                        }
                        else
                        {
                            a.Push(-1 * pushVec, map);
                            a1.Push(pushVec, map);

                        }
                    }
                    else
                    {
                        //if the players are different, push the animal that wants to move
                        //if both or none want to move, push both of them
                        if (a.CanBeMoved && !a1.CanBeMoved)
                        {
                            a.Push(-2 * pushVec, map);

                        }
                        else if (a1.CanBeMoved && !a.CanBeMoved)
                        {
                            a1.Push(2 * pushVec, map);
                        }
                        else
                        {
                            a.Push(-1 * pushVec, map);
                            a1.Push(pushVec, map);
                        }
                    }
                    //push animal with a pushing map if it gets into 
                    //collision with blocked node
                    PushOutsideOfObstacles(a);
                    PushOutsideOfObstacles(a1);
                }
            }
        }

        /// <summary>
        /// Uses pushing maps to push animals that are on blocked squares.
        /// </summary>
        public void PushOutsideOfObstacles(Map map, List<Animal> animals)
        {
            //set pushing maps or reset them if map was changed
            if (map.MapWasChanged
                || PushingMaps[Movement.LAND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.LAND_WATER] == null)
                UpdatePushingMaps(map);
            foreach (Animal a in animals)
            {
                PushOutsideOfObstacles(a);
            }
        }

        /// <summary>
        /// Set pushing maps or reset them if map was changed.
        /// </summary>
        /// <param name="map"></param>
        public void SetPushMaps(Map map)
        {
            if (map.MapWasChanged
                || PushingMaps[Movement.LAND] == null
                || PushingMaps[Movement.WATER] == null
                || PushingMaps[Movement.LAND_WATER] == null)
                UpdatePushingMaps(map);
        }

        /// <summary>
        /// Pushes each animal using pushing map corresponding to the animal's movement.
        /// </summary>
        public void PushOutsideOfObstacles(Animal animal)
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

        /// <summary>
        /// Move each animal.
        /// </summary>
        public void MoveAnimals(Map map, List<Animal> animals, float deltaT)
        {
            foreach(Animal a in animals)
            {
                a.Move(map,deltaT);
            }
        }

        /// <summary>
        /// True if physical entity with given location and radius collides with other
        /// buildings in the game.
        /// </summary>
        public bool CollidesWithBuilding(Game game, Vector2 location, float r)
        {
            //check collisions with buildings
            foreach (Building b in 
                                GameQuerying.SelectBuildingInArea(game.Map, new Rect((int)location.X - 1, 
                                    (int)location.Y - 1, (int)location.X + 1, (int)location.Y + 1)))
            {
                if (b.Physical &&
                    (b.Center - location).Length < b.Radius + r)
                        return true;
            }

            return false;
        }
        
        /// <summary>
        /// True if physical entity with given location and radius collides with other
        /// physical units in the game.
        /// </summary>
        public bool CollidesWithUnits(Game game, Vector2 location, float r)
        {
            //check collisions with units
            var physicalUnits = game.GetAll<Unit>().Where(u => u.Physical);
            foreach (var u in physicalUnits)
                if ((u.Center - location).Length < u.Radius + r)
                    return true;

            return false;
        }
    }
}
