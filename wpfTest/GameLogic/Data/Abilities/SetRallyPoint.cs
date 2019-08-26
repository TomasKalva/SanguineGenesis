using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{
    public sealed class SetRallyPoint : TargetAbility<Building, Vector2>
    {
        internal SetRallyPoint()
            : base(0, 0, false, false)
        {
        }

        public override Command NewCommand(Building caster, Vector2 target)
        {
            return new SetRallyPointCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string Description()
        {
            return "Sets rally point of this building.";
        }
    }

    public class SetRallyPointCommand : Command<Building, Vector2, SetRallyPoint>
    {
        public SetRallyPointCommand(Building commandedEntity, Vector2 target, SetRallyPoint spawn)
            : base(commandedEntity, target, spawn)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.RallyPoint = Targ;
            return true;
        }

        public override int Progress => 100;
    }
}
