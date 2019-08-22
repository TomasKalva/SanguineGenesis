using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class Spawn : TargetAbility<Entity, Vector2>
    {
        internal Spawn(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false)
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

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    public class SpawnCommand : Command<Entity, Vector2, Spawn>
    {
        /// <summary>
        /// How long the unit was spawning in s.
        /// </summary>
        public float SpawnTimer { get; private set; }

        private SpawnCommand() => throw new NotImplementedException();
        public SpawnCommand(Entity commandedEntity, Vector2 target, Spawn spawn)
            : base(commandedEntity, target, spawn)
        {
            SpawnTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            SpawnTimer += deltaT;
            if (SpawnTimer >= Ability.SpawningUnitFactory.SpawningTime)
            {
                Player newUnitOwner = CommandedEntity.Player;
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, Targ);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                return true;
            }
            return false;
        }
    }
}
