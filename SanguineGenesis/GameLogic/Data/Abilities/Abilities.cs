using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Contains abilities that are used in the game.
    /// </summary>
    class Abilities
    {
        /// <summary>
        /// List of all abilities that aren't moveToCast ability.
        /// </summary>
        public List<Ability> AllAbilities { get; }

        private readonly Dictionary<Ability, MoveTo> moveToCast;
        private readonly Dictionary<string, Spawn> unitSpawn;
        private readonly Dictionary<string, CreateAnimal> unitCreate;
        private readonly Dictionary<string, BuildBuilding> buildBuilding;

        public MoveTo UnbreakableMoveTo { get; }
        public MoveTo MoveTo { get; }
        public MoveTo MoveToCast(Ability ability) => moveToCast[ability];
        public Attack Attack { get; }
        public Attack UnbreakableAttack { get; }
        public Spawn UnitSpawn(string type) => unitSpawn[type];
        public CreateAnimal UnitCreate(string type) => unitCreate[type];
        public BuildBuilding BuildBuilding(string type) => buildBuilding[type];
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
        public KnockAway KnockBack { get; }
        public ClimbTree ClimbTree { get; }
        public ClimbDownTree ClimbDownTree { get; }
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

            //unit spawn
            unitSpawn = new Dictionary<string, Spawn>();
            foreach (var unitFac in gameStaticData.AnimalFactories.Factorys)
            {
                Spawn spawn = new Spawn(unitFac.Value);
                spawn.SetAbilities(this);
                unitSpawn.Add(unitFac.Value.EntityType, spawn);
            }

            //unit create
            unitCreate = new Dictionary<string, CreateAnimal>();
            foreach (var unitFac in gameStaticData.AnimalFactories.Factorys)
            {
                CreateAnimal createUnit = new CreateAnimal(unitFac.Value);
                createUnit.SetAbilities(this);
                unitCreate.Add(unitFac.Value.EntityType, createUnit);
            }

            //build buiding
            buildBuilding = new Dictionary<string, BuildBuilding>();
            foreach (var treeFac in gameStaticData.TreeFactories.Factorys)
            {
                BuildBuilding plant = new BuildBuilding(treeFac.Value);
                plant.SetAbilities(this);
                buildBuilding.Add(treeFac.Value.EntityType, plant);
            }
            foreach (var structureFac in gameStaticData.StructureFactories.Factorys)
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
            KnockBack = new KnockAway(20, 0.1f, 0.3f, gameStaticData.Statuses.KnockAwayFactory);
            KnockBack.SetAbilities(this);

            //climb tree
            ClimbTree = new ClimbTree(20, 0.3f);
            ClimbTree.SetAbilities(this);

            //climb down tree
            ClimbDownTree = new ClimbDownTree(0, 0.3f);
            ClimbDownTree.SetAbilities(this);

            //enter hole
            EnterHole = new EnterHole(0, 0.3f);
            EnterHole.SetAbilities(this);

            //enter hole
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

            //move to cast has to be initialized last because it uses other abilities
            moveToCast = new Dictionary<Ability, MoveTo>();

            MoveTo moveToAbility = new MoveTo(Attack.Distance, true);
            moveToCast.Add(Attack, moveToAbility);

            moveToAbility = new MoveTo(UnbreakableAttack.Distance, false);
            moveToCast.Add(UnbreakableAttack, moveToAbility);

            foreach (Ability a in AllAbilities.Where(ab=>!(ab is Attack)))
            {
                //move to cast abilities are not in AllAbilities to avoid infinite recursion
                moveToAbility = new MoveTo(a.Distance, false);
                moveToCast.Add(a, moveToAbility);
            }
        }
    }
}
