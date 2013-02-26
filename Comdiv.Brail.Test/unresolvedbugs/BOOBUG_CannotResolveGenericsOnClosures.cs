using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using NUnit.Framework;

namespace unresolvedbugs
{
	public class generichostclass {
			public T call<T>(Func<T> func) {
				return func();
			}
		}
		public abstract class baseclass {
			public generichostclass gh = new generichostclass();
			public abstract object call();
		}
	[TestFixture]
	public class BOOBUG_CannotResolveGenericsOnClosures {
		private void testcompilation(string code)
		{
			var compiler = new BooCompiler();
			compiler.Parameters.Pipeline = new CompileToMemory();
			compiler.Parameters.References.Add(typeof(baseclass).Assembly);
			compiler.Parameters.Input.Add(new StringInput("main", code));
			var result = compiler.Run();
			Assert.AreEqual(0, result.Errors.Count,result.Errors.ToString());
			Assert.NotNull(result.GeneratedAssembly);
			var type = result.GeneratedAssembly.GetType("myclass");
			var inst = (baseclass)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			Assert.AreEqual("test", inst.call());
		}


		public string expected_code = @"
import unresolvedbugs
public class myclass(baseclass) :
	public override def call () as object :
		return gh.call({'test'})";

		public string avoid_code = @"
import unresolvedbugs
public class myclass(baseclass) :
	public override def call () as object :
		return gh.call[of string]({'test'})";

		[Test]
		[Ignore]
		public void bug_cannot_compile_expected_code() {
			testcompilation(expected_code);
		}
		[Test]
		public void avoid_can_compile()
		{
			testcompilation(avoid_code);
		}
		
	}
}
