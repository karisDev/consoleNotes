/*
consoleNotes 
ВЕРСИЯ: b0.0.1
НАЗНАЧЕНИЕ ПРОГРАММЫ: Возможность быстро и удобно редактировать текстовые заметки не отходя от консоли.
АВТОР: К. Кирилл Д. (БИСТ-20-2), karjaakirill at gmail.com

Список использованных ресурсов:
https://stackoverflow.com/questions/14877237/getting-all-file-names-from-a-folder-using-c-sharp
https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter?view=netcore-3.1
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/
https://stackoverflow.com/questions/1019793/how-can-i-convert-string-to-int

P.S да, существует vim, но мне он не нравится.
*/

using System;
using System.IO;

namespace consoleNotes
{
    public class noteFunc //тут будут лежать все функции, связанные с заметками
    {
        public static int userNumber(int numberOfPaths) // заставляет пользователя написать число в диапозоне 1...numberOfPath.
        {
            int userAnswer;
            bool terminate;
            Console.Write("Input: ");
            terminate = Int32.TryParse(Console.ReadLine(), out userAnswer);
            while (!terminate || userAnswer < 1 || userAnswer > numberOfPaths)
            {
                Console.Write("You should pick a number between 1..." + numberOfPaths + ": ");
                terminate = Int32.TryParse(Console.ReadLine(), out userAnswer);
            }
            return userAnswer;
        }
        
        public static void noteCreate() // опция 1 на главной странице
        {
            Console.Clear();
            Console.Write("Welcome to the note creation process.\nChoose a name for your note or write 'exit' if you want to come back: ");
            string noteName;
            noteName = Console.ReadLine();
            if (noteName == "exit")
            {
                return;
            }
            noteName = noteName + ".txt";
            foreach(string fileName in noteList()) // проверяем если есть такой файл
            {
                if (noteName == fileName)
                {
                    Console.WriteLine("It appears that " + noteName + " already exists. ");
                    if (deleteDialog(noteName) == true)
                    {
                        noteNew(noteName);
                        return;
                    }
                    else
                    {
                        noteRead(noteName);
                        return;
                    }
                }
            }
            // если не нашлось файлов с именем noteName, начинается создание нового файла:
            noteNew(noteName);
        }

        public static void noteView() // опция 2 на главной странице
        {
            int userAnswer;
            string noteName;
            string[] notelist = noteList();
            Console.WriteLine("\nNumber of notes in database: " + notelist.Length + "\n");
            int i = 0;
            foreach(string file in notelist)
            {
                i++;
                Console.WriteLine(i + ") " + file);
            }
            if (i == 0)
            {
                Console.WriteLine("You don't have any existing notes.");
                Console.Write("Press any key to create a new note... ");
                Console.ReadLine();
                noteCreate();
                return;
            }
            Console.WriteLine("\nPlease, choose a note.");
            userAnswer = userNumber(i);
            noteName = notelist[userAnswer - 1];
            if (deleteDialog(noteName) == true)
            {
                noteNew(noteName);
            }
            else
            {
                noteRead(noteName);
            }
        }

        public static void directoryCheck() // если папки не существует, благодаря этой функции она будет создана 
        {
            if (Directory.Exists(@"noteDatabase"))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory("noteDatabase");
            }
        }
        static bool deleteDialog(string noteName) //возвращает true если перезаписать, false если изменить
        {
            Console.WriteLine("\nWhat would you like to do with " + noteName + "?\n1) View/Edit\n2) Rewrite");
            if (userNumber(2) == 1)
            {
                return false;
            }
            else { return true; }
        }        

        static string[] noteList() // список заметок в папке
        {
            DirectoryInfo directory = new DirectoryInfo(@"noteDatabase/"); //получаем всю информацию о каталоге
            FileInfo[] txtList = directory.GetFiles("*.txt"); //поиск по файлам с расширением .txt
            string[] arr = new string[txtList.Length];
            int i = 0;
            foreach(FileInfo file in txtList)
            {
                arr[i] = file.Name;
                i++;
            }
            return arr;
        }        

        static void noteNew(string noteName) // перезапись существующей заметки или создание новой
        {
            string consoleNotesWatermark = "This file was generated with consoleNotes";

            Console.Write("Lets start writing your new note!\nPress 'enter' to continue... ");
            Console.ReadLine();
            using (StreamWriter sw = new StreamWriter(@"noteDatabase/" + noteName, false, System.Text.Encoding.Default))
            {
                sw.Write(consoleNotesWatermark);
                //sw.Close();

            }
            noteEdit(noteName);
        }        

        static string[] noteInArray(string noteName) // возврат массива в котором каждый элемент - 1 строка файла
        {
            int i = 1;
            StreamReader sr = new StreamReader(@"noteDatabase/" + noteName);
            string userLine;
            while ((userLine = sr.ReadLine()) != null) 
            {
                i++;
            }
            string[] noteArr = new string[i];
            i = 0;
            sr = new StreamReader(@"noteDatabase/" + noteName);
            while ((userLine = sr.ReadLine()) != null)
            {
                noteArr[i] = userLine;
                i++;
            }
            sr.Close();
            return noteArr;
        }

