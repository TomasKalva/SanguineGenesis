using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GUI;

namespace wpfTest
{
    public abstract class Entity:ITargetable
    {
        public virtual Vector2 Center { get; }
        public float Size => 2 * Range;
        public abstract float Range { get; }//range of the circle collider
        public float ViewRange { get; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
        public View View => new View(Center, ViewRange);
        public Players Owner { get; }
        public EntityType UnitType { get; }
        public AnimationState AnimationState { get; set; }
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool IsDead => Health <= 0;
        public List<Ability> Abilities { get; }

        public Entity(Players owner, EntityType unitType, float maxHealth, float viewRange=6.0f)
        {
            Owner = owner;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new Queue<Command>();
            UnitType = unitType;
            MaxHealth = maxHealth;
            Health = maxHealth;
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(unitType));
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
                        ((MoveToPointCommand)command).RemoveFromCreator();
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
            Animation anim = atlas.GetAnimation(UnitType);
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
            Unit u = e as Unit;
            if (u != null)
                return (this.Center - u.Center).Length - this.Range - u.Range;
            else
                throw new NotImplementedException();
        }
    }

    public enum EntityType
    {
        TIGER,
        BAOBAB
    }

    public enum Movement
    {
        GROUND,
        WATER,
        GROUND_WATER
    }
}
