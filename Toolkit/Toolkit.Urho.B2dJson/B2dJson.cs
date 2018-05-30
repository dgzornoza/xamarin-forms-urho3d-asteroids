using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Urho;
using Urho.Urho2D;

namespace Toolkit.UrhoSharp.B2dJson
{

    internal static class Helpers
    {
        public static IEnumerable<T> GetRecursiveComponents<T>(this Urho.Node node)
        {
            List<T> components = node.Components.OfType<T>().ToList();

            // si se procesa en cascada, se hace lo mismo en los nodos hijos recursivamente
            foreach (var child in node.Children) components.AddRange(child.GetRecursiveComponents<T>());

            return components;
        }
    }

    public class B2dJsonColor4
    {
        public B2dJsonColor4() { R = G = B = A = 255; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    };

    public class B2dJsonCustomProperties
    {
        public Dictionary<string, T> GetCustomPropertyMapFromType<T>()
        {
            switch (typeof(T))
            {
                case Type intType when intType == typeof(int): return m_customPropertyMap_int as Dictionary<string, T>;
                case Type floatType when floatType == typeof(float): return m_customPropertyMap_float as Dictionary<string, T>;
                case Type stringType when stringType == typeof(string): return m_customPropertyMap_string as Dictionary<string, T>;
                case Type vectorType when vectorType == typeof(Vector2): return m_customPropertyMap_b2Vec2 as Dictionary<string, T>;
                case Type boolType when boolType == typeof(bool): return m_customPropertyMap_bool as Dictionary<string, T>;
                case Type colorType when colorType == typeof(B2dJsonColor4): return m_customPropertyMap_color as Dictionary<string, T>;
            }

            return new Dictionary<string, T>();
        }

        public Dictionary<string, int> m_customPropertyMap_int { get; set; }
        public Dictionary<string, float> m_customPropertyMap_float { get; set; }
        public Dictionary<string, string> m_customPropertyMap_string { get; set; }
        public Dictionary<string, Vector2> m_customPropertyMap_b2Vec2 { get; set; }
        public Dictionary<string, bool> m_customPropertyMap_bool { get; set; }
        public Dictionary<string, B2dJsonColor4> m_customPropertyMap_color { get; set; }
    };

    public class B2dJson
    {
        /// <summary>From box2d</summary>
        private const int b2_maxPolygonVertices = 8;

        private const string URHO2D_PHYSIC_ROOT_NODE_NAME = "B2dJsonPhysicWorldRoot";
        private readonly CreateMode m_creationMode;

        protected bool m_useHumanReadableFloats;
        protected Dictionary<int, RigidBody2D> m_indexToBodyMap;
        protected Dictionary<RigidBody2D, int> m_bodyToIndexMap;
        protected Dictionary<Constraint2D, int> m_jointToIndexMap;
        protected List<RigidBody2D> m_bodies;
        protected List<Constraint2D> m_joints;
        protected List<B2dJsonImage> m_images;

        protected Dictionary<RigidBody2D, string> m_bodyToNameMap;
        protected Dictionary<CollisionShape2D, string> m_fixtureToNameMap;
        protected Dictionary<Constraint2D, string> m_jointToNameMap;
        protected Dictionary<B2dJsonImage, string> m_imageToNameMap;

        protected Dictionary<RigidBody2D, string> m_bodyToPathMap;
        protected Dictionary<CollisionShape2D, string> m_fixtureToPathMap;
        protected Dictionary<Constraint2D, string> m_jointToPathMap;
        protected Dictionary<B2dJsonImage, string> m_imageToPathMap;

        // This maps an item (RigidBody2D, CollisionShape2D etc) to a set of custom properties.
        // Use null for world properties.
        protected Dictionary<object, B2dJsonCustomProperties> m_customPropertiesMap;

        // These are necessary to know what type of item the entries in the map above
        // are, which is necessary for the getBodyByCustomInt type functions.
        // We could have used a separate map for each item type, but there are many
        // combinations of item type and property type and the overall amount of
        // explicit coding to do becomes very large for no real benefit.
        protected SortedSet<RigidBody2D> m_bodiesWithCustomProperties;
        protected SortedSet<CollisionShape2D> m_fixturesWithCustomProperties;
        protected SortedSet<Constraint2D> m_jointsWithCustomProperties;
        protected SortedSet<B2dJsonImage> m_imagesWithCustomProperties;
        protected SortedSet<PhysicsWorld2D> m_worldsWithCustomProperties;


        /// <summary>
        /// default constructor
        /// </summary>
        public B2dJson(CreateMode creationMode = CreateMode.Local, bool useHumanReadableFloats = false)
        {
            m_creationMode = creationMode;
            m_useHumanReadableFloats = useHumanReadableFloats;

            m_indexToBodyMap = new Dictionary<int, RigidBody2D>();
            m_bodyToIndexMap = new Dictionary<RigidBody2D, int>();
            m_jointToIndexMap = new Dictionary<Constraint2D, int>();
            m_bodies = new List<RigidBody2D>();
            m_joints = new List<Constraint2D>();
            m_images = new List<B2dJsonImage>();

            m_bodyToNameMap = new Dictionary<RigidBody2D, string>();
            m_fixtureToNameMap = new Dictionary<CollisionShape2D, string>();
            m_jointToNameMap = new Dictionary<Constraint2D, string>();
            m_imageToNameMap = new Dictionary<B2dJsonImage, string>();

            m_bodyToPathMap = new Dictionary<RigidBody2D, string>();
            m_fixtureToPathMap = new Dictionary<CollisionShape2D, string>();
            m_jointToPathMap = new Dictionary<Constraint2D, string>();
            m_imageToPathMap = new Dictionary<B2dJsonImage, string>();

            m_customPropertiesMap = new Dictionary<object, B2dJsonCustomProperties>();

            m_bodiesWithCustomProperties = new SortedSet<RigidBody2D>();
            m_fixturesWithCustomProperties = new SortedSet<CollisionShape2D>();
            m_jointsWithCustomProperties = new SortedSet<Constraint2D>();
            m_imagesWithCustomProperties = new SortedSet<B2dJsonImage>();
            m_worldsWithCustomProperties = new SortedSet<PhysicsWorld2D>();
        }


        public void Clear()
        {
            m_indexToBodyMap.Clear();
            m_bodyToIndexMap.Clear();
            m_jointToIndexMap.Clear();
            m_bodies.Clear();
            m_joints.Clear();
            m_images.Clear();

            m_bodyToNameMap.Clear();
            m_fixtureToNameMap.Clear();
            m_jointToNameMap.Clear();
            m_imageToNameMap.Clear();

            m_bodyToPathMap.Clear();
            m_fixtureToPathMap.Clear();
            m_jointToPathMap.Clear();
            m_imageToPathMap.Clear();
        }


        #region [writing functions]

