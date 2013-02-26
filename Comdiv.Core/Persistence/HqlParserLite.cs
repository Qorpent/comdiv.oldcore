using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Comdiv.Extensions;

namespace Comdiv.Persistence
{
    public class HqlParseResult
    {
        public HqlParseResult() {
            Fields = new List<string>();
        }
        public bool IsSimple { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public IList<string> Fields { get; private set; }
        public string Source { get; set; }
        public bool All { get; set; }
        public string Condition { get; set; }
        public bool Processed { get; set; }
    }
    public class HqlParserLite {
        private string select;
        private string from;
        private string where;

        private static readonly string firstmask =
            @"^\s*(select(?<select>[\s\S]+?))?from\s+(?<from>[\s\S]+?)((?<where>where[\s\S]+$)|(\s*$))";
        public HqlParseResult Parse(string hql) {
            var result = new HqlParseResult();
            result.Source = hql;
            var match = Regex.Match(hql,firstmask,RegexOptions.Compiled);
            if(match.Success) {
                select = match.Groups["select"].Value;
                from = match.Groups["from"].Value.Trim();
                where = match.Groups["where"].Value.Trim();
                where = where.replace(@"order\sby[\s\S]+", "").Trim();
                if(from.hasContent()) {
                    result.Processed = true;
                    result.Condition = where;
                    if(!from.Contains(",") && !from.Contains("join")) {
                        result.IsSimple = true;
                        var pair = from.split(false, true, ' ');
                        result.TableName = pair[0];
                        result.TableAlias = "";
                        if(pair.Count==2) {
                            result.TableAlias = pair[1];
                        }
                        if(pair.Count==3) { //tname as alias - case
                            result.TableAlias = pair[2];
                        }
                        if(select.hasContent()) {
                            var fields = select.split(false, true, ',');
                            foreach (var field in fields) {
                                var fld = field.split(false, true, '.');
                                if(fld.Count==1) {
                                    result.Fields.Add(fld[0]);
                                }else {
                                    result.Fields.Add(fld[1]);
                                }
                            }
                        }else {
                            result.All = true;
                        }
                    }else if(from.Contains("join")) {
						var pair = from.split(false, true, ' ');
						result.TableName = pair[0];
						result.TableAlias = pair[1];
                    }
                }
            }
            return result;
        }

    }
}
