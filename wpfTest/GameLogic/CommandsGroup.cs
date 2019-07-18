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
        public List<Unit> Units { get; private set; }

        public CommandsGroup()
        {
            Units = new List<Unit>();
        }

        public void SetUnits(List<Unit> units)
        {
            foreach (Unit u in Units)
                u.Group = null;
            Units.Clear();
            foreach (Unit u in units)
            {
                Units.Add(u);
                u.Group = this;
            }
        }

        public void AddUnits(List<Unit> units)
        {
            foreach(Unit u in units)
                if (!Units.Contains(u))
                {
                    Units.Add(u);
                    u.Group = this;
                }
        }

        public void RemoveUnits(List<Unit> units)
        {
            foreach (Unit u in units)
                u.Group = null;
            Units.RemoveAll((unit) => units.Contains(unit));
        }

        /// <summary>
        /// Adds a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="commandFactory">Determines command type.</param>
        public void AddCommand(CommandAssignment commandFactory)
        {
            foreach (Unit u in Units)
                u.AddCommand(commandFactory.NewInstance(u));
        }

        /// <summary>
        /// Sets a new command created by the factory to every entity in the group.
        /// </summary>
        /// <param name="commandFactory">Determines command type.</param>
        public void SetCommand(CommandAssignment commandFactory)
        {
            foreach (Unit u in Units)
                u.SetCommand(commandFactory.NewInstance(u));
        }
    }
}
