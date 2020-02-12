/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using Sugang.INHA_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sugang
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sugang Remain Alarm V1.0");
            Console.WriteLine("Copyright (C) 2020. Jeong HyunJun. All Rights Reserved.");
            Console.WriteLine("Any problems arising from using this program are yours.");
            Console.WriteLine("Program developers are not liable for any problems arising from using this program.");
            Console.WriteLine("");

            SugangSession ss = null;

            while (ss == null || ss == SugangSession.ErrorSession)
            {
                Console.Write("Id: ");
                var id = read_pass();
                Console.Write("Password: ");
                var pwd = read_pass();

                ss = SugangSession.Create(id, pwd);

                if (ss == SugangSession.ErrorSession)
                {
                    Console.WriteLine("Try again!!");
                }
            }

            Console.WriteLine("Logined: " + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));

       RETRY:
            Console.Write("Enter Subscribe Course Number (ex. CHM1021-010): ");
            var hacksu = Console.ReadLine();

            var r1 = ss.QureyStatusByHaksu(hacksu);

            if (r1.Count == 0)
            {
                Console.WriteLine("'" + hacksu + "' doesn't seem to be an appropriate course number.");
                Console.WriteLine("Try again!");
                goto RETRY;
            }

            Console.WriteLine($"Start monitoring [{r1[0].Name}] [{r1[0].Professor}] [{r1[0].Department}]");

            var rand = new Random();

            while (true)
            {
                var r2 = ss.QureyStatusByHaksu(hacksu);
                Console.WriteLine("Time: " + DateTime.Now.ToString());

                if (r2.Any(x => x.Remain != "0"))
                {
                    Console.WriteLine("Found reamin course!!!");
                    foreach (var hh in r2.Where(x => x.Remain != "0"))
                    {
                        Console.WriteLine($"    {hh.Hacksu} - [{hh.Name}] [{hh.Professor}] [{hh.Department}] - {hh.Remain}명");
                    }
                }

                var sl = rand.Next(1000, 5000);
                Thread.Sleep(sl);
            }
        }

        static string read_pass()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine("");
            return pass;
        }
    }
}
