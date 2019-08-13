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
            TreeFactories.InitFactories("GameLogic/Trees.csv");
            UnitFactories = new UnitFactories();
            UnitFactories.InitFactories("GameLogic/Units.csv");

            Abilities = new Abilities(this);

            TreeFactories.InitAbilities(Abilities);
        }
    }
}
