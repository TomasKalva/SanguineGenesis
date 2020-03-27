using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsComponents;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Used to set commands to entities.
    /// </summary>
    abstract class Ability: IShowable
    {
        /// <summary>
        /// Group of abilities this ability belongs to.
        /// </summary>
        protected Abilities abilities;
        /// <summary>
        /// Sets this.abilities and adds reference to this to abilities.
        /// </summary>
        public void SetAbilities(Abilities abilities)
        {
            this.abilities = abilities;
            abilities.AllAbilities.Add(this);
        }

        /// <summary>
        /// Maximal distance from the target where the ability can be cast. If null, the attack distance of animal
        /// should be used.
        /// </summary>
        public float? Distance { get; }
        /// <summary>
        /// Energy required to use this ability.
        /// </summary>
        public float EnergyCost { get; }
        /// <summary>
        /// True iff this ability should be performed only by one of the selected units.
        /// </summary>
        public bool OnlyOne { get; }
        /// <summary>
        /// True iff the target of the ability can be its caster.
        /// </summary>
        public bool SelfCastable { get; }
        /// <summary>
        /// True iff the command can be removed from the first place in the command queue.
        /// </summary>
        public bool Interruptable { get; }
        /// <summary>
        /// How long it takes to perform this ability.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Set commands to the units. Calls the generic version of this method.
        /// </summary>
        /// <exception cref="InvalidCastException">If some casters or target have incompatible type.</exception>
        /// <exception cref="NullReferenceException">If some casters are null.</exception>
        public abstract void SetCommands(IEnumerable<Entity> casters, ITargetable target, bool resetCommandQueue);
        /// <summary>
        /// Returns true if the target type is valid for this ability.
        /// </summary>
        public abstract bool ValidTargetType(ITargetable target);
        /// <summary>
        /// Type of Target.
        /// </summary>
        public abstract Type TargetType { get; }
        /// <summary>
        /// Creates new instance of the command with specified caster and target. 
        /// Calls the generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If caster or target has incompatible type.</exception>
        public abstract Command NewCommand(Entity caster, ITargetable target);

        public Ability(float? distance, float energyCost, bool onlyOne, bool selfCastable, bool interruptable, float duration)
        {
            Distance = distance;
            EnergyCost = energyCost;
            OnlyOne = onlyOne;
            SelfCastable = selfCastable;
            Interruptable = interruptable;
            Duration = duration;
        }

        //IShowable
        public abstract string GetName();
        public abstract List<Stat> Stats();
        public abstract string Description();

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    abstract class Ability<Caster, Target> : Ability where Caster:Entity 
                                                                    where Target: ITargetable
    {
        public Ability(float? distance, float energyCost, bool onlyOne, bool selfCastable, bool interruptable=true, float duration = 0)
            :base(distance, energyCost, onlyOne, selfCastable, interruptable, duration)
        {
        }

        /// <summary>
        /// Creates new instance of the command with specified caster and target. 
        /// Calls the generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If caster or target has incompatible type.</exception>
        public sealed override Command NewCommand(Entity caster, ITargetable target)
        {
            return NewCommand((Caster)caster, (Target)target);
        }

        /// <summary>
        /// Creates new instance of the command with specified caster and target.
        /// </summary>
        public abstract Command NewCommand(Caster caster, Target target);

        /// <summary>
        /// Returns false if command with caster and target can't be created.
        /// </summary>
        public virtual bool ValidArguments(Caster caster, Target target) => true;

        /// <summary>
        /// Set commands to the units. Does nothing if target has wrong type. Calls the generic version of this method.
        /// </summary>
        /// <exception cref="NullReferenceException">If some casters are null.</exception>
        public sealed override void SetCommands(IEnumerable<Entity> casters, ITargetable target, bool resetCommandQueue)
        {
            if(target is Target t)
                SetCommands(casters.Where(c=>c is Caster).Cast<Caster>(), t, resetCommandQueue);
        }
        
        /// <summary>
        /// Returns true iff target has valid type.
        /// </summary>
        public sealed override bool ValidTargetType(ITargetable target)
        {
            return target is Target;
        }

        /// <summary>
        /// Type of Target.
        /// </summary>
        public sealed override Type TargetType 
            => typeof(Target);

        /// <summary>
        /// Returns name of target type visible to the player.
        /// </summary>
        public string TargetName
        {
            get
            {
                // instance of target isn't present during call of this method
                // and static methods can't be in interface, so checking manually is neccessary
                var targetType = TargetType;
                if (targetType == typeof( ICarnivoreFood)) return "CARN FOOD";
                else if (targetType == typeof(IHerbivoreFood)) return "HERB FOOD";
                else if (targetType == typeof(IMovementTarget)) return "MOVE TARG";
                else if (targetType == typeof(Vector2)) return "POINT";
                else if (targetType == typeof(Node)) return "NODE";
                else if (targetType == typeof(Animal)) return "ANIMAL";
                else if (targetType == typeof(Entity)) return "ENTITY";
                else if (targetType == typeof(Unit)) return "UNIT";
                else if (targetType == typeof(Building)) return "BUILDING";
                else if (targetType == typeof(Tree)) return "TREE";
                else if (targetType == typeof(Corpse)) return "CORPSE";
                else if (targetType == typeof(Structure)) return "STRUCTURE";
                else /*(targetType == typeof(Nothing))*/ return "NOTHING";
            }
        }

        /// <summary>
        /// Sets the command for this ability to all valid casters.
        /// </summary>
        /// <param name="casters">Casters who should receive the command. Only valid casters will receive the command.</param>
        /// <param name="target">Target of the new commands.</param>
        /// <param name="resetCommandQueue">If true, the casters CommandQueue will be reset.</param>
        public virtual void SetCommands(IEnumerable<Caster> casters, Target target, bool resetCommandQueue)
        {
            //casters are put to a list so that they can be enumerated multiple times
            List<Caster> validCasters = casters
                .Where((caster) =>caster.Energy >= EnergyCost)//select only the casters who can pay
                .Where((caster) => ValidArguments(caster, target))//select only casters who are valid for this target
                 .ToList();

            //remove caster that is also target if the ability can't be self casted
            if (!SelfCastable && target is Caster self)
                validCasters.Remove(self);

            //if there are no casters that can pay do nothing
            if (!validCasters.Any())
                return;

            //if the ability should be cast only by one caster,
            //find the most suitable caster to cast this ability
            if (OnlyOne)
            {
                //minimal nuber of active commands of casters
                int minCom = validCasters.Min(c => c.CommandQueue.Count);
                validCasters = validCasters.Where(c=>c.CommandQueue.Count == minCom)
                    .Take(1)//validCasters is nonempty and it has to have item with minimum command queue length
                    .ToList();
            }

            if(resetCommandQueue)
                //reset all commands
                foreach (Caster c in validCasters)
                    c.ResetCommands();

            //move units to the target until the required distance is reached
            if(validCasters.Where(caster => caster.GetType() == typeof(Animal)).Any() &&
                typeof(Target) != typeof(Nothing))
                abilities.MoveToCast(this)
                    .SetCommands(validCasters
                    .Where(caster=>caster.GetType()==typeof(Animal))
                    .Cast<Animal>(), target, resetCommandQueue);

            //give command to each caster
            foreach (Caster c in validCasters)
            {
                //create new command and assign it to c
                Command com = NewCommand(c, target);

                //instead of MoveTo command before com set MoveTo as FollowCommand for com
                if (c is Animal a && com.FollowTarget())
                {
                    MoveToCommand followCommand = (MoveToCommand)a.CommandQueue.Queue.LastOrDefault();
                    a.CommandQueue.Queue.Remove(followCommand);
                    com.FollowCommand = followCommand;
                }

                c.AddCommand(com);
            }
        }

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
            new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
            new Stat( "Self castable", SelfCastable.ToString()),
            new Stat("Only one", OnlyOne.ToString()),
            new Stat( "Target type", TargetName),
            new Stat( "Interruptable", Interruptable.ToString()),
            };
            return stats;
        }
    }

    /// <summary>
    /// Represents a target on the map.
    /// </summary>
    interface ITargetable
    {
        Vector2 Center { get; }
        /// <summary>
        /// Distance to animal.
        /// </summary>
        float DistanceTo(Entity entity);
    }

    /// <summary>
    /// Place to which an animal can go.
    /// </summary>
    interface IMovementTarget : ITargetable
    {
    }

    /// <summary>
    /// Used as generic parameter for abilities without target.
    /// </summary>
    sealed class Nothing : ITargetable
    {
        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public Vector2 Center => throw new NotImplementedException("Nothing doesn't have a center!");
        /// <summary>
        /// Returns the instance of Nothing.
        /// </summary>
        public static Nothing Get { get; }
        static Nothing()
        {
            Get = new Nothing();
        }
        private Nothing() { }

        /// <summary>
        /// Distance to nothing is 0.
        /// </summary>
        public float DistanceTo(Entity entity) => 0;
    }

    //Marks classes that can manipulate animal's movement.
    interface IAnimalStateManipulator
    {

    }
}
