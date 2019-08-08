using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Represents a target on the map.
    /// </summary>
    public interface ITargetable
    {
        Vector2 Center { get; }
    }

    /// <summary>
    /// Place where unit can go to.
    /// </summary>
    public interface IMovementTarget:ITargetable
    {
    }

    public abstract class Ability
    {
        /// <summary>
        /// Maximal distance from the target where the ability can be cast.
        /// </summary>
        public float Distance { get; }
        /// <summary>
        /// Energy required to use this ability.
        /// </summary>
        public float EnergyCost { get; }
        /// <summary>
        /// Resource required to use this ability.
        /// </summary>
        public float ResourceCost { get; }
        /// <summary>
        /// True iff this ability should be performed only by one of the selected units.
        /// </summary>
        public bool OnlyOne { get; }

        public abstract void SetCommands(IEnumerable<Entity> casters, ITargetable target);
        /// <summary>
        /// Returns true if the target type is valid for this ability.
        /// </summary>
        public abstract bool ValidTargetType(ITargetable target);
        public abstract Type TargetType { get; }
        public abstract Command NewCommand(Entity caster, ITargetable target);
        /// <summary>
        /// Returns text which describes what the ability does.
        /// </summary>>
        public abstract string Description();
        public override string ToString()
        {
            return GetType().Name;
        }
        public Ability(float distance, float energyCost, float resourceCost, bool onlyOne)
        {
            Distance = distance;
            EnergyCost = energyCost;
            ResourceCost = resourceCost;
            OnlyOne = onlyOne;
        }
    }

    public abstract class TargetAbility<Caster, Target> : Ability where Caster:Entity 
                                                                    where Target: ITargetable
    {
        public TargetAbility(float distance, float energyCost, float resourceCost, bool onlyOne)
            :base(distance, energyCost, resourceCost, onlyOne)
        {
        }

        /// <summary>
        /// Calls generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If some casters or target have incompatible type.</exception>
        /// <exception cref="NullReferenceException">If some casters are null.</exception>
        public sealed override Command NewCommand(Entity caster, ITargetable target)
        {
            return NewCommand((Caster)caster, (Target)target);
        }

        public abstract Command NewCommand(Caster caster, Target target);

        /// <summary>
        /// Assigns commands to the units.
        /// </summary>
        public sealed override void SetCommands(IEnumerable<Entity> casters, ITargetable target)
        {
            SetCommands(casters.Cast<Caster>(), (Target)target);
        }
        
        public sealed override bool ValidTargetType(ITargetable target)
        {
            return target is Target;
        }

        public sealed override Type TargetType 
            => typeof(Target);

        public virtual void SetCommands(IEnumerable<Caster> casters, Target target)
        {
            //select only the casters who can pay and put the to list so 
            //they can be enumerated multiple times
            List<Caster> ableToPay = casters
                .Where((caster) =>(caster.Energy >= EnergyCost &&
                     caster.Player.Resource >= ResourceCost))
                 .ToList();
            
            
            //if there are no casters that can pay do nothing
            if (!ableToPay.Any())
                return;

            if (OnlyOne)
                ableToPay = ableToPay.Take(1).ToList();

            //move units to the target until the required distance is reached
            MoveTo.GetMoveTo(this)
                .SetCommands(ableToPay
                .Where(caster=>caster.GetType()==typeof(Unit))
                .Cast<Unit>(), target);

            //give command to each caster
            foreach (Caster c in ableToPay)
            {
                //create new command and assign it to c
                Command com = NewCommand(c, target);
                c.AddCommand(com);
            }
        }
    }

    public sealed class MoveTo: TargetAbility<Unit,IMovementTarget>,IMovementParametrizing
    {
        private static MoveTo ability;
        /// <summary>
        /// Movement parameters for each ability other than MoveTo.
        /// </summary>
        private static Dictionary<Ability, MoveTo> moveToCast;
        static MoveTo()
        {
            ability = new MoveTo(0.1f, true, false);
            //initialize MoveTo for all abilities
            {
                moveToCast = new Dictionary<Ability, MoveTo>();
                moveToCast.Add(Attack.Get, new MoveTo(-1, true, true));
                //spawn abilities
                foreach (EntityType unit in EntityTypeExtensions.Units)
                {
                    Ability a = Spawn.GetAbility(unit);
                    moveToCast.Add(a, new MoveTo(a.Distance, false, false));
                }
                //build abilities
                foreach (EntityType building in EntityTypeExtensions.Buildings)
                {
                    Ability a = PlantBuilding.GetAbility(building);
                    moveToCast.Add(a, new MoveTo(a.Distance, false, false));
                }
            }
        }
        private MoveTo(float goalDistance, bool interruptable, bool usesAttackDistance)
            :base(-1, 0, 0, false)
        {
            GoalDistance = goalDistance;
            Interruptable = interruptable;
            UsesAttackDistance = usesAttackDistance;
        }
        public static MoveTo Get => ability;
        public static MoveTo GetMoveTo(Ability a) => moveToCast[a];

        //interface IMovementParametrizing properties
        public float GoalDistance { get; }
        public bool Interruptable { get; }
        public bool UsesAttackDistance { get; }

        public override void SetCommands(IEnumerable<Unit> casters, IMovementTarget target)
        {
            //if there are no casters do nothing
            if (!casters.Any())
                return;
            //player whose units are receiving commands
            Players player = casters.First().Player.PlayerID;

            //separete units to different groups by their movement
            var castersGroups = casters.ToLookup((unit) => unit.Movement);

            //volume of all units' circles /pi
            float volume = casters.Select((e) => e.Range * e.Range).Sum();
            //distance from the target when unit can stop if it gets stuck
            float minStoppingDistance = (float)Math.Sqrt(volume) * 1.3f;

            foreach(Movement m in Enum.GetValues(typeof(Movement)))
            {
                IEnumerable<Unit> castersMov = castersGroups[m];
                //set commands only if any unit can receive it
                if (!castersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, castersMov.Cast<Unit>().ToList(), m, target);
                //give command to each caster and set the command's creator
                foreach (Unit caster in castersMov)
                {
                    IComputable com = new MoveToPointCommand(caster, target, minStoppingDistance, this);
                    com.Assignment = mtca;

                    caster.AddCommand((Command)com);
                }
                MovementGenerator.GetMovementGenerator().AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Unit caster, IMovementTarget target)
        {
            throw new NotImplementedException("This method is not necessary because the virtual method " + nameof(SetCommands) + " was overriden");
        }

        public override string Description()
        {
            return "The unit moves to the target. If the target is on a terrain " +
                "this unit can't move to, the unit won't do anything. If unit meets an enemy it attacks it instead.";
        }
    }

    public sealed class Attack : TargetAbility<Unit, Entity>
    {
        private static Attack ability;
        static Attack()
        {
            ability = new Attack();
        }
        private Attack():base(-1, 0, 0, false) { }
        public static Attack Get => ability;
        public override Command NewCommand(Unit caster, Entity target)
        {
            return new AttackCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The unit deals repeatedly its attack damage to the target.";
        }
    }

    public sealed class Spawn : TargetAbility<Entity, Vector2>
    {
        private static Dictionary<EntityType,Spawn> unitSpawningAbilities;
        static Spawn()
        {
            unitSpawningAbilities = new Dictionary<EntityType, Spawn>()
            {
                { EntityType.TIGER, new Spawn(new UnitFactory(EntityType.TIGER, 0.5f, 2f, 4f, 50f, 20f, Movement.LAND_WATER,4f),0,50)}
            };
        }
        private Spawn(UnitFactory spawningUnitFactory, float energyCost, float resourceCost)
            : base(2 * spawningUnitFactory.Range, energyCost, resourceCost, true)
        {
            SpawningUnitFactory = spawningUnitFactory;
        }
        public static Spawn GetAbility(EntityType t) => unitSpawningAbilities[t];
        
        public UnitFactory SpawningUnitFactory { get; }

        public override Command NewCommand(Entity caster, Vector2 target)
        {
            return new SpawnCommand(caster, target, this, SpawningUnitFactory.UnitType);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningUnitFactory.UnitType;
        }

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }


    public sealed class PlantBuilding : TargetAbility<Entity, Node>
    {
        private static Dictionary<EntityType, PlantBuilding> plantingBuildingAbilities;
        static PlantBuilding()
        {
            plantingBuildingAbilities = new Dictionary<EntityType, PlantBuilding>()
            {
                { EntityType.BAOBAB, new PlantBuilding(new TreeFactory(EntityType.BAOBAB,  150, 100, 15, 3, true, 50, Biome.SAVANNA, Terrain.LAND, SoilQuality.MEDIUM, 2, 10),0,50)}
            };
        }
        private PlantBuilding(TreeFactory buildingFactory, float energyCost, float resourceCost)
            : base(20f, energyCost, resourceCost, true)
        {
            BuildingFactory = buildingFactory;
        }
        public static PlantBuilding GetAbility(EntityType t) => plantingBuildingAbilities[t];

        public BuildingFactory BuildingFactory { get; }

        public override Command NewCommand(Entity caster, Node target)
        {
            return new PlantBuildingCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + BuildingFactory.BuildingType;
        }

        public override string Description()
        {
            return "The building is build at the target node.";
        }
    }
}
