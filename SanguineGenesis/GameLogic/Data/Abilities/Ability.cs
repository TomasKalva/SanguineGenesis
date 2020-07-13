using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Data.Abilities;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GUI;
using SanguineGenesis.GUI.WinFormsControls;

namespace SanguineGenesis.GameLogic.Data.Abilities
{
    /// <summary>
    /// Used to set commands to entities.
    /// </summary>
    abstract class Ability: IShowable
    {
        /// <summary>
        /// Group of abilities this ability belongs to.
        /// </summary>
        protected Abilities abilities;
        /// <summary>
        /// Sets this.abilities and adds reference to this to abilities.
        /// </summary>
        public void SetAbilities(Abilities abilities)
        {
            this.abilities = abilities;
            abilities.AllAbilities.Add(this);
        }

        /// <summary>
        /// Maximal distance from the target where the ability can be use. If null, the attack distance of animal
        /// should be used.
        /// </summary>
        public float? Distance { get; }
        /// <summary>
        /// Energy required to use this ability.
        /// </summary>
        public float EnergyCost { get; }
        /// <summary>
        /// True if this ability should be performed only by one of the selected units.
        /// </summary>
        public bool OnlyOne { get; }
        /// <summary>
        /// True if the target of the ability can be its user.
        /// </summary>
        public bool SelfUseable { get; }
        /// <summary>
        /// True if the command can be removed from the first place in the command queue.
        /// </summary>
        public bool Interruptable { get; }
        /// <summary>
        /// How long it takes to perform this ability.
        /// </summary>
        public float Duration { get; }
        /// <summary>
        /// Returns true if the target type is valid for this ability.
        /// </summary>
        public abstract bool ValidTargetType(ITargetable target);
        /// <summary>
        /// Type of Target.
        /// </summary>
        public abstract Type TargetType { get; }
        /// <summary>
        /// Type of User.
        /// </summary>
        public abstract Type UserType { get; }

        public Ability(float? distance, float energyCost, bool onlyOne, bool selfUseable, bool interruptable, float duration)
        {
            Distance = distance;
            EnergyCost = energyCost;
            OnlyOne = onlyOne;
            SelfUseable = selfUseable;
            Interruptable = interruptable;
            Duration = duration;
        }

        /// <summary>
        /// Set commands to the entities. Calls the generic version of this method.
        /// </summary>
        /// <exception cref="InvalidCastException">If some users or target have incompatible type.</exception>
        /// <exception cref="NullReferenceException">If some users are null.</exception>
        public abstract void SetCommands(IEnumerable<Entity> users, ITargetable target, bool resetCommandQueue, ActionLog actionLog);
        
        /// <summary>
        /// Creates new instance of the command with specified user and target. 
        /// Calls the generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If user or target has incompatible type.</exception>
        public abstract Command NewCommand(Entity user, ITargetable target);

        //IShowable
        public abstract string GetName();
        public abstract List<Stat> Stats();
        public abstract string Description();

        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Returns name of target type visible to the player.
        /// </summary>
        public virtual string TargetName { get; }
    }

