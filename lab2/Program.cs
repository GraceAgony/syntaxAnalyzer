using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using lab1;

namespace lab2
{
    public class Program
    {
        public class SyntaxAnalyzer
        {
            public const int SimpleDelimitersOffset = 0;
            public const int MultiCharacterDelimitersOffset = 300;
            public const int KeyWordsOffset = 400;
            public const int ConstantsOffset = 500;
            public const int IdentifiersOffset = 1000;

            public string codesFile = @"D:\labs\ipz\lab1Files\codesFile.csv";
            public string tablesFile = @"D:\labs\ipz\lab1Files\result.json";
            public static string resultFile = @"D:\labs\ipz\lab2Files\result.txt";
            public static string jsonFile = @"D:\labs\ipz\lab2Files\tree.json";

            private Table _simpleDelimitersTable;
            private Table _multiCharacterDelimitersTable;
            private Table _keyWordsTable;
            private Table _constantsTable;
            private Table _identifiersTable;
            private Tables tables;

            private List<string> codes = new List<string>();
            private ErrorsList Errors = new ErrorsList(resultFile);

            private int codePosition = 0;
            private Table.Token currentToken;

            private Tree tree = new Tree(resultFile);
            public Dictionary<Table.Token, int> locationNumber = new Dictionary<Table.Token, int>();

            private void ReadFiles()
            {
                using (StreamReader sr = new StreamReader(tablesFile))
                {
                    tables = JsonConvert.DeserializeObject<Tables>(sr.ReadToEnd());
                    _simpleDelimitersTable = tables._simpleDelimitersTable;
                    _multiCharacterDelimitersTable = tables._multiCharacterDelimitersTable;
                    _keyWordsTable = tables._keyWordsTable;
                    _constantsTable = tables._constantsTable;
                    _identifiersTable = tables._identifiersTable;
                }

                using (StreamReader sr = new StreamReader(codesFile))
                {
                    codes.AddRange(sr.ReadToEnd().Split(separator: ',')); 
                    codes.RemoveAt(codes.Count - 1);
                }
            }

            private Table.Token GetNext()
            {
                int.TryParse(codes[codePosition], out var next);
                if (codePosition > codes.Count - 1) return null;
                codePosition++;
                return SearchTokenInTables(next);
            }

            public SyntaxAnalyzer()
            {

                ReadFiles();
                SignalProgram();
                Result.Write(tree, null, resultFile, jsonFile);
            }

            public Table.Token SearchTokenInTables(int code)
            {
                Table.Token token;
                if (code < 300)
                {
                    token = GetToken(_simpleDelimitersTable, code);
                }
                else if (code < 400)
                {
                    token = GetToken(_multiCharacterDelimitersTable, code);
                }
                else if (code < 500)
                {
                    token = GetToken(_keyWordsTable, code);
                }
                else if (code < 1000)
                {
                    token = GetToken(_constantsTable, code);
                }
                else
                {
                    token = GetToken(_identifiersTable, code);
                }

                return token;
            }

            private void IncrementLocation(Table.Token token)
            {
                if (token == null)
                {
                    Errors.Add($"unexpected end of file", tree);
                }
                if (locationNumber.ContainsKey(token))
                {
                    locationNumber[token]++;
                    return;
                }

                locationNumber.Add(token, 0);
            }

            private Table.Token GetToken(Table table, int code)
            {
                return table.Tokens.FirstOrDefault(token => token.Value.Code == code).Value;
            }

            private void AddError(int line, int column, string text, string symbol)
            {
                Errors.Add(line, column, text, symbol, tree);
            }



            private bool SignalProgram()
            {
                currentToken = GetNext();
                if (currentToken != null)
                {
                    IncrementLocation(currentToken);
                    return ProgramProc();
                }

                return true;
            }

