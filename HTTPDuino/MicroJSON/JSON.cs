using System;
using System.Text;
using Microsoft.SPOT;

namespace HTTPDuino.MicroJSON
{
    /// <summary>
    /// Represents a JSON message, can serialize itself and deserialize a string
    /// </summary>
    public class JSON : IDisposable
    {

        private System.Collections.ArrayList JSONValues;
        private System.Collections.ArrayList JSONValuesTypes;
        private System.Collections.ArrayList JSONNames;

        private int JSONEntityCounter;

        /// <summary>
        /// Creates an empty JSON
        /// </summary>
        public JSON()
        {
            //initialize the new storage system for json entities
            this.JSONNames = new System.Collections.ArrayList();
            this.JSONValues = new System.Collections.ArrayList();
            this.JSONValuesTypes = new System.Collections.ArrayList();
            JSONEntityCounter = 0;
        }

        /// <summary>
        /// Creates the JSON deserializing a JSON encoded as a string
        /// </summary>
        /// <param name="jsonMessage">the JSON string-encoded</param>
        public JSON(string jsonMessage)
        {
            //initialize the new storage system for json entities
            this.JSONNames = new System.Collections.ArrayList();
            this.JSONValues = new System.Collections.ArrayList();
            this.JSONValuesTypes = new System.Collections.ArrayList();
            JSONEntityCounter = 0;

            //what is the lexer reading?
            int reading = 0;
            /************************
             *   0 => Nothing       *
             *   1 => Name          *
             *   2 => String        *
             *   3 => Number        *
             *   4 => Boolean       *
             *   5 => JSON          *
             *   6 => After Name    *
             ***********************/

            System.Text.StringBuilder temp = new StringBuilder();
            string nameOfEntity = string.Empty;
            bool symbolFound = false;

            //read every character
            for (int i = 0; i < jsonMessage.Length; i++)
                if (reading == 0)
                {
                    if (jsonMessage[i] == '\"')
                    {
                        reading = 1;
                        temp.Clear();
                    }
                    else if (jsonMessage[i] != ' ')
                    {
                        throw new Exception("Unexpected character in a JSON message");
                    }
                }
                else if (reading == 1)
                {
                    if (jsonMessage[i] == '\"')
                    {
                        nameOfEntity = temp.ToString();
                        reading = 6;
                        symbolFound = false;
                    }
                    else
                    {
                        temp.Append(jsonMessage[i]);
                    }
                }
                else if (reading == 6)
                {
                    if (jsonMessage[i] == '=')
                    {
                        symbolFound = true;
                    }
                    else if (jsonMessage[i] == '\"')
                    {
                        if (symbolFound)
                        {
                            temp.Clear();
                            reading = 2;
                        }
                        else
                        {
                            throw new Exception("Unexpected beginning of entity string");
                        }
                    }
                    else if (jsonMessage[i] == '{')
                    {
                        if (symbolFound)
                        {
                            temp.Clear();
                            reading = 5;
                        }
                        else
                        {
                            throw new Exception("Unexpected beginning of entity JSON");
                        }
                    }
                    else if ((jsonMessage[i] == '-') || (jsonMessage[i] == '+') || (jsonMessage[i] == '0') || (jsonMessage[i] == '1') || (jsonMessage[i] == '2') || (jsonMessage[i] == '3') || (jsonMessage[i] == '4') || (jsonMessage[i] == '5') || (jsonMessage[i] == '6') || (jsonMessage[i] == '7') || (jsonMessage[i] == '8') || (jsonMessage[i] == '9'))
                    {
                        if (symbolFound)
                        {
                            temp.Clear();
                            temp.Append(jsonMessage[i]);
                            reading = 3;
                        }
                        else
                        {
                            throw new Exception("Unexpected beginning of entity number");
                        }
                    }
                    else if ((jsonMessage[i] == 't') || (jsonMessage[i] == 'f'))
                    {
                        if (symbolFound)
                        {
                            temp.Clear();
                            temp.Append(jsonMessage[i]);
                            reading = 4;
                        }
                        else
                        {
                            throw new Exception("Unexpected beginning of entity boolean");
                        }
                    }
                }
        }

