using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace lab2
{
    class Result
    {
        public static void Write(Tree tree, ErrorsList Errors, string file, string jsonFile)
        {
            Tree.Write(tree.Root, 0, file);
            var json = JsonConvert.SerializeObject(tree, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });
            using (StreamWriter sw = new StreamWriter(jsonFile))
            {
                sw.Write(json);
            }
            if (Errors != null)
            {
                foreach(ErrorsList.Error error in Errors) {
                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine($"Parser: Error (line {error.Line}, column {error.Column}): {error.Text}");
                    }
                }
            }
        }
    }
}
