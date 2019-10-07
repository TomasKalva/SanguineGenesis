using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Climb on a tree.
    /// </summary>
    sealed class ClimbTree : TargetAbility<Animal, Tree>
    {
        public AnimalsOnTreeFactory AnimalsOnTreeFactory { get; }

        internal ClimbTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false, duration:climbingTime)
        {
            AnimalsOnTreeFactory = new AnimalsOnTreeFactory();
        }

        public override Command NewCommand(Animal caster, Tree target)
        {
            return new ClimbTreeCommand(caster, target, this);
        }

        public override string GetName() => "Climb";

        public override string Description()
        {
            return "The animal climbs on the tree.";
        }
    }

    class ClimbTreeCommand : Command<Animal, Tree, ClimbTree>, IAnimalStateManipulator
    {
        public ClimbTreeCommand(Animal commandedEntity, Tree target, ClimbTree climbTree)
            : base(commandedEntity, target, climbTree)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (ElapsedTime >= Ability.Duration)
            {
                //put the animal on the target tree
                CommandedEntity.Faction.Entities.Remove(CommandedEntity);

                AnimalsOnTreeFactory anOnTreeFact = Ability.AnimalsOnTreeFactory;
                anOnTreeFact.PutOnTree = CommandedEntity;
                anOnTreeFact.ApplyToAffected(Targ);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Climb down a tree.
    /// </summary>
    sealed class ClimbDownTree : TargetAbility<Tree, Nothing>
    {
        internal ClimbDownTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false, duration:climbingTime)
        {
        }

        public override Command NewCommand(Tree caster, Nothing target)
        {
            return new ClimbDownTreeCommand(caster, target, this);
        }

        public override string GetName() => "Climb down";

        public override string Description()
        {
            return "The animal climbs down the tree.";
        }
    }

    class ClimbDownTreeCommand : Command<Tree, Nothing, ClimbDownTree>, IAnimalStateManipulator
    {
        public ClimbDownTreeCommand(Tree commandedEntity, Nothing target, ClimbDownTree climbDownTree)
            : base(commandedEntity, target, climbDownTree)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            var status = (AnimalsOnTree)CommandedEntity.Statuses.Where((s) => s.GetType() == typeof(AnimalsOnTree)).FirstOrDefault();
            //finish command if the status isn't on the tree anymore
            if (status == null)
                return true;
            
            if (ElapsedTime >= Ability.Duration)
            {
                ElapsedTime -= Ability.Duration;
                //put the animals from the tree back to the ground
                Animal anOnTree = status.Animals.FirstOrDefault();
                if (anOnTree == null)
                    //all animals already climbed down
                    return true;
                else
                {
                    anOnTree.StateChangeLock = null;
                    CommandedEntity.Faction.Entities.Add(anOnTree);
                    status.Animals.Remove(anOnTree);

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
