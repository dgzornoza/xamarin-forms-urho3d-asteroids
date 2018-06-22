using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Physics;
using Urho.Urho2D;

namespace Asteroids.Game.Components
{
    public class Ship : Component
    {        
        private const float ACCELERATION = 15.0f;
        private const float LINEAR_DAMPING = 0.5f;
        private const float MAX_LINEAR_VELOCITY = 15.0f;

        private const float ROTATION = 0.3f;
        private const float MAX_ANGULAR_VELOCITY = 5.0f;
        private const float ANGULAR_DAMPING = 3.0f;


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


        public Ship()
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
            

            //if (!_isBlinking && isColliding())
            //{
            //    _reset();
            //}

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
            RigidBody2D body = this.Node.GetChild(0).GetComponent<RigidBody2D>();
            if (null == body) return;

            Input input = this.Application.Input;

            // forward
            if (input.GetKeyDown(Key.W))
            {
                _isThrusting = true;
                
                float velocityX = (body.LinearVelocity.X > MAX_LINEAR_VELOCITY || body.LinearVelocity.X < -MAX_LINEAR_VELOCITY) ?
                    0 : body.Mass * ACCELERATION * -(float)Math.Sin(MathHelper.DegreesToRadians(body.Node.Rotation2D));
                float velocityY = (body.LinearVelocity.Y > MAX_LINEAR_VELOCITY || body.LinearVelocity.Y < -MAX_LINEAR_VELOCITY ) ?
                    0 : body.Mass * ACCELERATION * (float)Math.Cos(MathHelper.DegreesToRadians(body.Node.Rotation2D));

                if (0f != velocityX || 0f != velocityY) body.ApplyForceToCenter(new Vector2(velocityX, velocityY), true);
            }
            else
            {
                _isThrusting = false;
            }

            // Rotate CCW (left)
            if (input.GetKeyDown(Key.A) && body.AngularVelocity < MAX_ANGULAR_VELOCITY)
            {
                body.ApplyTorque(ROTATION, true);
            }


            // Rotate CW (right)
            if (input.GetKeyDown(Key.D) && body.AngularVelocity > -MAX_ANGULAR_VELOCITY)
            {
                body.ApplyTorque(-ROTATION, true);                
            }

            if (input.GetKeyDown(Key.N1))
            {
                body.AngularDamping += 0.01f;
            }
            if (input.GetKeyDown(Key.N2))
            {
                body.AngularDamping -= 0.01f;
            }
            if (input.GetKeyDown(Key.N3))
            {
                body.LinearDamping += 0.01f;
            }
            if (input.GetKeyDown(Key.N4))
            {
                body.LinearDamping -= 0.01f;
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
            Node node = this.Node.GetChild(0);
            if (null == node) return;

            Graphics graphics = this.Application.Graphics;
            Camera camera = this.Scene.GetChild(UrhoConfig.mainCameraNodeName).GetComponent<Camera>();
            Vector2 position = camera.WorldToScreenPoint(node.Position);
            Vector3 screenMin = camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
            Vector3 screenMax = camera.ScreenToWorldPoint(new Vector3(1.0f, 1.0f, 0));

            if (position.X > 1.0f)
                node.SetTransform2D(new Vector2(screenMin.X, node.Position.Y), node.Rotation2D);

            if (position.X < 0)
                node.SetTransform2D(new Vector2(screenMax.X, node.Position.Y), node.Rotation2D);

            if (position.Y > 1.0f)
                node.SetTransform2D(new Vector2(node.Position.X, screenMin.Y), node.Rotation2D);

            if (position.Y < 0)
                node.SetTransform2D(new Vector2(node.Position.X, screenMax.Y), node.Rotation2D);
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
            string filePath = this.Application.ResourceCache.GetResourceFileName("Urho2D/RubePhysics/ship.json");
            Toolkit.Urho.Rube.B2dJson b2dJson = new Toolkit.Urho.Rube.B2dJson();
            b2dJson.ReadIntoNodeFromFile(filePath, this.Node, false, out string errorMsg);

            temp(b2dJson);

            RigidBody2D body = this.Node.GetChild(0).GetComponent<RigidBody2D>();
            if (null == body) return;
            body.LinearDamping = LINEAR_DAMPING;
            body.AngularDamping = ANGULAR_DAMPING;


            //var cache = Application.ResourceCache;
            //Sprite2D sprite = cache.GetSprite2D("Urho2D/Sprites/Ship.png");
            //if (sprite == null) return;
            //Node spriteNode = Scene.CreateChild("StaticSprite2D");
            //StaticSprite2D staticSprite = spriteNode.CreateComponent<StaticSprite2D>();
            //staticSprite.Color = Color.Green;
            //staticSprite.BlendMode = BlendMode.Alpha;
            //staticSprite.Sprite = sprite;

            //spriteNode.RunActionsAsync(new Urho.Actions.TintTo(2, Color.Blue));
        }

        private void temp(Toolkit.Urho.Rube.B2dJson json)
        {
            // crear un vector con todas las imagenes de la escena del editor RUBE
            IEnumerable<Toolkit.Urho.Rube.B2dJsonImage> b2dImages = json.GetAllImages();
            var cache = Application.ResourceCache;

            // recorrer el vector, crear los sprites para cada imagen y almacenarla en el array con imagenes asociadas a cuerpos fisicos
            foreach (var img in b2dImages)
            {
                // si la imagen no tiene un nodo asociado y el flag indica que no se cargue, se continua con la siguiente
                if (null == img.Body) continue;

                // probar a cargar la imagen del sprite, ignorar si falla
                string fullPath = Path.GetFullPath(img.Path ?? "Urho2D/RubePhysics/" + img.File);
                Sprite2D sprite = cache.GetSprite2D(fullPath.Substring(fullPath.IndexOf("Urho2D")));
                if (sprite == null) continue;

                // añadir el sprite al nodo de fisicas y establecer el orden de renderizado
                StaticSprite2D staticSprite = img.Body.Node.CreateComponent<StaticSprite2D>();
                staticSprite.Sprite = sprite;
                staticSprite.OrderInLayer = (int)img.RenderOrder;

                // calcular tamaño de la imagen
                //img.Heig
                // establecer propiedades del sprite
                staticSprite.FlipX = img.Flip;
                staticSprite.Color = Color.FromByteFormat((byte)img.ColorTint[0], (byte)img.ColorTint[1], (byte)img.ColorTint[2], (byte)img.ColorTint[3]);
                staticSprite.Alpha = 0.2f; // 0.2f; // img.Opacity;
                staticSprite.BlendMode = BlendMode.Alpha;
            }
        }
    }
}
