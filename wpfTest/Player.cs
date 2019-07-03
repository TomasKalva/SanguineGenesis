﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class Player
    {
        public List<Unit> Units { get; private set; }

        public Player()
        {
            InitUnits();
        }

        public void InitUnits()
        {
            Units = new List<Unit>();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Units.Add(new Unit(20 + i*.25f,10+ j*.25f));
                }
            }
            Units.Add(new Unit(5f, 6f));
            Units.Add(new Unit(7f, 6f));
            Units.Add(new Unit(6.5f, 6f));
            Units.Add(new Unit(4f, 9f));
        }
    }
}
