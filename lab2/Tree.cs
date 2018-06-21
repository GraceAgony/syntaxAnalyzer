using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Eventing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace lab2
{
     public class Tree
    {
        public TreeNode Root;

        public Tree()
        {}


        public Tree(string file)
        {
            Root = new TreeNode("<signal-program>", null);
            if (!System.IO.File.Exists(file))
            {
                using (System.IO.File.CreateText(file))
                {
                }
            }
            File.WriteAllText(file, String.Empty);
        }

        public TreeNode Add(string value, TreeNode parent)
        {
            var newNode = new TreeNode(value, parent);
            parent.AddToChildren(newNode);
            return newNode;
        }

        public static void Write(TreeNode root, int deep, string file)
        {
            if (root != null)
            {
               
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(string.Concat(Enumerable.Repeat(".", deep)) + root.Name);
                }

                foreach (object node in root.ChildrenNodes)
                {
                    if (node.GetType() == typeof(TreeNode))
                    {
                        Write((TreeNode)node, deep + 2, file);
                    }
                    else
                    {
                        
                        using (StreamWriter sw = File.AppendText(file))
                        {
                            sw.WriteLine(string.Concat(Enumerable.Repeat(".", deep + 2)) + node);
                        }
                    }
                }
            }
        }

        public static Tree Read(string file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                var json = sr.ReadToEnd();
                Tree tree = JsonConvert.DeserializeObject<Tree>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                return tree;
            }
        }

        public class TreeNode
        {
            public string Name;
            public TreeNode ParentNode;
            public List<object> ChildrenNodes = new List<object>();
            

            public TreeNode(string name, TreeNode parent)
            {
                Name = name;
                ParentNode = parent;
                      
            }

            public void AddToChildren(object child)
            {
                ChildrenNodes.Add(child);
            }
 

        }
    }
}
