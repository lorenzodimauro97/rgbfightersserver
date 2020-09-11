using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public static class DataFormatter
{
    public static string[] StringToArray(string data, string separator)
    {
        return Regex.Split(data, separator);
    }

    public static string CalculateFileHash(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}