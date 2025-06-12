
namespace OkToPost.Utils
{
    public static class ShortCodeValidator
    {
        public static bool IsValid(string? code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.Length == 6;
        }
    }
}

