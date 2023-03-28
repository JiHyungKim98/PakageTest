using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public abstract class ComponentToolsReport<T> where T : Data
{
    protected List<T> checkCSV;
    protected List<T> changeCSV;

    public ComponentToolsReport()
    {
        checkCSV = new List<T>();
        changeCSV = new List<T>();
    }
    public void AddCheckData(T data)
    {
        checkCSV.Add(data);
    }
    public void AddChangeData(T data)
    {
        changeCSV.Add(data);
    }
    public int GetCount()
    {
        return checkCSV.Count;
    }
    public List<T> GetCheckData()
    {
        return checkCSV;
    }
    public List<T> GetChangeData()
    {
        return changeCSV;
    }
    public void ClearReport()
    {
        checkCSV.Clear();
        changeCSV.Clear();
    }

    public virtual void MakeCheckReport(string toolName) { }
    public virtual void MakeChangeReport(string toolName) { }
    protected virtual void CheckDataCopyToChangeData(int index) { }
    protected string MakeReportTitle(string toolName)
    {
        return $"{toolName}_{DateTime.Now.ToString("yyyy.MM.dd.HH.mm")}";
    }
}
public class TextureToolsReport : ComponentToolsReport<TextureData>
{
    public override void MakeCheckReport(string toolName)
    {
        using (StreamWriter checkReport = new StreamWriter(MakeReportTitle(toolName) + @"_Check_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            checkReport.WriteLine("Asset Path,Platform,Max Size,Read/Write Enabled,Generate Mip Maps,Format, ETC");
            for (int i = 0; i < checkCSV.Count; i++)
            {
                checkReport.WriteLine($"{checkCSV[i].path}, {checkCSV[i].platform}, {checkCSV[i].size},{checkCSV[i].isReadWriteEnabled},{checkCSV[i].isMipMapGenerate}, {checkCSV[i].format},{checkCSV[i].etc}");
            }
        }
    }
    public override void MakeChangeReport(string toolName)
    {
        using (StreamWriter changeReport = new StreamWriter(MakeReportTitle(toolName) + @"_Change_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            changeReport.WriteLine("Asset Path,Target Platform,before Max Size,after Max Size,before Format,after Format");
            for (int i = 0; i < changeCSV.Count; i++)
            {
                changeReport.WriteLine($"{checkCSV[i].path}, {checkCSV[i].platform}, {checkCSV[i].size}, {changeCSV[i].size},{checkCSV[i].format}, {changeCSV[i].format}");
                CheckDataCopyToChangeData(i);
            }
        }
    }
    protected override void CheckDataCopyToChangeData(int index)
    {
        checkCSV[index].size = changeCSV[index].size;
        checkCSV[index].isReadWriteEnabled = changeCSV[index].isReadWriteEnabled;
        checkCSV[index].isMipMapGenerate = changeCSV[index].isMipMapGenerate;
        checkCSV[index].format = changeCSV[index].format;
        checkCSV[index].platform = changeCSV[index].platform;
    }
}
public class AudioToolsReport : ComponentToolsReport<AudioData>
{
    public override void MakeCheckReport(string toolName)
    {
        using (StreamWriter checkReport = new StreamWriter(MakeReportTitle(toolName) + @"_Check_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            checkReport.WriteLine("Asset Path,Target Platform,Format,Load Type,Audio Length,Force To Mono, Extra etc");
            for (int i = 0; i < checkCSV.Count; i++)
            {
                checkReport.WriteLine($"{checkCSV[i].path}, {checkCSV[i].platform}, {checkCSV[i].format},{checkCSV[i].loadType},{checkCSV[i].clipSize}, {checkCSV[i].forceToMono}, {checkCSV[i].etc}");
            }
        }
    }

    #region Audio Component 변경 필요할 때 사용
    //public override void MakeChangeReport(string toolName)
    //{
    //    using (StreamWriter changeReport = new StreamWriter(MakeReportTitle(toolName) + @"_Change_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
    //    {
    //        changeReport.WriteLine("Asset Path,Platform,before Format,after Format,before Force To Mono,after Force To Mono,Extra Etc");
    //        for (int i = 0; i < changeCSV.Count; i++)
    //        {
    //            changeReport.WriteLine("{0},{1},{2},{3},{4},{5},{6}", checkCSV[i].path, checkCSV[i].platform, checkCSV[i].format, changeCSV[i].format, checkCSV[i].forceToMono, changeCSV[i].forceToMono, changeCSV[i].etc);
    //            CheckDataCopyToChangeData(i);
    //        }
    //    }
    //}
    //public override void CheckDataCopyToChangeData(int index)
    //{
    //    checkCSV[index].format = changeCSV[index].format;
    //    checkCSV[index].platform = changeCSV[index].platform;
    //}
    #endregion
}
public class ModelToolsReport : ComponentToolsReport<ModelData>
{
    public override void MakeCheckReport(string toolName)
    {
        using (StreamWriter checkReport = new StreamWriter(MakeReportTitle(toolName) + @"_Check_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            checkReport.WriteLine("Asset Path,Vertices Count");
            for (int i = 0; i < checkCSV.Count; i++)
            {
                checkReport.WriteLine($"{checkCSV[i].path}, {checkCSV[i].vertices}");
            }
        }
    }
}
public class AnimationToolsReport : ComponentToolsReport<AnimationData>
{
    public override void MakeCheckReport(string toolName)
    {
        using (StreamWriter checkReport = new StreamWriter(MakeReportTitle(toolName) + @"_Check_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            checkReport.WriteLine("Asset Path,Key Frame Count");
            for (int i = 0; i < checkCSV.Count; i++)
            {
                checkReport.WriteLine($"{checkCSV[i].path},{checkCSV[i].keyframes}");
            }
        }
    }
}

