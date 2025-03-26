namespace NoMercy.NmSystem.FileSystem;

public static class Folders
{
    public static void EmptyFolder(string folderPath)
    {
        DirectoryInfo directory = new(folderPath);
        if (!directory.Exists) return;

        foreach (FileInfo file in directory.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        {
            subDirectory.Delete(true);
        }
    }
}