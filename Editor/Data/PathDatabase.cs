using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System;

public class MyAllPostprocessor : AssetPostprocessor
{
    static PathDatabase pathDatabase;

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (pathDatabase == null)
        {
            Debug.Log("pathDB null!");
            return;
        }

        // 스크립트 편집에 의한 import의 경우
        if (Event.current == null)
            return;

        foreach (string importedAsset in importedAssets)
        {
            var guid = pathDatabase.PathToGUID(importedAsset);

            // 이미 검사한 data path 항목인데 새로 import 된 경우
            if (pathDatabase.isContainKey(guid))
            {
                // 검사완료 DB에서 제거
                pathDatabase.deletePath(importedAsset);
            }
        }
        foreach (string deletedAsset in deletedAssets)
        {
            var guid = pathDatabase.PathToGUID(deletedAsset);

            // 검사완료 DB에 있는 항목인 경우
            if (pathDatabase.isContainKey(deletedAsset))
                pathDatabase.deletePath(deletedAsset); // 제거
        }
        // 옮긴 후 path
        foreach (string movedAsset in movedAssets)
        {
            var guid = pathDatabase.PathToGUID(movedAsset);

            // 검사 완료 DB에 없는 경우 새로 추가
            if (pathDatabase.isContainKey(movedAsset))
                pathDatabase.AddToDB(movedAsset);
        }
        // 옮기기 전 path
        foreach (string movedFromAssetPath in movedFromAssetPaths)
        {
            var guid = pathDatabase.PathToGUID(movedFromAssetPath);

            // 검사 완료 DB에 있는 경우 => path 변경을 위해 제거
            if (pathDatabase.isContainKey(movedFromAssetPath))
                pathDatabase.deletePath(movedFromAssetPath);
        }
    }
}

public class PathDatabase : ScriptableObject
{
    public DictionarySO dictionarySO = (DictionarySO)AssetDatabase.LoadAssetAtPath<DictionarySO>("Packages/com.opttool.package/Editor/Data/NewDic.asset");
        //LoadAssetAtPath("Assets/Editor/MyTool/Data/NewDic.asset", typeof(DictionarySO));

    public PathDatabase()
    {
        if (dictionarySO == null)
        {
            var instance = Editor.CreateInstance<DictionarySO>();
            AssetDatabase.DeleteAsset("Packages/com.opttool.package/Editor/Data/NewDic.asset");
            AssetDatabase.CreateAsset(instance, "Packages/com.opttool.package/Editor/Data/NewDic.asset");
            AssetDatabase.SaveAssets();
            dictionarySO = (DictionarySO)AssetDatabase.LoadAssetAtPath<DictionarySO>("Packages/com.opttool.package/Editor/Data/NewDic.asset");
            //(DictionarySO)AssetDatabase.LoadAssetAtPath("Assets/Editor/MyTool/Data/NewDic.asset", typeof(DictionarySO));
        }
        else
            dictionarySO.myDic = new DictionaryWrapper<string, string>();
    }

    public string PathToGUID(string path)
    {
        return AssetDatabase.AssetPathToGUID(path);
    }
    public void AddToDB(string path)
    {
        if (!isContainKey(path))
        {
            dictionarySO.myDic.Add(PathToGUID(path), path);
            UpdateDB();
        }
    }
    public bool isContainKey(string path)
    {
        if (dictionarySO.myDic.ContainsKey(PathToGUID(path)))
            return true;
        else
            return false;
    }
    public void deletePath(string path)
    {
        if (isContainKey(path))
        {
            dictionarySO.myDic.Remove(PathToGUID(path));
            UpdateDB();
        }
    }
    private void UpdateDB()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        var enumer = GetEnumerator();
        while (enumer.MoveNext())
        {
            keys.Add(enumer.Current.Key);
            values.Add(enumer.Current.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new Exception(string.Format("there are {0} keys and {1} values after desc"));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}

[Serializable]
public class DictionaryWrapper<TKey, TValue> : SerializableDictionary<TKey, TValue> { }


[CreateAssetMenu(fileName = "NewDic", menuName = "Dic/DicTest")]
public class DictionarySO : ScriptableObject
{
    public DictionaryWrapper<string, string> myDic;
}
