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
        B2dJsonColor4() { R = G = B = A = 255; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    };

    public class B2dJsonCustomProperties
    {
        public Dictionary<string, int> m_customPropertyMap_int { get; set; }
        public Dictionary<string, float> m_customPropertyMap_float { get; set; }
        public Dictionary<string, string> m_customPropertyMap_string { get; set; }
        public Dictionary<string, Vector2> m_customPropertyMap_b2Vec2 { get; set; }
        public Dictionary<string, bool> m_customPropertyMap_bool { get; set; }
        public Dictionary<string, B2dJsonColor4> m_customPropertyMap_color { get; set; }
    };

    public class b2dJson
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
        public b2dJson(CreateMode creationMode = CreateMode.Local, bool useHumanReadableFloats = false)
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


        public void clear() { }


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
        public JObject writeToValue(Scene urhoScene)
        {
            PhysicsWorld2D world;
            if (null == urhoScene || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return new JObject();            

            return b2j(world);
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
        public string writeToString(Scene urhoScene)
        {
            PhysicsWorld2D world;
            if (null == urhoScene || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return string.Empty;            

            return b2j(world).ToString();
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
        public bool writeToFile(Scene urhoScene, string filename, out string errorMsg)
        {
            errorMsg = string.Empty;
            PhysicsWorld2D world;
            if (null == urhoScene || string.IsNullOrWhiteSpace(filename) || null == (world = urhoScene.GetComponent<PhysicsWorld2D>())) return false;
            
            using (TextWriter writeFile = new StreamWriter(filename))
            {
                try
                {
                    writeFile.WriteLine(b2j(world).ToString());
                }
                catch (Exception e)
                {
                    errorMsg = $"Error writing JSON to file: {filename} {e.Message}";
                    return false;
                }
            }

            return true;
        }


        public JObject b2j(PhysicsWorld2D world)
        {
            JObject worldValue = new JObject();

            m_bodyToIndexMap.Clear();
            m_jointToIndexMap.Clear();

            vecToJson("gravity", world.Gravity, worldValue);
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
                jArray.Add(b2j(item));
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
                jArray.Add(b2j(joint));
                index++;
            }
            foreach (var joint in worldJointList)
            {
                if (joint.TypeName != ConstraintGear2D.TypeNameStatic) continue;
                m_jointToIndexMap[joint] = index;
                jArray.Add(b2j(joint));
                index++;
            }
            worldValue["joint"] = jArray;

            // Images            
            jArray = new JArray();
            foreach (var image in m_imageToNameMap.Keys)
            {
                jArray.Add(b2j(image));
            }
            worldValue["image"] = jArray;

            // Custom properties
            JArray customPropertyValue = writeCustomPropertiesToJson(null);
            if (customPropertyValue.Count > 0) worldValue["customProperties"] = customPropertyValue;

            m_bodyToIndexMap.Clear();
            m_jointToIndexMap.Clear();

            return worldValue;
        }

        public JObject b2j(RigidBody2D body)
        {
            JObject bodyValue = new JObject();

            string bodyName = getBodyName(body);
            if (!string.IsNullOrWhiteSpace(bodyName)) bodyValue["name"] = bodyName;

            string bodyPath = getBodyPath(body);
            if (!string.IsNullOrWhiteSpace(bodyPath)) bodyValue["path"] = bodyPath;

            bodyValue["type"] = (int)body.BodyType;

            vecToJson("position", body.Node.Position2D, bodyValue);
            floatToJson("angle", body.Node.Rotation2D, bodyValue);

            vecToJson("linearVelocity", body.LinearVelocity, bodyValue);
            floatToJson("angularVelocity", body.AngularVelocity, bodyValue);


            if (body.LinearDamping != 0) floatToJson("linearDamping", body.LinearDamping, bodyValue);
            if (body.AngularDamping != 0) floatToJson("angularDamping", body.AngularDamping, bodyValue);
            if (body.GravityScale != 1) floatToJson("gravityScale", body.GravityScale, bodyValue);

            if (body.Bullet) bodyValue["bullet"] = true;
            if (!body.AllowSleep) bodyValue["allowSleep"] = false;
            if (body.Awake) bodyValue["awake"] = true;
            if (!body.Enabled) bodyValue["active"] = false;
            if (body.FixedRotation) bodyValue["fixedRotation"] = true;

            if (body.Mass != 0) floatToJson("massData-mass", body.Mass, bodyValue);
            if (body.MassCenter.X != 0 || body.MassCenter.Y != 0) vecToJson("massData-center", body.MassCenter, bodyValue);
            if (body.Inertia != 0) floatToJson("massData-I", body.Inertia, bodyValue);


            JArray jArray = new JArray();
            IEnumerable<CollisionShape2D> bodyFixturesList = body.Node.Components.OfType<CollisionShape2D>();
            foreach (var fixture in bodyFixturesList) jArray.Add(b2j(fixture));
            bodyValue["fixture"] = jArray;


            JArray customPropertyValue = writeCustomPropertiesToJson(body);
            if (customPropertyValue.Count > 0) bodyValue["customProperties"] = customPropertyValue;

            return bodyValue;
        }

        public JObject b2j(CollisionShape2D fixture)
        {
            JObject fixtureValue = new JObject();

            string fixtureName = getFixtureName(fixture);

            if (!string.IsNullOrWhiteSpace(fixtureName)) fixtureValue["name"] = fixtureName;

            string fixturePath = getFixturePath(fixture);
            if (!string.IsNullOrWhiteSpace(fixturePath)) fixtureValue["path"] = fixturePath;

            if (fixture.Restitution != 0) floatToJson("restitution", fixture.Restitution, fixtureValue);
            if (fixture.Friction != 0) floatToJson("friction", fixture.Friction, fixtureValue);
            if (fixture.Density != 0) floatToJson("density", fixture.Density, fixtureValue);
            if (fixture.Trigger) fixtureValue["sensor"] = true;

            if (fixture.CategoryBits != 0x0001) fixtureValue["filter-categoryBits"] = fixture.CategoryBits;
            if (fixture.MaskBits != 0xffff) fixtureValue["filter-maskBits"] = fixture.MaskBits;
            if (fixture.GroupIndex != 0) fixtureValue["filter-groupIndex"] = fixture.GroupIndex;


            JObject shapeValue = new JObject();
            switch (fixture)
            {
                case CollisionCircle2D circle:
                    floatToJson("radius", circle.Radius, shapeValue);
                    vecToJson("center", circle.Center, shapeValue);
                    fixtureValue["circle"] = shapeValue;
                    break;

                case CollisionEdge2D edge:
                    vecToJson("vertex1", edge.Vertex1, shapeValue);
                    vecToJson("vertex2", edge.Vertex2, shapeValue);
                    // not exists smooth collision in urho2d
                    //if (edge.m_hasVertex0) fixtureValue["edge"]["hasVertex0"] = true;
                    //if (edge.m_hasVertex3) fixtureValue["edge"]["hasVertex3"] = true;
                    //if (edge.m_hasVertex0) vecToJson("vertex0", edge.m_vertex0, fixtureValue["edge"]);
                    //if (edge.m_hasVertex3) vecToJson("vertex3", edge.m_vertex3, fixtureValue["edge"]);
                    fixtureValue["edge"] = shapeValue;
                    break;

                case CollisionChain2D chain:

                    uint count = chain.VertexCount;

                    for (uint i = 0; i < count; ++i) vecToJson("vertices", chain.GetVertex(i), shapeValue, (int)i);
                    // Urho2d not has next/previous vertex, only has loop.
                    // this code is created reading 'b2ChainShape.cpp' from box2d
                    if (chain.Loop)
                    {
                        shapeValue["hasPrevVertex"] = true;
                        shapeValue["hasNextVertex"] = true;

                        vecToJson("prevVertex", chain.GetVertex(count - 2), shapeValue);
                        vecToJson("nextVertex", chain.GetVertex(1), shapeValue);
                    }
                    fixtureValue["chain"] = shapeValue;

                    break;

                case CollisionPolygon2D poly:

                    uint vertexCount = poly.VertexCount;

                    for (uint i = 0; i < vertexCount; ++i) vecToJson("vertices", poly.GetVertex(i), shapeValue, (int)i);
                    fixtureValue["polygon"] = shapeValue;

                    break;
                default:
                    System.Diagnostics.Trace.WriteLine("Unknown shape type : " + fixture.TypeName);
                    break;
            }

            JArray customPropertyValue = writeCustomPropertiesToJson(fixture);
            if (customPropertyValue.Count > 0) fixtureValue["customProperties"] = customPropertyValue;

            return fixtureValue;
        }

        public JObject b2j(Constraint2D joint)
        {
            JObject jointValue = new JObject();

            string jointName = getJointName(joint);
            if (jointName != "") jointValue["name"] = jointName;

            string jointPath = getJointPath(joint);
            if (jointPath != "") jointValue["path"] = jointPath;


            RigidBody2D bodyA = joint.OwnerBody;
            RigidBody2D bodyB = joint.OtherBody;

            int bodyIndexA = lookupBodyIndex(bodyA);
            int bodyIndexB = lookupBodyIndex(bodyB);
            jointValue["bodyA"] = bodyIndexA;
            jointValue["bodyB"] = bodyIndexB;
            if (joint.CollideConnected) jointValue["collideConnected"] = true;

            switch (joint)
            {
                case ConstraintRevolute2D revoluteJoint:
                    jointValue["type"] = "revolute";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(revoluteJoint.Anchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(revoluteJoint.Anchor), jointValue);
                    floatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                    // not exists in urho2d
                    // floatToJson("jointSpeed", revoluteJoint.GetJointSpeed(), jointValue);
                    jointValue["enableLimit"] = revoluteJoint.EnableLimit;
                    floatToJson("lowerLimit", revoluteJoint.LowerAngle, jointValue);
                    floatToJson("upperLimit", revoluteJoint.UpperAngle, jointValue);
                    jointValue["enableMotor"] = revoluteJoint.EnableMotor;
                    floatToJson("motorSpeed", revoluteJoint.MotorSpeed, jointValue);
                    floatToJson("maxMotorTorque", revoluteJoint.MaxMotorTorque, jointValue);
                    break;

                case ConstraintPrismatic2D prismaticJoint:
                    {
                        jointValue["type"] = "prismatic";

                        vecToJson("anchorA", bodyA.Node.WorldToLocal2D(prismaticJoint.Anchor), jointValue);
                        vecToJson("anchorB", bodyB.Node.WorldToLocal2D(prismaticJoint.Anchor), jointValue);
                        vecToJson("localAxisA", prismaticJoint.Axis, jointValue);
                        floatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                        jointValue["enableLimit"] = prismaticJoint.EnableLimit;
                        floatToJson("lowerLimit", prismaticJoint.LowerTranslation, jointValue);
                        floatToJson("upperLimit", prismaticJoint.UpperTranslation, jointValue);
                        jointValue["enableMotor"] = prismaticJoint.EnableMotor;
                        floatToJson("maxMotorForce", prismaticJoint.MaxMotorForce, jointValue);
                        floatToJson("motorSpeed", prismaticJoint.MotorSpeed, jointValue);
                    }
                    break;

                case ConstraintDistance2D distanceJoint:
                    jointValue["type"] = "distance";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(distanceJoint.OwnerBodyAnchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(distanceJoint.OtherBodyAnchor), jointValue);
                    floatToJson("length", distanceJoint.Length, jointValue);
                    floatToJson("frequency", distanceJoint.FrequencyHz, jointValue);
                    floatToJson("dampingRatio", distanceJoint.DampingRatio, jointValue);
                    break;

                case ConstraintPulley2D pulleyJoint:
                    jointValue["type"] = "pulley";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(pulleyJoint.OwnerBodyAnchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(pulleyJoint.OtherBodyAnchor), jointValue);
                    vecToJson("groundAnchorA", pulleyJoint.OwnerBodyGroundAnchor, jointValue);
                    vecToJson("groundAnchorB", pulleyJoint.OtherBodyGroundAnchor, jointValue);
                    floatToJson("lengthA", (pulleyJoint.OwnerBodyGroundAnchor - pulleyJoint.OwnerBodyAnchor).Length, jointValue);
                    floatToJson("lengthB", (pulleyJoint.OtherBodyGroundAnchor - pulleyJoint.OtherBodyAnchor).Length, jointValue);
                    floatToJson("ratio", pulleyJoint.Ratio, jointValue);
                    break;

                case ConstraintMouse2D mouseJoint:
                    jointValue["type"] = "mouse";

                    vecToJson("target", mouseJoint.Target, jointValue);
                    // not exists in urho2d
                    // vecToJson("anchorB", mouseJoint.GetAnchorB(), jointValue);
                    floatToJson("maxForce", mouseJoint.MaxForce, jointValue);
                    floatToJson("frequency", mouseJoint.FrequencyHz, jointValue);
                    floatToJson("dampingRatio", mouseJoint.DampingRatio, jointValue);
                    break;

                case ConstraintGear2D gearJoint:
                    jointValue["type"] = "gear";

                    int jointIndex1 = lookupJointIndex(gearJoint.OwnerConstraint);
                    int jointIndex2 = lookupJointIndex(gearJoint.OtherConstraint);
                    jointValue["joint1"] = jointIndex1;
                    jointValue["joint2"] = jointIndex2;
                    jointValue["ratio"] = gearJoint.Ratio;
                    break;

                case ConstraintWheel2D wheelJoint:

                    jointValue["type"] = "wheel";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(wheelJoint.Anchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(wheelJoint.Anchor), jointValue);
                    vecToJson("localAxisA", wheelJoint.Axis, jointValue);
                    jointValue["enableMotor"] = wheelJoint.EnableMotor;
                    floatToJson("motorSpeed", wheelJoint.MotorSpeed, jointValue);
                    floatToJson("maxMotorTorque", wheelJoint.MaxMotorTorque, jointValue);
                    floatToJson("springFrequency", wheelJoint.FrequencyHz, jointValue);
                    floatToJson("springDampingRatio", wheelJoint.DampingRatio, jointValue);

                    break;

                case ConstraintMotor2D motorJoint:

                    jointValue["type"] = "motor";

                    vecToJson("linearOffset", motorJoint.LinearOffset, jointValue);
                    vecToJson("anchorA", motorJoint.LinearOffset, jointValue);
                    floatToJson("refAngle", motorJoint.AngularOffset, jointValue);
                    floatToJson("maxForce", motorJoint.MaxForce, jointValue);
                    floatToJson("maxTorque", motorJoint.MaxTorque, jointValue);
                    floatToJson("correctionFactor", motorJoint.CorrectionFactor, jointValue);

                    break;

                case ConstraintWeld2D weldJoint:

                    jointValue["type"] = "weld";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(weldJoint.Anchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(weldJoint.Anchor), jointValue);

                    floatToJson("refAngle", bodyB.Node.Rotation2D - bodyA.Node.Rotation2D, jointValue);
                    floatToJson("frequency", weldJoint.FrequencyHz, jointValue);
                    floatToJson("dampingRatio", weldJoint.DampingRatio, jointValue);

                    break;

                case ConstraintFriction2D frictionJoint:

                    jointValue["type"] = "friction";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(frictionJoint.Anchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(frictionJoint.Anchor), jointValue);
                    floatToJson("maxForce", frictionJoint.MaxForce, jointValue);
                    floatToJson("maxTorque", frictionJoint.MaxTorque, jointValue);

                    break;

                case ConstraintRope2D ropeJoint:
                    jointValue["type"] = "rope";

                    vecToJson("anchorA", bodyA.Node.WorldToLocal2D(ropeJoint.OwnerBodyAnchor), jointValue);
                    vecToJson("anchorB", bodyB.Node.WorldToLocal2D(ropeJoint.OtherBodyAnchor), jointValue);
                    floatToJson("maxLength", ropeJoint.MaxLength, jointValue);

                    break;

                default:
                    System.Diagnostics.Trace.WriteLine("Unknown joint type not stored in snapshot : " + joint.TypeName);
                    break;
            }

            JArray customPropertyValue = writeCustomPropertiesToJson(joint);
            if (customPropertyValue.Count > 0) jointValue["customProperties"] = customPropertyValue;

            return jointValue;
        }

        public JObject b2j(B2dJsonImage image)
        {
            JObject imageValue = new JObject();

            imageValue["body"] = null != image.Body ? lookupBodyIndex(image.Body) : -1;

            if (null != image.Name) imageValue["name"] = image.Name;
            if (image.Path != "") imageValue["path"] = image.Path;
            if (null != image.File) imageValue["file"] = image.File;

            vecToJson("center", image.Center, imageValue);
            floatToJson("angle", image.Angle, imageValue);
            floatToJson("scale", image.Scale, imageValue);
            floatToJson("aspectScale", image.AspectScale, imageValue);
            if (image.Flip) imageValue["flip"] = true;
            floatToJson("opacity", image.Opacity, imageValue);
            imageValue["filter"] = (int)image.Filter;
            floatToJson("renderOrder", image.RenderOrder, imageValue);

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
            for (int i = 0; i < 4; i++) vecToJson("corners", image.Corners[i], imageValue, i);

            // image->updateUVs();
            for (int i = 0; i < 2 * image.NumPoints; i++)
            {
                vecToJson("glVertexPointer", image.Points[i], imageValue, i);
                vecToJson("glTexCoordPointer", image.UvCoords[i], imageValue, i);
            }
            for (int i = 0; i < image.NumIndices; i++)
                vecToJson("glDrawElements", (uint)image.Indices[i], imageValue, i);

            JArray customPropertyValue = writeCustomPropertiesToJson(image);
            if (customPropertyValue.Count > 0) imageValue["customProperties"] = customPropertyValue;

            return imageValue;
        }

        #endregion [writing functions]



        #region [Setters]

        public void setBodyName(RigidBody2D body, string name) { m_bodyToNameMap[body] = name; }
        public void setFixtureName(CollisionShape2D fixture, string name) { m_fixtureToNameMap[fixture] = name; }
        public void setJointName(Constraint2D joint, string name) { m_jointToNameMap[joint] = name; }
        public void setImageName(B2dJsonImage image, string name) { m_imageToNameMap[image] = name; }

        public void setBodyPath(RigidBody2D body, string path) { m_bodyToPathMap[body] = path; }
        public void setFixturePath(CollisionShape2D fixture, string path) { m_fixtureToPathMap[fixture] = path; }
        public void setJointPath(Constraint2D joint, string path) { m_jointToPathMap[joint] = path; }
        public void setImagePath(B2dJsonImage image, string path) { m_imageToNameMap[image] = path; }

        public void addImage(B2dJsonImage image) { setImageName(image, image.Name); }

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
        public bool readIntoSceneFromValue(JObject b2djsonWorld, Scene urhoScene)
        {
            j2b2World(b2djsonWorld, urhoScene);
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
        public bool readIntoSceneFromString(string str, Scene urhoScene, out string errorMsg)
        {
            errorMsg = null;
            bool hasError;

            try
            {
                JObject worldValue = JObject.Parse(str);
                j2b2World(worldValue, urhoScene);
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
        public bool readIntoSceneFromFile(string filename, Scene ushoScene, out string errorMsg)
        {
            errorMsg = null;
            bool hasError;            

            try
            {
                if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException("Param filename is null or empty");

                string str = System.IO.File.ReadAllText(filename);
                JObject worldValue = JObject.Parse(str);
                j2b2World(worldValue, ushoScene);
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
        public Node j2b2World(JObject worldValue, Scene urhoScene)
        {            
            if (null == urhoScene) throw new ArgumentNullException("ushoScene");

            m_bodies.Clear();

            PhysicsWorld2D world = urhoScene.GetOrCreateComponent<PhysicsWorld2D>();
            world.Gravity = jsonToVec("gravity", worldValue);
            
            world.AllowSleeping = (bool)worldValue["allowSleep"];
            world.AutoClearForces = (bool)worldValue["autoClearForces"];
            world.WarmStarting = (bool)worldValue["warmStarting"];
            world.ContinuousPhysics = (bool)worldValue["continuousPhysics"];
            world.SubStepping = (bool)worldValue["subStepping"];

            readCustomPropertiesFromJson(world, worldValue);

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
                    RigidBody2D body = j2b2Body(physicRootNode.CreateChild(mode: m_creationMode), bodyValue);
                    readCustomPropertiesFromJson(body, bodyValue);
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
                        Constraint2D joint = j2b2Joint(world, jointValue);
                        readCustomPropertiesFromJson(joint, jointValue);
                        m_joints.Add(joint);
                    }
                }
                for (int i = 0; i < numJointValues; i++)
                {
                    JObject jointValue = (JObject)jointValues[i];
                    if (jointValue["type"].ToString() == "gear")
                    {
                        Constraint2D joint = j2b2Joint(world, jointValue);
                        readCustomPropertiesFromJson(joint, jointValue);
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
                    B2dJsonImage image = j2b2dJsonImage(imageValue);
                    readCustomPropertiesFromJson(image, imageValue);
                    m_images.Add(image);
                }
            }

            return physicRootNode;
        }


        public RigidBody2D j2b2Body(Node bodyNode, JObject bodyValue)
        {

            RigidBody2D body = bodyNode.CreateComponent<RigidBody2D>(mode: m_creationMode);

            body.BodyType = (BodyType2D)(int.Parse(bodyValue["type"].ToString()));
            bodyNode.Position = new Vector3(jsonToVec("position", bodyValue));
            bodyNode.Rotation2D = jsonToFloat("angle", bodyValue);
            body.SetLinearVelocity(jsonToVec("linearVelocity", bodyValue));
            body.AngularVelocity = jsonToFloat("angularVelocity", bodyValue);
            body.LinearDamping = jsonToFloat("linearDamping", bodyValue, -1, 0);
            body.AngularDamping = jsonToFloat("angularDamping", bodyValue, -1, 0);
            body.GravityScale = jsonToFloat("gravityScale", bodyValue, -1, 1);

            body.AllowSleep = bodyValue["allowSleep"] == null ? true : (bool)bodyValue["allowSleep"];
            body.Awake = bodyValue["awake"] == null ? false : (bool)bodyValue["awake"];
            body.FixedRotation = bodyValue["fixedRotation"] == null ? false : (bool)bodyValue["fixedRotation"];
            body.Bullet = bodyValue["bullet"] == null ? false : (bool)bodyValue["bullet"];
            body.Enabled = bodyValue["active"] == null ? true : (bool)bodyValue["active"];


            string bodyName = bodyValue["name"]?.ToString();
            if (null != bodyName) setBodyName(body, bodyName);

            string bodyPath = bodyValue["path"]?.ToString();
            if (null != bodyPath) setBodyPath(body, bodyPath);

            int i = 0;
            JArray fixtureValues = (JArray)bodyValue["fixture"];
            if (null != fixtureValues)
            {
                int numFixtureValues = fixtureValues.Count;
                for (i = 0; i < numFixtureValues; i++)
                {
                    JObject fixtureValue = (JObject)fixtureValues[i];
                    CollisionShape2D fixture = j2b2Fixture(body, fixtureValue);
                    readCustomPropertiesFromJson(fixture, fixtureValue);
                }
            }

            // may be necessary if user has overridden mass characteristics
            body.Mass = jsonToFloat("massData-mass", bodyValue);
            body.SetMassCenter(jsonToVec("massData-center", bodyValue));
            body.Inertia = jsonToFloat("massData-I", bodyValue);

            return body;
        }

        public CollisionShape2D j2b2Fixture(RigidBody2D body, JObject fixtureValue)
        {
            CollisionShape2D fixture = null;
            if (null == fixtureValue) return fixture;
            
            var restitution = jsonToFloat("restitution", fixtureValue);
            var friction = jsonToFloat("friction", fixtureValue);
            var density = jsonToFloat("density", fixtureValue);
            var isSensor = fixtureValue["sensor"] == null ? false : (bool)fixtureValue["sensor"];

            var categoryBits = fixtureValue["filter-categoryBits"] == null ? 0x0001 : (int)fixtureValue["filter-categoryBits"];
            var maskBits = fixtureValue["filter-maskBits"] == null ? 0xffff : (int)fixtureValue["filter-maskBits"];
            var groupIndex = fixtureValue["filter-groupIndex"] == null ? (short)0 : (short)fixtureValue["filter-groupIndex"];


            if (null != fixtureValue["circle"])
            {
                CollisionCircle2D circleFixture = body.Node.CreateComponent<CollisionCircle2D>(mode: m_creationMode);
                JObject circleValue = (JObject)fixtureValue["circle"];                
                circleFixture.Center = jsonToVec("center", circleValue);
                circleFixture.Radius = jsonToFloat("radius", circleValue);
                circleFixture.Density = density;
                fixture = circleFixture;
            }
            else if (null != fixtureValue["edge"])
            {
                CollisionEdge2D edgeFixture = body.Node.CreateComponent<CollisionEdge2D>(mode: m_creationMode);
                JObject edgeValue = (JObject)fixtureValue["edge"];
                edgeFixture.Vertex1 = jsonToVec("vertex1", edgeValue);
                edgeFixture.Vertex2 = jsonToVec("vertex2", edgeValue);
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
                for (int i = 0; i < numVertices; i++) chainFixture.SetVertex((uint)i, jsonToVec("vertices", chainValue, i));
                chainFixture.Loop = true;
                fixture = chainFixture;
            }
            else if (null != fixtureValue["chain"])
            {
                CollisionChain2D chainFixture = body.Node.CreateComponent<CollisionChain2D>(mode: m_creationMode);
                JObject chainValue = (JObject)fixtureValue["chain"];                
                int numVertices = ((JArray)chainValue["vertices"]["x"]).Count;
                List<Vector2> vertices = new List<Vector2>(numVertices);
                for (int i = 0; i < numVertices; i++) vertices.Add(jsonToVec("vertices", chainValue, i));

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
                    poligonFixture.Vertex1 = (jsonToVec("vertices", polygonValue, 0));
                    poligonFixture.Vertex2 = (jsonToVec("vertices", polygonValue, 1));
                    fixture = poligonFixture;
                }
                else
                {
                    CollisionPolygon2D poligonFixture = body.Node.CreateComponent<CollisionPolygon2D>(mode: m_creationMode);
                    for (int i = 0; i < numVertices; i++) poligonFixture.SetVertex((uint)i, jsonToVec("vertices", polygonValue, i));
                    fixture = poligonFixture;
                }                
            }

            string fixtureName = fixtureValue["name"]?.ToString();
            if (null != fixtureName) setFixtureName(fixture, fixtureName);
            string fixturePath = fixtureValue["path"]?.ToString();
            if (null != fixturePath) setFixturePath(fixture, fixturePath);

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

        public Constraint2D j2b2Joint(PhysicsWorld2D world, JObject jointValue)
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
                revoluteDef.OtherBody = bodyB;

                revoluteDef.Anchor = jsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB and reference angle
                // revoluteDef.localAnchorB = jsonToVec("anchorB", jointValue);
                // revoluteDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                revoluteDef.EnableLimit = jointValue["enableLimit"] == null ? false : (bool)jointValue["enableLimit"];
                revoluteDef.LowerAngle = jsonToFloat("lowerLimit", jointValue);
                revoluteDef.UpperAngle = jsonToFloat("upperLimit", jointValue);
                revoluteDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                revoluteDef.MotorSpeed = jsonToFloat("motorSpeed", jointValue);
                revoluteDef.MaxMotorTorque = jsonToFloat("maxMotorTorque", jointValue);                
            }
            else if (type == "prismatic")
            {
                joint = prismaticDef = bodyA.Node.CreateComponent<ConstraintPrismatic2D>(mode: m_creationMode);
                prismaticDef.OtherBody = bodyB;

                prismaticDef.Anchor = jsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB and reference angle
                // prismaticDef.localAnchorB = jsonToVec("anchorB", jointValue);
                // prismaticDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                prismaticDef.Axis = jointValue["localAxisA"] != null ? jsonToVec("localAxisA", jointValue) : jsonToVec("localAxis1", jointValue);
                prismaticDef.EnableLimit = jointValue["enableLimit"] == null ? false : (bool)jointValue["enableLimit"];
                prismaticDef.LowerTranslation = jsonToFloat("lowerLimit", jointValue);
                prismaticDef.UpperTranslation = jsonToFloat("upperLimit", jointValue);
                prismaticDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                prismaticDef.MotorSpeed = jsonToFloat("motorSpeed", jointValue);
                prismaticDef.MaxMotorForce = jsonToFloat("maxMotorForce", jointValue);
            }
            else if (type == "distance")
            {
                joint = distanceDef = bodyA.Node.CreateComponent<ConstraintDistance2D>(mode: m_creationMode);
                distanceDef.OtherBody = bodyB;

                distanceDef.OwnerBodyAnchor = jsonToVec("anchorA", jointValue);
                distanceDef.OtherBodyAnchor = jsonToVec("anchorB", jointValue);
                distanceDef.Length = jsonToFloat("length", jointValue);
                distanceDef.FrequencyHz = jsonToFloat("frequency", jointValue);
                distanceDef.DampingRatio = jsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "pulley")
            {
                joint = pulleyDef = bodyA.Node.CreateComponent<ConstraintPulley2D>(mode: m_creationMode);
                pulleyDef.OtherBody = bodyB;
                                 
                pulleyDef.OwnerBodyGroundAnchor = jsonToVec("groundAnchorA", jointValue);
                pulleyDef.OtherBodyGroundAnchor = jsonToVec("groundAnchorB", jointValue);
                pulleyDef.OwnerBodyAnchor = jsonToVec("anchorA", jointValue);
                pulleyDef.OtherBodyAnchor = jsonToVec("anchorB", jointValue);
                // urho2d not contains length (= OwnerBodyGroundAnchor - OtherBodyGroundAnchor)
                // pulleyDef.lengthA = jsonToFloat("lengthA", jointValue);
                // pulleyDef.lengthB = jsonToFloat("lengthB", jointValue);
                pulleyDef.Ratio = jsonToFloat("ratio", jointValue);
            }
            else if (type == "mouse")
            {
                joint = mouseDef = bodyA.Node.CreateComponent<ConstraintMouse2D>(mode: m_creationMode);
                mouseDef.OtherBody = bodyB;
                 
                mouseJointTarget = jsonToVec("target", jointValue);
                mouseDef.Target = jsonToVec("anchorB", jointValue); // alter after creating joint
                mouseDef.MaxForce = jsonToFloat("maxForce", jointValue);
                mouseDef.FrequencyHz = jsonToFloat("frequency", jointValue);
                mouseDef.DampingRatio = jsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "gear")
            {
                joint = gearDef = bodyA.Node.CreateComponent<ConstraintGear2D>(mode: m_creationMode);
                gearDef.OtherBody = bodyB;

                int jointIndex1 = (int)jointValue["joint1"];
                int jointIndex2 = (int)jointValue["joint2"];
                gearDef.OwnerConstraint = m_joints[jointIndex1];
                gearDef.OtherConstraint = m_joints[jointIndex2];
                gearDef.Ratio = jsonToFloat("ratio", jointValue);
            }
            else if (type == "wheel")
            {
                joint = wheelDef = bodyA.Node.CreateComponent<ConstraintWheel2D>(mode: m_creationMode);
                wheelDef.OtherBody = bodyB;
                 
                wheelDef.Anchor = jsonToVec("anchorA", jointValue);
                // Urho2d not contains anchorB
                // wheelDef.localAnchorB = jsonToVec("anchorB", jointValue);
                wheelDef.Axis = jsonToVec("localAxisA", jointValue);
                wheelDef.EnableMotor = jointValue["enableMotor"] == null ? false : (bool)jointValue["enableMotor"];
                wheelDef.MotorSpeed = jsonToFloat("motorSpeed", jointValue);
                wheelDef.MaxMotorTorque = jsonToFloat("maxMotorTorque", jointValue);
                wheelDef.FrequencyHz = jsonToFloat("springFrequency", jointValue);
                wheelDef.DampingRatio = jsonToFloat("springDampingRatio", jointValue);
            }
            else if (type == "motor")
            {
                jointDef = &motorDef;
                if (jointValue.isMember("linearOffset"))
                    motorDef.linearOffset = jsonToVec("linearOffset", jointValue);
                else
                    motorDef.linearOffset = jsonToVec("anchorA", jointValue); //pre v1.7 editor exported anchorA as the linear offset
                motorDef.angularOffset = jsonToFloat("refAngle", jointValue);
                motorDef.maxForce = jsonToFloat("maxForce", jointValue);
                motorDef.maxTorque = jsonToFloat("maxTorque", jointValue);
                motorDef.correctionFactor = jsonToFloat("correctionFactor", jointValue);
            }
            else if (type == "weld")
            {
                jointDef = &weldDef;
                weldDef.localAnchorA = jsonToVec("anchorA", jointValue);
                weldDef.localAnchorB = jsonToVec("anchorB", jointValue);
                weldDef.referenceAngle = jsonToFloat("refAngle", jointValue);
                weldDef.frequencyHz = jsonToFloat("frequency", jointValue);
                weldDef.dampingRatio = jsonToFloat("dampingRatio", jointValue);
            }
            else if (type == "friction")
            {
                jointDef = &frictionDef;
                frictionDef.localAnchorA = jsonToVec("anchorA", jointValue);
                frictionDef.localAnchorB = jsonToVec("anchorB", jointValue);
                frictionDef.maxForce = jsonToFloat("maxForce", jointValue);
                frictionDef.maxTorque = jsonToFloat("maxTorque", jointValue);
            }
            else if (type == "rope")
            {
                jointDef = &ropeDef;
                ropeDef.localAnchorA = jsonToVec("anchorA", jointValue);
                ropeDef.localAnchorB = jsonToVec("anchorB", jointValue);
                ropeDef.maxLength = jsonToFloat("maxLength", jointValue);
            }

            if (jointDef)
            {
                //set features common to all joints
                jointDef->bodyA = m_bodies[bodyIndexA];
                jointDef->bodyB = m_bodies[bodyIndexB];
                jointDef->collideConnected = jointValue.get("collideConnected", false).asBool();

                joint = world->CreateJoint(jointDef);

                if (type == "mouse") ((ConstraintMouse2D)joint).Target = mouseJointTarget;

                string jointName = jointValue.get("name", "").asString();
                if (jointName != "")
                {
                    setJointName(joint, jointName.c_str());
                }

                string jointPath = jointValue.get("path", "").asString();
                if (jointPath != "")
                {
                    setJointPath(joint, jointPath.c_str());
                }
            }

            return joint;
        }

        public B2dJsonImage j2b2dJsonImage(JObject imageValue)
        {

        }

        #endregion [reading functions]



        #region [Getters]

        public int getBodiesByName(string name, List<RigidBody2D> bodies) { }
        public int getFixturesByName(string name, List<CollisionShape2D> fixtures) { }
        public int getJointsByName(string name, List<Constraint2D> joints) { }
        public int getImagesByName(string name, List<B2dJsonImage> images) { }

        public int getBodiesByPath(string path, List<RigidBody2D> bodies) { }
        public int getFixturesByPath(string path, List<CollisionShape2D> fixtures) { }
        public int getJointsByPath(string path, List<Constraint2D> joints) { }
        public int getImagesByPath(string path, List<B2dJsonImage> images) { }

        public int getAllBodies(List<RigidBody2D> bodies) { }
        public int getAllFixtures(List<CollisionShape2D> fixtures) { }
        public int getAllJoints(List<Constraint2D> joints) { }
        public int getAllImages(List<B2dJsonImage> images) { }

        public RigidBody2D getBodyByName(string name) { }
        public CollisionShape2D getFixtureByName(string name) { }
        public Constraint2D getJointByName(string name) { }
        public B2dJsonImage getImageByName(string name) { }

        public RigidBody2D getBodyByPathAndName(string path, string name) { }
        public CollisionShape2D getFixtureByPathAndName(string path, string name) { }
        public Constraint2D getJointByPathAndName(string path, string name) { }
        public B2dJsonImage getImageByPathAndName(string path, string name) { }

        public Dictionary<Constraint2D, string> getJointToNameMap() { return m_jointToNameMap; }
        public Dictionary<CollisionShape2D, string> getFixtureToNameMap() { return m_fixtureToNameMap; }

        public string getBodyName(RigidBody2D body) { }
        public string getFixtureName(CollisionShape2D fixture) { }
        public string getJointName(Constraint2D joint) { }
        public string getImageName(B2dJsonImage img) { }

        public string getBodyPath(RigidBody2D body) { }
        public string getFixturePath(CollisionShape2D fixture) { }
        public string getJointPath(Constraint2D joint) { }
        public string getImagePath(B2dJsonImage img) { }


        #endregion [Getters]




        #region [custom properties]

        public B2dJsonCustomProperties getCustomPropertiesForItem(object item, bool createIfNotExisting) { }

        protected void setCustomInt(object item, string propertyName, int val) { }
        protected void setCustomFloat(object item, string propertyName, float val) { }
        protected void setCustomString(object item, string propertyName, string val) { }
        protected void setCustomVector(object item, string propertyName, Vector2 val) { }
        protected void setCustomBool(object item, string propertyName, bool val) { }
        protected void setCustomColor(object item, string propertyName, B2dJsonColor4 val) { }


        // //this define saves us writing out 25 functions which are almost exactly the same
        // #define DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(ucType, lcType)\
        //     void setCustom##ucType(RigidBody2D item, string propertyName, lcType val)          { m_bodiesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
        //     void setCustom##ucType(CollisionShape2D item, string propertyName, lcType val)       { m_fixturesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
        //     void setCustom##ucType(Constraint2D item, string propertyName, lcType val)         { m_jointsWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
        //     void setCustom##ucType(B2dJsonImage item, string propertyName, lcType val)    { m_imagesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
        //     void setCustom##ucType(PhysicsWorld2D item, string propertyName, lcType val)         { m_worldsWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }

        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Int, int)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Float, float)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(String, string)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Vector, Vector2)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Bool, bool)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Color, B2dJsonColor4)

        public bool hasCustomInt(object item, string propertyName) { }
        public bool hasCustomFloat(object item, string propertyName) { }
        public bool hasCustomString(object item, string propertyName) { }
        public bool hasCustomVector(object item, string propertyName) { }
        public bool hasCustomBool(object item, string propertyName) { }
        public bool hasCustomColor(object item, string propertyName) { }

        public int getCustomInt(object item, string propertyName, int defaultVal = 0) { }
        public float getCustomFloat(object item, string propertyName, float defaultVal = 0) { }
        public string getCustomString(object item, string propertyName, string defaultVal = "") { }
        public Vector2 getCustomVector(object item, string propertyName, Vector2 defaultVal = Vector2(0, 0)) { }
        public bool getCustomBool(object item, string propertyName, bool defaultVal = false) { }
        public B2dJsonColor4 getCustomColor(object item, string propertyName, B2dJsonColor4 defaultVal = B2dJsonColor4()) { }

        // //this define saves us writing out 20 functions which are almost exactly the same
        // #define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(ucType, lcType)\
        // int getBodiesByCustom##ucType(   string propertyName, lcType valueToMatch, List<RigidBody2D> bodies);\
        //     int getFixturesByCustom##ucType( string propertyName, lcType valueToMatch, List<CollisionShape2D> fixtures);\
        //     int getJointsByCustom##ucType(   string propertyName, lcType valueToMatch, List<Constraint2D> joints);\
        //     int getImagesByCustom##ucType(   string propertyName, lcType valueToMatch, List<B2dJsonImage> images);

        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Int, int)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Float, float)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(String, string)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Vector, Vector2)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Bool, bool)

        // //this define saves us writing out 20 functions which are almost exactly the same
        // #define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(ucType, lcType)\
        //     RigidBody2D getBodyByCustom##ucType(    string propertyName, lcType valueToMatch);\
        //     CollisionShape2D getFixtureByCustom##ucType( string propertyName, lcType valueToMatch);\
        //     Constraint2D getJointByCustom##ucType(   string propertyName, lcType valueToMatch);\
        //     B2dJsonImage getImageByCustom##ucType(   string propertyName, lcType valueToMatch);

        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Int, int)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Float, float)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(String, string)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Vector, Vector2)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Bool, bool)

        #endregion [custom properties]



        #region [member helpers]

        protected void vecToJson(string name, uint v, JObject value, int index = -1) { }
        protected void vecToJson(string name, float v, JObject value, int index = -1) { }
        protected void vecToJson(string name, Vector2 vec, JObject value, int index = -1) { }
        protected void floatToJson(string name, float f, JObject value) { }
        protected RigidBody2D lookupBodyFromIndex(uint index) { }
        protected int lookupBodyIndex(RigidBody2D body) { }
        protected int lookupJointIndex(Constraint2D joint) { }

        protected JArray writeCustomPropertiesToJson(object item) { }
        protected void readCustomPropertiesFromJson(RigidBody2D item, JObject value) { }
        protected void readCustomPropertiesFromJson(CollisionShape2D item, JObject value) { }
        protected void readCustomPropertiesFromJson(Constraint2D item, JObject value) { }
        protected void readCustomPropertiesFromJson(B2dJsonImage item, JObject value) { }
        protected void readCustomPropertiesFromJson(PhysicsWorld2D item, JObject value) { }

        #endregion [member helpers]



        #region [static helpers]

        public static string floatToHex(float f) { }
        public static float hexToFloat(string str) { }
        public static float jsonToFloat(string name, JObject value, int index = -1, float defaultValue = 0) { }
        public static Vector2 jsonToVec(string name, JObject value, int index = -1, Vector2 defaultValue = Vector2(0, 0)) { }

        #endregion [static helpers]

    }

}



