using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;

namespace SanguineGenesis.GameControls
{
    /// <summary>
    /// Represents entities selected by player.
    /// </summary>
    class SelectedGroup
    {
        /// <summary>
        /// Entities to this group are added during selecting.
        /// </summary>
        private List<Entity> TemporaryGroup { get; }
        /// <summary>
        /// Entities in this group are changed when selecting is over.
        /// </summary>
        private List<Entity> TotalGroup { get; set; }
        /// <summary>
        /// Operation that will be used to transfer from temporary group
        /// to total group.
        /// </summary>
        public Operation NextOperation { get; set; }
        /// <summary>
        /// List of currently selected entities.
        /// </summary>
        public List<Entity> Entities() => ComposeGroups(NextOperation, TotalGroup, TemporaryGroup);
        /// <summary>
        /// Set to true after Entities() is changed. Set to false after object using data of this instance
        /// acknowledges this change.
        /// </summary>
        public bool Changed { get; set; }

        public SelectedGroup()
        {
            TemporaryGroup = new List<Entity>();
            TotalGroup = new List<Entity>();
            NextOperation = Operation.ALREADY_SELECTED;
            Changed = true;
        }

        /// <summary>
        /// Remove all temporarily selected entities and the set them to be entities.
        /// </summary>
        public void SetTemporaryEntities(List<Entity> entities)
        {
            if (NextOperation == Operation.REPLACE &&
                TotalGroup.Any())
                ClearTotal();
            foreach(Entity e in TotalGroup)
                e.Selected = true;

            ClearTemporary();
            foreach (Entity e in entities)
            {
                TemporaryGroup.Add(e);
                if (NextOperation == Operation.SUBTRACT)
                    e.Selected = false;
                else
                    e.Selected = true;
            }

            Changed = true;
        }

        /// <summary>
        /// Removes entity from Entities().
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            entity.Selected = false;
            TemporaryGroup.Remove(entity);
            TotalGroup.Remove(entity);
            Changed = true;
        }

        /// <summary>
        /// Removes dead entities from Entities().
        /// </summary>
        public void RemoveDead()
        {
            int count = TemporaryGroup.Count;
            TemporaryGroup.RemoveAll((e) => { count++; return e.IsDead; });
            TotalGroup.RemoveAll((e) => { count++; return e.IsDead; });
            if (count != 0)
                Changed = true;
        }
        
        /// <summary>
        /// Remove temporarily selected entities.
        /// </summary>
        public void ClearTemporary()
        {
            foreach (Entity e in TemporaryGroup)
                e.Selected = false;
            TemporaryGroup.Clear();
            Changed = true;
        }

        /// <summary>
        /// Remove selected entities.
        /// </summary>
        public void ClearTotal()
        {
            foreach (Entity e in TotalGroup)
                e.Selected = false;
            TotalGroup.Clear();
            Changed = true;
        }

        /// <summary>
        /// Transfers TemporaryGroup to TotalGroup using NextOperation. 
        /// </summary>
        public void CommitEntities()
        {
            var newTotalGroup = ComposeGroups(NextOperation, TotalGroup, TemporaryGroup);
            ClearTemporary();
            ClearTotal();
            TotalGroup = newTotalGroup;
            foreach (Entity e in TotalGroup)
                e.Selected = true;
            NextOperation = Operation.ALREADY_SELECTED;
        }

        /// <summary>
        /// Commit entities and keep only the ones with the given type.
        /// </summary>
        public void KeepSelected(string entityType)
        {
            CommitEntities();
            foreach(var e in TotalGroup.Where(e => e.EntityType != entityType))
                e.Selected = false;
            TotalGroup = TotalGroup.Where(e => e.EntityType == entityType).ToList();
        }

        /// <summary>
        /// Composes the two groups using the operation and returns the result.
        /// </summary>
        private List<Entity> ComposeGroups(Operation opearation, List<Entity> total, List<Entity> temporary)
        {
            switch (opearation)
            {
                case Operation.ADD:
                    return total.Union(temporary).ToList();
                case Operation.SUBTRACT:
                    return total.Where(e => !temporary.Contains(e)).ToList();
                case Operation.REPLACE:
                    return temporary.ToList();
                default:
                    return total.ToList();
            }
        }
    }

    public enum Operation
    {
        ADD,
        SUBTRACT,
        REPLACE,
        ALREADY_SELECTED//if entities are currently not being selected
    }
}
