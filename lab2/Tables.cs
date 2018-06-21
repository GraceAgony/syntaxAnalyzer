using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace lab2
{
    public class Tables
    {
        public Table _simpleDelimitersTable;
        public Table _multiCharacterDelimitersTable;
        public Table _keyWordsTable;
        public Table _constantsTable;
        public Table _identifiersTable;

        public Tables(Table simpleDelimitersTable, Table multiCharacterDelimitersTable, Table keyWordsTable, Table constantsTable, Table identifiersTable)
        {
            _simpleDelimitersTable = simpleDelimitersTable;
            _multiCharacterDelimitersTable = multiCharacterDelimitersTable;
            _keyWordsTable = keyWordsTable;
            _constantsTable = constantsTable;
            _identifiersTable = identifiersTable;
        }


    }
}
