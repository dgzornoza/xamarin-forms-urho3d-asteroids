using System;
using System.Collections.Generic;
using System.Text;

namespace Asteroids.UrhoGame
{
    public class UrhoConfig
    {
        public static class Names
        {
            public const string MAIN_CAMERA_NODE = "MainCamera";

            public const string RUBE_BULLET_BODY = "bullet-body";
            public const string RUBE_SHIP_BODY = "ship-body";
            public const string RUBE_ASTEROIDS_BODY = "asteroid_{0}-body";

            public const string SPRITE_SHEET_ASTEROIDS = "asteroid_{0}";
        }



        /// <summary>
        /// Class with Assets resource names
        /// </summary>
        public static class Assets
        {
            public static class Fonts
            {
                public const string FONT = "Fonts/Anonymous Pro.ttf";
            }

            public static class Materials
            {
            }

            public static class Particles
            {
                public const string THRUSTER = "Particles/thruster.xml";
            }

            public static class Sounds
            {
            }

            public static class Textures
            {
            }

            public static class Urho2D
            {
                public static class RubePhysics
                {
                    public const string PATH = "Urho2D/RubePhysics/";
                    public const string ASTEROIDS = "Urho2D/RubePhysics/asteroids.json";
                    public const string BULLET = "Urho2D/RubePhysics/bullet.json";
                    public const string SHIP = "Urho2D/RubePhysics/ship.json";
                }

                public static class Sprites
                {
                    public const string BULLET = "Urho2D/Sprites/bullet.png";
                    public const string SHIP = "Urho2D/Sprites/ship.png";
                    public const string ASTEROIDS_SHEET = "Urho2D/Sprites/Asteroids.xml";
                }
            }




        }
    }
}
