using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    class Physics
    {
        private float terrainAcc=0.005f;
        private float unitAcc = 0.0005f;

        public static Physics GetPhysics() => new Physics();

        private Physics() { }

        public void Repulse(Map map, List<Unit> units)
        {
            foreach (Unit u1 in units)
                foreach(Unit u2 in units)
                {
                    if (u1.Equals(u2)) continue;

                    float totalR = u1.Range + u2.Range;
                    double dist = map.Distance(u1, u2);
                    if (dist < totalR && dist!=0)
                    {
                        Vector2 dir12=u1.Pos.UnitDirectionTo(u2.Pos);
                        u1.Accelerate((-unitAcc) * dir12);
                        u2.Accelerate((unitAcc) * dir12);
                        /*float dx = x.X - y.X;
                        float dy = x.Y - y.Y;
                        u1.AccelerateTowards(dx, dy);
                        u2.AccelerateTowards(-dx, -dy);*/
                    }
                }

        }

        public void PushOutsideOfObstacles(Map map, List<Unit> units)
        {
            FlowMap pMap=PushingMapGenerator.GeneratePushingMap(map.GetObstacleMap());
            foreach (Unit u in units)
            {
                u.Accelerate(
                    pMap.GetVelocity(u.Pos.X, u.Pos.Y, terrainAcc)
                    );
            }
        }

        public void Step(Map map, List<Unit> units)
        {
            foreach(Unit u in units)
            {
                u.Move(map);
            }
        }
    }
}
