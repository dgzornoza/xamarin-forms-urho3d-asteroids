using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Urho;
using Urho.Gui;
using Urho.Urho2D;
using XamarinForms.Toolkit.Helpers;

namespace Asteroids.UrhoGame
{


    public class UrhoApp : Application
    {
#if DEBUG
        private MonoDebugHud _monoDebugHud;
#endif

        private Scene _scene;
        private Camera _mainCamera;

        private bool _drawDebug = true;



        [Preserve]
        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
        }

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
            Log.LogMessage += e => $"[{e.Level}] {e.Message}".RegisterLog();
            
            base.Start();

#if DEBUG
            _monoDebugHud = new MonoDebugHud(this);
            _monoDebugHud.Show();
#endif

            this._createScene();
            this._createCamera();
            this._setupViewport();


            this._subscribeToEvents();

            // pantalla principal
            this._scene.CreateChild(nameof(Stages.SpaceArenaStage)).CreateComponent<Stages.SpaceArenaStage>();
        }



        protected override void OnUpdate(float timeStep)
        {
        }



        private void _createScene()
        {
            // create scene
            this._scene = new Scene();            
            this._scene.CreateComponent<Octree>();
#if DEBUG
            this._scene.CreateComponent<DebugRenderer>();
#endif
        }


        private void _createCamera()
        {
            // Create camera
            Node CameraNode = _scene.CreateChild(UrhoConfig.Names.MAIN_CAMERA_NODE);
            CameraNode.Position = (new Vector3(0.0f, 0.0f, -0.10f));
            this._mainCamera = CameraNode.CreateComponent<Camera>();
            this._mainCamera.Orthographic = true;

            var graphics = Graphics;
            // x = Screen Width (px)
            // y = Screen Height(px)
            // s = Desired Height of Photoshop Square(px)
            // Camera Size = x / ((( x / y ) * 2 ) * s ) = 10 sprites de 's'
            // this._camera.OrthoSize = graphics.Width / (((graphics.Width / graphics.Height) * 2) * 32);

            this._mainCamera.OrthoSize = (float)graphics.Height * PixelSize;
            // set zoom with design resolution (zomm (1.0) for view in 16:9 1920X1080)
            this._mainCamera.Zoom = 1.0f * Math.Min((float)graphics.Width / 1920, (float)graphics.Height / 1080);
        }




        private void _setupViewport()
        {
            Viewport viewport = new Viewport(Context, this._scene, this._mainCamera, null);
            var renderer = Renderer;

            renderer.SetViewport(0, viewport);

            // Set background color for the scene
            // Zone zone = renderer.DefaultZone;
            // zone.FogColor = (new Color(1f, 0.1f, 0.1f));
        }




        private void _subscribeToEvents()
        {
            Engine.PostRenderUpdate += (PostRenderUpdateEventArgs obj) =>
            {
                // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
                // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
                // bones properly
                if (_drawDebug)
                {
                    //this._scene.GetComponent<PhysicsWorld2D>()?.DrawDebugGeometry();
                    // Renderer.DrawDebugGeometry(false);                    
                }
            };
        }


    }
}
