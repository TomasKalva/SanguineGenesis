using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    class Building : Entity
    {
        public override Vector2 Center { get; }
        public override float Range { get; }
        public Building(Players owner, EntityType unitType, float maxHealth, float viewRange = 6) : base(owner, unitType, maxHealth, viewRange)
        {
        }
    }
}
