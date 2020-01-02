using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Create a new animal at the target location.
    /// </summary>
    sealed class Spawn : TargetAbility<Entity, Vector2>
    {
        internal Spawn(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false, duration:spawningUnitFactory.SpawningTime)
        {
            SpawningUnitFactory = spawningUnitFactory;
        }

        public AnimalFactory SpawningUnitFactory { get; }

        public override Command NewCommand(Entity caster, Vector2 target)
        {
            return new SpawnCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningUnitFactory.EntityType;
        }

        public override string GetName() => "SPAWN_" + SpawningUnitFactory.EntityType;

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    class SpawnCommand : Command<Entity, Vector2, Spawn>
    {
        public SpawnCommand(Entity commandedEntity, Vector2 target, Spawn spawn)
            : base(commandedEntity, target, spawn)
        {
            ElapsedTime = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                var newUnitOwner = CommandedEntity.Faction;
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, Target);
                newUnitOwner.Entities.Add(newUnit);
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
