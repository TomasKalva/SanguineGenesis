using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Apply poison to the animal.
    /// </summary>
    public sealed class PoisonousSpit : TargetAbility<Animal, Animal>
    {
        public PoisonFactory PoisonFactory { get; }

        internal PoisonousSpit(float distance, float timeUntilSpit, decimal energyCost, PoisonFactory poisonFactory)
            : base(distance, energyCost, false, false, duration:timeUntilSpit)
        {
            PoisonFactory = poisonFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PoisonousSpitCommand(caster, target, this);
        }

        public override string GetName() => "Poisonous spit";

        public override string Description()
        {
            return "The unit applies poison to the target after short period of time.";
        }
    }

    public class PoisonousSpitCommand : Command<Animal, Animal, PoisonousSpit>
    {
        public PoisonousSpitCommand(Animal commandedEntity, Animal target, PoisonousSpit attack)
            : base(commandedEntity, target, attack)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(CommandedEntity.Center);
            
            if (ElapsedTime >= Ability.Duration)
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
