using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "cachingData", menuName = "Scriptable Object/Data", order = 1)]
public class EditorCachingData : ScriptableObject
{
    [SerializeField]
    List<TextureToolComponent> textureComponentList=new List<TextureToolComponent>();
    [SerializeField]
    List<ModelToolComponent> modelComponentList=new List<ModelToolComponent>();
    [SerializeField]
    List<AudioToolComponent> audioComponentList=new List<AudioToolComponent>();
    [SerializeField]
    List<AnimationToolComponent> animationComponentList=new List<AnimationToolComponent>();

    List<ToolComponent> toolComponentList=new List<ToolComponent>();

    public List<ToolComponent> LoadToolCachingData()
    {
        if (toolComponentList.Count > 0)
            toolComponentList.Clear();

        if (textureComponentList == null && modelComponentList == null && audioComponentList == null && animationComponentList == null)
            return null;

        toolComponentList.AddRange(textureComponentList);
        toolComponentList.AddRange(modelComponentList);
        toolComponentList.AddRange(audioComponentList);
        toolComponentList.AddRange(animationComponentList);

        toolComponentList.Sort(ComparisonSortingComponent);

        return toolComponentList;
    }

    private static int ComparisonSortingComponent(ToolComponent l, ToolComponent R)
    {
        return l.sortingOrder.CompareTo(R.sortingOrder);
    }

    public void SaveToolCachingData(List<ToolComponent> _tools)
    {
        for (int i = 0; i < _tools.Count; i++)
        {
            _tools[i].sortingOrder = i;
        }

        textureComponentList.Clear();
        modelComponentList.Clear();
        audioComponentList.Clear();
        animationComponentList.Clear();

        foreach(var tool in _tools)
        {
            if (tool is TextureToolComponent)
                textureComponentList.Add(tool as TextureToolComponent);
            else if (tool is ModelToolComponent)
                modelComponentList.Add(tool as ModelToolComponent);
            else if(tool is AudioToolComponent)
                audioComponentList.Add(tool as AudioToolComponent);
            else if(tool is AnimationToolComponent)
                animationComponentList.Add(tool as AnimationToolComponent);
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}