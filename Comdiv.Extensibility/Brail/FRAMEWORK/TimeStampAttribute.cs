using System;
using System.Globalization;

namespace Comdiv.Extensibility.Brail
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = false)]
    public class TimeStampAttribute:Attribute
    {
        public TimeStampAttribute() {
            this.DateString = DateTime.Now.ToString("R");
        }
        public TimeStampAttribute(string datetime) {
            this.DateString = datetime;
        }
        public string DateString { get; set; }
        public DateTime GetDate() {
            return System.DateTime.ParseExact(this.DateString, "dd.MM.yyyy HH:mm:ss",CultureInfo.InvariantCulture);
        }
    }
}
