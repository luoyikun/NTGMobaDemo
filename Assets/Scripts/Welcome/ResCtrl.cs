using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ResCtrl : MonoBehaviour
{
    public static int DebugLevel = 0; //0:Release 1:Debug 2:Verbose
    public Transform m_panelRoot;
    public static bool IsApplePlatform
    {
        get
        {
            return Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.OSXEditor ||
                   Application.platform == RuntimePlatform.OSXPlayer;
        }
    }

    public void createPanel(string name)
    {
        string assetName = name + "Panel";
        var prefab = ResCtrl.Instance.LoadAsset(name, assetName);

        var go = GameObject.Instantiate(prefab);
        go.name = assetName;
        go.transform.SetParent(m_panelRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.SetActive(true);
        ResCtrl.Instance.UnloadAssetBundle(name, false,false);
    }

    //创建新的obj从asset中
    public GameObject objCreateFromAsset(string name)
    {
        string assetName = name;
        var prefab = ResCtrl.Instance.LoadAsset(name, assetName);

        var go = GameObject.Instantiate(prefab);
        go.name = assetName;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.SetActive(true);
        ResCtrl.Instance.UnloadAssetBundle(name, false, false);
        return go;
    }
    public static string DataPath
    {
        get
        {
            string game = AppConfig.ApplicationName.ToLower();
            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath + "/" + game + "/";
            }

            return "c:/" + game + "/";
            //return Application.dataPath + "/StreamingAssets/";
            //return "c:/ssss/";
        }
    }

    public static string GetDataPath(string folder)
    {
        folder = folder.ToLower();
        if (Application.isMobilePlatform)
        {
            return Application.persistentDataPath + "/" + folder + "/";
        }

        return "c:/" + folder + "/";
    }

    public UnityEngine.Object GetSprite(string bundleName, string assetName)
    {
        var prefab = ResCtrl.Instance.LoadAsset(bundleName, assetName, "UnityEngine.Sprite");
        if (prefab == null)
        {
            Debug.Log("找不到图" + bundleName + ".." + assetName);
            return null;
        }

        return (prefab);
    }
    public static string StreamingAssetsPath
    {
        get
        {
            string contentPath;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    contentPath = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    contentPath = Application.dataPath + "/Raw/";
                    break;
                default:
                    contentPath = Application.dataPath + "/StreamingAssets/";
                    break;
            }
            return contentPath;
        }
    }

    public static string LuaPath(string name)
    {
        string path = AppConfig.LuaDebugMode ? Application.dataPath + "/" : DataPath;
        string lowerName = name.ToLower();
        if (lowerName.EndsWith(".lua"))
        {
            int index = name.LastIndexOf('.');
            name = name.Substring(0, index);
        }
        name = name.Replace('.', '/');

        var filePath = path + "lua/" + name + ".lua";

        return filePath;

        if (File.Exists(filePath))
        {
            return filePath;
        }

        return name + ".lua";
    }

    public static void WriteAllText(string path, string text)
    {
        var fs = File.Open(path, FileMode.Create);
        var sw = new StreamWriter(fs);
        sw.Write(text);
        sw.Close();
        fs.Close();
    }

    public static string ReadAllText(string path)
    {
        var fs = File.Open(path, FileMode.Open);
        var sr = new StreamReader(fs);
        var text = sr.ReadToEnd();
        sr.Close();
        fs.Close();

        return text;
    }

    public static string GetFileMD5(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetFileMD5 Failed:" + ex.Message);
        }
    }

    private static ResCtrl _instance;

    public static ResCtrl Instance
    {

        get 
        { 
            return _instance; 
        }
    }

    private AssetBundleManifest manifest;
    private Dictionary<string, AssetBundle> bundles;
    private Dictionary<string, string[]> bundleDependencies;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            //InitBundleDependencies();
        }
    }

    public void InitBundleDependencies()
    {
        bundles = new Dictionary<string, AssetBundle>();
        var uri = DataPath + "StreamingAssets";

        if (!File.Exists(uri)) return;

        var stream = File.ReadAllBytes(uri);
        var assetbundle = AssetBundle.LoadFromMemory(stream);
        manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        bundleDependencies = new Dictionary<string, string[]>();
        foreach (var b in manifest.GetAllAssetBundles())
        {
            bundleDependencies[b] = manifest.GetAllDependencies(b);
        }

        assetbundle.Unload(false);
    }

    private AssetBundle LoadAssetBundle(string abname)
    {
        if (DebugLevel > 1)
            Debug.LogWarning("LoadAssetBundle " + abname);

        if (!abname.EndsWith(".assetbundle"))
        {
            abname += ".assetbundle";
        }

        AssetBundle bundle = null;
        if (!bundles.ContainsKey(abname))
        {
            byte[] stream = null;
            string uri = DataPath + abname;
            //Debug.Log("Loading AssetBundle: " + uri);

            if (!File.Exists(uri))
            {
                Debug.LogError(String.Format("AssetBundle {0} Not Found", uri));
                return null;
            }

            stream = File.ReadAllBytes(uri);
            bundle = AssetBundle.LoadFromMemory(stream);
            stream = null;
            bundles.Add(abname, bundle);

            if (bundleDependencies != null)
            {
                for (int i = 0; i < bundleDependencies[abname].Length; i++)
                {
                    LoadAssetBundle(bundleDependencies[abname][i]);
                }
            }
        }
        else
        {
            bundles.TryGetValue(abname, out bundle);
        }
        return bundle;
    }

    public bool UnloadAssetBundle(string abname, bool unloadAssets, bool unloadDepends)
    {
        if (abname == "fonts.assetbundle")
            return true;

        abname = abname.ToLower();

        if (!abname.EndsWith(".assetbundle"))
        {
            abname += ".assetbundle";
        }

        if (DebugLevel > 1)
            Debug.LogWarning("UnloadAssetBundle " + abname);

        if (bundles.ContainsKey(abname))
        {
            bundles[abname].Unload(unloadAssets);

            bundles.Remove(abname);

            if (unloadDepends)
            {
                for (int i = 0; i < bundleDependencies[abname].Length; i++)
                {
                    UnloadAssetBundle(bundleDependencies[abname][i], unloadAssets, unloadDepends);
                }
            }
        }

        return false;
    }

   
    public GameObject LoadAsset(string abname, string assetname)
    {
        if (DebugLevel > 1)
            Debug.LogWarning("LoadAsset " + abname + " " + assetname);

        abname = abname.ToLower();
        AssetBundle bundle = LoadAssetBundle(abname);
        if (bundle == null)
        {
            Debug.LogError(String.Format("Load Asset {0} Failed From Bundle {1}", assetname, abname));
            return null;
        }

        var asset = bundle.LoadAsset<GameObject>(assetname);
        if (asset == null)
        {
            Debug.LogError(String.Format("Load Asset {0} Failed From Bundle {1} (Asset Not Found)", assetname, abname));
        }

        return asset;
    }

    public UnityEngine.Object LoadAsset(string abname, string assetname, string assettype)
    {
        if (DebugLevel > 1)
            Debug.LogWarning("LoadAsset " + abname + " " + assetname);

        abname = abname.ToLower();
        AssetBundle bundle = LoadAssetBundle(abname);

        //return (UnityEngine.Object) typeof (AssetBundle).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        //    .Single(x => x.Name == "LoadAsset" && x.IsGenericMethod &&
        //                 x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof (string))
        //    .MakeGenericMethod(LuaScriptMgr.GetType(assettype)).Invoke(bundle, new object[] {assetname});
        return bundle.LoadAsset(assetname, AppCtrl.GetType(assettype));
    }

    public class AssetLoader
    {
        public bool Done;
        public UnityEngine.Object Asset;

        private ResCtrl rc;

        public string abname;
        public string assetname;
        public string assettype;
        public AssetBundle bundle;

        public AssetLoader()
        {
            this.rc = ResCtrl.Instance;
        }

        public void Close()
        {
            rc = null;
            Asset = null;
            bundle = null;
        }

        public void LoadAsset(string abname, string assetname)
        {
            if (DebugLevel > 1)
                Debug.LogWarning("LoadAsset[Async] " + abname + " " + assetname);

            this.abname = abname;
            this.assetname = assetname;
            this.assettype = "";

            Done = false;

            rc.StartCoroutine(doLoadAsset());
        }

        public void LoadAsset(string abname, string assetname, string assettype)
        {
            if (DebugLevel > 1)
                Debug.LogWarning("LoadAsset[Async] " + abname + " " + assetname);

            this.abname = abname;
            this.assetname = assetname;
            this.assettype = assettype;

            Done = false;

            rc.StartCoroutine(doLoadAsset());
        }

        private IEnumerator doLoadAsset()
        {
            abname = abname.ToLower();
            yield return rc.StartCoroutine(LoadAssetBundle(abname));
            if (string.IsNullOrEmpty(assettype))
            {
                var r = bundle.LoadAssetAsync<GameObject>(assetname);
                while (!r.isDone)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                Asset = r.asset;
            }
            else
            {
                var r = bundle.LoadAssetAsync(assetname, AppCtrl.GetType(assettype));
                while (!r.isDone)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                Asset = r.asset;
            }
            Done = true;
        }

        private IEnumerator LoadAssetBundle(string abname)
        {
            if (!abname.EndsWith(".assetbundle"))
            {
                abname += ".assetbundle";
            }

            if (!rc.bundles.ContainsKey(abname))
            {
                byte[] stream = null;
                string uri = DataPath + abname;
                //Debug.Log("Loading AssetBundle[Async]: " + uri);

                var fileReader = new FileReader();
                fileReader.Load(uri);
                while (!fileReader.Done)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                stream = fileReader.Bytes;

                var r = AssetBundle.LoadFromMemoryAsync(stream);
                while (!r.isDone)
                {
                    yield return new WaitForSeconds(0.05f);
                }
                stream = null;
                fileReader.Bytes = null;

                rc.bundles.Add(abname, r.assetBundle);

                for (int i = 0; i < rc.bundleDependencies[abname].Length; i++)
                {
                    yield return rc.StartCoroutine(LoadAssetBundle(rc.bundleDependencies[abname][i]));
                }

                bundle = r.assetBundle;
            }
            else
            {
                rc.bundles.TryGetValue(abname, out bundle);
            }
        }

        private class FileReader
        {
            public bool Done;
            public byte[] Bytes;

            public string uri;

            public void Load(string uri)
            {
                this.uri = uri;

                Done = false;

                var readerThread = new Thread(readerThreadRoutine);
                readerThread.Start();
            }

            private void readerThreadRoutine()
            {
                Bytes = File.ReadAllBytes(uri);

                Done = true;
            }
        }
    }

    public class FileDownloder
    {
        public System.Diagnostics.Stopwatch sw;

        public string DownloadingFileName;
        public float DownloadingSpeed;
        public bool DownloadComplete;

        public FileDownloder()
        {
            sw = new System.Diagnostics.Stopwatch();
            DownloadComplete = false;
            DownloadingFileName = "";
        }

        public bool DownloadFile(string url, string file)
        {
            if (DownloadingFileName == "")
            {
                DownloadComplete = false;
                DownloadingFileName = file;

                using (WebClient client = new WebClient())
                {
                    sw.Start();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                    //Debug.Log(String.Format("Download URL:{0} FILE:{1}", url, file));
                    client.DownloadFileAsync(new Uri(url), file);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debug.LogError(e.Error.Message);
            }
            //Debug.Log("DownloadFileCompleted");
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadingSpeed = float.Parse((e.BytesReceived/1024d/sw.Elapsed.TotalSeconds).ToString("0.00"));

            if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
            {
                sw.Reset();

                DownloadComplete = true;
                DownloadingFileName = "";
            }
        }
    }


}