using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules.Context;

namespace Comdiv.Rules{
    public class RuleConfigurable : RuleTemplated{
        private ExecModule execModule;
        private InitContextModule initModule;
        private TestModule testModule;

        public RuleConfigurable() {}

        public RuleConfigurable(RuleTestDelegate testDelegate){
            TestDelegate = testDelegate;
        }

        public RuleConfigurable(RuleExecuteDelegate execDelegate){
            ExecDelegate = execDelegate;
        }

        public RuleConfigurable(RuleTestDelegate testDelegate, RuleExecuteDelegate execDelegate){
            TestDelegate = testDelegate;
            ExecDelegate = execDelegate;
        }

        public RuleConfigurable(RuleTestDelegate testDelegate, RuleExecuteDelegate execDelegate, TestModule testModule,
                                ExecModule execModule){
            TestDelegate = testDelegate;
            ExecDelegate = execDelegate;
            TestModule = testModule;
            ExecModule = execModule;
        }

        public RuleConfigurable(RuleTestDelegate testDelegate, RuleExecuteDelegate execDelegate, TestModule testModule,
                                ExecModule execModule, RuleInitContextDelefate initDelegate,
                                InitContextModule initModule){
            TestDelegate = testDelegate;
            ExecDelegate = execDelegate;
            TestModule = testModule;
            ExecModule = execModule;
            InitDelegate = initDelegate;
            InitModule = initModule;
        }

        public RuleTestDelegate TestDelegate { get; set; }

        public RuleExecuteDelegate ExecDelegate { get; set; }

        public TestModule TestModule{
            get { return testModule; }
            set{
                testModule = value;
                if (testModule != null){
                    testModule.Target = this;
                }
            }
        }

        public ExecModule ExecModule{
            get { return execModule; }
            set{
                execModule = value;
                if (execModule != null){
                    execModule.Target = this;
                }
            }
        }

        public RuleInitContextDelefate InitDelegate { get; set; }

        public InitContextModule InitModule{
            get { return initModule; }
            set{
                initModule = value;
                if (initModule != null){
                    initModule.Target = this;
                }
            }
        }

        protected override void innerInitContext(IRuleContext context){
            if (null != InitDelegate){
                InitDelegate(this, context);
                return;
            }
            if (null != InitModule){
                InitModule.InitContext(context);
                return;
            }
            base.innerInitContext(context);
        }

        protected override void innerExecute(IRuleContext context){
            if (null != ExecDelegate){
                ExecDelegate(this, context);
                return;
            }
            if (null != ExecModule){
                ExecModule.Execute(context);
                return;
            }
            base.innerExecute(context);
        }

        protected override bool innerTest(IRuleContext context){
            if (null != TestDelegate){
                return TestDelegate(this, context);
            }
            if (null != TestModule){
                return TestModule.Test(context);
            }
            return base.innerTest(context);
        }
    }
}