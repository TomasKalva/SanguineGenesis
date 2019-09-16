using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic.Data.Entities
{
    /// <summary>
    /// Represent entity that isn't bound to the square grid.
    /// </summary>
    public abstract class Unit : Entity
    {
        /// <summary>
        /// Position of the unit on the map.
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// Center of the entity on the map.
        /// </summary>
        public override Vector2 Center => Position;
        /// <summary>
        /// Range of the circle collider.
        /// </summary>
        public override float Range { get; }

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
