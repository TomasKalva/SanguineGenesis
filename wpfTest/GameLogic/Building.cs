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
        public Building(Player player, EntityType bulidingType, float maxHealth, float viewRange, float maxEnergy) 
            : base(player, bulidingType, maxHealth, viewRange, maxEnergy)
        {
        }
    }
}
