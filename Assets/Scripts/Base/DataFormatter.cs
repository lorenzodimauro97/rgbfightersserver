using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataFormatter : MonoBehaviour
{
    public static string[] StringToArray(string data, string separator)
    {
        var arrayData = Regex.Split(data, separator);
        return arrayData;
    }

    public static string CalculateFileHash(string filename)
    {
        var md5 = MD5.Create();
        var stream = File.OpenRead(filename);
        var hash = md5.ComputeHash(stream);
        md5.Dispose();
        stream.Dispose();
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}