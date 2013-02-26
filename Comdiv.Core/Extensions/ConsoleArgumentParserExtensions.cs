namespace Comdiv.ConsoleUtils {
    public static class ConsoleArgumentParserExtensions {
        public static X toObject<X>(this string [] args)where X:class ,new() {
            return new ConsoleArgumentParser().ToObject<X>(args);
        }
    }
}