using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsComponents;

namespace SanguineGenesis.GameLogic.Data.Statuses
{
    /// <summary>
    /// Can be applied to Entity. Does something.
    /// </summary>
    abstract class Status: IShowable
    {
        /// <summary>
        /// Called when the status is added to the entity.
        /// </summary>
        public virtual void Added() { }
        /// <summary>
        /// Called on game update. Returns true if the status was finished and should be removed.
        /// </summary>
        public abstract bool Step(Game game, float deltaT);
        /// <summary>
        /// Called when the status is removed from the entity.
        /// </summary>
        public virtual void Removed() { }
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

    abstract class Status<Affected,Info>:Status where Info:StatusFactory<Affected>
                                                        where Affected: class, IStatusOwner
    {
        public override StatusFactory Creator => StatusInfo;
        /// <summary>
        /// Entity affected by this status.
        /// </summary>
        public Affected AffectedEntity { get; }
        /// <summary>
        /// Immutable information about the status.
        /// </summary>
        public Info StatusInfo { get; }

        #region IShowable
        public override string GetName() => StatusInfo.GetName();
        #endregion IShowable

        public Status(Affected affected, Info statusInfo)
        {
            AffectedEntity = affected;
            StatusInfo = statusInfo;
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
        
        public override void Added()
        {
            AffectedEntity.MaxSpeedLand += StatusInfo.SpeedBonus;
        }

        public override void Removed()
        {
            AffectedEntity.MaxSpeedLand -= StatusInfo.SpeedBonus;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the status if the animal doesn't have enough energy
            AffectedEntity.Energy -= StatusInfo.EnergyCostPerS * (float)deltaT;
            if (AffectedEntity.Energy <= 0)
                return true;
            return false;
        }

        public override string Description()
        {
            return $"Increases speed of animal by {StatusInfo.SpeedBonus}.";
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
            AnimalConsumed.StateChangeLock = this;
            timeElapsed = 0;
        }

        public override void Added()
        {
            //temporarily remove the animal from list of entities
            AnimalConsumed.Faction.RemoveEntity(AnimalConsumed);
            AnimalConsumed.StateChangeLock = this;
        }

        public override void Removed()
        {
            //add the consumed unit back to the list of entities
            //it spawns in front of the affected animal
            AnimalConsumed.Position = AffectedEntity.Position + (AffectedEntity.Radius + AnimalConsumed.Radius) * AffectedEntity.Direction;
            AnimalConsumed.Faction.AddEntity(AnimalConsumed);
            AnimalConsumed.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the command if the whole duration elapsed
            timeElapsed += deltaT;
            if (timeElapsed>=StatusInfo.Duration)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return $"Consumed another animal for {StatusInfo.Duration} seconds.";
        }
    }

    /// <summary>
    /// Deals damage over time.
    /// </summary>
    class Poison : Status<Entity, PoisonFactory>
    {
        /// <summary>
        /// Time from the last tick in s.
        /// </summary>
        private float tickTimeElapsed;
        /// <summary>
        /// Number of ticks already performed.
        /// </summary>
        private int ticksPerformed;


        public Poison(Entity affected, PoisonFactory poisonInfo)
            : base(affected, poisonInfo)
        {
            ticksPerformed = 0;
            tickTimeElapsed = 0f;
        }

        public override bool Step(Game game, float deltaT)
        {
            tickTimeElapsed += deltaT;
            if (tickTimeElapsed > StatusInfo.TickTime)
            {
                //reset timer
                tickTimeElapsed -= StatusInfo.TickTime;
                //deal damage to the entity and increment number of performed ticks
                AffectedEntity.Damage(StatusInfo.TickDamage, false);
                ticksPerformed++;
            }

            //finish if all ticks have been performed
            return ticksPerformed > StatusInfo.TotalNumberOfTicks;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Damage", StatusInfo.TickDamage.ToString()),
                new Stat( "Tick time", StatusInfo.TickTime.ToString()),
                new Stat( "Ticks", StatusInfo.TotalNumberOfTicks.ToString()),
            };
        }

        public override string Description()
        {
            return $"Deals {StatusInfo.TickDamage} damage every {StatusInfo.TickTime} seconds.";
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

        public override void Added()
        {
            AffectedEntity.ThickSkin = true;
        }

        public override void Removed()
        {
            AffectedEntity.ThickSkin = false;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over or animal moves
            timer += deltaT;
            if (timer > StatusInfo.Duration || AffectedEntity.WantsToMove)
                return true;
            return false;
        }

        public override string Description()
        {
            return $"Gives animal thick skin. Lasts {StatusInfo.Duration} seconds. Removed if animal moves.";
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

        public override void Added()
        {
            AffectedEntity.AttackPeriod -= StatusInfo.AttSpeedIncr;
        }

        public override void Removed()
        {
            AffectedEntity.AttackPeriod += StatusInfo.AttSpeedIncr;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusInfo.Duration)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return $"Increases attack speed of animal by {StatusInfo.AttSpeedIncr} for {StatusInfo.Duration} seconds.";
        }
    }

    /// <summary>
    /// Represents animal on plant.
    /// </summary>
    class AnimalsOnTree : Status<Plant, AnimalsOnTreeFactory>, IAnimalStateManipulator
    {
        public List<Animal> Animals { get; }

        public AnimalsOnTree(Plant affected, AnimalsOnTreeFactory animalsOnTreeInfo, Animal putOnPlant)
            : base(affected, animalsOnTreeInfo)
        {
            Animals = new List<Animal>(1) { putOnPlant };
        }

        public override void Added()
        {
            //add ability for animals climbing down from the tree
            AffectedEntity.Abilities.Add(AffectedEntity.Faction.GameData.Abilities.ClimbDownTree);
        }

        public override void Removed()
        {
            //remove ability for animals climbing down from the tree
            AffectedEntity.Abilities.Remove(AffectedEntity.Faction.GameData.Abilities.ClimbDownTree);
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
            return $"There are {Animals.Count()} animals on this tree.";
        }
    }

    /// <summary>
    /// Represents animals underground.
    /// </summary>
    class Underground : Status<Structure, UndergroundFactory>, IAnimalStateManipulator
    {
        public List<Animal> AnimalsUnderGround => StatusInfo.AnimalsUnderGround;

        public Underground(Structure affected, UndergroundFactory undergroundInfo)
            : base(affected, undergroundInfo)
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
                new Stat( "Animals", AnimalsUnderGround.Count.ToString())
            };
        }

