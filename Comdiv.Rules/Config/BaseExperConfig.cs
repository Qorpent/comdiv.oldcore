using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Rules.Config{
    public class BaseExperConfig : IExpertConfig{
        private IContextFactory contextFactory = new BaseContextFactory();
        private IExpertFactory expertFactory = new BaseExpertFactory();

        #region IExpertConfig Members

        public IExpertFactory ExpertFactory{
            get { return expertFactory; }
            set { expertFactory = value; }
        }

        public IContextFactory ContextFactory{
            get { return contextFactory; }
            set { contextFactory = value; }
        }

        #endregion
    }
}