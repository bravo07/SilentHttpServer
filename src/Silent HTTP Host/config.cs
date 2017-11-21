using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host
{
    public struct Config_Parent
    {
        public string name;
        public List<Config_Child> nodes;
    }

    public enum Config_Type : byte
    {
        STRING = 0x01,
        INTEGER = 0x02,
        LONG = 0x03,
        FLOAT = 0x04,
    }

    public struct Config_Child
    {
        public string encoding;
        public Config_Type type;
        public string name;
        public string value;
    }

    static class Config
    {
        /*
         * This is an example format of a saved config file
         * 
         * Note:
         *  This is based off of a child and parrnet based algo.
         *  Why? Well you can scan for the parent then the child,
         *  This will make searching through variables a lot faster.
         *  It won't be a lot faster, but if you have a lot of
         *  variables it will be a fairly big improvement.
         * 
         * Keywords(characters):
         *  ">" This is to inticate a variable.
         *  "[" (Closed by ']') Is to indicate a parent.
         *  "#" This is a comment. All comments are closed by a new line. Comments must be the first character on each line, if they're not it will more then likely result in a syntax error.
         *  
         * 
         * # This is a comment
         * [parent]
         * >child_name(encoding.type: value
         * >child_name(encoding.type: value
         * 
         * [parent_2]
         * #UTF8 encoded string.
         * >child_name(UTF8.s: value
         */

        /// <summary>
        /// The file location that the config will be saved to
        /// </summary>
        private static string filePath = Misc.ParseFileLocation("{CURRENT_DIR}\\config");

        /// <summary>
        /// The config data that's been loaded into memory.
        /// </summary>
        private static List<Config_Parent> m_config;

        #region Load & Save
        /// <summary>
        /// Loads the config from the saved file
        /// </summary>
        public static void LoadConfig(string path = null)
        {
            // Creating a new config instance, if it's invalid.
            if (m_config == null)
                m_config = new List<Config_Parent>();

            if (path != null)
                filePath = Misc.ParseFileLocation(path);

            // Making sure the file exists before we start reading it. The last thing we want is unhandled exceptions.
            if (!File.Exists(filePath))
                return;

            // Reading the contents of the file.
            string[] configContent = File.ReadAllLines(filePath);

            // The current parent name.
            string currentParent = "main";

            // Temp varaible used for scanning for keywords and shit. Just ignore it.
            int tmp = 0;

            for (int i = 0; i < configContent.Length; i++)
            {
                // Line is empty
                if (configContent[i].Length == 0)
                    continue;

                // Commented line
                if (configContent[i][0] == '#')
                    continue;

                // Start of a parent
                if (configContent[i][0] == '[')
                {
                    // Finding the end of the parent
                    tmp = configContent[i].IndexOf(']');

                    // Making sure that the enclosing parent name is found.
                    if (tmp == -1)
                        continue;

                    // Getting setting the name of the current parent
                    currentParent = configContent[i].Substring(1, tmp - 1);
                }

                // Variable of a parent
                if (configContent[i][0] == '>')
                {
                    // The minimum size of a variable.
                    if (configContent[i].Length <= 8)
                        continue;

                    // Declaring names of variables
                    string name;
                    string encoding;
                    string type;
                    string value;

                    // Getting the indexes of the variable
                    int encoding_start = configContent[i].IndexOf('(');
                    int type_start = configContent[i].IndexOf('.', encoding_start);
                    int value_start = configContent[i].IndexOf(':', type_start + 1);

                    // Parsing the syntax
                    name = configContent[i].Substring(1, encoding_start - 1);
                    encoding = configContent[i].Substring(encoding_start + 1, type_start - encoding_start - 1);
                    type = configContent[i].Substring(type_start + 1, 1);
                    value = configContent[i].Substring(value_start + 1);

                    // And inserting the new variable.
                    ManualInsert(currentParent, name, value, encoding, GetTypeByName(type));
                }
            }
        }

        /// <summary>
        /// Saves the current config to your HDD using the file location saved torwards the top of this file.
        /// </summary>
        public static void Save()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Opening the file strea,
            StreamWriter streamWriter = File.AppendText(filePath);

            // Clearing the file content
            streamWriter.Flush();

            // Looping through all the parents
            foreach (Config_Parent cp in m_config)
            {
                // Writing the parent to file
                streamWriter.Write(string.Format("[{0}]\n", cp.name));

                // Writing all the child's
                foreach (Config_Child cc in cp.nodes)
                    streamWriter.Write(string.Format(">{0}({1}.{2}:{3}\n", cc.name, cc.encoding, cc.type.GetNameByType(), cc.value));

                // Let's just append a few lines to make it look a little nicer.
                streamWriter.Write("\n\n");
            }

            // Saved the config to the config. Now let's cleanup.
            streamWriter.Close();
            streamWriter.Dispose();
        }
        #endregion
        #region Reading variables
        public static bool Exists(string parent, string name)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads a saed string
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="name">The name of the child variable</param>
        /// <returns></returns>
        public static string ReadString(string parent, string name)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            if (m_config[i].nodes[j].type != Config_Type.STRING)
                            {
                                throw new InvalidCastException(string.Format("{0}:{1} Is not a string", parent, name));
                            }

                            return m_config[i].nodes[j].value;
                        }
                    }

                    throw new KeyNotFoundException("Child not found");
                }
            }

            throw new KeyNotFoundException("Parent not found");
        }

        /// <summary>
        /// Reads a saved integer
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="name">The name of the child variable</param>
        /// <returns></returns>
        public static int ReadInteger(string parent, string name)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            if (m_config[i].nodes[j].type != Config_Type.INTEGER)
                            {
                                throw new InvalidCastException(string.Format("{0}:{1} Is not an integer", parent, name));
                            }

                            return int.Parse(m_config[i].nodes[j].value);
                        }
                    }

                    throw new KeyNotFoundException("Child not found");
                }
            }

            throw new KeyNotFoundException("Parent not found");
        }

        /// <summary>
        /// Reads a saved long
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="name">The name of the child variable</param>
        /// <returns></returns>
        public static long ReadLong(string parent, string name)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            if (m_config[i].nodes[j].type != Config_Type.LONG)
                            {
                                throw new InvalidCastException(string.Format("{0}:{1} Is not a long", parent, name));
                            }

                            return long.Parse(m_config[i].nodes[j].value);
                        }
                    }

                    throw new KeyNotFoundException("Child not found");
                }
            }

            throw new KeyNotFoundException("Parent not found");
        }

        /// <summary>
        /// Reads a saved float
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="name">The name of the child variable</param>
        /// <returns></returns>
        public static float ReadFloat(string parent, string name)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            if (m_config[i].nodes[j].type != Config_Type.FLOAT)
                            {
                                throw new InvalidCastException(string.Format("{0}:{1} Is not a float", parent, name));
                            }

                            return float.Parse(m_config[i].nodes[j].value);
                        }
                    }

                    throw new KeyNotFoundException("Child not found");
                }
            }

            throw new KeyNotFoundException("Parent not found");
        }
        #endregion
        #region Insert/Update variables
        /// <summary>
        /// Sets a string variable
        /// </summary>
        /// <param name="parent">Parent variable name</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value</param>
        public static void SetString(string parent, string name, string value)
        {
            ManualInsert(parent, name, value, "UTF8", Config_Type.STRING);
        }

        /// <summary>
        /// Sets an integer variable
        /// </summary>
        /// <param name="parent">Parent variable name</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value</param>
        public static void SetInt(string parent, string name, int value)
        {
            ManualInsert(parent, name, value.ToString(), "0", Config_Type.INTEGER);
        }

        /// <summary>
        /// Sets a long variable
        /// </summary>
        /// <param name="parent">Parent variable name</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value</param>
        public static void SetLong(string parent, string name, long value)
        {
            ManualInsert(parent, name, value.ToString(), "0", Config_Type.LONG);
        }

        /// <summary>
        /// Sets a float variable
        /// </summary>
        /// <param name="parent">Parent variable name</param>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value</param>
        public static void SetFloat(string parent, string name, float value)
        {
            ManualInsert(parent, name, value.ToString(), "0", Config_Type.FLOAT);
        }
        #endregion
        #region Private types
        #region Type-related functions
        /// <summary>
        /// Converts a Config Type string to an actual type.
        /// </summary>
        /// <param name="name">The name of the type you wish to convery</param>
        private static Config_Type GetTypeByName(string name)
        {
            name = name.ToLower();
            if (name == "s")
                return Config_Type.STRING;
            else if (name == "i")
                return Config_Type.INTEGER;
            else if (name == "l")
                return Config_Type.LONG;
            else if (name == "f")
                return Config_Type.FLOAT;
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Converts a config type to a string, This is mainly used for when saving the config as a local file.
        /// </summary>
        /// <param name="type">The config type you're converting to a string</param>
        private static string GetNameByType(this Config_Type type)
        {
            if (type == Config_Type.FLOAT)
                return "f";
            if (type == Config_Type.INTEGER)
                return "i";
            if (type == Config_Type.LONG)
                return "l";
            if (type == Config_Type.STRING)
                return "s";
            throw new KeyNotFoundException();
        }
        #endregion


        private static void ManualInsert(string parent, string name, string value, string encoding, Config_Type type)
        {
            parent = CleanName(parent);
            name = CleanName(name);

            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    for (int j = 0; j < m_config[i].nodes.Count; j++)
                    {
                        if (m_config[i].nodes[j].name == name)
                        {
                            // Fucking lists, not allowing me to edit a value -.-
                            m_config[i].nodes[j] = new Config_Child()
                            {
                                encoding = encoding,
                                value = value,
                                name = name,
                                type = type
                            };

                            // Updating existing, let's return
                            return;
                        }
                    }

                    // Variable doesn't exist, but parrrent does. Let's insert to parent.
                    m_config[i].nodes.Add(new Config_Child()
                    {
                        encoding = encoding,
                        value = value,
                        name = name,
                        type = type
                    });


                    // Returning.
                    return;
                }
            }


            // Parent doesn't exist, let's insert one.
            InsertParent(parent);

            // Now we need to find the newly inserted parent.
            for (int i = 0; i < m_config.Count; i++)
            {
                if (m_config[i].name == parent)
                {
                    m_config[i].nodes.Add(new Config_Child()
                    {
                        encoding = encoding,
                        value = value,
                        name = name,
                        type = type
                    });
                }
            }
        }

        /// <summary>
        /// Creates a new parent node
        /// </summary>
        /// <param name="name">The name of the parent node</param>
        private static void InsertParent(string name)
        {
            m_config.Add(new Config_Parent()
            {
                name = name,
                nodes = new List<Config_Child>()
            });
        }

        /// <summary>
        /// Cleans the name of a variable. Basically removes any non-alphanumeric characters.
        /// </summary>
        /// <param name="name">The name you want to clean</param>
        /// <returns>Filtered name</returns>
        private static string CleanName(string name)
        {
            string validChars = "abcdefghijklmnopqrstuvwxyz1234567890";
            string ret = null;

            for (int i = 0; i < name.Length; i++)
                for (int j = 0; j < validChars.Length; j++)
                    if (name[i] == validChars[j])
                        ret += name[i];

            return ret;
        }
        #endregion
    }
}
