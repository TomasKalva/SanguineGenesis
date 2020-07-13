using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsControls;

namespace SanguineGenesis.GameLogic.Data.Statuses
{
    /// <summary>
    /// Can be applied to Entity. Does something.
    /// </summary>
    abstract class Status: IShowable
    {
        /// <summary>
        /// Called when the status is added to the status owner.
        /// </summary>
        public virtual void OnAdd() { }
        /// <summary>
        /// Called on game update. Returns true if the status was finished and should be removed.
        /// </summary>
        public abstract bool Step(Game game, float deltaT);
        /// <summary>
        /// Called when the status is removed from the status owner.
        /// </summary>
        public virtual void OnRemove() { }
        /// <summary>
        /// Returns status factory, this status was created by.
        /// </summary>
        public abstract StatusFactory Creator { get; }

        #region IShowable
        public abstract string GetName();
        public virtual List<Stat> Stats()
        {
           return new List<Stat>();
        }
        public abstract string Description();
        #endregion IShowable
    }

    abstract class Status<Affected, StatusFactory>:Status where StatusFactory:StatusFactory<Affected>
                                                        where Affected: class, IStatusOwner
    {
        public override Data.Statuses.StatusFactory Creator => StatusFact;
        /// <summary>
        /// Object affected by this status.
        /// </summary>
        public Affected AffectedObj { get; }
        /// <summary>
        /// Immutable factory that created this status.
        /// </summary>
        public StatusFactory StatusFact { get; }

        #region IShowable
        public override string GetName() => StatusFact.GetName();
        #endregion IShowable

        public Status(Affected affected, StatusFactory statusInfo)
        {
            this.AffectedObj = affected;
            StatusFact = statusInfo;
        }
    }

    /// <summary>
    /// Increases speed.
    /// </summary>
    class Sprint : Status<Animal, SprintFactory>
    {
        public Sprint(Animal affected, SprintFactory sprintInfo)
            :base(affected, sprintInfo)
        {
        }
        
        public override void OnAdd()
        {
            AffectedObj.MaxSpeedLand += StatusFact.SpeedBonus;
        }

        public override void OnRemove()
        {
            AffectedObj.MaxSpeedLand -= StatusFact.SpeedBonus;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the status if the animal doesn't have enough energy
            AffectedObj.Energy -= StatusFact.EnergyCostPerS * (float)deltaT;
            if (AffectedObj.Energy <= 0)
                return true;
            return false;
        }

        public override string Description()
        {
            return $"Increases speed of this animal by {StatusFact.SpeedBonus}.";
        }
    }

    /// <summary>
    /// Represents consumed animal by this animal.
    /// </summary>
    class ConsumedAnimal : Status<Animal, ConsumedAnimalFactory>, IAnimalStateManipulator
    {
        /// <summary>
        /// Animal that is consumed.
        /// </summary>
        public Animal AnimalConsumed { get; }
        /// <summary>
        /// Time the animal has been consumed in s.
        /// </summary>
        private float timeElapsed;

        public ConsumedAnimal(Animal affected, ConsumedAnimalFactory consumeInfo, Animal animalConsumed)
            : base(affected, consumeInfo)
        {
            AnimalConsumed = animalConsumed;
            timeElapsed = 0;
        }

        public override void OnAdd()
        {
            //temporarily remove the animal from list of entities
            AnimalConsumed.Faction.RemoveEntity(AnimalConsumed);
            AnimalConsumed.StateChangeLock = this;
        }

