using System;


namespace BluConsole.Core
{

[Serializable]
public class BluLogFrame
{

    public string FrameInformation { get; private set; }

    public string File { get; private set; }

    public int Line { get; private set; }

    public BluLogFrame(
        string frameInformation)
    {
        FrameInformation = frameInformation;

        int index = frameInformation.IndexOf("(at");
        if (index == -1)
            return;

        index += 4;

        int begFile = index;
        while (index < frameInformation.Length && frameInformation[index] != ':')
            index++;
        int endFile = index-1;

        index += 1;

        int begLine = index;
        while (index < frameInformation.Length && frameInformation[index] != ')')
            index++;
        int endLine = index-1;

        int line;
        if (index+1 != frameInformation.Length || 
            !int.TryParse(frameInformation.Substring(begLine, endLine - begLine + 1), out line))
        {
            File = "";
            Line = 0;
            return;
        }

        File = frameInformation.Substring(begFile, endFile - begFile + 1);
        Line = line;
    }

    public override string ToString()
    {
        return string.Format("[BluLogFrame: FrameInformation={0}, File={1}, Line={2}]", 
                                 FrameInformation, File, Line);
    }
}

}
