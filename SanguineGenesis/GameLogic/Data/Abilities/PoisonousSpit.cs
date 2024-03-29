﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Apply poison to the animal.
    /// </summary>
    class PoisonousSpit : Ability<Animal, Animal>
    {
        public PoisonFactory PoisonFactory { get; }

        internal PoisonousSpit(float distance, float timeUntilSpit, float energyCost, PoisonFactory poisonFactory)
            : base(distance, energyCost, false, false, duration:timeUntilSpit)
        {
            PoisonFactory = poisonFactory;
        }

        public override Command NewCommand(Animal user, Animal target)
        {
            return new PoisonousSpitCommand(user, target, this);
        }

        public override string GetName() => "POISONOUS_SPIT";

        public override string Description()
        {
            return $"The animal applies poison to the target after short period of time. Poison deals {PoisonFactory.TickDamage} damage each {PoisonFactory.TickTime}s {PoisonFactory.TotalNumberOfTicks} times.";
        }
    }

    class PoisonousSpitCommand : Command<Animal, Animal, PoisonousSpit>
    {
        public PoisonousSpitCommand(Animal commandedEntity, Animal target, PoisonousSpit attack)
            : base(commandedEntity, target, attack)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                //deal damage to the target
                Target.Damage(Ability.PoisonFactory.TickDamage * 2, false);
                //apply poison to the target and finish the command
                if (!Ability.PoisonFactory.ApplyToStatusOwner(Target))
                {
                    Refund();
                }
                return true;
            }

            //command doesn't finish until it was spat
            return false;
        }
    }

}
