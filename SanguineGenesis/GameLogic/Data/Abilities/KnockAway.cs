using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Knock the target back.
    /// </summary>
    sealed class KnockAway : Ability<Animal, Animal>
    {
        public KnockAwayFactory KnockBackFactory { get; }

        internal KnockAway(float energyCost, float distance, float preparationTime, KnockAwayFactory knockBackFactory)
            : base(distance, energyCost, true, false, duration:preparationTime)
        {
            KnockBackFactory = knockBackFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KnockBackCommand(caster, target, this);
        }

        public override string GetName() => "KNOCK_AWAY";

        public override string Description()
        {
            return "The animal knocks other animal away.";
        }
    }

    class KnockBackCommand : Command<Animal, Animal, KnockAway>
    {
        public KnockBackCommand(Animal commandedEntity, Animal target, KnockAway knockBack)
            : base(commandedEntity, target, knockBack)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //try to apply the status to the caster, if
            //the application fails, caster gets refunded
            Ability.KnockBackFactory.Direction = CommandedEntity.Position.UnitDirectionTo(Target.Position);
            if (!Ability.KnockBackFactory.ApplyToStatusOwner(Target))
                Refund();

            return true;
        }
    }
}
