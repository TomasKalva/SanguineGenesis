using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Move quickly to the target and deal it damage.
    /// </summary>
    sealed class ChargeTo : TargetAbility<Animal, Entity>
    {
        public float AttackDamageMultiplier { get; }
        public float ChargeSpeed { get; }

        internal ChargeTo(float energyCost, float distance, float attackDamageMultiplier, float charageSpeed)
            : base(distance, energyCost, false, false, false)
        {
            AttackDamageMultiplier = attackDamageMultiplier;
            ChargeSpeed = charageSpeed;
        }

        public override Command NewCommand(Animal caster, Entity target)
        {
            return new ChargeToCommand(caster, target, this);
        }

        public override string GetName() => "CHARGE_TO";

        public override string Description()
        {
            return "The animal charges to the entity and deals it damage.";
        }
    }

    class ChargeToCommand : Command<Animal, Entity, ChargeTo>, IAnimalStateManipulator
    {
        private MoveAnimalToPoint moveAnimalToPoint;

        public ChargeToCommand(Animal commandedEntity, Entity target, ChargeTo chargeTo)
            : base(commandedEntity, target, chargeTo)
        {
            moveAnimalToPoint = new MoveAnimalToPoint(commandedEntity, target, Ability.ChargeSpeed, Distance/Ability.ChargeSpeed);
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.StateChangeLock = this;
            CommandedEntity.TurnToPoint(Target.Center);

            if (moveAnimalToPoint.Step(deltaT))
            {
                CommandedEntity.StateChangeLock = null;
                Target.Damage(Ability.AttackDamageMultiplier * CommandedEntity.AttackDamage, true);
                return true;
            }

            //command doesn't finish until the animal finishes charging
            return false;
        }

        public override void OnRemove()
        {
            //remove lock of movement for this entity
            CommandedEntity.StateChangeLock = null;
            //set physical of the animal to the base value - true
            CommandedEntity.Physical = true;
        }
    }
}
