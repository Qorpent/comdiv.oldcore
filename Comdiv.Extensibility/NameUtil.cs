namespace Comdiv.Extensibility
{
    public static class NameUtil
    {
        public static string ToPropertyName(this string  str){
            return str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1);
        }
        public static string ToFieldName(this string str)
        {
            return "__" + str.ToLower();
        }
    }
}
