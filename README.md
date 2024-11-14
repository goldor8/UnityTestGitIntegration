# Unity Test Git Integration
Unity Test Git Integration is a tool to run Unity tests and generate test reports connected to Git commits.

Its main objective is to monitor project performance over time using performance tests.

When committing, you will get a popup that will ask you to run the tests. If you choose to run the tests it will automatically run the tests and store the result linked to the commit.

## Usefull informations
- All the test results are stored in the folder `TestResults` in the root of the project.
- You can load test results from different commits in the `Test History Explorer` window.
- To view the loaded test results you need to open the `Test Report` window. You can do this by going to `Window -> General -> Performance Test Report`.
- To open the `Test History Explorer` window go to `Tool -> Unity Test Git Integration -> Test History Explorer`.
- If you want to run the tests manually you can go to `Tool -> Unity Test Git Integration -> Create Test Report`.