using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wpfTest.MainWindow;

namespace wpfTest
{
    /// <summary>
    /// The class describes player's view of the map.
    /// Enumerator returns the nodes inside current players view from left top to right bottom.
    /// </summary>
    public class MapView:IEntity
    {
        private float actualWidth;
        private float actualHeight;
        private float scrollSpeed;
        private float zoomSpeed;
        private float minNodeSize;
        private float maxNodeSize;

        public float NodeSize { get; set; }
        //These values are relative to size of one node.
        public float Bottom { get; private set; }
        public float Left { get; private set; }
        public float Width => actualWidth / NodeSize;
        public float Height => actualHeight / NodeSize;
        public float Right => Left + Width;
        public float Top => Bottom + Height;


        public MapView(float top, float left, float nodeSize, Map map, Game game,
            float minNodeSize = 30, float maxNodeSize = 70, float scrollSpeed = 0.5f, float zoomSpeed = 20)
        {
            Bottom = top;
            Left = left;
            NodeSize = nodeSize;
            this.scrollSpeed = scrollSpeed;
            this.zoomSpeed = zoomSpeed;
            this.minNodeSize = minNodeSize;
            this.maxNodeSize = maxNodeSize;
        }

        public Node[,] GetVisibleNodes(Game game)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return game.GameQuerying.SelectPartOfMap(game.Map, ((IEntity)this).GetRect());
        }

        public float[,] GetVisibleFlowMap(Game game)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return game.GameQuerying.SelectPartOfMap(game.FlowMap, ((IEntity)this).GetRect());
        }

        public List<Unit> GetVisibleUnits(Game game)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return game.GameQuerying.SelectUnits(game, ((IEntity)this).GetRect());
        }

        public void SetActualExtents(float width, float height)
        {
            this.actualWidth = width;
            this.actualHeight = height;
        }

        /// <summary>
        /// Moves the view in given direction so that it doesn't leave the map.
        /// </summary>
        /// <param name="dir">The direction.</param>
        public void Move(Direction dir, Map map)
        {
            switch (dir)
            {

                case Direction.DOWN: Bottom -= scrollSpeed; break;
                case Direction.UP: Bottom += scrollSpeed; break;
                case Direction.LEFT: Left -= scrollSpeed; break;
                case Direction.RIGHT: Left += scrollSpeed; break;
            }
            CorrectPosition(map);
        }

        /// <summary>
        /// Decreases size of viewed area.
        /// </summary>
        public bool ZoomIn(Map map)
        {
            float newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize + zoomSpeed, minNodeSize));
            if (newNodeSize != NodeSize)
            {
                NodeSize = newNodeSize;
                CorrectPosition(map);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Increases size of viewed area.
        /// </summary>
        public bool ZoomOut(Map map)
        {
            float newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize - zoomSpeed, minNodeSize));
            if (newNodeSize != NodeSize)
            {
                NodeSize = newNodeSize;
                CorrectPosition(map);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets view position into the map.
        /// </summary>
        public void CorrectPosition(Map map)
        {
            if (Right > map.Width)
                Left = map.Width - Width;
            if (Left < 0)
                Left = 0;
            if (Top > map.Height)
                Bottom = map.Height - Height;
            if (Bottom < 0)
                Bottom = 0;
        }

        /// <summary>
        /// Finds map coordinates (relative to the size of one node) that
        /// correspond to the point on the screen.
        /// </summary>
        /// <param name="screenPoint">Point on the screen.</param>
        public Vector2 ScreenToMap(Vector2 screenPoint)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return new Vector2(Left + screenPoint.X / NodeSize,
                Bottom + (actualHeight - screenPoint.Y)/ NodeSize);
        }
    }
}
