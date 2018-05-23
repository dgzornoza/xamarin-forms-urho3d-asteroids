﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Urho;
using Urho.Urho2D;

namespace Toolkit.UrhoSharp.B2dJson
{


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
            if (null == world) return JObject.CreateNull();

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

        private _getConstraints()
        {
            IEnumerable<Node> nodes2 = scene.GetChildrenWithComponent<Constraint2D>(recursive: true)
                .Concat(scene.GetChildrenWithComponent<ConstraintDistance2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintFriction2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintGear2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintMotor2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintMouse2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintPrismatic2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintPulley2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintRevolute2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintRope2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintWeld2D>(recursive: true))
                .Concat(scene.GetChildrenWithComponent<ConstraintWheel2D>(recursive: true));

            groundNode.CreateComponent<Constraint2D>();
            var c2 = nodes2.SelectMany(item => item.Components).Where(item => item.TypeName == Constraint2D.TypeNameStatic);
            var d2 = nodes2.SelectMany(item => item.Components).OfType<Constraint2D>();
        }

        public IEnumerable<Constraint2D> getConstraints(PhysicsWorld2D world)
        {
            IEnumerable<Component> components = world.Scene.Children.SelectMany(item => item.Components).OfType<Constraint2D>();

            // var t = node.Components.Where(item => item.GetType().GetTypeInfo().IsAssignableFrom(typeof(Constraint2D).GetTypeInfo()));

            IEnumerable<Component> getComponents(Node node)
            {
                return node.Children.SelectMany(item => item.Components).OfType<Constraint2D>();
            }

            // si se procesa en cascada, se hace lo mismo en los nodos hijos recursivamente
            foreach (var child in world.Scene.Children) world.Scene.Children.Select(item => getComponents(item));
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


            // Body
            int index = 0;            
            JArray jArray = new JArray();
            IEnumerable<RigidBody2D> worldBodyList = world.Scene.GetChildrenWithComponent<RigidBody2D>(recursive: true).SelectMany(item => item.Components).OfType<RigidBody2D>();
            foreach (var item in worldBodyList)
            {
                m_bodyToIndexMap.Add(item, index);
                jArray.Add(b2j(item));
                index++;
            }
            worldValue["body"] = jArray;


            // Joints: need two passes for joints because gear joints reference other joints
            index = 0;
            jArray = new JArray();
            IEnumerable<Constraint2D> worldBodyList = world.Scene.GetChildrenWithComponent<Constraint2D>(recursive: true).SelectMany(item => item.Components).OfType<Constraint2D>();
            foreach (var joint in world.JointList)
            {
                if (joint.JointType == JointType.Gear)
                    continue;
                m_jointToIndexMap[joint] = i;
                arr.Add(B2n(joint));
                i++;
            }

            foreach (var joint in world.JointList)
            {
                if (joint.JointType != JointType.Gear)
                    continue;
                m_jointToIndexMap[joint] = i;
                arr.Add(B2n(joint));
                i++;
            }
            worldValue["joint"] = arr;

            for (b2Joint* joint = world->GetJointList(); joint; joint = joint->GetNext())
            {
                if (joint->GetType() == e_gearJoint)
                    continue;
                worldValue["joint"][i] = b2j(joint);
                m_jointToIndexMap[joint] = index;
                index++;
            }
            for (b2Joint* joint = world->GetJointList(); joint; joint = joint->GetNext())
            {
                if (joint->GetType() != e_gearJoint)
                    continue;
                worldValue["joint"][i] = b2j(joint);
                m_jointToIndexMap[joint] = index;
                index++;
            }

            // Images
            index = 0;
            {
                std::map<b2dJsonImage*, string>::iterator it = m_imageToNameMap.begin();
                std::map<b2dJsonImage*, string>::iterator end = m_imageToNameMap.end();
                while (it != end)
                {
                    b2dJsonImage* image = it->first;
                    worldValue["image"][index] = b2j(image);
                    index++;

                    ++it;
                }
            }

            // Custom properties
            Json::Value customPropertyValue = writeCustomPropertiesToJson(NULL);
            if (!customPropertyValue.empty())
                worldValue["customProperties"] = customPropertyValue;

            m_bodyToIndexMap.clear();
            m_jointToIndexMap.clear();

            return worldValue;
        }

        public JObject b2j(RigidBody2D body) { }
        public JObject b2j(CollisionShape2D fixture) { }
        public JObject b2j(Constraint2D joint) { }
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

        protected JObject writeCustomPropertiesToJson(object item) { }
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



