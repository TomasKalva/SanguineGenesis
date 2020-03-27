using SanguineGenesis.GameLogic.Data.Entities;
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
    sealed class Attack : Ability<Animal, Entity>
    {
        /// <summary>
        /// Used just to return correct name.
        /// </summary>
        private bool Unbreakable { get; }

        internal Attack(bool unbreakable) : base(null, 0, false, false)
        {
            Unbreakable = unbreakable;
        }

        public override Command NewCommand(Animal caster, Entity target)
        {
            return new AttackCommand(caster, target, this);
        }

        public override string GetName() => Unbreakable?"UNBR_ATTACK":"ATTACK";

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
            CommandedEntity.TurnToPoint(Target.Center);
            
            if (ElapsedTime >= CommandedEntity.AttackPeriod)
            {
                ElapsedTime -= CommandedEntity.AttackPeriod;

                if (Target is Building && !CommandedEntity.MechanicalDamage)
                {
                    //deal less damage if animal without mechanical damage attacks a building
                    Target.Damage(CommandedEntity.AttackDamage / 10, true);
                }
                else
                {
                    //damage target
                    Target.Damage(CommandedEntity.AttackDamage, true);
                }
            }

            bool finished = CommandedEntity.DistanceTo(Target) >= CommandedEntity.AttackDistance;
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
        }

        public override bool FollowTarget() => true;
    }
}
