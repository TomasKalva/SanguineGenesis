using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Creates a new animal near the building.
    /// </summary>
    sealed class CreateAnimal : TargetAbility<Building, Nothing>
    {
        internal CreateAnimal(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false, duration: spawningUnitFactory.SpawningTime)
        {
            SpawningAnimalFactory = spawningUnitFactory;

        }

        public AnimalFactory SpawningAnimalFactory { get; }

        public override Command NewCommand(Building caster, Nothing target)
        {
            return new CreateUnitCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningAnimalFactory.EntityType;
        }

        public override string GetName() => "Create " + SpawningAnimalFactory.EntityType;

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    class CreateUnitCommand : Command<Building, Nothing, CreateAnimal>
    {
        public CreateUnitCommand(Building commandedEntity, Nothing target, CreateAnimal spawn)
            : base(commandedEntity, target, spawn)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                //if the player doesn't have enough air, wait until he does
                if (CommandedEntity.Player.AirTaken + Ability.SpawningAnimalFactory.Air > CommandedEntity.Player.MaxAirTaken)
                    return false;

                //if there is no position where the animal can spawn, don't spawn the animal
                Vector2? newUnitPosition = GetSpawnPosition(game);
                if (newUnitPosition == null)
                    return false;

                Player newUnitOwner = CommandedEntity.Player;
                //Vector2 newUnitPosition = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - Ability.SpawningAnimalFactory.Range);
                Animal newUnit = Ability.SpawningAnimalFactory.NewInstance(newUnitOwner, newUnitPosition.Value);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                //make unit go towards the rally point
                newUnitOwner.GameStaticData.Abilities.MoveTo.SetCommands(new List<Unit>(1) { newUnit }, CommandedEntity.RallyPoint, true);
                return true;
            }
            return false;
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
                    if (!obstMap[n.X, n.Y] && !n.MovementBlocked)
                        return new Vector2(n.X + 0.5f, n.Y + 0.5f);
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
