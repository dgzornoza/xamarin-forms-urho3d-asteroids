﻿using System;
using System.Collections.Generic;
using System.Text;
using Urho;

namespace Asteroids.UrhoGame.Components
{
    /// <summary>
    /// Base component with common functions
    /// </summary>
    public abstract class BaseComponent : Component
    {
        private Camera _mainCamera;

        /// <summary>
        /// Property for get camera
        /// </summary>
        protected Camera Camera => this._mainCamera ?? (this._mainCamera = this.Scene.GetChild(UrhoConfig.Names.MAIN_CAMERA_NODE).GetComponent<Camera>());

        public override void OnSceneSet(Scene scene)
        {
            base.OnSceneSet(scene);

            // attach to scene
            if (null != scene)
            {
                this._initialize();
            }
            // dettach from scene
            else
            {
                _destroy();
            }
        }

        protected virtual void _initialize() { }

        protected virtual void _destroy() { }
    }
}