using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Contains abilities that are used in the game.
    /// </summary>
    class Abilities
    {
        /// <summary>
        /// List of all abilities that aren't values in moveToUse.
        /// </summary>
        public List<Ability> AllAbilities { get; }

        /// <summary>
        /// MoveTo ability for each ability in AllAbilities.
        /// </summary>
        private readonly Dictionary<Ability, MoveTo> moveToUse;
        
        private readonly Dictionary<string, Spawn> animalSpawn;
        private readonly Dictionary<string, CreateAnimal> animalCreate;
        private readonly Dictionary<string, BuildBuilding> buildBuilding;

        public MoveTo UnbreakableMoveTo { get; }
        public MoveTo MoveTo { get; }
        public MoveTo MoveToUse(Ability ability) => moveToUse[ability];
        public Attack Attack { get; }
        public Attack UnbreakableAttack { get; }
        /// <summary>
        /// Returns Spawn ability of invalid animal if animal of the type doesn't exist.
        /// </summary>
        public Spawn UnitSpawn(string type)
        {
            if (animalSpawn.TryGetValue(type, out var anim))
            {
                return anim;
            }
            else
            {
                //invalid animal indicating that the animal doesn't exist
                return new Spawn(new AnimalFactory($"{anim}_not_exists", 1, 1, 1, 1, 1, 1, 1, 1, false, 1, 1, Movement.LAND, false, Diet.CARNIVORE, 1, false, 1, 1, new List<Statuses.StatusFactory>(), 1));
            }
        }
        /// <summary>
        /// Returns CreateAnimal ability of invalid animal if animal of the type doesn't exist.
        /// </summary>
        public CreateAnimal AnimalCreate(string type)
        {
            if(animalCreate.TryGetValue(type, out var anim))
            {
                return anim;
            }
            else
            {
                //invalid animal indicating that the animal doesn't exist
                return new CreateAnimal(new AnimalFactory($"{anim}_not_exists", 1, 1, 1, 1, 1, 1, 1, 1, false, 1, 1, Movement.LAND, false, Diet.CARNIVORE, 1, false, 1, 1, new List<Statuses.StatusFactory>(), 1));
            }
        }
        /// <summary>
        /// Returns BuildBuilding ability of invalid structure if building of the type doesn't exist.
        /// </summary>
        public BuildBuilding BuildBuilding(string type)
        {
            if (buildBuilding.TryGetValue(type, out var buil))
            {
                return buil;
            }
            else
            {
                //invalid building indicating that the building doesn't exist
                return new BuildBuilding(new StructureFactory($"{buil}_not_exists",1,1,1,false,1,Biome.DEFAULT,Terrain.LAND,SoilQuality.BAD,1,1,false,new List<StatusFactory>()));
            }
        }
        //=> buildBuilding[type];
        public Grow Grow { get; }
        public SetRallyPoint SetRallyPoint { get; }
        public HerbivoreEat HerbivoreEat { get; }
        public CarnivoreEat CarnivoreEat { get; }
        public PoisonousSpit PoisonousSpit { get; }
        public ApplyStatus ActivateSprint { get; }
        public PiercingBite PiercingBite { get; }
        public ConsumeAnimal ConsumeAnimal { get; }
        public Jump Jump { get; }
        public ApplyStatus ActivateShell { get; }
        public Pull Pull { get; }
        public Pull BigPull { get; }
        public ApplyStatus ActivateFarSight { get; }
        public KnockBack KnockBack { get; }
        public ClimbPlant ClimbPlant { get; }
        public ClimbDownPlant ClimbDownTree { get; }
        public EnterHole EnterHole { get; }
        public ExitHole ExitHole { get; }
        public ApplyStatus ActivateFastStrikes { get; }
        public ImproveStructure ImproveStructure { get; }
        public ChargeTo ChargeTo { get; }
        public Kick Kick { get; }

        internal Abilities(GameData gameStaticData)
        {
            AllAbilities = new List<Ability>();

            UnbreakableMoveTo = new MoveTo(0.1f, false);
            UnbreakableMoveTo.SetAbilities(this);

            MoveTo = new MoveTo(0.1f, true);
            MoveTo.SetAbilities(this);

            Attack = new Attack(false);
            Attack.SetAbilities(this);

            UnbreakableAttack = new Attack(true);
            UnbreakableAttack.SetAbilities(this);

            //animal spawn
            animalSpawn = new Dictionary<string, Spawn>();
            foreach (var animalFac in gameStaticData.AnimalFactories.FactoryMap)
            {
                Spawn spawn = new Spawn(animalFac.Value);
                spawn.SetAbilities(this);
                animalSpawn.Add(animalFac.Value.EntityType, spawn);
            }

            //animal create
            animalCreate = new Dictionary<string, CreateAnimal>();
            foreach (var animalFac in gameStaticData.AnimalFactories.FactoryMap)
            {
                CreateAnimal createUnit = new CreateAnimal(animalFac.Value);
                createUnit.SetAbilities(this);
                animalCreate.Add(animalFac.Value.EntityType, createUnit);
            }

            //build buiding
            buildBuilding = new Dictionary<string, BuildBuilding>();
            foreach (var plantFac in gameStaticData.PlantFactories.FactoryMap)
            {
                BuildBuilding plant = new BuildBuilding(plantFac.Value);
                plant.SetAbilities(this);
                buildBuilding.Add(plantFac.Value.EntityType, plant);
            }
            foreach (var structureFac in gameStaticData.StructureFactories.FactoryMap)
            {
                BuildBuilding plant = new BuildBuilding(structureFac.Value);
                plant.SetAbilities(this);
                buildBuilding.Add(structureFac.Value.EntityType, plant);
            }

            //grow
            Grow = new Grow();
            Grow.SetAbilities(this);

            //set rally point
            SetRallyPoint = new SetRallyPoint();
            SetRallyPoint.SetAbilities(this);

            //herbivore eating
            HerbivoreEat = new HerbivoreEat();
            HerbivoreEat.SetAbilities(this);

            //carnivore eating
            CarnivoreEat = new CarnivoreEat();
            CarnivoreEat.SetAbilities(this);

            //poisonous spit
            PoisonousSpit = new PoisonousSpit(4f, 0.2f, 30f, gameStaticData.Statuses.PoisonFactory);
            PoisonousSpit.SetAbilities(this);

            //sprint
            ActivateSprint = new ApplyStatus(20, gameStaticData.Statuses.SprintFactory);
            ActivateSprint.SetAbilities(this);

            //piercing bite
            PiercingBite = new PiercingBite(30, 100f, 0.5f);
            PiercingBite.SetAbilities(this);

            //consume animal
            ConsumeAnimal = new ConsumeAnimal(35, 0.5f, gameStaticData.Statuses.ConsumedAnimalFactory);
            ConsumeAnimal.SetAbilities(this);
            
            //shell
            ActivateShell = new ApplyStatus(20, gameStaticData.Statuses.ShellFactory);
            ActivateShell.SetAbilities(this);

            //jump
            Jump = new Jump(15, 4f, 0.1f, 10f );
            Jump.SetAbilities(this);

            //pull
            Pull = new Pull(75, 4f, 1f, 8f);
            Pull.SetAbilities(this);

            //big pull
            BigPull = new Pull(40, 6f, 1.2f, 8f);
            BigPull.SetAbilities(this);

            //activate far sight
            ActivateFarSight = new ApplyStatus(20, gameStaticData.Statuses.FarSightFactory);
            ActivateFarSight.SetAbilities(this);

            //knockback
            KnockBack = new KnockBack(20, 0.1f, 0.3f, gameStaticData.Statuses.KnockAwayFactory);
            KnockBack.SetAbilities(this);

            //climb plant
            ClimbPlant = new ClimbPlant(20, 0.3f);
            ClimbPlant.SetAbilities(this);

            //climb down plant
            ClimbDownTree = new ClimbDownPlant(0, 0.3f);
            ClimbDownTree.SetAbilities(this);

            //enter hole
            EnterHole = new EnterHole(0, 0.3f);
            EnterHole.SetAbilities(this);

            //exit hole
            ExitHole = new ExitHole(0, 0.3f);
            ExitHole.SetAbilities(this);

            //fast strikes
            ActivateFastStrikes = new ApplyStatus(20, gameStaticData.Statuses.FastStrikesFactory);
            ActivateFastStrikes.SetAbilities(this);

            //improve structure
            ImproveStructure = new ImproveStructure(3f);
            ImproveStructure.SetAbilities(this);

            //charge to
            ChargeTo = new ChargeTo(40f, 3f, 2.5f, 10f);
            ChargeTo.SetAbilities(this);

            //kick
            Kick = new Kick(20f, 0.1f, 0.2f, 30f);
            Kick.SetAbilities(this);

            //move to use has to be initialized last because it uses other abilities
            moveToUse = new Dictionary<Ability, MoveTo>();

            MoveTo moveToAbility = new MoveTo(Attack.Distance, true);
            moveToUse.Add(Attack, moveToAbility);

            moveToAbility = new MoveTo(UnbreakableAttack.Distance, false);
            moveToUse.Add(UnbreakableAttack, moveToAbility);

            foreach (Ability a in AllAbilities
                                    .Where(ab=>!(ab is Attack) && 
                                                ab.UserType.IsAssignableFrom(typeof(Animal))))
            {
                //move to use abilities are not in AllAbilities to avoid infinite recursion
                moveToAbility = new MoveTo(a.Distance, false);
                moveToUse.Add(a, moveToAbility);
            }
        }
    }
}