        public override string Description()
        {
            return $"The number of animals underground is {AnimalsUnderGround.Count()}.";
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

        public override void Added()
        {
            AffectedEntity.ViewRange += StatusInfo.RangeExtension;
        }

        public override void Removed()
        {
            AffectedEntity.ViewRange -= StatusInfo.RangeExtension;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the animal moves
            if (AffectedEntity.WantsToMove)
                return true;
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Range bonus", StatusInfo.RangeExtension.ToString())
            };
        }

        public override string Description()
        {
            return $"Increases view range by {StatusInfo.RangeExtension}. Removed if the animal moves.";
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
            Vector2 targetPoint = affected.Position + StatusInfo.Distance * StatusInfo.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(affected, targetPoint, StatusInfo.Speed, StatusInfo.Distance / StatusInfo.Speed);
        }

        public override void Added()
        {
            AffectedEntity.StateChangeLock = this;
        }

        public override void Removed()
        {
            AffectedEntity.StateChangeLock = null;
        }

        public override bool Step(Game game, float deltaT)
        {
            return moveAnimalToPoint.Step(deltaT);
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Distance", StatusInfo.Distance.ToString())
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
            AffectedEntity.Damage(deltaT * StatusInfo.DamagePerS, false);

            // remove the status if the animal can move on the terrain that is below it
            int x = (int)AffectedEntity.Position.X;
            int y = (int)AffectedEntity.Position.Y;
            var n = game.Map[x, y];
            if(n!=null)
                return AffectedEntity.CanMoveOn(n.Terrain);
            return true;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Dmg per s", StatusInfo.DamagePerS.ToString())
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
            AffectedEntity.Decay(deltaT * StatusInfo.EnergyLossPerS);

            // remove the status if the animal can move on the terrain that is below it
            return false;
        }

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Enrg loss ", StatusInfo.EnergyLossPerS.ToString())
            };
        }

        public override string Description()
        {
            return "This entity is losing energy. Once its energy reaches 0 it disappears.";
        }
    }
}
