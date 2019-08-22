using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class Jump : TargetAbility<Animal, Vector2>
    {
        public float PreparationTime { get; }
        public float JumpSpeed { get; }

        internal Jump(decimal energyCost, float distance, float preparationTime, float jumpSpeed)
            : base(distance, energyCost, false, false, false)
        {
            PreparationTime = preparationTime;
            JumpSpeed = jumpSpeed;
        }

        public override Command NewCommand(Animal caster, Vector2 target)
        {
            return new JumpCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal jumps to the target location.";
        }
    }

    public class JumpCommand : Command<Animal, Vector2, Jump>, IAnimalStateManipulator
    {
        private float timer;
        /// <summary>
        /// The unit is jumping.
        /// </summary>
        private bool jumping;
        private MoveAnimalToPoint moveAnimalToPoint;

        public JumpCommand(Animal commandedEntity, Vector2 target, Jump jump)
            : base(commandedEntity, target, jump)
        {
            timer = 0f;
            jumping = false;
            moveAnimalToPoint = new MoveAnimalToPoint(commandedEntity, target, Ability.JumpSpeed);
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.StateChangeLock = this;
            CommandedEntity.TurnToPoint(Targ);

            timer += deltaT;
            if (!jumping)
            {
                //animal is preparing to jump
                if (timer >= Ability.PreparationTime)
                {
                    jumping = true;
                    timer -= Ability.PreparationTime;
                }
            }

            if (jumping)
            {
                if (moveAnimalToPoint.Step(deltaT))
                {
                    CommandedEntity.StateChangeLock = null;
                    return true;
                }
            }

            //command doesn't finish until the animal jumps
            return false;
        }
    }
}
