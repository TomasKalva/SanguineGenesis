using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// The tree grows until it has full energy.
    /// </summary>
    sealed class Grow : TargetAbility<Tree, Nothing>
    {
        internal Grow()
            : base(0, 0, true, false, false)
        {
        }

        public override Command NewCommand(Tree caster, Nothing target)
        {
            return new GrowCommand(caster, this);
        }

        public override string GetName() => "Grow";

        public override string Description()
        {
            return "The tree grows until it is at max energy. The tree can't perform other commands while growing.";
        }
    }

    class GrowCommand : Command<Tree, Nothing, Grow>
    {
        public GrowCommand(Tree commandedEntity, Grow plantBuilding)
            : base(commandedEntity, Nothing.Get, plantBuilding)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //finish if the tree is at full energy
            if (CommandedEntity.Energy.Full)
            {
                CommandedEntity.Built = true;
                return true;
            }
            else
                return false;
        }

        public override int Progress => (int)(100 * CommandedEntity.Energy.Percentage);
    }

}
