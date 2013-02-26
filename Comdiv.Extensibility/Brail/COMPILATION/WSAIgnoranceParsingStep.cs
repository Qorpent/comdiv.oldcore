using System;
using System.IO;
using antlr;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Parser;

namespace Comdiv.Extensibility.Brail
{
    public class WSAIgnoranceParsingStep:ICompilerStep
    {
        private CompilerContext _context;

        public  void Run() {
            foreach (var input in _context.Parameters.Input)
            {
                try {
                    bool wsa = false;
                    bool isduck = true;
                    string code = "";
                    using (var reader = input.Open()) {
                        code = reader.ReadToEnd();
                        wsa = WSAHelper.IsWSA(code);
                        isduck = WSAHelper.IsDuck(code);
                    }
                    var _input = new StringInput(input.Name, code);

                    using (var reader = _input.Open()) {
                      var module =   ParseModule(wsa, input.Name, reader, OnParserError);
                        module["isduck"] = isduck;
                        module["iswsa"] = wsa;
                    }
                }
                catch (CompilerError error)
                {
                    _context.Errors.Add(error);
                }
                catch (antlr.TokenStreamRecognitionException x)
                {
                    OnParserError(x.recog);
                }
                catch (Exception x)
                {
                    _context.Errors.Add(CompilerErrorFactory.InputError(input.Name, x));
                }
            }
        }


        private Module ParseModule(bool wsa, string name, TextReader reader, ParserErrorHandler onParserError)
        {
            if (wsa) {
               return WSABooParser.ParseModule(4, _context.CompileUnit, name, reader, onParserError);
            }else {
               return BooParser.ParseModule(4, _context.CompileUnit, name, reader, onParserError);
            }
        }

        public void Dispose() {
            
        }

        void OnParserError(antlr.RecognitionException error)
        {
            var location = new LexicalInfo(error.getFilename(), error.getLine(), error.getColumn());
            var nvae = error as antlr.NoViableAltException;
            if (null != nvae)
                ParserError(location, nvae);
            else
                GenericParserError(location, error);
        }

        private void GenericParserError(LexicalInfo data, RecognitionException error)
        {
            _context.Errors.Add(CompilerErrorFactory.GenericParserError(data, error));
        }

        void ParserError(LexicalInfo data, antlr.NoViableAltException error)
        {
            _context.Errors.Add(CompilerErrorFactory.UnexpectedToken(data, error, error.token.getText()));
        }

        public void Initialize(CompilerContext context) {
            this._context = context;
        }
    }
}
