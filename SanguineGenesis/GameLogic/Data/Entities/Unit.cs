﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic.Data.Entities
{
    /// <summary>
    /// Represent entity that isn't bound to the square grid.
    /// </summary>
    abstract class Unit : Entity
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
        /// Radius of the circle collider.
        /// </summary>
        public override float Radius { get; }

        public Unit(Faction faction, string unitType, float maxHealth, float viewRange, float maxEnergy, List<Ability> abilities, Vector2 position, float radius, bool physical)
            :base(faction, unitType, maxHealth, viewRange, maxEnergy, physical, abilities)
        {
            Position = position;
            Radius = radius;
        }
    }
}
