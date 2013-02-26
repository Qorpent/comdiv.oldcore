using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comdiv.Extensions;
using NUnit.Framework;

namespace Comdiv.Core.Test {
    [TestFixture]
    public class DateExtensionsTest {
        [Test,Sequential]
        public void accomodateYear(
            [Values("1897-03-04","1895-02-28","1901-03-01")]string srcDate, 
            [Values(2001,2008,2010)]int yearToAccomodate, 
            [Values("1999-03-04","2004-02-29","1901-03-01")]string resultDate)
        {
            Assert.AreEqual(DateTime.Parse(resultDate), DateTime.Parse(srcDate).accomodateToYear(yearToAccomodate));
            
        }
    }
}
