using System;
using System.IO;
using UnityEngine;

public class ConfigParser
{
    private static readonly string Path = Directory.GetParent(Application.dataPath) + "\\Config\\config";

    private static readonly string DefaultConfig = "serverIntro:60" + Environment.NewLine +
                                                   "ipPort:1337" + Environment.NewLine +
                                                   "maximumPlayers:20" + Environment.NewLine +
                                                   "minimumPlayers:0" + Environment.NewLine +
                                                   "waitingRoomTime:5" + Environment.NewLine +
                                                   "gameplayTime:600" + Environment.NewLine +
                                                   "leaderboardTime:30";
                                                   

    public static string GetValueString(string value)
    {
        while (true)
        {
            if (!File.Exists(Path))
            {
                CreateConfigFile();
                Debug.LogWarning($"Creating config file at {Path}");
            }

            var lines = File.ReadAllLines(Path);

            foreach (var s in lines)
                if (s.Contains(value))
                    return DataFormatter.StringToArray(s, ":")[1];

            File.Delete(Path);

            CreateConfigFile();
        }
    }

    public static int GetValueInt(string value)
    {
        return int.Parse(GetValueString(value));
    }

    private static void CreateValue(string name, string value)
    {
        Debug.LogWarning($"Creazione parametro {name}");

        var newValue = $"{name}:{value}";

        File.AppendAllText(Path, newValue + Environment.NewLine);
    }

    private static void CreateConfigFile()
    {
        Directory.CreateDirectory(Directory.GetParent(Application.dataPath) + "\\Config\\");
        File.AppendAllText(Path, DefaultConfig);
    }

    public static void SetValue(string name, string value)
    {
        var lines = File.ReadAllLines(Path);

        for (var i = 0; i < lines.Length; i++)
            if (lines[i].Contains(name))
                lines[i] = $"{name}:{value}";

        File.WriteAllLines(Path, lines);
    }
}