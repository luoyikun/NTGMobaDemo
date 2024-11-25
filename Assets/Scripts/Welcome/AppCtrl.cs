using System;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class AppConfig
{
    public const bool Release = false;

#if UNITY_EDITOR
    public const bool LuaDebugMode = !Release;
    public const bool ResourceUpdateEnabled = Release;
    public const int ApplicationTargetFPS = 360;
#else
    public const bool LuaDebugMode = false;    
    public const bool ResourceUpdateEnabled = Release;
    public const int ApplicationTargetFPS = 30;
#endif

    public const bool LuaEncode = Release;

    public const string ResourceUpdateUrl_Android = Release ? "http://utggameupdate.oss-cn-hangzhou.aliyuncs.com/ntg/android/" : "http://10.10.0.100/ntg/android/";
    public const string ResourceUpdateUrl_IOS = Release ? "http://utggameupdate.oss-cn-hangzhou.aliyuncs.com/ntg/ios/" : "http://10.10.0.100/ntg/ios/";

    public const string ApplicationName = "KunMoba";

    public const bool AutoWrapMode = true;  

    public static string LuaBasePath
    {
        get { return Application.dataPath + "/uLua/Source/"; }
    }

    public static string LuaWrapPath
    {
        get { return LuaBasePath + "LuaWrap/"; }
    }
}

public class AppCtrl : MonoBehaviour
{
    public static Type GetType(string TypeName)
    {
        //var type = Type.GetType(TypeName);
        //if (type != null)
        //    return type;

        //var type = Types.GetType(TypeName, "Assembly-CSharp");
        var type = System.Reflection.Assembly.Load("Assembly-CSharp").GetType(TypeName);
        if (type != null)
            return type;

        var assemblyName = TypeName;
        while (assemblyName.LastIndexOf('.') != -1)
        {
            assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf('.'));
            //type = Types.GetType(TypeName, assemblyName);
            type = System.Reflection.Assembly.Load(assemblyName).GetType(TypeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private static AppCtrl _instance;

    public static AppCtrl Instance
    {
        get { return _instance; }
    }

    public static void SetShowQuality(bool show)
    {
        if (show)
        {
            QualitySettings.pixelLightCount = 4;
            QualitySettings.skinWeights = SkinWeights.TwoBones;
        }
        else
        {
            QualitySettings.pixelLightCount = 1;
            QualitySettings.skinWeights = SkinWeights.OneBone;
        }
    }

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = AppConfig.ApplicationTargetFPS;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            //    Destroy(standaloneInputModule);

            InitResources();
            //ResCtrl.Instance.InitBundleDependencies();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private bool UpdateResourcePanelLoaded = false;
    private GameObject UpdateResourcePanel;

    private void InitResources()
    {
        if (Directory.Exists(ResCtrl.DataPath) && File.Exists(ResCtrl.DataPath + "files.txt"))
        {
            //LoadUpdateResourcePanel();
            StartCoroutine(doUpdateResources());
        }
        else
        {
            StartCoroutine(doExtractResources());
        }
    }

    private IEnumerator ExtractFile(string infile, string outfile)
    {
        var dir = Path.GetDirectoryName(outfile);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(infile);
            yield return www;

            if (www.isDone)
            {
                File.WriteAllBytes(outfile, www.bytes);
            }
        }
        else
        {
            File.Copy(infile, outfile, true);
        }
    }

    private IEnumerator doExtractResources()
    {
        string dataPath = ResCtrl.DataPath;
        string assetsPath = ResCtrl.StreamingAssetsPath;

        //Debug.Log("Extracting from " + assetsPath + " to " + dataPath);

        string infile = assetsPath + "files.txt";
        string outfile = dataPath + "files.txt";
        yield return StartCoroutine(ExtractFile(infile, outfile));
        yield return null;

        int preload = 0;
        string[] files = File.ReadAllLines(outfile);
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            if (!file.StartsWith("lua") || (file.StartsWith("lua/Logic") && !file.StartsWith("lua/Logic/UpdateResource")))
                continue;
            string[] fs = file.Split('|');
            infile = assetsPath + fs[0];
            outfile = dataPath + fs[0];

            //Debug.Log("Extracting System File:>" + infile);
            yield return StartCoroutine(ExtractFile(infile, outfile));
            yield return null;

            preload++;
        }

        //释放资源面板
        //infile = assetsPath + "updateresource.assetbundle";
        //outfile = dataPath + "updateresource.assetbundle";
        //Debug.Log("Extracting UpdatePanel File:>" + infile);
        //yield return StartCoroutine(ExtractFile(infile, outfile));
        yield return null;

        //LoadUpdateResourcePanel();
        //LuaCall("UpdateResourceAPI", "ShowUpdateInfo", UpdateResourcePanelApi, 3, 0);
        //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, -1, 0);

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            if (!(!file.StartsWith("lua") || (file.StartsWith("lua/Logic") && !file.StartsWith("lua/Logic/UpdateResource"))))
            {
                //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, -1, ((float)i + 1) / (files.Length - preload));
                continue;
            }
            string[] fs = file.Split('|');
            infile = assetsPath + fs[0];
            outfile = dataPath + fs[0];

            //Debug.Log(String.Format("Extracting File:{0}->{1}", infile, outfile));
            ////LuaCall("UpdateResourceAPI", "DebugText", UpdateResourcePanelApi, "解压:" + fs[0]);
            yield return StartCoroutine(ExtractFile(infile, outfile));
            yield return null;

            //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, -1, ((float)i + 1) / (files.Length - preload));
        }

