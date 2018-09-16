using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Urho;
using System.Linq;
using Urho.Urho2D;
using XamarinForms.Toolkit.Helpers;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Rube;
using XamarinForms.Toolkit.Urho3D.Helpers;

namespace Asteroids.UrhoGame.Components
{


    /// <summary>
    /// Component for game Asteroids
    /// </summary>
    public class Asteroid : BaseComponent
    {
        private static JObject _asteroidsDefinitions;
        private static StringHash _asteroidSizeVarStringHash = new StringHash("asteroid-size");

        

            
        private Node _asteroids;
        

        public Asteroid()
        {
            this.ReceiveSceneUpdates = true;            
        }
        




        protected override void OnUpdate(float timeStep)
        {
            foreach (var node in this._asteroids.Children)
            {
                node.MirrorIfExitScreen(this.Camera);
            }
        }


        protected override void _initialize()
        {
            base._initialize();

            // store JObject from rube file for create bullets
            if (null == _asteroidsDefinitions) _asteroidsDefinitions = LoaderHelpers.GetJObjectFromJsonFile(UrhoConfig.Assets.Urho2D.RubePhysics.ASTEROIDS);

            // create asteroids node and asteroid
            this._asteroids = this.Node.CreateChild("asteroids");
            this._createAsteroid(5);

            // attach physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D += _onPhysicsBeginContact;
        }

        protected override void _destroy()
        {
            base._destroy();

            // remove physics events
            this.Scene.GetComponent<PhysicsWorld2D>().PhysicsBeginContact2D -= _onPhysicsBeginContact;
        }




        private void _createAsteroid(int size, Vector2 position = default)
        {
            Graphics graphics = this.Application.Graphics;

            // Create asteroid physics from rube format
            B2dJson b2dJson = LoaderHelpers.ReadIntoNodeFromValue(_asteroidsDefinitions, this._asteroids, false, UrhoConfig.Assets.Urho2D.RubePhysics.PATH);
            // get asteroids spritesheet
            var cache = Application.Current.ResourceCache;
            SpriteSheet2D spriteSheet = cache.GetSpriteSheet2D(UrhoConfig.Assets.Urho2D.Sprites.ASTEROIDS_SHEET);
            // attach physics to sprite
            string asteroidId = "01"; // RandomHelpers.NextRandom(1, 16).ToString("00");
            RigidBody2D asteroidBody = b2dJson.GetBodyByName(string.Format(UrhoConfig.Names.RUBE_ASTEROIDS_BODY, asteroidId));
            Node asteroidNode = asteroidBody.Node;
            StaticSprite2D asteroidSprite = asteroidNode.CreateComponent<StaticSprite2D>();
            asteroidSprite.Sprite = spriteSheet.GetSprite(string.Format(UrhoConfig.Names.SPRITE_SHEET_ASTEROIDS, asteroidId));

            // store asteroid size
            asteroidNode.SetVar(_asteroidSizeVarStringHash, size.ToString());

            // position
            Vector3 position3D = position == default ? 
                Camera.ScreenToWorldPoint(new Vector3(RandomHelpers.NextRandom(0.0f, 1.0f), RandomHelpers.NextRandom(0.0f, 1.0f), 0.0f)) :
                new Vector3(position);
            float angle = RandomHelpers.NextRandom(0.0f, 360.0f);
            asteroidNode.SetTransform2D(new Vector2(position3D.X, position3D.Y), angle);
            // scale 
            if (size > 1) asteroidNode.SetScale2D(_getScaleFromSize(size));

            // configure movement            
            asteroidBody.AngularVelocity = RandomHelpers.NextRandom(-1.0f, 1.0f);

            float velocityX = (float)Math.Sin(MathHelper.DegreesToRadians(angle));
            float velocityY = (float)Math.Cos(MathHelper.DegreesToRadians(angle));

            asteroidBody.SetLinearVelocity(new Vector2(velocityX, velocityY));
        }



        private void _splitAsteroid(Node asteroid)
        {
            // fragments 
            // int fragments = RandomHelpers.NextRandom(2, 5);
            // int fragments = 4;

            // current size
            int size = Convert.ToInt32(asteroid.GetVar(_asteroidSizeVarStringHash)) -1;
            asteroid.SetVar(_asteroidSizeVarStringHash, size.ToString());

            // create fragments
            switch (size)
            {
                case 5:
                    for (int i = 0; i < 4; i++) this._createAsteroid(size, asteroid.Position2D);                    
                    break;
                case 4:
                    for (int i = 0; i < 4; i++) this._createAsteroid(size, asteroid.Position2D);
                    break;
                case 3:
                    for (int i = 0; i < 3; i++) this._createAsteroid(size, asteroid.Position2D);
                    break;
                case 2:
                    for (int i = 0; i < 2; i++) this._createAsteroid(size, asteroid.Position2D);
                    break;
                default:
                    // 1 not split, last size
                    break;
            }

            asteroid.Remove();
        }

        private Vector2 _getScaleFromSize(int size)
        {
            Vector2 result;

            switch (size)
            {
                case 5:
                    result = new Vector2(1.0f, 1.0f);
                    break;
                case 4:
                    result = new Vector2(0.7f, 0.2f);
                    break;
                case 3:
                    result = new Vector2(0.5f, 0.5f);
                    break;
                case 2:
                    result = new Vector2(0.25f, 0.25f);                    
                    break;
                default:
                    result = new Vector2(0.10f, 0.10f);
                    break;
            }

            return result;
        }

        private void _onPhysicsBeginContact(PhysicsBeginContact2DEventArgs args)
        {
            // get asteroid and other object
            Node asteroid = null, otherObject = null;
            foreach (var item in this._asteroids.Children)
            {
                otherObject = args.GetOther(item);
                if (null != otherObject) { asteroid = item; break; }
            }
            if (null == otherObject) return;

            // collision actions
            switch (otherObject.Name)
            {
                // Bullet
                case UrhoConfig.Names.RUBE_BULLET_BODY:
                    this._splitAsteroid(asteroid);
                    break;
                // Ship
                case UrhoConfig.Names.RUBE_SHIP_BODY:
                    this._splitAsteroid(asteroid);
                    break;
            }
        }

    }
}
