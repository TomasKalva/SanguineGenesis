using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;

namespace SanguineGenesis
{
    /// <summary>
    /// The class describes player's view of the map.
    /// </summary>
    class MapView:IRectangle
    {
        /// <summary>
        /// Width of the component this view is drawn to.
        /// </summary>
        private float actualWidth;
        /// <summary>
        /// Height of the component this view is drawn to.
        /// </summary>
        private float actualHeight;
        /// <summary>
        /// Speed of scrolling through the map. Relative to size of one node.
        /// </summary>
        private float scrollSpeed;
        /// <summary>
        /// Size difference of a node before and after zoom.
        /// </summary>
        private float zoomSpeed;

        /// <summary>
        /// Minimal node size.
        /// </summary>
        private float minNodeSize;
        /// <summary>
        /// Maximal node size.
        /// </summary>
        private float maxNodeSize;
        /// <summary>
        /// Size of a node on screen.
        /// </summary>
        public float NodeSize { get; set; }
        
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Bottom { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Left { get; private set; }
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Width => actualWidth / NodeSize;
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Height => actualHeight / NodeSize;
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Right => Left + Width;
        /// <summary>
        /// In map coordinates.
        /// </summary>
        public float Top => Bottom + Height;


        public MapView(float top, float left, float nodeSize, Map map,
            float minNodeSize = 50, float maxNodeSize = 70, float scrollSpeed = 0.5f, float zoomSpeed = 20)
        {
            Bottom = top;
            Left = left;
            NodeSize = nodeSize;
            this.scrollSpeed = scrollSpeed;
            this.zoomSpeed = zoomSpeed;
            this.minNodeSize = minNodeSize;
            this.maxNodeSize = maxNodeSize;
        }

        /// <summary>
        /// Returns sub-rectangle of the map visible by this MapView.
        /// </summary>
        /// <exception cref="InvalidOperationException">If actual extents haven't been set.</exception>
        public Node[,] GetVisibleNodes(Map map)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return GameQuerying.SelectPartOfMap(map, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns sub-rectangle of game's FlowField visible by this MapView.
        /// </summary>
        /// <exception cref="InvalidOperationException">If actual extents haven't been set.</exception>
        public float?[,] GetVisibleFlowField(FlowField flowField)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return GameQuerying.SelectPartOfMap(flowField, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns sub-rectangle of the VisibilityMap visible by this MapView.
        /// Returns null if the map doesn't exist.
        /// </summary>
        /// <exception cref="InvalidOperationException">If actual extents haven't been set.</exception>
        public bool[,] GetVisibleVisibilityMap(VisibilityMap visibilityMap)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");
            if (visibilityMap == null)
                return null;

            return GameQuerying.SelectPartOfMap(visibilityMap, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns all entities visible by this MapView and also visible by observer.
        /// </summary>
        /// <exception cref="InvalidOperationException">If actual extents haven't been set.</exception>
        public List<Entity> GetVisibleEntities(Game game, Player observer)
        {
            if (actualHeight == 0 || actualWidth == 0)
                throw new InvalidOperationException(
                    "The actual extents have to be specified before calling this method");

            return GameQuerying
                .SelectVisibleEntities(observer, game.GetAll<Entity>())
                .ToList();
        }

        /// <summary>
        /// Sets extents of the component this MapView is drawn to.
        /// </summary>
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
        /// Decreases size of viewed area. Returns true iff zoom was succesfull.
        /// </summary>
        public bool ZoomIn(Map map)
        {
            float newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize + zoomSpeed, minNodeSize));
            if (newNodeSize != NodeSize)
            {
                ChangeNodeSizeAndCenterView(newNodeSize);
                CorrectPosition(map);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Increases size of viewed area. Returns true iff zoom was succesfull.
        /// </summary>
        public bool ZoomOut(Map map)
        {
            float newNodeSize = Math.Min(maxNodeSize, Math.Max(NodeSize - zoomSpeed, minNodeSize));
            if (newNodeSize != NodeSize)
            {
                ChangeNodeSizeAndCenterView(newNodeSize);
                CorrectPosition(map);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes NodeSize and keeps center of the view on the same point of the
        /// map if possible.
        /// </summary>
        private void ChangeNodeSizeAndCenterView(float newNodeSize)
        {
            float oldWidth = Width;
            float oldHeight = Height;
            NodeSize = newNodeSize;

            //center the view
            Left = Left + (oldWidth - Width) / 2;
            Bottom = Bottom + (oldHeight - Height) / 2;
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
        /// <exception cref="InvalidOperationException">If actual extents haven't been set.</exception>
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
