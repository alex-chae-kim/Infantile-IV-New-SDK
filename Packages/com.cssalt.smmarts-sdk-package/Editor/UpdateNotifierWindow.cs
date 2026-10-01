using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

namespace SMMARTS
{
    public class UpdateNotifierWindow : EditorWindow
    {
        IEnumerator cor;
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            UpdateNotifierWindow window = (UpdateNotifierWindow)GetWindow(typeof(UpdateNotifierWindow));
            window.Show();
        }


        private void OnGUI()
        {
            GUILayout.Label("CSSALT Update", EditorStyles.boldLabel);

            GUILayout.Label("An update is available. Download it from the repository " +
                "using the button below.");
            if (GUILayout.Button("Update Now"))
            {
                EditorCoroutine.start(DownloadDLL());
                EditorCoroutine.start(DownloadReadMe());
                this.Close();
            }
        }

        private static IEnumerator DownloadDLL()
        {
            var url = "https://raw.githubusercontent.com/DLizdas/SMARTS-SDK-build/MZ/testingdll/testingdll/bin/Debug/SMARTS-SDK.dll?token=AQaOm0Na8mYO9QOXhqRLA6EBWcrEiLwtks5ZnXv8wA%3D%3D";

            WWW dllFile = new WWW(url);

            yield return dllFile.isDone;
            while (!dllFile.isDone)
            { }

            if (dllFile.isDone)
            {
                string dllFromGit = "Assets/Plugins/temp.dll";
                string localDLL = "Assets/Plugins/SMARTS-SDK.dll";
                File.WriteAllBytes(dllFromGit, dllFile.bytes);
                File.Replace(dllFromGit, localDLL, "BackupSDK.dll");
                File.Delete(dllFromGit);
            }
        }
        private static IEnumerator DownloadReadMe()
        {
            var url = "https://raw.githubusercontent.com/DLizdas/SMARTS-SDK-build/MZ/README.md?token=AQaOmzQJe7KyI3jFwHzmQh5eu28a7vPfks5ZnXxywA%3D%3D";

            WWW readMeFile = new WWW(url);

            yield return readMeFile.isDone;
            while (!readMeFile.isDone)
            { }

            if (readMeFile.isDone)
            {
                string readMeFromGit = "Assets/Plugins/temp.md";
                string localReadMe = "Assets/Plugins/README.md";
                File.WriteAllBytes(readMeFromGit, readMeFile.bytes);
                File.Replace(readMeFromGit, localReadMe, "BackupReadMe.md");
                File.Delete(readMeFromGit);
            }
        }
    }
}
