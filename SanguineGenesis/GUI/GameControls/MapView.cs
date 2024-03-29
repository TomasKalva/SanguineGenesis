﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using SanguineGenesis.GameLogic;
using SanguineGenesis.GameLogic.Data.Entities;
using SanguineGenesis.GameLogic.Maps;
using SanguineGenesis.GameLogic.Maps.MovementGenerating;
using SanguineGenesis.GameLogic.Maps.VisibilityGenerating;
using SanguineGenesis.GUI;

namespace SanguineGenesis.GameControls
{
    /// <summary>
    /// The class describes player's view of the map.
    /// </summary>
    class MapView:IRectangle
    {
        /// <summary>
        /// Width of the control this view is drawn to.
        /// </summary>
        private float actualWidth;
        /// <summary>
        /// Height of the control this view is drawn to.
        /// </summary>
        private float actualHeight;
        /// <summary>
        /// Speed of scrolling through the map. Relative to size of one node.
        /// </summary>
        private readonly float scrollSpeed;
        /// <summary>
        /// Size difference of a node before and after zoom.
        /// </summary>
        private readonly float zoomSpeed;

        /// <summary>
        /// Minimal node size.
        /// </summary>
        private readonly float minNodeSize;
        /// <summary>
        /// Maximal node size.
        /// </summary>
        private readonly float maxNodeSize;
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

        public MapView(float bottom, float left, float nodeSize,
            float minNodeSize = 60, float maxNodeSize = 80, float scrollSpeed = 0.5f, float zoomSpeed = 10)
        {
            Bottom = bottom;
            Left = left;
            NodeSize = nodeSize;
            this.scrollSpeed = scrollSpeed;
            this.zoomSpeed = zoomSpeed;
            this.minNodeSize = minNodeSize;
            this.maxNodeSize = maxNodeSize;
        }

        #region Data retrieval

        /// <summary>
        /// Returns sub-rectangle of the map visible by this MapView.
        /// Returns Node[0, 0] if actual extents haven't been set.
        /// </summary>
        public Node[,] GetVisibleNodes(Map map)
        {
            if (actualHeight == 0 || actualWidth == 0)
                return new Node[0, 0];

            return GameQuerying.SelectPartOfMap(map, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns sub-rectangle of game's FlowField visible by this MapView.
        /// Returns float[0,0] if actual extents haven't been set.
        /// </summary>
        public float?[,] GetVisibleFlowField(FlowField flowField)
        {
            if (actualHeight == 0 || actualWidth == 0)
                return new float?[0, 0];

            return GameQuerying.SelectPartOfMap(flowField, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns sub-rectangle of the VisibilityMap visible by this MapView.
        /// Returns null if the map doesn't exist. Returns null if actual extents haven't been set.
        /// </summary>
        public bool[,] GetVisibleVisibilityMap(VisibilityMap visibilityMap)
        {
            if (actualHeight == 0 || actualWidth == 0)
                return null;
            if (visibilityMap == null)
                return null;

            return GameQuerying.SelectPartOfMap(visibilityMap, ((IRectangle)this).GetRect());
        }

        /// <summary>
        /// Returns all entities visible by this MapView and also visible by observer.
        /// Returns empty list if actual extents haven't been set.
        /// </summary>
        public IEnumerable<Entity> GetVisibleEntities(Game game, Player observer)
        {
            if (actualHeight == 0 || actualWidth == 0)
                return new List<Entity>(); ;

            return GameQuerying
                .SelectVisibleEntities(observer,
                    //sprite can be seen even if entity is slightly out of the window
                    GameQuerying.SelectEntitiesInArea(game, new Rect(Left - 2, Bottom - 4, Right + 2, Top + 1)));
        }

        #endregion Data retrieval

        #region Position and zoom changing

        /// <summary>
        /// Sets extents of the control this MapView is drawn to.
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
        /// Decreases size of viewed area. Returns true if zoom was succesfull.
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
        /// Increases size of viewed area. Returns true if zoom was succesfull.
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
            Left += (oldWidth - Width) / 2;
            Bottom += (oldHeight - Height) / 2;
        }

        /// <summary>
        /// Sets view position back into the map.
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
        /// Sets center of screen to point.
        /// </summary>
        public void CenterTo(Map map, Vector2 point)
        {
            Left = point.X - Width / 2;
            Bottom = point.Y - Height / 2;
            CorrectPosition(map);
        }

        #endregion Position and zoom changing

        /// <summary>
        /// Finds map coordinates (relative to the size of one node) that
        /// correspond to the point on the screen. Returns 0 vector if actual extents haven't been set.
        /// </summary>
        /// <param name="screenPoint">Point on the screen.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public Vector2 ScreenToMap(Vector2 screenPoint)
        {
            if (actualHeight == 0 || actualWidth == 0)
                return new Vector2(0, 0);

            return new Vector2(Left + screenPoint.X / NodeSize,
                Bottom + (actualHeight - screenPoint.Y)/ NodeSize);
        }
    }
}
