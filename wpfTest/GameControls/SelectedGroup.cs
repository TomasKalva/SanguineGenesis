using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic;

namespace wpfTest
{
    /// <summary>
    /// Represents entities selected by player.
    /// </summary>
    public class SelectedGroup
    {
        private List<Entity> entities;
        /// <summary>
        /// List of currently selected entities. Shouldn't be set to null.
        /// </summary>
        public List<Entity> Entities { get { lock (this) return entities; } private set { entities = value; } }
        /// <summary>
        /// Set to true after Entities was changed.
        /// </summary>
        public bool Changed { get; set; }

        public SelectedGroup()
        {
            entities = new List<Entity>();
            Changed = true;
        }

        /// <summary>
        /// Remove all selected entities and the set them to be entities.
        /// </summary>
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

        /// <summary>
        /// Adds entities to the Entities.
        /// </summary>
        public void AddEntities(List<Entity> entities)
        {
            lock (this)
            {
                foreach(Entity u in entities)
                    if (!Entities.Contains(u))
                    {
                        Entities.Add(u);
                        u.Selected = true;
                    }
                Changed = true;
            }
        }

        /// <summary>
        /// Removes entity from Entities.
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            lock (this)
            {
                entity.Selected = false;
                Entities.Remove(entity);
                Changed = true;
            }
        }

        /// <summary>
        /// Removes dead entities from Entities.
        /// </summary>
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

        /// <summary>
        /// Remove all entities.
        /// </summary>
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
    }
}
