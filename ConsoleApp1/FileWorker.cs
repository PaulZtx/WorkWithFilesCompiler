using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ConsoleApp1;

public class FileWorker
{
    private List<FileExtension> _files = new();

    enum TypeOfShow
    {
        Void = 0,
        File
    }
    private void ShowActions(TypeOfShow typeOfFiles)
    {
        const string messageVoid = "\t\t-----МЕНЮ-----\n" +
                                   "1) Вывод информации о файловой системе и дисках\n" +
                                   "2) Создать файл\n" +
                                   "3) Записать в файл\n" +
                                   "4) Прочитатать файл в консоль\n" +
                                   "5) Удалить файл\n" +
                                   ":Q для выхода\n";
        
        const string messageFile = "-----Выберите тип файла-----\n" +
                                   "1) Файл TXT\n" +
                                   "2) Файл JSON\n" +
                                   "3) Файл XML\n" +
                                   "4) Файл ZIP";
        
        Console.WriteLine(typeOfFiles == TypeOfShow.Void ? messageVoid : messageFile);
    }

    public void Start()
    {
        Console.WriteLine("Начало работы");
        while (true)
        {
            FileExtension? file;
            
            ShowActions(TypeOfShow.Void);
            var input = Console.ReadLine()!.Trim();
            if (input == ":Q")
                break;
            
            if (input == "1")
            {
                DriveWorker.PrintDriveInfo();
                continue;
            }
            if (input == "2")
            {
                ShowActions(TypeOfShow.File);
                var action = Console.ReadLine()!.Trim();
                Console.WriteLine("Введите путь для файла:");
                var path = Console.ReadLine();
                
                switch (action)
                {
                    case "1":
                        file = new FileTxt();
                        if(file.CreateFile(path!))
                            _files.Add(file);
                        break;
                    case "2":
                        file = new FileJson();
                        if(file.CreateFile(path!))
                            _files.Add(file);
                        break;
                    case "3":
                        file = new FileXml();
                        if(file.CreateFile(path!))
                            _files.Add(file);
                        break;
                    case "4":
                        file = new FileZip();
                        if(file.CreateFile(path!))
                            _files.Add(file);
                        break;
                    default:
                        Console.WriteLine("Такого действия нет, повторите попытку!");
                        break;
                }
                continue;
            }
            
            Console.WriteLine("Введите имя файла: ");
            
            var filename = Console.ReadLine();
            var shortName = filename!.Split("/")[^1];
            
            if (_files.Any(x => x.Filename == shortName))
                filename = shortName;
            
            else if (File.Exists(filename))
            {
                var extension = Path.GetExtension(filename);
                switch (extension)
                {
                    case ".txt":
                        file = new FileTxt();
                        file.AttachFile(filename);
                        _files.Add(file);
                        break;
                    case ".json":
                        file = new FileJson();
                        file.AttachFile(filename);
                        _files.Add(file);
                        break;
                    case ".xml":
                        file = new FileXml();
                        file.AttachFile(filename);
                        _files.Add(file);
                        break;
                    case ".zip":
                        file = new FileZip();
                        file.AttachFile(filename);
                        _files.Add(file);
                        break;
                }

                filename = shortName;
            }
            
            switch (input)
            {
                case "3":
                    if (TryGetFileByName(filename, out file))
                    {
                        switch (file!.GetType().ToString().Split(".")[^1])
                        {
                            case "FileTxt":
                            case "FileZip":
                                Console.WriteLine("Введите текст: ");
                                file.WriteFile(Console.ReadLine()!);
                                break;
                            case "FileJson":
                            case "FileXml":
                                Console.WriteLine("Введите Имя, Фамилию и Возраст студента через пробел: ");
                                var text = Console.ReadLine();
                                var info = Regex.Replace(text!, @"\s+", " ").Trim().Split(" ");
                                var student = new Person(info[0], info[1], int.Parse(info[2]));
                                file.WriteFile(student);
                                break;
                        }
                    }
                    continue;
                case "4":
                    if(TryGetFileByName(filename, out file))
                        Console.WriteLine(file!.ReadFile());
                    continue;
                case "5":
                    if(TryGetFileByName(filename!, out file))
                    {
                        file!.DeleteFile();
                        _files.Remove(file);
                        Console.WriteLine($"Файл {file.Filename} удален");
                    }
                    continue;
            }
        }
        Console.WriteLine("Завершение работы");
    }

    private bool TryGetFileByName(string filename, out FileExtension? file)
    {
        file = _files.FirstOrDefault(f => f.Filename == filename);
        if (file is not null) 
            return true;
        Console.WriteLine("Файла с таким именем в текущем контексте не существует!");
        return false;

    }
}

