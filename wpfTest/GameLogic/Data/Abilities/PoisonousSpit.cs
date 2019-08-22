using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic.Data.Abilities
{
    public sealed class PoisonousSpit : TargetAbility<Animal, Animal>
    {
        public float TimeUntilSpit { get; }
        public PoisonFactory PoisonFactory { get; }

        internal PoisonousSpit(float distance, float timeUntilSpit, decimal energyCost, PoisonFactory poisonFactory)
            : base(distance, energyCost, false, false)
        {
            TimeUntilSpit = timeUntilSpit;
            PoisonFactory = poisonFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PoisonousSpitCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The unit applies poison to the target after short period of time.";
        }
    }

    public class PoisonousSpitCommand : Command<Animal, Animal, PoisonousSpit>
    {
        private float spitTimer;//time in s until this unit attacks

        public PoisonousSpitCommand(Animal commandedEntity, Animal target, PoisonousSpit attack)
            : base(commandedEntity, target, attack)
        {
            this.spitTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(CommandedEntity.Center);

            spitTimer += deltaT;
            if (spitTimer >= Ability.TimeUntilSpit)
            {
                //apply poison to the target and finish the command
                Ability.PoisonFactory.ApplyToAffected(Targ);
                return true;
            }

            //command doesn't finish until it was spat
            return false;
        }
    }

}
