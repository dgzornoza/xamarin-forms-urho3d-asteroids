using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Urho2D;

namespace Asteroids.Game
{
    public class UrhoApp : Application
    {

        private Scene _scene;
        private Camera _camera;



        [Preserve]
        public UrhoApp(ApplicationOptions options = null) : base(options) { }

        static UrhoApp()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached) Debugger.Break();
                e.Handled = true;
            };
        }

        protected override void Start()
        {
            base.Start();
            _createScene();
            _setupViewport();
        }





        private void _setupViewport()
        {
            Viewport viewport = new Viewport(Context, this._scene, this._camera, null);
            var renderer = Renderer;
            renderer.SetViewport(0, viewport);
        }

        private void _createScene()
        {
            // https://discourse.urho3d.io/t/draw-a-line-with-custom-geometry-in-2d/3192/2

            // create scene
            this._scene = new Scene();
            this._scene.CreateComponent<Octree>();

            // Create camera
            Node CameraNode = _scene.CreateChild("Camera");            
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
            this._camera = CameraNode.CreateComponent<Camera>();
            this._camera.Orthographic = true;


            var graphics = Graphics;
            this._camera.OrthoSize = (float)graphics.Height * PixelSize;
            this._camera.Zoom = 1.5f * Math.Min((float)graphics.Width / 1920.0f, (float)graphics.Height / 1080.0f);
            // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.2) is set for full visibility at 1280x800 resolution)



            //----------------------------------------------------------------------------------------
            // background
            var cache = ResourceCache;
            Sprite2D backgroundSprite = cache.GetSprite2D("Textures/stars.png");
            
            if (backgroundSprite is null)
            {
                return;
            }

            Node backgroundNode = this._scene.CreateChild("StaticSprite2D");
            backgroundNode.Position = new Vector3(0.0f, 0.0f, 0.0f);

            StaticSprite2D backgroundNodeStaticSprite = backgroundNode.CreateComponent<StaticSprite2D>();
            // Set blend mode
            backgroundNodeStaticSprite.BlendMode = BlendMode.Alpha;
            backgroundNodeStaticSprite.Alpha = .5f;
            // Set sprite
            backgroundNodeStaticSprite.Sprite = backgroundSprite;
            
        }

    }
}
