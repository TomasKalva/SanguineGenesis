using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Attack the target.
    /// </summary>
    sealed class Attack : TargetAbility<Animal, Entity>
    {
        internal Attack() : base(null, 0, false, false) { }

        public override Command NewCommand(Animal caster, Entity target)
        {
            return new AttackCommand(caster, target, this);
        }

        public override string GetName() => "Attack";

        public override string Description()
        {
            return "The unit deals repeatedly its attack damage to the target.";
        }
    }

    class AttackCommand : Command<Animal, Entity, Attack>
    {
        public AttackCommand(Animal commandedEntity, Entity target, Attack attack)
            : base(commandedEntity, target, attack)
        {
            CommandedEntity.SetAnimation("ATTACKING");
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(Targ.Center);

            CommandedEntity.CanBeMoved = false;
            if (ElapsedTime >= CommandedEntity.AttackPeriod)
            {
                ElapsedTime -= CommandedEntity.AttackPeriod;

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

        public override int Progress => (int)((100*(ElapsedTime/CommandedEntity.AttackPeriod)));

        public override void OnRemove()
        {
            base.OnRemove();
            CommandedEntity.SetAnimation("IDLE");
            CommandedEntity.CanBeMoved = true;
        }

        public override bool FollowTarget() => true;
    }
}
