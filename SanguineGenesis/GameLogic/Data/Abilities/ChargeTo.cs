using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI.WinFormsComponents;
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
    sealed class ChargeTo : Ability<Animal, Entity>
    {
        public float AttackDamageMultiplier { get; }
        public float ChargeSpeed { get; }

        internal ChargeTo(float energyCost, float distance, float attackDamageMultiplier, float charageSpeed)
            : base(distance, energyCost, false, false, false)
        {
            AttackDamageMultiplier = attackDamageMultiplier;
            ChargeSpeed = charageSpeed;
        }

        public override Command NewCommand(Animal user, Entity target)
        {
            return new ChargeToCommand(user, target, this);
        }

        public override string GetName() => "CHARGE_TO";

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
            new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
            new Stat( "Self useable", SelfUseable.ToString()),
            new Stat( "Only one", OnlyOne.ToString()),
            new Stat( "Target type", TargetName),
            new Stat( "Dmg mult", AttackDamageMultiplier.ToString("0.0")),
            };
            return stats;
        }

        public override string Description()
        {
            return $"The animal charges to the entity and deals it {AttackDamageMultiplier.ToString("0.0")} times of its attack damage.";
        }
    }

    class ChargeToCommand : Command<Animal, Entity, ChargeTo>, IAnimalStateManipulator
    {
        private readonly MoveAnimalToPoint moveAnimalToPoint;

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
