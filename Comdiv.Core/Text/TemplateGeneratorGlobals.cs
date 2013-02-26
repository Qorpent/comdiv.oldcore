namespace Comdiv.Text {
    public static class TemplateGeneratorGlobals {
        public static string SubstitutionMask = @"\[\[(?<code>\w+)(:(?<replace>[\s\S]+?))?\]\]";
        public static string ConditionalMask = @"<<(?<code>\w+):(?<content>[\s\S]+?)>>";
    }
}