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
        private float _offset = 20;

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
            // this.Node.Rotation2D = body.Node.Rotation2D - 90;
            // Vector2 rotation2D = MathExtensions.DegreeToVector2(this.Node.Rotation2D) * 5;
            // this._particleEmitter.Effect.ConstantForce = new Vector3(rotation2D);

            this._particleEmitter.Effect.SizeAdd = (body.LinearVelocity.LengthFast * _offset) / 15;
        }

        private void _initialize()
        {
            var cache = this.Application.ResourceCache;
            ParticleEffect particleEffect = cache.GetParticleEffect("Particles/thruster.xml");
            if (particleEffect == null) return;

            this._particleEmitter = this.Node.CreateComponent<ParticleEmitter>();
            this._particleEmitter.Effect = particleEffect;


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

                // VELOCITY
                else if(obj.Key == Key.G)
                {
                    this._particleEmitter.Effect.MinVelocity += 1;
                }
                else if (obj.Key == Key.B)
                {
                    this._particleEmitter.Effect.MinVelocity -= 1;
                }
                else if (obj.Key == Key.N5)
                {
                    this._particleEmitter.Effect.MaxVelocity += 1;
                }
                else if (obj.Key == Key.T)
                {
                    this._particleEmitter.Effect.MaxVelocity -= 1;
                }

                // timetolive
                else if (obj.Key == Key.H)
                {
                    this._particleEmitter.Effect.MinTimeToLive += .05f;
                }
                else if (obj.Key == Key.N)
                {
                    this._particleEmitter.Effect.MinTimeToLive -= .05f;
                }
                else if (obj.Key == Key.N6)
                {
                    this._particleEmitter.Effect.MaxTimeToLive += .05f;
                }
                else if (obj.Key == Key.Y)
                {
                    this._particleEmitter.Effect.MaxTimeToLive -= .05f;
                }
            };
        }

    }
}
