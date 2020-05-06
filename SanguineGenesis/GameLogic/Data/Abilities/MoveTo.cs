using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// The animal moves to the target.
    /// </summary>
    sealed class MoveTo : Ability<Animal, IMovementTarget>
    {
        internal MoveTo(float? goalDistance, bool attackEnemyInstead)
            : base(null, 0, false, false)
        {
            GoalDistance = goalDistance;
            AttackEnemyInstead = attackEnemyInstead;
        }
        
        /// <summary>
        /// Null iff attack distance of commanded entity should be used.
        /// </summary>
        public float? GoalDistance { get; }
        public bool AttackEnemyInstead { get; }

        public override void SetCommands(IEnumerable<Animal> casters, IMovementTarget target, bool resetCommandQueue, ActionLog actionLog)
        {
            //if there are no casters do nothing
            if (!casters.Any())
                return;

            if (resetCommandQueue)
                //reset all commands
                foreach (Animal c in casters)
                    c.ResetCommands();

            //player whose animals are receiving commands
            FactionType player = casters.First().Faction.FactionID;

            //separete animals to different groups by their movement
            var castersGroups = casters.ToLookup((unit) => unit.Movement);

            //volume of all animals' circles /pi
            float volume = casters.Select((e) => e.Radius * e.Radius).Sum();
            //distance from the target when animal can stop if it gets stuck
            float minStoppingDistance = (float)Math.Sqrt(volume) * 1.3f;

            foreach (Movement m in Enum.GetValues(typeof(Movement)))
            {
                IEnumerable<Animal> castersMov = castersGroups[m];
                //set commands only if some unit can receive it
                if (!castersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, castersMov.Cast<Animal>().ToList(), m, target);
                //give command to each caster and set the command's creator
                foreach (Animal caster in castersMov)
                {
                    IComputable com = new MoveToCommand(caster, target, minStoppingDistance, this)
                    {
                        Assignment = mtca
                    };

                    caster.AddCommand((Command)com);
                }
                MovementGenerator.GetMovementGenerator().AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Animal caster, IMovementTarget target)
        {
            throw new NotImplementedException("This method is not necessary because the virtual method " + nameof(SetCommands) + " was overriden");
        }

        public override string GetName() => AttackEnemyInstead?"MOVE_TO":"UNBR_MOVE_TO";

        public override string Description()
        {
            return "The animal moves to the target. If the target is on a terrain " +
                "this animal can't move to, the animal won't do anything." + 
                (AttackEnemyInstead ? " If animal meets an enemy it attacks it instead." : "");
        }
    }


    class MoveToCommand : Command<Animal, IMovementTarget, MoveTo>, IComputable
    {
        /// <summary>
        /// Assignment for generating flowfield in other thread.
        /// </summary>
        public MoveToCommandAssignment Assignment { get; set; }
        /// <summary>
        /// Flowfield used for navigation. It can be set after the command was assigned.
        /// </summary>
        public FlowField FlowField { get; set; }
        /// <summary>
        /// Required distance between the animal and the target.
        /// </summary>
        public float GoalDistance => Ability.GoalDistance != null ? Ability.GoalDistance.Value : CommandedEntity.AttackDistance;
        /// <summary>
        /// Distance from the target when unit can stop if it gets stuck.
        /// </summary>
        private float MinStoppingDistance { get; }
        /// <summary>
        /// Detects if the animal got stuck.
        /// </summary>
        private NoMovementDetection NoMovementDetection { get; }

        public MoveToCommand(Animal commandedEntity, IMovementTarget target, float minStoppingDistance, MoveTo ability)
            : base(commandedEntity, target, ability)
        {
            MinStoppingDistance = minStoppingDistance;
            NoMovementDetection = new NoMovementDetection();
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            //command immediately finishes if the assignment was invalidated
            if (Assignment != null && Assignment.Invalid)
                return true;

            //set the command assignment to be active to increase its priority
            if (!Assignment.Active)
                Assignment.Active = true;

            //set correct animation
            if (CommandedEntity.AnimationState.Animation.Action!="RUNNING")
                CommandedEntity.SetAnimation("RUNNING");

            //if an enemy animal is in attack range, attack it instead of other commands
            if (Ability.AttackEnemyInstead)
            {
                Animal enemy = game.Players[CommandedEntity.Faction.FactionID.Opposite()]
                                .GetAll<Animal>().Where(
                                    (a) => a.Faction.FactionID == CommandedEntity.Faction.FactionID.Opposite()
                                    && CommandedEntity.DistanceTo(a) <= CommandedEntity.AttackDistance
                                    && CommandedEntity.Faction.CanSee(a))
                                .FirstOrDefault();
                if (enemy != null)
                {
                    //attack the enemy
                    CommandedEntity.WantsToMove = false;
                    RemoveFromAssignment();
                    CommandedEntity.SetCommand(new AttackCommand(CommandedEntity, enemy, game.CurrentPlayer.GameData.Abilities.Attack));
                    return false;//new command is already set
                }
            }

            //check if the map was set yet
            if (FlowField == null)
                return false;

            //accelerate animal
            Vector2 animalPos = CommandedEntity.Position;
            Building blockingBuilding;
            if ((blockingBuilding = game.Map[(int)animalPos.X, (int)animalPos.Y].Building) != null
                && blockingBuilding != Target
                && blockingBuilding.Physical)
            {
                //go outside of node with building to be able to use flowfield
                CommandedEntity.Accelerate(blockingBuilding.Center.UnitDirectionTo(animalPos),1000, game.Map);
            }
            else if (((int)Target.Center.X != (int)CommandedEntity.Center.X ||
                 (int)Target.Center.Y != (int)CommandedEntity.Center.Y))
            {
                //use flowfield
                CommandedEntity.Accelerate(FlowField.GetIntensity(CommandedEntity.Center, 1), Target.DistanceTo(CommandedEntity), game.Map);
            }
            else
            {
                //go in straight line
                Vector2 direction = CommandedEntity.Center.UnitDirectionTo(Target.Center);
                CommandedEntity.Accelerate(direction, Target.DistanceTo(CommandedEntity), game.Map);
            }

            //update last four positions
            NoMovementDetection.AddNextPosition(CommandedEntity.Center);

            //set that unit wants to move
            if(!CommandedEntity.WantsToMove)
                CommandedEntity.WantsToMove = true;
            
            //command is finished if unit reached the goal distance or if it was standing at one
            //place near the target position for a long time
            if (Finished() //unit is close to the target point
                || (NoMovementDetection.NotMovingMuch(CommandedEntity.MaxSpeedLand * deltaT / 2) && CanStop()))//unit is stuck
            {
                return true;
            }
            return false;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");

        /// <summary>
        /// Returns true iff the animal is close enough to the target.
        /// </summary>
        public bool Finished()
        {
             return Target.DistanceTo(CommandedEntity) <= GoalDistance;
        }

        /// <summary>
        /// Returns true if the unit should stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return Target.DistanceTo(CommandedEntity) < MinStoppingDistance;
        }

        /// <summary>
        /// Removes CommandedEntity from CommandAssignment.
        /// </summary>
        public void RemoveFromAssignment()
        {
            Assignment.Animals.Remove(CommandedEntity);
        }

        public override int Progress => 100;

        public override void OnRemove()
        {
            CommandedEntity.WantsToMove = false;
            CommandedEntity.SetAnimation("IDLE");
            RemoveFromAssignment();
        }
    }

    /// <summary>
    /// Detects if the animal is not moving much from positions this animal has recently
    /// been at.
    /// </summary>
    class NoMovementDetection
    {
        /// <summary>
        /// Last 4 positions of the entity.
        /// </summary>
        private readonly Vector2?[] last4positions;

        public NoMovementDetection()
        {
            last4positions = new Vector2?[4];
        }

        /// <summary>
        /// Adds a next position.
        /// </summary>
        /// <param name="v"></param>
        public void AddNextPosition(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        /// <summary>
        /// Returns true if the animal hasn't moved at least minDistSum in last 3 moves.
        /// </summary>
        public bool NotMovingMuch(float minDistSum)
        {
            if (last4positions[0] != null &&
                last4positions[1] != null &&
                last4positions[2] != null &&
                last4positions[3] != null)
            {
                float d1 = (last4positions[0] - last4positions[1]).Value.Length;
                float d2 = (last4positions[1] - last4positions[2]).Value.Length;
                float d3 = (last4positions[2] - last4positions[3]).Value.Length;

                if (d1 + d2 + d3 < minDistSum)
                    return true;
            }
            return false;
        }
    }

    interface IComputable
    {
        MoveToCommandAssignment Assignment { get; set; }
    }
}
