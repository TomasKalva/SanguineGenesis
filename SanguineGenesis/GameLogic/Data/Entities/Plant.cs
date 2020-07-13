using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsControls;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    class Plant:Building, IHerbivoreFood
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
        /// <summary>
        /// Produces energy for nodes around it.
        /// </summary>
        public bool Producer { get; }
        /// <summary>
        /// How much air this plant produces.
        /// </summary>
        public int Air { get; }


        public Plant(Faction faction, string plantType, Node[,] nodes, Node[,] rootNodes, int rootDistance, float maxHealth, float maxEnergy, float maxEnergyIntake, int size,
            bool physical, Biome biome, Terrain terrain, SoilQuality soilQuality, bool producer, float viewRange, bool blocksVision, int air, List<Ability> abilities)
            : base(faction, plantType, nodes,  maxHealth, maxEnergy, size, physical, biome, terrain, soilQuality, viewRange, blocksVision, abilities)
        {
            MaxEnergyIntake = maxEnergyIntake;
            RootNodes = rootNodes;
            RootDistance = rootDistance;
            Air = air;
            Producer = producer;
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
        /// Called after this entity dies. Creates a structure representing dead plant.
        /// </summary>
        public override void Die(Game game)
        {
            base.Die(game);

            //remove roots from map
            foreach(Node n in RootNodes)
            {
                n.Roots.Remove(this);
            }

            //after physical plant dies and has energy left, spawn a dead plant
            //don't spawn structures for dead trees from neutral faction
            if(Physical && Energy > 0 && Faction.FactionID!=game.NeutralFaction.FactionID)
            {
                var neutralFaction = game.NeutralFaction;
                Structure deadPlant = new Structure(neutralFaction, "DEAD_TREE", Nodes, Energy, Energy, Size,
                    Physical, Biome, Terrain, SoilQuality.BAD, 0, true, new List<Ability>())
                {
                    Energy = Energy
                };
                game.GameData.Statuses.DecayFactory.ApplyToAffected(deadPlant);
                neutralFaction.AddEntity(deadPlant);
                game.Map.AddBuilding(deadPlant);
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
