﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class Attack : TargetAbility<Animal, Entity>
    {
        internal Attack() : base(0.1f, 0, false, false) { }

        public override Command NewCommand(Animal caster, Entity target)
        {
            return new AttackCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The unit deals repeatedly its attack damage to the target.";
        }
    }

    public class AttackCommand : Command<Animal, Entity, Attack>
    {
        private float timeUntilAttack;//time in s until this unit attacks

        public AttackCommand(Animal commandedEntity, Entity target, Attack attack)
            : base(commandedEntity, target, attack)
        {
            this.timeUntilAttack = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(Targ.Center);

            CommandedEntity.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                timeUntilAttack -= CommandedEntity.AttackPeriod;

                if (Targ is Building && !CommandedEntity.MechanicalDamage)
                {
                    //deal less damage if animal without mechanical damage attacks a building
                    Targ.Damage(CommandedEntity.AttackDamage / 10);
                }
                else
                {
                    //damage target
                    Targ.Damage(CommandedEntity.AttackDamage);
                }
            }

            bool finished = CommandedEntity.DistanceTo(Targ) >= CommandedEntity.AttackDistance;
            if (finished)
            {
                return true;
            }
            return false;
        }
    }

}