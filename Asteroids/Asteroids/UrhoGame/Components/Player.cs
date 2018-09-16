using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Urho;
using Urho.Actions;

namespace Asteroids.UrhoGame.Components
{
    /// <summary>
    /// Player component
    /// </summary>
    public class Player : Ship
    {
        private int _lives;

        public Player()
        {
            _lives = UrhoConfig.Data.PLAYER_LIVES;
        }




        protected override void _initialize()
        {
            base._initialize();

            // attach events
            this.OnShipDestroy += _onShipDestroy;
        }



        protected override void _destroy()
        {
            base._destroy();

            // remove events
            this.OnShipDestroy -= _onShipDestroy;
        }




        private void _onShipDestroy(object sender, EventArgs e)
        {
            this._shipNode.RunActionsAsync(_blinkActions().ToArray());
        }


        private IList<FiniteTimeAction> _blinkActions()
        {
            FiniteTimeAction[] blinkAction() { return new FiniteTimeAction[] { new FadeOut(0.5f), new FadeOut(0.5f).Reverse() }; }

            List<FiniteTimeAction> result = new List<FiniteTimeAction>(UrhoConfig.Data.SHIP_RESET_BLINK_TIMES * 2);
            for (int i = 0; i < UrhoConfig.Data.SHIP_RESET_BLINK_TIMES * 2; i += 2) result.AddRange(blinkAction());

            return result;
        }



    }
}
