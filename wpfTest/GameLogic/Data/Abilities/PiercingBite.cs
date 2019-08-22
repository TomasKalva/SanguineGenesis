using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class PiercingBite : TargetAbility<Animal, Animal>
    {
        public decimal Damage { get; }
        public float TimeToAttack { get; }

        internal PiercingBite(decimal energyCost, decimal damage, float timeToAttack)
            : base(0.1f, energyCost, false, true)
        {
            Damage = damage;
            TimeToAttack = timeToAttack;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PiercingBiteCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal deals a large amount of damage to the target.";
        }
    }

    public class PiercingBiteCommand : Command<Animal, Animal, PiercingBite>
    {
        private float timer;

        public PiercingBiteCommand(Animal commandedEntity, Animal target, PiercingBite bite)
            : base(commandedEntity, target, bite)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer > Ability.TimeToAttack)
            {
                Targ.Damage(Ability.Damage);
                return true;
            }

            return false;
        }
    }
}