        static void noteRead(string noteName) // выводит на экран заметку noteName целиком
        {
            int i = 0;
            Console.Clear();
            Console.WriteLine(noteName + "\n");
            foreach (string noteLine in noteInArray(noteName))
            {
                i++;
                Console.WriteLine(noteLine);
            }
            Console.WriteLine("\n1) Edit\n2) Go back");
            if (userNumber(2) == 1)
            {
                noteEdit(noteName);
            }
        } 

        static void noteEdit(string noteName) // основной редактор заметки (самая сложная функция)
        {
            string[] editorArray = new string[10000]; // массив каждый элемент которого отражает одну строку в txt
            int arrayEndLocation = 0; // т.к. размер массива взят с запасом. Нам нужно знать какая строка является последней, чтобы не записать 10000 строк в файл
            int lineEdit; // в этой переменной будет храниться номер строки, если юзер решил писать в середине txt

            Console.Clear();
            Console.WriteLine(noteName + "\n");
            Console.WriteLine("Write the word 'line' to choose a line and edit it.\nWrite the word 'save' to save file and exit the editor.\n");

            foreach (string noteLine in noteInArray(noteName)) // первый вывод всех строк в файле поочередно и заполнение массива
            {
                editorArray[arrayEndLocation] = noteLine;
                arrayEndLocation++;
                Console.WriteLine(arrayEndLocation + ") " + noteLine);
            }
            arrayEndLocation++;
            Console.Write((arrayEndLocation) + ") "); 
            string userInput = Console.ReadLine();
            while (userInput != "save")
            {
                if (userInput != "line")
                {
                    editorArray[arrayEndLocation - 1] = userInput;
                    arrayEndLocation++;
                }
                else
                {
                    Console.Write("Choose the line.\n");
                    lineEdit = userNumber(arrayEndLocation - 1);
                    Console.Write("What would you like to do with line #" + lineEdit + "?\n1) Write after it.\n2) Delete\n");
                    if (userNumber(2) == 1) // запись после определенной строки
                    {
                        userInput = "";
                        Console.Clear();
                        Console.Write("Write 'line' again if you want to go back\n\n");
                        for (int i = 0; i < lineEdit; i++)
                        {
                            Console.WriteLine((i + 1) + ") " + editorArray[i]);
                        }
                        while (userInput != "line")
                        {
                            lineEdit++;
                            Console.Write(lineEdit + ") ");
                            userInput = Console.ReadLine();
                            if (userInput == "line")
                            {
                                continue;
                            }
                            for (int i = arrayEndLocation; i > lineEdit - 2; i--)
                            {
                                editorArray[i + 1] = editorArray[i];
                            }
                            editorArray[lineEdit - 1] = userInput;
                            arrayEndLocation++;
                        }
                    }
                    else // удаление строки сдвигом массива влево
                    {
                        arrayEndLocation--;
                        for (int i = lineEdit - 1; i < arrayEndLocation; i++)
                        {
                            editorArray[i] = editorArray[i + 1];
                        }
                    }
                    userInput = "";
                    Console.Clear();
                    Console.WriteLine(noteName + "\n");
                    Console.WriteLine("Write the word 'line' to choose a line and edit it.\nWrite the word 'save' to save file and exit the editor.\n");

                    for(int i = 0; i < arrayEndLocation - 1; i++)
                    {
                        Console.WriteLine((i + 1) +") " + editorArray[i]);
                    }

                }
                Console.Write(arrayEndLocation + ") ");
                userInput = Console.ReadLine();
            }
            // запись массива в файл
            // винда тупая и ей нужен костыль тк C# слишком быстрый
            Console.WriteLine("error starts here");
            
            using (StreamWriter sw = new StreamWriter(@"noteDatabase/" + noteName, false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < arrayEndLocation - 1; i++)
                {
                    sw.WriteLine(editorArray[i]);
                }
                //sw.Close();
            }
            Console.WriteLine("if you see this be happy!");
            Console.Write("Your note was saved in " + noteName + "\n Press 'enter' to come back...");
            Console.ReadLine();
        }
    }
    class mainCode
    {
        static void Main(string[] args)
        {
            noteFunc.directoryCheck();
            while(true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the beta 0.0.1 of consoleNotes!\n");
                Console.Write("What would you like to do?\n1) Create a new note\n2) View notes\n3) Leave the program\n");
                int userAnswer = noteFunc.userNumber(3);
                if (userAnswer == 1) // сделать новую заметку
                {
                    noteFunc.noteCreate();
                }
                else if (userAnswer == 2) //посмотреть заметки
                {
                    noteFunc.noteView();
                }
                else // завершить работу
                {
                    Console.Clear();
                    Console.WriteLine("Thanks for using consoleNotes!\n");
                    return; 
                } 
            }
        }
    }
}
/* 
using (StreamReader sr = new StreamReader(@"noteDatabase/" + noteName)) 
        {
            Console.Clear();
            Console.WriteLine(noteName);
            Console.WriteLine();
            string userLine;
            while ((userLine = sr.ReadLine()) != null) 
            {
                Console.WriteLine(userLine);
            }
        }
*/
