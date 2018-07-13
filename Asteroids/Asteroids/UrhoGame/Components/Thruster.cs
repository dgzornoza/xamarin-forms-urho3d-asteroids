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

        /// <summary>
        /// Function for set dynamic thruster parameters
        /// </summary>
        /// <param name="thrust">thrust for configure dynamic particles</param>
        /// <param name="thrustLimit">limit thrust for configure dynamic particles</param>
        public void SetParameters(float thrust, float thrustLimit)
        {
            float velocity = thrust / thrustLimit;

            // particles rotation with node
            this._particleEmitter.Effect.MinRotation = this._particleEmitter.Effect.MaxRotation = -this.Node.WorldRotation2D;

            // size particles increment with thrust in Y coodinates (for line effect on lower thruster)
            float minParticleSize = (velocity * 0.2f);
            float maxParticleSize = (velocity * 0.5f);
            this._particleEmitter.Effect.MinParticleSize = new Vector2(minParticleSize + 0.1f, 0.2f);
            this._particleEmitter.Effect.MaxParticleSize = new Vector2(maxParticleSize + 0.1f, 0.2f);
        }

        private void _initialize()
        {
            var cache = this.Application.ResourceCache;
            ParticleEffect particleEffect = cache.GetParticleEffect("Particles/thruster.xml");
            if (particleEffect == null) return;

            this._particleEmitter = this.Node.CreateComponent<ParticleEmitter>();
            this._particleEmitter.Effect = particleEffect;
        }


        ///// <summary>
        ///// http://gizma.com/easing/
        /////  t = time, b = startvalue, c = change in value, d = duration:
        ///// </summary>
        ///// <param name="t">time</param>
        ///// <param name="b">start value</param>
        ///// <param name="c">Change in value</param>
        ///// <param name="d">duration</param>
        ///// <returns></returns>
        //private float _easeInCubic(float t, float b, float c, float d)
        //{
        //    t /= d;
        //    return c * t * t * t + b;
        //}

    }
}
