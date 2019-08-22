using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic.Data.Abilities
{
    public sealed class CreateUnit : TargetAbility<Building, Nothing>
    {
        internal CreateUnit(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false)
        {
            SpawningUnitFactory = spawningUnitFactory;
        }

        public AnimalFactory SpawningUnitFactory { get; }

        public override Command NewCommand(Building caster, Nothing target)
        {
            return new CreateUnitCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningUnitFactory.EntityType;
        }

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    public class CreateUnitCommand : Command<Building, Nothing, CreateUnit>
    {
        /// <summary>
        /// How long the unit was spawning in s.
        /// </summary>
        public float SpawnTimer { get; private set; }

        private CreateUnitCommand() => throw new NotImplementedException();
        public CreateUnitCommand(Building commandedEntity, Nothing target, CreateUnit spawn)
            : base(commandedEntity, target, spawn)
        {
            SpawnTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            SpawnTimer += deltaT;
            if (SpawnTimer >= Ability.SpawningUnitFactory.SpawningTime)
            {
                //if the player doesn't have enough air, wait until he does
                if (CommandedEntity.Player.AirTaken + Ability.SpawningUnitFactory.Air > CommandedEntity.Player.MaxAirTaken)
                    return false;

                Player newUnitOwner = CommandedEntity.Player;
                Vector2 newUnitPosition = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - Ability.SpawningUnitFactory.Range);
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, newUnitPosition);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                //make unit go towards the rally point
                newUnitOwner.GameStaticData.Abilities.MoveTo.SetCommands(new List<Unit>(1) { newUnit }, CommandedEntity.RallyPoint);
                return true;
            }
            return false;
        }
    }

}
