using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using LitJson;
//using SimpleJSON;
using System.Collections.Generic;
public class ExcelToJson{

    [MenuItem("ExcelToJson/ExcelToJson")]
	static void excelToJson()
	{

		string dataFolderPath=Application.dataPath+"/Data";
        string outJsonPath = Application.dataPath + "/Resources/Json";
		if(!Directory.Exists(dataFolderPath))
		{
			Debug.LogError("请建立"+dataFolderPath+" 文件夹，并且把csv文件放入此文件夹内");
			return;
		}


		string[] allCSVFiles=Directory.GetFiles(dataFolderPath,"*.csv");
		if(allCSVFiles==null||allCSVFiles.Length<=0)
		{
			Debug.LogError(""+dataFolderPath+" 文件夹没有csv文件,请放入csv文件到此文件夹内");
			return;
		}
        //每次需要重新
        if (Directory.Exists(outJsonPath))
        {
            Directory.Delete(outJsonPath,true);
        }
		if(!Directory.Exists(outJsonPath))
		{
			Directory.CreateDirectory(outJsonPath);
		}

		for(int i=0;i<allCSVFiles.Length;i++)
		{
			string dictName=new DirectoryInfo(Path.GetDirectoryName(allCSVFiles[i])).Name;
			string fileName=Path.GetFileNameWithoutExtension(allCSVFiles[i]);

			string jsonData=readExcelDataByLitJson(allCSVFiles[i]);
			outJsonContentToFile(jsonData,outJsonPath+"/"+dictName+"/"+fileName+".json");
		}

	}

    static string readExcelDataByLitJson(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }
        string fileContent = File.ReadAllText(fileName, UnicodeEncoding.Default);
        string[] fileLineContent = fileContent.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
        if (fileLineContent != null)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, System.Object>>> listAll = new Dictionary<string, Dictionary<string, Dictionary<string, System.Object>>>();
            
            Dictionary<string, Dictionary<string, System.Object>> list2 = new Dictionary<string, Dictionary<string, object>>();

            //注释的名字
            string[] noteContents = fileLineContent[0].Split(new string[] { "," }, System.StringSplitOptions.None);
            string[] VariableType = fileLineContent[1].Split(new string[] { "," }, System.StringSplitOptions.None);
            //变量的名字
            string[] VariableNameContents = fileLineContent[2].Split(new string[] { "," }, System.StringSplitOptions.None);

            //JSONClass jsonData = new JSONClass();
            for (int i = 3; i < fileLineContent.Length - 1; i++)//从内容行开始
            {
                string[] lineContents = fileLineContent[i].Split(new string[] { "," }, System.StringSplitOptions.None); //第一行按照，分割
                //JSONClass classLine = new JSONClass();
                Dictionary<string, System.Object> list1 = new Dictionary<string, System.Object>();
                for (int j = 0; j < lineContents.Length; j++)
                {
                    if (VariableType[j] == "int")
                    {
                        list1[VariableNameContents[j]] = int.Parse(lineContents[j]);
                    }
                    else if (VariableType[j] == "float")
                    {
                        list1[VariableNameContents[j]] = float.Parse(lineContents[j]);
                    }
                    else if (VariableType[j] == "string")
                    {
                        list1[VariableNameContents[j]] = lineContents[j];
                    }
                }
                list2[lineContents[0].ToString()] = list1;
            }
            listAll["list"] = list2;
            string sJsonOut = JsonMapper.ToJson(listAll);

            return sJsonOut;
        }
        return null;
    }
	static void outJsonContentToFile(string jsonData,string jsonFilePath)
	{
		string directName=Path.GetDirectoryName(jsonFilePath);
		if(!Directory.Exists(directName))
		{
			Directory.CreateDirectory(directName);
		}
		File.WriteAllText(jsonFilePath,jsonData,Encoding.UTF8);
		Debug.Log("成功输出Json文件  :"+jsonFilePath);
	}

}
