using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic.Data.Entities
{
    class Corpse:Unit
    {
        public Corpse(Player player, string corpseType, decimal maxHealth, decimal maxEnergy, Vector2 pos, float range)
            : base(player, corpseType, maxHealth, 0, maxEnergy, new List<Ability>(), pos, range, false)
        {
        }

        public override bool IsVisible(VisibilityMap visibilityMap)
        {
            return true;
        }
    }
}
