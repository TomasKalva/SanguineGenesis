using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Set rally point of the building.
    /// </summary>
    class SetRallyPoint : Ability<Building, Vector2>
    {
        internal SetRallyPoint()
            : base(1000/*big number so that rally point can be set anywhere on the map*/, 0, false, false)
        {
        }

        public override Command NewCommand(Building user, Vector2 target)
        {
            return new SetRallyPointCommand(user, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string GetName() => "RALLY_POINT";

        public override string Description()
        {
            return "Sets rally point of this building. Spawned animals will move to the rally point.";
        }
    }

    class SetRallyPointCommand : Command<Building, Vector2, SetRallyPoint>
    {
        public SetRallyPointCommand(Building commandedEntity, Vector2 target, SetRallyPoint spawn)
            : base(commandedEntity, target, spawn)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.RallyPoint = Target;
            return true;
        }

        public override int Progress => 100;
    }
}
