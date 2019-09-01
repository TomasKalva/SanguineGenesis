﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest.GameLogic.Maps
{
    interface IPathfinding
    {
        FlowMap GenerateFlowMap(ObstacleMap obst, Vector2 targetLocation);
    }
}