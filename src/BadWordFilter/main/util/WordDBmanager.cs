using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;

namespace BadWordFilter.util
{
    public class WordDBmanager
    {
        private MySqlConnection connector;

        private string WordDBhost;
        private string WordDBuser;
        private string WordDBpw;
        private string WordDBname;
        private string WordDBtable;

        public WordDBmanager()//default-settings.
        {
            WordDBhost = "127.0.0.1";
            WordDBuser = "root";
            WordDBpw = "apmsetup";
            WordDBname = "worddb";
            WordDBtable = "badworddb";
        }

        private void RunSQL(string script)
        {
            string sql = script;

            MySqlCommand cmd = new MySqlCommand(sql, connector);
            cmd.ExecuteNonQuery();
        }

        private MySqlDataReader RunAndReadSQL(string script)
        {
            string sql = script;
            //ExecuteReader를 이용하여
            //연결 모드로 데이타 가져오기

            MySqlCommand cmd = new MySqlCommand(sql, connector);
            MySqlDataReader rdr = cmd.ExecuteReader();
            return rdr;
        }
        public bool login(string user="root", string pw="apmsetup")
        {
            string connStr = $"Server={WordDBhost};Database={WordDBname};Uid={WordDBuser = user};Pwd={WordDBpw = pw};Charset=utf8";
            connector = new MySqlConnection(connStr);
            try
            {
                connector.Open();
            }
            catch (MySqlException)
            {
                return false;
            }
            return true;
        }

        public void addWord(string FilterWord,string Originword,string Description)
        {
            RunSQL($"INSERT INTO `worddb`.`badworddb` (`FilterWord`, `OriginWord`, `Description`) VALUES ('{FilterWord}', '{Originword}','{Description}');");
            return;
        }

        public ConcurrentDictionary<string, Tuple<string, string>> ReadWordList()
        {
            MySqlDataReader reader = RunAndReadSQL("SELECT filterword,originword,description FROM badworddb ORDER BY binary(filterword) DESC;");
            ConcurrentDictionary<string, Tuple<string, string>> datalist = new ConcurrentDictionary<string, Tuple<string, string>>();

            while (reader.Read())
            {
                datalist.GetOrAdd(reader["FilterWord"].ToString(), Tuple.Create(reader["OriginWord"].ToString(), reader["Description"].ToString()));
            }

            reader.Close();

            return datalist;
        }
    }
    }

