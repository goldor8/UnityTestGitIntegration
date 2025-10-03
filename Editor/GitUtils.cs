using System.IO;
using System.Threading;
using LibGit2Sharp;
using UnityEditor;
using UnityEngine;

namespace UnityTestGitIntegration
{
    public static class GitUtils
    {
        private static readonly string _repoPath = Directory.GetCurrentDirectory() + "/.git";

        private static string _lastBranchName;
        private static string _lastCommitSha;

        public delegate void OnCheckoutDetectedDelegate(string branchName);

        public delegate void OnLocalCommitDetectedDelegate(string commitSha);

        public static event OnCheckoutDetectedDelegate OnCheckoutDetected;
        public static event OnLocalCommitDetectedDelegate OnLocalCommitDetected;
        
        private static float lastCheckTime = 0;

        [InitializeOnLoadMethod]
        private static void InitGitUpdateListener()
        {
            if (!IsGitInitialized())
            {
                Debug.LogWarning("Git is not initialized in this project. Git update listener will not be started.");
                return;
            }

            using var repo = new Repository(_repoPath);
            _lastBranchName = repo.Head.FriendlyName;
            _lastCommitSha = repo.Head.Tip.Sha;
            
            lastCheckTime = Time.realtimeSinceStartup;
            EditorApplication.update += () =>
            {
                if (Time.realtimeSinceStartup - lastCheckTime > 1)
                {
                    CheckForChanges();
                    lastCheckTime = Time.realtimeSinceStartup;
                }
            };
        }

        private static void CheckForChanges()
        {
            using var repo = new Repository(_repoPath);
            var currentBranchName = repo.Head.FriendlyName;
            var currentCommitSha = repo.Head.Tip.Sha;

            if (currentBranchName != _lastBranchName)
            {
                OnCheckoutDetected?.Invoke(currentBranchName);
                _lastBranchName = currentBranchName;
                _lastCommitSha = currentCommitSha;
            }
            else if (currentCommitSha != _lastCommitSha)
            {
                OnLocalCommitDetected?.Invoke(currentCommitSha);
                _lastCommitSha = currentCommitSha;
            }
        }

        public static bool IsGitInitialized()
        {
            return Repository.IsValid(_repoPath);
        }
        
        public static bool HasUncommittedChanges()
        {
            using var repo = new Repository(_repoPath);
            return repo.RetrieveStatus().IsDirty;
        }

        public static string GetLastCommitSha()
        {
            using var repo = new Repository(_repoPath);
            return repo.Head.Tip.Sha;
        }
        
        public static string GetCommitName(string commitSha)
        {
            using var repo = new Repository(_repoPath);
            return repo.Lookup<Commit>(commitSha).MessageShort;
        }
    }
}
