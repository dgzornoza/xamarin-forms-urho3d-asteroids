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
        public float RadialDistance { get; set; } = 0.3f;

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

            // get radial position            
            Vector2 radialPosition = MathHelpers.DegreeToVector2(angle) * 1.0f;

            // set bullet position
            position.X += radialPosition.X * RadialDistance;
            position.Y += radialPosition.Y * RadialDistance;
            bulletBody.Node.SetTransform2D(position, angle);

            // apply force            
            float velocityX = radialPosition.X * UrhoConfig.Data.BULLET_SPEED;
            float velocityY = radialPosition.Y * UrhoConfig.Data.BULLET_SPEED;
            bulletBody.SetLinearVelocity(new Vector2(velocityX, velocityY));
        }

        


        protected override void OnUpdate(float timeStep)
        {
            foreach (var node in this._bullets.Children)
            {
                float lifeTime = float.Parse(node.GetVar(_lifeTimeVarStringHash)) + timeStep;
                node.SetVar(_lifeTimeVarStringHash, lifeTime.ToString());

                node.MirrorIfExitScreen(this.Camera);

                if (lifeTime > UrhoConfig.Data.BULLET_LIFETIME)
                {
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

            // add physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D += _onPhysicsBeginContact;
        }

        protected override void _destroy()
        {
            base._destroy();

            // remove physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D -= _onPhysicsBeginContact;
        }





        private void _onPhysicsBeginContact(PhysicsBeginContact2DEventArgs args)
        {
            // get weapon and other object
            Node weapon = null, otherObject = null;
            foreach (var item in this._bullets.Children)
            {
                otherObject = args.GetOther(item);
                if (null != otherObject) { weapon = item; break; }
            }
            if (null == otherObject) return;

            // Asteroid
            if (UrhoConfig.Names.RUBE_ASTEROIDS_BODY_REGEX.IsMatch(otherObject.Name))
            {
                weapon.Remove();
            }
        }
    }
}
