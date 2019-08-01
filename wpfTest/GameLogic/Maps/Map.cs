﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using wpfTest.GameLogic;

namespace wpfTest
{
    public class Map:IMap<Node>
    {
        private Node[,] nodes { get; set; }
        public Node this[int i, int j]
        {
            get => nodes[i, j];
            set => nodes[i, j] = value;
        }
        public int Width => nodes.GetLength(0);
        public int Height => nodes.GetLength(1);
        //color is in rgb format
        private Dictionary<int, Terrain> colorToTerrain;
        public bool MapWasChanged { get; set; }//set to true after building was added/removed etc
        /// <summary>
        /// Obstacle maps for the current map. Is updated by the UpdateObstacleMaps.
        /// </summary>
        public Dictionary<Movement, ObstacleMap> ObstacleMaps { get; }

        public Map(PixelColor[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            nodes = new Node[width, height];
            InitializeColorToTerrain();
            for(int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    nodes[i, j] = new Node(i, j, colorToTerrain[map[i, j].RGB]);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            mg.SetMapChanged(wpfTest.Players.PLAYER0, ObstacleMaps);
            mg.SetMapChanged(wpfTest.Players.PLAYER1, ObstacleMaps);
        }

        private void InitializeColorToTerrain()
        {
            colorToTerrain = new Dictionary<int, Terrain>();
            colorToTerrain.Add(new PixelColor(0, 255, 0).RGB, Terrain.LOW_GRASS);
            colorToTerrain.Add(new PixelColor(0, 0, 255).RGB, Terrain.DEEP_WATER);
            colorToTerrain.Add(new PixelColor(168, 142, 78).RGB, Terrain.DIRT);
            colorToTerrain.Add(new PixelColor(0, 155, 255).RGB, Terrain.SHALLOW_WATER);
            colorToTerrain.Add(new PixelColor(0, 160, 0).RGB, Terrain.HIGH_GRASS);
            colorToTerrain.Add(new PixelColor(106, 103, 29).RGB, Terrain.ENTANGLING_ROOTS);
            colorToTerrain.Add(new PixelColor(213, 180, 99).RGB, Terrain.SAVANA_DIRT);
            colorToTerrain.Add(new PixelColor(0, 255, 137).RGB, Terrain.SAVANA_GRASS);
        }

        private void InitializeObstacleMaps()
        {
            ObstacleMaps.Add(Movement.LAND, GetObstacleMap(Movement.LAND));
            ObstacleMaps.Add(Movement.WATER, GetObstacleMap(Movement.WATER));
            ObstacleMaps.Add(Movement.LAND_WATER, GetObstacleMap(Movement.LAND_WATER));
        }

        /// <summary>
        /// Updates ObstacleMaps. Should be called only when the map chaneges.
        /// </summary>
        public void UpdateObstacleMaps()
        {
            ObstacleMaps[Movement.LAND] = GetObstacleMap(Movement.LAND);
            ObstacleMaps[Movement.WATER] = GetObstacleMap(Movement.WATER);
            ObstacleMaps[Movement.LAND_WATER] = GetObstacleMap(Movement.LAND_WATER);
            MapWasChanged = false;
        }

        public ObstacleMap GetObstacleMap(Movement movement)
        {
            ObstacleMap om = new ObstacleMap(Width,Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Terrain ter = this[i, j].Terrain;
                    switch (movement)
                    {
                        case Movement.LAND:
                            om[i, j] = ter == Terrain.DEEP_WATER ||
                                        nodes[i,j].Blocked;
                            break;
                        case Movement.WATER:
                            om[i, j] = (ter != Terrain.DEEP_WATER &&
                                        ter != Terrain.SHALLOW_WATER) ||
                                        nodes[i,j].Blocked;
                            break;
                        case Movement.LAND_WATER:
                            om[i, j] = nodes[i,j].Blocked;
                            break;
                    }
                }
            return om;
        }

        public ObstacleMap GetViewMap(Players player)
        {
            ObstacleMap om = new ObstacleMap(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    //player can't see through blocked squares that he doesn't own
                    om[i, j] = nodes[i, j].Blocked && nodes[i, j].Building.Player.PlayerID != player;
            return om;
        }
        /*
        /// <summary>
        /// Returns the distance between the closest points of the circles of the units.
        /// </summary>
        public float Distance(Entity u1, Entity u2)
        {
            float dx = u1.Pos.X - u2.Pos.X;
            float dy = u1.Pos.Y - u2.Pos.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy) - u1.Range - u2.Range;
        }*/
    }
}
