using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;

namespace MyAssetTools
{
    public class TextureImportValidFormats
    {
        private readonly Dictionary<TextureImporterType, IReadOnlyDictionary<BuildTarget, IReadOnlyList<TextureImporterFormat>>> _tableValidTextureFormats = new Dictionary<TextureImporterType, IReadOnlyDictionary<BuildTarget, IReadOnlyList<TextureImporterFormat>>>();
        
        public void BuildValidFormats()
        {
            _tableValidTextureFormats.Clear();

            var textureTypes = typeof(TextureImporterType).GetFields().Skip(1).ToArray();

            foreach (var importerType in textureTypes)
            {
                if (typeof(TextureImporterType).GetField(importerType.Name).IsDefined(typeof(ObsoleteAttribute), false))
                    continue;
            
                Dictionary<BuildTarget, IReadOnlyList<TextureImporterFormat>> tableFormatsPerPlatform = new Dictionary<BuildTarget, IReadOnlyList<TextureImporterFormat>>();

                foreach (BuildTarget platform in Enum.GetValues(typeof(BuildTarget)))
                {
                    if (typeof(BuildTarget).GetField(platform.ToString()).IsDefined(typeof(ObsoleteAttribute), false))
                        continue;

                    List<TextureImporterFormat> listValidFormats = new List<TextureImporterFormat>();

                    foreach (TextureImporterFormat format in Enum.GetValues(typeof(TextureImporterFormat)))
                    {
                        if (typeof(TextureImporterFormat).GetField(format.ToString()).IsDefined(typeof(ObsoleteAttribute), false))
                            continue;

                        if (TextureImporter.IsPlatformTextureFormatValid((TextureImporterType)Enum.Parse(typeof(TextureImporterType),importerType.Name), platform, format))
                        {
                            listValidFormats.Add(format);
                        }
                    }

                    if (!tableFormatsPerPlatform.ContainsKey(platform))
                    {
                        tableFormatsPerPlatform.Add(platform, listValidFormats);
                    }
                }
                _tableValidTextureFormats.Add((TextureImporterType)Enum.Parse(typeof(TextureImporterType), importerType.Name), tableFormatsPerPlatform);
            }
        }

        public IReadOnlyList<TextureImporterFormat> GetValidFormatList(TextureImporterType importerType, BuildTarget platform)
        {
            if (_tableValidTextureFormats.TryGetValue(importerType, out var tableValids))
            {
                if (tableValids.TryGetValue(platform, out var listValidList))
                {
                    return listValidList;
                }
            }

            return null;
        }
    }
}