using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest.GameLogic
{
    public class VisibilityGenerator
    {
        //inputs
        private List<UnitView> views;
        private ObstacleMap obstMap;
        private bool newTask;

        //outputs
        private VisibilityMap visibilityMap;
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

        public VisibilityGenerator()
        {
            Done = true;
            newTask = false;
            Thread t = new Thread(() => Generate());
            t.Start();
        }

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

        public void Generate()
        {
            while (true)
            {
                lock(this)
                    while (!newTask) Monitor.Wait(this);
                visibilityMap = new VisibilityMap(obstMap.Width, obstMap.Height);
                visibilityMap.FindVisibility(views, obstMap);
                newTask = false;
                Done = true;
            }
        }
    }
}
