using System;
using System.Collections.Generic;
using System.Reflection;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Comdiv.Logging;
using Module = Boo.Lang.Compiler.Ast.Module;

namespace Comdiv.Extensibility{
    public class SetupDefaultNamespacesAndReferencesStep : AbstractTransformerCompilerStep{
        private readonly IEnumerable<string> namespaces;
        private readonly IEnumerable<Assembly> references;

        public SetupDefaultNamespacesAndReferencesStep(IEnumerable<string> namespaces,IEnumerable<Assembly> references){
            this.namespaces = namespaces;
            this.references = references;
        }

        public override void Run(){
            logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep started");
            Console.WriteLine("SetupDefaultNamespacesAndReferencesStep started");
            
            Visit(CompileUnit);
            if (null != references)
            {
                foreach (var assembly in references)
                {
                    Context.Parameters.References.Add(assembly);
                    logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep on {0} - {1} ref added", Context.Parameters.OutputAssembly, assembly.GetName().Name);
                    Console.WriteLine("SetupDefaultNamespacesAndReferencesStep on {0} - {1} ref added", Context.Parameters.OutputAssembly, assembly.GetName().Name);
                }
            }
            logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep finished");
            Console.WriteLine("SetupDefaultNamespacesAndReferencesStep finished");
        }

        public override void OnModule(Module node){
            logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep on {0} started", node.Name);
            Console.WriteLine("SetupDefaultNamespacesAndReferencesStep on {0} started", node.Name);
            if(null==node.Namespace){
                node.Namespace = new NamespaceDeclaration(System.IO.Path.GetFileNameWithoutExtension(Context.Parameters.OutputAssembly));
            }
            if (null != namespaces){
                foreach (var ns in namespaces){
                    var import = new Import();
                    import.Namespace = ns;
                    node.Imports.Add(import);
                    logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep on {0} - {1} ns added", node.Name, ns);
                    Console.WriteLine("SetupDefaultNamespacesAndReferencesStep on {0} - {1} ns added", node.Name, ns);
                }
            }

            logger.get("comdiv.dsl").Debug("SetupDefaultNamespacesAndReferencesStep on {0} finished", node.Name);
        }
    }
}