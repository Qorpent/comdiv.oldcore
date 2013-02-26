using System.Collections.Generic;
using System.Linq;
using Comdiv.Rules;
using Comdiv.Rules.Context;

namespace Comdiv.Rules.Activation{
    public class ModuleService : ITargetedModule<IRuleContext>, IModuleService{
        private IDictionary<string, bool> moduleActivations;

        public ModuleService(){
            ChangeActivation("default", true);
            Version = 0;
        }

        public IDictionary<string, bool> ModuleActivations{
            get { return moduleActivations ?? (moduleActivations = new Dictionary<string, bool>()); }
        }

        #region IModuleService Members

        public void ChangeActivation(string moduleName, bool activate){
            Version = Version + 1;
            ModuleActivations[moduleName] = activate;
        }

        public int Version { get; set; }

        public bool IsActive(string moduleName){
            if (!ModuleActivations.ContainsKey(moduleName)){
                return false;
            }
            return ModuleActivations[moduleName];
        }

        public string[] GetActiveModules(){
            var activeModules = new List<string>();
            foreach (var pair in moduleActivations){
                if (pair.Value){
                    activeModules.Add(pair.Key);
                }
            }
            return activeModules.ToArray();
        }

        #endregion

        #region ITargetedModule<IRuleContext> Members

        public IRuleContext Target { get; set; }

        #endregion
    }
}