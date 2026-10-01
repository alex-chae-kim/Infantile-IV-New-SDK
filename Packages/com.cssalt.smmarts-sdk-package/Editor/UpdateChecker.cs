using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;


//TODO: POINT TO NEW REPO and TEST.

namespace SMMARTS
{
    public class UpdateChecker : Editor
    {
        //Placeholder. Place in EditorPreferences later.
        private static float CURRENT_VERSION = 0.92f;

        // THIS IS THE OLD REPO
        public static string REPO_URL = "https://github.com/DLizdas";
        private static string URL = "https://raw.githubusercontent.com/DLizdas/SMARTS-SDK-build/MZ/README.md?token=AQaOmzQJe7KyI3jFwHzmQh5eu28a7vPfks5ZnXxywA%3D%3D";

        public static void CheckUpdates()
        {
            //Check if a README exists in Assets, and set CURRENT_VERSION if so.
            if (!BaseReadmeExists())
                return;     //Nothing to check against!

            EditorCoroutine.start(CheckURL());
        }

        private static bool BaseReadmeExists(int fileNumber = 0)
        {
            try
            {
                var baseMD = Directory.GetFiles("Assets\\", "README.md", SearchOption.AllDirectories);
                string data = File.ReadAllText(baseMD[fileNumber]);

                //Debug.Log("Found " + baseMD[0]);

                string OldCode = data.Split(' ')[0].Substring(1);
                CURRENT_VERSION = float.Parse(OldCode);
                Debug.Log("[UPDATE CHECKER] CSSALT library version: " + CURRENT_VERSION);
                return true;
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(IndexOutOfRangeException))
                    Debug.LogWarning("[UPDATE CHECKER] Update file not found! Aborting update check.");

                else if (e.GetType() == typeof(FormatException))
                {
                    Debug.Log("Oops, wrong file!");
                    return BaseReadmeExists(++fileNumber);
                }

                //Debug.LogException(e);
                return false;
            }

        }

        private static IEnumerator CheckURL()
        {
            WWW versionFile = new WWW(URL);

            //Debug.Log("Download started");

            yield return versionFile.isDone;

            while (!versionFile.isDone)
            { }  //Wait till the download is done. Coroutine doesn't seem to be working :(
                 //Although Coroutine is probably on a different thread, so no slowdown.

            if (versionFile.isDone)
            {
                string data = System.Text.Encoding.Default.GetString(versionFile.bytes);

                string vCode = data.Split(' ')[0].Substring(1);

                Debug.Log("[UPDATE CHECKER] Online version: " + vCode);

                if (float.Parse(vCode) > CURRENT_VERSION)
                {
                    Debug.Log("Update available!");
                    UpdateNotifierWindow.Init();
                }

            }
        }
    }
}
