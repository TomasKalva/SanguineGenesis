using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GUI;

namespace wpfTest
{
    public abstract class Entity
    {
        public abstract Vector2 Pos { get; set; }
        public abstract float Size { get; }
        public float ViewRange { get; }//how far the unit sees
        public CommandsGroup Group { get; set; }
        public Queue<Command> CommandQueue { get; }
        public abstract UnitView UnitView { get; }
        public Players Owner { get; }
        public EntityType UnitType { get; }
        public AnimationState AnimationState { get; set; }
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool IsDead => Health <= 0;
        public List<AbilityType> Abilities { get; }

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
            Abilities = new List<AbilityType>();
        }

        public void PerformCommand(Game game, float deltaT)
        {
            if(CommandQueue.Any())
            {
                Command command = CommandQueue.Peek();
                if (command.PerformCommand(game, deltaT))
                {
                    //if command is finished, remove it from the queue
                    if(command.Creator!=null)
                        command.Creator.Entities.Remove(this);
                    CommandQueue.Dequeue();
                }
            }
        }
        
        public abstract float GetActualBottom(float imageBottom);
        public abstract float GetActualTop(float imageHeight, float imageBottom);
        public abstract float GetActualLeft(float imageLeft);
        public abstract float GetActualRight(float imageWidth, float imageLeft);
        public abstract Rect GetActualRect(ImageAtlas atlas);


        public abstract float Left { get; }
        public abstract float Right { get; }
        public abstract float Bottom { get; }
        public abstract float Top { get; }


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
                c.RemoveFromCreator();
            }
            CommandQueue.Clear();
        }

        /// <summary>
        /// Distance between closest parts of the entities.
        /// </summary>
        public abstract float DistanceTo(Entity u);
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
