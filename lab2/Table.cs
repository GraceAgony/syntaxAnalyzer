using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace lab2
{
    public class Table
    {
        public string Name;
        public Dictionary<string, Token> Tokens;
       
        public class Location
        {
            public int Line { get; set; }
            public int Column { get; set; }
        }


        public class Token
        {
            public string Name { get; set; }
            public int Code { get; set; }
            public List<Location> Locations = new List<Location>();

        }


        public Table(int displacement, string name, string codesFile)
        {
            Tokens = new Dictionary<string, Token>();
            Name = name;
        }
       
    }




}
