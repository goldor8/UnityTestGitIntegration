using System;
using System.IO;
using Unity.PerformanceTesting.Data;
using Unity.PerformanceTesting.Editor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public class TestHistory
{
    private static readonly string dirPath = GetTestHistoryDirectory();
    //private static readonly string metadataFile = "metadata.xml";
    
    public static string GetTestHistoryDirectory()
    {
        string[] explodedAssetsPath = Application.dataPath.Split("/");
        string[] explodedTestHistoryPath = new string[explodedAssetsPath.Length - 1];
        Array.Copy(explodedAssetsPath, explodedTestHistoryPath, explodedAssetsPath.Length - 1);
        return string.Join("/", explodedTestHistoryPath) + "/TestHistory";
    }
    
    public static void SaveTestResult(string commitSha, ITestResultAdaptor result)
    {
        DeleteTestResult(commitSha, false);
        string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string resultFile = $"{dirPath}/{date}_{commitSha}.xml";
        
        WriteResultToPath(result, resultFile);
    }
    
    public static void DeleteTestResult(string commitSha, bool warnIfNotFound = true)
    {
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string sha = file.Name.Split('_')[2].Split('.')[0];
            if (sha == commitSha)
            {
                file.Delete();
                return;
            }
        }

        if(warnIfNotFound)
            Debug.LogError($"No test result found for commit {commitSha}");
    }
    
    public static void LoadTestResultToReport(string commitSha)
    {
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string sha = file.Name.Split('_')[2].Split('.')[0];
            if (sha == commitSha)
            {
                string xmlPath = file.FullName;
                SendResultToReport(xmlPath);
            }
        }
    }
    
    private static void WriteResultToPath(ITestResultAdaptor result, string path)
    {
        var resultWriter = new ResultsWriter();
        resultWriter.WriteResultToFile(result, path);
    }

    private static void SendResultToReport(string xmlPath)
    {
        var jsonPath = Path.Combine(Application.persistentDataPath, "PerformanceTestResults.json");
        
        var xmlParser = new TestResultXmlParser();
        var run = xmlParser.GetPerformanceTestRunFromXml(xmlPath);
        if (run == null) return;
        
        File.WriteAllText(jsonPath, JsonUtility.ToJson(run, true));
    }

    public static void SendResultToReport(ITestResultAdaptor result)
    {
        var xmlPath = Path.Combine(Application.persistentDataPath, "TestResults.xml");
        
        WriteResultToPath(result, xmlPath);
        
        SendResultToReport(xmlPath);
    }

    public static (DateTime date, string commitSha)[] GetTestHistory()
    {
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        
        if (!dir.Exists)
            return Array.Empty<(DateTime date, string commitSha)>();
        
        FileInfo[] files = dir.GetFiles();
        (DateTime date, string commitSha)[] history = new (DateTime date, string commitSha)[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            string[] parts = files[i].Name.Split('_');
            string date = parts[0] + "_" + parts[1];
            string sha = parts[2].Split('.')[0];
            history[i] = (DateTime.ParseExact(date, "yyyy-MM-dd_HH-mm-ss", null), sha);
        }
        return history;
    }
}
