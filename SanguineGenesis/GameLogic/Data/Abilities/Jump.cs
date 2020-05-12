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
    /// Jump to the target location.
    /// </summary>
    class Jump : Ability<Animal, Vector2>
    {
        public float JumpSpeed { get; }

        internal Jump(float energyCost, float distance, float preparationTime, float jumpSpeed)
            : base(distance, energyCost, false, false, false, duration:preparationTime)
        {
            JumpSpeed = jumpSpeed;
        }

        public override Command NewCommand(Animal user, Vector2 target)
        {
            return new JumpCommand(user, target, this);
        }

        public override string GetName() => "JUMP";

        public override string Description()
        {
            return "The animal jumps to the target location.";
        }
    }

    class JumpCommand : Command<Animal, Vector2, Jump>, IAnimalStateManipulator
    {
        /// <summary>
        /// The unit is jumping.
        /// </summary>
        private bool jumping;
        private readonly MoveAnimalToPoint moveAnimalToPoint;

        public JumpCommand(Animal commandedEntity, Vector2 target, Jump jump)
            : base(commandedEntity, target, jump)
        {
            jumping = false;
            moveAnimalToPoint = new MoveAnimalToPoint(commandedEntity, target, Ability.JumpSpeed);
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.StateChangeLock = this;
            
            if (!jumping)
            {
                //animal is preparing to jump
                if (ElapsedTime >= Ability.Duration)
                {
                    jumping = true;
                    ElapsedTime -= Ability.Duration;

                    //don't jump if the endpoint collides with building
                    if (game.collisions.CollidesWithBuilding(game, Target, CommandedEntity.Radius))
                    {
                        CommandedEntity.StateChangeLock = null;
                        Refund();
                        ActionLog.LogError(CommandedEntity, Ability, "target is colliding with a building");
                        return true;
                    }
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

        public override int Progress => jumping ? 100 : base.Progress;
    }
}
