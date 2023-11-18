// See https://aka.ms/new-console-template for more information
//Задание 
//
// ReSharper disable once InvalidXmlDocComment
/// Вывести информацию в консоль о логических дисках, именах, метке тома, размере и типе файловой системы
/// Создание файла -> запись строки в файл -> прочитать файл в консоль -> удалить файл
/// Работа с json/xml -> создать файл -> выполнить сериализацию -> прочитать файл -> удалить файл
/// Создание zip архива -> добавить файл, выбранный пользователем -> разархивировать -> удалить файл/архив
/// 
/// 

using System.IO.Compression;

using ConsoleApp1;
// ZipFile.CreateFromDirectory("/Users/pashkevich/Desktop/test", "/Users/pashkevich/Desktop/test.zip");
//
// return;
//DriveWorker.PrintDriveInfo();


var worker = new FileWorker();
worker.Start();

public class Worker
{
    
}