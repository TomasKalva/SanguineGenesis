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
    /// Climb on a plant.
    /// </summary>
    sealed class ClimbPlant : Ability<Animal, Plant>
    {
        public AnimalsOnTreeFactory AnimalsOnPlantFactory { get; }

        internal ClimbPlant(float energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false, duration:climbingTime)
        {
            AnimalsOnPlantFactory = new AnimalsOnTreeFactory(new ClimbDownPlant(0, 0.5f));
        }

        public override Command NewCommand(Animal caster, Plant target)
        {
            return new ClimbPlantCommand(caster, target, this);
        }

        public override string GetName() => "CLIMB_UP";

        public override string Description()
        {
            return "The animal climbs on the plant.";
        }

        public override bool ValidArguments(Animal caster, Plant target, ActionLog actionLog)
        {
            if(caster.Faction.FactionID != target.Faction.FactionID)
            {
                actionLog.LogError(caster, this, "target doesn't have the same faction");
                return false;
            }
            return true;
        }
    }

    class ClimbPlantCommand : Command<Animal, Plant, ClimbPlant>, IAnimalStateManipulator
    {
        public ClimbPlantCommand(Animal commandedEntity, Plant target, ClimbPlant climbPlant)
            : base(commandedEntity, target, climbPlant)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                //put the animal on the target plant
                CommandedEntity.Faction.RemoveEntity(CommandedEntity);

                AnimalsOnTreeFactory anOnPlantFact = Ability.AnimalsOnPlantFactory;
                anOnPlantFact.PutOnTree = CommandedEntity;
                anOnPlantFact.ApplyToAffected(Target);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Climb down a plant.
    /// </summary>
    sealed class ClimbDownPlant : Ability<Plant, Nothing>
    {
        internal ClimbDownPlant(float energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false, duration:climbingTime)
        {
        }

        public override Command NewCommand(Plant caster, Nothing target)
        {
            return new ClimbDownPlantCommand(caster, target, this);
        }

        public override string GetName() => "CLIMB_DOWN";

        public override string Description()
        {
            return "The animal climbs down the plant.";
        }
    }

    class ClimbDownPlantCommand : Command<Plant, Nothing, ClimbDownPlant>, IAnimalStateManipulator
    {
        public ClimbDownPlantCommand(Plant commandedEntity, Nothing target, ClimbDownPlant climbDownPlant)
            : base(commandedEntity, target, climbDownPlant)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            var status = (AnimalsOnTree)CommandedEntity.Statuses.Where((s) => s.GetType() == typeof(AnimalsOnTree)).FirstOrDefault();
            //finish command if the status isn't on the plant anymore
            if (status == null)
                return true;
            
            if (ElapsedTime >= Ability.Duration)
            {
                ElapsedTime -= Ability.Duration;
                //put the animals from the plant back to the ground
                Animal anOnPlant = status.Animals.FirstOrDefault();
                if (anOnPlant == null)
                    //all animals already climbed down
                    return true;
                else
                {
                    anOnPlant.StateChangeLock = null;
                    CommandedEntity.Faction.AddEntity(anOnPlant);
                    status.Animals.Remove(anOnPlant);

                    //if there are no animals left, remove the status
                    if (!status.Animals.Any())
                    {
                        CommandedEntity.RemoveStatus(status);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
