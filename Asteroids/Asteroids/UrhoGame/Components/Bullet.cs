using System;
using System.Collections.Generic;
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

        private Node _bulletDefinition;
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


        private void _initialize()
        {
            // create from rube json format
            B2dJson b2dJson = LoaderHelpers.LoadRubeJson("Urho2D/RubePhysics/bullet.json", this.Node, false);
            this._bulletDefinition = b2dJson.GetBodyByName(UrhoConfig.RUBE_BULLET_BODY_NAME).Node;
            this._bulletDefinition.Enabled = false;

            // create bullets node
            this._bullets = this.Node.CreateChild("bullets");
        }




        public void Fire(Vector2 position, float angle)
        {
            Node bullet = this._bulletDefinition.Clone();
            RigidBody2D bulletBody = bullet.GetComponent<RigidBody2D>();
            bullet.Enabled = true;
            this._bullets.AddChild(bullet);


            // position.X += (float)Math.Sin(angle);
            // position.Y -= (float)Math.Cos(angle);
            bullet.SetTransform2D(position, angle);

            
            float velocityX = (float)Math.Sin(angle) * BULLET_SPEED * bulletBody.Mass;
            float velocityY = (float)-Math.Cos(angle) * BULLET_SPEED * bulletBody.Mass;
            bulletBody.ApplyForceToCenter(new Vector2(velocityX, velocityY), true);
        }

        protected override void OnUpdate(float timeStep)
        {
            _lifeTime++;
            this._mainCamera = this.Scene.GetChild(UrhoConfig.MAIN_CAMERA_NODE_NAME).GetComponent<Camera>();
            
            foreach (var node in this.Node.Children)
            {
                if (!node.Enabled) continue;

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

        
    }
}