public class FileZip : FileExtension
{
    private string? _compressPath;
    public override bool WriteFile(object newObject)
    {
        if (newObject.GetType() != typeof(string))
            return false;
        
        var info = newObject as string;
        File.AppendAllText(_file.FullName, info);
        using var original = new FileStream(_file.FullName, FileMode.OpenOrCreate);

        _compressPath = _file.FullName.Split(".")[0] + ".zip";
        using FileStream targetStream = File.Create(_compressPath);
        
        using var compressionStream = new GZipStream(targetStream, CompressionMode.Compress);
        original.CopyTo(compressionStream); 
 
        Console.WriteLine($"Сжатие файла {_file.FullName} завершено.");
        Console.WriteLine($"Исходный размер: {original.Length}  сжатый размер: {targetStream.Length}");

        return true;
    }

    public override string ReadFile()
    {
        using FileStream sourceStream = new FileStream(_compressPath, FileMode.OpenOrCreate);
        using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
        {
            using (FileStream targetStream = File.Create(_file.FullName))
            {
                decompressionStream.CopyTo(targetStream);
            }
        }
        
        Console.WriteLine($"Восстановлен файл: {_file.FullName}");
        return File.ReadAllText(_file.FullName);
    }

    public override bool DeleteFile()
    { 
        base.DeleteFile();
        if(File.Exists(_compressPath))
            File.Delete(_compressPath!);
        return true;
    }
}

public class FileXml : FileExtension
{
    public override bool WriteFile(object newObject)
    {
        if (newObject.GetType() != typeof(Person))
        {
            return false;
        }

        var xmlSerializer = new XmlSerializer(typeof(List<Person>));
        var data = File.ReadAllText(_file.FullName);
        using var fileStream = new FileStream(_file.FullName, FileMode.Open);
        var persons = data != String.Empty ? xmlSerializer.Deserialize(fileStream) as List<Person> : new List<Person>();
            
        persons!.Add((Person)newObject);
        fileStream.Seek(0, SeekOrigin.Begin);
        xmlSerializer.Serialize(fileStream, persons);
        return true;
    }
}

public class FileTxt: FileExtension
{
    public override bool WriteFile(object newObject)
    {
        if (newObject.GetType() != typeof(string))
        {
            return false;
        }

        using var fileStream = new FileStream(_file.FullName, FileMode.Append);
        fileStream.Write(Encoding.UTF8.GetBytes((string)newObject));
        return true;
    }
}
public class FileJson: FileExtension
{
    public override bool WriteFile(object newObject)
    {
        if (newObject.GetType() != typeof(Person))
            return false;

        var data = File.ReadAllText(_file.FullName);
        var persons = data == string.Empty ? new List<Person>() : JsonSerializer.Deserialize<List<Person>>(data);
        
        persons!.Add((Person)newObject);
        using var fileStream = new FileStream(_file.FullName, FileMode.Open);
        JsonSerializer.Serialize(fileStream, persons);

        return true;
    }
    
}

public abstract class FileExtension
{
    protected FileInfo _file;
    
    public string? Filename { private set; get; }
    public virtual string ReadFile()
    {
        if (!File.Exists(_file.FullName))
        {
            Console.WriteLine($"Файла {_file.FullName} не существует");
            return String.Empty;
        }
        var data = File.ReadAllText(_file.FullName);
        return data == String.Empty ? String.Empty : data;
    }
    
    public virtual bool CreateFile(string path)
    {
        _file = new FileInfo(path);
        if (File.Exists(_file.FullName))
        {
            Console.WriteLine("файл уже существует");
            return false;
        }
        
        File.Create(_file.FullName).Close();
        Filename = _file.FullName.Split("/")[^1];
        
        return File.Exists(_file.FullName);
    }
    public virtual bool DeleteFile()
    {
        if (!File.Exists(_file.FullName))
        {
            Console.WriteLine($"Файла {_file.FullName} не существует");
            return false;
        }
        File.Delete(_file.FullName);
        return true;
    }

    public virtual bool AttachFile(string filename)
    {
        if (!File.Exists(filename))
        {
            Console.WriteLine($"Файла {filename} не существует");
            return false;
        }
        
        _file = new FileInfo(filename);
        Filename = _file.FullName.Split("/")[^1];
        return true;
    }
    public abstract bool WriteFile(object newObject);
}

public class Person
{
    public Person(string? name, string? surname, int age)
    {
        Name = name;
        Surname = surname;
        Age = age;
    }

    public Person()
    {
        
    }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int Age { get; set; }
}