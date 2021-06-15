using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build All")]
    public static void BuidAll()
    {
        BuildWindowServer();

    }


    [MenuItem("Build/Build Window Server")]
    public static void BuildWindowServer()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new[] {"Assets/Scenes/Log.unity", "Assets/Scenes/Koko.unity"}; // Assets/Scenes/Scene_001
        opt.locationPathName = "Builds/Windows/Server/winser.exe";
        opt.target = BuildTarget.StandaloneWindows64;
        opt.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        Console.Write("Building server for windows .... ");
        BuildPipeline.BuildPlayer(opt);
        Console.Write("Starting the build");
    }

    [MenuItem("Build/Build Linux Server")]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new[] {"Assets/Scenes/Log.unity", "Assets/Scenes/Koko.unity"}; // Assets/Scenes/Scene_001
        opt.locationPathName = "Builds/Windows/Linux/linser.x86_64";
        opt.target = BuildTarget.StandaloneLinux64;
        opt.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        Console.Write("Building server for Linux .... ");
        BuildPipeline.BuildPlayer(opt);
        Console.Write("Starting the build");
    }

    [MenuItem("Build/Build Client Windows")]
    public static void BuildWindowClient()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new[] {"Assets/Scenes/Log.unity", "Assets/Scenes/Koko.unity"}; // Assets/Scenes/Scene_001
        opt.locationPathName = "Builds/Windows/Client/wincli.exe";
        opt.target = BuildTarget.StandaloneWindows64;
        opt.options = BuildOptions.CompressWithLz4HC | BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.AutoRunPlayer | BuildOptions.WaitForPlayerConnection | BuildOptions.ConnectToHost | BuildOptions.ConnectWithProfiler;
        Console.Write("Building Client for windows .... ");
        BuildPipeline.BuildPlayer(opt);
        Console.Write("Starting the build");
    }
}
