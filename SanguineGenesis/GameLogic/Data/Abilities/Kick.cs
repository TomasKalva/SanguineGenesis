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
        public decimal EnergyDamage { get; }

        internal Kick(decimal energyCost, float distance, float preparationTime, decimal energyDamage)
            : base(distance, energyCost, false, false, duration:preparationTime)
        {
            EnergyDamage = energyDamage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KickCommand(caster, target, this);
        }

        public override string GetName() => "Kick";

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
            CommandedEntity.TurnToPoint(Targ.Position);

            ElapsedTime += deltaT;
            if (ElapsedTime >= Ability.Duration)
            {
                //remove some of the target's energy
                Targ.Energy -= Ability.EnergyDamage;
                return true;
            }
            return false;
        }
    }
}
