using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
