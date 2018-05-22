using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Resources;
using Urho.Shapes;

namespace Asteroids.Game.Components
{
    /// <summary>
    /// Clase con un componente para implementar la nave del jugo
    /// </summary>
    public class ShipComponent : Component
    {
        Node _shipNode;
        bool _flashOnly;

        public ShipComponent()
        {
            // ReceiveSceneUpdates = true;

            _flashOnly = true;
            
        }




        #region [Component]

        public override void OnAttachedToNode(Node node)
        {
            this._shipNode = node.CreateChild();
            this._createGeometry();

            base.OnAttachedToNode(node);
        }

        //protected override void OnUpdate(float timeStep)
        //{            
        //}

        #endregion [Component]


        private void _createGeometry()
        {
            // https://forums.xamarin.com/discussion/81723/how-to-shade-surfaces-of-customgeometry-with-solid-color
            // https://forums.xamarin.com/discussion/70135/draw-circles-or-line

            CustomGeometry geometry = this._shipNode.CreateComponent<CustomGeometry>();
            geometry.BeginGeometry(0, PrimitiveType.TriangleStrip);


            // geometry.SetMaterial(UrhoApp.Current.ResourceCache.GetMaterial($"Materials/MatDiffEmissive.xml", sendEventOnFailure: false));
            var material = new Material();
            material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);
            // material.SetTechnique(0, CoreAssets.Techniques.DiffEmissive);
            // material.SetShaderParameter("MatEmissiveColor", new Vector3(0, 0, 4));
            // material.CullMode = CullMode.None; //didn't see any difference with or without
            geometry.SetMaterial(material);

            float size = 2;
            Vector3 p0 = new Vector3(0, 0, 0);
            Vector3 p1 = new Vector3(size, 0, 0);
            Vector3 p2 = new Vector3(0, -size, 0);
            // Vector3 p3 = new Vector3(10, 10, 0);

            // DEFINE BOTTOM FACE
            geometry.DefineVertex(p0);
            geometry.DefineColor(_flashOnly ? Color.Red : Color.Blue);
            geometry.DefineVertex(p1);
            geometry.DefineColor(_flashOnly ? Color.Red : Color.Blue);
            geometry.DefineVertex(p2);
            geometry.DefineColor(_flashOnly ? Color.Red : Color.Blue);

            //// FRONT FACE
            //geom.DefineVertex(p0);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Blue);
            //geom.DefineVertex(p1);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Blue);
            //geom.DefineVertex(p3);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Magenta);

            //// LEFT FACE
            //geom.DefineVertex(p0);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Cyan);
            //geom.DefineVertex(p2);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Blue);
            //geom.DefineVertex(p3);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Magenta);

            //// RIGHT FACE
            //geom.DefineVertex(p1);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Cyan);
            //geom.DefineVertex(p2);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Cyan);
            //geom.DefineVertex(p3);
            //geom.DefineColor(_flashOnly ? Color.Red : Color.Magenta);


            geometry.Commit();
        }

        
    }
}
