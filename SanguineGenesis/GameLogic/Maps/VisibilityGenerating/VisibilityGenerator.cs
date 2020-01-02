using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis.GameLogic.Maps.VisibilityGenerating
{
    /// <summary>
    /// Used for asynchronous creating of visibility maps. Lock for accessing public properties
    /// is the instance.
    /// </summary>
    class VisibilityGenerator
    {
        private static VisibilityGenerator visibilityGenerator;

        static VisibilityGenerator(){
            visibilityGenerator = new VisibilityGenerator();
        }

        public static VisibilityGenerator Get => visibilityGenerator;

        //inputs
        /// <summary>
        /// newTask has to be false to set this.
        /// </summary>
        private IVisibilityGeneratingTask task;
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
        /// Done has to be true to get calculated visiblity map. Returns null otherwise.
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
        private VisibilityGenerator()
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
        public void SetNewTask(/*ObstacleMap obstMap, List<View> unitViews*/IVisibilityGeneratingTask task)
        {
            lock(this)
                if (newTask) return;

            //this.obstMap = obstMap;
            //this.views = unitViews;
            this.task = task;
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
                visibilityMap = task.GenerateVisibilityMap();

                lock (this)
                {
                    newTask = false;
                    Done = true;
                }
            }
        }
    }

    /// <summary>
    /// Represents task that can be computed by VisibilityGenerator.
    /// </summary>
    interface IVisibilityGeneratingTask
    {
        VisibilityMap GenerateVisibilityMap();
    }

    class RayVisibilityGeneratingTask : IVisibilityGeneratingTask
    {
        private VisibilityMap VisibilityMap { get; }
        private List<View> Views { get; }
        private ObstacleMap VisionObstacleMap { get; }

        public RayVisibilityGeneratingTask(ObstacleMap obstMap, List<View> views)
        {
            this.VisibilityMap = new VisibilityMap(obstMap.Width, obstMap.Height);
            Views = views;
            VisionObstacleMap = obstMap;
        }

        /// <summary>
        /// Sets true to all squares visible by the entities.
        /// </summary>
        public VisibilityMap GenerateVisibilityMap()
        {
            foreach (View v in Views)
            {
                AddVisibility(v);
            }
            return VisibilityMap;
        }

        /// <summary>
        /// Set true to all squares visible by the entity.
        /// </summary>
        public void AddVisibility(View v)
        {
            float viewRange = v.Range;
            int left = (int)(v.Position.X - viewRange) - 1;
            int right = (int)(v.Position.X + viewRange) + 1;
            int bottom = (int)(v.Position.Y - viewRange) - 1;
            int top = (int)(v.Position.Y + viewRange) + 1;
            //cast rays to the lines on bottom and top of the square around v
            for (int i = left; i <= right; i++)
            {
                Ray rTop = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(i, top),
                    viewRange,
                    VisionObstacleMap);
                int x; int y;
                while (rTop.Next(out x, out y))
                    VisibilityMap[x, y] = true;
                if (x != -1 && y != -1)
                    VisibilityMap[x, y] = true;
                Ray rBottom = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(i, bottom),
                    viewRange,
                    VisionObstacleMap);
                while (rBottom.Next(out x, out y))
                    VisibilityMap[x, y] = true;
                if (x != -1 && y != -1)
                    VisibilityMap[x, y] = true;
            }
            //cast rays to the lines on left and right of the square around v
            for (int j = bottom; j <= top; j++)
            {
                Ray rLeft = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(left, j),
                    viewRange,
                    VisionObstacleMap);
                int x; int y;
                while (rLeft.Next(out x, out y))
                    VisibilityMap[x, y] = true;
                if (x != -1 && y != -1)
                    VisibilityMap[x, y] = true;
                Ray rRight = new Ray(new Vector2(v.Position.X, v.Position.Y),
                    new Vector2(right, j),
                    viewRange,
                    VisionObstacleMap);
                while (rRight.Next(out x, out y))
                    VisibilityMap[x, y] = true;
                if (x != -1 && y != -1)
                    VisibilityMap[x, y] = true;
            }
            //add the square which contains v
            int vX = (int)v.Position.X;
            int vY = (int)v.Position.Y;
            if (vX >= 0 && vX < VisibilityMap.Width && vY >= 0 && vY < VisibilityMap.Height)
                VisibilityMap[vX, vY] = true;
        }
    }

    /// <summary>
    /// The whole map is visible
    /// </summary>
    class UnlimitedVisibilityGeneratingTask : IVisibilityGeneratingTask
    {
        private VisibilityMap VisibilityMap { get; }

        public UnlimitedVisibilityGeneratingTask(int width, int height)
        {
            this.VisibilityMap = new VisibilityMap(width, height);
        }

        public VisibilityMap GenerateVisibilityMap()
        {
            for (int i = 0; i < VisibilityMap.Width; i++)
                for (int j = 0; j < VisibilityMap.Height; j++)
                    VisibilityMap[i, j] = true;
            //sleep so that the updates aren't too frequent
            Thread.Sleep(10);
            return VisibilityMap;
        }
    }

    /// <summary>
    /// Describes view of an entity. Used as parameter for VisibilityMapGenerator, which
    /// works in different thread than the game thread.
    /// </summary>
    public struct View
    {
        /// <summary>
        /// Position of the entity.
        /// </summary>
        public Vector2 Position { get; }
        /// <summary>
        /// View range of the entity.
        /// </summary>
        public float Range { get; }

        public View(Vector2 position, float range)
        {
            Position = position;
            Range = range;
        }
    }
}
