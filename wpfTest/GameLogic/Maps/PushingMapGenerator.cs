using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GameLogic.Maps;

namespace wpfTest
{
    /// <summary>
    /// Generates PushingMap for Map.
    /// </summary>
    static class PushingMapGenerator
    {
        /// <summary>
        /// For each possible 2x2 square arrangement of nodes where the bottom right is blocked contains
        /// angle where the animal should be pushed if its on the bottom right node.
        /// </summary>
        private static Dictionary<Pattern2x2, float> angleForPattern;

        static PushingMapGenerator()
        {
            //initialize the pattern dictionary with angles
            angleForPattern = new Dictionary<Pattern2x2, float>();
            float left= (float)(Math.PI);
            float leftUp = (float)(Math.PI) * 3 / 4f;
            float up = (float)(Math.PI) / 2f;
            float rightDown = (float)(Math.PI) * 7 / 4f;
            
            Pattern2x2[] patterns = new Pattern2x2[8];
            float[] angles = new float[8];

            patterns[0] = new Pattern2x2(
                true, true, 
                true, true);
            angles[0] = rightDown;

            patterns[1] = new Pattern2x2(
                true, true,
                false, true);
            angles[1] = left;

            patterns[2] = new Pattern2x2(
                true, false,
                true, true);
            angles[2] = up;

            patterns[3] = new Pattern2x2(
                false, true,
                true, true);
            angles[3] = leftUp;

            patterns[4] = new Pattern2x2(
                false, false,
                true, true);
            angles[4] = up;

            patterns[5] = new Pattern2x2(
                false, true,
                false, true);
            angles[5] = left;

            patterns[6] = new Pattern2x2(
                false, false,
                false, true);
            angles[6] = leftUp;

            patterns[7] = new Pattern2x2(
                true, false,
                false, true);
            angles[7] = leftUp;

            for(int i=0;i<8;i++)
                angleForPattern.Add(patterns[i], angles[i]);
        }

        /// <summary>
        /// Generates pushing map for the given obstacle map. Only blocked squares that are adjacent (by 
        /// edge or vertex) with a not blocked square contain a pushing square. The other squares are null.
        /// </summary>
        public static PushingMap GeneratePushingMap(ObstacleMap obstMap)
        {
            PushingMap pushingMap = new PushingMap(obstMap.Width, obstMap.Height);

            Pattern3x3 pat;
            for(int i = 0; i < pushingMap.Width; i++)
            {
                for(int j = 0; j < pushingMap.Height; j++)
                {
                    if (obstMap[i, j])
                    {
                        //select square around [i,j]
                        pat = new Pattern3x3(
                            obstMap[i - 1, j + 1], obstMap[i, j + 1], obstMap[i + 1, j + 1],
                            obstMap[i - 1, j], obstMap[i, j], obstMap[i + 1, j],
                            obstMap[i - 1, j - 1], obstMap[i, j - 1], obstMap[i + 1, j - 1]);

                        if (pat.AllBlocked())
                        {
                            pushingMap[i, j] = null;
                        }
                        else
                        {
                            //rotate the square right, find angle for the top left subpattern, rotate
                            //the angle back left
                            //repeat for all 4 directions
                            float rotation = 0f;
                            float[] directions = new float[4];
                            for (int k = 0; k < 4; k++)
                            {
                                Pattern2x2 sub = pat.LeftUpSubpattern();
                                directions[k] = angleForPattern[sub] - rotation;
                                pat = pat.RotateRight();
                                rotation += (float)((Math.PI) * 3 / 2f);
                            }
                            pushingMap[i, j] = new PushingSquare(directions[0], directions[3],
                                                                directions[1], directions[2]);
                        }
                    }
                    else
                        pushingMap[i, j] = null;
                }
            }
            return pushingMap;
        }
    }

    /// <summary>
    /// Represents 3x3 pattern of obstacles.
    /// </summary>
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

        /// <summary>
        /// Returns a pattern that is flipped horizontaly.
        /// </summary>
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

        /// <summary>
        /// Returns a pattern that is rotated by 90 degrees right.
        /// </summary>
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

        /// <summary>
        /// Returns left top subsquare of size 2x2.
        /// </summary>
        public Pattern2x2 LeftUpSubpattern()
            => new Pattern2x2( _11, _12, _21, _22);

        /// <summary>
        /// Returns true if all squares are blocked.
        /// </summary>
        public bool AllBlocked()
        {
            return _11 && _12 && _13 &&
                _21 && _22 && _23 &&
                _31 && _32 && _33;
        }
    }


    /// <summary>
    /// Represents 2x2 pattern of obstacles.
    /// </summary>
    struct Pattern2x2
    {
        bool _11; bool _12;
        bool _21; bool _22;

        public Pattern2x2(
            bool _11, bool _12,
            bool _21, bool _22)
        {
            this._11 = _11;
            this._12 = _12;
            this._21 = _21;
            this._22 = _22;
        }

        /// <summary>
        /// Returns a pattern that is flipped horizontaly.
        /// </summary>
        public Pattern2x2 FlipHoriz()
        {
            Pattern2x2 newP = new Pattern2x2();
            newP._11 = _21;
            newP._12 = _22;
            newP._21 = _11;
            newP._22 = _12;

            return newP;
        }

        /// <summary>
        /// Returns a pattern that is rotated by 90 degrees right.
        /// </summary>
        public Pattern2x2 RotateRight()
        {
            Pattern2x2 newP = new Pattern2x2();
            newP._11 = _21;
            newP._12 = _11;
            newP._22 = _12;
            newP._21 = _22;

            return newP;
        }
    }
}
