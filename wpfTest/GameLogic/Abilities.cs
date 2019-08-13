using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class Abilities
    {
        private Dictionary<Ability, MoveTo> moveToCast;
        private Dictionary<EntityType, Spawn> unitSpawn;
        private Dictionary<EntityType, PlantBuilding> plantBuilding;

        public MoveTo MoveTo { get; }
        public MoveTo MoveToCast(Ability ability) => moveToCast[ability];
        public Attack Attack { get; }
        public Spawn UnitSpawn(EntityType type) => unitSpawn[type];
        public PlantBuilding PlantBuilding(EntityType type) => plantBuilding[type];
        public Grow Grow { get; }

        internal Abilities(GameStaticData gameStaticData)
        {
            MoveTo = new MoveTo(0.1f, true, false);
            MoveTo.SetAbilities(this);
            Attack = new Attack();
            Attack.SetAbilities(this);
            
            //unit spawn
            //Spawn spawn = new Spawn(new UnitFactory(EntityType.TIGER, 200, 150, 0.5f, true, 30m, 5f, 2f, 4f, Movement.LAND_WATER, 15f, 5m, 0.3f, 0.1f), 0);
            //spawn.SetAbilities(this);
            //unitSpawn.Add(EntityType.TIGER, spawn);
            unitSpawn = new Dictionary<EntityType, Spawn>();
            foreach (EntityType unit in EntityTypeExtensions.Units)
            {
                Spawn spawn = new Spawn(gameStaticData.UnitFactories[unit]);
                spawn.SetAbilities(this);
                unitSpawn.Add(unit, spawn);
            }

            //plant buiding
            plantBuilding = new Dictionary<EntityType, GameLogic.PlantBuilding>();
            foreach (EntityType building in EntityTypeExtensions.Buildings)
            {
                PlantBuilding plant = new PlantBuilding(gameStaticData.TreeFactories[building]);
                plant.SetAbilities(this);
                plantBuilding.Add(building, plant);
            }

            //grow
            Grow = new Grow();
            Grow.SetAbilities(this);

            //move to cast has to be initialized last because it uses other abilities
            moveToCast = new Dictionary<Ability, MoveTo>();
            //attack
            moveToCast.Add(Attack, new MoveTo(-1, true, true));
            //spawn abilities
            foreach (var entTypeAbPair in unitSpawn)
            {
                Ability a = entTypeAbPair.Value;
                MoveTo moveToAbility = new MoveTo(a.Distance, false, false);
                moveToAbility.SetAbilities(this);
                moveToCast.Add(a, moveToAbility);
            }
            //plant abilities
            foreach (var entTypeAbPair in plantBuilding)
            {
                Ability a = entTypeAbPair.Value;
                MoveTo moveToAbility = new MoveTo(a.Distance, false, false);
                moveToAbility.SetAbilities(this);
                moveToCast.Add(a, moveToAbility);
            }
        }
    }
}
