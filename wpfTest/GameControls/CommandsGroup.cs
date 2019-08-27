using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class SelectedGroup
    {
        /// <summary>
        /// List of currently selected entities. Shouldn't be set to null.
        /// </summary>
        public List<Entity> Entities { get; private set; }
        /// <summary>
        /// Set to true after Entities was changed.
        /// </summary>
        public bool Changed { get; set; }

        public SelectedGroup()
        {
            Entities = new List<Entity>();
            Changed = true;
        }

        public void SetEntities(List<Entity> entities)
        {
            lock (this)
            {
                foreach (Entity e in Entities)
                    e.Selected = false;
                Entities.Clear();
                foreach (Entity e in entities)
                {
                    Entities.Add(e);
                    e.Selected = true;
                }
                Changed = true;
            }
        }

        public void AddEntities(List<Entity> units)
        {
            lock (this)
            {
                foreach(Entity u in units)
                    if (!Entities.Contains(u))
                    {
                        Entities.Add(u);
                        u.Selected = true;
                    }
                Changed = true;
            }
        }

        public void RemoveEntity(Entity entity)
        {
            lock (this)
            {
                entity.Selected = false;
                Entities.Remove(entity);
                Changed = true;
            }
        }

        public void RemoveDead()
        {
            lock (this)
            {
                int count = Entities.Count;
                Entities.RemoveAll((u) => u.IsDead);
                if (count != Entities.Count)
                    Changed = true;
            }
        }

        public void Clear()
        {
            lock (this)
            {
                foreach (Entity e in Entities)
                    e.Selected = false;
                Entities.Clear();
                Changed = true;
            }
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
