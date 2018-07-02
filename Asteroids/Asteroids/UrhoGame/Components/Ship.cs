using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Physics;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{
    public class Ship : Component
    {
        private const string RUBE_BODY_NAME = "ship-body";

        Camera _mainCamera;
        Thruster _thruster;
        Node _shipNode;
        RigidBody2D _shipBody;

        private float _acceleration;
        private float _maxLinearVelocity;
        private float _rotation;
        private float _maxAngularVelocity;


        private const int FIRE_DELAY = 30;
        private const int START_DELAY = 400;
        private const int BLINK_DELAY = 25;
        private const int THUMP_DELAY = 65;
        private const int THRUST_DELAY = 5;

        private int _fireDelay;
        private int _blinkDelay;
        private int _blinkTime;
        private bool _isBlinking;
        private int _lives;
        private int _thumpTime;
        private bool _thumpSwitch;

        private bool _thrustSwitch;


        public Ship()
        {
            _fireDelay = 0;
            _blinkDelay = 0;
            _blinkTime = 0;            
            _isBlinking = true;
            _lives = 3;
            _thumpTime = 0;
            _thumpSwitch = true;
            
            _thrustSwitch = false;

            this.ReceiveSceneUpdates = true;
        }

        public override void OnSceneSet(Scene scene)
        {
            base.OnSceneSet(scene);

            Input input = this.Application.Input;

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




        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            if (null == this._shipNode) return;

            if (_thumpTime >= (THUMP_DELAY + (_lives * 15)))
            {
                _thumpSwitch = !_thumpSwitch;
                _thumpTime = 0;
            }

            _thumpTime++;

            this._handleInput();

            this._mainCamera = this.Scene.GetChild(UrhoConfig.mainCameraNodeName).GetComponent<Camera>();
            this._shipBody.Node.MirrorIfExitScreen(this._mainCamera);

            if (_fireDelay > 0) _fireDelay--;

            if (_blinkDelay > 0) _blinkDelay--;

            if (_blinkDelay == 0 && _blinkTime < START_DELAY)
            {
                _blinkTime += BLINK_DELAY;
                _blinkDelay = BLINK_DELAY;
                Enabled = !Enabled;

                if (_blinkTime >= START_DELAY)
                {
                    Enabled = true;
                    _isBlinking = false;
                }
            }


            //if (!_isBlinking && isColliding())
            //{
            //    _reset();
            //}

            this._thruster.SetParametersFromBody(this._shipBody);
        }


        void render()
        {
            //if (m_isThrusting)
            //{
            //    m_thrustSwitch = !m_thrustSwitch;
            //    if (m_thrustSwitch)
            //    {
            //        scene()->draw(m_Thruster);
            //    }
            //}

            //float currentX = getPosition().x;
            //float currentY = getPosition().y;
            //float mirrorX = (currentX < scene()->window()->getCenter().x) ? currentX + scene()->window()->getWidth() : currentX - scene()->window()->getWidth();
            //float mirrorY = (currentY < scene()->window()->getCenter().y) ? currentY + scene()->window()->getHeight() : currentY - scene()->window()->getHeight();

            //RenderableObject::setPosition(mirrorX, currentY);
            //renderObject();
            //RenderableObject::setPosition(currentX, mirrorY);
            //renderObject();

            //RenderableObject::setRotation(0);
            //RenderableObject::setScale(0.75, 0.75);
            //for (int i = 0; i < m_lives; i++)
            //{
            //    RenderableObject::setPosition((float)130 + (i * 22), (float)30);
            //    renderObject();
            //}
            //RenderableObject::setScale(1, 1);
        }



        private void _handleInput()
        {
            if (null == this._shipBody) return;

            Input input = this.Application.Input;

            // forward
            if (input.GetKeyDown(Key.W))
            {
                float velocityX = (this._shipBody.LinearVelocity.X > this._maxLinearVelocity || this._shipBody.LinearVelocity.X < -this._maxLinearVelocity) ?
                    0 : this._shipBody.Mass * this._acceleration * -(float)System.Math.Sin(Urho.MathHelper.DegreesToRadians(this._shipBody.Node.Rotation2D));
                float velocityY = (this._shipBody.LinearVelocity.Y > this._maxLinearVelocity || this._shipBody.LinearVelocity.Y < -this._maxLinearVelocity) ?
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
            //if (input.GetKeyDown(Key.Space))
            //{
            //    if (_fireDelay == 0)
            //    {
            //        _fireDelay = FIRE_DELAY;                    

            //        scene()->addObject<Bullet>()->get()->set(body()->GetPosition(), body()->GetAngle());
            //    }
            //}
        }


        private void _reset()
        {
            if (_lives <= 1)
            {
                this.Remove();
            }
            else
            {
                _lives--;
                _blinkDelay = 0;
                _blinkTime = 0;
                this.Enabled = false;
                // setVisible(false);
                _isBlinking = true;

                //body()->SetTransform(b2Vec2(((scene()->window()->getCenter().x - 10) / Physics::Scale), ((scene()->window()->getCenter().y - 10) / Physics::Scale)), 0);
                //body()->SetLinearVelocity(b2Vec2_zero);
                //body()->SetAngularVelocity(0);
            }
        }

        private void _initialize()
        {
            // create from rube json format
            B2dJson b2dJson = LoaderHelpers.LoadRubeJson("Urho2D/RubePhysics/ship.json", this.Node, false);
            
            this._shipBody = b2dJson.GetBodyByName(RUBE_BODY_NAME);
            this._acceleration = b2dJson.GetCustomFloat(this._shipBody, nameof(_acceleration));
            this._maxLinearVelocity = b2dJson.GetCustomFloat(this._shipBody, nameof(_maxLinearVelocity));
            this._rotation = b2dJson.GetCustomFloat(this._shipBody, nameof(_rotation));
            this._maxAngularVelocity = b2dJson.GetCustomFloat(this._shipBody, nameof(_maxAngularVelocity));

            this._shipNode = this._shipBody.Node;

            // thruster
            this._thruster = this._shipNode.CreateChild(nameof(Thruster)).CreateComponent<Thruster>();
            this._thruster.Node.SetPosition2D(new Vector2(0.0f, -0.55f));

            // node text info
            this._shipNode.CreateComponent<NodeTextInfo>();
        }

    }
}
