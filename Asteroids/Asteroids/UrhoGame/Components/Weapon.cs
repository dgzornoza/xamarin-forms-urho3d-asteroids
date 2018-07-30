using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Helpers;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{


    /// <summary>
    /// Component for game Weapon
    /// </summary>
    public class Weapon : BaseComponent
    {
        private const int BULLET_SPEED = 20;
        private const int BULLET_LIFETIME = 400;

        private static JObject _bulletDefinition;
        private static StringHash _lifeTimeVarStringHash = new StringHash("life-time");

        private Node _bullets;
        

        public Weapon()
        {            
            this.ReceiveSceneUpdates = true;
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
            B2dJson b2dJson = LoaderHelpers.ReadIntoNodeFromValue(_bulletDefinition, this._bullets, false, UrhoConfig.Assets.Urho2D.RubePhysics.PATH);
            RigidBody2D bulletBody = b2dJson.GetBodyByName(UrhoConfig.Names.RUBE_BULLET_BODY);
            bulletBody.Node.SetVar(_lifeTimeVarStringHash, "0");
            bulletBody.Node.NodeCollisionStart += _onNodeCollisionStart;

            // get radial position
            Vector2 radialPosition = MathHelpers.DegreeToVector2(angle) * RadialDistance;

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
            foreach (var node in this._bullets.Children)
            {
                int lifeTime = Convert.ToInt32(node.GetVar(_lifeTimeVarStringHash));
                node.SetVar(_lifeTimeVarStringHash, (++lifeTime).ToString());

                node.MirrorIfExitScreen(this.Camera);

                if (lifeTime > BULLET_LIFETIME)
                {
                    node.NodeCollisionStart -= _onNodeCollisionStart;
                    node.Remove();
                }
            }
        }



        protected override void _initialize()
        {
            base._initialize();

            // store JObject from rube file for create bullets
            if (null == _bulletDefinition) _bulletDefinition = LoaderHelpers.GetJObjectFromJsonFile(UrhoConfig.Assets.Urho2D.RubePhysics.BULLET);

            // create bullets node
            this._bullets = this.Node.CreateChild("bullets");
        }

        private void _onNodeCollisionStart(NodeCollisionStartEventArgs obj)
        {
            int a = 5;
            int b = a / 5;
        }

    }
}
