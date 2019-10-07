using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic
{
    /// <summary>
    /// Contains entity factories, abilities and statuses that are in the game.
    /// </summary>
    class GameStaticData
    {
        public TreeFactories TreeFactories { get; }
        public StructureFactories StructureFactories { get; }
        public AnimalFactories AnimalFactories { get; }
        public Abilities Abilities { get; }
        public Statuses Statuses { get; }

        public GameStaticData()
        {
            Statuses = new Statuses();

            StructureFactories = new StructureFactories();
            StructureFactories.InitFactorys("GameLogic/Data/Entities/Structures.csv", Statuses);
            TreeFactories = new TreeFactories();
            TreeFactories.InitFactorys("GameLogic/Data/Entities/Trees.csv", Statuses);
            AnimalFactories = new AnimalFactories();
            AnimalFactories.InitFactorys("GameLogic/Data/Entities/Animals.csv", Statuses);

            Abilities = new Abilities(this);

            StructureFactories.InitAbilities(Abilities);
            TreeFactories.InitAbilities(Abilities);
            AnimalFactories.InitAbilities(Abilities);
        }
    }
}
