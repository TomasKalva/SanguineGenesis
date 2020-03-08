using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Deal huge amount of damage to animal.
    /// </summary>
    sealed class PiercingBite : Ability<Animal, Animal>
    {
        public float Damage { get; }

        internal PiercingBite(float energyCost, float damage, float timeToAttack)
            : base(0.1f, energyCost, false, true, duration:timeToAttack)
        {
            Damage = damage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PiercingBiteCommand(caster, target, this);
        }

        public override string GetName() => "PIERCING_BITE";

        public override string Description()
        {
            return "The animal deals a large amount of damage to the target.";
        }
    }

    class PiercingBiteCommand : Command<Animal, Animal, PiercingBite>
    {
        public PiercingBiteCommand(Animal commandedEntity, Animal target, PiercingBite bite)
            : base(commandedEntity, target, bite)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime > Ability.Duration)
            {
                Target.Damage(Ability.Damage, true);
                return true;
            }

            return false;
        }
    }
}
