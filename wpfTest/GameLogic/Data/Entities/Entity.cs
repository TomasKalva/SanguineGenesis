﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;

namespace wpfTest
{
    public abstract class Entity: ITargetable, IMovementTarget, IRectangle
    {
        public virtual Vector2 Center { get; }
        public abstract float Range { get; }//range of the circle collider
        public float ViewRange { get; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
        public View View => new View(Center, ViewRange);
        public Player Player { get; }
        public AnimationState AnimationState { get; set; }
        public bool IsDead => Health <= 0;
        public decimal Health { get; set; }
        public decimal Energy { get; set; }

        public string EntityType { get; }
        public decimal MaxHealth { get; set; }
        public decimal MaxEnergy { get; set; }
        public bool Physical { get; }
        public List<Ability> Abilities { get; }

        public Entity(Player player, string entityType, decimal maxHealth, float viewRange, decimal maxEnergy, bool physical, List<Ability> abilities)
        {
            Player = player;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new Queue<Command>();
            EntityType = entityType;
            MaxHealth = maxHealth;
            Health = maxHealth;
            MaxEnergy = maxEnergy;
            if (this is Animal)
                Energy = maxEnergy;
            else
                Energy = 0;
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(entityType));
            Physical = physical;
            Abilities = abilities;
        }

        public void PerformCommand(Game game, float deltaT)
        {
            if(CommandQueue.Any())
            {
                Command command = CommandQueue.Peek();
                if (command.PerformCommand(game, deltaT))
                {
                    //if command is finished, remove it from the queue
                    if(command is MoveToPointCommand)
                    {
                        ((MoveToPointCommand)command).RemoveFromAssignment();
                    }
                    CommandQueue.Dequeue();
                }
            }
        }

        public float GetActualBottom(float imageBottom)
            => Math.Min(Center.Y - Range, Center.Y - imageBottom);
        public float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Center.Y + Range, Center.Y - imageBottom + imageHeight);
        public float GetActualLeft(float imageLeft)
            => Math.Min(Center.X - Range, Center.X - imageLeft);
        public float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(Center.X + Range, Center.X - imageLeft + imageWidth);
        public Rect GetActualRect(ImageAtlas atlas)
        {
            Animation anim = atlas.GetAnimation(EntityType);
            return new Rect(
                Math.Min(Center.X - Range, Center.X - anim.LeftBottom.X),
                Math.Min(Center.Y - Range, Center.Y - anim.LeftBottom.Y),
                Math.Max(Center.X + Range, Center.X - anim.LeftBottom.X + anim.Width),
                Math.Max(Center.Y + Range, Center.Y - anim.LeftBottom.Y + anim.Height));
        }

        public float Left => Center.X - Range;
        public float Right => Center.X + Range;
        public float Bottom => Center.Y - Range;
        public float Top => Center.Y + Range;
        public float Width => Right - Left;
        public float Height => Top - Bottom;


        public void AddCommand(Command command)
        {
            if(!IsDead)
                CommandQueue.Enqueue(command);
        }

        public void SetCommand(Command command)
        {
            if (!IsDead)
            {
                //clear the queue
                RemoveFromAllCommandsAssignments();
                CommandQueue.Clear();

                //set new command
                CommandQueue.Enqueue(command);
            }
        }

        public void AnimationStep(float deltaT)
        {
            AnimationState.Step(deltaT);
        }

        /// <summary>
        /// Removes referece to this unit from all CommandsAssignments.
        /// </summary>
        public void RemoveFromAllCommandsAssignments()
        {
            foreach(Command c in CommandQueue)
            {
                //it is enough to remove unit from CommandAssignment because
                //there is no other reference to the Command other than this queue
                //c.RemoveFromCreator();
            }
            CommandQueue.Clear();
        }

        /// <summary>
        /// Distance between closest parts of the entities.
        /// </summary>

        public float DistanceTo(Entity e)
        {
            return (this.Center - e.Center).Length - this.Range - e.Range;
        }

        /// <summary>
        /// Returns true if the entity is visible.
        /// </summary>
        public abstract bool IsVisible(VisibilityMap visibilityMap);
    }

    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }
}