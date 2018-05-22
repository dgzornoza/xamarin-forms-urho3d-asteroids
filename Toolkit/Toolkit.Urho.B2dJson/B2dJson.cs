using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

    class b2dJson
    {


        public b2dJson(bool useHumanReadableFloats = false) { }

        public void clear() { }

        // writing functions
        public JObject writeToValue(PhysicsWorld2D world) { }
        public string writeToString(PhysicsWorld2D world) { }
        public bool writeToFile(PhysicsWorld2D world, string filename) { }
         
        public JObject b2j(PhysicsWorld2D world) { }
        public JObject b2j(RigidBody2D body) { }
        public JObject b2j(CollisionShape2D fixture) { }
        public JObject b2j(Constraint2D joint) { }
        // public JObject b2j(b2dJsonImage image) { }
         
        public void setBodyName(RigidBody2D body, string name) { }
        public void setFixtureName(CollisionShape2D fixture, string name) { }
        public void setJointName(Constraint2D joint, string name) { }
        public void setImageName(b2dJsonImage image, string name) { }
        public 
        public void addImage(b2dJsonImage image) { }
         
        // reading functions
        public PhysicsWorld2D readFromValue(JObject worldValue, PhysicsWorld2D existingWorld = NULL) { }
        public PhysicsWorld2D readFromString(string str, string& errorMsg, PhysicsWorld2D existingWorld = NULL) { }
        public PhysicsWorld2D readFromFile(string filename, string& errorMsg, PhysicsWorld2D existingWorld = NULL) { }
         
        //backward compatibility
        public bool readIntoWorldFromValue(PhysicsWorld2D existingWorld, JObject &worldValue) { return readFromValue(worldValue, existingWorld); }
        public bool readIntoWorldFromString(PhysicsWorld2D existingWorld, string str, string& errorMsg) { return readFromString(str, errorMsg, existingWorld); }
        public bool readIntoWorldFromFile(PhysicsWorld2D existingWorld, string filename, string& errorMsg) { return readFromFile(filename, errorMsg, existingWorld);



        public PhysicsWorld2D j2PhysicsWorld2D(JObject &worldValue, PhysicsWorld2D world = NULL) { }
        public RigidBody2D j2b2Body(PhysicsWorld2D world, JObject& bodyValue) { }
        public CollisionShape2D j2b2Fixture(RigidBody2D body, JObject& fixtureValue) { }
        public Constraint2D j2b2Joint(PhysicsWorld2D world, JObject& jointValue) { }
        public b2dJsonImage j2b2dJsonImage(JObject& imageValue) { }
         
        // function copies json world into existing world
        public bool j2IntoPhysicsWorld2D(PhysicsWorld2D world, JObject& worldValue) { }
         
        public int getBodiesByName(string name, List<RigidBody2D>& bodies) { }
        public int getFixturesByName(string name, List<CollisionShape2D>& fixtures) { }
        public int getJointsByName(string name, List<Constraint2D>& joints) { }
        public int getImagesByName(string name, List<b2dJsonImage>& images) { }
         
        public int getAllBodies(List<RigidBody2D>& bodies) { }
        public int getAllFixtures(List<CollisionShape2D>& fixtures) { }
        public int getAllJoints(List<Constraint2D>& joints) { }
        public int getAllImages(List<b2dJsonImage>& images) { }
         
        public RigidBody2D getBodyByName(string name) { }
        public CollisionShape2D getFixtureByName(string name) { }
        public Constraint2D getJointByName(string name) { }
        public b2dJsonImage getImageByName(string name) { }
         
        public Dictionary<Constraint2D, string> getJointToNameMap() const { return m_jointToNameMap; }
        public Dictionary<CollisionShape2D, string> getFixtureToNameMap() const { return m_fixtureToNameMap; }
         
        public string getBodyName(RigidBody2D body) { }
        public string getFixtureName(CollisionShape2D fixture) { }
        public string getJointName(Constraint2D joint) { }
        public string getImageName(b2dJsonImage img) { }





        #region [Protected]

        protected bool m_useHumanReadableFloats;
        protected Dictionary<int, RigidBody2D> m_indexToBodyMap;
        protected Dictionary<RigidBody2D, int> m_bodyToIndexMap;
        protected Dictionary<Constraint2D, int> m_jointToIndexMap;
        protected List<RigidBody2D> m_bodies;
        protected List<Constraint2D> m_joints;
        // protected List<b2dJsonImage> m_images;

        protected Dictionary<RigidBody2D, string> m_bodyToNameMap;
        protected Dictionary<CollisionShape2D, string> m_fixtureToNameMap;
        protected Dictionary<Constraint2D, string> m_jointToNameMap;
        // protected Dictionary<b2dJsonImage, string> m_imageToNameMap;

        // This maps an item (RigidBody2D, CollisionShape2D etc) to a set of custom properties.
        // Use NULL for world properties.
        protected Dictionary<void, B2dJsonCustomProperties> m_customPropertiesMap;

        // These are necessary to know what type of item the entries in the map above
        // are, which is necessary for the getBodyByCustomInt type functions.
        // We could have used a separate map for each item type, but there are many
        // combinations of item type and property type and the overall amount of
        // explicit coding to do becomes very large for no real benefit.
        protected SortedSet<RigidBody2D> m_bodiesWithCustomProperties;
        protected SortedSet<CollisionShape2D> m_fixturesWithCustomProperties;
        protected SortedSet<Constraint2D> m_jointsWithCustomProperties;
        // protected SortedSet<b2dJsonImage> m_imagesWithCustomProperties;
        protected SortedSet<PhysicsWorld2D> m_worldsWithCustomProperties;


#endregion [Protected]



////// custom properties

B2dJsonCustomProperties getCustomPropertiesForItem(void item, bool createIfNotExisting);
protected:
    void setCustomInt(void item, string propertyName, int val);
void setCustomFloat(void item, string propertyName, float val);
void setCustomString(void item, string propertyName, string val);
void setCustomVector(void item, string propertyName, b2Vec2 val);
void setCustomBool(void item, string propertyName, bool val);
void setCustomColor(void item, string propertyName, B2dJsonColor4 val);

public:
//this define saves us writing out 25 functions which are almost exactly the same
#define DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(ucType, lcType)\
    void setCustom##ucType(RigidBody2D item, string propertyName, lcType val)          { m_bodiesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
    void setCustom##ucType(CollisionShape2D item, string propertyName, lcType val)       { m_fixturesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
    void setCustom##ucType(Constraint2D item, string propertyName, lcType val)         { m_jointsWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
    void setCustom##ucType(b2dJsonImage item, string propertyName, lcType val)    { m_imagesWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }\
    void setCustom##ucType(PhysicsWorld2D item, string propertyName, lcType val)         { m_worldsWithCustomProperties.insert(item); setCustom##ucType((void)item, propertyName, val); }

    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Int, int)
    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Float, float)
    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(String, string)
    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Vector, b2Vec2)
    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Bool, bool)
    DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Color, B2dJsonColor4)

    bool hasCustomInt(void item, string propertyName);
