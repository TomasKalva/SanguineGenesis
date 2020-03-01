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
    /// Build a building at the target node.
    /// </summary>
    sealed class BuildBuilding : TargetAbility<Entity, Node>
    {
        internal BuildBuilding(BuildingFactory buildingFactory)
            : base(buildingFactory.BuildingDistance, buildingFactory.EnergyCost, true, false)
        {
            BuildingFactory = buildingFactory;
        }

        public BuildingFactory BuildingFactory { get; }

        public override Command NewCommand(Entity caster, Node target)
        {
            return new BuildBuildingCommand(caster, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + BuildingFactory.EntityType;
        }

        public override string GetName() => "BUILD_"+BuildingFactory.EntityType;

        public override string Description()
        {
            return "The building is built at the target node. Requires at least "+BuildingFactory.SoilQuality+" soil qulity in " +
                BuildingFactory.Biome+" to be built";
        }
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
            new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
            new Stat( "Self castable", SelfCastable.ToString()),
            new Stat("Only one", OnlyOne.ToString()),
            new Stat( "Target type", TargetName),
            new Stat( "Building size", BuildingFactory.Size.ToString()),
            };
            return stats;
        }
    }

    class BuildBuildingCommand : Command<Entity, Node, BuildBuilding>
    {
        private BuildBuildingCommand() : base(null, null, null) => throw new NotImplementedException();
        public BuildBuildingCommand(Entity commandedEntity, Node target, BuildBuilding plantBuilding)
            : base(commandedEntity, target, plantBuilding)
        {
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!CanBeUsed() || CommandedEntity.DistanceTo(Target) > Ability.Distance)
                //finish if the command can't be used
                return true;

            int nX = Target.X;
            int nY = Target.Y;

            if (game.Map.BuildingCanBePlaced(Ability.BuildingFactory, nX, nY))
            {
                if (!TryPay())
                    //entity doesn't have enough energy
                    return true;

                game.Map.PlaceBuilding(Ability.BuildingFactory, CommandedEntity.Faction, nX, nY);
            }
            //the command always immediately finishes regardless of the success of placing the building
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");
    }
}
