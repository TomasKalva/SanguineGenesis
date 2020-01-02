using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Pull the target to the caster.
    /// </summary>
    sealed class Pull : TargetAbility<Animal, Animal>
    {
        public float PullSpeed { get; }

        internal Pull(float energyCost, float distance, float preparationTime, float pullSpeed)
            : base(distance, energyCost, false, false, false, duration:preparationTime)
        {
            PullSpeed = pullSpeed;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PullCommand(caster, target, this);
        }

        public override string GetName() => "PULL";

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
        }
    }

    class PullCommand : Command<Animal, Animal, Pull>, IAnimalStateManipulator
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
            CommandedEntity.TurnToPoint(Target.Position);
            firstPullingStep = true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.StateChangeLock = this;
            Target.StateChangeLock = this;

            CommandedEntity.TurnToPoint(Target.Position);
            
            if (!pulling)
            {
                //animal is preparing to pull
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
                    //clear the command queue of Targ, so that it can't move away
                    Target.CommandQueue.Clear();
                    //create instance of MoveAnimalToPoint that will be moving Targ to CommandedEntity
                    Vector2 frontOfAnimal = CommandedEntity.Position + (CommandedEntity.Range + Target.Range) * CommandedEntity.Direction;
                    float dist = (Target.Center - frontOfAnimal).Length;
                    moveAnimalToPoint = new MoveAnimalToPoint(Target, frontOfAnimal, Ability.PullSpeed, dist / Ability.PullSpeed);

                    firstPullingStep = false;
                }
                if (moveAnimalToPoint.Step(deltaT))
                {
                    CommandedEntity.StateChangeLock = null;
                    Target.StateChangeLock = null;
                    return true;
                }
            }

            //command doesn't finish until the animal pulls the target animal
            return false;
        }

        public override int Progress => pulling ? 100 : base.Progress;

        public override void OnRemove()
        {
            base.OnRemove();
            CommandedEntity.StateChangeLock = null;
            Target.StateChangeLock = null;
        }
    }
}
