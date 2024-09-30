#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CavrnusSdk.Editor
{
    public static class CavrnusPackageSampleImporter
    {
        private static AddRequest addRequest;
        private static ListRequest listRequest;

        public static void TryImportPackage(string packageName, Action<PackageInfo> onSuccessImport = null, Action<string> onAlreadyExists = null)
        {
            listRequest = Client.List(true);
            EditorApplication.update += () => CheckForPackage(packageName, onSuccessImport, onAlreadyExists);
        }

        public static void FindPackage(string targetPackageName, Action<PackageInfo> onPackageFound)
        {
            listRequest = Client.List(true);
            EditorApplication.update += () => CheckForFoundPackage(targetPackageName, onPackageFound);
        }

        private static void CheckForFoundPackage(string targetPackageName, Action<PackageInfo> onPackageFound)
        {
            if (listRequest.IsCompleted) {
                var foundPackage =
                    listRequest.Result.FirstOrDefault(p => p.assetPath.ToLowerInvariant().Contains(targetPackageName));
                if (foundPackage != null) { onPackageFound?.Invoke(foundPackage); }

                EditorApplication.update -= () => CheckForFoundPackage(targetPackageName, onPackageFound);
            }
        }

        private static void CheckForPackage(string packageName, Action<PackageInfo> onSuccessImport, Action<string> onAlreadyExists)
        {
            if (listRequest.IsCompleted) {
                var cavrnusPackage =
                    listRequest.Result.FirstOrDefault(p => p.assetPath.ToLowerInvariant().Contains(packageName));
                if (cavrnusPackage == null) {
                    Debug.Log($"Now installing {packageName}...");

                    addRequest = Client.Add(packageName);
                    EditorApplication.update += () =>
                        CheckCurrentPackageInstall(packageName, onSuccessImport, onAlreadyExists);
                }
                else {
                    onAlreadyExists?.Invoke($"Package '{packageName}' is already installed.");
                    EditorApplication.update -= () => CheckForPackage(packageName, onSuccessImport, onAlreadyExists);
                }
            }
        }

        private static void CheckCurrentPackageInstall(string packageName, Action<PackageInfo> onSuccessImport, Action<string> onAlreadyExists)
        {
            if (addRequest.IsCompleted) {
                if (addRequest.Status == StatusCode.Success) {
                    Debug.Log($"Successfully installed {packageName}");
                    var installedPackage = addRequest.Result;

                    // Introduce a delay before invoking the success callback
                    EditorApplication.delayCall += () => onSuccessImport?.Invoke(installedPackage);
                }
                else if (addRequest.Status >= StatusCode.Failure) {
                    Debug.LogError($"Failed to install {packageName}: {addRequest.Error.message}");
                    onAlreadyExists?.Invoke($"Failed to install '{packageName}': {addRequest.Error.message}");
                }

                EditorApplication.update -= () =>
                    CheckCurrentPackageInstall(packageName, onSuccessImport, onAlreadyExists);
            }
        }

        public static void TryImportSample(PackageInfo packageInfo, string sampleName, Action onSuccess = null, Action onFailure = null)
        {
            var samples = Sample.FindByPackage(packageInfo.name, packageInfo.version).ToList();
            if (samples.Count > 0) {
                var sample = samples.FirstOrDefault(s => s.displayName == sampleName);
                if (!sample.isImported) {
                    sample.Import();

                    // Introduce a delay before invoking the success callback
                    EditorApplication.delayCall += () => {
                        onSuccess?.Invoke();
                        Debug.Log($"{sampleName} imported successfully!");
                    };
                }
                else {
                    Debug.Log($"{sampleName} is already imported");
                    onSuccess?.Invoke(); // Invoke success callback if already imported
                }
            }
            else {
                Debug.LogError($"No sample found in package {packageInfo.name}.");
                onFailure?.Invoke();
            }
        }
    }
}
#endif