        /// <summary>
        /// Serializes this JSON. The result is a valiid string
        /// </summary>
        /// <returns>this JSON encoded as a string</returns>
        public string Serialize()
        {
            //the JSON encoded as a string
            string stringJSON = string.Empty;

            //each JSON message strarts with a '{'
            stringJSON += "{ ";

            //encode each JSON entity
            for (int i = 0; i < this.JSONNames.Count; i++)
            {
                //add the name of the current JSON entity to the current JSON message
                stringJSON += "\"" + (string)this.JSONNames[i] + "\": ";

                //check the type of the value of the current JSON entity
                if ((JSONValueType)this.JSONValuesTypes[i] == JSONValueType.JSON)
                {
                    HTTPDuino.MicroJSON.JSON subJSON = (HTTPDuino.MicroJSON.JSON)this.JSONValues[i];
                    stringJSON += subJSON.Serialize();
                }
                else if ((JSONValueType)this.JSONValuesTypes[i] == JSONValueType.JSONString)
                {
                    stringJSON += "\"" + (string)this.JSONValues[i] + "\"";
                }
                else if ((JSONValueType)this.JSONValuesTypes[i] == JSONValueType.JSONNumber)
                {
                    stringJSON += this.JSONValues[i].ToString();
                }
                else if ((JSONValueType)this.JSONValuesTypes[i] == JSONValueType.JSONBoolean)
                {
                    if ((System.Boolean)this.JSONValues[i])
                        stringJSON += "true";
                    else
                        stringJSON += "false";
                }

                if (i != (this.JSONNames.Count - 1))
                    stringJSON += ", ";
            }

            //each JSON message ends with a '}'
            stringJSON += " }";

            //return the encoded JSON
            return stringJSON;
        }

        /// <summary>
        /// Restores the JSON entity value from the JSON entity with the given name
        /// </summary>
        /// <param name="name">the name of the JSON entity</param>
        /// <returns>the value of the JSON entity, null if it isn't found</returns>
        public object getEntityValue(string name)
        {
            //the object will be changed if the entity exists
            object entityValue = null;

            //look for a JSON entity with the given name
            for (int j = 0; j < this.JSONEntityCounter; j++)
                //check each entity if it has the right name
                if (string.Compare((string)this.JSONNames[j], name) == 0)
                {
                    entityValue = this.JSONValues[j];
                    break;
                }

            //return the object that IS the entity value
            return entityValue;
        }

        /// <summary>
        /// Stores a single JSON entity
        /// </summary>
        /// <param name="entityName">the name of the JSON entity</param>
        /// <param name="value">the value of the JSON entity</param>
        public void AppendEntity(string entityName, object value)
        {
            //get the type of the json entity
            Type typeOfEntity = value.GetType();

            //is the type supported?
            bool validType = true;

            //store the type of the entity
            switch (typeOfEntity.FullName)
            {
                case "System.String":
                    this.JSONValuesTypes.Add(HTTPDuino.MicroJSON.JSON.JSONValueType.JSONString);
                    break;

                case "System.Uint16":
                case "System.Int16":
                case "System.Uint32":
                case "System.Int32":
                case "System.Uint64":
                case "System.Int64":
                case "System.Float":
                case "System.Double":
                    this.JSONValuesTypes.Add(HTTPDuino.MicroJSON.JSON.JSONValueType.JSONNumber);
                    break;

                case "System.Boolean":
                    this.JSONValuesTypes.Add(HTTPDuino.MicroJSON.JSON.JSONValueType.JSONBoolean);
                    break;

                case "HTTPDuino.MicroJSON.JSON":
                    this.JSONValuesTypes.Add(HTTPDuino.MicroJSON.JSON.JSONValueType.JSON);
                    break;

                default:
                    validType = false;
                    this.JSONValuesTypes.Add(HTTPDuino.MicroJSON.JSON.JSONValueType.Unknown);
                    break;
            }
                
            //store the entity name
            this.JSONNames.Add(entityName);

            //store the entity as is
            this.JSONValues.Add(value);

            //update the number of entity stored
            this.JSONEntityCounter++;

            //throw an exception
            if (!validType)
                throw new Exception("Unsupported type for a JSON entity");
        }

        /// <summary>
        /// Identify the type of a JSON value
        /// </summary>
        private enum JSONValueType
        {
            JSONString,
            JSONBoolean,
            JSONNumber,
            JSON,
            Unknown
        }

        #region IDisposable Members
        ~JSON()
        {
            this.Dispose();
        }

        /// <summary>
        /// Deletes the memory used to store the JSON entities
        /// </summary>
        public void Dispose()
        {
            //delete every JSON entity
            this.JSONNames.Clear();
            this.JSONValues.Clear();
            this.JSONValuesTypes.Clear();

            //force the garbage collector to free more memory as it can
            Microsoft.SPOT.Debug.GC(true);
        }
        #endregion

    }
}
