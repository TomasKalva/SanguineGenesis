using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic.Data.Entities
{
    public abstract class Unit : Entity
    {
        public Vector2 Position { get; set; }
        public override Vector2 Center => Position;
        public override float Range { get; }//range of the circle collider

        public Unit(Player player, string unitType, decimal maxHealth, float viewRange, decimal maxEnergy, List<Ability> abilities, Vector2 position, float range, bool physical)
            :base(player, unitType, maxHealth, viewRange, maxEnergy, physical, abilities)
        {
            Position = position;
            Range = range;
        }

        /// <summary>
        /// Returns true if at least part of the unit is visible.
        /// </summary>
        public override bool IsVisible(VisibilityMap visibilityMap)
        {
            if (visibilityMap == null)
                return false;

            //todo: check for intersection with the circle instead of the center
            return visibilityMap[(int)Center.X, (int)Center.Y];
        }
    }
}