    abstract class Ability<User, Target> : Ability where User:Entity 
                                                                    where Target: ITargetable
    {
        public Ability(float? distance, float energyCost, bool onlyOne, bool selfUseable, bool interruptable=true, float duration = 0)
            :base(distance, energyCost, onlyOne, selfUseable, interruptable, duration)
        {
        }

        /// <summary>
        /// Creates new instance of the command with specified user and target. 
        /// Calls the generic version of this method. 
        /// </summary>
        /// <exception cref="InvalidCastException">If user or target has incompatible type.</exception>
        public sealed override Command NewCommand(Entity user, ITargetable target)
        {
            return NewCommand((User)user, (Target)target);
        }

        /// <summary>
        /// Creates new instance of the command with specified user and target.
        /// </summary>
        public abstract Command NewCommand(User user, Target target);

        /// <summary>
        /// Returns false if command with user and target can't be created. The errors are logged.
        /// </summary>
        public virtual bool ValidArguments(User user, Target target, ActionLog actionLog) => true;

        /// <summary>
        /// Set commands to the units. Does nothing if target has wrong type. Calls the generic version of this method.
        /// </summary>
        /// <exception cref="NullReferenceException">If some users are null.</exception>
        public sealed override void SetCommands(IEnumerable<Entity> users, ITargetable target, bool resetCommandQueue, ActionLog actionLog)
        {
            if(target is Target t)
                SetCommands(users.Where(c=>c is User).Cast<User>(), t, resetCommandQueue, actionLog);
        }
        
        /// <summary>
        /// Returns true if target has valid type.
        /// </summary>
        public sealed override bool ValidTargetType(ITargetable target)
        {
            return target is Target;
        }

        /// <summary>
        /// Type of Target.
        /// </summary>
        public sealed override Type TargetType 
            => typeof(Target);

        /// <summary>
        /// Type of User.
        /// </summary>
        public sealed override Type UserType
            => typeof(User);

        /// <summary>
        /// Returns name of target type visible to the player.
        /// </summary>
        public override string TargetName
        {
            get
            {
                // instance of target isn't present during call of this method
                // and static methods can't be in interface, so checking manually is neccessary
                var targetType = TargetType;
                if (targetType == typeof( ICarnivoreFood)) return "CARN FOOD";
                else if (targetType == typeof(IHerbivoreFood)) return "HERB FOOD";
                else if (targetType == typeof(IMovementTarget)) return "MOVE TARG";
                else if (targetType == typeof(Vector2)) return "POINT";
                else if (targetType == typeof(Node)) return "NODE";
                else if (targetType == typeof(Animal)) return "ANIMAL";
                else if (targetType == typeof(Entity)) return "ENTITY";
                else if (targetType == typeof(Unit)) return "UNIT";
                else if (targetType == typeof(Building)) return "BUILDING";
                else if (targetType == typeof(Plant)) return "PLANT";
                else if (targetType == typeof(Corpse)) return "CORPSE";
                else if (targetType == typeof(Structure)) return "STRUCTURE";
                else /*(targetType == typeof(Nothing))*/ return "NOTHING";
            }
        }

        /// <summary>
        /// Sets the command for this ability to all valid users.
        /// </summary>
        /// <param name="users">Users who should receive the command. Only valid users will receive the command.</param>
        /// <param name="target">Target of the new commands.</param>
        /// <param name="resetCommandQueue">If true, the users CommandQueue will be reset.</param>
        public virtual void SetCommands(IEnumerable<User> users, Target target, bool resetCommandQueue, ActionLog actionLog)
        {
            //select users who have this ability
            IEnumerable<User> usersWithThisAbility = users
                .Where(u => u.Abilities.Where(a => a == this).Any());

            //if there are no users with this ability
            if (!usersWithThisAbility.Any())
            {
                actionLog.LogError(null, this, "no user with this ability");
                return;
            }

            //select only users who are valid for this target
            IEnumerable<User> usersWithValidArguments = usersWithThisAbility
                .Where((user) => ValidArguments(user, target, (!OnlyOne)? actionLog : ActionLog.ThrowAway));

            //if there are no valid users
            if (!usersWithValidArguments.Any())
            {
                actionLog.LogError(null, this, "no user with valid target");
                return;
            }

            //select only the users who can pay
            List<User> validUsers = usersWithValidArguments
                .Where((user) =>user.Energy >= EnergyCost)
                .ToList();

            //remove user that is also target if the ability can't be self used
            if (!SelfUseable && target is User self)
            {
                if(validUsers.Contains(self) && validUsers.Count==1)
                {
                    actionLog.LogError(null, this, "user can't use this ability on itself");
                    return;
                }
                validUsers.Remove(self);
            }

            //if there are no users that can pay do nothing
            if (!validUsers.Any()) 
            {
                actionLog.LogError(null, this, "no valid user has enough energy");
                return; 
            }


            //if the ability should be used only by one user,
            //find the most suitable user to use this ability
            if (OnlyOne)
            {
                //minimal nuber of active commands of users
                int minCom = validUsers.Min(u => u.CommandQueue.Count);
                validUsers = validUsers.Where(u=>u.CommandQueue.Count == minCom)
                    .Take(1)//validUsers is nonempty and it has to have item with minimum command queue length
                    .ToList();
            }

            if (resetCommandQueue)
            {
                //reset all commands
                foreach (User u in validUsers)
                {
                    u.ResetCommands();
                    if (u.CommandQueue.Any())
                        actionLog.LogError(u, this, $"user has unremovable commands");
                }
            }

            //move units to the target until the required distance is reached
            if (validUsers.Where(user => user.GetType() == typeof(Animal)).Any() &&
                typeof(Target) != typeof(Nothing))
            {
                var moveTo = abilities.MoveToUse(this);
                moveTo.SetCommands(validUsers
                        .Where(user => user.GetType() == typeof(Animal))
                        .Cast<Animal>(), target, resetCommandQueue, actionLog);
            }

            //give command to each user
            foreach (User u in validUsers)
            {
                //create new command and assign it to c
                Command com = NewCommand(u, target);

                //instead of MoveTo command before com set MoveTo as FollowCommand for com
                if (u is Animal a && typeof(Target) != typeof(Nothing) && com.FollowTarget)
                {
                    MoveToCommand followCommand = (MoveToCommand)a.CommandQueue.Queue.LastOrDefault();
                    a.CommandQueue.Queue.Remove(followCommand);
                    com.FollowCommand = followCommand;
                }
                //set action log to the command to report errors
                com.ActionLog = actionLog;

                u.AddCommand(com);
            }
        }

        public override List<Stat> Stats()
        {
            List<Stat> stats = new List<Stat>()
            {
                new Stat( "Energy cost", EnergyCost.ToString()),
                new Stat( "Distance", Distance==null?"ATT DIST" : Distance.ToString()),
                new Stat( "Self useable", SelfUseable.ToString()),
                new Stat( "Only one", OnlyOne.ToString()),
                new Stat( "Target type", TargetName),
                new Stat( "Interruptable", Interruptable.ToString()),
            };
            return stats;
        }
    }

