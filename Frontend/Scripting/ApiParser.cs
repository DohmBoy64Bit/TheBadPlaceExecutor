using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

namespace Frontend.Scripting
{
    public class ApiParser
    {
        public Dictionary<string, PolytoriaClass> Classes { get; private set; } = new Dictionary<string, PolytoriaClass>();
        public Dictionary<string, List<string>> ClassHierarchy { get; private set; } = new Dictionary<string, List<string>>();

        public bool LoadFromFile(string xmlPath)
        {
            try
            {
                if (!File.Exists(xmlPath)) return false;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                XmlNodeList? xmlNodeList = xmlDocument.SelectNodes("//Type");
                if (xmlNodeList == null)
                {
                    return false;
                }

                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    PolytoriaClass? polytoriaClass = ParseClass(xmlNode);
                    if (polytoriaClass != null && !string.IsNullOrEmpty(polytoriaClass.Name))
                    {
                        Classes[polytoriaClass.Name] = polytoriaClass;
                    }
                }
                BuildInheritanceTree();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse API XML: " + ex.Message);
                return false;
            }
        }

        private PolytoriaClass? ParseClass(XmlNode typeNode)
        {
            XmlAttributeCollection? attributes = typeNode.Attributes;
            if (attributes == null) return null;

            PolytoriaClass polytoriaClass = new PolytoriaClass();
            XmlAttribute? nameAttr = attributes["Name"];
            polytoriaClass.Name = nameAttr?.Value?.Replace("Proxy", "") ?? string.Empty;
            
            XmlAttribute? fullNameAttr = attributes["FullName"];
            polytoriaClass.FullName = fullNameAttr?.Value ?? string.Empty;
            
            XmlAttribute? inheritsAttr = attributes["InheritsFrom"];
            polytoriaClass.InheritsFrom = inheritsAttr?.Value ?? string.Empty;

            XmlNodeList? propNodes = typeNode.SelectNodes("Property");
            if (propNodes != null)
            {
                foreach (XmlNode node in propNodes)
                {
                    XmlAttributeCollection? propAttrs = node.Attributes;
                    if (propAttrs != null)
                    {
                        Property property = new Property();
                        property.Name = propAttrs["Name"]?.Value ?? string.Empty;
                        property.Type = SimplifyType(propAttrs["Type"]?.Value ?? string.Empty);
                        property.CanRead = (propAttrs["CanRead"]?.Value ?? "") == "true";
                        property.CanWrite = (propAttrs["CanWrite"]?.Value ?? "") == "true";
                        polytoriaClass.Properties.Add(property);
                    }
                }
            }

            XmlNodeList? methodNodes = typeNode.SelectNodes("Method");
            if (methodNodes != null)
            {
                foreach (XmlNode node in methodNodes)
                {
                    XmlAttributeCollection? methodAttrs = node.Attributes;
                    if (methodAttrs != null)
                    {
                        Method method = new Method();
                        method.Name = methodAttrs["Name"]?.Value ?? string.Empty;
                        method.ReturnType = SimplifyType(methodAttrs["ReturnType"]?.Value ?? string.Empty);
                        
                        XmlNodeList? paramNodes = node.SelectNodes("Parameter");
                        if (paramNodes != null)
                        {
                            foreach (XmlNode pNode in paramNodes)
                            {
                                XmlAttributeCollection? pAttrs = pNode.Attributes;
                                if (pAttrs != null)
                                {
                                    Parameter parameter = new Parameter();
                                    parameter.Name = pAttrs["Name"]?.Value ?? string.Empty;
                                    parameter.Type = SimplifyType(pAttrs["Type"]?.Value ?? string.Empty);
                                    method.Parameters.Add(parameter);
                                }
                            }
                        }
                        polytoriaClass.Methods.Add(method);
                    }
                }
            }

            foreach (Property property in polytoriaClass.Properties.Where(p => p.Type == "LuaEvent"))
            {
                polytoriaClass.Events.Add(new Event
                {
                    Name = property.Name,
                    Type = "LuaEvent"
                });
            }

            return polytoriaClass;
        }

        private string SimplifyType(string fullType)
        {
            if (string.IsNullOrEmpty(fullType)) return "void";
            if (fullType.Contains("."))
            {
                string[] array = fullType.Split('.', StringSplitOptions.None);
                string text = array[array.Length - 1];
                if (text.Contains("`"))
                {
                    text = text.Substring(0, text.IndexOf('`'));
                }
                return text;
            }
            return fullType;
        }

        private void BuildInheritanceTree()
        {
            foreach (var kvp in Classes)
            {
                string key = kvp.Key;
                PolytoriaClass polytoriaClass = kvp.Value;
                ClassHierarchy[key] = new List<string>();
                
                PolytoriaClass current = polytoriaClass;
                while (!string.IsNullOrEmpty(current.InheritsFrom))
                {
                    string inheritsFrom = current.InheritsFrom;
                    string? text = inheritsFrom.Split('.').LastOrDefault()?.Replace("Proxy", "");
                    
                    if (string.IsNullOrEmpty(text) || !Classes.ContainsKey(text))
                    {
                        break;
                    }
                    ClassHierarchy[key].Add(text);
                    current = Classes[text];
                }
            }
        }

        public List<Property> GetAllProperties(string className)
        {
            List<Property> list = new List<Property>();
            if (Classes.ContainsKey(className))
            {
                list.AddRange(Classes[className].Properties);
                if (ClassHierarchy.ContainsKey(className))
                {
                    foreach (string text in ClassHierarchy[className])
                    {
                        if (Classes.ContainsKey(text))
                        {
                            list.AddRange(Classes[text].Properties);
                        }
                    }
                }
            }
            return list.GroupBy(p => p.Name).Select(g => g.First()).ToList();
        }

        public List<Method> GetAllMethods(string className)
        {
            List<Method> list = new List<Method>();
            if (Classes.ContainsKey(className))
            {
                list.AddRange(Classes[className].Methods);
                if (ClassHierarchy.ContainsKey(className))
                {
                    foreach (string text in ClassHierarchy[className])
                    {
                        if (Classes.ContainsKey(text))
                        {
                            list.AddRange(Classes[text].Methods);
                        }
                    }
                }
            }
            return list;
        }

        public List<Event> GetAllEvents(string className)
        {
            List<Event> list = new List<Event>();
            if (Classes.ContainsKey(className))
            {
                list.AddRange(Classes[className].Events);
                if (ClassHierarchy.ContainsKey(className))
                {
                    foreach (string text in ClassHierarchy[className])
                    {
                        if (Classes.ContainsKey(text))
                        {
                            list.AddRange(Classes[text].Events);
                        }
                    }
                }
            }
            return list.GroupBy(e => e.Name).Select(g => g.First()).ToList();
        }
    }
}
