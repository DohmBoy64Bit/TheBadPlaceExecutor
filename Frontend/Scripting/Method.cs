using System.Collections.Generic;

namespace Frontend.Scripting
{
    public class Method
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}
