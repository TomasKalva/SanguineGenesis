using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// The target is consumed for a short time by the caster.
    /// </summary>
    sealed class ConsumeAnimal : Ability<Animal, Animal>
    {
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }

        internal ConsumeAnimal(float energyCost, float timeToConsume, ConsumedAnimalFactory consumedAnimalFactory)
            : base(0.1f, energyCost, true, false, duration:timeToConsume)
        {
            ConsumedAnimalFactory = consumedAnimalFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new ConsumeAnimalCommand(caster, target, this);
        }

        /// <summary>
        /// Animal can only eat twice as small animals.
        /// </summary>
        public override bool ValidArguments(Animal caster, Animal target, ActionLog actionLog)
        {
            if (target.Radius * 2 > caster.Radius)
            {
                actionLog.LogError(caster, this, "target is too big");
                return false;
            }
            return true;
        }

        public override string GetName() => "CONSUME";

        public override string Description()
        {
            return "The target animal is tepmorarily removed from the map and then put back on the map. Animal can only eat twice as small animals.";
        }
    }

    class ConsumeAnimalCommand : Command<Animal, Animal, ConsumeAnimal>
    {
        public ConsumeAnimalCommand(Animal commandedEntity, Animal target, ConsumeAnimal bite)
            : base(commandedEntity, target, bite)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //animal is preparing to consume
            if (ElapsedTime >= Ability.Duration)
            {
                ConsumedAnimalFactory consumedFact = Ability.ConsumedAnimalFactory;
                consumedFact.AnimalConsumed = Target;
                consumedFact.ApplyToAffected(CommandedEntity);
                return true;
            }

            return false;
        }
    }

}