        StartCoroutine(doUpdateResources());
    }

    private IEnumerator doUpdateResources()
    {
        if (!AppConfig.ResourceUpdateEnabled)
        {
            StartGameManager();
            yield break;
        }

        //LuaCall("UpdateResourceAPI", "ShowUpdateInfo", UpdateResourcePanelApi, 1, 0);

        string dataPath = ResCtrl.DataPath; //数据目录
        string url = AppConfig.ResourceUpdateUrl_Android;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            url = AppConfig.ResourceUpdateUrl_IOS;

        WWW www = new WWW(url + "files.txt");
        yield return www;
        if (www.error != null)
        {
            Debug.LogError("Update Resources Failed! :" + www.error.ToString());
            StartGameManager();
            yield break;
        }

        var updateUrls = new ArrayList();
        var updateFiles = new ArrayList();

        File.WriteAllBytes(dataPath + "files.txt", www.bytes);
        string[] files = www.text.Split('\n');
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            if (string.IsNullOrEmpty(file))
                continue;

            string[] keyValue = file.Split('|');
            string filename = keyValue[0];
            string localfile = (dataPath + filename).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileUrl = url + filename;
            bool needUpdate = !File.Exists(localfile);
            if (!needUpdate)
            {
                string remoteMd5 = keyValue[1].Trim();
                string localMd5 = ResCtrl.GetFileMD5(localfile);
                needUpdate = !remoteMd5.Equals(localMd5);
            }

            if (needUpdate)
            {
                updateUrls.Add(fileUrl);
                updateFiles.Add(localfile);
            }
        }

        //LuaCall("UpdateResourceAPI", "ShowUpdateInfo", UpdateResourcePanelApi, 2, 0);
        //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, 0, 0);
        for (int i = 0; i < updateFiles.Count; i++)
        {
            File.Delete((string)updateFiles[i]);

            var downloader = new ResCtrl.FileDownloder();

            Debug.Log("Updating File:>" + updateUrls[i]);
            ////LuaCall("UpdateResourceAPI", "DebugText", UpdateResourcePanelApi, "下载:" + keyValue[0]);

            downloader.DownloadFile((string)updateUrls[i], (string)updateFiles[i]);
            while (!downloader.DownloadComplete)
            {
                yield return null;
                //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, downloader.DownloadingSpeed, ((float)i) / updateUrls.Count);
            }

            //LuaCall("UpdateResourceAPI", "GetLoadingData", UpdateResourcePanelApi, downloader.DownloadingSpeed, ((float)i + 1) / updateUrls.Count);
            yield return null;
        }

        StartGameManager();
    }

    //private LuaState lua = null;
    //private LuaLooper looper = null;


    public Transform panelRoot;
    public Transform gameRoot;
    public StandaloneInputModule standaloneInputModule;

  
    private void StartGameManager()
    {
        //LuaCall("UpdateResourceAPI", "ShowUpdateInfo", UpdateResourcePanelApi, 4, 0);

        if (UpdateResourcePanel != null)
            Destroy(UpdateResourcePanel);

        ResCtrl.Instance.InitBundleDependencies();

        //ResCtrl.Instance.createPanel("Fight");
        SceneManager.LoadSceneAsync("MyJoystick", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("02-01", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

}