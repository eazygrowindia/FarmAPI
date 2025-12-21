using System;

namespace FarmAPI.Utils
{
    public static class Base64Url
    {
        public static string Encode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.TrimEnd('=');           // remove '=' padding
            output = output.Replace('+', '-');      // 62nd char
            output = output.Replace('/', '_');      // 63rd char
            return output;
        }

        public static byte[] Decode(string input)
        {
            var output = input.Replace('-', '+').Replace('_', '/');

            switch (output.Length % 4)
            {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default:
                    throw new ArgumentException("Illegal base64url string", nameof(input));
            }

            return Convert.FromBase64String(output);
        }
    }
}
