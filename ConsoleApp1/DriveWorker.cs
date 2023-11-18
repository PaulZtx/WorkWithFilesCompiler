namespace ConsoleApp1;

public static class DriveWorker
{
    public static void PrintDriveInfo()
    {
        var drives = DriveInfo.GetDrives();
        Console.WriteLine("Информация о логических дисках устройства:");
        foreach (var drive in drives)
        {
            
            Console.WriteLine($"Имя файловой системы:{drive.DriveFormat}\n " +
                              $"Имя диска: {drive.Name}\n Тип диска:{drive.DriveType}");
            if (drive.IsReady)
            {
                Console.WriteLine($"Общий объем диска (байт):{drive.TotalSize}\n " +
                                  $"Общий объем свободного места на диске (байт):{drive.TotalFreeSpace}\n " +
                                  $"Метка тома:{drive.VolumeLabel}");
            }
            Console.WriteLine("<------------------>");
        }
    }
}