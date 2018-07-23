﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Helpers;
using XamarinForms.Toolkit.Urho3D;
using XamarinForms.Toolkit.Urho3D.Rube;

namespace Asteroids.UrhoGame.Components
{
    public class Asteroid : BaseComponent
    {
        private static JObject _asteroidsDefinitions;
        // private static StringHash _lifeTimeVarStringHash = new StringHash("life-time");

        

            
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
            if (null == _asteroidsDefinitions) _asteroidsDefinitions = LoaderHelpers.GetJObjectFromJsonFile(UrhoConfig.Assets.Urho2D.RubePhysics.ASTEROIDS);

            // create asteroids node
            this._asteroids = this.Node.CreateChild("asteroids");

            this._createAsteroid();
        }




        private void _createAsteroid()
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
            StaticSprite2D asteroidSprite = asteroidBody.Node.CreateComponent<StaticSprite2D>();
            asteroidSprite.Sprite = spriteSheet.GetSprite(string.Format(UrhoConfig.Names.SPRITE_SHEET_ASTEROIDS, asteroidId));
            
            asteroidBody.Node.NodeCollisionStart += _onNodeCollisionStart;

            // random position
            Vector3 position = Camera.ScreenToWorldPoint(new Vector3(RandomHelpers.NextRandom(0.0f, 1.0f), RandomHelpers.NextRandom(0.0f, 1.0f), 0.0f));
            float angle = RandomHelpers.NextRandom(0.0f, 360.0f);

            // configure movement
            asteroidBody.Node.SetTransform2D(new Vector2(position.X, position.Y), angle);

            asteroidBody.AngularVelocity = RandomHelpers.NextRandom(-1.0f, 1.0f);

            float velocityX = (float)Math.Sin(MathHelper.DegreesToRadians(angle));
            float velocityY = (float)Math.Cos(MathHelper.DegreesToRadians(angle));

            asteroidBody.SetLinearVelocity(new Vector2(velocityX, velocityY));
        }

        private void _onNodeCollisionStart(NodeCollisionStartEventArgs obj)
        {
            int a = 5;
            int b = a / 5;
            //var bulletNode = args.OtherNode;
            //if (IsAlive && bulletNode.Name != null && bulletNode.Name.StartsWith(nameof(Weapon)) && args.Body.Node == Node)
            //{
            //}
        }
    }
}
