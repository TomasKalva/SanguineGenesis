using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GUI;

namespace wpfTest
{
    public abstract class Entity: ITargetable, IMovementTarget
    {
        public virtual Vector2 Center { get; }
        public abstract float Range { get; }//range of the circle collider
        public float ViewRange { get; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
        public View View => new View(Center, ViewRange);
        public Player Player { get; }
        public EntityType EntityType { get; }
        public AnimationState AnimationState { get; set; }
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        /// <summary>
        /// True iff this entity uses energy.
        /// </summary>
        public bool HasEnergy => MaxEnergy > 0;
        public float MaxEnergy { get; set; }
        public float Energy { get; set; }
        public bool IsDead => Health <= 0;
        public List<Ability> Abilities { get; }

        public Entity(Player player, EntityType entityType, float maxHealth, float viewRange, float maxEnergy)
        {
            Player = player;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new Queue<Command>();
            EntityType = entityType;
            MaxHealth = maxHealth;
            Health = maxHealth;
            MaxEnergy = maxEnergy;
            Energy = maxEnergy;
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(entityType));
            Abilities = new List<Ability>();
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
    }

    public enum EntityType
    {
        TIGER,
        BAOBAB
    }

    public static class EntityTypeExtensions
    {
        private static Dictionary<EntityType, bool> isUnit;

        static EntityTypeExtensions()
        {
            isUnit = new Dictionary<EntityType, bool>()
            {
                {EntityType.TIGER, true },
                {EntityType.BAOBAB, false }
            };
        }

        /// <summary>
        /// Returns true iff the entity type is unit.
        /// </summary>
        public static bool Unit(this EntityType type) => isUnit[type];

        /// <summary>
        /// Returns true iff the entity type is unit.
        /// </summary>
        public static bool Building(this EntityType type) => !isUnit[type];

        /// <summary>
        /// Returns all EntityTypes representing units.
        /// </summary>
        public static IEnumerable<EntityType> Units
            => isUnit.Where((type) => type.Value).Select((type)=>type.Key);
        /// <summary>
        /// Returns all EntityTypes representing buildings.
        /// </summary>
        public static IEnumerable<EntityType> Buildings
            => isUnit.Where((type) => !type.Value).Select((type) => type.Key);
    }

    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }
}
