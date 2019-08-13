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
        public EntityType EntityType { get; }
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

        public Entity(Player player, EntityType entityType, decimal maxHealth, float viewRange, decimal maxEnergy, List<Ability> abilities)
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

    public enum EntityType
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
    }

    public static class EntityTypeExtensions
    {
        private static Dictionary<EntityType, bool> isUnit;

        static EntityTypeExtensions()
        {
            isUnit = new Dictionary<EntityType, bool>()
            {
                {EntityType.TIGER, true },
                {EntityType.BAOBAB, false },
                {EntityType.RHODE_GRASS, false },
                {EntityType.CANDELABRA, false },
                {EntityType.ACACIA, false },
                {EntityType.KIWI_TREE, false },
                {EntityType.WATER_LETTUCE, false },
                {EntityType.PANDANI, false },
                {EntityType.WHISTLING_THORN, false },
                {EntityType.EUCALYPTUS, false },
                {EntityType.MIAMBO, false },
                {EntityType.SPIKE_THORN, false },
                {EntityType.ELEPHANT_GRASS, false },
                {EntityType.JACKAL_BERRY_TREE, false },
                {EntityType.KAPOC, false },
                {EntityType.BANNANA_TREE, false },
                {EntityType.COCONUT_TREE, false },
                {EntityType.CANNONBALL_TREE, false },
                {EntityType.BAMBOO, false },
                {EntityType.TEAK, false },
                {EntityType.ASHOKA, false },
                {EntityType.WALKING_PALM, false },
                {EntityType.PITCHER, false },
                {EntityType.CYCAD, false },
                {EntityType.CYCAD_PALM, false },
                {EntityType.TYRANNOSAUR_TREE, false },
                {EntityType.FERN, false },
                {EntityType.MANGROVE, false },
                {EntityType.WATER_LILY, false },
                {EntityType.ALGAE, false },
                {EntityType.SWORD_PLANT, false },
                {EntityType.ZEBRA, true },
                {EntityType.GAZZELE, true },
                {EntityType.RHINO, true },
                {EntityType.GIRAFFE, true },
                {EntityType.HIPPO, true },
                {EntityType.TURTLE, true },
                {EntityType.MEERCAT, true },
                {EntityType.TASMANIAN_DEVIL, true },
                {EntityType.KANGAROO, true },
                {EntityType.HYENA, true },
                {EntityType.CHEETAH, true },
                {EntityType.TARANTULA, true },
                {EntityType.INDIAN_ELEPHANT, true },
                {EntityType.ELEPHANT, true },
                {EntityType.CHIMPANZEE, true },
                {EntityType.GORILLA, true },
                {EntityType.PANDA, true },
                {EntityType.OCELOT, true },
                {EntityType.RAPTOR, true },
                {EntityType.SAUROPOD, true },
                {EntityType.TYRANNOSAUR, true },
                {EntityType.ANACONDA, true },
                {EntityType.ALIGATOR, true },
                {EntityType.CROCODILE, true },
                {EntityType.PUMA, true },
                {EntityType.DODO, true },
                {EntityType.TRICERATOPS, true },
                {EntityType.BABOON, true }
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
