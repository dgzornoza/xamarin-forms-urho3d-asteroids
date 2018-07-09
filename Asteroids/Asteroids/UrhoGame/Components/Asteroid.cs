using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using XamarinForms.Toolkit.Helpers;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{
    public class Asteroid : Component
    {
        private static JObject _asteroidsDefinitions;
        // private static StringHash _lifeTimeVarStringHash = new StringHash("life-time");

        

        private Camera _mainCamera;
        private float _scale = 1.0f;
        private Node _asteroids;

        public Asteroid()
        {
            this.ReceiveSceneUpdates = true;
        }


        public float Scale
        {
            get => _scale;
            set
            {
                this._scale = value;
                // setAsBox(60.0f / _scale, 60.0f / _scale);
            }
        }

        /// <summary>
        /// Property for get camera
        /// </summary>
        private Camera Camera => this._mainCamera ?? (this._mainCamera = this.Scene.GetChild(UrhoConfig.MAIN_CAMERA_NODE_NAME).GetComponent<Camera>());



        public void set(float scale, Vector2 position, float angle)
        {
            //setScale(scale);

            //setAsBox(60 / m_Scale, 60 / m_Scale);
            //setColors();

            //setPosition(pos.x, pos.y);

            //createBody(scene()->physics()->world(), b2_dynamicBody, 0, 0, 5.0f);

            //applyTorque((float)glm::linearRand(-2000, 2000));

            ////float velocityX = (float)glm::linearRand(-5000, 5000);
            ////float velocityY = (float)glm::linearRand(-5000, 5000);

            //float velocityX = sin(glm::degrees(angle)) * m_Scale;
            //float velocityY = cos(glm::degrees(angle)) * m_Scale;

            //body()->ApplyForceToCenter(b2Vec2(velocityX, velocityY), true);
        }

        public void set(float scale)
        {
            //set(scale, getRandomPosition((int)scene()->window()->getWidth(), (int)scene()->window()->getHeight()), (float)glm::linearRand(0, 360));
        }

        protected override void OnUpdate(float timeStep)
        {
            this.Node.MirrorIfExitScreen(this.Camera);

            //if (isColliding<Bullet>())
            //{
            //    destroy();
            //}
        }




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

            }
        }


        private void _initialize()
        {
            // store JObject from rube file for create bullets
            if (null == _asteroidsDefinitions) _asteroidsDefinitions = LoaderHelpers.GetJObjectFromJsonFile("Urho2D/RubePhysics/asteroids.json");
        }


        private Vector2 _getRandomPosition(int with, int height)
        {
            Graphics graphics = this.Application.Graphics;
            return new Vector2(RandomHelpers.NextRandom(0, graphics.Width), RandomHelpers.NextRandom(0, graphics.Height));
        }




    }
}
