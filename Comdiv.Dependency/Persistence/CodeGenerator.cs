using System;
using System.Collections.Generic;
using Comdiv.Application;
using Comdiv.Extensions;
using Comdiv.Logging;
using Comdiv.Model.Interfaces;
using Comdiv.Persistence;

namespace Comdiv.Model
{
    public class CodeGenerator
    {
        public CodeGenerator()
        {

            Format = "~{sname}";
        }

        public string Format { get; set; }




        public void CheckCode(object target)
        {
            var index = 1;
            var basecode = target.Name();
            if (basecode.noContent())
            {
                basecode = "NONAME";
            }
            var code = basecode;
            using (new TemporarySimpleSession()){
                while (myapp.storage.Get(target.GetType()).Exists(target.GetType(), code, "")){
                    code = basecode + "_" + index++;
                }
            }

            target.setCode(code);

        }
    }
}