using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimMetricsMetricUtilities;
using System.Text.RegularExpressions;
using BadWordFilter.util;
using SimMetricsUtilities;

namespace BadWordFilter
{
    public sealed class BadWordFilter
    {
        static BadWordFilter bwd;
        static readonly object locks = new object();//use for multi-thread object;

        private SmithWaterman smithWaterMan = new SmithWaterman();
        private Levenstein needleman = new Levenstein();
        private SentenceDivider Divider = new SentenceDivider();

        private WordDBmanager DBmanager =new WordDBmanager();

        private ConcurrentDictionary<string, Tuple<string,string>> 욕설감지리스트 = new ConcurrentDictionary<string,Tuple<string,string>>();

        public BadWordFilter()
        {
            Divider.addStdChosung("ㅋ", "ㄱ").addStdChosung("ㄲ", "ㄱ").addStdChosung("ㅆ", "ㅅ").addStdChosung("ㅃ","ㅂ");
            Divider.addStdGungsung("ㅑ", "ㅏ").addStdGungsung("ㅕ", "ㅓ").addStdGungsung("ㅛ", "ㅗ").addStdGungsung("ㅒ", "ㅐ") .addStdGungsung("ㅖ", "ㅔ").addStdGungsung("ㅞ","ㅔ").addStdGungsung("ㅢ","ㅣ").addStdGungsung("ㅙ","ㅐ").addStdGungsung("ㅟ","ㅣ");
            Divider.addStdGongsung("ㅄ", "ㅂ").addStdGongsung("ㄳ", "ㄱ").addStdGongsung("ㄼ","ㄹ").addStdGongsung("ㄾ","ㄹ").addStdGongsung("ㄵ","ㄴ");
        }

        static public BadWordFilter Filter {//thread-safe
            get
            {
                if (bwd == null)
                {
                    lock (locks)
                    {
                        if (bwd == null)
                        {
                            bwd = new BadWordFilter();
                        }
                    }
                }
                return bwd;
            }
        }

        public void LoadwordDB()
        {
            DBmanager.login();
            욕설감지리스트 = DBmanager.ReadWordList();
        }

        public Tuple<bool, List<string>> FilterWords(string origin, int rate)
        {
            if (Regex.Replace(origin, " ", string.Empty) == "") throw new Exception("Input is empty");
            if (욕설감지리스트.Count == 0) throw new Exception("plz call LoadWordDB() before calling");
            if (rate > 100) throw new Exception("rate should be under 100");
            

            List<string> badWordsUsed = new List<string>();
            origin = Regex.Replace(origin, @"[\d-]|[^\w\d]", string.Empty);

            string origin2 = Divider.SliceLetter(origin);
            bool success = false;

            if (origin2.Length < 5)
            {
                Parallel.ForEach(욕설감지리스트, (kv) =>
                {
               
                        if (!badWordsUsed.Contains(kv.Value.Item1))
                        {
                            string s = kv.Key;
                            double d = needleman.GetSimilarity(origin, s) * 100;
                            int percent = Convert.ToInt32(d);
                            Console.WriteLine($"{kv.Key}와 {percent}% 일치합니다.   ({origin})<==>({s})");
                            if (percent >= rate)
                            {
                                lock (locks)
                            {    
                               badWordsUsed.Add(kv.Value.Item1); success = true;
                            }
                        }
                    }
                });
            }
            else
            {
                Parallel.ForEach(욕설감지리스트, (kv) =>
                {
                    if (!badWordsUsed.Contains(kv.Value.Item1))
                    {
                        string s = Divider.SliceLetter(kv.Key);
                        double d = smithWaterMan.GetSimilarity(origin2, s) * 100;
                        int percent = Convert.ToInt32(d);
                        Console.WriteLine($"{kv.Key}와 {percent}% 일치합니다.   ({origin2})<==>({s})");
                        if (percent >= rate)
                        {
                            badWordsUsed.Add(kv.Value.Item1); success = true;
                        }
                    }
            });
            }
            return Tuple.Create<bool, List<string>>(success, badWordsUsed);
        }


        public async Task<Tuple<bool,List<string>>> FilterWordsAsync(string origin,int rate)
        {
            if (rate > 100) throw new Exception("rate should be under 100");
            if (Regex.Replace(origin," ",string.Empty)== "")throw new Exception("Input is empty");

            List<string> badWordsUsed = new List<string>();
            origin = Regex.Replace(origin, @"[\d-]|[^\w\d]", string.Empty);

            string origin2 = Divider.SliceLetter(origin);
            bool success = false;

            if (origin2.Length < 5)
            {
                await Task.Run(() =>Parallel.ForEach(욕설감지리스트, (kv) =>
                {
                    // lock (locks){
                    string s = kv.Key;
                    double d = needleman.GetSimilarity(origin, s) * 100;
                    int percent = Convert.ToInt32(d);
                    Console.WriteLine($"{kv.Key}와 {percent}% 일치합니다.   ({origin})<==>({s})");
                    if (percent >= rate)
                    {
                        badWordsUsed.Add(kv.Value.Item1); success = true;
                    }
                }));

            }
            else
            {
                await Task.Run(() => Parallel.ForEach(욕설감지리스트, (kv) =>
                {
                    // lock (locks){
                    string s = Divider.SliceLetter(kv.Key);
                    double d = smithWaterMan.GetSimilarity(origin2, s) * 100;
                    int percent = Convert.ToInt32(d);
                    Console.WriteLine($"{kv.Key}와 {percent}% 일치합니다.   ({origin2})<==>({s})");
                    if (percent >= rate)
                    {
                        badWordsUsed.Add(kv.Value.Item1); success = true;
                    } //}

                }));
            }

            return Tuple.Create<bool,List<string>>(success,badWordsUsed);
        }
    }
}
