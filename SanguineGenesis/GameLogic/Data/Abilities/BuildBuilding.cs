using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GUI.WinFormsControls;
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
    class BuildBuilding : Ability<Entity, Node>
    {
        internal BuildBuilding(BuildingFactory buildingFactory)
            : base(buildingFactory.BuildingDistance, buildingFactory.EnergyCost, true, false)
        {
            BuildingFactory = buildingFactory;
        }

        public BuildingFactory BuildingFactory { get; }

        public override Command NewCommand(Entity user, Node target)
        {
            return new BuildBuildingCommand(user, target, this);
        }

        public override string ToString()
        {
            return base.ToString() + " " + BuildingFactory.EntityType;
        }

        public override string GetName() => "BUILD_"+BuildingFactory.EntityType;

        public override string Description()
        {
            return $"The building is built on the target node. Requires at least {BuildingFactory.SoilQuality} soil qulity in " +
                $"{BuildingFactory.Biome} on {BuildingFactory.Terrain} to be built. The building can't be built on a node blocked by another entity.";
        }
        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
                new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
                new Stat( "Self useable", SelfUseable.ToString()),
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
            if (!CanBeUsed())
                return true;
            
            if(CommandedEntity.DistanceTo(Target) > Ability.Distance)
            {
                //finish if the command can't be used
                ActionLog.LogError(CommandedEntity, Ability, "target is too far away");
                return true;
            }

            int nX = Target.X;
            int nY = Target.Y;
            float buildingRadius = Ability.BuildingFactory.Size / 2f;
            var targetCenter = new Vector2(Target.X + buildingRadius,
                                           Target.Y + buildingRadius);

            if (!game.Map.BuildingCanBePlaced(Ability.BuildingFactory, nX, nY))
            {
                ActionLog.LogError(CommandedEntity, Ability, "wrong terrain or nodes blocked by buildings");
                return true;
            }

            if (game.Collisions.CollidesWithUnits(game, targetCenter, buildingRadius))
            {
                ActionLog.LogError(CommandedEntity, Ability, "units collide with the new building");
                return true;
            }

            {
                if (!TryPay())
                {
                    //entity doesn't have enough energy
                    ActionLog.LogError(CommandedEntity, Ability, "entity doesn't have enough energy");
                    return true;
                }

                game.Map.PlaceBuilding(Ability.BuildingFactory, CommandedEntity.Faction, nX, nY, game);
            }

            //the command always immediately finishes regardless of the success of placing the building
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");
    }
}
