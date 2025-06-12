namespace OkToPost.Utils
{
    public interface ICodeGenerator
    {
        string GenerateCode();
    }


    public class RandomCodeGenerator : ICodeGenerator
    {
        private static readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly Random _random = new();

        public string GenerateCode()
        {
            return new string(Enumerable.Range(0, 6)
                .Select(x => _chars[_random.Next(_chars.Length)]).ToArray());
        }
    }

    public class GuidCodeGenerator : ICodeGenerator
    {
        public string GenerateCode()
        {
            return Guid.NewGuid().ToString("N")[..6];
        }
    }

}



