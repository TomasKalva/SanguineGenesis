using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GUI.WinFormsComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Deal energy damage to the animal.
    /// </summary>
    class Kick : Ability<Animal, Animal>
    {
        public float EnergyDamage { get; }

        internal Kick(float energyCost, float distance, float preparationTime, float energyDamage)
            : base(distance, energyCost, true, true, duration:preparationTime)
        {
            EnergyDamage = energyDamage;
        }

        public override Command NewCommand(Animal user, Animal target)
        {
            return new KickCommand(user, target, this);
        }

        public override string GetName() => "KICK";

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
            new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
            new Stat( "Self useable", SelfUseable.ToString()),
            new Stat("Only one", OnlyOne.ToString()),
            new Stat( "Target type", TargetName),
            new Stat( "Energy dmg", EnergyDamage.ToString("0.0")),
            };
            return stats;
        }

        public override string Description()
        {
            return "The animal kicks the target animal removing some of its energy.";
        }
    }

    class KickCommand : Command<Animal, Animal, Kick>
    {
        public KickCommand(Animal commandedEntity, Animal target, Kick kick)
            : base(commandedEntity, target, kick)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            ElapsedTime += deltaT;
            if (ElapsedTime >= Ability.Duration)
            {
                //remove some of the target's energy
                Target.Energy -= Ability.EnergyDamage;
                return true;
            }
            return false;
        }
    }
}
