using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    public interface IMovementParametrizing
    {
        /// <summary>
        /// Distance where the unit naturaly stops moving.
        /// </summary>
        float GoalDistance { get; }
        /// <summary>
        /// If enemy in range, cancel commands and attack the enemy.
        /// </summary>
        bool AttackEnemyInstead { get; }
        /// <summary>
        /// True if the goal distance should be attack distance of the moving unit.
        /// </summary>
        bool UsesAttackDistance { get; }
    }
}
