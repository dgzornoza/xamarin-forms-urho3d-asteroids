﻿using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Asteroids.UrhoGame.Stages
{
    public class StageManager : Component
    {
        public StageManager() { }

        public override void OnSceneSet(Scene scene)
        {            
            base.OnSceneSet(scene);

            // attach to scene
            if (null != scene)
            {

            }
            // dettach from scene
            else
            {

            }
        }


        private void _initialize()
        {
        }
    }
}