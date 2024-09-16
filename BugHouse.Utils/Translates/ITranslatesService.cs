namespace BugHouse.Utils.Translates
{
    public interface ITranslatesService
    {
        string Get(string key);
        string Get(string key, string cultureName);
    }
}
