using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Applies status to the CommandedEntity.
    /// </summary>
    sealed class ApplyStatus : TargetAbility<Animal, Nothing>
    {
        public StatusFactory StatusFactory { get; }

        internal ApplyStatus(decimal energyCost, StatusFactory statusFactory)
            : base(0, energyCost, false, false)
        {
            StatusFactory = statusFactory;
        }

        public override Command NewCommand(Animal caster, Nothing target)
        {
            return new ApplyStatusCommand(caster, target, this);
        }

        public override string GetName() => "Apply " + StatusFactory.ToString();

        public override string Description()
        {
            return "The unit gain status from " + nameof(StatusFactory) + ".";
        }
    }

    class ApplyStatusCommand : Command<Animal, Nothing, ApplyStatus>
    {
        public ApplyStatusCommand(Animal commandedEntity, Nothing target, ApplyStatus applyStatus)
            : base(commandedEntity, target, applyStatus)
        {
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (CanPay())
            {
                //if caster can pay, try to apply the status to the caster, if
                //the application succeeds, caster pays
                if (Ability.StatusFactory.ApplyToEntity(CommandedEntity))
                    TryPay();

            }
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");

    }
}
