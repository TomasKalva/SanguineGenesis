using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.AI
{
    /// <summary>
    /// Creates AIs.
    /// </summary>
    interface IAIFactory
    {
        /// <summary>
        /// Creates new instance that implements IAI.
        /// </summary>
        IAI NewInstance(Player controlledPlayer);
    }

    /// <summary>
    /// Plays the game.
    /// </summary>
    interface IAI
    {
        /// <summary>
        /// The player this AI controls.
        /// </summary>
        Player ControlledPlayer { get; }

        /// <summary>
        /// AI gives command to its entities. It is called every step.
        /// </summary>
        void Play(float deltaT, Game game);
    }

    /// <summary>
    /// Creates DumbAI.
    /// </summary>
    class DumbAIFactory: IAIFactory
    {
        public IAI NewInstance(Player controlledPlayer)
        {
            return new DumbAI(controlledPlayer, 1f);
        }
    }

    /// <summary>
    /// Sends all of its units to attack selected units of the enemy player. BuildBuildings and
    /// CreateAnimal are used randomly.
    /// </summary>
    class DumbAI:IAI
    {
        Random random = new Random(42);
        public Player ControlledPlayer { get; }
        private float TimeUntilDecision { get; set; }
        private float DecisionPeriod { get; }
        /// <summary>
        /// Trees with corresponding ability to be used next.
        /// </summary>
        private Dictionary<Tree, BuildBuilding> ToBuildNext { get; }
        /// <summary>
        /// Buildings with corresponding ability to be used next.
        /// </summary>
        private Dictionary<Building, CreateAnimal> ToCreateAnimalNext { get; }

        public DumbAI(Player controlledPlayer, float decisionPeriod)
        {
            ControlledPlayer = controlledPlayer;
            DecisionPeriod = decisionPeriod;
            TimeUntilDecision = DecisionPeriod;
            ToBuildNext = new Dictionary<Tree, BuildBuilding>();
            ToCreateAnimalNext = new Dictionary<Building, CreateAnimal>();
        }

        public void Play(float deltaT, Game game)
        {
            TimeUntilDecision -= deltaT;
            if(TimeUntilDecision <= 0)
            {
                PlaceBuildings(game.Map);
                CreateAnimals();
                AttackEnemies(game);
                TimeUntilDecision += DecisionPeriod;
            }
        }

        /// <summary>
        /// Send all idle animals to attack enemy building if it exists.
        /// </summary>
        public void AttackEnemies(Game game)
        {
            var idleAnimals = ControlledPlayer.GetAll<Animal>()
                .Where(a => !a.CommandQueue.Any());
            var target = game.Players[ControlledPlayer.FactionID.Opposite()].GetAll<Building>().FirstOrDefault();

            if (idleAnimals.Any()
                && target != null)
            {
                ControlledPlayer.GameStaticData.Abilities.Attack.SetCommands(idleAnimals, target, false);
            }
        }

        /// <summary>
        /// Set CreateAnimals commands to buildings.
        /// </summary>
        private void CreateAnimals()
        {
            var buildings = ControlledPlayer.GetAll<Building>();

            //only do it for 3 buildings so that the ai doesn't perform too many actions per second
            foreach (Building b in buildings.ToRandomizedList().Take(3))
            {
                CreateAnimals(b);
            }
        }

        /// <summary>
        /// Set CreateAnimal command to caster.
        /// </summary>
        /// <param name="caster"></param>
        private void CreateAnimals(Building caster)
        {
            //add caster to ToCreateAnimalNext if it isn't there yet, set its next ability
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
                //cast the ability
                ability.SetCommands(new List<Building>() { caster }, Nothing.Get, false);
                
                //select ability to use next by caster from all of its CreateAnimal abilities
                var spawningAbilities = caster.Abilities.Where(a => a is CreateAnimal).Cast<CreateAnimal>().ToList();
                if (!spawningAbilities.Any())
                    return;
                ToCreateAnimalNext[caster] = spawningAbilities[random.Next(spawningAbilities.Count)];
            }
        }

        /// <summary>
        /// Sets BuildBuilding abilities.
        /// </summary>
        public void PlaceBuildings(Map map)
        {
            var trees = ControlledPlayer.GetAll<Tree>();

            //only do it for 3 buildings so that the ai doesn't perform too many actions per second
            foreach (Tree b in trees.ToRandomizedList().Take(3))
            {
                PlaceBuildings(b, map);
            }
        }

        /// <summary>
        /// Set BuildBuilding command to caster.
        /// </summary>
        private void PlaceBuildings(Tree caster, Map map)
        {
            //add caster to ToBuildNext if it isn't there yet, set its next ability
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
                        //cast ability
                        ability.SetCommands(new List<Tree>() { caster }, n, false);

                        //select ability to use next by caster from all of its BuildBuilding abilities
                        var buildingAbilities = caster.Abilities.Where(a => a is BuildBuilding).Cast<BuildBuilding>().ToList();
                        if (!buildingAbilities.Any())
                            return;
                        ToBuildNext[caster] = buildingAbilities[random.Next(buildingAbilities.Count)];
                    }
                }
        }
    }

    /// <summary>
    /// Extensions methods for List<T>.
    /// </summary>
    public static class ListExtensions
    {
        private static readonly Random random = new Random(0);

        /// <summary>
        /// Returns random permutation of list.
        /// </summary>
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
