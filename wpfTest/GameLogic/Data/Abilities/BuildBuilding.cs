using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Data.Abilities
{

    public sealed class BuildBuilding : TargetAbility<Entity, Node>
    {
        internal BuildBuilding(BuildingFactory buildingFactory)
            : base(20f, buildingFactory.EnergyCost, true, false)
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

        public override string Description()
        {
            return "The building is build at the target node.";
        }
    }

    public class BuildBuildingCommand : Command<Entity, Node, BuildBuilding>
    {
        private BuildBuildingCommand() : base(null, null, null) => throw new NotImplementedException();
        public BuildBuildingCommand(Entity commandedEntity, Node target, BuildBuilding plantBuilding)
            : base(commandedEntity, target, plantBuilding)
        {
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!CanBeUsed())
                //finish if the command can't be used
                return true;

            Map map = game.Map;
            BuildingFactory bf = Ability.BuildingFactory;
            //check if the building can be built on the node
            bool canBeBuilt = true;
            int nX = Targ.X;
            int nY = Targ.Y;
            int size = bf.Size;
            Node[,] buildNodes = GameQuerying.GetGameQuerying().SelectNodes(map, nX, nY, nX + (size - 1), nY + (size - 1));

            if (buildNodes.GetLength(0) == size &&
                buildNodes.GetLength(1) == size)
            {
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        Node ijN = buildNodes[i, j];
                        //the building can't be built if the node is blocked or contains
                        //incompatible terrain
                        if (ijN.Blocked || !(bf.CanBeOn(ijN)))
                            canBeBuilt = false;
                    }
            }
            else
            {
                //the whole building has to be on the map
                canBeBuilt = false;
            }

            if (canBeBuilt)
            {
                if (!TryPay())
                    //entity doesn't have enough energy
                    return true;

                Player newUnitOwner = CommandedEntity.Player;
                Building newBuilding;
                if (bf is TreeFactory trF)
                {
                    //find energy source nodes
                    Node[,] rootNodes;
                    int rDist = trF.RootsDistance;
                    rootNodes = GameQuerying.GetGameQuerying().SelectNodes(map, nX - rDist, nY - rDist, nX + (size + rDist - 1), nY + (size + rDist - 1));
                    newBuilding = trF.NewInstance(newUnitOwner, buildNodes, rootNodes);
                    //make the tree grow
                    newUnitOwner.GameStaticData.Abilities.Grow.SetCommands(new List<Tree>(1) { (Tree)newBuilding }, Nothing.Get);
                }
                else
                {
                    StructureFactory stF = bf as StructureFactory;
                    newBuilding = stF.NewInstance(newUnitOwner, buildNodes);
                }
                //put the new building on the main map
                game.Players[newUnitOwner.PlayerID].Entities.Add(newBuilding);
                map.AddBuilding(newBuilding);
                game.Map.MapWasChanged = true;
            }
            //the command always immediately finishes regardless of the success of placing the building
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");
    }
}
