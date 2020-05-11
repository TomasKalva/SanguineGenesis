using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.PushingMapGenerating;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Used for handling collisions.
    /// </summary>
    class Collisions
    {
        public Collisions(Map map)
        {
            PushingMaps = new Dictionary<Movement, PushingMap>
            {
                { Movement.LAND, null },
                { Movement.WATER, null },
                { Movement.LAND_WATER, null }
            };
            HashAnimals = new HashedAnimals(map.Width, map.Height);
        }

        #region Collisions with entities
        /// <summary>
        /// Data structure used in the method PushAway.
        /// </summary>
        private HashedAnimals HashAnimals { get; }

        /// <summary>
        /// Handles collisions between animals and entities. If animal gets pushed to blocked node,
        /// use corresponding PushingMap to push it back.
        /// </summary>
        public void ResolveCollisions(Game game)
        {
            Map map = game.Map;
            //create structure that hashes animals by their position
            HashAnimals.Init(game.GetAll<Animal>());

            foreach (Animal a in game.GetAll<Animal>().Where(animal=>animal.Physical))
            {
                //resolve collisions with nearby animals
                foreach (Animal otherA in HashAnimals.CloseAnimals(a.Position.X, a.Position.Y)
                                                        .Where(animal=>animal.Physical))
                {
                    //calculate collisions for each pair of animals only once
                    if (a != otherA && a.GetHashCode() <= otherA.GetHashCode())
                        Push(a, otherA, map);
                }
                //resolve collisions with nearby buildings
                int animalX = (int)a.Position.X;
                int animalY = (int)a.Position.Y;
                foreach (Building b in GameQuerying
                                        .SelectBuildingsInArea(map, new Rect(
                                            animalX - 1, animalY - 1, animalX + 1, animalY + 1))
                                        .Where(building => building.Physical))
                {
                    Push(a, b, map);
                }
            }
            HashAnimals.Reset();
        }

        private void Push(Animal a, Entity e, Map map)
        {
            float dist = (a.Center - e.Center).Length;
            //if two units get stuck on top of each other, move them apart
            if (dist == 0)
            {
                Vector2 epsilon = new Vector2(0.1f, 0.1f);
                a.Position = a.Center + epsilon;
                dist = epsilon.Length;
            }

            float totalR = a.Radius + e.Radius;
            //check if a and e are in collision
            if (dist < totalR)
            {
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
                    a.Push(-2 * pushVec, map);
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
                        else if (!a.WantsToMove && a1.WantsToMove)
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
                        else if (!a.CanBeMoved && a1.CanBeMoved)
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
                    PushOutOfObstacles(a);
                    PushOutOfObstacles(a1);
                }
            }
        }

        /// <summary>
        /// Hashes animals with their positions.
        /// </summary>
        private class HashedAnimals : IMap<List<Animal>>
        {
            public List<Animal> this[int i, int j] => hashedAnimals[i, j];
            private readonly List<Animal>[,] hashedAnimals;
            //list of indexes in hashedAnimals that are initialized
            private readonly List<Index> initialized;
            public int Width => hashedAnimals.GetLength(0);
            public int Height => hashedAnimals.GetLength(1);

            /// <summary>
            /// Index to hashedAnimals.
            /// </summary>
            private struct Index
            {
                public int I { get; }
                public int J { get; }

                public Index(int i, int j)
                {
                    this.I = i;
                    this.J = j;
                }
            }

            public HashedAnimals(int width, int height)
            {
                hashedAnimals = new List<Animal>[width, height];
                initialized = new List<Index>();
            }

            /// <summary>
            /// Hash animal to the map.
            /// </summary>
            public void Add(Animal animal)
            {
                int ax = ((int)animal.Position.X) % Width;
                int ay = ((int)animal.Position.Y) % Height;
                //initialize new list
                if (hashedAnimals[ax, ay] == null)
                {
                    hashedAnimals[ax, ay] = new List<Animal>();
                }
                //hash the animal
                hashedAnimals[ax, ay].Add(animal);
                initialized.Add(new Index(ax, ay));
            }

            /// <summary>
            /// Initializes map with animals hashed to their position.
            /// </summary>
            public void Init(IEnumerable<Animal> animals)
            {
                foreach (var a in animals)
                {
                    Add(a);
                }
            }

            /// <summary>
            /// Removes all animals from this map.
            /// </summary>
            public void Reset()
            {
                foreach (var index in initialized)
                {
                    hashedAnimals[index.I, index.J]?.Clear();
                }
                initialized.Clear();
            }

            /// <summary>
            /// Returns animals close to the coordinates.
            /// </summary>
            public IEnumerable<Animal> CloseAnimals(float x, float y)
            {
                //extents of area with close animals, maximal radius of animal is 0.5,
                //other animals in collision are at distance at most 1
                int left = (int)x - 1;
                int bottom = (int)y - 1;
                int right = (int)x + 2;//rounding down
                int top = (int)y + 2;
                //make IEnumerable of close animals by iterating all squares in the area
                IEnumerable<Animal> closeAnimals = new List<Animal>().AsEnumerable();
                foreach (var list in GameQuerying.SelectPartOfMap(this, left, bottom, right, top))
                {
                    //if lists are null, they contain no animals
                    if (list != null)
                    {
                        closeAnimals = closeAnimals.Concat(list);
                    }
                }
                return closeAnimals;
            }
        }
        #endregion Collisions with entities

        #region Collisions with environment
        /// <summary>
        /// Pushing maps for the current map. If the map changes they are updated by the method UpdatePushingMaps.
        /// </summary>
        public Dictionary<Movement, PushingMap> PushingMaps { get; }

        /// <summary>
        /// Creates PushingMaps from ObstacleMaps of map.
        /// </summary>
        public void SetPushingMaps(Map map)
        {

            ObstacleMap groundObst = map.ObstacleMaps[Movement.LAND];
            ObstacleMap waterObst = map.ObstacleMaps[Movement.WATER];
            ObstacleMap bothObst = map.ObstacleMaps[Movement.LAND_WATER];
            PushingMap gPMap = PushingMapGenerator.GeneratePushingMap(groundObst);
            PushingMap wPMap = PushingMapGenerator.GeneratePushingMap(waterObst);
            PushingMap gwPMap = PushingMapGenerator.GeneratePushingMap(bothObst);
            PushingMaps[Movement.LAND] = gPMap;
            PushingMaps[Movement.WATER] = wPMap;
            PushingMaps[Movement.LAND_WATER] = gwPMap;
        }

        /// <summary>
        /// Uses pushing maps to push animals that are on blocked squares.
        /// </summary>
        public void PushAllOutOfObstacles(IEnumerable<Animal> animals)
        {
            foreach (Animal a in animals.Where(animal=>animal.Physical))
            {
                PushOutOfObstacles(a);
            }
        }

        /// <summary>
        /// Pushes each animal using pushing map corresponding to the animal's movement.
        /// </summary>
        public void PushOutOfObstacles(Animal animal)
        {
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
            Vector2? pushDirNullable = pushingMap.GetDirection(animal.Center);
            if (pushDirNullable == null)
                return;

            Vector2 pushDir = pushDirNullable.Value;
            float t1, t2;
            //t1 * pushDirection.X = point outside of the blocked square
            if (pushDir.X > 0)
                t1 = ((float)Math.Ceiling(animal.Position.X) - animal.Position.X) / pushDir.X;
            else
                t1 = ((float)Math.Floor(animal.Position.X) - animal.Position.X) / pushDir.X;

            //t2 * pushDirection.Y = point outside of the blocked square
            if (pushDir.Y > 0)
                t2 = ((float)Math.Ceiling(animal.Position.Y) - animal.Position.Y) / pushDir.Y;
            else
                t2 = ((float)Math.Floor(animal.Position.Y) - animal.Position.Y) / pushDir.Y;

            //use the smaller of t1, t2 to push animal outside of the blocked square
            float t = Math.Min(t1, t2);
            if (!float.IsNaN(t) && !float.IsInfinity(t))
                animal.Position += (t + 0.01f) * pushDir;
        }
        #endregion Collisions with environment

        #region Collision testing
        /// <summary>
        /// True if physical entity with given location and radius collides with other
        /// buildings in the game.
        /// </summary>
        public bool CollidesWithBuilding(Game game, Vector2 location, float r)
        {
            //check collisions with buildings
            foreach (Building b in
                                GameQuerying.SelectBuildingsInArea(game.Map, new Rect((int)location.X - 1,
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
        #endregion Collision testing
    }
}