        /// <summary>
        /// Write urho2d scene (only box2d physic world data) into b2djson format for R.U.B.E editor
        /// </summary>
        /// <param name="urhoScene">Urho2d scene</param>
        /// <returns>json object with urho2d physic world in b2djson format</returns>
        /// <remarks>
        /// b2djson do not maintain the tree structure of nodes with the physical components from urho2d, 
        /// so that if it is loaded, the structure of nodes will be different from the current one.
        /// </remarks>
        public JObject WriteToValue(Scene urhoScene)
        {
            PhysicsWorld2D world;
            if (null == urhoScene || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return new JObject();

            return B2j(world);
        }

        /// <summary>
        /// Write urho2d scene (only box2d physic world data) into b2djson format for R.U.B.E editor
        /// </summary>
        /// <param name="urhoScene">Urho2d scene</param>
        /// <returns>string with urho2d physic world in b2djson format</returns>
        /// <remarks>
        /// b2djson do not maintain the tree structure of nodes with the physical components from urho2d, 
        /// so that if it is loaded, the structure of nodes will be different from the current one.
        /// </remarks>
        public string WriteToString(Scene urhoScene)
        {
            PhysicsWorld2D world;
            if (null == urhoScene || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return string.Empty;

            return B2j(world).ToString();
        }

        /// <summary>
        /// Write urho2d scene (only box2d physic world data) into b2djson format for R.U.B.E editor
        /// </summary>
        /// <param name="urhoScene">Urho2d scene</param>
        /// <param name="filename">path to filename to write</param>
        /// <param name="errorMsg">error message</param>
        /// <returns>true if write to file, false otherwise</returns>
        /// <remarks>
        /// b2djson do not maintain the tree structure of nodes with the physical components from urho2d, 
        /// so that if it is loaded, the structure of nodes will be different from the current one.
        /// </remarks>
        public bool WriteToFile(Scene urhoScene, string filename, out string errorMsg)
        {
            errorMsg = string.Empty;
            PhysicsWorld2D world;
            if (null == urhoScene || string.IsNullOrWhiteSpace(filename) || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return false;

            using (TextWriter writeFile = new StreamWriter(filename))
            {
                try
                {
                    writeFile.WriteLine(B2j(world).ToString());
                }
                catch (Exception e)
                {
                    errorMsg = $"Error writing JSON to file: {filename} {e.Message}";
                    return false;
                }
            }

            return true;
        }


        public JObject B2j(PhysicsWorld2D world)
        {
            JObject worldValue = new JObject();

            m_bodyToIndexMap.Clear();
            m_jointToIndexMap.Clear();

            VecToJson("gravity", world.Gravity, worldValue);
            worldValue["allowSleep"] = world.AllowSleeping;
            worldValue["autoClearForces"] = world.AutoClearForces;
            worldValue["warmStarting"] = world.WarmStarting;
            worldValue["continuousPhysics"] = world.ContinuousPhysics;
            worldValue["subStepping"] = world.SubStepping;
            //worldValue["hasDestructionListener"] = world->HasDestructionListener();
            //worldValue["hasContactFilter"] = world->HasContactFilter();
            //worldValue["hasContactListener"] = world->HasContactListener();

            // TODO: index ??? parece no tener sentido junto con el diccionario
            // Body
            int index = 0;
            JArray jArray = new JArray();
            IEnumerable<RigidBody2D> worldBodyList = world.Scene.GetRecursiveComponents<RigidBody2D>();
            foreach (var item in worldBodyList)
            {
                m_bodyToIndexMap.Add(item, index);
                jArray.Add(B2j(item));
                index++;
            }
            worldValue["body"] = jArray;

            // Joints
            // need two passes for joints because gear joints reference other joints
            index = 0;
            jArray = new JArray();
            IEnumerable<Constraint2D> worldJointList = world.Scene.GetRecursiveComponents<Constraint2D>();
            foreach (var joint in worldJointList)
            {
                if (joint.TypeName == ConstraintGear2D.TypeNameStatic) continue;
                m_jointToIndexMap[joint] = index;
                jArray.Add(B2j(joint));
                index++;
            }
            foreach (var joint in worldJointList)
            {
                if (joint.TypeName != ConstraintGear2D.TypeNameStatic) continue;
                m_jointToIndexMap[joint] = index;
                jArray.Add(B2j(joint));
                index++;
            }
            worldValue["joint"] = jArray;

            // Images            
            jArray = new JArray();
            foreach (var image in m_imageToNameMap.Keys)
            {
                jArray.Add(B2j(image));
            }
            worldValue["image"] = jArray;

            // Custom properties
            JArray customPropertyValue = WriteCustomPropertiesToJson(null);
            if (customPropertyValue.Count > 0) worldValue["customProperties"] = customPropertyValue;

            m_bodyToIndexMap.Clear();
            m_jointToIndexMap.Clear();

            return worldValue;
        }

        public JObject B2j(RigidBody2D body)
        {
            JObject bodyValue = new JObject();

            string bodyName = GetBodyName(body);
            if (!string.IsNullOrWhiteSpace(bodyName)) bodyValue["name"] = bodyName;

            string bodyPath = GetBodyPath(body);
            if (!string.IsNullOrWhiteSpace(bodyPath)) bodyValue["path"] = bodyPath;

            bodyValue["type"] = (int)body.BodyType;

            VecToJson("position", body.Node.Position2D, bodyValue);
            FloatToJson("angle", body.Node.Rotation2D, bodyValue);

            VecToJson("linearVelocity", body.LinearVelocity, bodyValue);
            FloatToJson("angularVelocity", body.AngularVelocity, bodyValue);


            if (body.LinearDamping != 0) FloatToJson("linearDamping", body.LinearDamping, bodyValue);
            if (body.AngularDamping != 0) FloatToJson("angularDamping", body.AngularDamping, bodyValue);
            if (body.GravityScale != 1) FloatToJson("gravityScale", body.GravityScale, bodyValue);

            if (body.Bullet) bodyValue["bullet"] = true;
            if (!body.AllowSleep) bodyValue["allowSleep"] = false;
            if (body.Awake) bodyValue["awake"] = true;
            if (!body.Enabled) bodyValue["active"] = false;
            if (body.FixedRotation) bodyValue["fixedRotation"] = true;

            if (body.Mass != 0) FloatToJson("massData-mass", body.Mass, bodyValue);
            if (body.MassCenter.X != 0 || body.MassCenter.Y != 0) VecToJson("massData-center", body.MassCenter, bodyValue);
            if (body.Inertia != 0) FloatToJson("massData-I", body.Inertia, bodyValue);


            JArray jArray = new JArray();
            IEnumerable<CollisionShape2D> bodyFixturesList = body.Node.Components.OfType<CollisionShape2D>();
            foreach (var fixture in bodyFixturesList) jArray.Add(B2j(fixture));
            bodyValue["fixture"] = jArray;


            JArray customPropertyValue = WriteCustomPropertiesToJson(body);
            if (customPropertyValue.Count > 0) bodyValue["customProperties"] = customPropertyValue;

            return bodyValue;
        }

        public JObject B2j(CollisionShape2D fixture)
        {
            JObject fixtureValue = new JObject();

            string fixtureName = GetFixtureName(fixture);

            if (!string.IsNullOrWhiteSpace(fixtureName)) fixtureValue["name"] = fixtureName;

            string fixturePath = GetFixturePath(fixture);
            if (!string.IsNullOrWhiteSpace(fixturePath)) fixtureValue["path"] = fixturePath;

            if (fixture.Restitution != 0) FloatToJson("restitution", fixture.Restitution, fixtureValue);
            if (fixture.Friction != 0) FloatToJson("friction", fixture.Friction, fixtureValue);
            if (fixture.Density != 0) FloatToJson("density", fixture.Density, fixtureValue);
            if (fixture.Trigger) fixtureValue["sensor"] = true;

            if (fixture.CategoryBits != 0x0001) fixtureValue["filter-categoryBits"] = fixture.CategoryBits;
            if (fixture.MaskBits != 0xffff) fixtureValue["filter-maskBits"] = fixture.MaskBits;
            if (fixture.GroupIndex != 0) fixtureValue["filter-groupIndex"] = fixture.GroupIndex;


            JObject shapeValue = new JObject();
            switch (fixture)
            {
                case CollisionCircle2D circle:
                    FloatToJson("radius", circle.Radius, shapeValue);
                    VecToJson("center", circle.Center, shapeValue);
                    fixtureValue["circle"] = shapeValue;
                    break;

                case CollisionEdge2D edge:
                    VecToJson("vertex1", edge.Vertex1, shapeValue);
                    VecToJson("vertex2", edge.Vertex2, shapeValue);
                    // not exists smooth collision in urho2d
                    //if (edge.m_hasVertex0) fixtureValue["edge"]["hasVertex0"] = true;
                    //if (edge.m_hasVertex3) fixtureValue["edge"]["hasVertex3"] = true;
                    //if (edge.m_hasVertex0) vecToJson("vertex0", edge.m_vertex0, fixtureValue["edge"]);
                    //if (edge.m_hasVertex3) vecToJson("vertex3", edge.m_vertex3, fixtureValue["edge"]);
                    fixtureValue["edge"] = shapeValue;
                    break;

                case CollisionChain2D chain:

                    uint count = chain.VertexCount;

                    for (uint i = 0; i < count; ++i) VecToJson("vertices", chain.GetVertex(i), shapeValue, (int)i);
                    // Urho2d not has next/previous vertex, only has loop.
                    // this code is created reading 'b2ChainShape.cpp' from box2d
                    if (chain.Loop)
                    {
                        shapeValue["hasPrevVertex"] = true;
                        shapeValue["hasNextVertex"] = true;

                        VecToJson("prevVertex", chain.GetVertex(count - 2), shapeValue);
                        VecToJson("nextVertex", chain.GetVertex(1), shapeValue);
                    }
                    fixtureValue["chain"] = shapeValue;

                    break;

                case CollisionPolygon2D poly:

                    uint vertexCount = poly.VertexCount;

                    for (uint i = 0; i < vertexCount; ++i) VecToJson("vertices", poly.GetVertex(i), shapeValue, (int)i);
                    fixtureValue["polygon"] = shapeValue;

                    break;
                default:
                    System.Diagnostics.Trace.WriteLine("Unknown shape type : " + fixture.TypeName);
                    break;
            }

            JArray customPropertyValue = WriteCustomPropertiesToJson(fixture);
            if (customPropertyValue.Count > 0) fixtureValue["customProperties"] = customPropertyValue;

            return fixtureValue;
        }

        public JObject B2j(Constraint2D joint)
        {
            JObject jointValue = new JObject();

            string jointName = GetJointName(joint);
            if (jointName != "") jointValue["name"] = jointName;

            string jointPath = GetJointPath(joint);
            if (jointPath != "") jointValue["path"] = jointPath;


            RigidBody2D bodyA = joint.OwnerBody;
            RigidBody2D bodyB = joint.OtherBody;

            int bodyIndexA = LookupBodyIndex(bodyA);
            int bodyIndexB = LookupBodyIndex(bodyB);
            jointValue["bodyA"] = bodyIndexA;
            jointValue["bodyB"] = bodyIndexB;
            if (joint.CollideConnected) jointValue["collideConnected"] = true;

            switch (joint)
            {
                case ConstraintRevolute2D revoluteJoint:
                    jointValue["type"] = "revolute";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(revoluteJoint.Anchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(revoluteJoint.Anchor), jointValue);
                    FloatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                    // not exists in urho2d
                    // floatToJson("jointSpeed", revoluteJoint.GetJointSpeed(), jointValue);
                    jointValue["enableLimit"] = revoluteJoint.EnableLimit;
                    FloatToJson("lowerLimit", revoluteJoint.LowerAngle, jointValue);
                    FloatToJson("upperLimit", revoluteJoint.UpperAngle, jointValue);
                    jointValue["enableMotor"] = revoluteJoint.EnableMotor;
                    FloatToJson("motorSpeed", revoluteJoint.MotorSpeed, jointValue);
                    FloatToJson("maxMotorTorque", revoluteJoint.MaxMotorTorque, jointValue);
                    break;

                case ConstraintPrismatic2D prismaticJoint:
                    {
                        jointValue["type"] = "prismatic";

                        VecToJson("anchorA", bodyA.Node.WorldToLocal2D(prismaticJoint.Anchor), jointValue);
                        VecToJson("anchorB", bodyB.Node.WorldToLocal2D(prismaticJoint.Anchor), jointValue);
                        VecToJson("localAxisA", prismaticJoint.Axis, jointValue);
                        FloatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                        jointValue["enableLimit"] = prismaticJoint.EnableLimit;
                        FloatToJson("lowerLimit", prismaticJoint.LowerTranslation, jointValue);
                        FloatToJson("upperLimit", prismaticJoint.UpperTranslation, jointValue);
                        jointValue["enableMotor"] = prismaticJoint.EnableMotor;
                        FloatToJson("maxMotorForce", prismaticJoint.MaxMotorForce, jointValue);
                        FloatToJson("motorSpeed", prismaticJoint.MotorSpeed, jointValue);
                    }
                    break;

                case ConstraintDistance2D distanceJoint:
                    jointValue["type"] = "distance";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(distanceJoint.OwnerBodyAnchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(distanceJoint.OtherBodyAnchor), jointValue);
                    FloatToJson("length", distanceJoint.Length, jointValue);
                    FloatToJson("frequency", distanceJoint.FrequencyHz, jointValue);
                    FloatToJson("dampingRatio", distanceJoint.DampingRatio, jointValue);
                    break;

                case ConstraintPulley2D pulleyJoint:
                    jointValue["type"] = "pulley";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(pulleyJoint.OwnerBodyAnchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(pulleyJoint.OtherBodyAnchor), jointValue);
                    VecToJson("groundAnchorA", pulleyJoint.OwnerBodyGroundAnchor, jointValue);
                    VecToJson("groundAnchorB", pulleyJoint.OtherBodyGroundAnchor, jointValue);
                    FloatToJson("lengthA", (pulleyJoint.OwnerBodyGroundAnchor - pulleyJoint.OwnerBodyAnchor).Length, jointValue);
                    FloatToJson("lengthB", (pulleyJoint.OtherBodyGroundAnchor - pulleyJoint.OtherBodyAnchor).Length, jointValue);
                    FloatToJson("ratio", pulleyJoint.Ratio, jointValue);
                    break;

                case ConstraintMouse2D mouseJoint:
                    jointValue["type"] = "mouse";

                    VecToJson("target", mouseJoint.Target, jointValue);
                    // not exists in urho2d
                    // vecToJson("anchorB", mouseJoint.GetAnchorB(), jointValue);
                    FloatToJson("maxForce", mouseJoint.MaxForce, jointValue);
                    FloatToJson("frequency", mouseJoint.FrequencyHz, jointValue);
                    FloatToJson("dampingRatio", mouseJoint.DampingRatio, jointValue);
                    break;

                case ConstraintGear2D gearJoint:
                    jointValue["type"] = "gear";

                    int jointIndex1 = LookupJointIndex(gearJoint.OwnerConstraint);
                    int jointIndex2 = LookupJointIndex(gearJoint.OtherConstraint);
                    jointValue["joint1"] = jointIndex1;
                    jointValue["joint2"] = jointIndex2;
                    jointValue["ratio"] = gearJoint.Ratio;
                    break;

                case ConstraintWheel2D wheelJoint:

                    jointValue["type"] = "wheel";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(wheelJoint.Anchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(wheelJoint.Anchor), jointValue);
                    VecToJson("localAxisA", wheelJoint.Axis, jointValue);
                    jointValue["enableMotor"] = wheelJoint.EnableMotor;
                    FloatToJson("motorSpeed", wheelJoint.MotorSpeed, jointValue);
                    FloatToJson("maxMotorTorque", wheelJoint.MaxMotorTorque, jointValue);
                    FloatToJson("springFrequency", wheelJoint.FrequencyHz, jointValue);
                    FloatToJson("springDampingRatio", wheelJoint.DampingRatio, jointValue);

                    break;

                case ConstraintMotor2D motorJoint:

                    jointValue["type"] = "motor";

                    VecToJson("linearOffset", motorJoint.LinearOffset, jointValue);
                    VecToJson("anchorA", motorJoint.LinearOffset, jointValue);
                    FloatToJson("refAngle", motorJoint.AngularOffset, jointValue);
                    FloatToJson("maxForce", motorJoint.MaxForce, jointValue);
                    FloatToJson("maxTorque", motorJoint.MaxTorque, jointValue);
                    FloatToJson("correctionFactor", motorJoint.CorrectionFactor, jointValue);

                    break;

                case ConstraintWeld2D weldJoint:

                    jointValue["type"] = "weld";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(weldJoint.Anchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(weldJoint.Anchor), jointValue);

                    FloatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                    FloatToJson("frequency", weldJoint.FrequencyHz, jointValue);
                    FloatToJson("dampingRatio", weldJoint.DampingRatio, jointValue);

                    break;

                case ConstraintFriction2D frictionJoint:

                    jointValue["type"] = "friction";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(frictionJoint.Anchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(frictionJoint.Anchor), jointValue);
                    FloatToJson("maxForce", frictionJoint.MaxForce, jointValue);
                    FloatToJson("maxTorque", frictionJoint.MaxTorque, jointValue);

                    break;

                case ConstraintRope2D ropeJoint:
                    jointValue["type"] = "rope";

                    VecToJson("anchorA", bodyA.Node.WorldToLocal2D(ropeJoint.OwnerBodyAnchor), jointValue);
                    VecToJson("anchorB", bodyB.Node.WorldToLocal2D(ropeJoint.OtherBodyAnchor), jointValue);
                    FloatToJson("maxLength", ropeJoint.MaxLength, jointValue);

                    break;

                default:
                    System.Diagnostics.Trace.WriteLine("Unknown joint type not stored in snapshot : " + joint.TypeName);
                    break;
            }

            JArray customPropertyValue = WriteCustomPropertiesToJson(joint);
            if (customPropertyValue.Count > 0) jointValue["customProperties"] = customPropertyValue;

            return jointValue;
        }

        public JObject B2j(B2dJsonImage image)
        {
            JObject imageValue = new JObject();

            imageValue["body"] = null != image.Body ? LookupBodyIndex(image.Body) : -1;

            if (null != image.Name) imageValue["name"] = image.Name;
            if (image.Path != "") imageValue["path"] = image.Path;
            if (null != image.File) imageValue["file"] = image.File;

            VecToJson("center", image.Center, imageValue);
            FloatToJson("angle", image.Angle, imageValue);
            FloatToJson("scale", image.Scale, imageValue);
            FloatToJson("aspectScale", image.AspectScale, imageValue);
            if (image.Flip) imageValue["flip"] = true;
            FloatToJson("opacity", image.Opacity, imageValue);
            imageValue["filter"] = (int)image.Filter;
            FloatToJson("renderOrder", image.RenderOrder, imageValue);

            bool defaultColorTint = true;
            for (int i = 0; i < 4; i++)
            {
                if (image.ColorTint[i] != 255)
                {
                    defaultColorTint = false;
                    break;
                }
            }

            if (!defaultColorTint)
            {
                for (int i = 0; i < 4; i++) imageValue["colorTint"][i] = image.ColorTint[i];
            }

            // image->updateCorners();
            for (int i = 0; i < 4; i++) VecToJson("corners", image.Corners[i], imageValue, i);

            // image->updateUVs();
            for (int i = 0; i < 2 * image.NumPoints; i++)
            {
                VecToJson("glVertexPointer", image.Points[i], imageValue, i);
                VecToJson("glTexCoordPointer", image.UvCoords[i], imageValue, i);
            }
            for (int i = 0; i < image.NumIndices; i++)
                VecToJson("glDrawElements", (uint)image.Indices[i], imageValue, i);

            JArray customPropertyValue = WriteCustomPropertiesToJson(image);
            if (customPropertyValue.Count > 0) imageValue["customProperties"] = customPropertyValue;

            return imageValue;
        }

        #endregion [writing functions]



        #region [Setters]

        public void SetBodyName(RigidBody2D body, string name) { m_bodyToNameMap[body] = name; }
        public void SetFixtureName(CollisionShape2D fixture, string name) { m_fixtureToNameMap[fixture] = name; }
        public void SetJointName(Constraint2D joint, string name) { m_jointToNameMap[joint] = name; }
        public void SetImageName(B2dJsonImage image, string name) { m_imageToNameMap[image] = name; }

        public void SetBodyPath(RigidBody2D body, string path) { m_bodyToPathMap[body] = path; }
        public void SetFixturePath(CollisionShape2D fixture, string path) { m_fixtureToPathMap[fixture] = path; }
        public void SetJointPath(Constraint2D joint, string path) { m_jointToPathMap[joint] = path; }
        public void SetImagePath(B2dJsonImage image, string path) { m_imageToNameMap[image] = path; }

        public void AddImage(B2dJsonImage image) { SetImageName(image, image.Name); }

        #endregion [Setters]


        #region [reading functions]

        /// <summary>
        /// Read b2djson format from R.U.B.E editor into urho2d scene (only box2d physic world data)
        /// </summary>
        /// <param name="b2djsonWorld">physic world in b2djson format from R.U.B.E editor</param>
        /// <param name="urhoScene">Urho2d scene where will be loaded</param>
        /// <returns>true if exit, false otherwise</returns>
        /// <remarks>
        /// All components of the physical world will be created under a root node called from const 'URHO2D_PHYSIC_ROOT_NOE_NAME', since RUBE is agnostic to the system of components of urho.
        /// </remarks>
        public bool ReadIntoSceneFromValue(JObject b2djsonWorld, Scene urhoScene)
        {
            J2b2World(b2djsonWorld, urhoScene);
            return true;
        }

        /// <summary>
        /// Read b2djson format from R.U.B.E editor into urho2d scene (only box2d physic world data)
        /// </summary>
        /// <param name="str">string with physic world in b2djson format from R.U.B.E editor</param>
        /// <param name="urhoScene">Urho2d scene where will be loaded</param>
        /// <param name="errorMsg">error message</param>
        /// <returns>true if exit, false otherwise</returns>
        /// <remarks>
        /// All components of the physical world will be created under a root node called 'b2djson', since RUBE is agnostic to the system of components of urho.
        /// </remarks>
        public bool ReadIntoSceneFromString(string str, Scene urhoScene, out string errorMsg)
        {
            errorMsg = null;
            bool hasError;

            try
            {
                JObject worldValue = JObject.Parse(str);
                J2b2World(worldValue, urhoScene);
                hasError = false;
            }
            catch (IOException ex)
            {
                errorMsg = $"Failed to parse JSON: {ex.Message}";
                hasError = true;
            }

            return hasError;
        }

        /// <summary>
        /// Read b2djson format from R.U.B.E editor into urho2d scene (only box2d physic world data)
        /// </summary>
        /// <param name="filename">file with physic world in b2djson format from R.U.B.E editor</param>
        /// <param name="urhoScene">Urho2d scene where will be loaded</param>
        /// <param name="errorMsg">error message</param>
        /// <returns></returns>
        /// <remarks>
        /// All components of the physical world will be created under a root node called 'b2djson', since RUBE is agnostic to the system of components of urho.
        /// </remarks>
        public bool ReadIntoSceneFromFile(string filename, Scene ushoScene, out string errorMsg)
        {
            errorMsg = null;
            bool hasError;

            try
            {
                if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException("Param filename is null or empty");

                string str = System.IO.File.ReadAllText(filename);
                JObject worldValue = JObject.Parse(str);
                J2b2World(worldValue, ushoScene);
                hasError = false;
            }
            catch (IOException ex)
            {
                errorMsg = $"Error reading file: {filename}, {ex.Message}";
                hasError = true;
            }

            return hasError;
        }



        /// <summary>
        /// Create physic world in urho2d scene from b2dson format
        /// </summary>
        /// <param name="worldValue">world value in b2djson format</param>
        /// <param name="urhoScene">urho2d scene where will be loaded</param>
        /// <returns>root node for all physic world</returns>
        public Node J2b2World(JObject worldValue, Scene urhoScene)
        {
            if (null == urhoScene) throw new ArgumentNullException("ushoScene");

            m_bodies.Clear();

            PhysicsWorld2D world = urhoScene.GetOrCreateComponent<PhysicsWorld2D>();
            world.Gravity = JsonToVec("gravity", worldValue);

            world.AllowSleeping = (bool)worldValue["allowSleep"];
            world.AutoClearForces = (bool)worldValue["autoClearForces"];
            world.WarmStarting = (bool)worldValue["warmStarting"];
            world.ContinuousPhysics = (bool)worldValue["continuousPhysics"];
            world.SubStepping = (bool)worldValue["subStepping"];

            ReadCustomPropertiesFromJson(world, worldValue);

            // Create RootNode            
            Node physicRootNode = urhoScene.Children.FirstOrDefault(item => item.Name == URHO2D_PHYSIC_ROOT_NODE_NAME);
            if (null != physicRootNode) urhoScene.RemoveChild(physicRootNode);
            physicRootNode = urhoScene.CreateChild(URHO2D_PHYSIC_ROOT_NODE_NAME);


            //bool recreationMayDiffer = false; //hahaha
            //if ( ! world->GetAutoClearForces() ) { std::cout << "Note: world is not set to auto clear forces.\n"; recreationMayDiffer = true; }
            //if ( world->GetWarmStarting() ) { std::cout << "Note: world is set to use warm starting.\n"; recreationMayDiffer = true; }
            //if ( world->GetContinuousPhysics() ) { std::cout << "Note: world is set to use continuous physics.\n"; recreationMayDiffer = true; }
            //if ( world->GetSubStepping() ) { std::cout << "Note: world is set to use sub stepping.\n"; recreationMayDiffer = true; }
            //if ( worldValue["hasDestructionListener"].asBool() ) { std::cout << "Note: world originally had a destruction listener set.\n"; recreationMayDiffer = true; }
            //if ( worldValue["hasContactFilter"].asBool() ) { std::cout << "Note: world originally had a contact filter set.\n"; recreationMayDiffer = true; }
            //if ( worldValue["hasContactListener"].asBool() ) { std::cout << "Note: world originally had a contact listener set.\n"; recreationMayDiffer = true; }
            //if ( recreationMayDiffer )
            //    std::cout << "Recreated behaviour may differ from original.\n";


            JArray bodyValues = (JArray)worldValue["body"];
            if (null != bodyValues)
            {
                int numBodyValues = bodyValues.Count;
                for (int i = 0; i < numBodyValues; i++)
                {
                    JObject bodyValue = (JObject)bodyValues[i];
                    RigidBody2D body = J2b2Body(physicRootNode.CreateChild(mode: m_creationMode), bodyValue);
                    ReadCustomPropertiesFromJson(body, bodyValue);
                    m_bodies.Add(body);
                    m_indexToBodyMap.Add(i, body);
                }
            }

            // need two passes for joints because gear joints reference other joints
            JArray jointValues = (JArray)worldValue["joint"];
            if (null != jointValues)
            {
                int numJointValues = jointValues.Count;
                for (int i = 0; i < numJointValues; i++)
                {
                    JObject jointValue = (JObject)jointValues[i];
                    if (jointValue["type"].ToString() != "gear")
                    {
                        Constraint2D joint = J2b2Joint(jointValue);
                        ReadCustomPropertiesFromJson(joint, jointValue);
                        m_joints.Add(joint);
                    }
                }
                for (int i = 0; i < numJointValues; i++)
                {
                    JObject jointValue = (JObject)jointValues[i];
                    if (jointValue["type"].ToString() == "gear")
                    {
                        Constraint2D joint = J2b2Joint(jointValue);
                        ReadCustomPropertiesFromJson(joint, jointValue);
                        m_joints.Add(joint);
                    }
                }
            }

            JArray imageValues = (JArray)worldValue["image"];
            if (null != imageValues)
            {
                int numImageValues = imageValues.Count;
                for (int i = 0; i < numImageValues; i++)
                {
                    JObject imageValue = (JObject)imageValues[i];
                    B2dJsonImage image = J2b2dJsonImage(imageValue);
                    ReadCustomPropertiesFromJson(image, imageValue);
                    m_images.Add(image);
                }
            }

            return physicRootNode;
        }


        public RigidBody2D J2b2Body(Node bodyNode, JObject bodyValue)
        {

            RigidBody2D body = bodyNode.CreateComponent<RigidBody2D>(mode: m_creationMode);

            body.BodyType = (BodyType2D)(int.Parse(bodyValue["type"].ToString()));
            bodyNode.Position = new Vector3(JsonToVec("position", bodyValue));
            bodyNode.Rotation2D = JsonToFloat("angle", bodyValue);
            body.SetLinearVelocity(JsonToVec("linearVelocity", bodyValue));
            body.AngularVelocity = JsonToFloat("angularVelocity", bodyValue);
            body.LinearDamping = JsonToFloat("linearDamping", bodyValue, -1, 0);
            body.AngularDamping = JsonToFloat("angularDamping", bodyValue, -1, 0);
            body.GravityScale = JsonToFloat("gravityScale", bodyValue, -1, 1);

            body.AllowSleep = bodyValue["allowSleep"] == null ? true : (bool)bodyValue["allowSleep"];
            body.Awake = bodyValue["awake"] == null ? false : (bool)bodyValue["awake"];
            body.FixedRotation = bodyValue["fixedRotation"] == null ? false : (bool)bodyValue["fixedRotation"];
            body.Bullet = bodyValue["bullet"] == null ? false : (bool)bodyValue["bullet"];
            body.Enabled = bodyValue["active"] == null ? true : (bool)bodyValue["active"];


            string bodyName = bodyValue["name"]?.ToString();
            if (null != bodyName) SetBodyName(body, bodyName);

            string bodyPath = bodyValue["path"]?.ToString();
            if (null != bodyPath) SetBodyPath(body, bodyPath);

            int i = 0;
            JArray fixtureValues = (JArray)bodyValue["fixture"];
            if (null != fixtureValues)
            {
                int numFixtureValues = fixtureValues.Count;
                for (i = 0; i < numFixtureValues; i++)
                {
                    JObject fixtureValue = (JObject)fixtureValues[i];
                    CollisionShape2D fixture = J2b2Fixture(body, fixtureValue);
                    ReadCustomPropertiesFromJson(fixture, fixtureValue);
                }
            }

            // may be necessary if user has overridden mass characteristics
            body.Mass = JsonToFloat("massData-mass", bodyValue);
            body.SetMassCenter(JsonToVec("massData-center", bodyValue));
            body.Inertia = JsonToFloat("massData-I", bodyValue);

            return body;
        }

        public CollisionShape2D J2b2Fixture(RigidBody2D body, JObject fixtureValue)
        {
            CollisionShape2D fixture = null;
            if (null == fixtureValue) return fixture;

            var restitution = JsonToFloat("restitution", fixtureValue);
            var friction = JsonToFloat("friction", fixtureValue);
            var density = JsonToFloat("density", fixtureValue);
            var isSensor = fixtureValue["sensor"] == null ? false : (bool)fixtureValue["sensor"];

            var categoryBits = fixtureValue["filter-categoryBits"] == null ? 0x0001 : (int)fixtureValue["filter-categoryBits"];
            var maskBits = fixtureValue["filter-maskBits"] == null ? 0xffff : (int)fixtureValue["filter-maskBits"];
            var groupIndex = fixtureValue["filter-groupIndex"] == null ? (short)0 : (short)fixtureValue["filter-groupIndex"];


            if (null != fixtureValue["circle"])
            {
                CollisionCircle2D circleFixture = body.Node.CreateComponent<CollisionCircle2D>(mode: m_creationMode);
                JObject circleValue = (JObject)fixtureValue["circle"];
                circleFixture.Center = JsonToVec("center", circleValue);
                circleFixture.Radius = JsonToFloat("radius", circleValue);
                circleFixture.Density = density;
                fixture = circleFixture;
            }
            else if (null != fixtureValue["edge"])
            {
                CollisionEdge2D edgeFixture = body.Node.CreateComponent<CollisionEdge2D>(mode: m_creationMode);
                JObject edgeValue = (JObject)fixtureValue["edge"];
                edgeFixture.Vertex1 = JsonToVec("vertex1", edgeValue);
                edgeFixture.Vertex2 = JsonToVec("vertex2", edgeValue);
                // not exists smooth collision in urho2d
                // edgeShape.m_hasVertex0 = fixtureValue["edge"].get("hasVertex0", false).asBool();
                // edgeShape.m_hasVertex3 = fixtureValue["edge"].get("hasVertex3", false).asBool();
                // if (edgeShape.m_hasVertex0) edgeShape.m_vertex0 = jsonToVec("vertex0", fixtureValue["edge"]);
                // if (edgeShape.m_hasVertex3) edgeShape.m_vertex3 = jsonToVec("vertex3", fixtureValue["edge"]);
                fixture = edgeFixture;
            }
            else if (null != fixtureValue["loop"])
            {
                // support old format (r197)
                CollisionChain2D chainFixture = body.Node.CreateComponent<CollisionChain2D>(mode: m_creationMode);
                JObject chainValue = (JObject)fixtureValue["loop"];
                int numVertices = ((JArray)chainValue["x"]).Count;
                for (int i = 0; i < numVertices; i++) chainFixture.SetVertex((uint)i, JsonToVec("vertices", chainValue, i));
                chainFixture.Loop = true;
                fixture = chainFixture;
            }
            else if (null != fixtureValue["chain"])
            {
                CollisionChain2D chainFixture = body.Node.CreateComponent<CollisionChain2D>(mode: m_creationMode);
                JObject chainValue = (JObject)fixtureValue["chain"];
                int numVertices = ((JArray)chainValue["vertices"]["x"]).Count;
                List<Vector2> vertices = new List<Vector2>(numVertices);
                for (int i = 0; i < numVertices; i++) vertices.Add(JsonToVec("vertices", chainValue, i));

                // Urho2d not has next/previous vertex, only has loop.
                // this code is created reading 'b2ChainShape.cpp' from box2d
                var hasPrevVertex = chainValue["hasPrevVertex"] == null ? false : (bool)chainValue["hasPrevVertex"];
                var hasNextVertex = chainValue["hasNextVertex"] == null ? false : (bool)chainValue["hasNextVertex"];
                if (hasPrevVertex && hasNextVertex)
                {
                    chainFixture.Loop = true;
                }
                fixture = chainFixture;
            }
            else if (null != fixtureValue["polygon"])
            {
                JObject polygonValue = (JObject)fixtureValue["polygon"];

                int numVertices = ((JArray)polygonValue["vertices"]["x"]).Count;
                if (numVertices > b2_maxPolygonVertices)
                {
                    Console.WriteLine("Ignoring polygon fixture with too many vertices.");
                }
                else if (numVertices < 2)
                {
                    Console.WriteLine("Ignoring polygon fixture less than two vertices.");
                }
                else if (numVertices == 2)
                {
                    Console.WriteLine("Creating edge shape instead of polygon with two vertices.");
                    CollisionEdge2D poligonFixture = body.Node.CreateComponent<CollisionEdge2D>(mode: m_creationMode);
                    poligonFixture.Vertex1 = (JsonToVec("vertices", polygonValue, 0));
                    poligonFixture.Vertex2 = (JsonToVec("vertices", polygonValue, 1));
                    fixture = poligonFixture;
                }
                else
                {
                    CollisionPolygon2D poligonFixture = body.Node.CreateComponent<CollisionPolygon2D>(mode: m_creationMode);
                    for (int i = 0; i < numVertices; i++) poligonFixture.SetVertex((uint)i, JsonToVec("vertices", polygonValue, i));
                    fixture = poligonFixture;
                }
            }

            string fixtureName = fixtureValue["name"]?.ToString();
            if (null != fixtureName) SetFixtureName(fixture, fixtureName);
            string fixturePath = fixtureValue["path"]?.ToString();
            if (null != fixturePath) SetFixturePath(fixture, fixturePath);

            if (fixture != null)
            {
                fixture.Restitution = restitution;
                fixture.Friction = friction;
                fixture.Density = density;
                fixture.Trigger = isSensor;
                fixture.CategoryBits = categoryBits;
                fixture.MaskBits = maskBits;
                fixture.GroupIndex = groupIndex;
            }

            return fixture;
        }

        public Constraint2D J2b2Joint(JObject jointValue)
        {
            Constraint2D joint = null;

            int bodyIndexA = (int)jointValue["bodyA"];
            int bodyIndexB = (int)jointValue["bodyB"];
            if (bodyIndexA >= m_bodies.Count || bodyIndexB >= (int)m_bodies.Count) return null;

            // set features common to all joints
            var bodyA = m_bodies[bodyIndexA];
            var bodyB = m_bodies[bodyIndexB];
            var collideConnected = jointValue["collideConnected"] == null ? false : (bool)jointValue["collideConnected"];

            // keep these in scope after the if/else below
            ConstraintRevolute2D revoluteDef;
            ConstraintPrismatic2D prismaticDef;
            ConstraintDistance2D distanceDef;
            ConstraintPulley2D pulleyDef;
            ConstraintMouse2D mouseDef;
            ConstraintGear2D gearDef;
            ConstraintWheel2D wheelDef;
            ConstraintMotor2D motorDef;
            ConstraintWeld2D weldDef;
            ConstraintFriction2D frictionDef;
            ConstraintRope2D ropeDef;


            Vector2 mouseJointTarget = new Vector2(0, 0);
            string type = jointValue["type"]?.ToString();
            if (type == "revolute")
            {
                joint = revoluteDef = bodyA.Node.CreateComponent<ConstraintRevolute2D>(mode: m_creationMode);

                revoluteDef.Anchor = JsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB and reference angle
                // revoluteDef.localAnchorB = jsonToVec("anchorB", jointValue);
                // revoluteDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                revoluteDef.EnableLimit = jointValue["enableLimit"] == null ? false : (bool)jointValue["enableLimit"];
                revoluteDef.LowerAngle = JsonToFloat("lowerLimit", jointValue);
                revoluteDef.UpperAngle = JsonToFloat("upperLimit", jointValue);
                revoluteDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                revoluteDef.MotorSpeed = JsonToFloat("motorSpeed", jointValue);
                revoluteDef.MaxMotorTorque = JsonToFloat("maxMotorTorque", jointValue);
            }
            else if (type == "prismatic")
            {
                joint = prismaticDef = bodyA.Node.CreateComponent<ConstraintPrismatic2D>(mode: m_creationMode);

                prismaticDef.Anchor = JsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB and reference angle
                // prismaticDef.localAnchorB = jsonToVec("anchorB", jointValue);
                // prismaticDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                prismaticDef.Axis = jointValue["localAxisA"] != null ? JsonToVec("localAxisA", jointValue) : JsonToVec("localAxis1", jointValue);
                prismaticDef.EnableLimit = jointValue["enableLimit"] == null ? false : (bool)jointValue["enableLimit"];
                prismaticDef.LowerTranslation = JsonToFloat("lowerLimit", jointValue);
                prismaticDef.UpperTranslation = JsonToFloat("upperLimit", jointValue);
                prismaticDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                prismaticDef.MotorSpeed = JsonToFloat("motorSpeed", jointValue);
                prismaticDef.MaxMotorForce = JsonToFloat("maxMotorForce", jointValue);
            }
            else if (type == "distance")
            {
                joint = distanceDef = bodyA.Node.CreateComponent<ConstraintDistance2D>(mode: m_creationMode);

                distanceDef.OwnerBodyAnchor = JsonToVec("anchorA", jointValue);
                distanceDef.OtherBodyAnchor = JsonToVec("anchorB", jointValue);
                distanceDef.Length = JsonToFloat("length", jointValue);
                distanceDef.FrequencyHz = JsonToFloat("frequency", jointValue);
                distanceDef.DampingRatio = JsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "pulley")
            {
                joint = pulleyDef = bodyA.Node.CreateComponent<ConstraintPulley2D>(mode: m_creationMode);

                pulleyDef.OwnerBodyGroundAnchor = JsonToVec("groundAnchorA", jointValue);
                pulleyDef.OtherBodyGroundAnchor = JsonToVec("groundAnchorB", jointValue);
                pulleyDef.OwnerBodyAnchor = JsonToVec("anchorA", jointValue);
                pulleyDef.OtherBodyAnchor = JsonToVec("anchorB", jointValue);
                // urho2d not contains length (= OwnerBodyGroundAnchor - OtherBodyGroundAnchor)
                // pulleyDef.lengthA = jsonToFloat("lengthA", jointValue);
                // pulleyDef.lengthB = jsonToFloat("lengthB", jointValue);
                pulleyDef.Ratio = JsonToFloat("ratio", jointValue);
            }
            else if (type == "mouse")
            {
                joint = mouseDef = bodyA.Node.CreateComponent<ConstraintMouse2D>(mode: m_creationMode);

                mouseJointTarget = JsonToVec("target", jointValue);
                mouseDef.Target = JsonToVec("anchorB", jointValue); // alter after creating joint
                mouseDef.MaxForce = JsonToFloat("maxForce", jointValue);
                mouseDef.FrequencyHz = JsonToFloat("frequency", jointValue);
                mouseDef.DampingRatio = JsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "gear")
            {
                joint = gearDef = bodyA.Node.CreateComponent<ConstraintGear2D>(mode: m_creationMode);

                int jointIndex1 = (int)jointValue["joint1"];
                int jointIndex2 = (int)jointValue["joint2"];
                gearDef.OwnerConstraint = m_joints[jointIndex1];
                gearDef.OtherConstraint = m_joints[jointIndex2];
                gearDef.Ratio = JsonToFloat("ratio", jointValue);
            }
            else if (type == "wheel")
            {
                joint = wheelDef = bodyA.Node.CreateComponent<ConstraintWheel2D>(mode: m_creationMode);

                wheelDef.Anchor = JsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB
                // wheelDef.localAnchorB = jsonToVec("anchorB", jointValue);
                wheelDef.Axis = JsonToVec("localAxisA", jointValue);
                wheelDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                wheelDef.MotorSpeed = JsonToFloat("motorSpeed", jointValue);
                wheelDef.MaxMotorTorque = JsonToFloat("maxMotorTorque", jointValue);
                wheelDef.FrequencyHz = JsonToFloat("springFrequency", jointValue);
                wheelDef.DampingRatio = JsonToFloat("springDampingRatio", jointValue);
            }
            else if (type == "motor")
            {
                joint = motorDef = bodyA.Node.CreateComponent<ConstraintMotor2D>(mode: m_creationMode);

                // pre v1.7 editor exported anchorA as the linear offset
                motorDef.LinearOffset = null != jointValue["linearOffset"] ? JsonToVec("linearOffset", jointValue) : JsonToVec("anchorA", jointValue);
                motorDef.AngularOffset = JsonToFloat("refAngle", jointValue);
                motorDef.MaxForce = JsonToFloat("maxForce", jointValue);
                motorDef.MaxTorque = JsonToFloat("maxTorque", jointValue);
                motorDef.CorrectionFactor = JsonToFloat("correctionFactor", jointValue);
            }
            else if (type == "weld")
            {
                joint = weldDef = bodyA.Node.CreateComponent<ConstraintWeld2D>(mode: m_creationMode);

                weldDef.Anchor = JsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB and refAngle
                // weldDef.localAnchorB = jsonToVec("anchorB", jointValue);
                // weldDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                weldDef.FrequencyHz = JsonToFloat("frequency", jointValue);
                weldDef.DampingRatio = JsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "friction")
            {
                joint = frictionDef = bodyA.Node.CreateComponent<ConstraintFriction2D>(mode: m_creationMode);

                frictionDef.Anchor = JsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB
                // frictionDef.localAnchorB = jsonToVec("anchorB", jointValue);
                frictionDef.MaxForce = JsonToFloat("maxForce", jointValue);
                frictionDef.MaxTorque = JsonToFloat("maxTorque", jointValue);
            }
            else if (type == "rope")
            {
                joint = ropeDef = bodyA.Node.CreateComponent<ConstraintRope2D>(mode: m_creationMode);

                ropeDef.OwnerBodyAnchor = JsonToVec("anchorA", jointValue);
                ropeDef.OtherBodyAnchor = JsonToVec("anchorB", jointValue);
                ropeDef.MaxLength = JsonToFloat("maxLength", jointValue);
            }

            if (null != joint)
            {
                // set features common to all joints                
                joint.OtherBody = m_bodies[bodyIndexB];
                joint.CollideConnected = collideConnected;

                if (type == "mouse") ((ConstraintMouse2D)joint).Target = mouseJointTarget;

                string jointName = jointValue["name"]?.ToString();
                if (null != jointName) SetJointName(joint, jointName);

                string jointPath = jointValue["path"]?.ToString();
                if (null != jointPath) SetJointPath(joint, jointPath);
            }

            return joint;
        }

        public B2dJsonImage J2b2dJsonImage(JObject imageValue)
        {
            B2dJsonImage img = new B2dJsonImage();

            int bodyIndex = imageValue["body"] == null ? -1 : (int)imageValue["body"];
            if (-1 != bodyIndex) img.Body = LookupBodyFromIndex(bodyIndex);

            string imageName = imageValue["name"]?.ToString();
            if (null != imageName)
            {
                img.Name = imageName;
                SetImageName(img, imageName);
            }

            string imagePath = imageValue["path"]?.ToString();
            if (null != imagePath)
            {
                img.Path = imagePath;
                SetImagePath(img, imagePath);
            }

            string fileName = imageValue["file"]?.ToString();
            if (null != fileName) img.File = fileName;


            img.Center = JsonToVec("center", imageValue);
            img.Angle = JsonToFloat("angle", imageValue);
            img.Scale = JsonToFloat("scale", imageValue);
            img.AspectScale = JsonToFloat("aspectScale", imageValue, -1, 1);
            img.Opacity = JsonToFloat("opacity", imageValue);
            img.RenderOrder = JsonToFloat("renderOrder", imageValue);

            JArray colorTintArray = (JArray)imageValue["colorTint"];
            if (null != colorTintArray)
            {
                for (int i = 0; i < 4; i++) if (null != colorTintArray[i]) img.ColorTint[i] = (int)colorTintArray[i];
            }

            if (null != imageValue["flip"]) img.Flip = (bool)imageValue["flip"];
            if (null != imageValue["filter"]) img.Filter = (B2dJsonImagefilterType)(int)imageValue["filter"];

            for (int i = 0; i < 4; i++) img.Corners[i] = JsonToVec("corners", imageValue, i);


            JArray vertexPointerArray = (JArray)imageValue["glVertexPointer"];
            JArray texCoordArray = (JArray)imageValue["glTexCoordPointer"];
            if (null != vertexPointerArray && null != texCoordArray && vertexPointerArray.Count == texCoordArray.Count)
            {
                int numFloats = vertexPointerArray.Count;
                img.NumPoints = numFloats / 2;
                img.Points = new float[numFloats];
                img.UvCoords = new float[numFloats];
                for (int i = 0; i < numFloats; i++)
                {
                    img.Points[i] = JsonToFloat("glVertexPointer", imageValue, i);
                    img.UvCoords[i] = JsonToFloat("glTexCoordPointer", imageValue, i);
                }
            }


            JArray drawElementsArray = (JArray)imageValue["glDrawElements"];
            if (null != drawElementsArray)
            {
                img.NumIndices = drawElementsArray.Count;
                img.Indices = new ushort[img.NumIndices];
                for (int i = 0; i < img.NumIndices; i++) img.Indices[i] = (ushort)drawElementsArray[i];
            }

            return img;
        }

        #endregion [reading functions]



        #region [Getters]

        public IEnumerable<RigidBody2D> GetBodiesByName(string name) { return m_bodyToNameMap.Where(item => item.Value == name).Select(item => item.Key); }
        public IEnumerable<CollisionShape2D> GetFixturesByName(string name) { return m_fixtureToNameMap.Where(item => item.Value == name).Select(item => item.Key); }
        public IEnumerable<Constraint2D> GetJointsByName(string name) { return m_jointToNameMap.Where(item => item.Value == name).Select(item => item.Key); }
        public IEnumerable<B2dJsonImage> GetImagesByName(string name) { return m_imageToNameMap.Where(item => item.Value == name).Select(item => item.Key); }


        public IEnumerable<RigidBody2D> GetBodiesByPath(string path) { return m_bodyToPathMap.Where(item => item.Value == path).Select(item => item.Key); }
        public IEnumerable<CollisionShape2D> GetFixturesByPath(string path) { return m_fixtureToPathMap.Where(item => item.Value == path).Select(item => item.Key); }
        public IEnumerable<Constraint2D> GetJointsByPath(string path) { return m_jointToPathMap.Where(item => item.Value == path).Select(item => item.Key); }
        public IEnumerable<B2dJsonImage> GetImagesByPath(string path) { return m_imageToPathMap.Where(item => item.Value == path).Select(item => item.Key); }


        public IEnumerable<RigidBody2D> GetAllBodies() { return m_bodies.AsEnumerable(); }
        public IEnumerable<CollisionShape2D> GetAllFixtures() { return m_bodies.SelectMany(item => item.Node.Components.OfType<CollisionShape2D>()); }
        public IEnumerable<Constraint2D> GetAllJoints() { return m_joints.AsEnumerable(); }
        public IEnumerable<B2dJsonImage> GetAllImages() { return m_images.AsEnumerable(); }

        public RigidBody2D GetBodyByName(string name) { return m_bodyToNameMap.FirstOrDefault(item => item.Value == name).Key; }
        public CollisionShape2D GetFixtureByName(string name) { return m_fixtureToNameMap.FirstOrDefault(item => item.Value == name).Key; }
        public Constraint2D GetJointByName(string name) { return m_jointToNameMap.FirstOrDefault(item => item.Value == name).Key; }
        public B2dJsonImage GetImageByName(string name) { return m_imageToNameMap.FirstOrDefault(item => item.Value == name).Key; }

        public RigidBody2D GetBodyByPathAndName(string path, string name) { return m_bodyToNameMap.FirstOrDefault(item => name == item.Value && name == GetBodyPath(item.Key)).Key; }
        public CollisionShape2D GetFixtureByPathAndName(string path, string name) { return m_fixtureToNameMap.FirstOrDefault(item => name == item.Value && name == GetFixturePath(item.Key)).Key; }
        public Constraint2D GetJointByPathAndName(string path, string name) { return m_jointToNameMap.FirstOrDefault(item => name == item.Value && name == GetJointPath(item.Key)).Key; }
        public B2dJsonImage GetImageByPathAndName(string path, string name) { return m_imageToNameMap.FirstOrDefault(item => name == item.Value && name == GetImagePath(item.Key)).Key; }

        public Dictionary<Constraint2D, string> GetJointToNameMap() { return m_jointToNameMap; }
        public Dictionary<CollisionShape2D, string> GetFixtureToNameMap() { return m_fixtureToNameMap; }

        public string GetBodyName(RigidBody2D body) { return m_bodyToNameMap.FirstOrDefault(item => item.Key == body).Value; }
        public string GetFixtureName(CollisionShape2D fixture) { return m_fixtureToNameMap.FirstOrDefault(item => item.Key == fixture).Value; }
        public string GetJointName(Constraint2D joint) { return m_jointToNameMap.FirstOrDefault(item => item.Key == joint).Value; }
        public string GetImageName(B2dJsonImage img) { return m_imageToNameMap.FirstOrDefault(item => item.Key == img).Value; }

        public string GetBodyPath(RigidBody2D body) { return m_bodyToPathMap.FirstOrDefault(item => item.Key == body).Value; }
        public string GetFixturePath(CollisionShape2D fixture) { return m_fixtureToPathMap.FirstOrDefault(item => item.Key == fixture).Value; }
        public string GetJointPath(Constraint2D joint) { return m_jointToPathMap.FirstOrDefault(item => item.Key == joint).Value; }
        public string GetImagePath(B2dJsonImage img) { return m_imageToPathMap.FirstOrDefault(item => item.Key == img).Value; }


        #endregion [Getters]




        #region [custom properties]

        public B2dJsonCustomProperties GetCustomPropertiesForItem(object item, bool createIfNotExisting)
        {
            if (m_customPropertiesMap.ContainsKey(item)) return m_customPropertiesMap[item];
            if (!createIfNotExisting) return null;

            B2dJsonCustomProperties props = new B2dJsonCustomProperties();
            m_customPropertiesMap[item] = props;

            return props;
        }


        public void SetCustomInt(object item, string propertyName, int val) { SetCustomValueType<int>(item, propertyName, val); }
        public void SetCustomFloat(object item, string propertyName, float val) { SetCustomValueType<float>(item, propertyName, val); }
        public void SetCustomString(object item, string propertyName, string val) { SetCustomValueType<string>(item, propertyName, val); }
        public void SetCustomVector(object item, string propertyName, Vector2 val) { SetCustomValueType<Vector2>(item, propertyName, val); }
        public void SetCustomBool(object item, string propertyName, bool val) { SetCustomValueType<bool>(item, propertyName, val); }
        public void SetCustomColor(object item, string propertyName, B2dJsonColor4 val) { SetCustomValueType<B2dJsonColor4>(item, propertyName, val); }


        protected void SetCustomValueType<T>(object item, string propertyName, T val)
        {
            switch (item)
            {
                case RigidBody2D body:
                    m_bodiesWithCustomProperties.Add(body);
                    break;
                case CollisionShape2D fixture:
                    m_fixturesWithCustomProperties.Add(fixture);
                    break;
                case Constraint2D joint:
                    m_jointsWithCustomProperties.Add(joint);
                    break;
                case B2dJsonImage image:
                    m_imagesWithCustomProperties.Add(image);
                    break;
                case PhysicsWorld2D world:
                    m_worldsWithCustomProperties.Add(world);
                    break;
                default:
                    break;
            }

            B2dJsonCustomProperties properties = GetCustomPropertiesForItem(item, true);
            properties.GetCustomPropertyMapFromType<T>()[propertyName] = val;
        }



        public bool HasCustomInt(object item, string propertyName) { return HasCustomValueType<int>(item, propertyName); }
        public bool HasCustomFloat(object item, string propertyName) { return HasCustomValueType<float>(item, propertyName); }
        public bool HasCustomString(object item, string propertyName) { return HasCustomValueType<string>(item, propertyName); }
        public bool HasCustomVector(object item, string propertyName) { return HasCustomValueType<Vector2>(item, propertyName); }
        public bool HasCustomBool(object item, string propertyName) { return HasCustomValueType<bool>(item, propertyName); }
        public bool HasCustomColor(object item, string propertyName) { return HasCustomValueType<B2dJsonColor4>(item, propertyName); }

        protected bool HasCustomValueType<T>(object item, string propertyName)
        {
            B2dJsonCustomProperties properties = GetCustomPropertiesForItem(item, false);
            return properties != null && properties.GetCustomPropertyMapFromType<T>().Count(property => property.Key == propertyName) > 0;
        }


        public int GetCustomInt(object item, string propertyName, int defaultVal = 0) { return GetCustomValueType(item, propertyName, defaultVal); }
        public float GetCustomFloat(object item, string propertyName, float defaultVal = 0) { return GetCustomValueType(item, propertyName, defaultVal); }
        public string GetCustomString(object item, string propertyName, string defaultVal = "") { return GetCustomValueType(item, propertyName, defaultVal); }
        public Vector2 GetCustomVector(object item, string propertyName, Vector2 defaultVal = default(Vector2)) { return GetCustomValueType(item, propertyName, defaultVal); }
        public bool GetCustomBool(object item, string propertyName, bool defaultVal = false) { return GetCustomValueType(item, propertyName, defaultVal); }
        public B2dJsonColor4 GetCustomColor(object item, string propertyName, B2dJsonColor4 defaultVal = default(B2dJsonColor4)) { return GetCustomValueType(item, propertyName, defaultVal); }

        protected T GetCustomValueType<T>(object item, string propertyName, T defaultVal = default(T))
        {
            B2dJsonCustomProperties props = GetCustomPropertiesForItem(item, false);
            if (null == props) return defaultVal;

            Dictionary<string, T> propertiesMap = props.GetCustomPropertyMapFromType<T>();

            return propertiesMap.Any(property => property.Key == propertyName) ? propertiesMap.First(property => property.Key == propertyName).Value : defaultVal;
        }




        public IEnumerable<RigidBody2D> GetBodiesByCustomValueType<T>(string propertyName, T valueToMatch)
        {
            return m_bodiesWithCustomProperties.Where(item => HasCustomValueType<T>(item, propertyName) &&  GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public IEnumerable<CollisionShape2D> GetFixturesByCustomValueType<T>(string propertyName, T valueToMatch)
        {
            return m_fixturesWithCustomProperties.Where(item => HasCustomValueType<T>(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public IEnumerable<Constraint2D> GetJointsByCustomValueType<T>(string propertyName, T valueToMatch)
        {
            return m_jointsWithCustomProperties.Where(item => HasCustomValueType<T>(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public IEnumerable<B2dJsonImage> GetImagesByCustomValueType<T>(string propertyName, T valueToMatch)
        {
            return m_imagesWithCustomProperties.Where(item => HasCustomValueType<T>(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }


        public RigidBody2D GetBodyByCustomType<T>(string propertyName, T valueToMatch)
        {
            return m_bodiesWithCustomProperties.FirstOrDefault(item => HasCustomInt(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public CollisionShape2D GetFixtureByCustomType<T>(string propertyName, T valueToMatch)
        {
            return m_fixturesWithCustomProperties.FirstOrDefault(item => HasCustomInt(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public Constraint2D GetJointByCustomType<T>(string propertyName, T valueToMatch)
        {
            return m_jointsWithCustomProperties.FirstOrDefault(item => HasCustomInt(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }
        public B2dJsonImage GetImageByCustomType<T>(string propertyName, T valueToMatch)
        {
            return m_imagesWithCustomProperties.FirstOrDefault(item => HasCustomInt(item, propertyName) && GetCustomValueType<T>(item, propertyName).Equals(valueToMatch));
        }



        public SortedSet<T> GetSetWithCustomPropertiesFromType<T>()
        {
            switch (typeof(T))
            {
                case Type bodyType when bodyType == typeof(RigidBody2D): return m_bodiesWithCustomProperties as SortedSet<T>;
                case Type fixtureType when fixtureType == typeof(CollisionShape2D): return m_fixturesWithCustomProperties as SortedSet<T>;
                case Type jointType when jointType == typeof(Constraint2D): return m_jointsWithCustomProperties as SortedSet<T>;
                case Type imageType when imageType == typeof(B2dJsonImage): return m_imagesWithCustomProperties as SortedSet<T>;
                case Type worldType when worldType == typeof(PhysicsWorld2D): return m_worldsWithCustomProperties as SortedSet<T>;
            }

            return new SortedSet<T>();
        }

        #endregion [custom properties]



        #region [member helpers]

        protected void VecToJson(string name, uint v, JObject value, int index = -1)
        {
            if (index > -1)
                value[name][index] = v;
            else
                value[name] = v;
        }
        protected void VecToJson(string name, float v, JObject value, int index = -1)
        {
            if (index > -1)
            {
                if (m_useHumanReadableFloats)
                {
                    value[name][index] = v;
                }
                else
                {
                    if (v == 0)
                        value[name][index] = 0;
                    else if (v == 1)
                        value[name][index] = 1;
                    else
                        value[name][index] = FloatToHex(v);
                }
            }
            else
                FloatToJson(name, v, value);
        }
        protected void VecToJson(string name, Vector2 vec, JObject value, int index = -1)
        {
            if (index > -1)
            {
                if (m_useHumanReadableFloats)
                {
                    value[name]["x"][index] = vec.X;
                    value[name]["y"][index] = vec.Y;
                }
                else
                {
                    if (vec.X == 0)
                        value[name]["x"][index] = 0;
                    else if (vec.X == 1)
                        value[name]["x"][index] = 1;
                    else
                        value[name]["x"][index] = FloatToHex(vec.X);
                    if (vec.Y == 0)
                        value[name]["y"][index] = 0;
                    else if (vec.Y == 1)
                        value[name]["y"][index] = 1;
                    else
                        value[name]["y"][index] = FloatToHex(vec.Y);
                }
            }
            else
            {
                if (vec.X == 0 && vec.Y == 0)
                    value[name] = 0; // cut down on file space for common values
                else
                {
                    JObject vecValue = new JObject();
                    FloatToJson("x", vec.X, vecValue);
                    FloatToJson("y", vec.Y, vecValue);
                    value[name] = vecValue;
                }
            }
        }

        protected void FloatToJson(string name, float f, JObject value)
        {
            // cut down on file space for common values
            if (f == 0) value[name] = 0;
            else if (f == 1) value[name] = 1;
            else
            {
                if (m_useHumanReadableFloats)
                    value[name] = f;
                else
                    value[name] = FloatToHex(f);
            }
        }

        protected RigidBody2D LookupBodyFromIndex(int index) { return m_indexToBodyMap.ContainsKey(index) ? m_indexToBodyMap[index] : null; }

        protected int LookupBodyIndex(RigidBody2D body)
        {
            int? val = m_bodyToIndexMap[body];
            return null != val ? val.Value : -1;
        }

        protected int LookupJointIndex(Constraint2D joint)
        {
            int? val = m_jointToIndexMap[joint];
            return null != val ? val.Value : -1;
        }

        protected JArray WriteCustomPropertiesToJson(object item)
        {
            JArray customPropertiesValue = new JArray();
            if (null == item) return customPropertiesValue;

            B2dJsonCustomProperties props = GetCustomPropertiesForItem(item, false);
            if (null == props) return customPropertiesValue;


            foreach (var customProp in props.m_customPropertyMap_int)
            {
                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key,
                    ["int"] = customProp.Value
                };
                customPropertiesValue.Add(proValue);
            }

            foreach (var customProp in props.m_customPropertyMap_string)
            {
                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key,
                    ["string"] = customProp.Value
                };
                customPropertiesValue.Add(proValue);
            }

            foreach (var customProp in props.m_customPropertyMap_bool)
            {
                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key,
                    ["bool"] = customProp.Value
                };
                customPropertiesValue.Add(proValue);
            }

            foreach (var customProp in props.m_customPropertyMap_float)
            {
                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key,
                    ["float"] = customProp.Value
                };
                customPropertiesValue.Add(proValue);
            }

            foreach (var customProp in props.m_customPropertyMap_b2Vec2)
            {
                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key
                };
                VecToJson("vec2", customProp.Value, proValue);
                customPropertiesValue.Add(proValue);
            }

            foreach (var customProp in props.m_customPropertyMap_color)
            {
                JArray jColorArray = new JArray();
                jColorArray.Add(customProp.Value.R);
                jColorArray.Add(customProp.Value.G);
                jColorArray.Add(customProp.Value.B);
                jColorArray.Add(customProp.Value.A);

                JObject proValue = new JObject
                {
                    ["name"] = customProp.Key,
                    ["color"] = jColorArray
                };

                customPropertiesValue.Add(proValue);
            }

            return customPropertiesValue;
        }

        protected void ReadCustomPropertiesFromJson(RigidBody2D item, JObject value) { ReadCustomPropertiesFromJson<RigidBody2D>(item, value); }
        protected void ReadCustomPropertiesFromJson(CollisionShape2D item, JObject value) { ReadCustomPropertiesFromJson<CollisionShape2D>(item, value); }
        protected void ReadCustomPropertiesFromJson(Constraint2D item, JObject value) { ReadCustomPropertiesFromJson<Constraint2D>(item, value); }
        protected void ReadCustomPropertiesFromJson(B2dJsonImage item, JObject value) { ReadCustomPropertiesFromJson<B2dJsonImage>(item, value); }
        protected void ReadCustomPropertiesFromJson(PhysicsWorld2D item, JObject value) { ReadCustomPropertiesFromJson<PhysicsWorld2D>(item, value); }

        protected void ReadCustomPropertiesFromJson<T>(T item, JObject value)
        {
            JArray propValues = (JArray)value["customProperties"];
            if (null == item || null == propValues) return;            
            
            for (int i = 0; i < propValues.Count; i++)
            {
                JObject propValue = (JObject)propValues[i];
                string propertyName = propValue["name"].ToString();
                if (propValue["int"] != null) SetCustomInt(item, propertyName, (int)propValue["int"]);
                if (propValue["float"] != null) SetCustomFloat(item, propertyName, (float)propValue["float"]);
                if (propValue["string"] != null) SetCustomString(item, propertyName, propValue["string"].ToString());
                if (propValue["vec2"] != null) SetCustomVector(item, propertyName, JsonToVec("vec2", propValue));
                if (propValue["bool"] != null) SetCustomBool(item, propertyName, (bool)propValue["bool"]);
                if (propValue["color"] != null)
                {
                    JArray colorJArray = (JArray)propValue["color"];
                    if (null != colorJArray && colorJArray.Count > 3)
                    {
                        B2dJsonColor4 color4 = new B2dJsonColor4
                        {
                            R = (int)colorJArray[0],
                            G = (int)colorJArray[1],
                            B = (int)colorJArray[2],
                            A = (int)colorJArray[3],
                        };
                        
                        SetCustomColor(item, propertyName, color4);
                    }
                }
            }            
        }

        #endregion [member helpers]



        #region [static helpers]

        public static string FloatToHex(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            int i = BitConverter.ToInt32(bytes, 0);
            return i.ToString("X");
        }

        public static float HexToFloat(string str)
        {            
            uint num = uint.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);

            byte[] floatVals = BitConverter.GetBytes(num);
            float f = BitConverter.ToSingle(floatVals, 0);
            return f;
        }

        public static float JsonToFloat(string name, JObject value, int index = -1, float defaultValue = 0)
        {
            if (null == value[name]) return defaultValue;

            if (index > -1)
            {
                if (null == value[name][index]) return defaultValue;
                else if (value[name][index].GetType() == typeof(int)) return (int)value[name][index]; // usually 0 or 1
                else if (value[name][index].GetType() == typeof(string)) return HexToFloat((string)value[name][index]);
                else return (float)value[name][index];
            }
            else
            {
                if (null == value[name]) return defaultValue;
                else if (value[name].GetType() == typeof(int)) return (int)value[name]; // usually 0 or 1
                else if (value[name].GetType() == typeof(string)) return HexToFloat((string)value[name]);
                else return (float)value[name];
            }
        }

        public static Vector2 JsonToVec(string name, JObject value, int index = -1, Vector2 defaultValue = default(Vector2))
        {
            Vector2 vec = null == defaultValue ? Vector2.Zero : defaultValue;

            if (null == value[name]) return defaultValue;

            if (index > -1)
            {

                if (value[name]["x"][index].GetType() == typeof(int)) vec.X = (int)value[name]["x"][index]; //usually 0 or 1
                else if (value[name]["x"][index].GetType() == typeof(string)) vec.X = HexToFloat((string)value[name]["x"][index]);
                else vec.X = (float)value[name]["x"][index];

                if (value[name]["y"][index].GetType() == typeof(int)) vec.Y = (int)value[name]["y"][index]; //usually 0 or 1
                else if (value[name]["y"][index].GetType() == typeof(string)) vec.Y = HexToFloat((string)value[name]["y"][index]);
                else vec.Y = (float)value[name]["y"][index];
            }
            else
            {
                if (value[name].GetType() == typeof(int)) vec = Vector2.Zero; // zero vector
                else
                {
                    vec.X = JsonToFloat("x", (JObject)value[name]);
                    vec.Y = JsonToFloat("y", (JObject)value[name]);
                }
            }

            return vec;
        }

        #endregion [static helpers]

    }

}



