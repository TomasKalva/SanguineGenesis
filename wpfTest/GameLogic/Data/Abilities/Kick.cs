using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class Kick : TargetAbility<Animal, Animal>
    {
        public float PreparationTime { get; }
        public decimal EnergyDamage { get; }

        internal Kick(decimal energyCost, float distance, float preparationTime, decimal energyDamage)
            : base(distance, energyCost, false, false)
        {
            PreparationTime = preparationTime;
            EnergyDamage = energyDamage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KickCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal kicks the target animal removing some of its energy.";
        }
    }

    public class KickCommand : Command<Animal, Animal, Kick>
    {
        private float timer;

        public KickCommand(Animal commandedEntity, Animal target, Kick kick)
            : base(commandedEntity, target, kick)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(Targ.Position);

            timer += deltaT;
            if (timer >= Ability.PreparationTime)
            {
                //remove some of the target's energy
                Targ.Energy -= Ability.EnergyDamage;
                return true;
            }
            return false;
        }
    }
}
