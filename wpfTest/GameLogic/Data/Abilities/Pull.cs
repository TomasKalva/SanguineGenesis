using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class Pull : TargetAbility<Animal, Animal>
    {
        public float PullSpeed { get; }

        internal Pull(decimal energyCost, float distance, float preparationTime, float pullSpeed)
            : base(distance, energyCost, false, false, false, duration:preparationTime)
        {
            PullSpeed = pullSpeed;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PullCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
        }
    }

    public class PullCommand : Command<Animal, Animal, Pull>, IAnimalStateManipulator
    {
        /// <summary>
        /// The unit is jumping.
        /// </summary>
        private bool pulling;
        private MoveAnimalToPoint moveAnimalToPoint;
        private bool firstPullingStep;

        public PullCommand(Animal commandedEntity, Animal target, Pull pull)
            : base(commandedEntity, target, pull)
        {
            pulling = false;
            CommandedEntity.TurnToPoint(Targ.Position);
            Vector2 frontOfAnimal = commandedEntity.Position + (commandedEntity.Range + target.Range) * commandedEntity.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(target, frontOfAnimal, Ability.PullSpeed);
            firstPullingStep = true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.StateChangeLock = this;
            Targ.StateChangeLock = this;

            CommandedEntity.TurnToPoint(Targ.Position);

            ElapsedTime += deltaT;
            if (!pulling)
            {
                //animal is preparing to jump
                if (ElapsedTime >= Ability.Duration)
                {
                    pulling = true;
                    ElapsedTime -= Ability.Duration;
                }
            }

            if (pulling)
            {
                if (firstPullingStep)
                {
                    //if(!Targ.CanBeTarget)
                    //target can't be used as target => fail the ability
                    //  return true;

                    firstPullingStep = false;
                }
                if (moveAnimalToPoint.Step(deltaT))
                {
                    CommandedEntity.StateChangeLock = null;
                    Targ.StateChangeLock = null;
                    return true;
                }
            }

            //command doesn't finish until the animal pulls the target animal
            return false;
        }

        public override int Progress => pulling ? 100 : base.Progress;
    }
}
