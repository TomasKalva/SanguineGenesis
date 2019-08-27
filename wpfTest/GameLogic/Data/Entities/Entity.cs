using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Abilities;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;
using static wpfTest.MainWindow;

namespace wpfTest
{
    /// <summary>
    /// Represents an object on the map.
    /// </summary>
    public abstract class Entity: ITargetable, IMovementTarget, IRectangle, IShowable
    {
        /// <summary>
        /// Center of this entity on the map.
        /// </summary>
        public virtual Vector2 Center { get; }
        /// <summary>
        /// Range of the circle collider.
        /// </summary>
        public abstract float Range { get; }
        /// <summary>
        /// How far the unit sees.
        /// </summary>
        public float ViewRange { get; set; }
        /// <summary>
        /// The entity is selected by the player and highlighted on the map.
        /// </summary>
        public bool Selected { get; set; }
        /// <summary>
        /// The commands this entity will perform.
        /// </summary>
        public CommandQueue CommandQueue { get; }
        /// <summary>
        /// Represents parameters of this entity's view of the map. Can be used from
        /// other thread.
        /// </summary>
        public View View => new View(Center, ViewRange);
        /// <summary>
        /// Player who owns this unit.
        /// </summary>
        public Player Player { get; }
        /// <summary>
        /// Entity's current animation.
        /// </summary>
        public AnimationState AnimationState { get; set; }
        /// <summary>
        /// The entity has no more health left.
        /// </summary>
        public bool IsDead => Health <= 0;
        /// <summary>
        /// If health reaches 0 the unit dies and is removed from the game.
        /// </summary>
        public DecRange Health { get; set; }
        /// <summary>
        /// Used for casting abilities.
        /// </summary>
        public DecRange Energy { get; set; }
        /// <summary>
        /// Name of the entity.
        /// </summary>
        public string EntityType { get; }
        /// <summary>
        /// True iff the entity collides with other physical entities.
        /// </summary>
        public bool Physical { get; set; }
        /// <summary>
        /// Abilities this entity can cast.
        /// </summary>
        public List<Ability> Abilities { get; }
        /// <summary>
        /// Statuses that are affecting this entity.
        /// </summary>
        public List<Status> Statuses { get; }

        //map extents
        public float Left => Center.X - Range;
        public float Right => Center.X + Range;
        public float Bottom => Center.Y - Range;
        public float Top => Center.Y + Range;
        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public Entity(Player player, string entityType, decimal maxHealth, float viewRange, decimal maxEnergy, bool physical, List<Ability> abilities)
        {
            Player = player;
            ViewRange = viewRange;
            Selected = false;
            CommandQueue = new CommandQueue();
            EntityType = entityType;
            Health = new DecRange(maxHealth, maxHealth);
            if (this is Animal)
                Energy = new DecRange(maxEnergy,maxEnergy);
            else
                Energy = new DecRange(maxEnergy, maxEnergy);
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(entityType));
            Physical = physical;
            Abilities = abilities;
            Statuses = new List<Status>();
        }
        
        #region Animation
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

        /// <summary>
        /// Perform one step of the animation.
        /// </summary>
        public void AnimationStep(float deltaT)
        {
            AnimationState.Step(deltaT);
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
                    if (command is MoveToCommand)
                    {
                        ((MoveToCommand)command).RemoveFromAssignment();
                    }
                    CommandQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Adds command to this entity. If the entity is dead, the command is not added. 
        /// </summary>
        public void AddCommand(Command command)
        {
            if(!IsDead)
                CommandQueue.Enqueue(command);
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
        /// Remove command from this entity. If the entity is dead, the command is not removed. 
        /// </summary>
        public void RemoveCommand(Command command)
        {
            if (!IsDead)
                CommandQueue.RemoveCommand(command);
        }

        /// <summary>
        /// Removes all commands from this entity. If the entity is dead, the commands are not reset.
        /// </summary>
        public void ResetCommands()
        {
            if (!IsDead)
            {
                CommandQueue.Clear();
            }
        }

        #endregion Commands

        #region Statuses

        public void AddStatus(Status status)
        {
            Statuses.Add(status);
            status.Added();
        }

        public void RemoveStatus(Status status)
        {
            Statuses.Remove(status);
            status.Removed();
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
                    s.Removed();
                }
            //remove all finished statuses
            Statuses.RemoveAll((s) => toRemove.Contains(s));
        }

        #endregion Statuses
        
        /// <summary>
        /// Deals damage to the entity, equal to the damage.
        /// </summary>
        public virtual void Damage(decimal damage)
        {
            //only damage entity if the damage is positive
            if (damage > 0)
                Health -= damage;
        }

        /// <summary>
        /// Distance between closest parts of the entities.
        /// </summary>
        public float DistanceTo(Entity e)
        {
            return (this.Center - e.Center).Length - this.Range - e.Range;
        }

        /// <summary>
        /// Distance between node n and this entity.
        /// </summary>
        public float DistanceTo(Node n)
        {
            return (this.Center - n.Center).Length - this.Range - 0.5f;
        }

        /// <summary>
        /// Returns true if the entity is visible on visibilityMap.
        /// </summary>
        public abstract bool IsVisible(VisibilityMap visibilityMap);

        /// <summary>
        /// Called after this entity dies.
        /// </summary>
        public virtual void Die()
        {
        }

        float IMovementTarget.DistanceTo(Animal animal)
        {
            return (animal.Position - Center).Length - animal.Range - Range;
        }

        #region IShowable
        string IShowable.GetName => EntityType;
        string IShowable.Description() => "Represents an object in the game.";
        public abstract List<Stat> Stats();
        #endregion IShowable
    }

    /// <summary>
    /// Implements logic of command queue for an entity.
    /// </summary>
    public class CommandQueue:IEnumerable<Command>
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
            Queue.Remove(first);
            if (first != null)
                first.OnRemove();
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

            Queue.Remove(command);
            if (command != null)
                command.OnRemove();
        }

        /// <summary>
        /// Return the command queue as a list.
        /// </summary>
        public List<Command> ToList() => Queue.ToList();
        /// <summary>
        /// True iff the queue is not empty.
        /// </summary>
        public bool Any() => Queue.Any();

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
