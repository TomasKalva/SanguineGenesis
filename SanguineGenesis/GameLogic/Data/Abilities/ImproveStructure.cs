using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Transfer energy to the target structure.
    /// </summary>
    sealed class ImproveStructure : TargetAbility<Animal, Structure>
    {
        public float EnergyPerS { get; }

        internal ImproveStructure(float energyPerS)
            : base(0.1f, 0, false, false)
        {
            EnergyPerS = energyPerS;
        }

        public override Command NewCommand(Animal caster, Structure target)
        {
            return new ImproveStructureCommand(caster, target, this);
        }

        public override string GetName() => "Improve structure";

        public override string Description()
        {
            return "The animal gives its energy to the structure.";
        }
    }

    class ImproveStructureCommand : Command<Animal, Structure, ImproveStructure>, IAnimalStateManipulator
    {
        public ImproveStructureCommand(Animal commandedEntity, Structure target, ImproveStructure improveStructure)
            : base(commandedEntity, target, improveStructure)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //calculate transfered energy so that no energy is gained or lost during the transfer
            float transferedEn = Math.Min(((float)deltaT) * Ability.EnergyPerS, CommandedEntity.Energy);
            transferedEn = Math.Min(transferedEn, Target.Energy.AmountNotFilled);


            //transfer the energy
            CommandedEntity.Energy -= transferedEn;
            Target.Energy += transferedEn;

            //if the commanded entity doesn't have energy anymore, it can't give energy to the structure
            if (CommandedEntity.Energy == 0f)
                return true;

            return false;
        }

        public override int Progress => 100;
    }
}
