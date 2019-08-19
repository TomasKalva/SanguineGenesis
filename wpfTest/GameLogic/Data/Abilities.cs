using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Abilities
    {
        public List<Ability> AllAbilities { get; }

        private Dictionary<Ability, MoveTo> moveToCast;
        private Dictionary<string, Spawn> unitSpawn;
        private Dictionary<string, CreateUnit> unitCreate;
        private Dictionary<string, PlantBuilding> plantBuilding;

        public MoveTo MoveTo { get; }
        public MoveTo MoveToCast(Ability ability) => moveToCast[ability];
        public Attack Attack { get; }
        public Spawn UnitSpawn(string type) => unitSpawn[type];
        public CreateUnit UnitCreate(string type) => unitCreate[type];
        public PlantBuilding PlantBuilding(string type) => plantBuilding[type];
        public Grow Grow { get; }
        public SetRallyPoint SetRallyPoint { get; }
        public HerbivoreEat HerbivoreEat { get; }
        public CarnivoreEat CarnivoreEat { get; }

        internal Abilities(GameStaticData gameStaticData)
        {
            AllAbilities = new List<Ability>();

            MoveTo = new MoveTo(0.1f, true, false);
            MoveTo.SetAbilities(this);
            Attack = new Attack();
            Attack.SetAbilities(this);
            
            //unit spawn
            unitSpawn = new Dictionary<string, Spawn>();
            foreach (var unitFac in gameStaticData.UnitFactories.Factorys)
            {
                Spawn spawn = new Spawn(unitFac.Value);
                spawn.SetAbilities(this);
                unitSpawn.Add(unitFac.Value.EntityType, spawn);
            }

            //unit create
            unitCreate = new Dictionary<string, CreateUnit>();
            foreach (var unitFac in gameStaticData.UnitFactories.Factorys)
            {
                CreateUnit createUnit = new CreateUnit(unitFac.Value);
                createUnit.SetAbilities(this);
                unitCreate.Add(unitFac.Value.EntityType, createUnit);
            }

            //plant buiding
            plantBuilding = new Dictionary<string, GameLogic.PlantBuilding>();
            foreach (var buildingFac in gameStaticData.TreeFactories.Factorys)
            {
                PlantBuilding plant = new PlantBuilding(buildingFac.Value);
                plant.SetAbilities(this);
                plantBuilding.Add(buildingFac.Value.EntityType, plant);
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

            //move to cast has to be initialized last because it uses other abilities
            moveToCast = new Dictionary<Ability, MoveTo>();
            foreach(Ability a in AllAbilities)
            {
                //move to cast abilities are not in AllAbilities to avoid infinite recursion
                MoveTo moveToAbility = new MoveTo(a.Distance, false, false);
                moveToCast.Add(a, moveToAbility);
            }
        }
    }
}
