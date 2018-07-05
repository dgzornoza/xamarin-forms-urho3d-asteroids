using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{

    public class Bullet : Component
    {
        private const int BULLET_SPEED = 10;
        private const int BULLET_LIFETIME = 400;

        Camera _mainCamera;

        private JObject _bulletDefinition;
        private Node _bullets;
        private int _lifeTime;
        

        public Bullet()
        {
            this._lifeTime = 0;

            this.ReceiveSceneUpdates = true;
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

        /// <summary>
        /// Radial distance multiplier for adjust distance from fire position center
        /// </summary>
        public float RadialDistance { get; set; } = 1.0f;

        /// <summary>
        /// Function for fire bullet
        /// </summary>
        /// <param name="position">world position for start bullet</param>
        /// <param name="angle">world angle for start bullet</param>
        public void Fire(Vector2 position, float angle)
        {
            // Create bullet from rube format
            B2dJson b2dJson = LoaderHelpers.ReadIntoNodeFromValue(this._bulletDefinition, this._bullets, false, "Urho2D/RubePhysics/");
            RigidBody2D bulletBody = b2dJson.GetBodyByName(UrhoConfig.RUBE_BULLET_BODY_NAME);

            // get radial position
            Vector2 radialPosition = MathExtensions.DegreeToVector2(angle) * RadialDistance;

            // set bullet position
            position.X += radialPosition.X;
            position.Y += radialPosition.Y;
            bulletBody.Node.SetTransform2D(position, angle);

            // apply force            
            float velocityX = radialPosition.X * BULLET_SPEED;
            float velocityY = radialPosition.Y * BULLET_SPEED;
            bulletBody.SetLinearVelocity(new Vector2(velocityX, velocityY));
        }


        protected override void OnUpdate(float timeStep)
        {
            _lifeTime++;
            this._mainCamera = this.Scene.GetChild(UrhoConfig.MAIN_CAMERA_NODE_NAME).GetComponent<Camera>();

            foreach (var node in this._bullets.Children)
            {
                node.MirrorIfExitScreen(this._mainCamera);

                // if (_lifeTime > BULLET_LIFETIME) node.Remove();

                //if (_lifeTime > BULLET_LIFETIME || isColliding())
                //{
                //    if (isColliding<Rock>())
                //    {
                //        float scale = getCollider<Rock>()->getScale();
                //        if (scale < 3)
                //        {
                //            scene()->addObject<Rock>()->get()->set(scale + 1, getCollider<Rock>()->getPosition(), body()->GetAngle());
                //            scene()->addObject<Rock>()->get()->set(scale + 1, getCollider<Rock>()->getPosition(), -body()->GetAngle());
                //        }
                //    }

                //    destroy();
                //}
            }

        }


        private void _initialize()
        {
            // store JObject from rube file for create bullets
            this._bulletDefinition = LoaderHelpers.GetJObjectFromJsonFile("Urho2D/RubePhysics/bullet.json");

            // create bullets node
            this._bullets = this.Node.CreateChild("bullets");
        }







        
    }
}
