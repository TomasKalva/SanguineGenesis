using SanguineGenesis.GameLogic.Data.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.AI
{
    interface IAiFactory
    {
        IAi NewInstance(Player controlledPlayer);
    }

    interface IAi
    {
        void Update(float deltaT, Game game);
    }

    class DumbAiFactory: IAiFactory
    {


        public DumbAiFactory()
        {

        }

        public IAi NewInstance(Player controlledPlayer)
        {
            return new DumbAi(controlledPlayer, 1f);
        }
    }

    class DumbAi:IAi
    {
        Random random = new Random(42);
        public Player ControlledPlayer { get; }
        private float TimeUntilDecision { get; set; }
        private float DecisionPeriod { get; }
        private Dictionary<Tree, BuildBuilding> ToBuildNext { get; }
        private Dictionary<Building, CreateAnimal> ToCreateAnimalNext { get; }

        public DumbAi(Player controlledPlayer, float decisionPeriod)
        {
            ControlledPlayer = controlledPlayer;
            DecisionPeriod = decisionPeriod;
            TimeUntilDecision = DecisionPeriod;
            ToBuildNext = new Dictionary<Tree, BuildBuilding>();
            ToCreateAnimalNext = new Dictionary<Building, CreateAnimal>();
        }

        public void Update(float deltaT, Game game)
        {
            TimeUntilDecision -= deltaT;
            if(TimeUntilDecision <= 0)
            {
                PlaceBuildings(game.Map);
                SpawnAnimals();
                TimeUntilDecision += DecisionPeriod;
            }
        }

        private void SpawnAnimals()
        {
            var buildings = ControlledPlayer.GetAll<Building>();

            foreach (Building b in buildings.ToRandomizedList().Take(3))//only do it for 3 buildings so that the ai doesn't have too much apm
            {
                SpawnAnimals(b);
            }
        }

        private void SpawnAnimals(Building caster)
        {
            if (!ToCreateAnimalNext.ContainsKey(caster))
            {
                var spawningAbilities = caster.Abilities.Where(a => a is CreateAnimal).Cast<CreateAnimal>().ToList();
                if (!spawningAbilities.Any())
                    return;

                ToCreateAnimalNext.Add(caster, spawningAbilities[random.Next(spawningAbilities.Count)]);
            }


            CreateAnimal ability = ToCreateAnimalNext[caster];
            if (caster.Energy >= ability.EnergyCost)
            {
                ability.SetCommands(new List<Building>() { caster }, Nothing.Get, false);

                var spawningAbilities = caster.Abilities.Where(a => a is CreateAnimal).Cast<CreateAnimal>().ToList();
                if (!spawningAbilities.Any())
                    return;
                ToCreateAnimalNext[caster] = spawningAbilities[random.Next(spawningAbilities.Count)];
            }
        }

        public void PlaceBuildings(Map map)
        {
            var trees = ControlledPlayer.GetAll<Tree>();

            foreach(Tree b in trees.ToRandomizedList().Take(3))//only do it for 3 buildings so that it doesn't take too much time
            {
                PlaceBuildings(b, map);
            }
        }

        private void PlaceBuildings(Tree caster, Map map)
        {
            if (!ToBuildNext.ContainsKey(caster))
            {
                var buildingAbilities = caster.Abilities.Where(a => a is BuildBuilding).Cast<BuildBuilding>().ToList();
                if (!buildingAbilities.Any())
                    return;

                ToBuildNext.Add(caster, buildingAbilities[random.Next(buildingAbilities.Count)]);
            }


            var possibleTargets = caster.RootNodes.OfType<Node>().ToList().ToRandomizedList() ;
            
            BuildBuilding ability = ToBuildNext[caster];
            if (caster.Energy >= ability.EnergyCost)
                foreach (Node n in possibleTargets)
                {
                    if(map.BuildingCanBePlaced(ability.BuildingFactory, n.X, n.Y))
                    {
                        ability.SetCommands(new List<Tree>() { caster }, n, false);

                        var buildingAbilities = caster.Abilities.Where(a => a is BuildBuilding).Cast<BuildBuilding>().ToList();
                        if (!buildingAbilities.Any())
                            return;
                        ToBuildNext[caster] = buildingAbilities[random.Next(buildingAbilities.Count)];
                    }
                }
        }
    }

    public static class ListExtensions
    {
        private static readonly Random random = new Random(0);

        public static List<T> ToRandomizedList<T>(this List<T> list)
        {
            var newL = new List<T>();
            for(int i = 0; i < list.Count; i++)
            {
                int nextItem = random.Next(list.Count);
                newL.Add(list[nextItem]);
                list.RemoveAt(nextItem);
            }
            return newL;
        }
    }
}
