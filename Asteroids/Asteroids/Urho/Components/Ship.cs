using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Physics;
using Urho.Urho2D;

namespace Asteroids.Game.Components
{
    public class Ship : LogicComponent
    {
        private const float ACCELERATION = 20.0f;
        private const float ROTATION = 5.0f;
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
        private bool _isThrusting;


        public Ship() { }

        public override void OnSceneSet(Scene scene)
        {
            base.OnSceneSet(scene);

            Input input = this.Application.Input;

            // attach to scene
            if (null != scene)
            {
                this._create();
            }
            // dettach from scene
            else
            {
            }
        }


        private void _create()
        {
            _fireDelay = 0;
            _blinkDelay = 0;
            _blinkTime = 0;
            // setVisible(false);
            _isBlinking = true;
            _lives = 3;
            _thumpTime = 0;
            _thumpSwitch = true;

            // _Thruster.init();
            _isThrusting = false;
            _thrustSwitch = false;
        }



        protected override void OnFixedUpdate(PhysicsPreStepEventArgs e)
        {

            if (_thumpTime >= (THUMP_DELAY + (_lives * 15)))
            {
                _thumpSwitch = !_thumpSwitch;
                _thumpTime = 0;
            }

            _thumpTime++;

            this._handleInput();
            checkOffscreen();

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

            if (!_isBlinking && isColliding())
            {
                _reset();
            }

            // m_Thruster.set(getPosition(), getRotation());
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
            Input input = this.Application.Input;

            // forward
            if (input.GetKeyDown(Key.W))
            {
                _isThrusting = true;

                float velocityX = body()->GetMass() * ACCELERATION * glm::sin(body()->GetAngle());
                float velocityY = body()->GetMass() * ACCELERATION * -glm::cos(body()->GetAngle());
                body()->ApplyForceToCenter(b2Vec2(velocityX, velocityY), true);
            }
            else
            {
                _isThrusting = false;
            }

            // Rotate CCW (left)
            if (input.GetKeyDown(Key.A))
            {
                applyTorque(-ROTATION);
            }


            // Rotate CW (right)
            if (input.GetKeyDown(Key.D))
            {
                applyTorque(ROTATION);
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


        void checkOffscreen()
        {
            //b2Vec2 position(body()->GetPosition().x* Physics::Scale, body()->GetPosition().y * Physics::Scale);

            //if (position.x > scene()->window()->getWidth())
            //    body()->SetTransform(b2Vec2(0, position.y / Physics::Scale), body()->GetAngle());

            //if (position.x < 0)
            //    body()->SetTransform(b2Vec2(scene()->window()->getWidth() / Physics::Scale, position.y / Physics::Scale), body()->GetAngle());

            //if (position.y > scene()->window()->getHeight())
            //    body()->SetTransform(b2Vec2(position.x / Physics::Scale, 0), body()->GetAngle());

            //if (position.y < 0)
            //    body()->SetTransform(b2Vec2(position.x / Physics::Scale, scene()->window()->getHeight() / Physics::Scale), body()->GetAngle());
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

        private void _createShip()
        {

            var cache = Application.ResourceCache;
            Sprite2D sprite = cache.GetSprite2D("Textures/Ship.dds");
            if (sprite == null) return;
            Node spriteNode = Scene.CreateChild("StaticSprite2D");
            StaticSprite2D staticSprite = spriteNode.CreateComponent<StaticSprite2D>();
            staticSprite.Color = Color.Green;
            staticSprite.BlendMode = BlendMode.Alpha;
            staticSprite.Sprite = sprite;

            //spriteNode.RunActionsAsync(new Urho.Actions.TintTo(2, Color.Blue));

            // float points[4][2] = { { -10, 10 }, { 0, -20 }, { 10, 10 }, {0, 0} };
            //setPoints(4, points);
            //setColors();
            //setPosition(((float) scene()->window()->getCenter().x) - 10, ((float) scene()->window()->getCenter().y) - 10);
            //createBody(scene()->physics()->world(), b2_dynamicBody, 0.5f, 10.0f, 5.0f);
        }

    }
}
