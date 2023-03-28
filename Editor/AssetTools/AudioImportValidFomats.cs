using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyAssetTools
{
    public class AudioImportValidFomats
    {
        public List<AudioCompressionFormat> GetFormatsForPlatform(BuildTargetGroup platform)
        {
            List<AudioCompressionFormat> formatsForPlatform = new List<AudioCompressionFormat>();
            if (platform == BuildTargetGroup.WebGL)
            {
                formatsForPlatform.Add(AudioCompressionFormat.AAC);
                return formatsForPlatform;
            }
            formatsForPlatform.Add(AudioCompressionFormat.PCM);
            formatsForPlatform.Add(AudioCompressionFormat.Vorbis);
            formatsForPlatform.Add(AudioCompressionFormat.ADPCM);
            if (platform != BuildTargetGroup.Standalone && platform != BuildTargetGroup.WSA && platform != BuildTargetGroup.XboxOne && platform != 0)
                formatsForPlatform.Add(AudioCompressionFormat.MP3);
            if (platform == BuildTargetGroup.PS4 || platform == BuildTargetGroup.PS5)
                formatsForPlatform.Add(AudioCompressionFormat.ATRAC9);
            if (platform == BuildTargetGroup.XboxOne || platform == BuildTargetGroup.GameCoreXboxSeries || platform == BuildTargetGroup.GameCoreXboxOne)
                formatsForPlatform.Add(AudioCompressionFormat.XMA);
            return formatsForPlatform;
        }
    }
}

