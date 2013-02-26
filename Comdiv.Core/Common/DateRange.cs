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
using System.Collections;
using System.Collections.Generic;
using Comdiv.Extensions;

namespace Comdiv.Model{
    /// <summary>
    /// Для обеспечения работы расширения... тут сложный момент с избавлением 
    /// Comdiv.Common от зависимостей при 
    /// </summary>
    /// 
    public class DateRange {
        private const string deltaPatternRegex =
            @"(?ix)^
			(?<delta>-|\+)?
			(?<year>\d{1,4})-
			(?<month>\d{1,2})-
			(?<day>\d{1,2})(\s
			(?<hour>\d{1,2}):
			(?<min>\d{1,2}))?
			$
			";

        private DateTime finish = DateTime.Now;
        private DateTime start = DateTime.Now;

        public DateRange() {}

        public DateRange(DateTime startDate, DateTime endDate){
            Start = startDate;
            Finish = endDate;
        }

        public DateRange(DateTime keydate){
            if (keydate < DateTime.Now){
                Start = keydate;
            }
            if (keydate > DateTime.Now){
                Finish = keydate;
            }
        }

        #region IDateRange Members

        public virtual DateTime Start{
            get { return start; }
            set { start = value; }
        }

        public virtual DateTime Finish{
            get { return finish; }
            set { finish = value; }
        }

        #endregion

        public DateRange GetNotLimited(){
            return new DateRange(new DateTime(1900, 1, 1, 0, 0, 0, 0), new DateTime(3000, 1, 1, 0, 0, 0, 0));
        }

        public static DateRange Parse(string s){
            return Parse(s, 0);
        }

        public static DateRange Parse(string s, int startIndex){
            var pars = s.Split(';');
            var startDescriptor = pars[startIndex];
            var endDescriptor = pars[startIndex + 1];
            var s_context = pars[startIndex + 2];
            var e_context = pars[startIndex + 3];
            return Parse(startDescriptor, endDescriptor, s_context, e_context);
        }

        public static DateRange Parse(string startDescriptor, string endDescriptor, string s_context,
                                          string e_context){
            var start = GetDelted(startDescriptor);
            var end = GetDelted(endDescriptor);
            return GetPeriod(start, end, s_context, e_context);
        }

        public static DateRange GetPeriod(DateTime startDelta, DateTime endDelta, string s_context, string e_context){
            var start = GetCorrected(startDelta, s_context);
            var end = GetCorrected(endDelta, e_context);
            return new DateRange(start, end);
        }
        public bool IsInRange(DateTime d)
        {
            return d >= Start && d <= Finish;
        }
        public static DateTime GetCorrected(DateTime d, string context){
            var res = d;
            var delta = 1;
            if (context.IndexOf("G") != -1){
                return res;
            }
            if (context.IndexOf("L") != -1){
                delta = 2;
            }
            if (context.IndexOf("D") != -1){
                res = res - new TimeSpan(0, res.Hour, res.Minute, res.Second, res.Millisecond);
                res = res.AddDays(-(delta - 1));
            }
            if (context.IndexOf("W") != -1){
                res = res.AddDays(-(int) d.DayOfWeek + 1);
                res = res.AddDays(-(delta - 1)*7);
            }
            if (context.IndexOf("M") != -1){
                res = res.AddDays(-d.Day + 1);
                res = res.AddMonths(-(delta - 1));
            }
            if (context.IndexOf("Y") != -1){
                return new DateTime(d.Year, 1, 1, 0, 0, 0, 0);
            }
            return res;
        }


        public static DateTime GetDelted(string descriptor){
            var res = deltaPatternRegex.toRegex().toDictionary(descriptor);
            var l = new List<string>(res.Keys);
            foreach (var k in l){
                //	res[k] = ExtendedRegex.replace(res[k] as string,"^0+","0",System.Text.RegularExpressions.RegexOptions.Compiled);
                if (string.IsNullOrEmpty(res[k])){
                    res[k] = "0";
                }
            }
            if (res["delta"].no()){
                try{
                    return new DateTime(
                        res["year"].toInt(),
                        res["month"].toInt(),
                        res["day"].toInt(),
                        res["hour"].toInt(),
                        res["min"].toInt(), 0, 0);
                }
                catch (FormatException e){
                    throw new Exception("date range parse error ${year},${month},${day},${hour},${min}"._formatex(res), e);
                }
            }
            var s = new TimeSpan(res["day"].toInt(), res["hour"].toInt(),
                                 res["min"].toInt(), 0, 0);
            var result = DateTime.Now - s;
            if (res["delta"] == "+"){
                result = DateTime.Now + s;
            }
            return result;
        }

        public static DateRange[] Merge(IEnumerable periods){
            var basis = new ArrayList();
            foreach (var p_ in periods){
                var p = p_ as DateRange;
                if (p == null){
                    continue;
                }
                var merged = false;
                foreach (DateRange b in basis){
                    if (b.Appart(p)){
                        continue;
                    }
                    b.Merge(p);
                    merged = true;
                    break;
                }
                if (merged){
                    continue;
                }
                basis.Add(p.Clone());
            }
            return basis.ToArray(typeof (DateRange)) as DateRange[];
        }

        public object Clone(){
            return new DateRange(Start, Finish);
        }

        public bool Appart(DateRange p){
            if (Start > p.Finish){
                return true;
            }
            if (Finish < p.Start){
                return true;
            }
            return false;
        }

        public void Merge(DateRange p){
            if (Appart(p)){
                return;
            }
            Start = Start > p.Start ? p.Start : Start;
            Finish = Finish < p.Finish ? p.Finish : Finish;
        }

        public static DateRange Infinite(){
            return new DateRange(DateExtensions.Begin, DateExtensions.End);
        }
    }
}