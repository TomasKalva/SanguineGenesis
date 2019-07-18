using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GUI;

namespace wpfTest
{
    public class Unit
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Range { get; }//range of the circle collider
        public float ViewRange { get; }//how far the unit sees
        public bool WantsToMove { get; set; }//true if the unit has a target destination
        public bool IsInCollision { get; set; }//true if the unit is colliding with obstacles or other units
        public float MaxSpeed { get; }
        public float Acceleration { get; }
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
        public UnitView UnitView => new UnitView(Pos, ViewRange);
        public Players Owner { get; }
        public UnitType UnitType { get; }
        public AnimationState AnimationState { get; set; }
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool HasEnergy { get; }//true if the unit uses energy
        public float MaxEnergy { get; set; }
        public float Energy { get; set; }
        public Vector2 Direction { get; set; }//direction the unit is facing
        public bool FacingLeft => Direction.X <= 0;
        public Movement Movement { get; }//where can the unit walk
        public float AttackDamage { get; }
        public float AttackPeriod { get; }
        public float AttackDistance { get; }
        public bool IsDead => Health <= 0;

        public Unit(Players owner, UnitType unitType, float maxHealth, float maxEnergy, Vector2 pos, Movement movement=Movement.GROUND, float range = 0.5f, float viewRange=6.0f, float maxSpeed=2f, float acceleration=4f,
            float attackDamage=10f, float attackPeriod=0.9f, float attackDistance=0.2f)
        {
            Owner = owner;
            Pos = pos;
            Vel = new Vector2(0f, 0f);
            Range = range;
            ViewRange = viewRange;
            IsInCollision = false;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            Group = null;
            CommandQueue = new Queue<Command>();
            UnitType = unitType;
            MaxHealth = maxHealth;
            Health = maxHealth;
            if (maxEnergy > 0)
                HasEnergy = true;
            MaxEnergy = maxEnergy;
            Energy = maxEnergy;
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(unitType));
            Direction = new Vector2(1f, 0f);
            Movement = movement;
            AttackDamage = attackDamage;
            AttackPeriod = attackPeriod;
            AttackDistance = attackDistance;
        }

        public void PerformCommand(Game game, float deltaT)
        {
            if(CommandQueue.Any())
            {
                Command command = CommandQueue.Peek();
                if (command.PerformCommand(game, deltaT))
                    //if command is finished, remove it from the queue
                    CommandQueue.Dequeue();
            }
        }

        public void Move(Map map, float deltaT)
        {
            Pos = new Vector2( 
                Math.Max(Range, Math.Min(Pos.X + deltaT * Vel.X,map.Width-Range)),
                Math.Max(Range, Math.Min(Pos.Y + deltaT * Vel.Y, map.Height-Range)));
            if(Vel.Length!=0)
                Direction = Vel;
        }
        
        public float GetActualBottom(float imageBottom)
            => Math.Min(Pos.Y - Range, Pos.Y - imageBottom);
        public float GetActualTop(float imageHeight, float imageBottom)
            => Math.Max(Pos.Y + Range, Pos.Y - imageBottom+imageHeight);
        public float GetActualLeft(float imageLeft)
            => Math.Min(Pos.X - Range, Pos.X - imageLeft);
        public float GetActualRight(float imageWidth, float imageLeft)
            => Math.Max(Pos.X + Range, Pos.X - imageLeft + imageWidth);
        public Rect GetActualRect(ImageAtlas atlas)
        {
            Animation anim = atlas.GetAnimation(UnitType);
            return new Rect(
                Math.Min(Pos.X - Range, Pos.X - anim.LeftBottom.X),
                Math.Min(Pos.Y - Range, Pos.Y - anim.LeftBottom.Y),
                Math.Max(Pos.X + Range, Pos.X - anim.LeftBottom.X + anim.Width),
                Math.Max(Pos.Y + Range, Pos.Y - anim.LeftBottom.Y + anim.Height));
        }

        public float Left => Pos.X - Range;
        public float Right => Pos.X + Range;
        public float Bottom => Pos.Y - Range;
        public float Top => Pos.Y + Range;

        public void Accelerate(Vector2 acc)
        {
            Vel += acc;
            float l= Vel.Length;
            if (l>MaxSpeed && l!=0)
                Vel = (MaxSpeed / l)*Vel;
        }

        public void AddCommand(Command command)
        {
            CommandQueue.Enqueue(command);
        }

        public void SetCommand(Command command)
        {
            CommandQueue.Clear();
            CommandQueue.Enqueue(command);
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
                c.Creator.Units.Remove(this);
            }
        }
    }

    public enum UnitType
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
