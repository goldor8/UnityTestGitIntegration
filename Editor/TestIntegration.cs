using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityTestGitIntegration
{
    public static class TestIntegration
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            GitUtils.OnLocalCommitDetected += (commitSha) =>
            {
                Debug.Log($"Local commit detected");
                bool userWantReport = EditorUtility.DisplayDialog("Commit detected.", "A commit has been detected, would you like to run tests and create a report linked to the git commit ?", "Yes", "No");
                if (userWantReport)
                {
                    CreateTestReport(commitSha);
                }
            };
        }
        
        
        [MenuItem("Tool/Unity Test Git Integration/Create Test Report")]
        public static void CreateTestReport()
        {
            string commitSha = GitUtils.GetLastCommitSha();
            CreateTestReport(commitSha);
        }
        
        private static void CreateTestReport(string commitSha)
        {
            if (GitUtils.HasUncommittedChanges())
            {
                if(!EditorUtility.DisplayDialog("Uncommitted changes", "You have uncommitted changes, do you want to create a report anyway (it will override previous commit report) ?", "Yes", "No"))
                { 
                    return;   
                }
            }

            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.RegisterCallbacks(new TestCallbacks(commitSha));

            Filter filter = new Filter()
            {
                testMode = TestMode.EditMode
            };
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }
        
        private class TestCallbacks : ICallbacks
        {
            private readonly string _commitSha;

            public TestCallbacks(string commitSha)
            {
                _commitSha = commitSha;
            }
            
            public void RunStarted(ITestAdaptor testsToRun)
            {
                //Debug.Log($"Starting tests : {testsToRun.Name}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                //Debug.Log($"Tests finished : {result.Name}");
                TestHistory.SaveTestResult(_commitSha, result);
                TestHistory.LoadTestResultToReport(_commitSha);
                if (EditorWindow.HasOpenInstances<TestHistoryExplorer>())
                {
                    EditorWindow.GetWindow<TestHistoryExplorer>().Repaint();
                }
            }

            public void TestStarted(ITestAdaptor test)
            {
                //Debug.Log($"Starting test : {test.Name}");
            }

            public void TestFinished(ITestResultAdaptor result)
            { 
                //Debug.Log($"Test finished : {result.Name}");
            }
        }
    }
}