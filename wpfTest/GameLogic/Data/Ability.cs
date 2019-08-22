using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic
{

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
        /// <summary>
        /// True iff the command can be removed from the first place in the command queue.
        /// </summary>
        public bool Interruptable { get; }

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
        public Ability(float distance, decimal energyCost, bool onlyOne, bool selfCastable, bool interruptable)
        {
            Distance = distance;
            EnergyCost = energyCost;
            OnlyOne = onlyOne;
            SelfCastable = selfCastable;
            Interruptable = interruptable;
        }
    }

    public abstract class TargetAbility<Caster, Target> : Ability where Caster:Entity 
                                                                    where Target: ITargetable
    {
        public TargetAbility(float distance, decimal energyCost, bool onlyOne, bool selfCastable, bool interruptable=true)
            :base(distance, energyCost, onlyOne, selfCastable, interruptable)
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
    public interface IMovementTarget : ITargetable
    {
        float DistanceTo(Animal animal);
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

    //Marks classes that can manipulate animal's movement.
    public interface IAnimalStateManipulator
    {

    }
}
