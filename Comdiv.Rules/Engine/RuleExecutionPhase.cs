using System;


namespace Comdiv.Rules.Engine{
    [Flags]
    public enum RuleExecutionPhase{
        None = 0,
        InitContext = 1,
        PreExpertRun = 2,
        PreStep = 4,
        PreTest = 8,
        Test = 16,
        PostTest = 32,
        PreRun = 64,
        Run = 128,
        PostRun = 256,
        PostStep = 512,
        PostExpertRun = 1024
    }
}