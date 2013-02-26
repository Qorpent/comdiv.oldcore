using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Comdiv._Extensions_Internals{
    /// <summary>
    /// Инкапсулирует единицу захвата, привязанную к определенному месту в иерархии	разбора с сервисами для удобства генерации XML (не мешают использовать	стандартные регулярки
    /// </summary>
    internal class AdressedCapture : IComparable{
        private readonly List<AdressedCapture> _childs = new List<AdressedCapture>();
        private string _normalname;
        private string _postfix;

        public AdressedCapture(int id, string name, Capture c, int index, int number)
            : this(id, name, c.Value, c.Index, c.Length, index, number) {}

        public AdressedCapture(int id, string name, Capture c, int number)
            : this(id, name, c.Value, c.Index, c.Length, 0, number) {}

        public AdressedCapture(int id, string name, string value, int start, int length, int index, int number){
            Id = id;
            Name = name;
            Value = value;
            Start = start;
            Length = length;
            FirstIndex = index;
            Number = number;
        }

        public int Number { get; set; }

        public string NormalName{
            get{
                if (_normalname == null)
                    _normalname = Regex.Replace(Name, @"_[\s\S]*$", string.Empty);
                return _normalname;
            }
        }


        public bool Inner{
            get { return PostFix.IndexOf("i") != -1; }
        }

        public bool Outer{
            get { return PostFix.IndexOf("o") != -1; }
        }

        public string PostFix{
            get{
                if (_postfix == null)
                    _postfix = Regex.Match(Name, @"_(?<pf>[\S\s]*)$").Groups["pf"].Value;
                return _postfix;
            }
        }

        public bool UseAsAttribute{
            get { return PostFix.IndexOf("a") != -1; }
        }

        public bool UseAlwaysWithValue{
            get { return PostFix.IndexOf("v") != -1; }
        }

        public bool GroupInCollection{
            get { return PostFix.IndexOf("c") != -1; }
        }

        public List<AdressedCapture> Childs{
            get { return _childs; }
        }

        public AdressedCapture Parent { get; set; }

        public string Name { get; set; }

        public int FirstIndex { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public string Value { get; set; }

        public int Id { get; set; }

        public int End{
            get { return Start + Length; }
        }

        public object Tag { get; set; }

        #region IComparable Members

        public int CompareTo(object obj){
            var c = obj as AdressedCapture;
            if (c == null) return 0;
            if (Contains(c)) return -1;
            if (c.Contains(this)) return 1;
            return Start.CompareTo(c.Start);
            //	return 0;
#if old
			if (!(obj is AdressedCapture))
				return 0;
			AdressedCapture t = obj as AdressedCapture;
			int s = this.Start.CompareTo(t.Start);
			if (s != 0)
				return s;
			int l = this.Length.CompareTo(t.Length);
			return -l;
#endif
        }

        #endregion

        public override string ToString(){
            return string.Format("{5} {0}{1}({2}-{3}){{ {6} }}{4}",
                                 Parent == null ? string.Empty : ("[" + Parent.Id + "]"),
                                 Name,
                                 Start,
                                 Start + Length,
                                 Childs.Count == 0 ? string.Empty : "*",
                                 Id,
                                 Value);
        }

        public bool Contains(AdressedCapture c){
            if (Start > c.Start)
                return false;
            if (End < c.End)
                return false;
            if (Outer && (!c.Outer)) return true;
            if (Inner && (!c.Inner)) return false;
            if (Number > c.Number) return false;
            return true;
        }
    }
}