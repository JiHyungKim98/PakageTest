using MyAssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Diagnostics;

[Serializable]
public class TextureToolComponent : ToolComponent
{
    [SerializeField]
    private enum MaxSizeType
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
        _16384 = 16384
    }
    [SerializeField]
    private TextureImporterType _textureType = TextureImporterType.Default;
    [SerializeField]
    private BuildTarget _buildTarget = BuildTarget.NoTarget;

    [SerializeField]
    private int _selectedFormat = 0;
    [SerializeField]
    private MaxSizeType _maxSizeType = MaxSizeType._1024;

    [SerializeField]
    private TextureImportValidFormats _validFormatTable;
    [SerializeField]
    private List<TextureImporterFormat> validFormatList;

    [SerializeField]
    private bool isReadWriteEnabled;
    [SerializeField]
    private bool isMipMapGenerate;

    private ComponentToolsReport<TextureData> textureReport;

    public override void OnInitialize(SummaryReport summaryReport, TotalComponentToolsReport totalComponentToolsReport, PathDatabase pathDatabase)
    {
        base.OnInitialize(summaryReport, totalComponentToolsReport, pathDatabase);

        toolName = "Texture";
        textureReport = new TextureToolsReport();

        _validFormatTable = new TextureImportValidFormats();
        _validFormatTable.BuildValidFormats();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        EditorGUILayout.BeginVertical();
        {
            _textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", _textureType);
            _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildTarget);
            validFormatList = new List<TextureImporterFormat>(_validFormatTable.GetValidFormatList(_textureType, _buildTarget));

            if (validFormatList != null)
            {
                _selectedFormat = EditorGUILayout.Popup("Format", _selectedFormat, System.Array.ConvertAll(validFormatList.ToArray(), (v) => v.ToString()));
            }
            _maxSizeType = (MaxSizeType)EditorGUILayout.EnumPopup("Texture Max Size", _maxSizeType);
            isReadWriteEnabled = EditorGUILayout.Toggle("Read/Write Enabled", isReadWriteEnabled);
            isMipMapGenerate = EditorGUILayout.Toggle("Generate Mip Maps", isMipMapGenerate);
            searchFolderName = EditorGUILayout.TextField("Search Folder", searchFolderName);
            exceptionUIPopUp.DoLayoutList();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Check", GUILayout.Width(60)))
            {
                GetToolConfiguration();
                textureReport.MakeCheckReport(toolName);
            }

            if (GUILayout.Button("Change", GUILayout.Width(60)))
            {
                SetToolConfiguration();
                textureReport.MakeChangeReport(toolName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public override void GetToolConfiguration()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        base.GetToolConfiguration();

        textureReport.ClearReport();

        foreach (var texturePath in selectedFilePaths)
        {
            if (!searchFolderName.Equals(""))
            {
                if (!isContainFolder(texturePath))
                {
                    notScanFilesCnt++;
                    continue;
                }
            }

            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (textureImporter == null)
            {
                notScanFilesCnt++;
                continue;
            }

            UnityEngine.Debug.Log("Start");

            // 검사 완료 DB에 있는 데이터인지 확인
            if (_pathDatabase.isContainKey(texturePath))
                continue;
            else
            {
                notScanFilesCnt++;
                _pathDatabase.AddToDB(texturePath);
            }

            UnityEngine.Debug.Log("End");

            TextureImporterPlatformSettings textureInfo = textureImporter.GetPlatformTextureSettings(_buildTarget.ToString());
            if (validFormatList == null)
            {
                validFormatList = new List<TextureImporterFormat>();
            }
            if (validFormatList.Count == 0)
            {
                validFormatList.AddRange(_validFormatTable.GetValidFormatList(_textureType, _buildTarget));
            }

            if (textureInfo.maxTextureSize <= (int)_maxSizeType
                && textureInfo.format.Equals(validFormatList[_selectedFormat])
                && textureInfo.overridden
                && textureImporter.mipmapEnabled == isMipMapGenerate
                && textureImporter.isReadable == isReadWriteEnabled)
                continue;

            TextureData checkData = new TextureData();
            TextureData changeData = new TextureData();

            if (textureInfo.overridden == false)
                checkData.etc = _buildTarget + "override 꺼져있음";

            checkData.path = texturePath;
            checkData.size = textureInfo.maxTextureSize;
            checkData.isReadWriteEnabled = textureImporter.isReadable;
            checkData.isMipMapGenerate = textureImporter.mipmapEnabled;
            checkData.format = textureInfo.format.ToString();
            checkData.platform = _buildTarget.ToString();

            changeData.path = texturePath;
            changeData.size = textureInfo.maxTextureSize;
            changeData.isReadWriteEnabled = textureImporter.isReadable;
            changeData.isMipMapGenerate = textureImporter.mipmapEnabled;
            changeData.format = textureInfo.format.ToString();
            changeData.platform = _buildTarget.ToString();

            textureReport.AddCheckData(checkData);
            textureReport.AddChangeData(changeData);
        }
        _summaryData.SaveSummaryData(selectedFilePaths.Count, textureReport.GetCount(), this, notScanFilesCnt);
        _totalComponentToolsReport.AddToTotalCheckData(textureReport.GetCheckData());

        stopwatch.Stop();
        UnityEngine.Debug.Log("First Try"+stopwatch.ElapsedMilliseconds);
    }

    public override void SetToolConfiguration()
    {
        var textureDatas = textureReport.GetChangeData();

        if (textureReport.GetCount() == 0)
            UnityEngine.Debug.Log("Click Show Button First!");

        for (int i = 0; i < textureReport.GetCount(); i++)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(textureDatas[i].path) as TextureImporter;
            TextureImporterPlatformSettings textureInfo = textureImporter.GetPlatformTextureSettings(_buildTarget.ToString());
            textureInfo.overridden = true;

            if (textureInfo.maxTextureSize > int.Parse(_maxSizeType.ToString().Substring(1)))
            {
                textureInfo.maxTextureSize = int.Parse(_maxSizeType.ToString().Substring(1));
                textureDatas[i].size = int.Parse(_maxSizeType.ToString().Substring(1));
            }

            if (textureInfo.format == TextureImporterFormat.Automatic || textureInfo.format == TextureImporterFormat.ETC2_RGBA8)
            {
                textureInfo.format = validFormatList[_selectedFormat];
                textureDatas[i].format = validFormatList[_selectedFormat].ToString();
            }

            textureImporter.SetPlatformTextureSettings(textureInfo);
            AssetDatabase.ImportAsset(textureDatas[i].path);
        }
        _totalComponentToolsReport.AddToTotalChangeData(textureReport.GetChangeData());
    }

    public override void OnRelease()
    {
        base.OnRelease();
        exceptionUIPopUp = null;
        textureReport = null;
        _validFormatTable = null;
        validFormatList = null;
    }
}


