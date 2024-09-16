namespace BugHouse.Utils.Translates
{
    public static partial class TranslatationTools
    {
        public class ConfigureTranslate
        {
            public string TranslationsPath { get; set; }
            public string DefaultRequestCulture { get; set; }
            public string ExtensionName { get; set; } = "_translation";
        }
    }
}
