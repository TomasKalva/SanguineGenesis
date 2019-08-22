using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic
{
    public class GameStaticData
    {

        public TreeFactories TreeFactories { get; }
        public StructureFactories StructureFactories { get; }
        public UnitFactories UnitFactories { get; }
        public Abilities Abilities { get; }
        public Statuses Statuses { get; }

        public GameStaticData()
        {
            Statuses = new Statuses();

            StructureFactories = new StructureFactories();
            StructureFactories.InitFactorys("GameLogic/Data/Entities/Structures.csv", Statuses);
            TreeFactories = new TreeFactories();
            TreeFactories.InitFactorys("GameLogic/Data/Entities/Trees.csv", Statuses);
            UnitFactories = new UnitFactories();
            UnitFactories.InitFactorys("GameLogic/Data/Entities/Animals.csv", Statuses);

            Abilities = new Abilities(this);

            StructureFactories.InitAbilities(Abilities);
            TreeFactories.InitAbilities(Abilities);
            UnitFactories.InitAbilities(Abilities);
        }
    }
}
