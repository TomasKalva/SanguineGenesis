using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    public abstract class Command
    {
        /// <summary>
        /// Performs one step of the command. Returns true if command is finished.
        /// </summary>
        public abstract bool PerformCommand(Game game, float deltaT);
    }

    public abstract class Command<Caster, Target, Abil> : Command where Caster : Entity
                                                                    where Target : ITargetable
                                                                    where Abil:TargetAbility<Caster,Target>
    {
        /// <summary>
        /// The ability this command is performing.
        /// </summary>
        public Abil Ability { get; }
        /// <summary>
        /// The entity who performs this command.
        /// </summary>
        public Caster CommandedEntity { get; }
        /// <summary>
        /// Target of the ability.
        /// </summary>
        public Target Targ { get; }
        /// <summary>
        /// True iff the ability was paid.
        /// </summary>
        private bool Paid { get; set; }

        protected Command(Caster commandedEntity, Target target, Abil ability)
        {
            Ability = ability;
            CommandedEntity = commandedEntity;
            Targ = target;
        }
        public override string ToString()
        {
            return Ability.ToString();
        }

        /// <summary>
        /// Try to pay for the ability. Returns if paying was successful.
        /// </summary>
        protected bool TryPay()
        {
            if (Paid)
                //the ability was paid already
                return true;

            if (CommandedEntity.Energy >= Ability.EnergyCost &&
                CommandedEntity.Player.Resource >= Ability.ResourceCost)
            {
                //pay for the ability
                CommandedEntity.Energy -= Ability.EnergyCost;
                CommandedEntity.Player.Resource -= Ability.ResourceCost;
                Paid = true;
                return true;
            }
            //not enough resource/energy
            return false;
        }
    }

    public class AttackCommand : Command<Unit, Entity, Attack>
    {
        private float timeUntilAttack;//time in s until this unit attacks

        private AttackCommand():base(null,null,null) => throw new NotImplementedException();
        public AttackCommand(Unit commandedEntity, Entity target, Attack attack)
            : base(commandedEntity, target, attack)
        {
            this.timeUntilAttack = 0f;

        }
        
        public override bool PerformCommand(Game game, float deltaT)
        {
            //dead target cannont be attacked
            if (Targ.IsDead)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            CommandedEntity.Direction = Targ.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                //damage target
                timeUntilAttack -= CommandedEntity.AttackPeriod;
                Targ.Health -= CommandedEntity.AttackDamage;
            }

            bool finished = CommandedEntity.DistanceTo(Targ) >= CommandedEntity.AttackDistance;
            if (finished)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }
            return false;
        }
    }

    public interface IComputable
    {
        MoveToCommandAssignment Assignment { get; set; }
    }

    public class MoveToPointCommand : Command<Unit, IMovementTarget, MoveTo>,IComputable
    {
        public MoveToCommandAssignment Assignment { get; set; }
        private MoveToLogic moveToLogic;
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        

        private MoveToPointCommand() : base(null, null, null) => throw new NotImplementedException();
        public MoveToPointCommand(Unit commandedEntity, IMovementTarget target, float minStoppingDistance, MoveTo ability)
            : base(commandedEntity, target, ability)
        {
            moveToLogic = new MoveToLogic(CommandedEntity, null, minStoppingDistance, target, Ability);
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            bool finished = false;
            //command immediately finishes if the assignment was invalidated
            if (Assignment != null && Assignment.Invalid)
                finished = true;
            else
                finished = moveToLogic.Step(game, deltaT, flowMap, this);


            if (finished)
            {
                CommandedEntity.StopMoving = true;
                this.RemoveFromAssignment();
            }
            return finished;
        }


        public void UpdateFlowMap(FlowMap flowMap)
        {
            this.flowMap = flowMap;
        }

        public void RemoveFromAssignment()
        {
            Assignment.Units.Remove(CommandedEntity);
        }
    }
    
    public class MoveToLogic
    {
        /// <summary>
        /// If the distance to the target is higher than this, flowmap will be used. 
        /// Otherwise unit will walk straight to the target.
        /// </summary>
        private const float FLOWMAP_DISTANCE = 1.41f;

        /// <summary>
        /// Moving unit.
        /// </summary>
        private Unit unit;
        /// <summary>
        /// Point on the map where the unit should go.
        /// </summary>
        public ITargetable TargetPoint { get; }
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        /// <summary>
        /// Distance from the target when unit can stop if it gets stuck.
        /// </summary>
        private float minStoppingDistance;
        private IMovementParametrizing movementParametrizing;
        private NoMovementDetection noMovementDetection;

        public MoveToLogic(Unit unit, FlowMap flowMap, float minStoppingDistance, IMovementTarget target, IMovementParametrizing movementParametrizing)
        {
            this.unit = unit;
            this.flowMap = flowMap;
            this.minStoppingDistance = minStoppingDistance;
            noMovementDetection = new NoMovementDetection();
            this.TargetPoint = target;
            this.movementParametrizing = movementParametrizing;
        }

        public bool Step(Game game, float deltaT, FlowMap flowMap, MoveToPointCommand command)
        {
            //set the command assignment to be active to increase its priority
            command.Assignment.Active = true;
            
            //if an enemy is in attack range, attack it instead of other commands
            if(movementParametrizing.Interruptable)
            {
                Entity enemy = GameQuerying.GetGameQuerying().SelectUnits(game, 
                    (u) => u.Player!=unit.Player 
                            && unit.DistanceTo(u) <= unit.AttackDistance).FirstOrDefault();
                if(enemy!=null)
                {
                    //attack the enemy
                    unit.StopMoving = true;
                    command.RemoveFromAssignment();
                    unit.SetCommand(new AttackCommand(unit, enemy, Attack.Get));
                    return false;//new command is already set
                }
            }

            //check if the map was set yet
            if (flowMap == null)
                return false;

            float dist = (unit.Center - TargetPoint.Center).Length;
            if (dist > FLOWMAP_DISTANCE)
            {
                //use flowmap
                unit.Accelerate(flowMap.GetIntensity(unit.Center, unit.Acceleration));
            }
            else
            {
                //go in straight line
                Vector2 direction = unit.Center.UnitDirectionTo(TargetPoint.Center);
                unit.Accelerate(unit.Acceleration * direction);
            }
            //update last four positions
            noMovementDetection.AddNextPosition(unit.Center);
            //set that unit wants to move
            unit.WantsToMove = true;

            bool finished=Finished();

            //command is finished if unit reached the goal distance or if it stayed at one
            //place near the target position for a long time
            if (finished //unit is close to the target point
                || (noMovementDetection.NotMovingMuch(deltaT, unit.MaxSpeed * deltaT / 2) && CanStop())//unit is stuck
                /*|| game.Players[unit.Player.PlayerID].MapView
                    .GetObstacleMap(unit.Movement)[(int)TargetPoint.Center.X,(int)TargetPoint.Center.Y]*/)//target point is blocked
            {
                return true;
            }
            return false;
        }

        public bool Finished()
        {
            Entity e;
            if ((e=TargetPoint as Entity)==null)
            {
                //target is vector
                //use distance between center of the unit and the point
                return (TargetPoint.Center - unit.Center).Length <= movementParametrizing.GoalDistance;
            }
            else
            {
                //target is entity
                //use distance between closest points of the entities
                if(movementParametrizing.UsesAttackDistance)
                    return unit.DistanceTo(e) <= unit.AttackDistance;
                else
                    return unit.DistanceTo(e) <= movementParametrizing.GoalDistance;
            }
        }

        public void UpdateFlowMap(FlowMap flMap)
        {
            this.flowMap = flMap;
        }

        /// <summary>
        /// Returns true if the unit can stop because of being stuck.
        /// </summary>
        private bool CanStop()
        {
            return (TargetPoint.Center - unit.Center).Length < minStoppingDistance;
        }
    }

    /// <summary>
    /// Detects if the unit is not moving much.
    /// </summary>
    public class NoMovementDetection
    {
        private static Vector2 INVALID { get; }
        private Vector2[] last4positions;
        static NoMovementDetection()
        {
            INVALID = new Vector2(-1, -1);
        }
        public NoMovementDetection()
        {
            last4positions = new Vector2[4];
            last4positions[0] = INVALID;
            last4positions[1] = INVALID;
            last4positions[2] = INVALID;
            last4positions[3] = INVALID;
        }

        public void AddNextPosition(Vector2 v)
        {
            last4positions[3] = last4positions[2];
            last4positions[2] = last4positions[1];
            last4positions[1] = last4positions[0];
            last4positions[0] = v;
        }

        public bool NotMovingMuch(float deltaT, float minDistSum)
        {
            if (last4positions[0] != INVALID &&
                last4positions[1] != INVALID &&
                last4positions[2] != INVALID &&
                last4positions[3] != INVALID)
            {
                float d1 = (last4positions[0] - last4positions[1]).Length;
                float d2 = (last4positions[1] - last4positions[2]).Length;
                float d3 = (last4positions[2] - last4positions[3]).Length;

                if (d1 + d2 + d3 < minDistSum)
                    return true;
            }
            return false;
        }
    }


    public class SpawnCommand : Command<Entity, Vector2, Spawn>
    {
        /// <summary>
        /// How long the unit was spawning in s.
        /// </summary>
        public float SpawnTimer { get; private set; }

        private SpawnCommand() : base(null, default(Vector2), null) => throw new NotImplementedException();
        public SpawnCommand(Entity commandedEntity, Vector2 target, Spawn spawn, EntityType entityType)
            : base(commandedEntity, target, spawn)
        {
            SpawnTimer = 0f;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!TryPay())
                //finish command if paying was unsuccessful
                return true;

            SpawnTimer += deltaT;
            if (SpawnTimer >= Ability.SpawningUnitFactory.SpawningTime)
            {
                Player newUnitOwner = CommandedEntity.Player;
                Unit newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, Targ);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                return true;
            }
            return false;
        }
    }

    public class PlantBuildingCommand : Command<Entity, Node, PlantBuilding>
    {
        private PlantBuildingCommand() : base(null, null, null) => throw new NotImplementedException();
        public PlantBuildingCommand(Entity commandedEntity, Node target, PlantBuilding plantBuilding)
            : base(commandedEntity, target, plantBuilding)
        {
        }

        public override bool PerformCommand(Game game, float deltaT)
        {

            Map map = game.Map;
            BuildingFactory bf = Ability.BuildingFactory;
            //check if the building can be built on the node
            bool canBeBuilt = true;
            int nX = Targ.X;
            int nY = Targ.Y;
            int size = bf.Size;
            Node[,] buildNodes = GameQuerying.GetGameQuerying().SelectNodes(map, nX, nY, nX + (size-1), nY + (size-1));

            if (buildNodes.GetLength(0)==size &&
                buildNodes.GetLength(1)==size)
            {
                for(int i=0;i<size;i++)
                    for (int j = 0; j < size; j++)
                    {
                        Node ijN = buildNodes[i, j];
                        //the building can't be built if the node is blocked or contains
                        //incompatible terrain
                        if (ijN.Blocked || !(bf.NodeIsValid(ijN)))
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
                    //finish command if paying was unsuccessful
                    return true;

                //find energy source nodes
                Node[,] energySourceNodes;
                if (bf is TreeFactory trF)
                { 
                    int rDist = trF.RootsDistance;
                    energySourceNodes = GameQuerying.GetGameQuerying().SelectNodes(map, nX - rDist, nY - rDist, nX + (size + rDist - 1), nY + (size + rDist - 1));
                }
                else
                {
                    energySourceNodes = buildNodes;
                }
                //put the new building on the main map
                Player newUnitOwner = CommandedEntity.Player;
                Building newBuilding = Ability.BuildingFactory.NewInstance(newUnitOwner, buildNodes, buildNodes);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newBuilding);
                map.AddBuilding(newBuilding);
                game.Map.MapWasChanged = true;
            }
            //the command always immediately finishes regardless of the success of placing the building
            return true;
        }
    }
}