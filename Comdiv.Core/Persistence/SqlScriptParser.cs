using System.Linq;
using System.Text.RegularExpressions;

namespace mia.utils {
    /// <summary>
    /// Разбивает скрипт sql на отдельные комманды
    /// </summary>
    public class SqlScriptParser {
        /// <summary>
        /// Простой паттерн нахождения Go
        /// </summary>
        private const string GOPATTERN = @"(?ixm)^\s*go\s*$";

        /// <summary>
        /// Временный заместитель go
        /// </summary>
        private const string NOTREALGOSUBST = "SqlScriptParser_Spliter";
        /// <summary>
        /// Временный заместитель go
        /// </summary>
        private const string DOUBLEDAPOSTROFES = "SqlScriptParser_DoubleApos";
        private const string PSEUDOAPOS = "SqlScriptParser_Apos";
        private const string ITSSUBST = "SqlScriptParser_Its";
        private const string RSSUBST = "SqlScriptParser_Rs";
        private const string XPATH1 = "SqlScriptParser_XPATH1";

        /// <summary>
        /// Разбивает скрипт sql на отдельные комманды
        /// </summary>
        /// <param name="sqlString">Исходный текст sql скрипта</param>
        /// <returns>Возвращает массив sql комманд</returns>
        public string[] Parse(string sqlString) {
            string safestring = safeSql(sqlString);
            return (from x in Regex.Split(safestring, GOPATTERN)
                    let t = x.Trim()
                    where !string.IsNullOrWhiteSpace(t) && t.ToLower() != "go"
                    select unsafeSql(t)
                   )
                .ToArray();
        }

        private string safeSql(string sqlstring) {
			//TODO: регексы по сути неправильные, обрабатывают не все ситуации, приложены тесты
			//TIP: а) надо и строки и комменты проходить в 2 этапа (посмотри класс MatchEvaluator и его использование в Regex.Replace)
			//TIP: б) в защиту и снятие защиты надо включить экранирование и деэкранирование 2-х апострофов - тогда все проще
            //NOTE: ситуация с разорванными апострофами в каментах - не обрабатываема в данном случае!!!
            //NOTE: вложенные каменты не поддерживаются!!!
            MatchEvaluator goreplace = m => m.Value.Replace("go", NOTREALGOSUBST);
            var newstring = sqlstring;
            newstring = newstring.Replace("''", DOUBLEDAPOSTROFES);
            newstring = newstring.Replace("t's", ITSSUBST);
            newstring = newstring.Replace("r's", RSSUBST);
            newstring = newstring.Replace("//*", XPATH1);
            newstring = Regex.Replace(newstring, @"(?ix)(^|[\r\n])\s*/\*@[\s\S]+?@\*/",
                                      m => {
                                          var result =Regex.Replace(m.Value,"go",NOTREALGOSUBST);
                                          result = result.Replace("'", PSEUDOAPOS);

                                          return result;

                                      });
            newstring = Regex.Replace(newstring, @"(?ix)'[^']*'", goreplace);
            newstring = Regex.Replace(newstring, @"(?ix)/\*[\s\S]+?\*/", goreplace);
            return newstring;
        }

        private string unsafeSql(string safedSql) {
            var result = safedSql.Replace(NOTREALGOSUBST, "go");
            result = result.Replace(DOUBLEDAPOSTROFES, "''");
            result = result.Replace(ITSSUBST, "t's");
            result = result.Replace(RSSUBST, "r's");
            result = result.Replace(XPATH1, "//*");
            result = result.Replace(PSEUDOAPOS, "'");
            return result;
        }
    }
}