using SanguineGenesis.GameLogic.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// The plant grows until it has full energy.
    /// </summary>
    sealed class Grow : Ability<Plant, Nothing>
    {
        internal Grow()
            : base(0, 0, true, false, false)
        {
        }

        public override Command NewCommand(Plant caster, Nothing target)
        {
            return new GrowCommand(caster, this);
        }

        public override string GetName() => "GROW";

        public override string Description()
        {
            return "The plant grows until it is at max energy. The plant can't perform other commands while growing.";
        }
    }

    class GrowCommand : Command<Plant, Nothing, Grow>
    {
        public GrowCommand(Plant commandedEntity, Grow plantBuilding)
            : base(commandedEntity, Nothing.Get, plantBuilding)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //finish if the plant is at full energy
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
