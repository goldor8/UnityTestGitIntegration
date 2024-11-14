using System;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.PerformanceTesting.Editor;
using Button = UnityEngine.UIElements.Button;

namespace UnityTestGitIntegration
{
    public class TestHistoryExplorer : EditorWindow
    {
        [MenuItem("Tool/Unity Test Git Integration/Test History Explorer")]
        public static void ShowWindow()
        {
            TestHistoryExplorer window = GetWindow<TestHistoryExplorer>("Test History Explorer");
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            
            ScrollView commitResults = new ScrollView();
            
            (DateTime date, string commitSha)[] testHistory = TestHistory.GetTestHistory();
            foreach ((DateTime date, string commitSha) in testHistory)
            {
                string commitName = GitUtils.GetCommitName(commitSha);
                Box box = new Box();
                Label label = new Label($"Report : {commitName} - {date}");
                box.Add(label);
                
                Box buttonsBox = new Box();
                buttonsBox.style.flexDirection = FlexDirection.Row;
                
                Button loadButton = new Button(() =>
                {
                    TestHistory.LoadTestResultToReport(commitSha);
                });
                loadButton.text = "Load";
                buttonsBox.Add(loadButton);
                
                Button deleteButton = new Button(() =>
                {
                    if(EditorUtility.DisplayDialog("Delete report", "Are you sure you want to delete this report ?", "Yes", "No"))
                    {
                        TestHistory.DeleteTestResult(commitSha);
                        commitResults.Remove(box);
                    }
                });
                deleteButton.text = "Delete";
                buttonsBox.Add(deleteButton);
                
                box.Add(buttonsBox);
                commitResults.Add(box);
            }
            
            root.Add(commitResults);
        }
    }
}