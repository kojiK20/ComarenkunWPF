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
        private string memberFilePath = "/Texts/members.txt";//絶対パスは"pack://application:,,,/Comarenkun;component//"
        private string foreignerFilePath = "/Texts/foreigners.txt";

        public MatchingLogic(MainWindow window)
        {//メインウィンドウのオブジェクトをwindowから参照する
            this.window = window;
        }
        
        public List<string[]> ReadMemberFile()
        {//メンバテキストを読み込み配列にして返す
            Uri memberFileUri = new Uri(memberFilePath, UriKind.Relative);
            Uri foreignerFileUri = new Uri(foreignerFilePath, UriKind.Relative);
            var memberFileInfo = Application.GetResourceStream(memberFileUri);
            var foreignerFileInfo = Application.GetResourceStream(foreignerFileUri);
            StreamReader sr1 = new StreamReader(memberFileInfo.Stream, Encoding.GetEncoding("Shift_JIS"));
            StreamReader sr2 = new StreamReader(foreignerFileInfo.Stream, Encoding.GetEncoding("Shift_JIS"));
            List<string[]> result = new List<string[]>();
            string line;
            while((line = sr1.ReadLine()) != null)
            {//テキストが無くなるまで1行ずつ読む
                if(line != "")
                {//ランク，名前，所属の配列
                    string[] memberNames = line.Split(':');
                    memberNames[2] = "部内";
                    result.Add(memberNames);
                }
            }
            while((line = sr2.ReadLine()) != null)
            {//外部参加者も数える
                if(line != "")
                {
                    string[] foreignerNames = line.Split(':');
                    if(foreignerNames[2] == "")
                    {
                        foreignerNames[2] = "所属ナシ";
                    }
                    result.Add(foreignerNames);
                }
            }
            return result;
        }

        public List<string> AllMemberNames(List<string[]> m)
        {
            List<string> result = new List<string>();
            foreach(string[] mem in m)
            {
                result.Add(mem[1]);
            }
            return result;
        }
        public List<string> AllGroups(List<string[]> m)
        {
            List<string> result = new List<string>();
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

    }
}
