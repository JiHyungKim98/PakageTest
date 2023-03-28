using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalComponentToolsReport
{
    ComponentToolsReport<AudioData> audioTotalData;
    ComponentToolsReport<TextureData> textureTotalData;
    ComponentToolsReport<ModelData> modelTotalData;
    ComponentToolsReport<AnimationData> animationTotalData;

    public TotalComponentToolsReport()
    {
        audioTotalData = new AudioToolsReport();
        textureTotalData = new TextureToolsReport();
        modelTotalData = new ModelToolsReport();
        animationTotalData = new AnimationToolsReport();
    }
    public void AddToTotalCheckData(object data)
    {
        if (data is List<TextureData>)
        {
            var collectedDatas = data as List<TextureData>;

            foreach (var collectedData in collectedDatas)
                textureTotalData.AddCheckData(collectedData);
        }
        else if (data is List<AudioData>)
        {
            var collectedDatas = data as List<AudioData>;
            foreach (var collectedData in collectedDatas)
                audioTotalData.AddCheckData(collectedData);

        }
        else if (data is List<ModelData>)
        {
            var collectedDatas = data as List<ModelData>;
            foreach (var collectData in collectedDatas)
                modelTotalData.AddCheckData(collectData);

        }
        else if (data is List<AnimationData>)
        {
            var collectedDatas = data as List<AnimationData>;
            foreach (var collectedData in collectedDatas)
                animationTotalData.AddCheckData(collectedData);
        }
    }

    public void AddToTotalChangeData(object data)
    {
        if (data is List<TextureData>)
        {
            var collectedDatas = data as List<TextureData>;
            foreach (var collectedData in collectedDatas)
                textureTotalData.AddChangeData(collectedData);
        }
        else
            Debug.Log("err!");
    }

    public void MakeTotalCheckReport(List<ToolComponent> _tools)
    {
        textureTotalData.MakeCheckReport("TotalTexture_Check");
        audioTotalData.MakeCheckReport("TotalAudio_Check");
        modelTotalData.MakeCheckReport("TotalModel_Check");
        animationTotalData.MakeCheckReport("TotalAnimtaion_Check");
    }

    public void MakeTotalChangeReport(List<ToolComponent> _tools)
    {
        textureTotalData.MakeChangeReport("TotalTexture_Change");

        ClearTotalReport();
    }

    public void ClearTotalReport()
    {
        audioTotalData.ClearReport();
        textureTotalData.ClearReport();
        modelTotalData.ClearReport();
        animationTotalData.ClearReport();
    }
}
