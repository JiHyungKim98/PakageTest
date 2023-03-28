using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class SummaryReport
{
    SummaryData textureSummaryData = new SummaryData();
    SummaryData audioSummaryData = new SummaryData();
    SummaryData modelSummaryData = new SummaryData();
    SummaryData aniSummaryData = new SummaryData();

    public void SaveSummaryData(int totalDataCount, int checkDataCount, ToolComponent tool, int nullCnt)
    {
        if (tool is TextureToolComponent)
        {
            textureSummaryData.totalDataCount += (totalDataCount - nullCnt);
            textureSummaryData.checkDataCount += checkDataCount;
        }
        else if (tool is AudioToolComponent)
        {
            audioSummaryData.totalDataCount += (totalDataCount - nullCnt);
            audioSummaryData.checkDataCount += checkDataCount;
        }
        else if (tool is ModelToolComponent)
        {
            modelSummaryData.totalDataCount += (totalDataCount - nullCnt);
            modelSummaryData.checkDataCount += checkDataCount;
        }
        else if (tool is AnimationToolComponent)
        {
            aniSummaryData.totalDataCount += (totalDataCount - nullCnt);
            aniSummaryData.checkDataCount += checkDataCount;
        }
        MakeSummaryReport();
    }

    private void MakeSummaryReport()
    {
        using (StreamWriter summaryReport = new StreamWriter(@"Summary_Report.csv", false, System.Text.Encoding.GetEncoding("utf-8")))
        {
            summaryReport.WriteLine("Type,Total Count,Check Count");
            summaryReport.WriteLine($"Texture,{textureSummaryData.totalDataCount},{textureSummaryData.checkDataCount}");
            summaryReport.WriteLine($"Audio,{audioSummaryData.totalDataCount},{audioSummaryData.checkDataCount}");
            summaryReport.WriteLine($"Model,{modelSummaryData.totalDataCount},{modelSummaryData.checkDataCount}");
            summaryReport.WriteLine($"Animation,{aniSummaryData.totalDataCount},{aniSummaryData.checkDataCount}");
        }
    }
}

