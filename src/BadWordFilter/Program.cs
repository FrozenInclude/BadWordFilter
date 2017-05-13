using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BadWordFilter;


namespace BadWordFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            string w = "지ㅎ랄 병신 개새끼 존나 시발";
            BadWordFilter.Filter.LoadwordDB();
            while (true)
            {
                Console.Write("입력:");
                w = Console.ReadLine().ToString();

                Console.WriteLine($"\n욕설감지목록:{string.Join("\t", (BadWordFilter.Filter.FilterWords(w,80)).Item2.Cast<string>().ToArray())}\n");

                //Console.Read();
            }
        }
    }
}
