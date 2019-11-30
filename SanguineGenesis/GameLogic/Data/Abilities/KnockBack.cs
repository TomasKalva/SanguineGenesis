using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Knock the target back.
    /// </summary>
    sealed class KnockBack : TargetAbility<Animal, Animal>
    {
        public KnockAwayFactory KnockAwayFactory { get; }

        internal KnockBack(float energyCost, float distance, float preparationTime, KnockAwayFactory knockAwayFactory)
            : base(distance, energyCost, true, false, duration:preparationTime)
        {
            KnockAwayFactory = knockAwayFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KnockBackCommand(caster, target, this);
        }

        public override string GetName() => "Knock back";

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
            Ability.KnockAwayFactory.Direction = CommandedEntity.Position.UnitDirectionTo(Targ.Position);
            if (!Ability.KnockAwayFactory.ApplyToEntity(Targ))
                Refund();

            return true;
        }
    }
}