bool hasCustomFloat(void item, string propertyName);
bool hasCustomString(void item, string propertyName);
bool hasCustomVector(void item, string propertyName);
bool hasCustomBool(void item, string propertyName);
bool hasCustomColor(void item, string propertyName);

int getCustomInt(void item, string propertyName, int defaultVal = 0);
float getCustomFloat(void item, string propertyName, float defaultVal = 0);
string getCustomString(void item, string propertyName, string defaultVal = "");
b2Vec2 getCustomVector(void item, string propertyName, b2Vec2 defaultVal = b2Vec2(0, 0));
bool getCustomBool(void item, string propertyName, bool defaultVal = false);
B2dJsonColor4 getCustomColor(void item, string propertyName, B2dJsonColor4 defaultVal = B2dJsonColor4());

//this define saves us writing out 20 functions which are almost exactly the same
#define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(ucType, lcType)\
int getBodiesByCustom##ucType(   string propertyName, lcType valueToMatch, List<RigidBody2D>& bodies);\
    int getFixturesByCustom##ucType( string propertyName, lcType valueToMatch, List<CollisionShape2D>& fixtures);\
    int getJointsByCustom##ucType(   string propertyName, lcType valueToMatch, List<Constraint2D>& joints);\
    int getImagesByCustom##ucType(   string propertyName, lcType valueToMatch, List<b2dJsonImage>& images);

    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Int, int)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Float, float)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(String, string)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Vector, b2Vec2)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Bool, bool)

//this define saves us writing out 20 functions which are almost exactly the same
#define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(ucType, lcType)\
    RigidBody2D getBodyByCustom##ucType(    string propertyName, lcType valueToMatch);\
    CollisionShape2D getFixtureByCustom##ucType( string propertyName, lcType valueToMatch);\
    Constraint2D getJointByCustom##ucType(   string propertyName, lcType valueToMatch);\
    b2dJsonImage getImageByCustom##ucType(   string propertyName, lcType valueToMatch);

    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Int, int)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Float, float)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(String, string)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Vector, b2Vec2)
    DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Bool, bool)

    //////



protected:
    //member helpers
    void vecToJson(string name, unsigned int v, JObject& value, int index = -1);
void vecToJson(string name, float v, JObject& value, int index = -1);
void vecToJson(string name, b2Vec2 vec, JObject& value, int index = -1);
void floatToJson(string name, float f, JObject& value);
RigidBody2D lookupBodyFromIndex(unsigned int index);
int lookupBodyIndex(RigidBody2D body);
int lookupJointIndex(Constraint2D joint);

JObject writeCustomPropertiesToJson(void item);
void readCustomPropertiesFromJson(RigidBody2D item, JObject value);
void readCustomPropertiesFromJson(CollisionShape2D item, JObject value);
void readCustomPropertiesFromJson(Constraint2D item, JObject value);
void readCustomPropertiesFromJson(b2dJsonImage item, JObject value);
void readCustomPropertiesFromJson(PhysicsWorld2D item, JObject value);

public:
    //static helpers
    static string floatToHex(float f);
static float hexToFloat(string str);
static float jsonToFloat(string name, JObject& value, int index = -1, float defaultValue = 0);
static b2Vec2 jsonToVec(string name, JObject& value, int index = -1, b2Vec2 defaultValue = b2Vec2(0, 0));
};

#endif // B2DJSON_H








}



