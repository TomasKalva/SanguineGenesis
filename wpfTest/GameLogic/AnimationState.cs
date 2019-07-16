using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    public class AnimationState
    {
        public Animation Animation { get; }
        private float progress;
        private int image;

        public Rect CurrentImage => Animation.Images[image];

        public AnimationState(Animation anim)
        {
            Animation = anim;
            progress = 0;
        }

        public void Step(float deltaT)
        {
            progress += deltaT;
            if (progress >= Animation.ChangeTimeS)
            {
                progress -= Animation.ChangeTimeS;
                image = (image + 1) % Animation.Length;
            }
        }
    }
}
