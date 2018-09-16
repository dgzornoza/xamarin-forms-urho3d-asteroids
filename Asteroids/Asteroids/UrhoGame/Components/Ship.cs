using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Physics;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Helpers;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{

    /// <summary>
    /// Component for game Ship
    /// </summary>
    public class Ship : BaseComponent
    {
        private float _acceleration;
        private float _maxLinearVelocity;
        private float _rotation;
        private float _maxAngularVelocity;

        private float _fireDelay;

        protected Node _shipNode;
        protected RigidBody2D _shipBody;
        protected Thruster _thruster;
        protected Weapon _weapon;


        public Ship()
        {
            _fireDelay = 0;            

            this.ReceiveSceneUpdates = true;
        }

        /// <summary>
        /// Event called on ship destroy
        /// </summary>
        public event EventHandler OnShipDestroy;



        
        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (null == this._shipNode) return;

            this._handleInput();

            this._shipBody.Node.MirrorIfExitScreen(this.Camera);

            // update delays
            if (_fireDelay > 0) _fireDelay -= timeStep;
            // configure thruster
            this._thruster.SetParameters(this._shipBody.LinearVelocity.Length, this._maxLinearVelocity);
        }


        protected override void _initialize()
        {
            base._initialize();

            // create from rube json format
            B2dJson b2dJson = LoaderHelpers.LoadRubeJson(UrhoConfig.Assets.Urho2D.RubePhysics.SHIP, this.Node, false);

            this._shipBody = b2dJson.GetBodyByName(UrhoConfig.Names.RUBE_SHIP_BODY);
            this._acceleration = b2dJson.GetCustomFloat(this._shipBody, nameof(_acceleration));
            this._maxLinearVelocity = b2dJson.GetCustomFloat(this._shipBody, nameof(_maxLinearVelocity));
            this._rotation = b2dJson.GetCustomFloat(this._shipBody, nameof(_rotation));
            this._maxAngularVelocity = b2dJson.GetCustomFloat(this._shipBody, nameof(_maxAngularVelocity));

            this._shipNode = this._shipBody.Node;

            // thruster
            this._thruster = this._shipNode.CreateChild($"{nameof(Thruster)}").CreateComponent<Thruster>();
            this._thruster.Node.SetPosition2D(new Vector2(-0.25f, 0.0f));

            // bullet node
            this._weapon = this.Node.CreateChild($"{nameof(Weapon)}").CreateComponent<Weapon>();

            // add physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D += _onPhysicsBeginContact;
        }

        protected override void _destroy()
        {
            base._destroy();

            // remove physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D -= _onPhysicsBeginContact;
        }




        private void _handleInput()
        {
            if (null == this._shipBody) return;

            Input input = this.Application.Input;

            // forward
            if (input.GetKeyDown(Key.W))
            {

                float velocityY = (this._shipBody.LinearVelocity.X > this._maxLinearVelocity || this._shipBody.LinearVelocity.X < -this._maxLinearVelocity) ?
                    0 : this._shipBody.Mass * this._acceleration * (float)System.Math.Sin(Urho.MathHelper.DegreesToRadians(this._shipBody.Node.Rotation2D));
                float velocityX = (this._shipBody.LinearVelocity.Y > this._maxLinearVelocity || this._shipBody.LinearVelocity.Y < -this._maxLinearVelocity) ?
                    0 : this._shipBody.Mass * this._acceleration * (float)System.Math.Cos(Urho.MathHelper.DegreesToRadians(this._shipBody.Node.Rotation2D));

                if (0f != velocityX || 0f != velocityY) this._shipBody.ApplyForceToCenter(new Vector2(velocityX, velocityY), true);
            }


            // Rotate CCW (left)
            if (input.GetKeyDown(Key.A) && this._shipBody.AngularVelocity < this._maxAngularVelocity)
            {
                this._shipBody.ApplyTorque(this._rotation, true);
            }


            // Rotate CW (right)
            if (input.GetKeyDown(Key.D) && this._shipBody.AngularVelocity > -this._maxAngularVelocity)
            {
                this._shipBody.ApplyTorque(-this._rotation, true);
            }

            // Fire Bullet
            if (input.GetKeyDown(Key.Space))
            {
                if (_fireDelay <= 0)
                {
                    _fireDelay = UrhoConfig.Data.SHIP_FIRE_DELAY;
                    this._weapon.Fire(this._shipNode.Position2D, this._shipNode.WorldRotation2D);
                }
            }
        }


        private void _onPhysicsBeginContact(PhysicsBeginContact2DEventArgs args)
        {
            Node otherObject = args.GetOther(this._shipNode);
            if (null == otherObject) return;

            // Asteroid
            if (UrhoConfig.Names.RUBE_ASTEROIDS_BODY_REGEX.IsMatch(otherObject.Name))
            {
                OnShipDestroy?.Invoke(this, null);
            }
        }

    }
}
