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
        private ParticleEmitter2D _particleEmitter;

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
            this.Node.Rotation2D = body.Node.Rotation2D -90;
            this._particleEmitter.Effect.Gravity = MathHelpers.DegreeToVector2(-this.Node.Rotation2D) * 3000;
        }

        private void _initialize()
        {
            var cache = this.Application.ResourceCache;
            ParticleEffect2D particleEffect = cache.GetParticleEffect2D("Urho2D/Particles/thruster.pex");
            if (particleEffect == null) return;

            this._particleEmitter = this.Node.CreateComponent<ParticleEmitter2D>();
            this._particleEmitter.Effect = particleEffect;            
        }
    }
}
