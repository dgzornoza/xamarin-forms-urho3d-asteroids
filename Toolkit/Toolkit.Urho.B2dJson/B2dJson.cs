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
        public b2dJson(bool useHumanReadableFloats = false)
        {
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

        public JObject writeToValue(PhysicsWorld2D world)
        {
            if (null == world) return new JObject();

            return b2j(world);
        }

        public string writeToString(PhysicsWorld2D world)
        {
            if (null == world) return string.Empty;

            return b2j(world).ToString();
        }

        public bool writeToFile(PhysicsWorld2D world, string filename, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (null == world || string.IsNullOrWhiteSpace(filename)) return false;

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
            index = 0;
            jArray = new JArray();
            IEnumerable<Constraint2D> worldJointList = world.Scene.GetRecursiveComponents<Constraint2D>();
            foreach (var joint in worldJointList)
            {
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

                    vecToJson("anchorA", bodyA. GetLocalPoint(revoluteJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(revoluteJoint.GetAnchorB()), jointValue);
                    floatToJson("refAngle", bodyB.GetAngle() - bodyA.GetAngle() - revoluteJoint.GetJointAngle(), jointValue);
                    floatToJson("jointSpeed", revoluteJoint.GetJointSpeed(), jointValue);
                    jointValue["enableLimit"] = revoluteJoint.IsLimitEnabled();
                    floatToJson("lowerLimit", revoluteJoint.GetLowerLimit(), jointValue);
                    floatToJson("upperLimit", revoluteJoint.GetUpperLimit(), jointValue);
                    jointValue["enableMotor"] = revoluteJoint.IsMotorEnabled();
                    floatToJson("motorSpeed", revoluteJoint.GetMotorSpeed(), jointValue);
                    floatToJson("maxMotorTorque", revoluteJoint.GetMaxMotorTorque(), jointValue);
                    break;

                case ConstraintPrismatic2D prismaticJoint:
                    {
                        jointValue["type"] = "prismatic";

                        vecToJson("anchorA", bodyA.GetLocalPoint(prismaticJoint.GetAnchorA()), jointValue);
                        vecToJson("anchorB", bodyB.GetLocalPoint(prismaticJoint.GetAnchorB()), jointValue);
                        vecToJson("localAxisA", prismaticJoint.GetLocalAxisA(), jointValue);
                        floatToJson("refAngle", prismaticJoint.GetReferenceAngle(), jointValue);
                        jointValue["enableLimit"] = prismaticJoint.IsLimitEnabled();
                        floatToJson("lowerLimit", prismaticJoint.GetLowerLimit(), jointValue);
                        floatToJson("upperLimit", prismaticJoint.GetUpperLimit(), jointValue);
                        jointValue["enableMotor"] = prismaticJoint.IsMotorEnabled();
                        floatToJson("maxMotorForce", prismaticJoint.GetMaxMotorForce(), jointValue);
                        floatToJson("motorSpeed", prismaticJoint.GetMotorSpeed(), jointValue);
                    }
                    break;

                case ConstraintDistance2D distanceJoint:
                    jointValue["type"] = "distance";

                    vecToJson("anchorA", bodyA.GetLocalPoint(distanceJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(distanceJoint.GetAnchorB()), jointValue);
                    floatToJson("length", distanceJoint.GetLength(), jointValue);
                    floatToJson("frequency", distanceJoint.GetFrequency(), jointValue);
                    floatToJson("dampingRatio", distanceJoint.GetDampingRatio(), jointValue);
                    break;

                case ConstraintPulley2D pulleyJoint:
                    jointValue["type"] = "pulley";

                    vecToJson("groundAnchorA", pulleyJoint.GetGroundAnchorA(), jointValue);
                    vecToJson("groundAnchorB", pulleyJoint.GetGroundAnchorB(), jointValue);
                    vecToJson("anchorA", bodyA.GetLocalPoint(pulleyJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(pulleyJoint.GetAnchorB()), jointValue);
                    floatToJson("lengthA", (pulleyJoint.GetGroundAnchorA() - pulleyJoint.GetAnchorA()).Length(), jointValue);
                    floatToJson("lengthB", (pulleyJoint.GetGroundAnchorB() - pulleyJoint.GetAnchorB()).Length(), jointValue);
                    floatToJson("ratio", pulleyJoint.GetRatio(), jointValue);
                    break;

                case ConstraintMouse2D mouseJoint:
                    jointValue["type"] = "mouse";

                    vecToJson("target", mouseJoint.GetTarget(), jointValue);
                    vecToJson("anchorB", mouseJoint.GetAnchorB(), jointValue);
                    floatToJson("maxForce", mouseJoint.GetMaxForce(), jointValue);
                    floatToJson("frequency", mouseJoint.GetFrequency(), jointValue);
                    floatToJson("dampingRatio", mouseJoint.GetDampingRatio(), jointValue);
                    break;

                case ConstraintGear2D gearJoint:
                    jointValue["type"] = "gear";

                    int jointIndex1 = lookupJointIndex(gearJoint.GetJoint1());
                    int jointIndex2 = lookupJointIndex(gearJoint.GetJoint2());
                    jointValue["joint1"] = jointIndex1;
                    jointValue["joint2"] = jointIndex2;
                    jointValue["ratio"] = gearJoint.GetRatio();
                    break;

                case ConstraintGear2D wheelJoint:

                    jointValue["type"] = "wheel";

                    vecToJson("anchorA", bodyA.GetLocalPoint(wheelJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(wheelJoint.GetAnchorB()), jointValue);
                    vecToJson("localAxisA", wheelJoint.GetLocalAxisA(), jointValue);
                    jointValue["enableMotor"] = wheelJoint.IsMotorEnabled();
                    floatToJson("motorSpeed", wheelJoint.GetMotorSpeed(), jointValue);
                    floatToJson("maxMotorTorque", wheelJoint.GetMaxMotorTorque(), jointValue);
                    floatToJson("springFrequency", wheelJoint.GetSpringFrequencyHz(), jointValue);
                    floatToJson("springDampingRatio", wheelJoint.GetSpringDampingRatio(), jointValue);

                    break;

                case ConstraintGear2D motorJoint:

                    jointValue["type"] = "motor";

                    vecToJson("linearOffset", motorJoint.GetLinearOffset(), jointValue);
                    vecToJson("anchorA", motorJoint.GetLinearOffset(), jointValue);
                    floatToJson("refAngle", motorJoint.GetAngularOffset(), jointValue);
                    floatToJson("maxForce", motorJoint.GetMaxForce(), jointValue);
                    floatToJson("maxTorque", motorJoint.GetMaxTorque(), jointValue);
                    floatToJson("correctionFactor", motorJoint.GetCorrectionFactor(), jointValue);

                    break;

                case ConstraintGear2D weldJoint:

                    jointValue["type"] = "weld";

                    vecToJson("anchorA", bodyA.GetLocalPoint(weldJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(weldJoint.GetAnchorB()), jointValue);
                    floatToJson("refAngle", weldJoint.GetReferenceAngle(), jointValue);
                    floatToJson("frequency", weldJoint.GetFrequency(), jointValue);
                    floatToJson("dampingRatio", weldJoint.GetDampingRatio(), jointValue);

                    break;

                case ConstraintGear2D frictionJoint:

                    jointValue["type"] = "friction";

                    vecToJson("anchorA", bodyA.GetLocalPoint(frictionJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(frictionJoint.GetAnchorB()), jointValue);
                    floatToJson("maxForce", frictionJoint.GetMaxForce(), jointValue);
                    floatToJson("maxTorque", frictionJoint.GetMaxTorque(), jointValue);

                    break;

                case ConstraintGear2D ropeJoint:
                    jointValue["type"] = "rope";

                    vecToJson("anchorA", bodyA.GetLocalPoint(ropeJoint.GetAnchorA()), jointValue);
                    vecToJson("anchorB", bodyB.GetLocalPoint(ropeJoint.GetAnchorB()), jointValue);
                    floatToJson("maxLength", ropeJoint->GetMaxLength(), jointValue);

                    break;

                default:
                    System.Diagnostics.Trace.WriteLine("Unknown joint type not stored in snapshot : " + joint.TypeName);
                    break;
            }

            JArray customPropertyValue = writeCustomPropertiesToJson(joint);
            if (customPropertyValue.Count > 0) jointValue["customProperties"] = customPropertyValue;

            return jointValue;
        }

        public JObject b2j(B2dJsonImage image) { }

        public void setBodyName(RigidBody2D body, string name) { }
        public void setFixtureName(CollisionShape2D fixture, string name) { }
        public void setJointName(Constraint2D joint, string name) { }
        public void setImageName(B2dJsonImage image, string name) { }

        public void setBodyPath(RigidBody2D body, string path) { }
        public void setFixturePath(CollisionShape2D fixture, string path) { }
        public void setJointPath(Constraint2D joint, string path) { }
        public void setImagePath(B2dJsonImage image, string path) { }

        public void addImage(B2dJsonImage image) { }

        #endregion [writing functions]


        #region [reading functions]

        public PhysicsWorld2D readFromValue(JObject worldValue, PhysicsWorld2D existingWorld = null) { }
        public PhysicsWorld2D readFromString(string str, out string errorMsg, PhysicsWorld2D existingWorld = null) { }
        public PhysicsWorld2D readFromFile(string filename, out string errorMsg, PhysicsWorld2D existingWorld = null) { }

        #endregion [reading functions]



        #region [backward compatibility]

        public bool readIntoWorldFromValue(PhysicsWorld2D existingWorld, JObject worldValue) { return null != readFromValue(worldValue, existingWorld); }
        public bool readIntoWorldFromString(PhysicsWorld2D existingWorld, string str, out string errorMsg) { return null != readFromString(str, out errorMsg, existingWorld); }
        public bool readIntoWorldFromFile(PhysicsWorld2D existingWorld, string filename, out string errorMsg) { return null != readFromFile(filename, out errorMsg, existingWorld); }

        #endregion [backward compatibility]


        public PhysicsWorld2D j2b2World(JObject worldValue, PhysicsWorld2D world = null) { }
        public RigidBody2D j2b2Body(PhysicsWorld2D world, JObject bodyValue) { }
        public CollisionShape2D j2b2Fixture(RigidBody2D body, JObject fixtureValue) { }
        public Constraint2D j2b2Joint(PhysicsWorld2D world, JObject jointValue) { }
        public B2dJsonImage j2b2dJsonImage(JObject imageValue) { }

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



