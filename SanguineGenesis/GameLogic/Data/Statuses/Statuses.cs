using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Contains statuses that are used in the game.
    /// </summary>
    class Statuses
    {
        public PoisonFactory PoisonFactory { get; }
        public SprintFactory SprintFactory { get; }
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }
        public AnimalsOnTreeFactory AnimalsOnTreeFactory { get; }
        public UndergroundFactory UndergroundFactory { get; }
        public ShellFactory ShellFactory { get; }
        public FastStrikesFactory FastStrikesFactory { get; }
        public FarSightFactory FarSightFactory { get; }
        public KnockAwayFactory KnockAwayFactory { get; }
        public SuffocatingFactory SuffocatingFactory { get; }


        public Statuses()
        {
            PoisonFactory = new PoisonFactory(tickDamage: 6, totalNumberOfTicks: 4, tickTime: 1.2f);
            SprintFactory = new SprintFactory(speedBonus: 1f, energyPerS: 10f);
            ConsumedAnimalFactory = new ConsumedAnimalFactory(3f);
            AnimalsOnTreeFactory = new AnimalsOnTreeFactory();
            UndergroundFactory = new UndergroundFactory();
            ShellFactory = new ShellFactory(duration: 6f);
            FastStrikesFactory = new FastStrikesFactory(duration: 4f);
            FarSightFactory = new FarSightFactory(rangeExtension: 6f);
            KnockAwayFactory = new KnockAwayFactory(distance: 2f, speed: 6f);
            SuffocatingFactory = new SuffocatingFactory(5);
        }

    }
}
