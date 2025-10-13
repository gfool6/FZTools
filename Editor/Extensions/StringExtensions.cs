using UnityEngine;
namespace FZTools
{
    public static class StringExtensions
    {
        public static bool isEmpty(this string str)
        {
            return str.Equals(string.Empty);
        }

        public static bool isNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string convertWinPath2Path(this string path)
        {
            if (path.isNullOrEmpty())
            {
                return path;
            }
            return path.Replace("\\", "/");
        }
    }
}