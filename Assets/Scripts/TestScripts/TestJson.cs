using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class myframe
{
    public int x;
    public int y;
    public int w;
    public int h;
}

public class mySpriteSourceSize
{
    public int x;
    public int y;
    public int w;
    public int h;
}

public class mySourceSize
{
    public int w;
    public int h;
}


public class myPng
{
    public mySourceSize sourceSize;
    public mySpriteSourceSize spriteSourceSize;
    public myframe frame;
    public bool rotated;
    public bool trimmed;
}

public class myPngs
{
    public Dictionary<string, myPng> frames;

}

public class TestItem
{
    public int id;
    public int itemId; //只会读取和项目名一致的
    public string name;
    public float ff;
}
public class TestItems
{
    public Dictionary<string, TestItem> list;
}
public class TestJson : MonoBehaviour
{

    void Start()
    {
        var textObj = Resources.Load("mytestui") as TextAsset;

        myPngs pngs = JsonMapper.ToObject<myPngs>(textObj.text);

        foreach (var i in pngs.frames)
        {
            print(i.Key + " " + i.Value.rotated);
        }

        var textItem = Resources.Load("Json/Data/TestJson") as TextAsset;
        TestItems items = JsonMapper.ToObject<TestItems>(textItem.text);

        foreach (var i in items.list)
        {
            print(i.Key + " " + i.Value.name);
        }
        
        //Dictionary<string,Dictionary<string, Dictionary<string, System.Object>>> listAll = new Dictionary<string, Dictionary<string, Dictionary<string, System.Object>>>();
        //Dictionary<string, System.Object> list1 = new Dictionary<string, System.Object>();
        //list1["itemId"] = 123;
        //list1["Name"] = "luoyikun";
        //Dictionary<string, Dictionary<string, System.Object>> list2 = new Dictionary<string, Dictionary<string, object>>();
        //list2["1"] = list1;
        //listAll["list"] = list2;
        //string json_bill = JsonMapper.ToJson(listAll);
        //Debug.Log(json_bill);
    }
}

