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
    public abstract class Entity: ITargetable, IMovementTarget, IRectangle, IShowable
    {
        public virtual Vector2 Center { get; }
        public abstract float Range { get; }//range of the circle collider
        public float ViewRange { get; set; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public CommandQueue CommandQueue { get; }
        public View View => new View(Center, ViewRange);
        public Player Player { get; }
        public AnimationState AnimationState { get; set; }
        public bool IsDead => Health <= 0;
        public DecRange Health { get; set; }
        public DecRange Energy { get; set; }
        public string EntityType { get; }
        public decimal MaxHealth { get; set; }
        public decimal MaxEnergy { get; set; }
        public bool Physical { get; set; }
        public List<Ability> Abilities { get; }
        public List<Status> Statuses { get; }
        string IShowable.GetName => EntityType;
        string IShowable.Description() => "Represents an object in the game.";
        public abstract List<Stat> Stats();

        public Entity(Player player, string entityType, decimal maxHealth, float viewRange, decimal maxEnergy, bool physical, List<Ability> abilities)
        {
            Player = player;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new CommandQueue();
            EntityType = entityType;
            MaxHealth = maxHealth;
            Health = new DecRange(maxHealth, maxHealth);
            MaxEnergy = maxEnergy;
            if (this is Animal)
                Energy = new DecRange(maxEnergy,maxEnergy);
            else
                Energy = new DecRange(maxEnergy, maxEnergy);
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(entityType));
            Physical = physical;
            Abilities = abilities;
            Statuses = new List<Status>();
        }

        public void PerformCommand(Game game, float deltaT)
        {
            if(CommandQueue.Any())
            {
                Command command = CommandQueue.First();
                if (command.PerformCommand(game, deltaT))
                {
                    //if command is finished, remove it from the queue
                    if(command is MoveToCommand)
                    {
                        ((MoveToCommand)command).RemoveFromAssignment();
                    }
                    CommandQueue.Dequeue();
                }
            }
        }

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

        /// <summary>
        /// Deals damage to the entity, equal to the damage.
        /// </summary>
        public virtual void Damage(decimal damage)
        {
            //only damage entity if the damage is positive
            if(damage > 0)
                Health -= damage;
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

        public void RemoveCommand(Command command)
        {
            if (!IsDead)
                CommandQueue.RemoveCommand(command);
        }

        public void ResetCommands()
        {
            CommandQueue.Clear();
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
        /// Distance between closest parts of the entities.
        /// </summary>
        public float DistanceTo(Node n)
        {
            return (this.Center - n.Center).Length - this.Range - 0.5f;
        }

        /// <summary>
        /// Returns true if the entity is visible.
        /// </summary>
        public abstract bool IsVisible(VisibilityMap visibilityMap);

        /// <summary>
        /// Called after entity dies.
        /// </summary>
        public virtual void Die()
        {
            RemoveFromAllCommandsAssignments();
        }

        float IMovementTarget.DistanceTo(Animal animal)
        {
            return (animal.Position - Center).Length - animal.Range - Range;
        }
    }

    public class CommandQueue:IEnumerable<Command>
    {
        public List<Command> Queue { get; }

        public CommandQueue()
        {
            Queue = new List<Command>();
        }

        public void Enqueue(Command command)
        {
            Queue.Add(command);
        }

        public Command First()
        {
            if (Queue.Any())
                return Queue[0];
            else
                return null;
        }

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
                foreach(Command c in Queue)
                    if (c != first)
                        c.OnRemove();
                Queue.RemoveAll(comm => comm != first);
            }
            else
            {
                foreach (Command c in Queue)
                    c.OnRemove();
                Queue.Clear();
            }
        }

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

        public List<Command> ToList() => Queue.ToList();
        public bool Any() => Queue.Any();

        public IEnumerator<Command> GetEnumerator()
        {
            return Queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }
}
