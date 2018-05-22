using Asteroids.Game.Components;
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

            this._createCamera();

            // this._createBackground();

            Node ShipNode = this._scene.CreateChild("Ship1");
            ShipNode.CreateComponent<ShipComponent>();
            ShipNode.Position = new Vector3(0.0f, 0.0f, 0.0f);
        }


        private void _createCamera()
        {
            // Create camera
            Node CameraNode = _scene.CreateChild("MainCamera");
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
            this._camera = CameraNode.CreateComponent<Camera>();
            this._camera.Orthographic = true;


            var graphics = Graphics;
            // x = Screen Width (px)
            // y = Screen Height(px)
            // s = Desired Height of Photoshop Square(px)
            // Camera Size = x / ((( x / y ) * 2 ) * s ) = 10 sprites de 's'
            // this._camera.OrthoSize = graphics.Width / (((graphics.Width / graphics.Height) * 2) * 32);

            this._camera.OrthoSize = (float)graphics.Height * PixelSize;
            // establecer el zoom segun la resolucion de diseño para asegurar visibilidad completa (zomm (1.0) para completa visibildiad en una resolucion 3:2 1920X1280)
            this._camera.Zoom = 1.0f * Math.Min((float)graphics.Width / 1080.0f, (float)graphics.Height / 720.0f);
        }


        private void _createBackground()
        {
            var cache = ResourceCache;
            Sprite2D backgroundSprite = cache.GetSprite2D("Textures/stars.png");

            if (backgroundSprite is null)
            {
                return;
            }

            Node backgroundNode = this._scene.CreateChild("StaticSprite2D");
            backgroundNode.Position = new Vector3(0.0f, 0.0f, 0.0f);

            StaticSprite2D backgroundNodeStaticSprite = backgroundNode.CreateComponent<StaticSprite2D>();
            // blend mode
            backgroundNodeStaticSprite.BlendMode = BlendMode.Alpha;
            // backgroundNodeStaticSprite.Alpha = .5f;
            // sprite
            backgroundNodeStaticSprite.Sprite = backgroundSprite;
        }
    }
}
