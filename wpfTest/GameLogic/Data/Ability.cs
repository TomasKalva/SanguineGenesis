using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;
using static wpfTest.HerbivoreEatCommand;

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
        float DistanceTo(Animal animal);
    }

    public abstract class Ability
    {
        /// <summary>
        /// Group of abilities this ability belongs to.
        /// </summary>
        protected Abilities abilities;
        public void SetAbilities(Abilities abilities)
        {
            this.abilities = abilities;
            abilities.AllAbilities.Add(this);
        }

        /// <summary>
        /// Maximal distance from the target where the ability can be cast.
        /// </summary>
        public float Distance { get; }
        /// <summary>
        /// Energy required to use this ability.
        /// </summary>
        public decimal EnergyCost { get; }
        /// <summary>
        /// True iff this ability should be performed only by one of the selected units.
        /// </summary>
        public bool OnlyOne { get; }
        /// <summary>
        /// True iff the target of the ability can be its caster.
        /// </summary>
        public bool SelfCastable { get; }

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
        public Ability(float distance, decimal energyCost, bool onlyOne, bool selfCastable)
        {
            Distance = distance;
            EnergyCost = energyCost;
            OnlyOne = onlyOne;
            SelfCastable = selfCastable;
        }
    }

    public abstract class TargetAbility<Caster, Target> : Ability where Caster:Entity 
                                                                    where Target: ITargetable
    {
        public TargetAbility(float distance, decimal energyCost, bool onlyOne, bool selfCastable)
            :base(distance, energyCost, onlyOne, selfCastable)
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

        public virtual bool ValidArguments(Caster caster, Target target) => true;

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
            //casters are put to a list so that they can be enumerated multiple times
            List<Caster> validCasters = casters
                .Where((caster) =>caster.Energy >= EnergyCost)//select only the casters who can pay
                .Where((caster) => ValidArguments(caster, target))//select only casters who are valid for this target
                 .ToList();

            //remove caster that is also target if the ability can't be self casted
            Caster self= target as Caster;
            if (!SelfCastable && self !=null)
                validCasters.Remove(self);
            
            //if there are no casters that can pay do nothing
            if (!validCasters.Any())
                return;

            if (OnlyOne)
                validCasters = validCasters.Take(1).ToList();

            //move units to the target until the required distance is reached
            if(typeof(Animal).IsAssignableFrom(typeof(Caster)) &&
                !typeof(Nothing).IsAssignableFrom(typeof(Target)))
                abilities.MoveToCast(this)
                    .SetCommands(validCasters
                    .Where(caster=>caster.GetType()==typeof(Animal))
                    .Cast<Animal>(), target);

            //give command to each caster
            foreach (Caster c in validCasters)
            {
                //create new command and assign it to c
                Command com = NewCommand(c, target);
                c.AddCommand(com);
            }
        }
    }

    public sealed class MoveTo: TargetAbility<Animal,IMovementTarget>,IMovementParametrizing
    {
        private static MoveTo ability;
        /// <summary>
        /// Movement parameters for each ability other than MoveTo.
        /// </summary>
        //private static Dictionary<Ability, MoveTo> moveToCast;
        static MoveTo()
        {
            ability = new MoveTo(0.1f, true, false);
            //initialize MoveTo for all abilities
            {
                //moveToCast = new Dictionary<Ability, MoveTo>();
                //moveToCast.Add(Attack.Get, new MoveTo(-1, true, true));
                //spawn abilities
                /*foreach (string unit in stringExtensions.Units)
                {
                    Ability a = Spawn.GetAbility(unit);
                    moveToCast.Add(a, new MoveTo(a.Distance, false, false));
                }*/
                //plant abilities
                /*foreach (string building in stringExtensions.Buildings)
                {
                    Ability a = PlantBuilding.GetAbility(building);
                    moveToCast.Add(a, new MoveTo(a.Distance, false, false));
                }*/
            }
        }
        internal MoveTo(float goalDistance, bool interruptable, bool usesAttackDistance)
            :base(-1, 0, false, false)
        {
            GoalDistance = goalDistance;
            Interruptable = interruptable;
            UsesAttackDistance = usesAttackDistance;
        }
        //public static MoveTo Get => ability;
        //public static MoveTo GetMoveTo(Ability a) => moveToCast[a];

        //interface IMovementParametrizing properties
        public float GoalDistance { get; }
        public bool Interruptable { get; }
        public bool UsesAttackDistance { get; }

        public override void SetCommands(IEnumerable<Animal> casters, IMovementTarget target)
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
                IEnumerable<Animal> castersMov = castersGroups[m];
                //set commands only if any unit can receive it
                if (!castersMov.Any())
                    continue;

                MoveToCommandAssignment mtca = new MoveToCommandAssignment(player, castersMov.Cast<Animal>().ToList(), m, target);
                //give command to each caster and set the command's creator
                foreach (Animal caster in castersMov)
                {
                    IComputable com = new MoveToPointCommand(caster, target, minStoppingDistance, this);
                    com.Assignment = mtca;

                    caster.AddCommand((Command)com);
                }
                MovementGenerator.GetMovementGenerator().AddNewCommand(player, mtca);
            }
        }

        public override Command NewCommand(Animal caster, IMovementTarget target)
        {
            throw new NotImplementedException("This method is not necessary because the virtual method " + nameof(SetCommands) + " was overriden");
        }

        public override string Description()
        {
            return "The unit moves to the target. If the target is on a terrain " +
                "this unit can't move to, the unit won't do anything. If unit meets an enemy it attacks it instead.";
        }
    }

    public sealed class Attack : TargetAbility<Animal, Entity>
    {
        internal Attack():base(0.1f, 0, false, false) { }

        public override Command NewCommand(Animal caster, Entity target)
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
        internal Spawn(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false)
        {
            SpawningUnitFactory = spawningUnitFactory;
        }
        
        public AnimalFactory SpawningUnitFactory { get; }

        public override Command NewCommand(Entity caster, Vector2 target)
        {
            return new SpawnCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningUnitFactory.EntityType;
        }

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }
    
    public sealed class SetRallyPoint : TargetAbility<Building, Vector2>
    {
        internal SetRallyPoint()
            : base(0,0,false, false)
        {
        }

        public override Command NewCommand(Building caster, Vector2 target)
        {
            return new SetRallyPointCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "Sets rally point of this building.";
        }
    }
    
    public sealed class BuildBuilding : TargetAbility<Entity, Node>
    {
        internal BuildBuilding(BuildingFactory buildingFactory)
            : base(20f, buildingFactory.EnergyCost, true, false)
        {
            BuildingFactory = buildingFactory;
        }

        public BuildingFactory BuildingFactory { get; }

        public override Command NewCommand(Entity caster, Node target)
        {
            return new BuildBuildingCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + BuildingFactory.EntityType;
        }

        public override string Description()
        {
            return "The building is build at the target node.";
        }
    }

    public sealed class HerbivoreEat : TargetAbility<Animal, IHerbivoreFood>
    {
        internal HerbivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal caster, IHerbivoreFood target)
        {
            return new HerbivoreEatCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }


    public sealed class CarnivoreEat : TargetAbility<Animal, ICarnivoreFood>
    {
        internal CarnivoreEat()
            : base(0.1f, 0, false, false)
        {
        }

        public override Command NewCommand(Animal caster, ICarnivoreFood target)
        {
            return new CarnivoreEatCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "The commanded herbivore eats tree or node.";
        }
    }

    public sealed class Grow : TargetAbility<Tree, Nothing>
    {
        internal Grow()
            : base(0, 0, true, false)
        {
        }

        public override Command NewCommand(Tree caster, Nothing target)
        {
            return new GrowCommand(caster, this);
        }

        public override string Description()
        {
            return "The tree grows until it is at max energy. The tree can't perform other commands while growing.";
        }
    }

    public sealed class CreateUnit : TargetAbility<Building, Nothing>
    {
        internal CreateUnit(AnimalFactory spawningUnitFactory)
            : base(2 * spawningUnitFactory.Range, spawningUnitFactory.EnergyCost, true, false)
        {
            SpawningUnitFactory = spawningUnitFactory;
        }

        public AnimalFactory SpawningUnitFactory { get; }

        public override Command NewCommand(Building caster, Nothing target)
        {
            return new CreateUnitCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + SpawningUnitFactory.EntityType;
        }

        public override string Description()
        {
            return "The entity spawns a new unit at the target point.";
        }
    }

    public sealed class PoisonousSpit : TargetAbility<Animal, Animal>
    {
        public float TimeUntilSpit { get; }
        public PoisonFactory PoisonFactory { get; }

        internal PoisonousSpit(float distance, float timeUntilSpit, decimal energyCost, PoisonFactory poisonFactory) 
            : base(distance, energyCost, false, false)
        {
            TimeUntilSpit = timeUntilSpit;
            PoisonFactory = poisonFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PoisonousSpitCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The unit applies poison to the target after short period of time.";
        }
    }

    public sealed class ApplyStatus : TargetAbility<Animal, Nothing>
    {
        public StatusFactory StatusFactory { get; }

        internal ApplyStatus(decimal energyCost, StatusFactory statusFactory)
            : base(0, energyCost, false, false)
        {
            StatusFactory = statusFactory;
        }

        public override Command NewCommand(Animal caster, Nothing target)
        {
            return new ApplyStatusCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The unit gain status from "+nameof(StatusFactory)+".";
        }
    }

    public sealed class PiercingBite : TargetAbility<Animal, Animal>
    {
        public decimal Damage { get; }
        public float TimeToAttack { get; }

        internal PiercingBite(decimal energyCost, decimal damage, float timeToAttack)
            : base(0.1f, energyCost, false, true)
        {
            Damage = damage;
            TimeToAttack = timeToAttack;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PiercingBiteCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal deals a large amount of damage to the target.";
        }
    }

    public sealed class ConsumeAnimal : TargetAbility<Animal, Animal>
    {
        public float TimeToConsume { get; }
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }

        internal ConsumeAnimal(decimal energyCost, float timeToConsume, ConsumedAnimalFactory consumedAnimalFactory)
            : base(0.1f, energyCost, false, false)
        {
            TimeToConsume = timeToConsume;
            ConsumedAnimalFactory = consumedAnimalFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new ConsumeAnimalCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The target animal is tepmorarily removed from the map and then put back on the map.";
        }
    }

    public sealed class Jump : TargetAbility<Animal, Vector2>
    {
        public float PreparationTime { get; }
        public float JumpSpeed { get; }

        internal Jump(decimal energyCost, float distance, float preparationTime, float jumpSpeed)
            : base(distance, energyCost, false, false)
        {
            PreparationTime = preparationTime;
            JumpSpeed = jumpSpeed;
        }

        public override Command NewCommand(Animal caster, Vector2 target)
        {
            return new JumpCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal jumps to the target location.";
        }
    }

    public sealed class ChargeTo : TargetAbility<Animal, Entity>
    {
        public decimal AttackDamageMultiplier { get; }
        public float ChargeSpeed { get; }

        internal ChargeTo(decimal energyCost, float distance, decimal attackDamageMultiplier, float charageSpeed)
            : base(distance, energyCost, false, false)
        {
            AttackDamageMultiplier = attackDamageMultiplier;
            ChargeSpeed = charageSpeed;
        }

        public override Command NewCommand(Animal caster, Entity target)
        {
            return new ChargeToCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal charges to the entity and deals it damage.";
        }
    }

    public sealed class Pull : TargetAbility<Animal, Animal>
    {
        public float PreparationTime { get; }
        public float PullSpeed { get; }

        internal Pull(decimal energyCost, float distance, float preparationTime, float pullSpeed)
            : base(distance, energyCost, false, false)
        {
            PreparationTime = preparationTime;
            PullSpeed = pullSpeed;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new PullCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
        }
    }

    public sealed class Kick : TargetAbility<Animal, Animal>
    {
        public float PreparationTime { get; }
        public decimal EnergyDamage { get; }

        internal Kick(decimal energyCost, float distance, float preparationTime, decimal energyDamage)
            : base(distance, energyCost, false, false)
        {
            PreparationTime = preparationTime;
            EnergyDamage = energyDamage;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KickCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal kicks the target animal removing some of its energy.";
        }
    }

    public sealed class ClimbTree : TargetAbility<Animal, Tree>
    {
        public float ClimbingTime { get; }
        public AnimalsOnTreeFactory AnimalsOnTreeFactory { get; }

        internal ClimbTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false)
        {
            ClimbingTime = climbingTime;
            AnimalsOnTreeFactory = new AnimalsOnTreeFactory();
        }

        public override Command NewCommand(Animal caster, Tree target)
        {
            return new ClimbTreeCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal climbs on the tree.";
        }
    }

    public sealed class ClimbDownTree : TargetAbility<Tree, Nothing>
    {
        public float ClimbingTime { get; }

        internal ClimbDownTree(decimal energyCost, float climbingTime)
            : base(0.1f, energyCost, false, false)
        {
            ClimbingTime = climbingTime;
        }

        public override Command NewCommand(Tree caster, Nothing target)
        {
            return new ClimbDownTreeCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal climbs down the tree.";
        }
    }


    public sealed class EnterHole : TargetAbility<Animal, Structure>
    {
        public float EnteringTime { get; }

        internal EnterHole(decimal energyCost, float enteringTime)
            : base(0.1f, energyCost, false, false)
        {
            EnteringTime = enteringTime;
        }

        public override bool ValidArguments(Animal caster, Structure target)
        {
            //target has to have underground status
            return target.Statuses.Where((s) => s is Underground).Any();
        }

        public override Command NewCommand(Animal caster, Structure target)
        {
            return new EnterHoleCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal enters the hole.";
        }
    }

    public sealed class ImproveStructure : TargetAbility<Animal, Structure>
    {
        public decimal EnergyPerS { get; }

        internal ImproveStructure(decimal energyPerS)
            : base(0.1f, 0, false, false)
        {
            EnergyPerS = energyPerS;
        }

        public override Command NewCommand(Animal caster, Structure target)
        {
            return new ImproveStructureCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal gives its energy to the structure.";
        }
    }

    public sealed class ExitHole : TargetAbility<Structure, Nothing>
    {
        public float ExitingTime { get; }

        internal ExitHole(decimal energyCost, float exitingTime)
            : base(0.1f, energyCost, false, false)
        {
            ExitingTime = exitingTime;
        }

        public override Command NewCommand(Structure caster, Nothing target)
        {
            return new ExitHoleCommand(caster, target, this);
        }

        public override string Description()
        {
            return "All animals exit the hole the hole.";
        }
    }

    public sealed class KnockBack : TargetAbility<Animal, Animal>
    {
        public float PreparationTime { get; }
        public KnockAwayFactory KnockAwayFactory { get; }

        internal KnockBack(decimal energyCost, float distance, float preparationTime, KnockAwayFactory knockAwayFactory)
            : base(distance, energyCost, false, true)
        {
            PreparationTime = preparationTime;
            KnockAwayFactory = knockAwayFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KnockBackCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
        }
    }

    /// <summary>
    /// Used as generic parameter for abilities without target.
    /// </summary>
    public class Nothing : ITargetable
    {
        public Vector2 Center => throw new NotImplementedException("Nothing doesn't have a center!");
        public static Nothing Get { get; }
        static Nothing()
        {
            Get = new Nothing();
        }
        private Nothing() { }
    }

    /// <summary>
    /// Marks classes that can be eaten by animals.
    /// </summary>
    public interface IFood:ITargetable
    {
        bool FoodLeft { get; }
        void EatFood(Animal eater);
    }

    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    public interface IHerbivoreFood:IFood
    {
    }
    /// <summary>
    /// Marks classes that can be eaten by herbivores.
    /// </summary>
    public interface ICarnivoreFood : IFood
    {
    }

    //Marks classes that can manipulate animal's movement.
    public interface IAnimalStateManipulator
    {

    }
}
