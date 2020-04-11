using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsComponents;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    class Tree:Building, IHerbivoreFood
    {
        /// <summary>
        /// Maximum energy taken from one source per second.
        /// </summary>
        public float MaxEnergyIntake { get; }
        /// <summary>
        /// Sources from which the building takes energy.
        /// </summary>
        public Node[,] RootNodes { get; }
        /// <summary>
        /// Maximal distance of roots.
        /// </summary>
        public int RootDistance { get; }
        public int Air { get; }


        public Tree(Faction faction, string treeType, Node[,] nodes, Node[,] rootNodes, int rootDistance, float maxHealth, float maxEnergy, float maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float buildingDistance, float viewRange, bool blocksVision, int air, List<Ability> abilities)
            : base(faction, treeType, nodes,  maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, producer, buildingDistance, viewRange, blocksVision, abilities)
        {
            MaxEnergyIntake = maxEnergyIntake;
            RootNodes = rootNodes;
            RootDistance = rootDistance;
            Air = air;
            foreach(Node n in rootNodes)
                n.Roots.Add(this);
        }
        #region IFood
        bool IFood.FoodLeft => !IsDead;
        void IFood.EatFood(Animal eater)
        {
            float nutrientsToEat = Math.Min(eater.FoodEnergyRegen, Health);
            Health -= nutrientsToEat;
            eater.Energy += nutrientsToEat;
        }
        #endregion IFood

        /// <summary>
        /// Called after this entity dies. Creates a structure representing dead tree.
        /// </summary>
        public override void Die(Game game)
        {
            base.Die(game);

            //remove roots from map
            foreach(Node n in RootNodes)
            {
                n.Roots.Remove(this);
            }

            //after physical tree dies and has energy left, spawn a dead tree
            if(Physical && Energy > 0)
            {
                var neutralFaction = game.NeutralFaction;
                Structure deadTree = new Structure(neutralFaction, "DEAD_TREE", Nodes, Energy, Energy, Size,
                    Physical, Biome, Terrain, SoilQuality.BAD, false, 0, 0, true, new List<Ability>());
                deadTree.Energy = Energy;
                neutralFaction.GameStaticData.Statuses.DecayFactory.ApplyToAffected(deadTree);
                neutralFaction.AddEntity(deadTree);
                game.Map.AddBuilding(deadTree);
            }
        }

        #region Energy manipulation
        /// <summary>
        /// Transforms nutrients from energy sources to energy.
        /// </summary>
        public void DrainEnergy()
        {
            foreach (Node n in RootNodes)
            {
                //check if building can take nutrients from this node
                if (n.Terrain == Terrain &&
                    n.Biome == Biome)
                {
                    //take nutrients from this node
                    float nutrientsTaken;

                    //building can't take amount of nutrients that would reduce soil quality
                    nutrientsTaken = Math.Min(MaxEnergyIntake, n.ActiveNutrients - n.Terrain.Nutrients(n.Biome, n.SoilQuality));

                    //nutrients can't drop below zero
                    nutrientsTaken = Math.Min(nutrientsTaken, n.ActiveNutrients);

                    //building only takes energy it can use
                    nutrientsTaken = Math.Min(nutrientsTaken, Energy.AmountNotFilled);

                    Energy += nutrientsTaken;
                    n.ActiveNutrients -= nutrientsTaken;
                }
            }
        }
        #endregion Energy manipulation

        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Faction", Faction.FactionID.ToString()),
            new Stat( "Health", Health.ToString("0.0")),
            new Stat("Energy", Energy.ToString("0.0")),
            new Stat( "Air", Air.ToString()),
            new Stat( "Size", Size.ToString()),
            new Stat( "Root distance", RootDistance.ToString()),
            new Stat( "Biome", Biome.ToString()),
            new Stat( "Terrain", Terrain.ToString()),
            new Stat( "Soil quality", SoilQuality.ToString()),
            new Stat( "Energy/node", MaxEnergyIntake.ToString("0.00")),
            new Stat( "Physical", Physical.ToString()),
            new Stat( "View range", ViewRange.ToString("0.0")),
            new Stat( "Producer", Producer.ToString())
            };
            return stats;
        }
        #endregion IShowable
    }
}
