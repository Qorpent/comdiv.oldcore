using System.Collections.Generic;
using System.Linq;

namespace Comdiv.Useful{
    public interface IUniqueStringProvider{
        string New();
        void Lock(IEnumerable<string> existed);
        void Release(IEnumerable<string> notexisted);
        void Release(string code);
    }
}