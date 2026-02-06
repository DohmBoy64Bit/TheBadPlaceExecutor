using System;
using System.Collections.Generic;

namespace Frontend.Scripting
{
    public class PolytoriaClass
    {
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string InheritsFrom { get; set; } = string.Empty;
        public List<Property> Properties { get; set; } = new List<Property>();
        public List<Method> Methods { get; set; } = new List<Method>();
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
