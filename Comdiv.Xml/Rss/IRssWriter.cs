namespace Comdiv.Rss
{
    public interface IRssWriter
    {
        void Write(string fileName, IRssSource src);
    }
}
