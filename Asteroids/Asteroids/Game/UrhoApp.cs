using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Gui;

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
            var renderer = Renderer;
            renderer.SetViewport(0, new Viewport(Context, this._scene, this._camera, null));            
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


            // Create Text Element
            var text = new Text()
            {
                Value = "Hello World!",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            text.SetColor(Color.Cyan);
            text.SetFont(font: ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), size: 30);
            // Add to UI Root
            UI.Root.AddChild(text);
        }



    }
}
