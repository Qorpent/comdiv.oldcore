using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using NUnit.Framework;

namespace Comdiv.MAS
{
    [TestFixture]
    public class ProcessModelTest
    {
        [Test]
        public void can_generate() {
            new MasModel().WriteMappingsTo("processmodeltest".prepareTemporaryDirectory());
        }
    }
}
