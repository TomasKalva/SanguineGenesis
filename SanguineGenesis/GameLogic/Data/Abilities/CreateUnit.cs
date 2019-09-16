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
    public sealed class CreateAnimal : TargetAbility<Building, Nothing>
    {
        internal CreateAnimal(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false, duration: spawningUnitFactory.SpawningTime)
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

        public override string GetName() => "Create " + SpawningUnitFactory.EntityType;

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    public class CreateUnitCommand : Command<Building, Nothing, CreateAnimal>
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
                if (CommandedEntity.Player.AirTaken + Ability.SpawningUnitFactory.Air > CommandedEntity.Player.MaxAirTaken)
                    return false;

                Player newUnitOwner = CommandedEntity.Player;
                Vector2 newUnitPosition = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - Ability.SpawningUnitFactory.Range);
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, newUnitPosition);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                //make unit go towards the rally point
                newUnitOwner.GameStaticData.Abilities.MoveTo.SetCommands(new List<Unit>(1) { newUnit }, CommandedEntity.RallyPoint, true);
                return true;
            }
            return false;
        }

        public override void OnRemove()
        {
            //refund the energy after canceling spawn command
            if (Paid)
                CommandedEntity.Energy += Ability.EnergyCost;
        }
    }

}
