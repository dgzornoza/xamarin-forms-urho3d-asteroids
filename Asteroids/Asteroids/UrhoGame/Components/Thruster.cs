using System;
using System.Collections.Generic;
using System.Text;
using Urho;
using Urho.Urho2D;
using XamarinForms.Toolkit.Urho3D;

namespace Asteroids.UrhoGame.Components
{
    public class Thruster : Component
    {
        private ParticleEmitter _particleEmitter;
        private float _offset = 1;

        public Thruster()
        {
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

        public void SetParametersFromBody(RigidBody2D body)
        {
            float maxlength = 15.0f;
            this.Node.Rotation2D = this.Node.Parent.Rotation2D - 90;//  body.Node.Rotation2D - 90;
            Vector2 rotation2D = MathExtensions.DegreeToVector2(this.Node.Rotation2D);
            Vector2 forceVector = (rotation2D * ((body.LinearVelocity.LengthFast * 5) / maxlength));            

            this._particleEmitter.Effect.ConstantForce = new Vector3(forceVector);
            this._particleEmitter.Effect.MinDirection = new Vector3(rotation2D);
            // this._particleEmitter.Effect.MaxDirection = new Vector3(rotation2D);

            // this._particleEmitter.Effect.MinParticleSize = this._particleEmitter.Effect.MaxParticleSize = rotation2D;
            this._particleEmitter.Effect.MinEmissionRate = (body.LinearVelocity.LengthFast * 80) / maxlength - 2;
            // this._particleEmitter.Effect.SizeAdd = (body.LinearVelocity.LengthFast * _offset) / maxlength;

            this._particleEmitter.Effect.SizeAdd = _offset;
        }

        private void _initialize()
        {
            var cache = this.Application.ResourceCache;
            ParticleEffect particleEffect = cache.GetParticleEffect("Particles/thruster.xml");
            if (particleEffect == null) return;

            this._particleEmitter = this.Node.CreateComponent<ParticleEmitter>();
            this._particleEmitter.Effect = particleEffect;

            this.Node.SetScale(1.5f);
            // node text info
            NodeTextInfo nodeInfo = this.Node.CreateComponent<NodeTextInfo>();
            nodeInfo.VerticalTextAlignment = Urho.Gui.VerticalAlignment.Bottom;


            this.Application.Input.KeyDown += (obj) =>
            {
                // OFFSET SIZEADD
                if (obj.Key == Key.N1)
                {
                    _offset += 1;
                }
                else if (obj.Key == Key.N2)
                {
                    _offset -= 1;
                }

                // EMISIONRATE
                else if (obj.Key == Key.F)
                {
                    this._particleEmitter.Effect.MinEmissionRate += 1;
                }
                else if (obj.Key == Key.V)
                {
                    this._particleEmitter.Effect.MinEmissionRate -= 1;
                }
                else if(obj.Key == Key.N4)
                {
                    this._particleEmitter.Effect.MaxEmissionRate += 1;
                }
                else if (obj.Key == Key.R)
                {
                    this._particleEmitter.Effect.MaxEmissionRate -= 1;
                }

                // MinParticleSize
                else if (obj.Key == Key.G)
                {
                    this._particleEmitter.Effect.MinDirection = new Vector3(this._particleEmitter.Effect.MinDirection.X, this._particleEmitter.Effect.MinDirection.Y + 0.1f, this._particleEmitter.Effect.MinDirection.Z);
                }
                else if (obj.Key == Key.B)
                {
                    this._particleEmitter.Effect.MinDirection = new Vector3(this._particleEmitter.Effect.MinDirection.X, this._particleEmitter.Effect.MinDirection.Y - 0.1f, this._particleEmitter.Effect.MinDirection.Z);
                }
                else if (obj.Key == Key.N5)
                {
                    this._particleEmitter.Effect.MinDirection = new Vector3(this._particleEmitter.Effect.MinDirection.X + 0.1f, this._particleEmitter.Effect.MinDirection.Y, this._particleEmitter.Effect.MinDirection.Z);
                }
                else if (obj.Key == Key.T)
                {
                    this._particleEmitter.Effect.MinDirection = new Vector3(this._particleEmitter.Effect.MinDirection.X - 0.1f, this._particleEmitter.Effect.MinDirection.Y, this._particleEmitter.Effect.MinDirection.Z);
                }

            };
        }

    }
}
