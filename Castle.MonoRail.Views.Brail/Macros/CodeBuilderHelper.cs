// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
namespace Castle.MonoRail.Views.Brail
{
	using System.IO;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.TypeSystem;

	public class CodeBuilderHelper
	{
		public static Expression CreateCallableFromMacroBody(BooCodeBuilder builder, MacroStatement macro)
		{
			// create closure for macro's body or null
			Expression macroBody = new NullLiteralExpression();
			if (macro.Body.Statements.Count > 0)
			{
				BlockExpression callableExpr = new BlockExpression();
				callableExpr.Body = macro.Body;
				callableExpr.Parameters.Add(
					new ParameterDeclaration("OutputStream",
					                         builder.CreateTypeReference(typeof(TextWriter))));

				macroBody = callableExpr;
			}
			return macroBody;
		}
	}
}