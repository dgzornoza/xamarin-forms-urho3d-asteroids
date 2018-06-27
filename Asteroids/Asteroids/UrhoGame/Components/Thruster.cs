﻿using System;
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

            this._particleEmitter.Effect.SizeAdd = (body.LinearVelocity.LengthFast * 15) / 15;
        }

        private void _initialize()
        {
            var cache = this.Application.ResourceCache;
            ParticleEffect particleEffect = cache.GetParticleEffect("Particles/thruster.xml");
            if (particleEffect == null) return;

            this._particleEmitter = this.Node.CreateComponent<ParticleEmitter>();
            this._particleEmitter.Effect = particleEffect;
        }
    }
}
