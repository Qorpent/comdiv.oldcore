using System.Collections.Generic;
using System.IO;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using System.Linq;
using Comdiv.Extensibility.Boo.Dsl;
using Comdiv.Extensions;

namespace Comdiv.Extensibility{
    public class CommonDataReferencePipelineModifier:IBooCompilerPipelineModifier,ICompilerStep{
        private CompilerContext context;

        public void Modify(CompilerPipeline pipeline){
            
          
            pipeline.Insert(0, new CommonDataReferencePipelineModifier());
        }

        

        public int Order{
            get { return 0; }
        }

        public void Initialize(CompilerContext context){
            this.context = context;
        }

        public void Run(){
            var newinputs = context.Parameters.Input.Select(i =>
                                                            {
                                                                var reader = i.Open();
                                                                var str = reader.ReadToEnd();
                                                                str =
                                                                    str.replace(
                                                                        "(\"@(?<v>[^\"]+)@\")|(@(?<v>\\w+)@)",
                                                                        m =>
                                                                        {
                                                                            return
                                                                                string.Format(
                                                                                    "CommonData.Data[\"{0}\"]",
                                                                                    m.Groups["v"].Value);
                                                                        });
                                                                return new ReaderInput(i.Name, new StringReader(str));
                                                            }).ToList();
            context.Parameters.Input.Clear();
            newinputs.map(i=>
                context.Parameters.Input.Add(i)
                );
        }

        public void Dispose(){
           
        }
    }


    /// <summary>
    /// syg: 
    ///   commondata key,value
    ///   where
    ///       key = string | literal | didgit
    ///       value = any
    /// 
    /// usefull to collect common data of module by namespace
    /// creates static class with singleton dictionary string->object and stores key=>value in it
    /// its like vars of namespace level
    /// 
    /// for example:
    /// 
    /// namespace A.X
    /// import Comdiv.Extensibility.Boo
    /// 
    /// commondata 1,"a"
    /// commondata str,2
    /// commondata "my data", false
    /// 
    /// leads to :
    /// 
    /// namespace A.X
    /// import Comdiv.Extensibility.Boo
    /// 
    /// public static class CommonData:
    /// ...
    ///     public static Data as System.Collections.Generic.IDictionary[of string,object]:
    ///         get:
    /// ...
    /// 
    /// and this dictionary will be filled with {{"1","a"},{"str",2},{"my data",false}}
    /// 
    /// this does matter in case you're using CommonDataReferencePipelineModifier class to modify pipeline, so
    /// you can reach your common data in @name@ or "@name@" notation in your namespace to reach values
    /// in variances:
    ///          @name@ = newvalue
    ///          and
    ///          myvar  = @name@
    /// 
    /// </summary>
    /// 
    /// 
    
     

    public class CommondataMacro : AbstractAstMacro{
        private const string resultVar = "result";

        public override Statement Expand(MacroStatement macro){
            var unit = macro.GetParentNodeOfType<CompileUnit>();
            var module = macro.GetParentNodeOfType<Module>();
            var ns = module.Namespace.Name;
            

            var key = new StringLiteralExpression(macro.Arguments[0].LiftToString());
            var value = macro.Arguments[1];


            var cls = unit.FindOrCreateClass(ns, "CommonData", null, c => c.Static());
            prepareField(cls);
            preparePublicProperty(cls);
            getBuilderMethod(cls).Append(1, Variable.Ref(resultVar).ByIndex(key).Assign(value));
            return null;
        }

  

        private static Method getBuilderMethod(ClassDefinition cls){
            return cls.FindOrCreateMember<Method>("buildData",
                                                  f => f
                                                           .Private().Static().As<IDictionary<string, object>>()
                                                           .Append(Variable.Define(resultVar,
                                                                                   typeof (Dictionary<string, object>)))
                                                           .Append(Variable.Return(resultVar)));
        }

        private static void prepareField(ClassDefinition cls){
            cls.FindOrCreateMember<Field>("_data", f => f
                                                            .Static().Private().As<IDictionary<string, object>>()
                                                            .Init("buildData"));
        }

        private static void preparePublicProperty(ClassDefinition cls){
            cls.FindOrCreateMember<Property>("Data", f => f
                                                              .Static().As<IDictionary<string, object>>()
                                                              .ReturnsField("_data")
                                                              .NoSetter());
        }
    }
}