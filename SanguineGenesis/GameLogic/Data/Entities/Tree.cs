﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Abilities;
using wpfTest.GUI;
using static wpfTest.MainWindow;

namespace wpfTest.GameLogic
{
    public class Tree:Building, IHerbivoreFood
    {
        /// <summary>
        /// Maximum energy taken from one source per second.
        /// </summary>
        public decimal MaxEnergyIntake { get; }
        /// <summary>
        /// Sources from which the building takes energy.
        /// </summary>
        public Node[,] RootNodes { get; }
        public int Air { get; }


        public Tree(Player player, string treeType, Node[,] nodes, Node[,] rootNodes, decimal maxHealth, decimal maxEnergy, decimal maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, int air, List<Ability> abilities)
            : base(player, treeType, nodes,  maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, producer, viewRange, abilities)
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
            decimal nutrientsToEat = Math.Min(eater.FoodEnergyRegen, Health);
            Health -= nutrientsToEat;
            eater.Energy += nutrientsToEat;
        }
        #endregion IFood

        /// <summary>
        /// Called after this entity dies. Creates a structure representing dead tree.
        /// </summary>
        public override void Die()
        {
            base.Die();

            //remove roots from map
            foreach(Node n in RootNodes)
            {
                n.Roots.Remove(this);
            }

            //after physical tree dies and has energy left, spawn a dead tree
            if(Physical && Energy > 0)
                Player.Entities.Add(
                    new Structure(Player, "DEAD_TREE", Nodes, Energy, 0, Size,
                    Physical, Biome, Terrain, SoilQuality.BAD, false, 0, new List<Ability>()));
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
                    decimal nutrientsTaken;

                    //building can't take amount of nutrients that would reduce soil quality
                    nutrientsTaken = Math.Min(MaxEnergyIntake, n.Nutrients - n.Terrain.Nutrients(n.Biome, n.SoilQuality));

                    //nutrients can't drop below zero
                    nutrientsTaken = Math.Min(nutrientsTaken, n.Nutrients);

                    //building only takes energy it can use
                    nutrientsTaken = Math.Min(nutrientsTaken, Energy.AmountNotFilled);

                    Energy += nutrientsTaken;
                    n.Nutrients -= nutrientsTaken;
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
                n.Nutrients += MaxEnergyIntake * 2;
            }
        }
        #endregion Energy manipulation

        #region IShowable
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Player", Player.ToString()),
            new Stat( "EntityType", EntityType),
            new Stat( "Health", Health.ToString()),
            new Stat("Energy", Energy.ToString()),
            new Stat( "Air", Air.ToString()),
            new Stat( "Size", Size.ToString()),
            new Stat( "Biome", Biome.ToString()),
            new Stat( "Terrain", Terrain.ToString()),
            new Stat( "Soil quality", SoilQuality.ToString()),
            new Stat( "Energy intake", MaxEnergyIntake.ToString()),
            new Stat( "Physical", Physical.ToString()),
            new Stat( "View range", ViewRange.ToString()),
            };
            return stats;
        }
        #endregion IShowable
    }
}