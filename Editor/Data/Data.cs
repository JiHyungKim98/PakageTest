public class Data
{
    public string path;
    public string etc;
}

public class TextureData : Data
{
    public string platform;
    public int size;
    public bool isReadWriteEnabled;
    public bool isMipMapGenerate;
    public string format;
}

public class ModelData : Data
{
    public int vertices;
}

public class AudioData : Data
{
    public string platform;
    public string format;
    public string loadType;
    public float clipSize;
    public bool forceToMono;
}
public class AnimationData : Data
{
    public int keyframes;
}
public class SummaryData
{
    public int totalDataCount;
    public int checkDataCount;

    public void clearSummaryData()
    {
        totalDataCount = 0;
        checkDataCount = 0;
    }
}