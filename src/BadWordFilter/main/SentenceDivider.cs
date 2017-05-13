using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadWordFilter
{
  public class SentenceDivider
    {
        private static char[] 초성테이블 = ("ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ").ToCharArray();
        private static char[] 중성테이블 = ("ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ").ToCharArray();
        private static char[] 종성테이블 = (" ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ").ToCharArray();

        private Dictionary<string, string> 초성표준화리스트 = new Dictionary<string, string>();
        private Dictionary<string, string> 중성표준화리스트 = new Dictionary<string, string>();
        private Dictionary<string, string> 종성표준화리스트 = new Dictionary<string, string>();

        private static ushort startLetter = 0xAC00;
        private static ushort endLetter = 0xD79F;

        private enum Option
        {
            Chosung,
            Jungsung,
            Jongsung
        }

        public SentenceDivider addStdChosung(string key,string value)
        {
           초성표준화리스트.Add(key, value);
            return this;
        }

        public SentenceDivider addStdGungsung(string key, string value)
        {
            중성표준화리스트.Add(key, value);
            return this;
        }

        public SentenceDivider addStdGongsung(string key, string value)
        {
            종성표준화리스트.Add(key, value);
            return this;
        }

        private string standardization(string letter, Option option)
        {
            if (option == Option.Chosung)
            {
                try
                {
                    letter = 초성표준화리스트[letter];
                }
                catch (KeyNotFoundException)
                {
                    return letter;
                }
            }
            else if (option == Option.Jungsung)
            {
                try
                {
                    letter = 중성표준화리스트[letter];
                }
                catch (KeyNotFoundException)
                {
                    return letter;
                }
            }
            else if(option == Option.Jongsung)
            {
                try
                {
                    letter = 종성표준화리스트[letter];
                }
                catch (KeyNotFoundException)
                {
                    return letter;
                }
            }
            return letter;
        }

        /*
        한 글자의 유니코드값은 startLetter+(초성*21+ 중성)*28+종성
      */
        public string SliceLetter(string letter)
        {
            StringBuilder result = new StringBuilder();
            string 초성="", 중성="",초성백업="",중성백업="";
         
         foreach(var c in letter.ToCharArray())
            {
                int 초성인덱스 = 0, 중성인덱스 = 0, 종성인덱스 = 0;
                ushort temp = Convert.ToUInt16(c);
                if ((temp >= startLetter) && (temp <= endLetter))
                {
                    // result.Clear();
                    int ttemp = temp - startLetter;
                    초성인덱스 = (ttemp) / (21 * 28);
                    ttemp = (ttemp) % (21 * 28);
                    중성인덱스 = (ttemp) / 28;
                    ttemp = (ttemp) % 28;
                    종성인덱스 = ttemp;

                    초성 = standardization(초성테이블[초성인덱스].ToString(), Option.Chosung);
                    중성 = standardization(중성테이블[중성인덱스].ToString(), Option.Jungsung);

                    if (중성 == 중성백업&&초성 == "ㅇ") 초성 = 초성백업;

                    초성백업 = 초성;
                    중성백업 = 중성;

                    result.Append(초성).Append(중성);

                    if (종성인덱스 != 0)result.Append(standardization(종성테이블[종성인덱스].ToString()+"..",Option.Jongsung));
                }
                else result.Append(c);
            }//);
            return result.ToString();
        }

    }
}
