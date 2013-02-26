using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Application;
using Comdiv.MAS;

namespace masaudit
{
 
        public class Program : MasConsoleApplication
        {
            public Program()
            {
                this.CanIgnoreMas = true;
                this.IgnoreMasByDefault = false;

            }

            protected override void initialize()
            {
                base.initialize();
              
            }

            static void Main(string[] args)
            {
                new Program().Run(args);
            }

            protected override void execute()
            {
                var providers = myapp.storage.Get<TestProvider>().WithSystem("mas").All().ToArray();
                foreach (var provider in providers)
                {
                    loginfo("try to call provider "+provider.Code);
                    try
                    {
                        provider.Execute(null, null, 0, this);
                        loginfo("end call provider " + provider.Code);    
                    }catch(Exception e)
                    {
                        logerror(e.Message,"error in provider",provider.Code);
                    }
                    
                }
            }
        }
    
}
