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
        public List<Entity> Units { get; private set; }

        public CommandsGroup()
        {
            Units = new List<Entity>();
        }

        public void SetUnits(List<Entity> units)
        {
            foreach (Entity u in Units)
                u.Group = null;
            Units.Clear();
            foreach (Entity u in units)
            {
                Units.Add(u);
                u.Group = this;
            }
        }

        public void AddUnits(List<Entity> units)
        {
            foreach(Entity u in units)
                if (!Units.Contains(u))
                {
                    Units.Add(u);
                    u.Group = this;
                }
        }

        public void RemoveUnits(List<Entity> units)
        {
            foreach (Entity u in units)
                u.Group = null;
            Units.RemoveAll((unit) => units.Contains(unit));
        }

        public void RemoveDead()
        {
            Units.RemoveAll((u) => u.IsDead);
        }

        /// <summary>
        /// Adds a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="commandFactory">Determines command type.</param>
        public void AddCommand(CommandAssignment commandFactory)
        {
            Units.RemoveAll((u) => u.IsDead);
            foreach (Entity u in Units)
                u.AddCommand(commandFactory.NewInstance(u));
        }

        /// <summary>
        /// Sets a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="commandFactory">Determines command type.</param>
        public void SetCommand(CommandAssignment commandFactory)
        {
            Units.RemoveAll((u) => u.IsDead);
            foreach (Entity u in Units)
                u.SetCommand(commandFactory.NewInstance(u));
        }
    }
}
