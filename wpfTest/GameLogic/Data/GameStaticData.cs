using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public class GameStaticData
    {

        public TreeFactories TreeFactories { get; }
        public UnitFactories UnitFactories { get; }
        public Abilities Abilities { get; }

        public GameStaticData()
        {
            TreeFactories = new TreeFactories();
            TreeFactories.InitFactorys("GameLogic/Data/Entities/Trees.csv");
            UnitFactories = new UnitFactories();
            UnitFactories.InitFactorys("GameLogic/Data/Entities/Units.csv");

            Abilities = new Abilities(this);

            TreeFactories.InitAbilities(Abilities);
        }
    }
}
