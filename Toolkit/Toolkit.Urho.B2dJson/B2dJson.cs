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


        protected bool m_useHumanReadableFloats;
        protected std::map<int, b2Body*> m_indexToBodyMap;
        protected std::map<b2Body*, int> m_bodyToIndexMap;
        protected std::map<b2Joint*, int> m_jointToIndexMap;
        protected std::vector<b2Body*> m_bodies;
        protected std::vector<b2Joint*> m_joints;
        protected std::vector<b2dJsonImage*> m_images;

        protected std::map<b2Body*, std::string> m_bodyToNameMap;
        protected std::map<b2Fixture*, std::string> m_fixtureToNameMap;
        protected std::map<b2Joint*, std::string> m_jointToNameMap;
        protected std::map<b2dJsonImage*, std::string> m_imageToNameMap;

        protected std::map<b2Body*, std::string> m_bodyToPathMap;
        protected std::map<b2Fixture*, std::string> m_fixtureToPathMap;
        protected std::map<b2Joint*, std::string> m_jointToPathMap;
        protected std::map<b2dJsonImage*, std::string> m_imageToPathMap;

        // This maps an item (b2Body*, b2Fixture* etc) to a set of custom properties.
        // Use NULL for world properties.
        protected std::map<void*, b2dJsonCustomProperties*> m_customPropertiesMap;

        // These are necessary to know what type of item the entries in the map above
        // are, which is necessary for the getBodyByCustomInt type functions.
        // We could have used a separate map for each item type, but there are many
        // combinations of item type and property type and the overall amount of
        // explicit coding to do becomes very large for no real benefit.
        protected std::set<b2Body*> m_bodiesWithCustomProperties;
        protected std::set<b2Fixture*> m_fixturesWithCustomProperties;
        protected std::set<b2Joint*> m_jointsWithCustomProperties;
        protected std::set<b2dJsonImage*> m_imagesWithCustomProperties;
        protected std::set<b2World*> m_worldsWithCustomProperties;


        //constructor
        public b2dJon(bool useHumanReadableFloats = false);


        public void clear();

        //writing functions
        public Json::Value writeToValue(b2World* world);
        public std::string writeToString(b2World* world);
        public bool writeToFile(b2World* world, const char* filename);

        public Json::Value b2j(b2World* world);
        public Json::Value b2j(b2Body* body);
        public Json::Value b2j(b2Fixture* fixture);
        public Json::Value b2j(b2Joint* joint);
        public Json::Value b2j(b2dJsonImage* image);

        public void setBodyName(b2Body* body, const char* name);
        public void setFixtureName(b2Fixture* fixture, const char* name);
        public void setJointName(b2Joint* joint, const char* name);
        public void setImageName(b2dJsonImage* image, const char* name);

        public void setBodyPath(b2Body* body, const char* path);
        public void setFixturePath(b2Fixture* fixture, const char* path);
        public void setJointPath(b2Joint* joint, const char* path);
        public void setImagePath(b2dJsonImage* image, const char* path);

        public void addImage(b2dJsonImage* image);

        //reading functions
        public b2World* readFromValue(Json::Value worldValue, b2World* existingWorld = NULL);
        public b2World* readFromString(std::string str, std::string& errorMsg, b2World* existingWorld = NULL);
        public b2World* readFromFile(const char* filename, std::string& errorMsg, b2World* existingWorld = NULL);

        //backward compatibility
        public bool readIntoWorldFromValue(b2World* existingWorld, Json::Value &worldValue) { return readFromValue(worldValue, existingWorld); }
        public bool readIntoWorldFromString(b2World* existingWorld, std::string str, std::string& errorMsg) { return readFromString(str, errorMsg, existingWorld); }
        public bool readIntoWorldFromFile(b2World* existingWorld, const char* filename, std::string& errorMsg) { return readFromFile(filename, errorMsg, existingWorld); }

        public b2World* j2b2_World(Json::Value &worldValue, b2World* world = NULL);
        public b2Body* j2b2_Body(b2World* world, Json::Value& bodyValue);
        public b2Fixture* j2b2_Fixture(b2Body* body, Json::Value& fixtureValue);
        public b2Joint* j2b2_Joint(b2World* world, Json::Value& jointValue);
        public b2dJsonImage* j2b2d_JsonImage(Json::Value& imageValue);

        public int getBodiesByName(std::string name, std::vector<b2Body*>& bodies);
        public int getFixturesByName(std::string name, std::vector<b2Fixture*>& fixtures);
        public int getJointsByName(std::string name, std::vector<b2Joint*>& joints);
        public int getImagesByName(std::string name, std::vector<b2dJsonImage*>& images);

        public int getBodiesByPath(std::string path, std::vector<b2Body*>& bodies);
        public int getFixturesByPath(std::string path, std::vector<b2Fixture*>& fixtures);
        public int getJointsByPath(std::string path, std::vector<b2Joint*>& joints);
        public int getImagesByPath(std::string path, std::vector<b2dJsonImage*>& images);

        public int getAllBodies(std::vector<b2Body*>& bodies);
        public int getAllFixtures(std::vector<b2Fixture*>& fixtures);
        public int getAllJoints(std::vector<b2Joint*>& joints);
        public int getAllImages(std::vector<b2dJsonImage*>& images);

        public b2Body* getBodyByName(std::string name);
        public b2Fixture* getFixtureByName(std::string name);
        public b2Joint* getJointByName(std::string name);
        public b2dJsonImage* getImageByName(std::string name);

        public b2Body* getBodyByPathAndName(std::string path, std::string name);
        public b2Fixture* getFixtureByPathAndName(std::string path, std::string name);
        public b2Joint* getJointByPathAndName(std::string path, std::string name);
        public b2dJsonImage* getImageByPathAndName(std::string path, std::string name);

        public std::map<b2Joint*, std::string> getJointToNameMap() const { return m_jointToNameMap; }
        public std::map<b2Fixture*, std::string> getFixtureToNameMap() const { return m_fixtureToNameMap; }

        public std::string getBodyName(b2Body* body);
        public std::string getFixtureName(b2Fixture* fixture);
        public std::string getJointName(b2Joint* joint);
        public std::string getImageName(b2dJsonImage* img);

        public std::string getBodyPath(b2Body* body);
        public std::string getFixturePath(b2Fixture* fixture);
        public std::string getJointPath(b2Joint* joint);
        public std::string getImagePath(b2dJsonImage* img);

        ////// custom properties

        public b2dJsonCustomProperties* getCustomPropertiesForItem(void* item, bool createIfNotExisting);

        protected void setCustomInt(void* item, std::string propertyName, int val);
        protected void setCustomFloat(void* item, std::string propertyName, float val);
        protected void setCustomString(void* item, std::string propertyName, std::string val);
        protected void setCustomVector(void* item, std::string propertyName, b2Vec2 val);
        protected void setCustomBool(void* item, std::string propertyName, bool val);
        protected void setCustomColor(void* item, std::string propertyName, b2dJsonColor4 val);


        // //this define saves us writing out 25 functions which are almost exactly the same
        // #define DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(ucType, lcType)\
        //     void setCustom##ucType(b2Body* item, std::string propertyName, lcType val)          { m_bodiesWithCustomProperties.insert(item); setCustom##ucType((void*)item, propertyName, val); }\
        //     void setCustom##ucType(b2Fixture* item, std::string propertyName, lcType val)       { m_fixturesWithCustomProperties.insert(item); setCustom##ucType((void*)item, propertyName, val); }\
        //     void setCustom##ucType(b2Joint* item, std::string propertyName, lcType val)         { m_jointsWithCustomProperties.insert(item); setCustom##ucType((void*)item, propertyName, val); }\
        //     void setCustom##ucType(b2dJsonImage* item, std::string propertyName, lcType val)    { m_imagesWithCustomProperties.insert(item); setCustom##ucType((void*)item, propertyName, val); }\
        //     void setCustom##ucType(b2World* item, std::string propertyName, lcType val)         { m_worldsWithCustomProperties.insert(item); setCustom##ucType((void*)item, propertyName, val); }

        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Int, int)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Float, float)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(String, std::string)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Vector, b2Vec2)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Bool, bool)
        //     DECLARE_SET_CUSTOM_PROPERTY_VALUE_FUNCTIONS(Color, b2dJsonColor4)

        public bool hasCustomInt(void* item, std::string propertyName);
        public bool hasCustomFloat(void* item, std::string propertyName);
        public bool hasCustomString(void* item, std::string propertyName);
        public bool hasCustomVector(void* item, std::string propertyName);
        public bool hasCustomBool(void* item, std::string propertyName);
        public bool hasCustomColor(void* item, std::string propertyName);

        public int getCustomInt(void* item, std::string propertyName, int defaultVal = 0);
        public float getCustomFloat(void* item, std::string propertyName, float defaultVal = 0);
        public std::string getCustomString(void* item, std::string propertyName, std::string defaultVal = "");
        public b2Vec2 getCustomVector(void* item, std::string propertyName, b2Vec2 defaultVal = b2Vec2(0, 0));
        public bool getCustomBool(void* item, std::string propertyName, bool defaultVal = false);
        public b2dJsonColor4 getCustomColor(void* item, std::string propertyName, b2dJsonColor4 defaultVal = b2dJsonColor4());

        // //this define saves us writing out 20 functions which are almost exactly the same
        // #define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(ucType, lcType)\
        // int getBodiesByCustom##ucType(   std::string propertyName, lcType valueToMatch, std::vector<b2Body*>& bodies);\
        //     int getFixturesByCustom##ucType( std::string propertyName, lcType valueToMatch, std::vector<b2Fixture*>& fixtures);\
        //     int getJointsByCustom##ucType(   std::string propertyName, lcType valueToMatch, std::vector<b2Joint*>& joints);\
        //     int getImagesByCustom##ucType(   std::string propertyName, lcType valueToMatch, std::vector<b2dJsonImage*>& images);

        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Int, int)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Float, float)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(String, std::string)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Vector, b2Vec2)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_VECTOR(Bool, bool)

        // //this define saves us writing out 20 functions which are almost exactly the same
        // #define DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(ucType, lcType)\
        //     b2Body* getBodyByCustom##ucType(    std::string propertyName, lcType valueToMatch);\
        //     b2Fixture* getFixtureByCustom##ucType( std::string propertyName, lcType valueToMatch);\
        //     b2Joint* getJointByCustom##ucType(   std::string propertyName, lcType valueToMatch);\
        //     b2dJsonImage* getImageByCustom##ucType(   std::string propertyName, lcType valueToMatch);

        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Int, int)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Float, float)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(String, std::string)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Vector, b2Vec2)
        //     DECLARE_GET_BY_CUSTOM_PROPERTY_VALUE_FUNCTIONS_SINGLE(Bool, bool)

        //////




        //member helpers
        protectedvoid vecToJson(const char* name, unsigned int v, Json::Value& value, int index = -1);
        protectedvoid vecToJson(const char* name, float v, Json::Value& value, int index = -1);
        protectedvoid vecToJson(const char* name, b2Vec2 vec, Json::Value& value, int index = -1);
        protectedvoid floatToJson(const char* name, float f, Json::Value& value);
        protectedb2Body* lookupBodyFromIndex(unsigned int index);
        protectedint lookupBodyIndex(b2Body* body);
        protectedint lookupJointIndex(b2Joint* joint);

        protectedJson::Value writeCustomPropertiesToJson(void* item);
        protectedvoid readCustomPropertiesFromJson(b2Body* item, Json::Value value);
        protectedvoid readCustomPropertiesFromJson(b2Fixture* item, Json::Value value);
        protectedvoid readCustomPropertiesFromJson(b2Joint* item, Json::Value value);
        protectedvoid readCustomPropertiesFromJson(b2dJsonImage* item, Json::Value value);
        protectedvoid readCustomPropertiesFromJson(b2World* item, Json::Value value);


        //static helpers
        public static std::string floatToHex(float f);
        public static float hexToFloat(std::string str);
        public static float jsonToFloat(const char* name, Json::Value& value, int index = -1, float defaultValue = 0);
        public static b2Vec2 jsonToVec(const char* name, Json::Value& value, int index = -1, b2Vec2 defaultValue = b2Vec2(0, 0));

    }

}



