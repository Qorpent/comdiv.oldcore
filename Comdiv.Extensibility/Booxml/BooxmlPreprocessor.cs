using System;
using System.Text;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Steps;
using Comdiv.Extensions;

namespace Comdiv.Booxml
{
    public class BooxmlPreprocessor : AbstractVisitorCompilerStep
    {
        public override void Run() {
#if LIB2
            var sources = this.Parameters.Input.ToArray();
            this.Parameters.Input.Clear();
            foreach (var compilerInput in sources) {
                var val = "";
                using (var r = compilerInput.Open()) {
                    val = r.ReadToEnd();
                }
                val = val.replace(@"\$(?=[^\{])", "_QUOTED_USD_");
                var src = new StringInput(compilerInput.Name, val);
                this.Parameters.Input.Add(src);
            }
#endif
        }
    }
}
