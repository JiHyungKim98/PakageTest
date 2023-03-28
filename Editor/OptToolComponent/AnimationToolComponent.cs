using MyAssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class AnimationToolComponent : ToolComponent
{
    [SerializeField]
    private int selectedKeyFrame;
    [SerializeField]
    private List<int> keyFrameCount;

    private ComponentToolsReport<AnimationData> aniReport;

    public override void OnInitialize(SummaryReport summaryReport, TotalComponentToolsReport totalComponentToolsReport, PathDatabase pathDatabase)
    {
        base.OnInitialize(summaryReport, totalComponentToolsReport, pathDatabase);
        toolName = "Animation";
        aniReport = new AnimationToolsReport();
    }
    public override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.BeginVertical();
        {
            selectedKeyFrame = EditorGUILayout.IntField("KeyFrame", selectedKeyFrame);
            searchFolderName = EditorGUILayout.TextField("Search Folder", searchFolderName);
            exceptionUIPopUp.DoLayoutList();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Check", GUILayout.Width(60)))
            {
                GetToolConfiguration();
                aniReport.MakeCheckReport(toolName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public override void GetToolConfiguration()
    {
        base.GetToolConfiguration();
        aniReport.ClearReport();

        foreach (var aniPath in selectedFilePaths)
        {
            if (!searchFolderName.Equals(""))
            {
                if (!isContainFolder(aniPath))
                {
                    notScanFilesCnt++;
                    continue;
                }  
            }

            AnimationClip aniClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(aniPath, typeof(AnimationClip));

            if (aniClip == null)
            {
                notScanFilesCnt++;
                continue;
            }

            // 검사 완료 DB에 있는 데이터인지 확인
            if (_pathDatabase.isContainKey(aniPath))
                continue;
            else
            {
                notScanFilesCnt++;
                _pathDatabase.AddToDB(aniPath);
            }

            keyFrameCount = new List<int>();

            var allbindlings = AnimationUtility.GetCurveBindings(aniClip);
            int count = 0;
            foreach (var bind in allbindlings)
            {
                var keyframes = AnimationUtility.GetObjectReferenceCurve(aniClip, bind);

                if (keyframes != null)
                    count += keyframes.Length;

                else
                {
                    var curves = AnimationUtility.GetEditorCurve(aniClip, bind);
                    count += curves.keys.Length;
                }
            }

            if (count > selectedKeyFrame)
            {
                AnimationData aniData = new AnimationData();

                aniData.path = aniPath;
                aniData.keyframes = count;

                aniReport.AddCheckData(aniData);
            }
        }
        _summaryData.SaveSummaryData(selectedFilePaths.Count, aniReport.GetCount(), this, notScanFilesCnt);
        _totalComponentToolsReport.AddToTotalCheckData(aniReport.GetCheckData());
    }

    public override void SetToolConfiguration() { }
    public override void OnRelease()
    {
        base.OnRelease();
        aniReport = null;
    }
}
