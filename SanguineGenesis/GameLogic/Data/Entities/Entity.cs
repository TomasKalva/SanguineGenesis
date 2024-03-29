﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsControls;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Represents an object on the map.
    /// </summary>
    abstract class Entity: ITargetable, IMovementTarget, IRectangle, IShowable, IStatusOwner
    {
        /// <summary>
        /// Name of the entity.
        /// </summary>
        public string EntityType { get; }
        /// <summary>
        /// If health reaches 0 the unit dies and is removed from the game.
        /// </summary>
        public FloatRange Health { get; set; }
        /// <summary>
        /// The entity will be removed from the game.
        /// </summary>
        public virtual bool IsDead => Health <= 0;
        /// <summary>
        /// Used for useing abilities.
        /// </summary>
        public FloatRange Energy { get; set; }
        /// <summary>
        /// Center of this entity on the map.
        /// </summary>
        public virtual Vector2 Center { get; }
        /// <summary>
        /// Radius of the circle collider.
        /// </summary>
        public abstract float Radius { get; }
        /// <summary>
        /// True if the entity collides with other physical entities.
        /// </summary>
        public bool Physical { get; set; }
        /// <summary>
        /// How far the unit sees.
        /// </summary>
        public float ViewRange { get; set; }
        /// <summary>
        /// Faction which owns this unit.
        /// </summary>
        public Faction Faction { get; }

        /// <summary>
        /// Represents parameters of this entity's view of the map. Can be used from
        /// other thread.
        /// </summary>
        public View View => new View(Center, ViewRange);
        /// <summary>
        /// Entity's current animation.
        /// </summary>
        public AnimationState AnimationState { get; set; }
        /// <summary>
        /// The entity is selected by the player and highlighted on the map.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Abilities this entity can use.
        /// </summary>
        public List<Ability> Abilities { get; }
        /// <summary>
        /// The commands this entity will perform.
        /// </summary>
        public CommandQueue CommandQueue { get; }
        /// <summary>
        /// Statuses that are affecting this entity.
        /// </summary>
        public List<Status> Statuses { get; }

        //entity extents
        public float Left => Center.X - Radius;
        public float Right => Center.X + Radius;
        public float Bottom => Center.Y - Radius;
        public float Top => Center.Y + Radius;
        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public Entity(Faction faction, string entityType, float maxHealth, float viewRange, float maxEnergy, bool physical, List<Ability> abilities)
        {
            Faction = faction;
            ViewRange = viewRange;
            Selected = false;
            CommandQueue = new CommandQueue();
            EntityType = entityType;
            Health = new FloatRange(maxHealth, maxHealth);
            //animals start with bonus energy
            if(this is Animal)
                Energy = new FloatRange(maxEnergy, maxEnergy/2);
            else
                Energy = new FloatRange(maxEnergy, 0);
            SetAnimation("IDLE");
            Physical = physical;
            Abilities = abilities;
            Statuses = new List<Status>();
        }
        
        #region Animation
        public float GetActualBottom(float imageBottom)
            => Math.Min(Center.Y - Radius, Center.Y - imageBottom);
        public float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Center.Y + Radius, Center.Y - imageBottom + imageHeight);
        public float GetActualLeft(float imageLeft)
            => Math.Min(Center.X - Radius, Center.X - imageLeft);
        public float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(Center.X + Radius, Center.X - imageLeft + imageWidth);
        public Rect GetActualRect()
        {
            Animation anim = AnimationState.Animation;
            return new Rect(
                Math.Min(Center.X - Radius, Center.X - anim.Center.X),
                Math.Min(Center.Y - Radius, Center.Y - anim.Center.Y),
                Math.Max(Center.X + Radius, Center.X - anim.Center.X + anim.Width),
                Math.Max(Center.Y + Radius, Center.Y - anim.Center.Y + anim.Height));
        }

        /// <summary>
        /// Perform one step of the animation.
        /// </summary>
        public void AnimationStep(float deltaT)
        {
            AnimationState.Step(deltaT);
        }

        /// <summary>
        /// Sets animation of corresponding action to this entity.
        /// </summary>
        public void SetAnimation(string action)
        {
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetEntityAnimation(EntityType, action));
        }
        #endregion Animation
        
        #region Commands

        /// <summary>
        /// Perform the first command in the command queue, if it exits. If the
        /// command finished, move to the next command in the queue.
        /// </summary>
        public void PerformCommand(Game game, float deltaT)
        {
            if (CommandQueue.Any())
            {
                Command command = CommandQueue.First();
                if (command.PerformCommand(game, deltaT))
                {
                    //if command is finished, remove it from the queue
                    CommandQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Adds command to this entity. If the entity is dead or it is animal and
        /// its state is locked, the command is not added. 
        /// </summary>
        public void AddCommand(Command command)
        {

            if (!IsDead) 
            { 
                if(this is Animal a)
                {
                    if(a.StateChangeLock == null)
                        CommandQueue.Enqueue(command);
                }
                else
                {
                    CommandQueue.Enqueue(command);
                }
            }
        }

        /// <summary>
        /// Remove all commands from this entity and then add command. Works only if the entity is not dead.
        /// </summary>
        public void SetCommand(Command command)
        {
            if (!IsDead)
            {
                //clear the queue
                ResetCommands();

                //set new command
                CommandQueue.Enqueue(command);
            }
        }

        /// <summary>
        /// Remove command from this entity.
        /// </summary>
        public void RemoveCommand(Command command)
        {
            CommandQueue.RemoveCommand(command);
        }

        /// <summary>
        /// Removes all commands from this entity.
        /// </summary>
        public void ResetCommands()
        {
            CommandQueue.Clear();
        }

        #endregion Commands

        #region Statuses

        /// <summary>
        /// Removes all statuses from this entity.
        /// </summary>
        public void ResetStatuses()
        {
            foreach (var s in Statuses)
            {
                s.OnRemove();
            }
            Statuses.Clear();
        }

        public void AddStatus(Status status)
        {
            Statuses.Add(status);
            status.OnAdd();
        }

        public void RemoveStatus(Status status)
        {
            Statuses.Remove(status);
            status.OnRemove();
        }

        /// <summary>
        /// Perfrom step of all statuses. Remove statuses that finished.
        /// </summary>
        public void StepStatuses(Game game, float deltaT)
        {
            var toRemove = new List<Status>();
            foreach (Status s in Statuses)
                if (s.Step(game, deltaT))
                {
                    //status is finished
                    toRemove.Add(s);
                    s.OnRemove();
                }
            //remove all finished statuses
            Statuses.RemoveAll((s) => toRemove.Contains(s));
        }

        #endregion Statuses
        
        /// <summary>
        /// Deals damage to the entity, equal to the damage.
        /// </summary>
        protected void Damage(float damage)
        {
            //only damage entity if the damage is positive
            if (damage > 0)
                Health -= damage;
        }

        /// <summary>
        /// Deals damage to the entity. physical should be true if the damage
        /// is dealth physical (by being hit). Non-physical damage is poison,
        /// bleeding, ...
        /// </summary>
        public virtual void Damage(float damage, bool physical)
        {
            Damage(damage);
        }

        /// <summary>
        /// Called after this entity dies. Can add a new Structure to neutral faction but only from
        /// not neutral faction.
        /// </summary>
        public virtual void Die(Game game)
        {
            //reset commands - some commands might require entity to be unregistered (e.g. MoveTo)
            ResetCommands();
            //reset statuses
            ResetStatuses();
        }

        /// <summary>
        /// Distance between closest parts of the entities.
        /// </summary>
        public float DistanceTo(Entity e)
        {
            return (this.Center - e.Center).Length - this.Radius - e.Radius;
        }

        /// <summary>
        /// Distance between node n and this entity.
        /// </summary>
        public float DistanceTo(Node n)
        {
            return (this.Center - n.Center).Length - this.Radius - 0.5f;
        }

        float ITargetable.DistanceTo(Entity entity)
        {
            return DistanceTo(entity);
        }

        #region IShowable
        string IShowable.GetName() => EntityType;
        string IShowable.Description() => "Represents an object in the game.";
        public abstract List<Stat> Stats();
        #endregion IShowable
    }

    /// <summary>
    /// Implements logic of command queue for an entity.
    /// </summary>
    class CommandQueue:IEnumerable<Command>
    {
        public List<Command> Queue { get; }

        public CommandQueue()
        {
            Queue = new List<Command>();
        }

        /// <summary>
        /// Adds command to the end of the queue.
        /// </summary>
        public void Enqueue(Command command)
        {
            Queue.Add(command);
        }

        /// <summary>
        /// Returns the first command in the queue, or null if it doesn't exist.
        /// </summary>
        public Command First()
        {
            if (Queue.Any())
                return Queue[0];
            else
                return null;
        }

        /// <summary>
        /// Removes first command in the queue. Calls the command's OnRemove method.
        /// </summary>
        public void Dequeue()
        {
            Command first = Queue.FirstOrDefault();
            Remove(first);
        }

        /// <summary>
        /// Removes all commands. Leaves the first one if it is not interruptable.
        /// </summary>
        public void Clear()
        {
            Command first = First();
            if (first != null && !first.Interruptable)
            {
                //first command isn't interruptable, remove all
                //the other commands
                foreach(Command c in Queue)
                    if (c != first)
                        c.OnRemove();
                Queue.RemoveAll(comm => comm != first);
            }
            else
            {
                //first command is interruptable, remove
                //all the commands
                foreach (Command c in Queue)
                    c.OnRemove();
                Queue.Clear();
            }
        }

        /// <summary>
        /// Remove command from the queue if it can be removed.
        /// </summary>
        public void RemoveCommand(Command command)
        {
            //don't remove command that is first and uninterruptable
            if (command != null &&
                command == Queue.FirstOrDefault() &&
                !command.Interruptable)
                return;

            Remove(command);
        }

        /// <summary>
        /// Removes this command from the Queue.
        /// </summary>
        private void Remove(Command command)
        {
            Queue.Remove(command);
            if (command != null)
                command.OnRemove();
        }

        /// <summary>
        /// Return the command queue as a list.
        /// </summary>
        public List<Command> ToList() => Queue.ToList();

        /// <summary>
        /// True if the queue is not empty.
        /// </summary>
        public bool Any() => Queue.Any();

        /// <summary>
        /// Returns number of elements in this command queue.
        /// </summary>
        public int Count => Queue.Count;

        /// <summary>
        /// Returns commands in the queue from first to the last.
        /// </summary>
        public IEnumerator<Command> GetEnumerator()
        {
            return Queue.GetEnumerator();
        }

        /// <summary>
        /// Returns commands in the queue from first to the last.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
