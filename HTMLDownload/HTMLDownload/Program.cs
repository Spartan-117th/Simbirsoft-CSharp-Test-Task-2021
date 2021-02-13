using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace HTMLDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                WebClient client = new WebClient();

                string url;
                string file = Directory.GetCurrentDirectory() + "//myfile.txt";     //Имя файла, в который сохранится страница              
                string logname = Directory.GetCurrentDirectory() + "//log.txt";     //Имя файла, в который сохраняются результаты работы

                string s;
                bool is_script = false;
                bool is_style = false;
                var list = new Dictionary<string, int>();                          //Словарь для слов со страницы

                Console.WriteLine("Введите URL-адрес страницы, например https://www.simbirsoft.com/");
                
                do                                                                 //Проверка корректности введенного адреса
                {
                    url = Console.ReadLine();
                    if (!(Regex.IsMatch(url, @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", RegexOptions.None)))
                    {
                        Console.WriteLine("Введен неверный адрес, попробуйте еще раз!");
                    }
                }
                while (!(Regex.IsMatch(url, @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", RegexOptions.None)));

                //url = null;                                                   //Для тестирования исключений 
                client.DownloadFile(url, file);               
                StreamReader sr = new StreamReader(file, Encoding.UTF8);                              

                do                                                              //Пока не выбраны все строки
                {
                    s = sr.ReadLine();
                    if (s != null)                                              //Удаление скриптов
                    {
                        if (s.Contains("<script") && s.Contains("/script>"))
                        {
                            s = "";
                        }
                        else if (s.Contains("<script") && !(is_script))
                        {
                            is_script = true;
                            s = " ";                        
                        }
                        else if (s.Contains("/script>") && is_script)
                        {
                            is_script = false;
                            s = " ";                        
                        }
                        else if (is_script)
                        {
                            s = " ";                        
                        }
                    }
                  

                    if (s != null)                                            //Удаление стилей
                    {
                        if (s.Contains("<style") && s.Contains("/style>"))
                        {
                            s = "";
                        }
                        else if (s.Contains("<style") && !(is_style))
                        {
                            is_style = true;
                            s = " ";                       
                        }
                        else if (s.Contains("/style>") && is_style)
                        {
                            is_style = false;
                            s = " ";                            
                        }
                        else if (is_style)
                        {
                            s = " ";                           
                        }
                    }


                    if (s != null)                                              //Удаление остальных html тэгов
                    {
                        s = Regex.Replace(s, "<.*?>", " ");
                    }                   


                    if (s != null)                                              //Заполнение словаря словами со страницы с одновременным подсчетом
                    {
                        string[] n = s.Split(new char[] { ' ', '.', '!', '?', ';', '>', ',', '(', ')' });       //Разделители слов

                        foreach (string p in n)
                        {
                            if (Regex.IsMatch(p, @"^[A-Za-zА-Яа-я]+$", RegexOptions.None) && p != null)         //Проверка на недопустимые символы в словах
                            {
                                if (list.ContainsKey(p.ToUpper()))
                                {
                                    list[p.ToUpper()]++;
                                }
                                else
                                {
                                    list.Add(p.ToUpper(), 1);
                                }
                            }
                        }
                    }



                }
                while (s != null);
               
                sr.Close();

                Logger.WriteWordLog("\n*********************************************************************************");     //Логирование и вывод в консоль
                Logger.WriteWordLog("Дата: " + DateTime.Now.ToString());
                Logger.WriteWordLog("Введен следующий адрес: " + url);
                Logger.WriteWordLog("Страница содержит следующие слова: ");
                Console.WriteLine("Дата: " + DateTime.Now.ToString());
                Console.WriteLine("Введен следующий адрес: " + url);
                Console.WriteLine("Страница содержит следующие слова: ");
                
                foreach (KeyValuePair<string, int> kpv in list)
                {
                    Console.WriteLine(kpv.Key + "  " + kpv.Value);                   
                    Logger.WriteWordLog(kpv.Key + "  " + kpv.Value);
                }               


                Console.WriteLine("Нажмите любую клавишу для завершения...");
                Console.ReadKey();
            }
            catch (IOException ex)                                                                  //Обработка исключений
            {                                        
                Logger.WriteExLog(ex.Message, ex);
                Console.WriteLine("Дата: " + DateTime.Now.ToString() + "\nОшибка ввода-вывода!\n\n" + ex.Message);
                Console.WriteLine();
                Console.ReadKey();
            }
            catch (WebException ex)
            {
                Logger.WriteExLog(ex.Message, ex);
                Console.WriteLine("Дата: " + DateTime.Now.ToString() + "\nОшибка!\n\n" + ex.Message);           
                Console.WriteLine();
                Console.ReadKey();
            }
            catch (ArgumentNullException ex)
            {
                Logger.WriteExLog(ex.Message, ex);
                Console.WriteLine("Дата: " + DateTime.Now.ToString() + "\nОшибка! Пустой аргумент!\n\n" + ex.Message);               
                Console.WriteLine();
                Console.ReadKey();
            }

        }
    }

    public static class Logger                                                      //Класс для логирования
    {
        public static void WriteExLog(string msg, Exception ex)                     //Логирование слов со страницы
        {
            if (string.IsNullOrEmpty(msg)) return;
            string path = Directory.GetCurrentDirectory() + "//log.txt";
            using (var sw = new StreamWriter(path, true, Encoding.UTF8))
            {              
                sw.WriteLine("\n******** Ошибка! **********");
                sw.WriteLine("Дата: {0}", DateTime.Now);
                sw.WriteLine(ex.TargetSite.ToString());
                sw.WriteLine();
                sw.Write(msg);
                sw.WriteLine("\n***************************");              
                sw.Close();
            }
        }

        public static void WriteWordLog(string msg)                                //Логирование исключений
        {
            if (string.IsNullOrEmpty(msg)) return;
            string path = Directory.GetCurrentDirectory() + "//log.txt";
            using (var sw = new StreamWriter(path, true, Encoding.UTF8))
            {               
                sw.WriteLine(msg);              
                sw.Close();
            }
        }
    }


}
