using MyAssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public abstract class ToolComponent
{
    [HideInInspector]
    public bool toggle = false;
    [HideInInspector]
    public int sortingOrder;

    protected List<string> selectedFilePaths;
    protected string searchFolderName = "";
    protected List<string> exceptFilePaths = new List<string>();

    protected ReorderableList exceptionUIPopUp;

    protected string toolName;
    protected int notScanFilesCnt = 0;

    protected SummaryReport _summaryData;
    protected TotalComponentToolsReport _totalComponentToolsReport;

    protected PathDatabase _pathDatabase;

    public virtual void OnInitialize(SummaryReport summaryData, TotalComponentToolsReport totalComponentToolsReport, PathDatabase pathDatabase)
    {
        _summaryData = summaryData;
        _pathDatabase = pathDatabase;
        _totalComponentToolsReport = totalComponentToolsReport;

        exceptionUIPopUp = new ReorderableList(exceptFilePaths, typeof(string));
        exceptionUIPopUp.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Exception File");
        exceptionUIPopUp.drawElementCallback = (Rect rt, int idx, bool isActive, bool isFocused) =>
        {
            rt.y += 2f;
            rt.height = EditorGUIUtility.singleLineHeight;

            List<string> list = exceptionUIPopUp.list as List<string>;
            list[idx] = EditorGUI.TextField(rt, list[idx]);
        };
    }
    public virtual void OnGUI()
    {
        toggle = GUILayout.Toggle(toggle, toolName);
    }
    public virtual void GetToolConfiguration()
    {
        notScanFilesCnt = 0;

        selectedFilePaths = new List<string>(AssetDatabase.GetAllAssetPaths().Where(x => x.Substring(0, 6).Equals("Assets")));

        foreach (var exceptFile in exceptFilePaths)
        {
            if (exceptFile.Equals(null))
                continue;

            selectedFilePaths.RemoveAll(x => x.Contains(exceptFile));
        }
    }
    protected bool isContainFolder(string path)
    {
        var folders = path.Split('/', '\\');
        if (!folders.Contains(searchFolderName))
            return false;
        return true;
    }
    public virtual void SetToolConfiguration() { }
    public virtual void OnRelease() { }

    public void makeTotalCheckReport(List<ToolComponent> _tools)
    {
        _totalComponentToolsReport.MakeTotalCheckReport(_tools);
    }
    public void makeTotalChangeReport(List<ToolComponent> _tools)
    {
        _totalComponentToolsReport.MakeTotalChangeReport(_tools);
    }
    public void ClearCheckReport()
    {
        _totalComponentToolsReport.ClearTotalReport();
    }
}
public class ToolBoxWindow : EditorWindow
{
    [MenuItem("OptimizationTools/Tool Box")]
    public static void OpenWindow()
    {
        GetWindow<ToolBoxWindow>();
    }

    Vector2 _scrollPos = Vector2.zero;

    EditorCachingData cachingData;

    List<ToolComponent> _tools;

    SummaryReport sumData;
    PathDatabase pathDatabase;

    TotalComponentToolsReport totalComponentToolsReport;

    private void OnEnable()
    {
        cachingData = (EditorCachingData)AssetDatabase.LoadAssetAtPath("Packages/com.opttool.package/Editor/Data/cachingData.asset", typeof(EditorCachingData));

        sumData = new SummaryReport();
        pathDatabase = new PathDatabase();

        totalComponentToolsReport = new TotalComponentToolsReport();

        if (cachingData == null)
        {
            Debug.Log("CachingData 없어서 새로 만들겠습니다~");
            var tmp = Editor.CreateInstance<EditorCachingData>();
            AssetDatabase.CreateAsset(tmp, "Packages/com.opttool.package/Editor/Data/cachingData.asset");
            AssetDatabase.SaveAssets();
            cachingData = (EditorCachingData)AssetDatabase.LoadAssetAtPath("Packages/com.opttool.package/Editor/Data/cachingData.asset", typeof(EditorCachingData));
            _tools = new List<ToolComponent>();

        }
        else
        {
            var tmpDatas = cachingData.LoadToolCachingData();
            if (tmpDatas == null)
                _tools = new List<ToolComponent>();
            else
                _tools = new List<ToolComponent>(tmpDatas);
        }

        foreach (var tmp in _tools)
            tmp.OnInitialize(sumData, totalComponentToolsReport, pathDatabase);
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Texture"))
                {
                    TextureToolComponent textureTool = new TextureToolComponent();
                    textureTool.OnInitialize(sumData, totalComponentToolsReport, pathDatabase);
                    _tools.Add(textureTool);
                    cachingData.SaveToolCachingData(_tools);
                }
                if (GUILayout.Button("Audio"))
                {
                    AudioToolComponent audioTool = new AudioToolComponent();
                    audioTool.OnInitialize(sumData, totalComponentToolsReport, pathDatabase);
                    _tools.Add(audioTool);
                    cachingData.SaveToolCachingData(_tools);
                }
                if (GUILayout.Button("Model"))
                {
                    ModelToolComponent modelTool = new ModelToolComponent();
                    modelTool.OnInitialize(sumData, totalComponentToolsReport, pathDatabase);
                    _tools.Add(modelTool);
                    cachingData.SaveToolCachingData(_tools);
                }
                if (GUILayout.Button("Animation"))
                {
                    AnimationToolComponent animationTool = new AnimationToolComponent();
                    animationTool.OnInitialize(sumData, totalComponentToolsReport, pathDatabase);
                    _tools.Add(animationTool);
                    cachingData.SaveToolCachingData(_tools);
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Remove"))
            {
                for (int index = _tools.Count - 1; index >= 0; index--)
                {
                    if (_tools[index].toggle)
                    {
                        _tools.RemoveAt(index);
                    }
                }
                cachingData.SaveToolCachingData(_tools);
            }
            EditorLineDrawGUI.DrawSeparatorHorizontal(2, 0, Color.grey);

            foreach (var tool in _tools)
            {
                tool?.OnGUI();
                EditorLineDrawGUI.DrawSeparatorHorizontal(2, 0, Color.grey);
            }

            if (GUILayout.Button("Total Check"))
            {
                OnClickTotalCheckButton();
            }

            if (GUILayout.Button("Total Change"))
            {
                if (_tools.Count == 0)
                {
                    Debug.Log("검사 항목이 없습니다!");
                }
                else
                {
                    foreach (var tool in _tools)
                    {
                        tool.SetToolConfiguration();
                    }
                    cachingData.SaveToolCachingData(_tools);
                    _tools[0].makeTotalChangeReport(_tools);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
    private void OnDisable()
    {
        cachingData.SaveToolCachingData(_tools);

        foreach (var tool in _tools)
            tool.OnRelease();

        cachingData = null;
    }

    public void OnClickTotalCheckButton()
    {
        if (_tools.Count == 0)
        {
            Debug.Log("검사 항목이 없습니다");
        }
        else
        {
            totalComponentToolsReport.ClearTotalReport();
            foreach (var tool in _tools)
            {
                tool.GetToolConfiguration();
            }
            cachingData.SaveToolCachingData(_tools);
            _tools[0].makeTotalCheckReport(_tools);
        }

    }
}



