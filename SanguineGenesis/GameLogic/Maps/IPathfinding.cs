using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanguineGenesis.GameLogic.Maps
{
    interface IPathfinding
    {
        FlowField GenerateFlowField();
    }
}
