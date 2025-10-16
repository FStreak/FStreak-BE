using System.Text;

namespace AgoraIO.Media
{
    public static class StringExtensions
    {
        public static byte[] getBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string getString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}