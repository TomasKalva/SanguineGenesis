﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameLogic.Data.Entities
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
                                                        where Affected:Entity
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

        public Status(Affected affectedEntity, Info statusInfo)
        {
            AffectedEntity = affectedEntity;
            StatusInfo = statusInfo;
        }
    }

    /// <summary>
    /// Increases speed.
    /// </summary>
    class Sprint : Status<Animal, SprintFactory>
    {
        public Sprint(Animal affectedEntity, SprintFactory sprintInfo)
            :base(affectedEntity, sprintInfo)
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

        public override string GetName() => "Sprint";

        public override string Description()
        {
            return "Increases speed of animal by " + StatusInfo.SpeedBonus + ".";
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

        public ConsumedAnimal(Animal affectedEntity, ConsumedAnimalFactory consumeInfo, Animal animalConsumed)
            : base(affectedEntity, consumeInfo)
        {
            AnimalConsumed = animalConsumed;
            AnimalConsumed.StateChangeLock = this;
            timeElapsed = 0;
        }

        public override void Added()
        {
            //temporarily remove the animal from list of entities
            AnimalConsumed.Faction.Entities.Remove(AnimalConsumed);
            AnimalConsumed.StateChangeLock = this;
        }

        public override void Removed()
        {
            //add the consumed unit back to the list of entities
            //it spawns in front of the affected animal
            AnimalConsumed.Position = AffectedEntity.Position + (AffectedEntity.Range + AnimalConsumed.Range) * AffectedEntity.Direction;
            AnimalConsumed.Faction.Entities.Add(AnimalConsumed);
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

        public override string GetName() => "Consumed";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return "Consumed another animal for " + StatusInfo.Duration + " seconds.";
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


        public Poison(Entity affectedEntity, PoisonFactory poisonInfo)
            : base(affectedEntity, poisonInfo)
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
                AffectedEntity.Damage(StatusInfo.TickDamage);
                ticksPerformed++;
            }

            //finish if all ticks have been performed
            return ticksPerformed > StatusInfo.TotalNumberOfTicks;
        }

        public override string GetName() => "Poison";

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
            return "Deals " + StatusInfo.TickDamage + " damage every "+StatusInfo.TickTime+" seconds.";
        }
    }

    /// <summary>
    /// Gives thick skin.
    /// </summary>
    class Shell : Status<Animal, ShellFactory>
    {
        private float timer;

        public Shell(Animal affectedEntity, ShellFactory shellInfo)
            : base(affectedEntity, shellInfo)
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
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusInfo.Duration)
                return true;
            return false;
        }

        public override string GetName() => "Shell";

        public override string Description()
        {
            return "Gives animal thick skin.";
        }
    }

    /// <summary>
    /// Increases attack speed.
    /// </summary>
    class FastStrikes : Status<Animal, FastStrikesFactory>
    {
        private float timer;

        public FastStrikes(Animal affectedEntity, FastStrikesFactory fastStrikesInfo)
            : base(affectedEntity, fastStrikesInfo)
        {
            timer = 0f;
        }

        public override void Added()
        {
            AffectedEntity.AttackPeriod -= 0.1f;
        }

        public override void Removed()
        {
            AffectedEntity.AttackPeriod += 0.1f;
        }

        public override bool Step(Game game, float deltaT)
        {
            //remove the staus if the duration is over
            timer += deltaT;
            if (timer > StatusInfo.Duration)
                return true;
            return false;
        }

        public override string GetName() => "Fast strikes";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Duration", StatusInfo.Duration.ToString())
            };
        }

        public override string Description()
        {
            return "Increases attack speed of animal by " + 0.1f + ".";
        }
    }

    /// <summary>
    /// Represents animal on tree.
    /// </summary>
    class AnimalsOnTree : Status<Tree, AnimalsOnTreeFactory>, IAnimalStateManipulator
    {
        public List<Animal> Animals { get; }

        public AnimalsOnTree(Tree affectedEntity, AnimalsOnTreeFactory animalsOnTreenfo, Animal putOnTree)
            : base(affectedEntity, animalsOnTreenfo)
        {
            Animals = new List<Animal>(1) { putOnTree };
        }

        public override void Added()
        {
            //add ability for animals climbing down from the tree
            AffectedEntity.Abilities.Add(AffectedEntity.Faction.GameStaticData.Abilities.ClimbDownTree);
        }

        public override void Removed()
        {
            //remove ability for animals climbing down from the tree
            AffectedEntity.Abilities.Remove(AffectedEntity.Faction.GameStaticData.Abilities.ClimbDownTree);
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override string GetName() => "On tree";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", Animals.Count.ToString())
            };
        }

        public override string Description()
        {
            return "There are " + Animals.Count() + " animals on this tree.";
        }
    }

    /// <summary>
    /// Represents animals underground.
    /// </summary>
    class Underground : Status<Structure, UndergroundFactory>, IAnimalStateManipulator
    {
        public List<Animal> AnimalsUnderGround => StatusInfo.AnimalsUnderGround;

        public Underground(Structure affectedEntity, UndergroundFactory undergroundInfo)
            : base(affectedEntity, undergroundInfo)
        {
        }

        public override bool Step(Game game, float deltaT)
        {
            return false;
        }

        public override string GetName() => "Underground";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Animals", AnimalsUnderGround.Count.ToString())
            };
        }

        public override string Description()
        {
            return "There are " + AnimalsUnderGround.Count() + " underground.";
        }
    }

    /// <summary>
    /// Increases view range.
    /// </summary>
    class FarSight : Status<Animal, FarSightFactory>
    {
        public FarSight(Animal affectedEntity, FarSightFactory farSightInfo)
            : base(affectedEntity, farSightInfo)
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
            //remove the staus if the unit moves
            if (AffectedEntity.WantsToMove)
                return true;
            return false;
        }

        public override string GetName() => "Far sight";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Range bonus", StatusInfo.RangeExtension.ToString())
            };
        }

        public override string Description()
        {
            return "Increases view range by " + StatusInfo.RangeExtension + ".";
        }
    }

    /// <summary>
    /// Animal is being knocked away.
    /// </summary>
    class KnockAway : Status<Animal, KnockBackFactory>, IAnimalStateManipulator
    {
        private MoveAnimalToPoint moveAnimalToPoint;

        public KnockAway(Animal affectedEntity, KnockBackFactory knockBackInfo)
            : base(affectedEntity, knockBackInfo)
        {
            Vector2 targetPoint = affectedEntity.Position + StatusInfo.Distance * StatusInfo.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(affectedEntity, targetPoint, StatusInfo.Speed, StatusInfo.Distance / StatusInfo.Speed);
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

        public override string GetName() => "Knocked away";

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
        public Suffocating(Animal affectedEntity, SuffocatingFactory suffocatingInfo)
            : base(affectedEntity, suffocatingInfo)
        {
        }

        public override bool Step(Game game, float deltaT)
        {
            AffectedEntity.Damage(deltaT * StatusInfo.DamagePerS);

            // remove the status if the animal can move on the terrain that is below it
            int x = (int)AffectedEntity.Position.X;
            int y = (int)AffectedEntity.Position.Y;
            var n = game.Map[x, y];
            if(n!=null)
                return AffectedEntity.CanMoveOn(n.Terrain);
            return true;
        }

        public override string GetName() => "Suffocating";

        public override List<Stat> Stats()
        {
            return new List<Stat>()
            {
                new Stat( "Dmg per s", StatusInfo.DamagePerS.ToString())
            };
        }

        public override string Description()
        {
            return "This animal is out of its natural terrain and is therefore suffocating.";
        }
    }
}
