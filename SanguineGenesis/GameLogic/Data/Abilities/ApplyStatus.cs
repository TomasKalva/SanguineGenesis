﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Data.Statuses;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Applies status to the CommandedEntity.
    /// </summary>
    sealed class ApplyStatus : Ability<Animal, Nothing>
    {
        public StatusFactory StatusFactory { get; }

        internal ApplyStatus(float energyCost, StatusFactory statusFactory)
            : base(0, energyCost, false, false)
        {
            StatusFactory = statusFactory;
        }

        public override Command NewCommand(Animal caster, Nothing target)
        {
            return new ApplyStatusCommand(caster, target, this);
        }

        public override string GetName() => "APPLY_" + StatusFactory.GetName();

        public override string Description()
        {
            return $"The unit gain status {StatusFactory.ToString()}.";
        }
    }

    class ApplyStatusCommand : Command<Animal, Nothing, ApplyStatus>
    {
        public ApplyStatusCommand(Animal commandedEntity, Nothing target, ApplyStatus applyStatus)
            : base(commandedEntity, target, applyStatus)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //try to apply the status to the caster, if
            //the application fails, caster gets refunded
            if (!Ability.StatusFactory.ApplyToStatusOwner(CommandedEntity))
                Refund();

            return true;
        }
    }
}
