using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class CommandsGroup
    {
        public List<Entity> Entities { get; private set; }

        public CommandsGroup()
        {
            Entities = new List<Entity>();
        }

        public void SetUnits(List<Entity> units)
        {
            foreach (Entity u in Entities)
                u.Group = null;
            Entities.Clear();
            foreach (Entity u in units)
            {
                Entities.Add(u);
                u.Group = this;
            }
        }

        public void AddUnits(List<Entity> units)
        {
            foreach(Entity u in units)
                if (!Entities.Contains(u))
                {
                    Entities.Add(u);
                    u.Group = this;
                }
        }

        public void RemoveUnits(List<Entity> units)
        {
            foreach (Entity u in units)
                u.Group = null;
            Entities.RemoveAll((unit) => units.Contains(unit));
        }

        public void RemoveDead()
        {
            Entities.RemoveAll((u) => u.IsDead);
        }
        /*
        /// <summary>
        /// Adds a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="ability">Determines command type.</param>
        public void AddCommand(Ability ability, ITargetable target)
        {
            Units.RemoveAll((u) => u.IsDead);
            ability.SetCommands(Players.PLAYER0,Units, target);
            //foreach (Entity u in Units)
            //    u.AddCommand(ability.NewInstance(u));
        }

        /// <summary>
        /// Sets a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="ability">Determines command type.</param>
        public void SetCommand(Ability ability, ITargetable target)
        {
            Units.RemoveAll((u) => u.IsDead);
            ability.SetCommands(Players.PLAYER0, Units, target);
            //foreach (Entity u in Units)
            //    u.SetCommand(commandFactory.NewInstance(u));
        }*/
    }
}
