using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfTest
{
    static class PushingMapGenerator
    {
        private static Dictionary<Pattern3x3, float> angleForPattern;

        static PushingMapGenerator()
        {
            //initialize the pattern dictionary with angles
            angleForPattern = new Dictionary<Pattern3x3, float>();

            Pattern3x3 p0 = new Pattern3x3(
                true, true, true,
                true, true, true,
                false, false, false);
            float angleP0 = (float)(Math.PI) * 3 / 2;

            Pattern3x3 p1 = new Pattern3x3(
                true, true, true,
                false, true, true,
                false, false, true);
            float angleP1 = (float)(Math.PI) * 5 / 4;

            Pattern3x3 p2 = new Pattern3x3(
                false, true, true,
                false, true, true,
                false, false, false);
            float angleP2 = (float)(Math.PI) * 5 / 4;

            Pattern3x3 p3 = new Pattern3x3(
                true, true, true,
                true, true, true,
                false, false, true);
            float angleP3 = (float)(Math.PI) * 5 / 4;

            Pattern3x3 p3Fli = p3.FlipHoriz();
            float angleP3Fli = (float)(Math.PI) * 7 / 4;

            Pattern3x3 p4 = new Pattern3x3(
                true, true, true,
                false, true, true,
                false, false, false);
            float angleP4 = (float)(Math.PI) * 5 / 4;

            Pattern3x3 p4Fli = p4.FlipHoriz();
            float angleP4Fli = (float)(Math.PI) * 7 / 4;

            Pattern3x3 p5 = new Pattern3x3(
                true, true, true,
                true, true, true,
                false, true, true);
            float angleP5 = (float)(Math.PI) * 5 / 4;

            Func<float, float> RotateRight = (angle)=>angle + (float)Math.PI * 3 / 2;
            Action<Pattern3x3, float> AddToDic = (pat, angle) =>
              {
                  for (int i = 0; i < 4; i++)
                  {
                      pat = pat.RotateRight();
                      angle = RotateRight(angle);
                      angleForPattern.Add(pat, angle);
                  }
              };

            AddToDic(p0, angleP0);
            AddToDic(p1, angleP1);
            AddToDic(p2, angleP2);
            AddToDic(p3, angleP3);
            AddToDic(p4, angleP4);
            AddToDic(p5, angleP5);
            AddToDic(p3Fli, angleP3Fli);
            AddToDic(p4Fli, angleP4Fli);
        }

        public static FlowMap GeneratePushingMap(ObstacleMap obstMap)
        {
            FlowMap flowMap = new FlowMap(obstMap.Width, obstMap.Height);
            
            Pattern3x3 curr;
            for(int i = 0; i < flowMap.Width; i++)
            {
                for(int j = 0; j < flowMap.Height; j++)
                {
                    curr = new Pattern3x3(
                        obstMap[i - 1, j + 1], obstMap[i, j + 1], obstMap[i + 1, j + 1],
                        obstMap[i - 1, j    ], obstMap[i, j    ], obstMap[i + 1, j    ],
                        obstMap[i - 1, j - 1], obstMap[i, j - 1], obstMap[i + 1, j - 1]);
                    if (angleForPattern.TryGetValue(curr, out float angle))
                        flowMap[i, j] = angle;
                }
            }
            return flowMap;
        }
    }

    struct Pattern3x3
    {
        bool _11; bool _12; bool _13;
        bool _21; bool _22; bool _23;
        bool _31; bool _32; bool _33;

        public Pattern3x3(
            bool _11, bool _12, bool _13,
        bool _21, bool _22, bool _23,
        bool _31, bool _32, bool _33)
        {
            this._11 = _11;
            this._12 = _12;
            this._13 = _13;

            this._21 = _21;
            this._22 = _22;
            this._23 = _23;

            this._31 = _31;
            this._32 = _32;
            this._33 = _33;
        }

        public Pattern3x3 FlipHoriz()
        {
            Pattern3x3 newP = new Pattern3x3();
            newP._11 = _13;
            newP._12 = _12;
            newP._13 = _11;

            newP._21 = _23;
            newP._22 = _22;
            newP._23 = _21;

            newP._31 = _33;
            newP._32 = _32;
            newP._33 = _31;

            return newP;
        }

        public Pattern3x3 RotateRight()
        {
            Pattern3x3 newP = new Pattern3x3();
            newP._11 = _31;
            newP._12 = _21;
            newP._13 = _11;

            newP._21 = _32;
            newP._22 = _22;
            newP._23 = _12;

            newP._31 = _33;
            newP._32 = _23;
            newP._33 = _13;

            return newP;
        }
    }
}
