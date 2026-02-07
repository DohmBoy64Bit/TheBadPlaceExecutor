using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Frontend.Scripting
{
    public class AutocompleteGenerator
    {
        public static string GeneratePolytoriaCompletions(ApiParser parser)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// Polytoria API Autocomplete - Generated from ReflectionData.xml");
            sb.AppendLine("var polytoriaCompletions = {");
            sb.AppendLine("    // Global Objects");
            sb.AppendLine("    game: { meta: 'global', type: 'Environment', caption: 'game', snippet: 'game', docHTML: 'The game environment (equivalent to workspace)' },");
            sb.AppendLine("    workspace: { meta: 'global', type: 'Environment', caption: 'workspace', snippet: 'workspace', docHTML: 'The game environment (alias for game)' },");
            sb.AppendLine("    script: { meta: 'global', type: 'BaseScript', caption: 'script', snippet: 'script', docHTML: 'Reference to the current script' },");
            sb.AppendLine("    Vector3: { meta: 'class', caption: 'Vector3', snippet: 'Vector3', docHTML: 'Vector3 class for 3D vectors' },");
            sb.AppendLine("    Vector2: { meta: 'class', caption: 'Vector2', snippet: 'Vector2', docHTML: 'Vector2 class for 2D vectors' },");
            sb.AppendLine("    Vector4: { meta: 'class', caption: 'Vector4', snippet: 'Vector4', docHTML: 'Vector4 class for 4D vectors' },");
            sb.AppendLine("    Color3: { meta: 'class', caption: 'Color3', snippet: 'Color3', docHTML: 'Color3 class for RGB colors' },");
            sb.AppendLine("    Quaternion: { meta: 'class', caption: 'Quaternion', snippet: 'Quaternion', docHTML: 'Quaternion class for rotations' },");
            sb.AppendLine("    Bounds: { meta: 'class', caption: 'Bounds', snippet: 'Bounds', docHTML: 'Bounds class for bounding boxes' },");
            sb.AppendLine("    JSON: { meta: 'class', caption: 'JSON', snippet: 'JSON', docHTML: 'JSON encoding/decoding' },");
            sb.AppendLine("    // Custom Executor Functions");
            sb.AppendLine("    readfile: { meta: 'function', caption: 'readfile', snippet: 'readfile(${1:path})', docHTML: 'Read file contents from workspace folder' },");
            sb.AppendLine("    writefile: { meta: 'function', caption: 'writefile', snippet: 'writefile(${1:path}, ${2:content})', docHTML: 'Write content to a file in workspace folder' },");
            sb.AppendLine("    appendfile: { meta: 'function', caption: 'appendfile', snippet: 'appendfile(${1:path}, ${2:content})', docHTML: 'Append content to a file' },");
            sb.AppendLine("    isfile: { meta: 'function', caption: 'isfile', snippet: 'isfile(${1:path})', docHTML: 'Check if path is a file' },");
            sb.AppendLine("    isfolder: { meta: 'function', caption: 'isfolder', snippet: 'isfolder(${1:path})', docHTML: 'Check if path is a folder' },");
            sb.AppendLine("    makefolder: { meta: 'function', caption: 'makefolder', snippet: 'makefolder(${1:path})', docHTML: 'Create a folder' },");
            sb.AppendLine("    delfile: { meta: 'function', caption: 'delfile', snippet: 'delfile(${1:path})', docHTML: 'Delete a file' },");
            sb.AppendLine("    delfolder: { meta: 'function', caption: 'delfolder', snippet: 'delfolder(${1:path})', docHTML: 'Delete a folder' },");
            sb.AppendLine("    listfiles: { meta: 'function', caption: 'listfiles', snippet: 'listfiles(${1:path})', docHTML: 'List files in a folder' },");
            sb.AppendLine("    getclipboard: { meta: 'function', caption: 'getclipboard', snippet: 'getclipboard()', docHTML: 'Get clipboard contents' },");
            sb.AppendLine("    setclipboard: { meta: 'function', caption: 'setclipboard', snippet: 'setclipboard(${1:text})', docHTML: 'Set clipboard contents' },");
            sb.AppendLine("    toclipboard: { meta: 'function', caption: 'toclipboard', snippet: 'toclipboard(${1:text})', docHTML: 'Alias for setclipboard' },");
            sb.AppendLine("};");
            sb.AppendLine();
            sb.AppendLine("// Class members for autocomplete");
            sb.AppendLine("var classMembers = {");
            
            bool firstClass = true;
            foreach (var kvp in parser.Classes)
            {
                string className = kvp.Key;
                PolytoriaClass cls = kvp.Value;
                
                if (!firstClass) sb.Append(",");
                sb.AppendLine($"    '{className}': [");
                
                List<string> members = new List<string>();
                
                foreach (var prop in cls.Properties)
                {
                    string rw = (prop.CanRead && prop.CanWrite) ? "read/write" : (prop.CanRead ? "read-only" : "write-only");
                    members.Add($"{{ meta: 'property', caption: '{prop.Name}', value: '{prop.Name}', type: '{prop.Type}', docHTML: '{rw} property of type {prop.Type}' }}");
                }
                
                foreach (var method in cls.Methods)
                {
                    string paramsStr = string.Join(", ", method.Parameters.Select(p => $"{p.Name}: {p.Type}"));
                    string snippet = method.Name + "(";
                    if (method.Parameters.Count > 0)
                    {
                        var paramSnippets = method.Parameters.Select((p, i) => $"${{{i + 1}:{p.Name}}}");
                        snippet += string.Join(", ", paramSnippets);
                    }
                    snippet += ")";
                    
                    members.Add($"{{ meta: 'method', caption: '{method.Name}', value: '{method.Name}', snippet: '{snippet}', type: '{method.ReturnType}', docHTML: 'Method ({paramsStr}): {method.ReturnType}' }}");
                }
                
                foreach (var ev in cls.Events)
                {
                    members.Add($"{{ meta: 'event', caption: '{ev.Name}', value: '{ev.Name}', docHTML: 'Event - use {ev.Name}:Connect(function() end)' }}");
                }
                
                sb.AppendLine("        " + string.Join(",\n        ", members));
                sb.AppendLine("    ]");
                firstClass = false;
            }
            
            sb.AppendLine("};");
            return sb.ToString();
        }

        public static string GenerateHighlightConfig(ApiParser parser)
        {
            var classes = parser.Classes.Keys.ToList();
            var functions = new List<string> { 
                "readfile", "writefile", "appendfile", "isfile", "isfolder", 
                "makefolder", "delfile", "delfolder", "listfiles", 
                "getclipboard", "setclipboard", "toclipboard" 
            };
            var constants = new List<string> { "game", "workspace", "script" };

            var config = new
            {
                keywords = string.Join("|", classes),
                functions = string.Join("|", functions),
                constants = string.Join("|", constants)
            };

            return System.Text.Json.JsonSerializer.Serialize(config);
        }
    }
}
