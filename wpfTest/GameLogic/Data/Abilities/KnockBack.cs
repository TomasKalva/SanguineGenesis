using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Data.Entities;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class KnockBack : TargetAbility<Animal, Animal>
    {
        public KnockAwayFactory KnockAwayFactory { get; }

        internal KnockBack(decimal energyCost, float distance, float preparationTime, KnockAwayFactory knockAwayFactory)
            : base(distance, energyCost, false, true, duration:preparationTime)
        {
            KnockAwayFactory = knockAwayFactory;
        }

        public override Command NewCommand(Animal caster, Animal target)
        {
            return new KnockBackCommand(caster, target, this);
        }

        public override string Description()
        {
            return "The animal pulls the other animal to itself.";
        }
    }

    public class KnockBackCommand : Command<Animal, Animal, KnockBack>
    {
        public KnockBackCommand(Animal commandedEntity, Animal target, KnockBack knockBack)
            : base(commandedEntity, target, knockBack)
        {
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (CanPay() && CanBeUsed())
            {

                //if caster can pay, try to apply the status to the caster, if
                //the application succeeds, caster pays
                Ability.KnockAwayFactory.Direction = CommandedEntity.Position.UnitDirectionTo(Targ.Position);
                if (Ability.KnockAwayFactory.ApplyToEntity(Targ))
                    TryPay();
            }
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");

    }
}
