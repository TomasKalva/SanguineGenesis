using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Used for asynchronous creating of visibility maps. Lock for accessing public properties
    /// is the instance.
    /// </summary>
    public class VisibilityGenerator
    {
        //inputs
        /// <summary>
        /// newTask has to be false to set this.
        /// </summary>
        private List<View> views;
        /// <summary>
        /// newTask has to be false to set this.
        /// </summary>
        private ObstacleMap obstMap;
        /// <summary>
        /// Set to true after new task was given. Set to false after completing the task. New task
        /// can't be given if newTask is true. To access, this instance has to be locked.
        /// </summary>
        private bool newTask;

        //outputs
        private bool done;
        public bool Done
        {
            get
            {
                lock (this) return done;
            }
            private set
            {
                lock (this) done = value;
            }
        }
        private VisibilityMap visibilityMap;
        /// <summary>
        /// Done has to be true to get this.
        /// </summary>
        public VisibilityMap VisibilityMap
        {
            get
            {
                lock (this)
                {
                    if (Done)
                    {
                        Done = false;
                        return visibilityMap;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Starts a new thread for creating visibility maps.
        /// </summary>
        public VisibilityGenerator()
        {
            Done = true;
            newTask = false;
            Thread t = new Thread(() => Generate());
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Set parameters for creating the visibility map. Does nothing if the current
        /// task is not done yet.
        /// </summary>
        public void SetNewTask(ObstacleMap obstMap, List<View> unitViews)
        {
            lock(this)
                if (newTask) return;

            this.obstMap = obstMap;
            this.views = unitViews;
            lock (this)
            {
                newTask = true;
                Monitor.Pulse(this);
            }
        }

        /// <summary>
        /// Infinite loop for generating visibility maps. Listens to Pulses on this instance,
        /// the pulse condition is newTask.
        /// </summary>
        public void Generate()
        {
            while (true)
            {
                lock(this)
                    while (!newTask) Monitor.Wait(this);
                visibilityMap = new VisibilityMap(obstMap.Width, obstMap.Height);
                visibilityMap.FindVisibility(views, obstMap);

                lock (this)
                {
                    newTask = false;
                    Done = true;
                }
            }
        }
    }
}
