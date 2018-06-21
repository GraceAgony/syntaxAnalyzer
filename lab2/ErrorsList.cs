
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mime;
using Newtonsoft.Json;

namespace lab2
{
    class ErrorsList : IEnumerable
    {
        public class Error
        {
            public int Line { get; set; }
            public int Column { get; set; }
            public string Text { get; set; }
            public string Symbol { get; set; }
        }

        public string File;

        public ErrorsList(string file)
        {
            File = file;
        }

        public List<Error> Errors = new List<Error>();

        public void Add(int line, int column, string text, string symbol, Tree tree)
        {
            Errors.Add(new Error { Line = line, Column = column, Text = text, Symbol = symbol });
            Result.Write(tree, this, this.File, Program.SyntaxAnalyzer.jsonFile);
            System.Environment.Exit(0);
        }

        public void Add(string text, Tree tree)
        {
            Errors.Add(new Error{Text = text});
            Result.Write(tree, this, this.File, Program.SyntaxAnalyzer.jsonFile);
            System.Environment.Exit(0);
        }

        public void AddToken(Table.Token currentToken, Dictionary<Table.Token, int> locationNumber, string text, Tree tree)
        {
            this.Add(currentToken.Locations[locationNumber[currentToken]].Line,
                currentToken.Locations[locationNumber[currentToken]].Column,
                text, currentToken.Name, tree);
        }

        public IEnumerator GetEnumerator()
        {
            return Errors.GetEnumerator();
        }
    }
}