    /// <summary>
    /// Represents a target on the map.
    /// </summary>
    interface ITargetable
    {
        Vector2 Center { get; }
        /// <summary>
        /// Distance to animal.
        /// </summary>
        float DistanceTo(Entity entity);
    }

    /// <summary>
    /// Place to which an animal can go.
    /// </summary>
    interface IMovementTarget : ITargetable
    {
    }

    /// <summary>
    /// Used as generic parameter for abilities without target.
    /// </summary>
    class Nothing : ITargetable
    {
        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public Vector2 Center => throw new NotImplementedException("Nothing doesn't have a center!");
        /// <summary>
        /// Returns the instance of Nothing.
        /// </summary>
        public static Nothing Get { get; }
        static Nothing()
        {
            Get = new Nothing();
        }
        private Nothing() { }

        /// <summary>
        /// Distance to nothing is 0.
        /// </summary>
        public float DistanceTo(Entity entity) => 0;
    }

    //Marks classes that can manipulate animal's movement.
    interface IAnimalStateManipulator
    {
    }

    /// <summary>
    /// Logs last few errors by user. Used in ability-command pipeline. 
    /// </summary>
    class ActionLog
    {
        public static ActionLog ThrowAway = new ActionLog(1);

        public int Size { get; }
        private readonly Message[] messages;

        /// <summary>
        /// Logged message.
        /// </summary>
        class Message
        {
            public Entity Entity { get; }
            public Ability Ability { get; }
            public string Text { get; }
            public Message(Entity entity, Ability ability, string text)
            {
                Entity = entity;
                Ability = ability;
                Text = text;
            }

            public override string ToString()
            {
                return $"{(Entity != null ? (Entity.EntityType + ", ") : "")}{(Ability != null ? (Ability.GetName() + ": ") : "")}{Text}";
            }
        }

        public ActionLog(int size)
        {
            this.Size = size >= 1 ? size : 1;
            messages = new Message[this.Size];
        }

        /// <summary>
        /// Adds message to the log and moves other messages by one. Can
        /// erase the last message in the log.
        /// </summary>
        private void PushMessage(Message message)
        {
            //move messages by one
            for (int i = Size - 2; i >= 0; i--)
            {
                messages[i + 1] = messages[i];
            }
            messages[0] = message;
        }

        /// <summary>
        /// Log a new message. entity and ability should correspond to the message.
        /// If no corresponding entity or ability exist, use null instead.
        /// </summary>
        public void LogError(Entity entity, Ability ability, string message)
        {
            PushMessage(new Message(entity, ability, message));
        }

        /// <summary>
        /// Returns error messages from oldest to newest separated by \n.
        /// </summary>
        public string GetMessages()
        {
            StringBuilder messagesText = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                messagesText.Append(messages[i]);
                messagesText.Append("\n");
            }
            return messagesText.ToString();
        }
    }
}
