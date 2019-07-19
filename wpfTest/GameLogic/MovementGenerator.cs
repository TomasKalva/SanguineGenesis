﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic
{
    /*class MovementGenerator
    {//inputs
        private List<UnitView> views;
        private ObstacleMap obstMap;
        private bool newTask;

        //outputs
        private List<FlowMap> visibilityMap;
        public bool Done { get; private set; }
        public VisibilityMap VisibilityMap
        {
            get
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
        public void SetNewTask(ObstacleMap obstMap, List<UnitView> unitViews)
        {
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
        /// Infinite loop for generating visibility maps.
        /// </summary>
        public void Generate()
        {
            while (true)
            {
                lock (this)
                    while (!newTask) Monitor.Wait(this);
                visibilityMap = new VisibilityMap(obstMap.Width, obstMap.Height);
                visibilityMap.FindVisibility(views, obstMap);
                newTask = false;
                Done = true;
            }
        }
    }*/
}