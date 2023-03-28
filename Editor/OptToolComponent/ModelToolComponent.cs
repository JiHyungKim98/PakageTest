using MyAssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class ModelToolComponent : ToolComponent
{
    [SerializeField]
    private int vertices;

    [SerializeField]
    private int selectedInequality;

    [SerializeField]
    private enum VertexInequality
    {
        GreaterThan,
        LessThan,
    }
    private VertexInequality vertexInequality = VertexInequality.GreaterThan;
    private ComponentToolsReport<ModelData> modelReport;

    public override void OnInitialize(SummaryReport summaryReport, TotalComponentToolsReport totalComponentToolsReport, PathDatabase pathDatabase) 
    {
        base.OnInitialize(summaryReport, totalComponentToolsReport, pathDatabase);
        toolName = "Model";
        modelReport = new ModelToolsReport();
    }
    public override void OnGUI() 
    {
        base.OnGUI();
        
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                vertices = EditorGUILayout.IntField("Vertices", vertices);
                vertexInequality = (VertexInequality)EditorGUILayout.EnumPopup(vertexInequality);
            }
            EditorGUILayout.EndHorizontal();
            searchFolderName = EditorGUILayout.TextField("Search Folder", searchFolderName);
            exceptionUIPopUp.DoLayoutList();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Check", GUILayout.Width(60)))
            {
                GetToolConfiguration();
                modelReport.MakeCheckReport(toolName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public override void GetToolConfiguration()
    {
        base.GetToolConfiguration();
        modelReport.ClearReport();
        
        foreach (var modelPath in selectedFilePaths)
        {
            if (!searchFolderName.Equals(""))
            {
                if (!isContainFolder(modelPath))
                {
                    notScanFilesCnt++;
                    continue;
                }
            }

            Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>(modelPath);

            if (mesh == null)
            {
                notScanFilesCnt++;
                continue;
            }

            // 검사 완료 DB에 있는 데이터인지 확인
            if (_pathDatabase.isContainKey(modelPath))
                continue;
            else
            {
                notScanFilesCnt++;
                _pathDatabase.AddToDB(modelPath);
            }

            switch (vertexInequality)
            {
                case VertexInequality.GreaterThan:
                    if (mesh.vertexCount < vertices)
                        continue;
                    break;
                case VertexInequality.LessThan:
                    if (mesh.vertexCount > vertices)
                        continue;
                    break;
            }

            ModelData modelData = new ModelData();

            modelData.path = modelPath;
            modelData.vertices = mesh.vertexCount;

            modelReport.AddCheckData(modelData);
        }
        _summaryData.SaveSummaryData(selectedFilePaths.Count, modelReport.GetCount(), this, notScanFilesCnt);
        _totalComponentToolsReport.AddToTotalCheckData(modelReport.GetCheckData());
    }

    public override void SetToolConfiguration(){ }
    public override void OnRelease() 
    {
        base.OnRelease();
        modelReport = null;
    }
}
