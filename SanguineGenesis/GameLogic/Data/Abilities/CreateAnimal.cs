using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Creates a new animal near the building.
    /// </summary>
    sealed class CreateAnimal : Ability<Building, Nothing>
    {
        internal CreateAnimal(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Radius, spawningUnitFactory.EnergyCost, true, false, duration: spawningUnitFactory.SpawningTime)
        {
            SpawningAnimalFactory = spawningUnitFactory;
        }

        public AnimalFactory SpawningAnimalFactory { get; }

        public override Command NewCommand(Building user, Nothing target)
        {
            return new CreateAnimalCommand(user, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningAnimalFactory.EntityType;
        }

        public override string GetName() => "CREATE_" + SpawningAnimalFactory.EntityType;

        public override string Description()
        {
            return $"The entity creates a new {SpawningAnimalFactory.EntityType}. It will move to this entity's rally point.";
        }
    }

    class CreateAnimalCommand : Command<Building, Nothing, CreateAnimal>
    {
        public CreateAnimalCommand(Building commandedEntity, Nothing target, CreateAnimal spawn)
            : base(commandedEntity, target, spawn)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                //if the player doesn't have enough air, wait until he does
                if (CommandedEntity.Faction.AirTaken + Ability.SpawningAnimalFactory.Air > CommandedEntity.Faction.MaxAirTaken)
                    return false;

                //if there is no position where the animal can spawn, don't spawn the animal
                Vector2? newUnitPosition = GetSpawnPosition(game);
                if (newUnitPosition == null)
                    return false;

                var newUnitOwner = CommandedEntity.Faction;
                Animal newUnit = Ability.SpawningAnimalFactory.NewInstance(newUnitOwner, newUnitPosition.Value);
                game.Players[newUnitOwner.FactionID].AddEntity(newUnit);
                //make unit go towards the rally point
                if(CommandedEntity.RallyPoint!=null)
                    game.GameData.Abilities.MoveTo.SetCommands(new List<Unit>(1) { newUnit }, CommandedEntity.RallyPoint.Value, true, ActionLog.ThrowAway);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Helper class for generating pseudorandom vectors of small length.
        /// </summary>
        class SmallPseudoRandomVector
        {
            private static readonly Vector2[] vectors = {
                    new Vector2(-0.1f,0.1f),
                    new Vector2(0f,-0.1f),
                    new Vector2(0.1f,-0.1f),
                    new Vector2(0f,0.1f),
                    new Vector2(-0.1f,-0.1f),
                    new Vector2(0.1f,0f),
                    new Vector2(0.1f,0.1f),
                    new Vector2(-0.1f,-0f),
                };
            private static int i = 0;

            public static Vector2 Next()
            {
                i = (i + 1) % vectors.Length;
                return vectors[i];
            }
        }

        /// <summary>
        /// Returns position where the animal should spawn, if no such position exists,
        /// returns null.
        /// </summary>
        private Vector2? GetSpawnPosition(Game game)
        {
            Movement movement = Ability.SpawningAnimalFactory.Movement;
            ObstacleMap obstMap = game.Map.ObstacleMaps[movement];
            if (obstMap == null)
            {
                //obstacle maps are not loaded yet, it can't be determined where to
                //correctly spawn the animal
                return null;
            }
            else
            {
                int left = CommandedEntity.Left; int right = CommandedEntity.Right;
                int bottom = CommandedEntity.Bottom; int top = CommandedEntity.Top;
                var frame = GameQuerying.SelectNeighbors(game.Map,
                    left , bottom, right, top) ;
                foreach(Node n in frame)
                {
                    //return coordinate of square where the animal can spawn
                    //randomize the coordinate so that new animal doesn't spawn
                    //on the same spot as the last one
                    if (!obstMap[n.X, n.Y] && !n.MovementBlocked)
                        return new Vector2(n.X + 0.5f, n.Y + 0.5f) + SmallPseudoRandomVector.Next();
                }
                //no square where the animal can spawn was found
                return null;
            }
        }

        public override void OnRemove()
        {
            //refund the energy after canceling spawn command
            if (Paid)
                CommandedEntity.Energy += Ability.EnergyCost;
        }
    }

}
