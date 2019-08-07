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
        private Dictionary<int, Node> colorToNode;
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
                    nodes[i, j] = colorToNode[map[i, j].RGB].Copy(i, j);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
            MovementGenerator mg = MovementGenerator.GetMovementGenerator();
            mg.SetMapChanged(wpfTest.Players.PLAYER0, ObstacleMaps);
            mg.SetMapChanged(wpfTest.Players.PLAYER1, ObstacleMaps);
        }

        /// <summary>
        /// Copy the terrain of already existing map.
        /// </summary>
        public Map(Map map)
        {
            int width = map.Width;
            int height = map.Height;
            nodes = new Node[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Node n = map[i, j];
                    nodes[i, j] = new Node(i, j, n.Nutrients, n.Biome, n.Terrain);
                }
            }
            ObstacleMaps = new Dictionary<Movement, ObstacleMap>();
            InitializeObstacleMaps();
        }

        private void InitializeColorToTerrain()
        {
            colorToNode = new Dictionary<int, Node>();
            colorToNode.Add(new PixelColor(168, 142, 78).RGB, new Node(-1,-1, 0,Biome.DEFAULT,Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 155, 255).RGB, new Node(-1, -1, 0, Biome.DEFAULT, Terrain.SHALLOW_WATER));
            colorToNode.Add(new PixelColor(0, 0, 255).RGB, new Node(-1, -1, 0, Biome.DEFAULT, Terrain.DEEP_WATER));
            colorToNode.Add(new PixelColor(0, 255, 0).RGB, new Node(-1, -1, 
                Biome.RAINFOREST.Nutrients(SoilQuality.LOW), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 160, 0).RGB, new Node(-1, -1,
                Biome.RAINFOREST.Nutrients(SoilQuality.MEDIUM), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(106, 103, 29).RGB, new Node(-1, -1,
                Biome.RAINFOREST.Nutrients(SoilQuality.HIGH), Biome.RAINFOREST, Terrain.LAND));
            colorToNode.Add(new PixelColor(213, 180, 99).RGB, new Node(-1, -1,
                Biome.RAINFOREST.Nutrients(SoilQuality.LOW), Biome.SAVANNA, Terrain.LAND));
            colorToNode.Add(new PixelColor(0, 255, 137).RGB, new Node(-1, -1,
                Biome.RAINFOREST.Nutrients(SoilQuality.MEDIUM), Biome.SAVANNA, Terrain.LAND));
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

        public void AddBuilding(Building building)
        {
            Node[,] nodes = GameQuerying.GetGameQuerying()
                .SelectNodes(this,
                building.NodeLeft,
                building.NodeBottom,
                building.NodeLeft + (building.Size - 1),
                building.NodeBottom + (building.Size - 1));
            foreach (Node n in nodes)
            {
                n.Building = building;
            }
            MapWasChanged = true;
        }

        public void RemoveBuilding(Building building)
        {
            foreach (Node n in building.Nodes)
            {
                this[n.X,n.Y].Building = null;
            }
            MapWasChanged = true;
        }


    }
}
