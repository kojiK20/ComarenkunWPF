using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Comarenkun
{
    public class FileLogic
    {
        private MainWindow window;
        private string memberFilePath = "Texts/members.txt";//絶対パスは"pack://application:,,,/Comarenkun;component//"
        private string foreignerFilePath = "Texts/foreigners.txt";
        private string configFilePath = "Texts/config.txt";
        private string LINEtokenFilePath = "Texts/LINEtoken.txt";
        private string talkFilePath = "Texts/talk.txt";
        StreamReader sr;//config
        StreamReader sr1;//member
        StreamReader sr2;//foreigner
        StreamReader sr3;//LINEtoken
        StreamReader sr4;//Talk
        StreamWriter sw;
        StreamWriter sw1;
        StreamWriter sw2;
        StreamWriter sw3;

        public FileLogic(MainWindow window)
        {//メインウィンドウのコントロールをwindowから参照する
            this.window = window;
        }

        public void CreateIfNotExistsConfig()
        {
            if (!Directory.Exists("Texts"))
            {
                Directory.CreateDirectory("Texts");
            }
            if (!File.Exists("Texts/config.txt"))
            {
                //File.Create("Texts/members.txt");
                sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw.WriteLine("table:10\ncoma:5\n1\n1\n2\n1\n0");
                sw.Close();
            }
        }

        public List<string> ReadConfigFile()
        {//configテキストを読み込み配列にして返す
            CreateIfNotExistsConfig();
            //ReWriteIfNotRightConfig();
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));

            List<string> result = new List<string>();//台数,コマ数,各コマのアルゴリズム(0 or 1 or 2)
            string line;
            int fuga = 0;
            try
            {
                while ((line = sr.ReadLine()) != null)
                {//テキストが無くなるまで1行ずつ読む
                    if (line != "")
                    {
                        if(fuga == 0)
                        {//台数
                            string[] t = line.Split(':');
                            int piyo;
                            if(t.Length > 2 || !int.TryParse(t[1], out piyo) || t[0] != "table")
                            {//形式がおかしいのでエラー
                                throw new IndexOutOfRangeException();
                            }else if(int.Parse(t[1]) > 50)
                            {//max50台
                                t[1] = "50";
                            }
                            result.Add(t[1]);
                            fuga++;
                        }else if(fuga == 1)
                        {//コマ数
                            string[] c = line.Split(':');
                            int piyo;
                            if (c.Length > 2 || !int.TryParse(c[1], out piyo) || c[0] != "coma")
                            {//形式がおかしいのでエラー
                                throw new IndexOutOfRangeException();
                            }else if(int.Parse(c[1]) > 20)
                            {//max20コマ
                                c[1] = "20";
                            }
                            result.Add(c[1]);
                            fuga++;
                        }
                        else
                        {//各コマのアルゴリズム
                            if(line != "0" && line != "1" && line != "2")
                            {//形式がおかしいのでエラー
                                throw new IndexOutOfRangeException();
                            }
                            result.Add(line);
                        }
                    }
                }
            }
            catch
            {//ファイルの内容が形式に沿わない場合は，旨を伝えそのファイルをコピーし，新たに初期ファイルを作成する
                sr.Close();
                string path = "";
                for (int i = 1; true; i++)
                {
                    if (File.Exists("Texts/config_copy.txt") && File.Exists("Texts/config_copy" + i.ToString() + ".txt"))
                    {//コピー先ファイル名が重複しているとき連番にする
                    }
                    else if (File.Exists("Texts/config_copy.txt"))
                    {
                        path = "config_copy" + i.ToString() + ".txt";
                        File.Copy(configFilePath, "Texts/" + path);
                        break;
                    }
                    else
                    {
                        path = "config_copy.txt";
                        File.Copy(configFilePath, "Texts/" + path);
                        break;
                    }
                }
                sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw.WriteLine("table:10\ncoma:5\n1\n1\n2\n1\n0");
                sw.Close();
                MessageBox.Show("Comarenkun/Texts/config.txtの形式が不正です．\n" + path + "にコピーして新たにconfig.txtを作成しました．");
                return ReadConfigFile();//再帰
            }
            
            sr.Close();

            return result;
        }

        public void CreateIfNotExists()
        {
            if (!Directory.Exists("Texts"))
            {
                Directory.CreateDirectory("Texts");
            }
            if (!File.Exists("Texts/members.txt"))
            {
                //File.Create("Texts/members.txt");
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.WriteLine("1:sample:");
                sw1.Close();
            }
            if (!File.Exists("Texts/foreigners.txt"))
            {
                //File.Create("Texts/foreigners.txt");
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.WriteLine(":sample:");
                sw2.Close();
            }
        }
        public void ReWriteIfNotRight()
        {//ランクが整数値あるいは↑↓空以外のものは0もしくは空に置き換える
            sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
            sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s1 = "";// sr1.ReadToEnd();
            string s2 = "";// sr2.ReadToEnd();
            
            string line;
            List<string> names = new List<string>();
            
            while ((line = sr1.ReadLine()) != null)
            {
                if(line != "")
                {
                    line = line.Replace('：', ':');
                    string[] c = line.Split(':');
                    c[1].Replace("/", "");
                    c[1].Replace("＋", "");
                    c[1].Replace("ー", "");
                    //名前が重複しているなら連番にする
                    c[1] = NameDuplicateCheck(names, c[1]);
                    c[2] = "";
                    names.Add(c[1]);

                    int i;
                    if (int.TryParse(c[0], out i) == false)
                    {//ランクが整数値以外の場合0に置き換え,名前や所属が置き換えられてはいけないので1行ずつやる
                        s1 = s1 + "0:" + c[1] + ":" + c[2] + "\n";
                    }
                    else if(int.Parse(c[0]) < 0)
                    {
                        s1 = s1 + "0:" + c[1] + ":" + c[2] + "\n";
                    }
                    else if (int.Parse(c[0]) > 1000)
                    {
                        s1 = s1 + "1000:" + c[1] + ":" + c[2] + "\n";
                    }
                    else
                    {
                        s1 = s1 + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                    }
                    
                }
            }
            sr1.Close();
            //names = new List<string>();
            while((line = sr2.ReadLine()) != null)
            {//ランクが↑↓空以外の場合空に置き換え
                if(line != "")
                {
                    line = line.Replace('：', ':');
                    string[] c = line.Split(':');

                    c[1].Replace("/", "");
                    c[1].Replace("＋", "");
                    c[1].Replace("ー", "");
                    c[1].Replace(" ", "　");
                    //名前が重複しているなら連番にする
                    c[1] = NameDuplicateCheck(names, c[1]);
                    if(c[2] == "所属ナシ")
                    {
                        c[2] = "";
                    }else if(c[2] == "部内")
                    {
                        c[2] = "部内'";
                    }
                    names.Add(c[1]);
                    
                    if (c[0] != "↑" && c[0] != "↓" && c[0] != "")
                    {
                        s2 = s2 + ":" + c[1] + ":" + c[2] + "\n";
                    }
                    else
                    {
                        s2 = s2 + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                    }
                }  
            }
            //上書き保存
            sr2.Close();
            sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw1.Write(s1);
            sw2.Write(s2);
            sw1.Close();
            sw2.Close();
        }
        public string NameDuplicateCheck(List<string> names, string name)
        {//名前が重複してたら連番にした名前を返す
            if(names.IndexOf(name) == -1)
            {//重複していない
                return name;
            }
            else
            {//重複している時，連番を1増やした文字で再帰チェック
                return NameDuplicateCheck(names, name, 2);
            }
        }
        public string NameDuplicateCheck(List<string> names,string original, int i)
        {//名前が重複してたら連番にした名前を返す
            if (names.IndexOf(original + i.ToString()) == -1)
            {//重複していない
                return original + i.ToString();
            }
            else
            {//重複している時，連番を1増やした文字で再帰チェック
                return NameDuplicateCheck(names, original, i + 1);
            }
        }

        public List<string[]> ReadMemberFile()
        {//メンバテキストを読み込み配列にして返す
            CreateIfNotExists();
            ReWriteIfNotRight();

            sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
            sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));

            List<string[]> result1 = new List<string[]>();
            List<string[]> result2 = new List<string[]>();
            string line;
            try
            {
                while ((line = sr1.ReadLine()) != null)
                {//テキストが無くなるまで1行ずつ読む
                    if (line != "")
                    {//ランク，名前，所属の配列
                        string[] memberNames = line.Split(':');
                        if (memberNames.Length > 3)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        memberNames[2] = "部内";
                        result1.Add(memberNames);
                    }
                }
            }
            catch
            {//ファイルの内容が形式に沿わない場合は，旨を伝えそのファイルをコピーし，新たに初期ファイルを作成する
                sr1.Close();
                sr2.Close();
                string path = "";
                for (int i = 1; true; i++)
                {
                    if (File.Exists("Texts/members_copy.txt") && File.Exists("Texts/members_copy" + i.ToString() + ".txt"))
                    {//コピー先ファイル名が重複しているとき連番にする
                    }
                    else if (File.Exists("Texts/members_copy.txt"))
                    {
                        path = "members_copy" + i.ToString() + ".txt";
                        File.Copy(memberFilePath, "Texts/" + path);
                        break;
                    }
                    else
                    {
                        path = "members_copy.txt";
                        File.Copy(memberFilePath, "Texts/" + path);
                        break;
                    }
                }
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.WriteLine("1:sample:");
                sw1.Close();
                MessageBox.Show("Comarenkun/Texts/members.txtの形式が不正です．\n" + path + "にコピーして新たにmembers.txtを作成しました．");
                return ReadMemberFile();//再帰
            }
            //ソートしておく
            //数値ソート
            result1.Sort((a, b) => int.Parse(a[0]) - int.Parse(b[0]));//ランクで昇順ソート(たぶんn^2以下)

            try
            {
                while ((line = sr2.ReadLine()) != null)
                {//外部参加者も数える
                    if (line != "")
                    {
                        string[] foreignerNames = line.Split(':');
                        if(foreignerNames.Length > 3)
                        {
                            throw new IndexOutOfRangeException();
                        }

                        if (foreignerNames[2] == "")
                        {
                            foreignerNames[2] = "所属ナシ";
                        }
                        else if (foreignerNames[2] == "部内")
                        {//外部所属に「部内」を作れないように
                            foreignerNames[2] = "部内2";
                        }
                        result2.Add(foreignerNames);
                    }
                }
            }
            catch
            {
                sr1.Close();
                sr2.Close();
                string path = "";
                for (int i = 1; true; i++)
                {
                    if (File.Exists("Texts/foreigners_copy.txt") && File.Exists("Texts/foreigners_copy" + i.ToString() + ".txt"))
                    {//コピー先ファイル名が重複しているとき連番にする
                    }
                    else if (File.Exists("Texts/foreigners_copy.txt"))
                    {
                        path = "foreigners_copy" + i.ToString() + ".txt";
                        File.Copy(foreignerFilePath, "Texts/" + path);
                        break;
                    }
                    else
                    {
                        path = "foreigners_copy.txt";
                        File.Copy(foreignerFilePath, "Texts/" + path);
                        break;
                    }
                }
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.WriteLine(":sample:");
                sw2.Close();
                MessageBox.Show("Comarenkun/Texts/foreigners.txtの形式が不正です．\n" + path + "にコピーして新たにforeigners.txtを作成しました．");
                return ReadMemberFile();//再帰
            }
            //ソートしておく
            //文字列ソート
            result2.Sort((a, b) => a[0].CompareTo(b[0]));

            sr1.Close();
            sr2.Close();

            result1.AddRange(result2);
            return result1;
        }

        public List<string> AllMemberNames()
        {
            List<string[]> m = ReadMemberFile();
            List<string> result = new List<string>();
            foreach(string[] mem in m)
            {
                result.Add(mem[1]);
            }
            return result;
        }
        public List<MemberNode> AllMemberNodes()
        {//マッチング時に必要
            List<string[]> m = ReadMemberFile();
            List<MemberNode> result = new List<MemberNode>();
            int maxRank = 0;
            int tmp;
            for(int i = 0; i < m.Count; i++)
            {//member内のランクの最大値を求めておく
                if(int.TryParse(m[i][0],out tmp))
                {
                    if(tmp > maxRank)
                    {
                        maxRank = tmp;
                    }
                }
            }
            foreach (string[] mem in m)
            {
                int rank;
                if(mem[0] == "↑")
                {
                    rank = 0;
                }
                else if(mem[0] == "↓")
                {
                    rank = maxRank;
                }
                else if(mem[0] == "")
                {
                    Random r = new Random();//乱数
                    int rand = r.Next(2);//0 or 1
                    if (rand == 0)
                    {//上位として扱う
                        rank = 0;
                    }
                    else
                    {
                        rank = maxRank;
                    }
                }
                else
                {
                    rank = int.Parse(mem[0]);
                }
                result.Add(new MemberNode(rank,mem[1],mem[2]));
            }
            return result;
        }
        public List<string[]> MembersOfGroup(string groupName)
        {//引数に渡した所属に属するメンバーの{ランク，名前}を返す
            List<string[]> members = ReadMemberFile();
            List<string[]> result = new List<string[]>();
            foreach (string[] mem in members)
            {
                if(mem[2] == groupName)
                {
                    string[] m = { mem[0], mem[1] };
                    result.Add(m);
                }
            }
            return result;
        }
        public void AddMember(string rank, string name, string group)
        {
            if(group == "部内")
            {//所属が部内ならmembers.txtに書く(所属は""に)
                sw1 = new StreamWriter(memberFilePath, true, Encoding.GetEncoding("Shift_JIS"));
                try
                {
                    sw1.WriteLine("\n" + rank + ":" + name + ":" + "\n");
                }
                finally
                {
                    sw1.Close();
                }
            }
            else
            {//foreigners.txtに書く
                sw2 = new StreamWriter(foreignerFilePath, true, Encoding.GetEncoding("Shift_JIS"));
                try
                {
                    if(group == "所属ナシ")
                    {
                        sw2.WriteLine("\n" + rank + ":" + name + ":" + "\n");
                    }
                    else
                    {
                        sw2.WriteLine("\n" + rank + ":" + name + ":" + group + "\n");
                    }
                    
                }
                finally
                {
                    sw2.Close();
                }
            }
        }
        public void DeleteMember(string name, string group)
        {//nameはmembername
            if(group == "部内")
            {//mmebers.txt
                sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr1.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//こいつの行を削除,ランクや所属が置き換えられてはいけないので1行ずつやる
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr1.Close();
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                //上書き保存
                sw1.Write(s);
                sw1.Close();
            }
            else
            {//foreigners.txt
                sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr2.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//こいつの行を削除,ランクや所属が置き換えられてはいけないので1行ずつやる
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr2.Close();
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                //上書き保存
                sw2.Write(s);
                sw2.Close();
            }
        }
        public void ChangeMember(string rank, string preName, string name, string group)
        {
            if(group == "部内")
            {//members.txt
                sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr1.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == preName)
                        {//メンバを置き換え,1行ずつやる
                            s = s + rank + ":" + name + ":" + c[2] + "\n";
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr1.Close();
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.Write(s);
                sw1.Close();
            }else
            {//foreigners.txt
                sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr2.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == preName)
                        {//メンバを置き換え,1行ずつやる
                            s = s + rank + ":" + name + ":" + c[2] + "\n";
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr2.Close();
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.Write(s);
                sw2.Close();
            }
        }
        public void PlusRank(string name, string group)
        {
            if (group == "部内")
            {//数字を1上げる
                sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr1.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//ランクを置き換え,1行ずつやる
                            if(int.Parse(c[0]) >= 1000)
                            {
                                s = s + "1000:" + c[1] + ":" + c[2] + "\n";
                            }
                            else
                            {
                                s = s + (int.Parse(c[0]) + 1).ToString() + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr1.Close();
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.Write(s);
                sw1.Close();
            }
            else
            {//↑→""→↓の順
                sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr2.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//ランクを置き換え,1行ずつやる
                            if(c[0] == "↓")
                            {
                                s = s + "↑" + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            else if(c[0] == "")
                            {
                                s = s + "↓" + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            else if(c[0] == "↑")
                            {
                                s = s + "" + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr2.Close();
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.Write(s);
                sw2.Close();
            }
        }
        public void MinusRank(string name, string group)
        {
            if (group == "部内")
            {//数字を1下げる
                sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr1.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//ランクを置き換え,1行ずつやる
                            if(int.Parse(c[0]) <= 0)
                            {
                                s = s + "0:" + c[1] + ":" + c[2] + "\n";
                            }
                            else
                            {
                                s = s + (int.Parse(c[0]) - 1).ToString() + ":" + c[1] + ":" + c[2] + "\n";
                            }
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr1.Close();
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.Write(s);
                sw1.Close();
            }
            else
            {//↓→""→↑の順
                sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
                string s = "";
                string line;
                while ((line = sr2.ReadLine()) != null)
                {
                    if (line != "")
                    {
                        string[] c = line.Split(':');
                        if (c[1] == name)
                        {//ランクを置き換え,1行ずつやる
                            if (c[0] == "↓")
                            {
                                s = s + "" + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            else if (c[0] == "")
                            {
                                s = s + "↑" + ":" + c[1] + ":" + c[2] + "\n";
                            }
                            else if (c[0] == "↑")
                            {
                                s = s + "↓" + ":" + c[1] + ":" + c[2] + "\n";
                            }

                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                        }
                    }
                }
                sr2.Close();
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.Write(s);
                sw2.Close();
            }
        }

        public List<string> AllGroups()
        {
            List<string[]> m = ReadMemberFile();
            List<string> result = new List<string>();
            result.Add("部内");
            result.Add("所属ナシ");
            foreach(string[] mem in m)
            {
                if(result.IndexOf(mem[2]) == -1)
                {
                    result.Add(mem[2]);
                }
                else
                {//既にリストに加えている
                }
            }
            return result;
        }
        public void ChangeGroup(string pre, string name)
        {//所属名をnameでおきかえ,部内は書き換え不可なのでforeignersのみでよい
            sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            while ((line = sr2.ReadLine()) != null)
            {
                if(line != "")
                {
                    string[] c = line.Split(':');
                    if (c[2] == pre)
                    {//所属名を置き換え,ランクや所属が置き換えられてはいけないので1行ずつやる
                        s = s + c[0] + ":" + c[1] + ":" + name + "\n";
                    }
                    else
                    {
                        s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                    }
                }             
            }
            sr2.Close();
            sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw2.Write(s);
            sw2.Close();
        }

        public void AddGroup(string name)
        {
            sw2 = new StreamWriter(foreignerFilePath, true, Encoding.GetEncoding("Shift_JIS"));
            try
            {
                sw2.WriteLine("\n:sample:" + name);
            }
            finally
            {
                sw2.Close();
            }
        }
        public void DeleteGroup(string name)
        {//nameの所属を空文字に置き換えて所属ナシにする
            sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            while ((line = sr2.ReadLine()) != null)
            {
                if (line != "")
                {
                    string[] c = line.Split(':');
                    if (c[2] == name)
                    {//所属名を置き換え,ランクや所属が置き換えられてはいけないので1行ずつやる
                        if(c[1].Length >= 6)
                        {
                            if (c[1].Substring(0, 6) == "sample")
                            {//こいつは消す(所属ナシにsampleが溜まってしまうため)

                            }
                            else
                            {
                                s = s + c[0] + ":" + c[1] + ":" + "\n";
                            }
                        }
                        else
                        {
                            s = s + c[0] + ":" + c[1] + ":" + "\n";
                        }  
                    }
                    else
                    {
                        s = s + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                    }
                }
            }
            sr2.Close();
            sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            //上書き保存
            sw2.Write(s);
            sw2.Close();
        }

        public void SetAlgorithm(string tag)
        {
            string[] t = tag.Split(':');
            int coma = int.Parse(t[0]);
            int set = int.Parse(t[1]);
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            int i = -1;
            while ((line = sr.ReadLine()) != null)
            {
                if (line != "")
                {
                    if(i < 1)
                    {
                        s = s + line + "\n";
                        i++;
                    }
                    else
                    {//iコマ目
                        if (i == coma)
                        {//アルゴリズム置き換え対象
                            s = s + set + "\n";
                        }
                        else
                        {
                            s = s + line + "\n";
                        }
                        i++;
                    }      
                }
            }
            sr.Close();
            sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw.Write(s);
            sw.Close();
        }
        public void PlusTable()
        {
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            int i = -1;
            while ((line = sr.ReadLine()) != null)
            {
                if (line != "")
                {
                    if (i == -1)
                    {//台数を1増やす
                        string[] table = line.Split(':');
                        if(int.Parse(table[1]) >= 50)
                        {//50以降は増えない
                            s = s + line + "\n";
                        }
                        else
                        {
                            s = s + "table:" + (int.Parse(table[1]) + 1).ToString() + "\n";
                        }
                        
                    }
                    else
                    {//他は変えない
                        s = s + line + "\n";
                    }
                    i++;
                }
            }
            sr.Close();
            sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw.Write(s);
            sw.Close();
        }
        public void MinusTable()
        {
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            int i = -1;
            while ((line = sr.ReadLine()) != null)
            {
                if (line != "")
                {
                    if (i == -1)
                    {//台数を1減らす
                        string[] table = line.Split(':');
                        if (int.Parse(table[1]) <= 1)
                        {//1以降は減らない
                            s = s + line + "\n";
                        }
                        else
                        {
                            s = s + "table:" + (int.Parse(table[1]) - 1).ToString() + "\n";
                        }

                    }
                    else
                    {//他は変えない
                        s = s + line + "\n";
                    }
                    i++;
                }
            }
            sr.Close();
            sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw.Write(s);
            sw.Close();
        }
        public void PlusComa()
        {
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            int i = -1;
            while((line = sr.ReadLine()) != null)
            {
                if(i == 0)
                {
                    string coma = line.Split(':')[1];
                    i = int.Parse(coma);
                    break;//コマ数を取得
                }
                s = s + line + "\n";
                i++;
            }
            if(i >= 50)
            {//50以上は増えない
                sr.Close();
            }
            else
            {
                s = s + "coma:" + (i + 1).ToString() + "\n";
                line = sr.ReadToEnd();
                //末尾に0を足す
                s = s + line + "\n0\n";
                sr.Close();
                sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw.Write(s);
                sw.Close();
            }
            
        }
        public void MinusComa()
        {
            sr = new StreamReader(configFilePath, Encoding.GetEncoding("Shift_JIS"));
            string s = "";
            string line;
            int i = -1;
            while ((line = sr.ReadLine()) != null)
            {
                if (i == 0)
                {
                    string coma = line.Split(':')[1];
                    i = int.Parse(coma);
                    break;//コマ数を取得
                }
                s = s + line + "\n";
                i++;
            }
            if(i <= 1)
            {//これ以上は減らない
                sr.Close();
            }
            else
            {
                s = s + "coma:" + (i - 1).ToString() + "\n";
                line = (sr.ReadToEnd()).TrimEnd();
                s = s + line.Remove(line.Length - 1);//末尾一文字削除
                sr.Close();
                sw = new StreamWriter(configFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw.Write(s);
                sw.Close();
            }
        }
        public void AddLINEToken(string token)
        {
            if (!Directory.Exists("Texts"))
            {
                Directory.CreateDirectory("Texts");
            }
            //ファイルがどんな状況でも上書き
            sw3 = new StreamWriter(LINEtokenFilePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw3.WriteLine(token);
            sw3.Close();
        }

        public bool SendToLINE(string result)
        {
            if (!File.Exists(LINEtokenFilePath))
            {
                return false;
            }

            sr3 = new StreamReader(LINEtokenFilePath, Encoding.GetEncoding("Shift_JIS"));

            List<string> tokens = new List<string>();
            string line;
            while ((line = sr3.ReadLine()) != null)
            {//テキストが無くなるまで1行ずつ読む
                if (line != "")
                {//ランク，名前，所属の配列
                    string token = line;
                    tokens.Add(token);
                }
            }

            var url = "https://notify-api.line.me/api/notify";
            var enc = Encoding.UTF8;
            try
            {
                //foreach (string token in tokens)
                //{
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "message", "\n組み合わせたコマよ～↓\n" + result },
                        });
                using (var hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens[0]);

                    var response = hc.PostAsync(url, content);
                    response.Wait();
                    sr3.Close();
                    if (response.Result.IsSuccessStatusCode)
                    {//正常に送信できている
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //}
            }
            catch
            {
                sr3.Close();
                //MessageBox.Show("送信できませんでした");
                return false;
            }
            
            
        }

        public List<string> ReadTalkFile()
        {//セリフテキストを読み込み配列にして返す
            if (!File.Exists(talkFilePath))
            {//なければnull
                return null;
            }

            sr4 = new StreamReader(talkFilePath, Encoding.GetEncoding("UTF-8"));

            List<string> result = new List<string>();

            string line;
            int i = 0;
            string[] n = { "\\n" };
            while ((line = sr4.ReadLine()) != null && i < 100)
            {//テキストが無くなるまで1行ずつ読む(Max100)
                if (line != "")
                {
                    string res = "";
                    //改行エスケープ文字はこちらで処理
                    string[] l = line.Split(n, StringSplitOptions.None);
                    foreach(string ll in l)
                    {
                        res = res + ll + "\n";
                    }
                    result.Add(res);
                }
                i++;
            }
        
            sr4.Close();

            return result;
        }

    }
}
