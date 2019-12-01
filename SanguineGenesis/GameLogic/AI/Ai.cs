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

        public DumbAi(Player controlledPlayer, float decisionPeriod)
        {
            ControlledPlayer = controlledPlayer;
            DecisionPeriod = decisionPeriod;
            TimeUntilDecision = DecisionPeriod;
            ToBuildNext = new Dictionary<Tree, BuildBuilding>();
        }

        public void Update(float deltaT, Game game)
        {
            TimeUntilDecision -= deltaT;
            if(TimeUntilDecision <= 0)
            {
                PlaceBuildings(game.Map);
                TimeUntilDecision += DecisionPeriod;
            }
        }

        public void PlaceBuildings(Map map)
        {
            var buildings = ControlledPlayer.GetAll<Tree>();

            foreach(Tree b in buildings.ToRandomizedList().Take(3))//only do it for 3 buildings so that it doesn't take too much time
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
