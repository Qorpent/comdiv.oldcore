// Copyright 2007-2010 Comdiv (F. Sadykov) - http://code.google.com/u/fagim.sadykov/
// Supported by Media Technology LTD 
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// MODIFICATIONS HAVE BEEN MADE TO THIS FILE
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Comdiv.Extensions{
    /// <summary>
    /// DateExtensions provides pseudo-nulls for DateTime, provides some
    /// extensions to manage it
    /// <remarks>
    /// usefull for DB-oriented applications, us substitution
    /// to real db-nulls and DateTime.Min-Max which interval
    /// is not supported by all rdbms
    /// </remarks>
    /// </summary>
    public static class DateExtensions{
       
        /// <summary>
        /// Pseudo Null for Begin date, default to 1900-01-01, all
        /// dates that are less or equal 1900-01-01 are trated as
        /// logical null or you may treat it as minus unlimited for dates
        /// </summary>
        public static readonly DateTime Begin = new DateTime(1900, 1, 1);
        /// <summary>
        /// Pseudo Null for End date, default to 3000-01-01, all
        /// dates that are greater or equal 3000-01-01 are treated as
        /// logical null or you may treat it as plus unlimited for dates
        /// </summary>
        public static readonly DateTime End = new DateTime(3000, 1, 1);
        /// <summary>
        /// Checks if datetime is logical null 
        /// </summary>
        /// <param name="date">date to check</param>
        /// <returns>true if date &lt;= Begin or date &gt;= End</returns>
        public static bool isNull(this DateTime date){
            return date <= Begin || date >= End;
        }

        /// <summary>
        /// accomodates dates before Begin to given year with offset
        /// </summary>
        /// <param name="source">source date</param>
        /// <param name="year">base year</param>
        /// <returns>accommodated year</returns>
        /// <remarks>usefull for economical applications, we
        /// can define and keep in database date in such form:
        /// 1899-01-01, that means "first jan of given year", because
        /// after  new DateTime(1899,1,1).accomodateToYear(2009) =&gt; 2009-01-01"</remarks>
        public static DateTime accomodateToYear(this DateTime source, int year)
        {
            if (source.Year >= Begin.Year) return source;
            var newYear = year - (Begin.Year - 1 - source.Year);
            var result = new DateTime(newYear, source.Month, source.Day, source.Hour, source.Minute, source.Second);
            if (result.Month == 2 && result.Day == 28 && (result.Year % 4 == 0))
            {
                result = result.AddDays(1);
            }
            return result;
        }
    }
}