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
        public int Air { get; }


        public Tree(Faction faction, string treeType, Node[,] nodes, Node[,] rootNodes, float maxHealth, float maxEnergy, float maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float buildingDistance, float viewRange, int air, List<Ability> abilities)
            : base(faction, treeType, nodes,  maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, producer, buildingDistance, viewRange, abilities)
        {
            MaxEnergyIntake = maxEnergyIntake;
            RootNodes = rootNodes;
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
                Structure deadTree = new Structure(game.NeutralFaction, "DEAD_TREE", Nodes, Energy, Energy, Size,
                    Physical, Biome, Terrain, SoilQuality.BAD, false, 0, 0, new List<Ability>());
                Faction.GameStaticData.Statuses.DecayFactory.ApplyToAffected(deadTree);
                Faction.Entities.Add(deadTree);
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

        /// <summary>
        /// Puts nutrients to the soil at its roots.
        /// </summary>
        public void ProduceNutrients()
        {
            foreach (Node n in RootNodes)
            {
                n.ActiveNutrients += MaxEnergyIntake * 2;
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
            new Stat( "Biome", Biome.ToString()),
            new Stat( "Terrain", Terrain.ToString()),
            new Stat( "Soil quality", SoilQuality.ToString()),
            new Stat( "Energy intake", MaxEnergyIntake.ToString("0.0")),
            new Stat( "Physical", Physical.ToString()),
            new Stat( "View range", ViewRange.ToString("0.0")),
            new Stat( "Producer", Producer.ToString())
            };
            return stats;
        }
        #endregion IShowable
    }
}
