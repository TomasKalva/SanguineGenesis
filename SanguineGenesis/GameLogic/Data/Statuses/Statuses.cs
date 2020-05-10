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
        public PoisonFactory PoisonFactory { get; }
        public SprintFactory SprintFactory { get; }
        public ConsumedAnimalFactory ConsumedAnimalFactory { get; }
        public AnimalsOnPlantFactory AnimalsOnPlantFactory { get; }
        public HoleSystemFactory HoleSystem { get; }
        public ShellFactory ShellFactory { get; }
        public FastStrikesFactory FastStrikesFactory { get; }
        public FarSightFactory FarSightFactory { get; }
        public KnockAwayFactory KnockAwayFactory { get; }
        public SuffocatingFactory SuffocatingFactory { get; }
        public DecayFactory DecayFactory { get; }


        public Statuses()
        {
            //poison
            PoisonFactory = new PoisonFactory(tickDamage: 6, totalNumberOfTicks: 4, tickTime: 1.2f);

            //sprint
            SprintFactory = new SprintFactory(speedBonus: 0.4f, energyPerS: 10f);

            //consumed animal
            ConsumedAnimalFactory = new ConsumedAnimalFactory(3f);

            //animals on plant
            AnimalsOnPlantFactory = new AnimalsOnPlantFactory(new Abilities.ClimbDownPlant(0, 0.5f));

            //underground
            HoleSystem = new HoleSystemFactory();

            //shell
            ShellFactory = new ShellFactory(duration: 6f);

            //fast strikes
            FastStrikesFactory = new FastStrikesFactory(duration: 4f);

            //far sight
            FarSightFactory = new FarSightFactory(rangeExtension: 6f);

            //knock away
            KnockAwayFactory = new KnockAwayFactory(distance: 2f, speed: 6f);

            //suffocating
            SuffocatingFactory = new SuffocatingFactory(5);

            //decay
            DecayFactory = new DecayFactory(1);
        }
    }
}
