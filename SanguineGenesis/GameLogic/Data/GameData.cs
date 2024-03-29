﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data
{
    /// <summary>
    /// Contains entity factories, abilities and statuses that are in the game.
    /// </summary>
    class GameData
    {
        public PlantFactories PlantFactories { get; }
        public StructureFactories StructureFactories { get; }
        public AnimalFactories AnimalFactories { get; }
        public Abilities.Abilities Abilities { get; }
        public Statuses.Statuses Statuses { get; }

        /// <exception cref="ArgumentException">Thrown if the data files are not valid.</exception>
        public GameData()
        {
            Statuses = new Statuses.Statuses();

            StructureFactories = new StructureFactories();
            StructureFactories.InitFactoryMap("GameLogic/Data/Entities/Structures.csv", Statuses);
            PlantFactories = new PlantFactories();
            PlantFactories.InitFactoryMap("GameLogic/Data/Entities/Plants.csv", Statuses);
            AnimalFactories = new AnimalFactories();
            AnimalFactories.InitFactoryMap("GameLogic/Data/Entities/Animals.csv", Statuses);

            Abilities = new Abilities.Abilities(this);

            StructureFactories.InitAbilities(Abilities);
            PlantFactories.InitAbilities(Abilities);
            AnimalFactories.InitAbilities(Abilities);
        }
    }
}
