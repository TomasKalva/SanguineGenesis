using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfTest.GUI;

namespace wpfTest.GameLogic
{
    /// <summary>
    /// Animation with current image and timer.
    /// </summary>
    public class AnimationState
    {
        /// <summary>
        /// Animation whose images are used.
        /// </summary>
        public Animation Animation { get; }
        /// <summary>
        /// Time the current image was shown for.
        /// </summary>
        private float progress;
        /// <summary>
        /// Index of the current image.
        /// </summary>
        private int image;

        /// <summary>
        /// Location of the current image in atlas.
        /// </summary>
        public Rect CurrentImage => Animation.Images[image];

        public AnimationState(Animation anim)
        {
            Animation = anim;
            progress = 0;
        }

        /// <summary>
        /// Update the state.
        /// </summary>
        public void Step(float deltaT)
        {
            progress += deltaT;
            if (progress >= Animation.ChangeTimeS)
            {
                //move to the next image
                progress -= Animation.ChangeTimeS;
                image = (image + 1) % Animation.Length;
            }
        }
    }
}
