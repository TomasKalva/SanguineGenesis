using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Deal energy damage to the animal.
    /// </summary>
    sealed class Kick : TargetAbility<Animal, Animal>
    {
        public float EnergyDamage { get; }

        internal Kick(float energyCost, float distance, float preparationTime, float energyDamage)
            : base(distance, energyCost, true, true, duration:preparationTime)
        {
            EnergyDamage = energyDamage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KickCommand(caster, target, this);
        }

        public override string GetName() => "KICK";

        public override string Description()
        {
            return "The animal kicks the target animal removing some of its energy.";
        }
    }

    class KickCommand : Command<Animal, Animal, Kick>
    {
        public KickCommand(Animal commandedEntity, Animal target, Kick kick)
            : base(commandedEntity, target, kick)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(Target.Position);

            ElapsedTime += deltaT;
            if (ElapsedTime >= Ability.Duration)
            {
                //remove some of the target's energy
                Target.Energy -= Ability.EnergyDamage;
                return true;
            }
            return false;
        }
    }
}
