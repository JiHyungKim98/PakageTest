using MyAssetTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class AudioToolComponent : ToolComponent
{
    [SerializeField]
    private BuildTarget _buildTarget = BuildTarget.NoTarget;
    [SerializeField]
    private int _selectedFormat = 0;
    [SerializeField]
    private bool forceToMono;
    [SerializeField]
    private int audioLength;

    private BuildTargetGroup platform;

    private ComponentToolsReport<AudioData> audioReport;

    private List<AudioCompressionFormat> _validFormat;
    private AudioImportValidFomats audioImportValidFomats;

    
    public enum VertexInequality
    {
        GreaterThan,
        LessThan,
    }
    VertexInequality vertexInequality = VertexInequality.GreaterThan;

    public List<string> loadTypeList;
    public int selectedLoadType = 0;

    public override void OnInitialize(SummaryReport summaryReport, TotalComponentToolsReport totalComponentToolsReport, PathDatabase pathDatabase)
    {
        base.OnInitialize(summaryReport, totalComponentToolsReport, pathDatabase);

        toolName = "Audio";
        audioReport = new AudioToolsReport();
        audioImportValidFomats = new AudioImportValidFomats();
        _validFormat = new List<AudioCompressionFormat>();
        loadTypeList = new List<string>(Enum.GetNames(typeof(AudioClipLoadType)).ToList());
    }
    public override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginVertical();
        {
            _buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildTarget);
            platform = BuildPipeline.GetBuildTargetGroup(_buildTarget);
            _validFormat = audioImportValidFomats.GetFormatsForPlatform(platform);
            _selectedFormat = EditorGUILayout.Popup("Format", _selectedFormat, System.Array.ConvertAll(_validFormat.ToArray(), (v) => v.ToString()));
            selectedLoadType = EditorGUILayout.Popup("Load Type", selectedLoadType, System.Array.ConvertAll(loadTypeList.ToArray(), (v) => v.ToString()));
            EditorGUILayout.BeginHorizontal();
            {
                vertexInequality = (VertexInequality)EditorGUILayout.EnumPopup("Audio Length(int)", vertexInequality);
                audioLength = EditorGUILayout.IntField(audioLength);
                EditorGUILayout.LabelField("sec");
            }
            EditorGUILayout.EndHorizontal();
            forceToMono = EditorGUILayout.Toggle("Force To Mono", forceToMono);
            searchFolderName = EditorGUILayout.TextField("Search Folder", searchFolderName);
            exceptionUIPopUp.DoLayoutList();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Check", GUILayout.Width(60)))
            {
                GetToolConfiguration();
                audioReport.MakeCheckReport(toolName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public override void GetToolConfiguration()
    {
        base.GetToolConfiguration();

        audioReport.ClearReport();

        foreach (var audioPath in selectedFilePaths)
        {
            if (!searchFolderName.Equals(""))
            {
                if (!isContainFolder(audioPath))
                {
                    notScanFilesCnt++;
                    continue;
                }
            }

            AudioImporter audioImporter = AssetImporter.GetAtPath(audioPath) as AudioImporter;

            if (audioImporter == null)
            {
                notScanFilesCnt++;
                continue;
            }

            // 검사 완료 DB에 있는 데이터인지 확인
            if (_pathDatabase.isContainKey(audioPath))
                continue;
            else
            {
                notScanFilesCnt++;
                _pathDatabase.AddToDB(audioPath);
            }

            if (_validFormat == null)
            {
                _validFormat = new List<AudioCompressionFormat>();
            }

            if (_validFormat.Count == 0)
            {
                _validFormat.AddRange(audioImportValidFomats.GetFormatsForPlatform(platform));
            }

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);

            if (CompareClipSize(clip))
                continue;

            if (audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).compressionFormat.Equals(_validFormat[_selectedFormat])
                && audioImporter.forceToMono.Equals(forceToMono)
                && audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).loadType.ToString().Equals(loadTypeList[selectedLoadType])
                )
                continue;

            AudioData checkData = new AudioData();
            AudioData changeData = new AudioData();

            if (!audioImporter.ContainsSampleSettingsOverride(_buildTarget.ToString()))
                checkData.etc = _buildTarget + "override 꺼져있음";

            checkData.path = audioPath;
            checkData.platform = _buildTarget.ToString();
            checkData.format = audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).compressionFormat.ToString();
            checkData.loadType = audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).loadType.ToString();
            checkData.clipSize = clip.length;
            checkData.forceToMono = audioImporter.forceToMono;

            changeData.path = audioPath;
            changeData.platform = _buildTarget.ToString();
            changeData.format = audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).compressionFormat.ToString();
            changeData.loadType = audioImporter.GetOverrideSampleSettings(_buildTarget.ToString()).loadType.ToString();
            changeData.clipSize = clip.length;
            changeData.forceToMono = forceToMono;

            audioReport.AddCheckData(checkData);
            audioReport.AddChangeData(changeData);
        }
        _summaryData.SaveSummaryData(selectedFilePaths.Count, audioReport.GetCount(), this, notScanFilesCnt);
        _totalComponentToolsReport.AddToTotalCheckData(audioReport.GetCheckData());
    }

    public bool CompareClipSize(AudioClip clip)
    {
        switch (vertexInequality)
        {
            case VertexInequality.GreaterThan:
                if (clip.length < audioLength)
                    return true;
                else
                    return false;
            case VertexInequality.LessThan:
                if (clip.length > audioLength)
                    return true;
                else
                    return false;
            default:
                return true;
        }
    }

    #region Audio Component 변경 필요할 때 사용
    //public override void SetToolConfiguration()
    //{
    //if (audioReport.checkCSV.Count == 0)
    //    Debug.Log("Click Show Button First!");

    //for (int i = 0; i < audioReport.checkCSV.Count; i++)
    //{
    //    AudioImporter audioImporter = AssetImporter.GetAtPath(audioReport.checkCSV[i].path) as AudioImporter;
    //    AudioImporterSampleSettings audioImporterSampleSettings = new AudioImporterSampleSettings();

    //    audioImporter.forceToMono = forceToMono;
    //    audioImporterSampleSettings.compressionFormat = _validFormat[_selectedFormat];

    //    audioReport.changeCSV[i].format = _validFormat[_selectedFormat].ToString();
    //    audioReport.changeCSV[i].forceToMono = forceToMono;

    //    audioImporter.SetOverrideSampleSettings(_buildTarget.ToString(), audioImporterSampleSettings);
    //    AssetDatabase.ImportAsset(audioReport.changeCSV[i].path);
    //}
    //}
    #endregion

    public override void OnRelease()
    {
        base.OnRelease();
        audioReport = null;
        exceptionUIPopUp = null;
        _validFormat = null;
        audioImportValidFomats = null;
    }
}
