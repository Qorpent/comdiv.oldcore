using Comdiv.Rules;
using Comdiv.Rules.Context;
using Comdiv.Rules.Support;

namespace Comdiv.Rules{
    public abstract class RuleBase : IRule, IWithServices{
        private IServicesContainer _services;
        private IParametersProvider<IRule> meta;

        public string Uid{
            get { return this.Uid(); }
            set{
                //@"value".contract_HasContent(value);
                this.SetUid(value);
            }
        }

        public string Module{
            get { return this.Module(); }
            set{
                //@"value".contract_HasContent(value);
                this.SetModule(value);
            }
        }

        #region IRule Members

        public IRule Parent { get; set; }

        public IParametersProvider<IRule> Params{
            get { return meta ?? (meta = new RulesPartametersContainer(this)); }
            set { meta = value; }
        }

        public abstract bool Test(IRuleContext context);

        public abstract void Execute(IRuleContext context);

        #endregion

        #region IWithServices Members

        public virtual IServicesContainer Services{
            get{
                if (_services == null){
                    if (Parent == null || !(Parent is IWithServices))
                        return null;
                    else
                        return ((IWithServices) Parent).Services;
                }
                return _services;
            }
            set { _services = value; }
        }

        #endregion

        public void SetupCountHints(int maxBads, int maxExecs){
            RuleExtensions.SetupCountHints(this, maxBads, maxExecs);
        }

        public void SetupSelfServiceContainer(){
            Services = new RuleServicesContainer(this);
        }
    }
}