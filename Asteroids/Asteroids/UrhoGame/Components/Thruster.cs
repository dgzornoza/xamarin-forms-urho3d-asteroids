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
        private float _offset = 0;

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
            float velocityLimit = 15.0f;
            float minParticleSizeLimit = 0.4f;
            float maxParticleSizeLimit = 0.8f;
            float velocity = body.LinearVelocity.LengthFast / velocityLimit;

            // Vector2 rotation = MathExtensions.DegreeToVector2(this.Node.WorldRotation2D - 180.0f);
            // Vector2 perpendicularRotation = MathExtensions.DegreeToVector2(this.Node.WorldRotation2D - 90);
            this._particleEmitter.Effect.MinRotation = this._particleEmitter.Effect.MaxRotation = -this.Node.WorldRotation2D;



            // Vector2 forceVector = rotation * ((body.LinearVelocity.LengthFast * 5) / maxlength);            

            // this._particleEmitter.Effect.ConstantForce = new Vector3(forceVector);
            //this._particleEmitter.Effect.MinDirection = new Vector3(rotation2D);
            // this._particleEmitter.Effect.MaxDirection = new Vector3(rotation2D);

            float minParticleSize = (velocity * 0.3f) + 0.1f;
            float maxParticleSize = (velocity * 0.7f) + 0.1f;
            this._particleEmitter.Effect.MinParticleSize = new Vector2(this._particleEmitter.Effect.MinParticleSize.X, minParticleSize);
            this._particleEmitter.Effect.MaxParticleSize = new Vector2(this._particleEmitter.Effect.MaxParticleSize.X, maxParticleSize);
            // this._particleEmitter.Effect.MinParticleSize = perpendicularRotation;
            // this._particleEmitter.Effect.MaxParticleSize = perpendicularRotation;


            // this._particleEmitter.Effect.MinEmissionRate = (body.LinearVelocity.LengthFast * 40) / maxlength - 2;
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

                // recargar
                else if (obj.Key == Key.F)
                {
                    cache.ReleaseAllResources(true);
                    this._particleEmitter.Effect = cache.GetParticleEffect("Particles/thruster.xml");
                }


            };
        }

    }
}
