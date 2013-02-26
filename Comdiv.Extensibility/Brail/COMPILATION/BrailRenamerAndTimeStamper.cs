using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;

namespace Comdiv.Extensibility.Brail {
    public class BrailRenamerAndTimeStamper:ICompilerStep {
        private ViewCodeSource[] sources;
        private CompilerContext _context;
        private bool staticsources;

        public BrailRenamerAndTimeStamper() {
            
        }
        public BrailRenamerAndTimeStamper(ViewCodeSource[] sources) {
            this.staticsources = true;
            this.sources = sources;
        }
        public void Dispose() {
            
        }

        public void Initialize(CompilerContext context) {
            this._context = context;
        }

        public void Run() {
            if (!staticsources) {
                sources =  _context.CompileUnit["sources"] as ViewCodeSource[];
            }
            foreach (var module in _context.CompileUnit.Modules) {
                processModule(module);
            }   
        }

        private void processModule(Module module) {
            var key = module.LexicalInfo.FileName;
            var source = sources.FirstOrDefault(x => x.Key == key);
            if (null == source) return;
            var type = module.Members[0] as ClassDefinition;
            type.Name = source.Key.Replace("/","_0_");
            module.Namespace = null;
            var attr = new Attribute();
            attr.Name = typeof (TimeStampAttribute).Name;
            attr.Arguments.Add(new StringLiteralExpression(source.LastModified.ToString("dd.MM.yyyy HH:mm:ss")));
            type.Attributes.Add(attr);

        }
    }
}