        public override void OnRemove()
        {
            //add the consumed unit back to the list of entities
            //it spawns in front of the affected animal
            AnimalConsumed.Position = AffectedObj.Position + (AffectedObj.Radius + AnimalConsumed.Radius) * AffectedObj.Direction;
            AnimalConsumed.Faction.AddEntity(AnimalConsumed);
            AnimalConsumed.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the command if the whole duration elapsed
            timeElapsed += deltaT;
            if (timeElapsed>=StatusFact.Duration)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusFact.Duration.ToString())
            };
        }

        public override string Description()
        {
            return $"Consumed another animal for {StatusFact.Duration} seconds.";
        }
    }

    /// <summary>
    /// Deals damage over time.
    /// </summary>
    class Poison : Status<Animal, PoisonFactory>
    {
        /// <summary>
        /// Time from the last tick in s.
        /// </summary>
        private float tickTimeElapsed;
        /// <summary>
        /// Number of ticks already performed.
        /// </summary>
        private int ticksPerformed;


        public Poison(Animal affected, PoisonFactory poisonInfo)
            : base(affected, poisonInfo)
        {
            ticksPerformed = 0;
            tickTimeElapsed = 0f;
        }

        public override bool Step(Game game, float deltaT)
        {
            tickTimeElapsed += deltaT;
            if (tickTimeElapsed > StatusFact.TickTime)
            {
                //reset timer
                tickTimeElapsed -= StatusFact.TickTime;
                //deal damage to the entity and increment number of performed ticks
                AffectedObj.Damage(StatusFact.TickDamage, false);
                ticksPerformed++;
            }

            //finish if all ticks have been performed
            return ticksPerformed > StatusFact.TotalNumberOfTicks;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Damage", StatusFact.TickDamage.ToString()),
                new Stat( "Tick time", StatusFact.TickTime.ToString()),
                new Stat( "Ticks", StatusFact.TotalNumberOfTicks.ToString()),
            };
        }

        public override string Description()
        {
            return $"Deals {StatusFact.TickDamage} damage every {StatusFact.TickTime} seconds.";
        }
    }

    /// <summary>
    /// Gives thick skin.
    /// </summary>
    class Shell : Status<Animal, ShellFactory>
    {
        private float timer;

        public Shell(Animal affected, ShellFactory shellInfo)
            : base(affected, shellInfo)
        {
            timer = 0f;
        }

        public override void OnAdd()
        {
            AffectedObj.ThickSkin = true;
        }

        public override void OnRemove()
        {
            AffectedObj.ThickSkin = false;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over or animal moves
            timer += deltaT;
            if (timer > StatusFact.Duration || AffectedObj.WantsToMove || AffectedObj.StateChangeLock != null)
                return true;
            return false;
        }

        public override string Description()
        {
            return $"Gives animal thick skin. Lasts {StatusFact.Duration} seconds. Removed if animal moves.";
        }
    }

    /// <summary>
    /// Increases attack speed.
    /// </summary>
    class FastStrikes : Status<Animal, FastStrikesFactory>
    {
        private float timer;

        public FastStrikes(Animal affected, FastStrikesFactory fastStrikesInfo)
            : base(affected, fastStrikesInfo)
        {
            timer = 0f;
        }

        public override void OnAdd()
        {
            AffectedObj.AttackPeriod -= StatusFact.AttSpeedIncr;
        }

        public override void OnRemove()
        {
            AffectedObj.AttackPeriod += StatusFact.AttSpeedIncr;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusFact.Duration)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusFact.Duration.ToString()),
                new Stat( "Att speed incr", StatusFact.AttSpeedIncr.ToString())
            };
        }

        public override string Description()
        {
            return $"Increases attack speed of animal by {StatusFact.AttSpeedIncr} for {StatusFact.Duration} seconds.";
        }
    }

    /// <summary>
    /// Represents animal on plant.
    /// </summary>
    class AnimalsOnPlant : Status<Plant, AnimalsOnPlantFactory>, IAnimalStateManipulator
    {
        public List<Animal> Animals { get; }

        public AnimalsOnPlant(Plant affected, AnimalsOnPlantFactory animalsOnPlantInfo, Animal putOnPlant)
            : base(affected, animalsOnPlantInfo)
        {
            Animals = new List<Animal>(1) { putOnPlant };
        }

        public override void OnAdd()
        {
            //add ability for animals climbing down from the plant
            AffectedObj.Abilities.Add(StatusFact.ClimbDownPlant);
        }

        public override void OnRemove()
        {
            //remove ability for animals climbing down from the plant
            AffectedObj.Abilities.Remove(StatusFact.ClimbDownPlant);
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", Animals.Count.ToString())
            };
        }

        public override string Description()
        {
            return $"There are animals on this plant.";
        }
    }

    /// <summary>
    /// Represents animals in hole.
    /// </summary>
    class HoleSystem : Status<Structure, HoleSystemFactory>, IAnimalStateManipulator
    {
        public List<Animal> AnimalsInHole => StatusFact.AnimalsInHole;

        public HoleSystem(Structure affected, HoleSystemFactory holeSystemInfo)
            : base(affected, holeSystemInfo)
        {
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", AnimalsInHole.Count.ToString())
            };
        }

        public override string Description()
        {
            return $"Animals can enter the hole system through this building.";
        }
    }

    /// <summary>
    /// Increases view range.
    /// </summary>
    class FarSight : Status<Animal, FarSightFactory>
    {
        public FarSight(Animal affected, FarSightFactory farSightInfo)
            : base(affected, farSightInfo)
        {
        }

        public override void OnAdd()
        {
            AffectedObj.ViewRange += StatusFact.RangeExtension;
        }

        public override void OnRemove()
        {
            AffectedObj.ViewRange -= StatusFact.RangeExtension;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the animal moves
            if (AffectedObj.WantsToMove || AffectedObj.StateChangeLock != null)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Range bonus", StatusFact.RangeExtension.ToString())
            };
        }

        public override string Description()
        {
            return $"Increases view range by {StatusFact.RangeExtension}. Removed if the animal moves.";
        }
    }

    /// <summary>
    /// Animal is being knocked away.
    /// </summary>
    class KnockAway : Status<Animal, KnockAwayFactory>, IAnimalStateManipulator
    {
        private readonly MoveAnimalToPoint moveAnimalToPoint;

        public KnockAway(Animal affected, KnockAwayFactory knockAwayInfo)
            : base(affected, knockAwayInfo)
        {
            Vector2 targetPoint = affected.Position + StatusFact.Distance * StatusFact.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(affected, targetPoint, StatusFact.Speed);
        }

        public override void OnAdd()
        {
            AffectedObj.StateChangeLock = this;
        }

        public override void OnRemove()
        {
            AffectedObj.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            return moveAnimalToPoint.Step(deltaT);
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Distance", StatusFact.Distance.ToString())
            };
        }

        public override string Description()
        {
            return "This animal is being knocked away.";
        }
    }

    /// <summary>
    /// Animal is taking damage continously.
    /// </summary>
    class Suffocating : Status<Animal, SuffocatingFactory>
    {
        public Suffocating(Animal affected, SuffocatingFactory suffocatingInfo)
            : base(affected, suffocatingInfo)
        {
        }

        public override bool Step(Game game, float deltaT)
        {
            AffectedObj.Damage(deltaT * StatusFact.DamagePerS, false);

            // remove the status if the animal can move on the terrain that is below it
            int x = (int)AffectedObj.Position.X;
            int y = (int)AffectedObj.Position.Y;
            var n = game.Map[x, y];
            if(n!=null)
                return AffectedObj.CanMoveOn(n.Terrain);
            return true;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Dmg per s", StatusFact.DamagePerS.ToString())
            };
        }

        public override string Description()
        {
            return "This animal is out of its natural environment and is therefore suffocating.";
        }
    }

    /// <summary>
    /// Owner loses energy. Once energy drops to 0, it dies.
    /// </summary>
    class Decay : Status<IDecayable, DecayFactory>
    {
        public Decay(IDecayable affected, DecayFactory decayInfo)
            : base(affected, decayInfo)
        {
        }

        public override bool Step(Game game, float deltaT)
        {
            AffectedObj.Decay(deltaT * StatusFact.EnergyLossPerS);

            // remove the status if the animal can move on the terrain that is below it
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Enrg loss ", StatusFact.EnergyLossPerS.ToString())
            };
        }

        public override string Description()
        {
            return "This entity is losing energy. Once its energy reaches 0 it disappears.";
        }
    }
}
