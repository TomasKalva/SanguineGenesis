using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{
    /// <summary>
    /// Deal huge amount of damage to animal.
    /// </summary>
    public sealed class PiercingBite : TargetAbility<Animal, Animal>
    {
        public decimal Damage { get; }

        internal PiercingBite(decimal energyCost, decimal damage, float timeToAttack)
            : base(0.1f, energyCost, false, true, duration:timeToAttack)
        {
            Damage = damage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PiercingBiteCommand(caster, target, this);
        }

        public override string GetName() => "Piercing bite";

        public override string Description()
        {
            return "The animal deals a large amount of damage to the target.";
        }
    }

    public class PiercingBiteCommand : Command<Animal, Animal, PiercingBite>
    {
        public PiercingBiteCommand(Animal commandedEntity, Animal target, PiercingBite bite)
            : base(commandedEntity, target, bite)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime > Ability.Duration)
            {
                Targ.Damage(Ability.Damage);
                return true;
            }

            return false;
        }
    }
}
