using System;
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
        public string EntityType { get; }
        public AnimationState AnimationState { get; set; }
        public decimal MaxHealth { get; set; }
        public decimal Health { get; set; }
        /// <summary>
        /// True iff this entity uses energy.
        /// </summary>
        public bool HasEnergy => MaxEnergy > 0;
        public decimal MaxEnergy { get; set; }
        public decimal Energy { get; set; }
        public bool IsDead => Health <= 0;
        public List<Ability> Abilities { get; }

        public Entity(Player player, string entityType, decimal maxHealth, float viewRange, decimal maxEnergy, List<Ability> abilities)
        {
            Player = player;
            ViewRange = viewRange;
            Group = null;
            CommandQueue = new Queue<Command>();
            EntityType = entityType;
            MaxHealth = maxHealth;
            Health = maxHealth;
            MaxEnergy = maxEnergy;
            if (this is Unit)
                Energy = maxEnergy;
            else
                Energy = 0;
            AnimationState = new AnimationState(ImageAtlas.GetImageAtlas.GetAnimation(entityType));
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
    /*
    public enum string
    {
        TIGER,
        BAOBAB,
        RHODE_GRASS,
        CANDELABRA,        ACACIA,
        KIWI_TREE,
        WATER_LETTUCE,
        PANDANI,
        WHISTLING_THORN,
        EUCALYPTUS,
        MIAMBO,
        SPIKE_THORN,
        ELEPHANT_GRASS,
        JACKAL_BERRY_TREE,
        KAPOC,
        BANNANA_TREE,
        COCONUT_TREE,
        CANNONBALL_TREE,
        BAMBOO,
        TEAK,
        ASHOKA,
        WALKING_PALM,
        PITCHER,
        CYCAD,
        CYCAD_PALM,
        TYRANNOSAUR_TREE,
        FERN,
        MANGROVE,
        WATER_LILY,
        ALGAE,
        SWORD_PLANT,
        ZEBRA, 
        GAZZELE, 
        RHINO, 	
        GIRAFFE, 		
        HIPPO, 		
        TURTLE, 		
        MEERCAT, 		
        TASMANIAN_DEVIL, 		
        KANGAROO, 		
        HYENA, 		
        CHEETAH, 		
        TARANTULA, 		
        INDIAN_ELEPHANT, 		
        ELEPHANT, 		
        CHIMPANZEE, 		
        GORILLA, 		
        PANDA, 
        OCELOT, 		
        RAPTOR, 		
        SAUROPOD, 		
        TYRANNOSAUR, 		
        ANACONDA, 		
        COBRA, 		
        ALIGATOR, 		
        CROCODILE, 		
        PUMA, 		
        DODO, 		
        TRICERATOPS, 		
        BABOON
    }*/
    /*
    public static class stringExtensions
    {
        private static Dictionary<string, bool> isUnit;

        static stringExtensions()
        {
            isUnit = new Dictionary<string, bool>()
            {
                {string.TIGER, true },
                {string.BAOBAB, false },
                {string.RHODE_GRASS, false },
                {string.CANDELABRA, false },
                {string.ACACIA, false },
                {string.KIWI_TREE, false },
                {string.WATER_LETTUCE, false },
                {string.PANDANI, false },
                {string.WHISTLING_THORN, false },
                {string.EUCALYPTUS, false },
                {string.MIAMBO, false },
                {string.SPIKE_THORN, false },
                {string.ELEPHANT_GRASS, false },
                {string.JACKAL_BERRY_TREE, false },
                {string.KAPOC, false },
                {string.BANNANA_TREE, false },
                {string.COCONUT_TREE, false },
                {string.CANNONBALL_TREE, false },
                {string.BAMBOO, false },
                {string.TEAK, false },
                {string.ASHOKA, false },
                {string.WALKING_PALM, false },
                {string.PITCHER, false },
                {string.CYCAD, false },
                {string.CYCAD_PALM, false },
                {string.TYRANNOSAUR_TREE, false },
                {string.FERN, false },
                {string.MANGROVE, false },
                {string.WATER_LILY, false },
                {string.ALGAE, false },
                {string.SWORD_PLANT, false },
                {string.ZEBRA, true },
                {string.GAZZELE, true },
                {string.RHINO, true },
                {string.GIRAFFE, true },
                {string.HIPPO, true },
                {string.TURTLE, true },
                {string.MEERCAT, true },
                {string.TASMANIAN_DEVIL, true },
                {string.KANGAROO, true },
                {string.HYENA, true },
                {string.CHEETAH, true },
                {string.TARANTULA, true },
                {string.INDIAN_ELEPHANT, true },
                {string.ELEPHANT, true },
                {string.CHIMPANZEE, true },
                {string.GORILLA, true },
                {string.PANDA, true },
                {string.OCELOT, true },
                {string.RAPTOR, true },
                {string.SAUROPOD, true },
                {string.TYRANNOSAUR, true },
                {string.ANACONDA, true },
                {string.ALIGATOR, true },
                {string.CROCODILE, true },
                {string.PUMA, true },
                {string.DODO, true },
                {string.TRICERATOPS, true },
                {string.BABOON, true }
            };
        }

        /// <summary>
        /// Returns true iff the entity type is unit.
        /// </summary>
        public static bool Unit(this string type) => isUnit[type];

        /// <summary>
        /// Returns true iff the entity type is unit.
        /// </summary>
        public static bool Building(this string type) => !isUnit[type];

        /// <summary>
        /// Returns all strings representing units.
        /// </summary>
        public static IEnumerable<string> Units
            => isUnit.Where((type) => type.Value).Select((type)=>type.Key);
        /// <summary>
        /// Returns all strings representing buildings.
        /// </summary>
        public static IEnumerable<string> Buildings
            => isUnit.Where((type) => !type.Value).Select((type) => type.Key);
    }*/

    public enum Movement
    {
        LAND,
        WATER,
        LAND_WATER
    }
}
