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
    class KnockBack : Ability<Animal, Animal>
    {
        public KnockAwayFactory KnockAwayFactory { get; }

        internal KnockBack(float energyCost, float distance, float preparationTime, KnockAwayFactory knockBackFactory)
            : base(distance, energyCost, true, false, duration:preparationTime)
        {
            KnockAwayFactory = knockBackFactory;
        }

        public override Command NewCommand(Animal user, Animal target)
        {
            return new KnockBackCommand(user, target, this);
        }

        public override string GetName() => "KNOCK_BACK";

        public override string Description()
        {
            return "The animal knocks the target animal back.";
        }
    }

    class KnockBackCommand : Command<Animal, Animal, KnockBack>
    {
        public KnockBackCommand(Animal commandedEntity, Animal target, KnockBack knockBack)
            : base(commandedEntity, target, knockBack)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //try to apply the status to the user, if
            //the application fails, user gets refunded
            Ability.KnockAwayFactory.Direction = CommandedEntity.Position.UnitDirectionTo(Target.Position);
            if (!Ability.KnockAwayFactory.ApplyToStatusOwner(Target))
                Refund();

            return true;
        }
    }
}
