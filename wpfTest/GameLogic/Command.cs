using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;
using wpfTest.GameLogic.Data.Entities;
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
        protected Command() { }
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

            if (CommandedEntity.Energy >= Ability.EnergyCost)
            {
                //pay for the ability
                CommandedEntity.Energy -= Ability.EnergyCost;
                Paid = true;
                return true;
            }
            //not enough energy
            return false;
        }

        /// <summary>
        /// Returns if entity can pay this ability.
        /// </summary>
        protected bool CanPay()
        {
            if (Paid)
                //the ability was paid already
                return true;
            else
                return CommandedEntity.Energy >= Ability.EnergyCost;
        }
        
        /// <summary>
        /// Returns true iff the target can be used as target.
        /// </summary>
        protected bool ValidTarget()
        {
            Entity e = Targ as Entity;
            if (e != null)
                return e.CanBeTarget && !e.IsDead;
            return true;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!ValidTarget())
                //finish if the target is invalid
                return true;

            if (!TryPay())
                //finish command if paying was unsuccessful
                return true;

            return PerformCommandLogic(game, deltaT);
        }

        public abstract bool PerformCommandLogic(Game game, float deltaT);
    }

    public class AttackCommand : Command<Animal, Entity, Attack>
    {
        private float timeUntilAttack;//time in s until this unit attacks
        
        public AttackCommand(Animal commandedEntity, Entity target, Attack attack)
            : base(commandedEntity, target, attack)
        {
            this.timeUntilAttack = 0f;
        }
        
        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint( Targ.Center );

            CommandedEntity.CanBeMoved = false;
            timeUntilAttack += deltaT;
            if (timeUntilAttack >= CommandedEntity.AttackPeriod)
            {
                timeUntilAttack -= CommandedEntity.AttackPeriod;

                if (Targ is Building && !CommandedEntity.MechanicalDamage)
                {
                    //deal less damage if animal without mechanical damage attacks a building
                    Targ.Damage(CommandedEntity.AttackDamage/10);
                }
                else
                {
                    //damage target
                    Targ.Damage(CommandedEntity.AttackDamage);
                }
            }

            bool finished = CommandedEntity.DistanceTo(Targ) >= CommandedEntity.AttackDistance;
            if (finished)
            {
                return true;
            }
            return false;
        }
    }

    public class PoisonousSpitCommand : Command<Animal, Animal, PoisonousSpit>
    {
        private float spitTimer;//time in s until this unit attacks
        
        public PoisonousSpitCommand(Animal commandedEntity, Animal target, PoisonousSpit attack)
            : base(commandedEntity, target, attack)
        {
            this.spitTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(CommandedEntity.Center);
            
            spitTimer += deltaT;
            if (spitTimer >= Ability.TimeUntilSpit)
            {
                //apply poison to the target and finish the command
                Ability.PoisonFactory.ApplyToAffected(Targ);
                return true;
            }

            //command doesn't finish until it was spat
            return false;
        }
    }

    public class ApplyStatusCommand : Command<Animal, Nothing, ApplyStatus>
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

    public class PiercingBiteCommand : Command<Animal, Animal, PiercingBite>
    {
        private float timer;

        public PiercingBiteCommand(Animal commandedEntity, Animal target, PiercingBite bite)
            : base(commandedEntity, target, bite)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer > Ability.TimeToAttack)
            {
                Targ.Damage(Ability.Damage);
                return true;
            }

            return false;
        }
    }

    public class ConsumeAnimalCommand : Command<Animal, Animal, ConsumeAnimal>
    {
        private float timer;

        public ConsumeAnimalCommand(Animal commandedEntity, Animal target, ConsumeAnimal bite)
            : base(commandedEntity, target, bite)
        {
            timer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            timer += deltaT;
            if (timer > Ability.TimeToConsume)
            {
                ConsumedAnimalFactory consumedFact = Ability.ConsumedAnimalFactory;
                consumedFact.AnimalConsumed = Targ;
                consumedFact.ApplyToAffected(CommandedEntity);
                return true;
            }

            return false;
        }
    }

    public interface IComputable
    {
        MoveToCommandAssignment Assignment { get; set; }
    }

    public class MoveToPointCommand : Command<Animal, IMovementTarget, MoveTo>,IComputable
    {
        public MoveToCommandAssignment Assignment { get; set; }
        private MoveToLogic moveToLogic;
        /// <summary>
        /// Flowmap used for navigation. It can be set after the command was assigned.
        /// </summary>
        private FlowMap flowMap;
        

        private MoveToPointCommand()  => throw new NotImplementedException();
        public MoveToPointCommand(Animal commandedEntity, IMovementTarget target, float minStoppingDistance, MoveTo ability)
            : base(commandedEntity, target, ability)
        {
            moveToLogic = new MoveToLogic(CommandedEntity, null, minStoppingDistance, target, Ability);
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!ValidTarget())
            {
                //finish if the target is invalid
                CommandedEntity.StopMoving = true;
                return true;
            }

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

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");


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
        private Animal unit;
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

        public MoveToLogic(Animal unit, FlowMap flowMap, float minStoppingDistance, IMovementTarget target, IMovementParametrizing movementParametrizing)
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
                    unit.SetCommand(new AttackCommand(unit, enemy, game.CurrentPlayer.GameStaticData.Abilities.Attack));
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
                unit.Accelerate(flowMap.GetIntensity(unit.Center, unit.Acceleration), game.Map);
            }
            else
            {
                //go in straight line
                Vector2 direction = unit.Center.UnitDirectionTo(TargetPoint.Center);
                unit.Accelerate(unit.Acceleration * direction, game.Map);
            }
            //update last four positions
            noMovementDetection.AddNextPosition(unit.Center);
            //set that unit wants to move
            unit.WantsToMove = true;

            bool finished=Finished();

            //command is finished if unit reached the goal distance or if it stayed at one
            //place near the target position for a long time
            if (finished //unit is close to the target point
                || (noMovementDetection.NotMovingMuch(deltaT, unit.MaxSpeedLand * deltaT / 2) && CanStop())//unit is stuck
                /*|| game.Players[unit.Player.PlayerID].MapView
                    .GetObstacleMap(unit.Movement)[(int)TargetPoint.Center.X,(int)TargetPoint.Center.Y]*/)//target point is blocked
            {
                return true;
            }
            return false;
        }

        public bool Finished()
        {
            if (TargetPoint is Entity entity)
            {
                //target is entity
                //use distance between closest points of the unit and the target
                if (movementParametrizing.UsesAttackDistance)
                    return unit.DistanceTo(entity) <= unit.AttackDistance;
                else
                    return unit.DistanceTo(entity) <= movementParametrizing.GoalDistance;
            }
            else if(TargetPoint is Vector2 vector)
            {
                //target is vector
                //use distance between center of the unit and the point
                return (vector - unit.Center).Length <= movementParametrizing.GoalDistance;
            }
            else
            {
                Node node = TargetPoint as Node;
                return unit.DistanceTo(node) <= movementParametrizing.GoalDistance;
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

        private SpawnCommand() => throw new NotImplementedException();
        public SpawnCommand(Entity commandedEntity, Vector2 target, Spawn spawn)
            : base(commandedEntity, target, spawn)
        {
            SpawnTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            SpawnTimer += deltaT;
            if (SpawnTimer >= Ability.SpawningUnitFactory.SpawningTime)
            {
                Player newUnitOwner = CommandedEntity.Player;
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, Targ);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                return true;
            }
            return false;
        }
    }

    public class SetRallyPointCommand : Command<Building, Vector2, SetRallyPoint>
    {
        public SetRallyPointCommand(Building commandedEntity, Vector2 target, SetRallyPoint spawn)
            : base(commandedEntity, target, spawn)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.RallyPoint = Targ;
            return true;
        }
    }

    public class CreateUnitCommand : Command<Building, Nothing, CreateUnit>
    {
        /// <summary>
        /// How long the unit was spawning in s.
        /// </summary>
        public float SpawnTimer { get; private set; }

        private CreateUnitCommand() => throw new NotImplementedException();
        public CreateUnitCommand(Building commandedEntity, Nothing target, CreateUnit spawn)
            : base(commandedEntity, target, spawn)
        {
            SpawnTimer = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            SpawnTimer += deltaT;
            if (SpawnTimer >= Ability.SpawningUnitFactory.SpawningTime)
            {
                Player newUnitOwner = CommandedEntity.Player;
                Vector2 newUnitPosition = new Vector2(CommandedEntity.Center.X, CommandedEntity.Bottom - Ability.SpawningUnitFactory.Range);
                Animal newUnit = Ability.SpawningUnitFactory.NewInstance(newUnitOwner, newUnitPosition);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newUnit);
                //make unit go towards the rally point
                newUnitOwner.GameStaticData.Abilities.MoveTo.SetCommands(new List<Unit>(1) { newUnit }, CommandedEntity.RallyPoint);
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
                Building newBuilding = Ability.BuildingFactory.NewInstance(newUnitOwner, buildNodes, energySourceNodes);
                game.Players[newUnitOwner.PlayerID].Entities.Add(newBuilding);
                map.AddBuilding(newBuilding);
                game.Map.MapWasChanged = true;

                //if the new building is a tree, make it grow
                if(newBuilding.GetType()==typeof(Tree))
                    newUnitOwner.GameStaticData.Abilities.Grow.SetCommands(new List<Tree>(1){ (Tree)newBuilding}, Nothing.Get);
            }
            //the command always immediately finishes regardless of the success of placing the building
            return true;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");
    }

    public class GrowCommand : Command<Tree, Nothing, Grow>
    {
        private GrowCommand() => throw new NotImplementedException();
        public GrowCommand(Tree commandedEntity, Grow plantBuilding)
            : base(commandedEntity, Nothing.Get, plantBuilding)
        {
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            //finish if the tree is at full energy
            if (CommandedEntity.Energy == CommandedEntity.MaxEnergy)
            {
                CommandedEntity.Built = true;
                return true;
            }
            else
                return false;
            
        }
    }

    public class HerbivoreEatCommand : Command<Animal, IHerbivoreFood, HerbivoreEat>
    {
        /// <summary>
        /// Time in s until this animal eats.
        /// </summary>
        private float timeUntilEating;

        public HerbivoreEatCommand(Animal commandedEntity, IHerbivoreFood target, HerbivoreEat eat)
            : base(commandedEntity, target, eat)
        {
            this.timeUntilEating = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Targ.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Targ.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilEating += deltaT;
            if (timeUntilEating >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Targ.EatFood(CommandedEntity);
                //reset timer
                timeUntilEating -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }
    }

    public class CarnivoreEatCommand : Command<Animal, ICarnivoreFood, CarnivoreEat>
    {
        /// <summary>
        /// Time in s until this animal eats.
        /// </summary>
        private float timeUntilEating;

        public CarnivoreEatCommand(Animal commandedEntity, ICarnivoreFood target, CarnivoreEat eat)
            : base(commandedEntity, target, eat)
        {
            this.timeUntilEating = 0f;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            if (!Targ.FoodLeft)
            {
                CommandedEntity.CanBeMoved = true;
                return true;
            }

            CommandedEntity.Direction = Targ.Center - CommandedEntity.Center;

            CommandedEntity.CanBeMoved = false;
            timeUntilEating += deltaT;
            if (timeUntilEating >= CommandedEntity.FoodEatingPeriod)
            {
                //eat
                Targ.EatFood(CommandedEntity);
                //reset timer
                timeUntilEating -= CommandedEntity.FoodEatingPeriod;
            }

            return false;
        }
    }

    public class JumpCommand : Command<Animal, Vector2, Jump>
    {
        private float timer;
        /// <summary>
        /// The unit is jumping.
        /// </summary>
        private bool jumping;
        private MoveAnimalToPoint moveAnimalToPoint;

        public JumpCommand(Animal commandedEntity, Vector2 target, Jump jump)
            : base(commandedEntity, target, jump)
        {
            timer = 0f;
            jumping = false;
            moveAnimalToPoint = new MoveAnimalToPoint(commandedEntity, target, Ability.JumpTime);
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
        {
            CommandedEntity.TurnToPoint(Targ);

            timer += deltaT;
            if (!jumping)
            { 
                //animal is preparing to jump
                if (timer >= Ability.PreparationTime)
                {
                    jumping = true;
                    timer -= Ability.PreparationTime;
                }
            }
            
            if(jumping)
            {
                return moveAnimalToPoint.Step(deltaT);
            }

            //command doesn't finish until the animal jumps
            return false;
        }
    }

    public class PullCommand : Command<Animal, Animal, Pull>
    {
        private float timer;
        /// <summary>
        /// The unit is jumping.
        /// </summary>
        private bool pulling;
        private MoveAnimalToPoint moveAnimalToPoint;
        private bool firstPullingStep;

        public PullCommand(Animal commandedEntity, Animal target, Pull pull)
            : base(commandedEntity, target, pull)
        {
            timer = 0f;
            pulling = false;
            CommandedEntity.TurnToPoint(Targ.Position);
            Vector2 frontOfAnimal = commandedEntity.Position + (commandedEntity.Range + target.Range) * commandedEntity.Direction;
            moveAnimalToPoint = new MoveAnimalToPoint(target, frontOfAnimal, Ability.PullTime);
            firstPullingStep = true;
        }

        public override bool PerformCommand(Game game, float deltaT)
        {
            if (!TryPay())
                return true;

            CommandedEntity.TurnToPoint(Targ.Position);

            timer += deltaT;
            if (!pulling)
            {
                //animal is preparing to jump
                if (timer >= Ability.PreparationTime)
                {
                    pulling = true;
                    timer -= Ability.PreparationTime;
                }
            }

            if (pulling)
            {
                if (firstPullingStep)
                { 
                    if(!Targ.CanBeTarget)
                        //target can't be used as target => fail the ability
                        return true;

                    firstPullingStep = false;
                }
                return moveAnimalToPoint.Step(deltaT);
            }

            //command doesn't finish until the animal pulls the target animal
            return false;
        }

        public override bool PerformCommandLogic(Game game, float deltaT)
            => throw new NotImplementedException("This method is never used.");
    }

    /// <summary>
    /// Moves animal to point on the map with even movement in given time. During the movement
    /// the animal doesn't check for collisions and can't be used as a target for a command.
    /// </summary>
    public class MoveAnimalToPoint
    {
        public Animal Animal { get; }
        public Vector2 Point { get; }
        public float Time { get; }
        public float Speed { get; }
        private float timer;

        public MoveAnimalToPoint(Animal animal, Vector2 point, float time)
        {
            Animal = animal;
            Point = point;
            Time = time;
            timer = 0f;
            Speed = (animal.Position - point).Length / time;
        }

        /// <summary>
        /// Returns true if the unit reached its destination.
        /// </summary>
        public bool Step(float deltaT)
        {
            //animal can jump over all obstacles
            Animal.Physical = false;
            Animal.CanBeTarget = false;
            //the animal can't move naturally
            Animal.Velocity = new Vector2(0, 0);
            //calculate maximal displacement of Animal
            float posChange = deltaT * Speed;
            float distanceToPoint = (Animal.Position - Point).Length;
            float speed = Math.Min(posChange, distanceToPoint);
            //change Animal's position
            Animal.Position += speed * Animal.Position.UnitDirectionTo(Point);

            //finish command if Animal is close enough or the time is over
            if (timer >= Time || posChange >= distanceToPoint)
            {
                Animal.Physical = true;
                Animal.CanBeTarget = true;
                return true;
            }

            return false;
        }
    }
}