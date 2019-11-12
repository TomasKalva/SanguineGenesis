using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// The target is consumed for a short time by the caster.
    /// </summary>
    sealed class ConsumeAnimal : TargetAbility<Animal, Animal>
    {
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }

        internal ConsumeAnimal(float energyCost, float timeToConsume, ConsumedAnimalFactory consumedAnimalFactory)
            : base(0.1f, energyCost, false, false, duration:timeToConsume)
        {
            ConsumedAnimalFactory = consumedAnimalFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new ConsumeAnimalCommand(caster, target, this);
        }

        public override string GetName() => "Consume";

        public override string Description()
        {
            return "The target animal is tepmorarily removed from the map and then put back on the map.";
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
            if (ElapsedTime > Ability.Duration)
            {
                ConsumedAnimalFactory consumedFact = Ability.ConsumedAnimalFactory;
                consumedFact.AnimalConsumed = Targ;
                consumedFact.ApplyToAffected(CommandedEntity);
                return true;
            }

            return false;
        }
    }

}
