﻿using SanguineGenesis.GameLogic.Data.Entities;
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
    class MoveTo : Ability<Animal, IMovementTarget>
    {
        internal MoveTo(float? goalDistance, bool attackEnemyInstead)
            : base(null, 0, false, false)
        {
            GoalDistance = goalDistance;
            AttackEnemyInstead = attackEnemyInstead;
        }
        
        /// <summary>
        /// Null if attack distance of commanded entity should be used.
        /// </summary>
        public float? GoalDistance { get; }
        public bool AttackEnemyInstead { get; }

        public override void SetCommands(IEnumerable<Animal> users, IMovementTarget target, bool resetCommandQueue, ActionLog actionLog)
        {
            //if there are no users do nothing
            if (!users.Any())
                return;

            if (resetCommandQueue)
            {
                //reset all commands
                foreach (Animal c in users)
                {
                    c.ResetCommands();
                    if (c.CommandQueue.Any())
                        actionLog.LogError(c, this, $"user has unremovable commands");
                }
            }

            //player whose animals are receiving commands
            FactionType player = users.First().Faction.FactionID;

            //separete animals to different groups by their movement
            var usersMovementGroups = users.ToLookup((animal) => animal.Movement);
            foreach (var group in usersMovementGroups)
            {
                IEnumerable<Animal> usersMov = group;
                Movement m = group.Key;

                //set commands only if some animal can receive it
                if (!usersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, usersMov.Cast<Animal>().ToList(), m, target);
                //give command to each user and set the command's creator
                foreach (Animal user in usersMov)
                {
                    MoveToCommand com = new MoveToCommand(user, target, this, mtca);
                    user.AddCommand(com);
                }

                //enqueue mtca for computation
                MovementGenerator.GetMovementGenerator.AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Animal user, IMovementTarget target)
        {
            throw new NotImplementedException("This method shouldn't be used because the virtual method " + nameof(SetCommands) + " was overriden");
        }

        public override string GetName() => AttackEnemyInstead?"MOVE_TO":"UNBR_MOVE_TO";

        public override string Description()
        {
            return "The animal moves to the target. " + 
                (AttackEnemyInstead ? "If animal meets an enemy it attacks it instead." : "");
        }
    }


    class MoveToCommand : Command<Animal, IMovementTarget, MoveTo>
    {
        /// <summary>
        /// Assignment for generating flowfield in other thread.
        /// </summary>
        public MoveToCommandAssignment Assignment { get; }
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
        private float MinNoMovement { get; }
        /// <summary>
        /// Detects if the animal got stuck.
        /// </summary>
        private NoMovementDetection NoMovementDetection { get; }

        public MoveToCommand(Animal commandedEntity, IMovementTarget target, MoveTo ability, MoveToCommandAssignment moveToCommandAssignment)
            : base(commandedEntity, target, ability)
        {
            Assignment = moveToCommandAssignment;
            //calculate minimal distance at which animal can stop if it gets stuck
            //R is radius of circle with area equal to sum of areas of circles of animals
            //ra_1^2*pi + ra_2^2*pi + ... + ra_n^2*pi = R^2*pi => R = sqrt(ra_1^2 + ra_2^2 + ... + ra_n^2) = sqrt(volumeNoPI)
            float volumeNoPI = Assignment.Animals.Select((e) => e.Radius * e.Radius).Sum();
            float R = (float)Math.Sqrt(volumeNoPI);
            MinNoMovement = R * 1.3f;
            NoMovementDetection = new NoMovementDetection();
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            //set the command assignment to be active to increase its priority
            if (!Assignment.Active)
                Assignment.Active = true;

            //if an enemy animal is in attack range, attack it instead of other commands
            if (AttackEnemyInstead(game))
                return false;//new command is already set

            //check if the map was set yet
            if (FlowField == null)
                return false;

            //accelerate animal
            Vector2 animalPos = CommandedEntity.Position;
            Building blockingBuilding;
            //check if the animal is stuck on node with building (other node than node where its target is)
            if ((blockingBuilding = game.Map[(int)animalPos.X, (int)animalPos.Y].Building) != null
                && game.Map[(int)animalPos.X, (int)animalPos.Y] != game.Map[(int)Target.Center.X, (int)Target.Center.Y]
                && blockingBuilding != Target
                && blockingBuilding.Physical)
            {
                //go outside of node with building to be able to use flowfield
                CommandedEntity.SetVelocity(blockingBuilding.Center.UnitDirectionTo(animalPos),1000, game.Map);
            }
            else
            {
                //use flowfield
                CommandedEntity.SetVelocity(FlowField.GetDirection(CommandedEntity.Center), Target.DistanceTo(CommandedEntity), game.Map);
            }

            //set correct animation
            if (CommandedEntity.AnimationState.Animation.Action != "RUNNING")
                CommandedEntity.SetAnimation("RUNNING");

            //set that unit wants to move
            if (!CommandedEntity.WantsToMove)
                CommandedEntity.WantsToMove = true;

            //update last four positions
            NoMovementDetection.AddNextPosition(CommandedEntity.Center);
            
            //command is finished if animal reached the goal distance or if it was standing at one
            //place near the target position for a long time
            if (Finished() //animal is close to the target point
                || NoMovement(deltaT))//animal is stuck
            {
                return true;
            }
            return false;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");

        /// <summary>
        /// If an enemy animal is in attack range, attack it instead of other commands. Returns
        /// true if attack command was set.
        /// </summary>
        private bool AttackEnemyInstead(Game game)
        {
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
                    OnRemove();
                    //attack the enemy
                    CommandedEntity.SetCommand(new AttackCommand(CommandedEntity, enemy, game.GameData.Abilities.Attack));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the animal is close enough to the target.
        /// </summary>
        public bool Finished()
        {
             return Target.DistanceTo(CommandedEntity) <= GoalDistance;
        }

        /// <summary>
        /// Returns true if the animal is moving to a point, is close to it
        /// and it hasn't moved much lately.
        /// </summary>
        public bool NoMovement(float deltaT)
        {
            float speed;
            if (Assignment.Movement == Movement.WATER)
                speed = CommandedEntity.MaxSpeedWater;
            else
                speed = CommandedEntity.MaxSpeedLand;
            return (Target is Vector2) &&
                    NoMovementDetection.NotMovingMuch(speed * deltaT / 2) && CanStop();
        }

        /// <summary>
        /// Returns true if the animal should stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return Target.DistanceTo(CommandedEntity) < MinNoMovement;
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
            base.OnRemove();
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
        public void AddNextPosition(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        /// <summary>
        /// Returns true if the animal hasn't moved at least minDistSum in last 3 moves.
        /// Four last positions have to be defined.
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
}
