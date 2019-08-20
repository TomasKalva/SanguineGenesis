using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
using wpfTest.GameLogic.Maps;
using wpfTest.GUI;

namespace wpfTest
{
    public abstract class Entity: ITargetable, IMovementTarget, IRectangle
    {
        public virtual Vector2 Center { get; }
        public abstract float Range { get; }//range of the circle collider
        public float ViewRange { get; set; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
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
        /// <summary>
        /// True iff the entity can be used as a target for ability. 
        /// </summary>
        public bool CanBeTarget { get; set; }
        public List<Ability> Abilities { get; }
        public List<Status> Statuses { get; }

        public Entity(Player player, string entityType, decimal maxHealth, float viewRange, decimal maxEnergy, bool physical, List<Ability> abilities)
        {
            Player = player;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new Queue<Command>();
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
            CanBeTarget = true;
            Abilities = abilities;
            Statuses = new List<Status>();
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

        public void AddStatus(Status status)
        {
            Statuses.Add(status);
            status.Added();
        }

        public void RemoveStatus(Status status)
        {
            Statuses.Add(status);
            status.Added();
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
    }

    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }
}
