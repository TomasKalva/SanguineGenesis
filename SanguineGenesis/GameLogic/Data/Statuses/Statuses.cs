using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Statuses
{
    /// <summary>
    /// Contains statuses that are used in the game.
    /// </summary>
    class Statuses
    {
        public List<StatusFactory> AllStatusFactories { get; }

        public PoisonFactory PoisonFactory { get; }
        public SprintFactory SprintFactory { get; }
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }
        public AnimalsOnTreeFactory AnimalsOnPlantFactory { get; }
        public UndergroundFactory UndergroundFactory { get; }
        public ShellFactory ShellFactory { get; }
        public FastStrikesFactory FastStrikesFactory { get; }
        public FarSightFactory FarSightFactory { get; }
        public KnockAwayFactory KnockAwayFactory { get; }
        public SuffocatingFactory SuffocatingFactory { get; }
        public DecayFactory DecayFactory { get; }


        public Statuses()
        {
            AllStatusFactories = new List<StatusFactory>();

            //poison
            PoisonFactory = new PoisonFactory(tickDamage: 6, totalNumberOfTicks: 4, tickTime: 1.2f);
            AllStatusFactories.Add(PoisonFactory);

            //sprint
            SprintFactory = new SprintFactory(speedBonus: 0.4f, energyPerS: 10f);
            AllStatusFactories.Add(SprintFactory);

            //consumed animal
            ConsumedAnimalFactory = new ConsumedAnimalFactory(3f);
            AllStatusFactories.Add(ConsumedAnimalFactory);

            //animals on plant
            AnimalsOnPlantFactory = new AnimalsOnTreeFactory(new Abilities.ClimbDownPlant(0, 0.5f));
            AllStatusFactories.Add(AnimalsOnPlantFactory);

            //underground
            UndergroundFactory = new UndergroundFactory();
            AllStatusFactories.Add(UndergroundFactory);

            //shell
            ShellFactory = new ShellFactory(duration: 6f);
            AllStatusFactories.Add(ShellFactory);

            //fast strikes
            FastStrikesFactory = new FastStrikesFactory(duration: 4f);
            AllStatusFactories.Add(FastStrikesFactory);

            //far sight
            FarSightFactory = new FarSightFactory(rangeExtension: 6f);
            AllStatusFactories.Add(FarSightFactory);

            //knock away
            KnockAwayFactory = new KnockAwayFactory(distance: 2f, speed: 6f);
            AllStatusFactories.Add(KnockAwayFactory);

            //suffocating
            SuffocatingFactory = new SuffocatingFactory(5);
            AllStatusFactories.Add(SuffocatingFactory);

            //decay
            DecayFactory = new DecayFactory(1);
            AllStatusFactories.Add(DecayFactory);
        }

    }
}