            private bool ProgramProc()
            {
                var newNode = tree.Add("<program>", tree.Root);
                if (currentToken.Name == "PROCEDURE")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    ProcedureIdentifier(newNode);
                    ParametersList(newNode);
                    if (currentToken.Name == ";")
                    {
                        newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                        Block(newNode);
                        if (currentToken.Name == ";")
                        {
                            newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                            return true;
                        }
                        else
                        {
                            Errors.AddToken(currentToken, locationNumber,
                                $"';' expected but {currentToken.Name} found", tree);
                            return false;
                        }
                    }
                    else
                    {
                        Errors.AddToken(currentToken, locationNumber,
                            $"';' expected but {currentToken.Name} found", tree);
                        return false;
                    }

                }
                else
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"'PROCEDURE' expected but {currentToken.Name} found", tree);
                    return false;
                }


            }

            
            private bool Block(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<block>", parent);
                Declarations(newNode);
                if (currentToken.Name == "BEGIN")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    StatementsList(newNode);
                    if (currentToken.Name == "END")
                    {
                        newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                        currentToken = GetNext();
                        IncrementLocation(currentToken);
                        return true;
                    }
                    else
                    {
                        Errors.AddToken(currentToken, locationNumber,
                            $"'END' expected but {currentToken.Name} found", tree);
                        return false;
                    }
                }
                else
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"'BEGIN' expected but {currentToken.Name} found", tree);
                    return false;
                }
            }

            private bool ParametersList(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<parameters-list>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if (currentToken.Name == ";")
                {
                    newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
                    return true;
                }

                if (currentToken.Name == "(")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    currentToken = GetNext();
                    IncrementLocation(currentToken);
                    DeclarationList(newNode);
                    if (currentToken.Name != ")")
                    {
                        Errors.AddToken(currentToken, locationNumber,
                            $"')' expected but {currentToken.Name} found", tree);
                        return false;
                    }
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    currentToken = GetNext();
                    IncrementLocation(currentToken);
                    return true;
                }
                else
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"'(' expected but {currentToken.Name} found", tree);
                    return false;
                }

            }


            private bool DeclarationList(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<declaration-list>", parent);
                if ((currentToken.Name == ")")|| (currentToken.Name == ";") || (currentToken.Name == "ELSE"))
                {
                    newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
                    return true;
                }

                while ((currentToken.Name != ")")&&(currentToken.Name != ";")&&(currentToken.Name != "ELSE"))
                {
                    Declaration(newNode);
                }
                    
               return true;
               
            }

            private bool StatementsList(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<statements-list>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                //if (!ConditionStatement(newNode))
                //{
                    newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
                //}

                //currentToken = GetNext();
                //IncrementLocation(currentToken);
                return true;
            }

            //private bool ConditionStatement(Tree.TreeNode parent)
            //{
            //    if (currentToken.Name == "IF")
            //    {
            //        var newNode = tree.Add("<condition-statement>", parent);
            //        LeftCondition(newNode);
            //        RightCondition(newNode);
            //        if (currentToken.Name == ";")
            //        {
            //            newNode.AddToChildren(new Tree.TreeNode(currentToken.Name, newNode));
            //        }
            //        else
            //        {
            //            Errors.AddToken(currentToken, locationNumber,
            //                $"';' expected but {currentToken.Name} found", tree);
            //            return false;
            //        }
            //        return true;
            //    }

            //    return false;
            //}

            //private bool LeftCondition(Tree.TreeNode parent)
            //{
            //    var newNode = tree.Add("<left-condition>", parent);
            //    newNode.AddToChildren(new Tree.TreeNode(currentToken.Name, newNode));
            //    currentToken = GetNext();
            //    IncrementLocation(currentToken);
            //    Constant(newNode);
            //    currentToken = GetNext();
            //    IncrementLocation(currentToken);
            //    if (currentToken.Name == "THEN")
            //    {
            //        newNode.AddToChildren(new Tree.TreeNode(currentToken.Name, newNode));
            //        currentToken = GetNext();
            //        IncrementLocation(currentToken);
            //        DeclarationList(newNode);
            //    }
            //    else
            //    {
            //        Errors.AddToken(currentToken, locationNumber,
            //            $"'THEN' expected but {currentToken.Name} found", tree);
            //        return false;
            //    }

            //    return true;
            //}

            //private bool RightCondition(Tree.TreeNode parent)
            //{
            //    var newNode = tree.Add("<right-condition>", parent);
            //    if (currentToken.Name == ";")
            //    {
            //        newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
            //    }
            //    else
            //    {
            //        if (currentToken.Name == "ELSE")
            //        {
            //            newNode.AddToChildren(new Tree.TreeNode(currentToken.Name, newNode));
            //            currentToken = GetNext();
            //            IncrementLocation(currentToken);
            //            DeclarationList(newNode);
            //        }
            //        else
            //        {
            //            Errors.AddToken(currentToken, locationNumber,
            //                $"'ELSE' expected but {currentToken.Name} found", tree);
            //            return false;
            //        }
            //    }
            //    return true;
            //}


            private bool Declaration(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<declaration>", parent);
                VariableIdentifier(newNode);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if (currentToken.Name == ":")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    AttributeProc(newNode);
                    if (currentToken.Name != ";")
                    {
                        Errors.AddToken(currentToken, locationNumber,
                            $"';' expected but {currentToken.Name} found", tree);
                        return false;
                    }
                    else
                    {
                        newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                        currentToken = GetNext();
                        IncrementLocation(currentToken);
                        return true;
                    }
                }
                else
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"':' expected but {currentToken.Name} found", tree);
                    return false;
                }

            }

            private bool AttributeProc(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<attribute>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if ((currentToken.Name != "INTEGER") && (currentToken.Name != "FLOAT"))
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"'INTEGER' or 'FLOAT' expected but {currentToken.Name} found", tree);
                    return false;
                }
                newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                return true;
            }

            private bool Declarations(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<declarations>", parent);
                return ConstantDeclarations(newNode);
            }

            private bool ConstantDeclarations(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<constant-declarations>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if (currentToken.Name == "CONST")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    return ConstantDeclarationsList(newNode);
                }
                newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
                return true;
            }

            private bool ConstantDeclarationsList(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<constant-declarations-list>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if (!ConstantDeclaration(newNode))
                {
                    currentToken = GetNext();
                    IncrementLocation(currentToken);
                    newNode.AddToChildren(new Tree.TreeNode("<empty>", newNode));
                    return true;
                }

                while (currentToken.Name != "BEGIN")
                {
                    ConstantDeclaration(newNode);
                }

                return true;
            }

            private bool ConstantDeclaration(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<constant-declaration>", parent);
                ConstantIdentifier(newNode);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                if (currentToken.Name == "=")
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    currentToken = GetNext();
                    IncrementLocation(currentToken);
                    Constant(newNode);
                    currentToken = GetNext();
                    IncrementLocation(currentToken);
                    if (currentToken.Name == ";")
                    {
                        newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                        currentToken = GetNext();
                        IncrementLocation(currentToken);
                        return true;
                    }
                    else
                    {
                        Errors.AddToken(currentToken, locationNumber,
                            $"';' expected but {currentToken.Name} found", tree);
                        return false;
                    }
                }
                else
                {
                    Errors.AddToken(currentToken, locationNumber,
                        $"'=' expected but {currentToken.Name} found", tree);
                    return false;
                }
            }

            private bool Constant(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<constant>", parent);
                if (_constantsTable.Tokens.ContainsKey(currentToken.Name))
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    return true;
                }
                return false;
            }


            private bool ConstantIdentifier(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<constant-identifier>", parent);
                return Identifier(newNode);
            }


            private bool VariableIdentifier(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<variable-identifier>", parent);
                return Identifier(newNode);
            }


            private bool ProcedureIdentifier(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<procedure-identifier>", parent);
                currentToken = GetNext();
                IncrementLocation(currentToken);
                return Identifier(newNode);
            }


            private bool Identifier(Tree.TreeNode parent)
            {
                var newNode = tree.Add("<identifier>", parent);
                if (_identifiersTable.Tokens.ContainsKey(currentToken.Name))
                {
                    newNode.AddToChildren(currentToken.Code + " " + currentToken.Name);
                    return true;
                }

                return false;
            }
        }


        static void Main()
        {
           lab1.Program.Entry("D:/labs/ipz/lab1Files/test3.txt");
           new SyntaxAnalyzer();
        }

        public static void Entry(string file = "D:/labs/ipz/lab1Files/test3.txt")
        {
            lab1.Program.Entry(file);
            new SyntaxAnalyzer();
        }
    }
}
