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
    sealed class KnockBack : Ability<Animal, Animal>
    {
        public KnockBackFactory KnockBackFactory { get; }

        internal KnockBack(float energyCost, float distance, float preparationTime, KnockBackFactory knockBackFactory)
            : base(distance, energyCost, true, false, duration:preparationTime)
        {
            KnockBackFactory = knockBackFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KnockBackCommand(caster, target, this);
        }

        public override string GetName() => "KNOCK_BACK";

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
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
            //try to apply the status to the caster, if
            //the application fails, caster gets refunded
            Ability.KnockBackFactory.Direction = CommandedEntity.Position.UnitDirectionTo(Target.Position);
            if (!Ability.KnockBackFactory.ApplyToStatusOwner(Target))
                Refund();

            return true;
        }
    }
}
