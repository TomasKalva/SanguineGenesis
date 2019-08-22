using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class ClimbTree : TargetAbility<Animal, Tree>
    {
        public float ClimbingTime { get; }
        public AnimalsOnTreeFactory AnimalsOnTreeFactory { get; }

        internal ClimbTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false)
        {
            ClimbingTime = climbingTime;
            AnimalsOnTreeFactory = new AnimalsOnTreeFactory();
        }

        public override Command NewCommand(Animal caster, Tree target)
        {
            return new ClimbTreeCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal climbs on the tree.";
        }
    }

    public class ClimbTreeCommand : Command<Animal, Tree, ClimbTree>, IAnimalStateManipulator
    {
        private float timer;

        public ClimbTreeCommand(Animal commandedEntity, Tree target, ClimbTree climbTree)
            : base(commandedEntity, target, climbTree)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer >= Ability.ClimbingTime)
            {
                //put the animal on the target tree
                CommandedEntity.Player.Entities.Remove(CommandedEntity);

                AnimalsOnTreeFactory anOnTreeFact = Ability.AnimalsOnTreeFactory;
                anOnTreeFact.PutOnTree = CommandedEntity;
                anOnTreeFact.ApplyToAffected(Targ);
                return true;
            }

            return false;
        }
    }

    public sealed class ClimbDownTree : TargetAbility<Tree, Nothing>
    {
        public float ClimbingTime { get; }

        internal ClimbDownTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false)
        {
            ClimbingTime = climbingTime;
        }

        public override Command NewCommand(Tree caster, Nothing target)
        {
            return new ClimbDownTreeCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal climbs down the tree.";
        }
    }

    public class ClimbDownTreeCommand : Command<Tree, Nothing, ClimbDownTree>, IAnimalStateManipulator
    {
        private float timer;

        public ClimbDownTreeCommand(Tree commandedEntity, Nothing target, ClimbDownTree climbDownTree)
            : base(commandedEntity, target, climbDownTree)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            var status = (AnimalsOnTree)CommandedEntity.Statuses.Where((s) => s.GetType() == typeof(AnimalsOnTree)).FirstOrDefault();
            //finish command if the status isn't on the tree anymore
            if (status == null)
                return true;

            timer += deltaT;
            if (timer >= Ability.ClimbingTime)
            {
                timer -= Ability.ClimbingTime;
                //put the animals from the tree back to the ground
                Animal anOnTree = status.Animals.FirstOrDefault();
                if (anOnTree == null)
                    //all animals already climbed down
                    return true;
                else
                {
                    anOnTree.StateChangeLock = null;
                    CommandedEntity.Player.Entities.Add(anOnTree);
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
