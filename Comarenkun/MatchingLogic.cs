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

namespace Comarenkun
{
    public class MatchingLogic
    {
        private MainWindow window;
        private string memberFilePath = "../../Texts/members.txt";//絶対パスは"pack://application:,,,/Comarenkun;component//"
        private string foreignerFilePath = "../../Texts/foreigners.txt";
        StreamReader sr1;
        StreamReader sr2;
        StreamWriter sw1;
        StreamWriter sw2;

        public MatchingLogic(MainWindow window)
        {//メインウィンドウのオブジェクトをwindowから参照する
            this.window = window;
        }
        
        public void CreateIfNotExists()
        {
            if (!Directory.Exists("../../Texts"))
            {
                Directory.CreateDirectory("../../Texts");
            }
            if (!File.Exists("../../Texts/members.txt"))
            {
                //File.Create("../../Texts/members.txt");
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.WriteLine("1:sample:");
                sw1.Close();
            }
            if (!File.Exists("../../Texts/foreigners.txt"))
            {
                //File.Create("../../Texts/foreigners.txt");
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
                    string[] c = line.Split(':');
                    //名前が重複しているなら連番にする
                    c[1] = NameDuplicateCheck(names, c[1]);
                    names.Add(c[1]);

                    int i;
                    if (int.TryParse(c[0], out i) == false)
                    {//ランクが整数値以外の場合0に置き換え,名前や所属が置き換えられてはいけないので1行ずつやる
                        s1 = s1 + "0:" + c[1] + ":" + c[2] + "\n";
                    }
                    else
                    {
                        s1 = s1 + c[0] + ":" + c[1] + ":" + c[2] + "\n";
                    }
                    
                }
            }
            sr1.Close();
            names = new List<string>();
            while((line = sr2.ReadLine()) != null)
            {//ランクが↑↓空以外の場合空に置き換え
                if(line != "")
                {
                    string[] c = line.Split(':');

                    //名前が重複しているなら連番にする
                    c[1] = NameDuplicateCheck(names, c[1], 2);
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
                return NameDuplicateCheck(names, name + 2, 2 + 1);
            }
        }
        public string NameDuplicateCheck(List<string> names, string name, int i)
        {//名前が重複してたら連番にした名前を返す
            if (names.IndexOf(name) == -1)
            {//重複していない
                return name;
            }
            else
            {//重複している時，連番を1増やした文字で再帰チェック
                return NameDuplicateCheck(names, name + i.ToString(), i + 1);
            }
        }

        public List<string[]> ReadMemberFile()
        {//メンバテキストを読み込み配列にして返す
            CreateIfNotExists();
            ReWriteIfNotRight();

            sr1 = new StreamReader(memberFilePath, Encoding.GetEncoding("Shift_JIS"));
            sr2 = new StreamReader(foreignerFilePath, Encoding.GetEncoding("Shift_JIS"));
            
            List<string[]> result = new List<string[]>();
            string line;
            try
            {
                while ((line = sr1.ReadLine()) != null)
                {//テキストが無くなるまで1行ずつ読む
                    if (line != "")
                    {//ランク，名前，所属の配列
                        string[] memberNames = line.Split(':');
                        if(memberNames.Length > 3)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        memberNames[2] = "部内";
                        result.Add(memberNames);
                    }
                }
            }
            catch
            {//ファイルの内容が形式に沿わない場合は，旨を伝えそのファイルをコピーし，新たに初期ファイルを作成する
                sr1.Close();
                sr2.Close();
                string path = "";
                for(int i = 1; true; i++)
                {
                    if(File.Exists("../../Texts/members_copy.txt") && File.Exists("../../Texts/members_copy" + i.ToString() + ".txt"))
                    {//コピー先ファイル名が重複しているとき連番にする
                    }
                    else if(File.Exists("../../Texts/members_copy.txt"))
                    {
                        path = "members_copy" + i.ToString() + ".txt";
                        File.Copy(memberFilePath, "../../Texts/" + path);
                        break;
                    }
                    else
                    {
                        path = "members_copy.txt";
                        File.Copy(memberFilePath, "../../Texts/" + path);
                        break;
                    }
                }
                sw1 = new StreamWriter(memberFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw1.WriteLine("1:sample:");
                sw1.Close();
                MessageBox.Show("Comarenkun/Texts/members.txtの形式が不正です．\n" + path + "にコピーして新たにmembers.txtを作成しました．");
                return ReadMemberFile();//再帰
            }
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
                        result.Add(foreignerNames);
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
                    if (File.Exists("../../Texts/foreigners_copy.txt") && File.Exists("../../Texts/foreigners_copy" + i.ToString() + ".txt"))
                    {//コピー先ファイル名が重複しているとき連番にする
                    }
                    else if (File.Exists("../../Texts/foreigners_copy.txt"))
                    {
                        path = "foreigners_copy" + i.ToString() + ".txt";
                        File.Copy(foreignerFilePath, "../../Texts/" + path);
                        break;
                    }
                    else
                    {
                        path = "foreigners_copy.txt";
                        File.Copy(foreignerFilePath, "../../Texts/" + path);
                        break;
                    }
                }
                sw2 = new StreamWriter(foreignerFilePath, false, Encoding.GetEncoding("Shift_JIS"));
                sw2.WriteLine(":sample:");
                sw2.Close();
                MessageBox.Show("Comarenkun/Texts/foreigners.txtの形式が不正です．\n" + path + "にコピーして新たにforeigners.txtを作成しました．");
                return ReadMemberFile();//再帰
            }
            
            sr1.Close();
            sr2.Close();
            
            return result;
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
                    sw1.WriteLine("\n" + rank + ":" + name + ":" + group + "\n");
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
                    sw2.WriteLine("\n" + rank + ":" + name + ":" + group + "\n");
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
                        {//所属名を置き換え,ランクや所属が置き換えられてはいけないので1行ずつやる
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
                        if (c[2] == preName)
                        {//所属名を置き換え,ランクや所属が置き換えられてはいけないので1行ずつやる
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
                            s = s + (int.Parse(c[0]) + 1).ToString() + ":" + c[1] + ":" + c[2] + "\n";
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
                            s = s + (int.Parse(c[0]) - 1).ToString() + ":" + c[1] + ":" + c[2] + "\n";
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
                    if (c[1] == "sample" && c[2] == name)
                    {//こいつは消す(所属ナシにsampleが溜まってしまうため)
                    }else if (c[2] == name)
                    {//所属名を置き換え,ランクや所属が置き換えられてはいけないので1行ずつやる
                           s = s + c[0] + ":" + c[1] + ":" + "\n";
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
}
