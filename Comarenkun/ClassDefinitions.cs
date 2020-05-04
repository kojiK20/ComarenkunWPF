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
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;


namespace Comarenkun
{
    public class Group : INotifyPropertyChanged
    {//所属名のクラス,INotifyPropertyChangedインタフェースを継承することでプロパティの変更を通知する

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }
        private double _FontSize;
        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                _FontSize = value;
                OnPropertyChanged("FontSize");
            }
        }
        private SolidColorBrush _Color;
        public SolidColorBrush Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                OnPropertyChanged("Color");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GroupList
    {//Groupのコレクションをプロパティにもつクラス

        public ObservableCollection<Group> List { get; set; }
        public GroupList()
        {//コンストラクタ
            List = new ObservableCollection<Group>();
        }
        public void Add(Group g)
        {
            List.Add(g);
        }
        public void Remove(Group g)
        {
            List.Remove(g);
        }
        public void Clear()
        {
            List.Clear();
        }
    }

    public class Member : INotifyPropertyChanged
    {//メンバーのクラス
        private string _Rank;
        public string Rank
        {
            get { return _Rank; }
            set
            {
                _Rank = value;
                OnPropertyChanged("Rank");
            }
        }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }
        private SolidColorBrush _Color;
        public SolidColorBrush Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                OnPropertyChanged("Color");
            }
        }
        private string _Group;
        public string Group
        {
            get { return _Group; }
            set
            {
                _Group = value;
                OnPropertyChanged("Group");
            }
        }
        private double _RankFontSize;
        public double RankFontSize
        {
            get { return _RankFontSize; }
            set
            {
                _RankFontSize = value;
                OnPropertyChanged("RankFontSize");
            }
        }
        private double _NameFontSize;
        public double NameFontSize
        {
            get { return _NameFontSize; }
            set
            {
                _NameFontSize = value;
                OnPropertyChanged("NameFontSize");
            }
        }
        private double _DeleteFontSize;
        public double DeleteFontSize
        {
            get { return _DeleteFontSize; }
            set
            {
                _DeleteFontSize = value;
                OnPropertyChanged("DeleteFontSize");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class MemberList
    {//メンバーのObservableCollectionをもつクラス
        public ObservableCollection<Member> List { set; get; }
        public MemberList()
        {
            List = new ObservableCollection<Member>();
        }
        public string Rank(string name)
        {
            foreach(Member m in List)
            {
                if(m.Name == name)
                {
                    return m.Rank;
                }
            }
            return null;
        }
        public void Add(Member m)
        {
            List.Add(m);
        }
        public void Remove(Member m)
        {
            List.Remove(m);
        }
        public void Clear()
        {
            List.Clear();
        }
        public Member Find(string name)
        {//nameで探す
            foreach(Member m in List)
            {
                if(m.Name == name)
                {
                    return m;
                }
            }
            return null;
        }
        public void Sort()
        {//ランクで昇順ソート．いったんListに移してソートしてObservableCollectionに戻す.計算量<=2n + n^2　=O(n^2)
            List<Member> pre = new List<Member>();
            foreach(Member m in List)
            {
                pre.Add(m);
            }
            int hoge = 0;
            if(int.TryParse(pre[0].Rank, out hoge))
            {//ランクが整数＝部内なら数値ソート
                pre.Sort((a, b) => int.Parse(a.Rank) - int.Parse(b.Rank));//ランクで昇順ソート(たぶんn^2以下)
            }
            else
            {//部外なら文字列ソート
                pre.Sort((a,b) => a.Rank.CompareTo(b.Rank));
            }
            
            List.Clear();
            foreach(Member m in pre)
            {
                List.Add(m);
            }
        }
    }

    public class Coma : INotifyPropertyChanged
    {//所属名のクラス,INotifyPropertyChangedインタフェースを継承することでプロパティの変更を通知する

        private string _Number0;
        public string Number0
        {//何コマ目のどのアルゴリズムか
            get { return _Number0; }
            set
            {
                _Number0 = value;
                OnPropertyChanged("Number0");
            }
        }
        private string _Number1;
        public string Number1
        {//何コマ目のどのアルゴリズムか
            get { return _Number1; }
            set
            {
                _Number1 = value;
                OnPropertyChanged("Number1");
            }
        }
        private string _Number2;
        public string Number2
        {//何コマ目のどのアルゴリズムか
            get { return _Number2; }
            set
            {
                _Number2 = value;
                OnPropertyChanged("Number2");
            }
        }
        private int _Algorithm;
        public int Algorithm
        {//0->遠，1->近，2->ランダム
            get { return _Algorithm; }
            set
            {
                _Algorithm = value;
                OnPropertyChanged("Algorithm");
            }
        }
        private string _Label;
        public string Label
        {
            get { return _Label; }
            set
            {
                _Label = value;
                OnPropertyChanged("Label");
            }
        }
        /*private double _CheckFontSize;
        public double CheckFontSize
        {
            get { return _CheckFontSize; }
            set
            {
                _CheckFontSize = value;
                OnPropertyChanged("CheckFontSize");
            }
        }*/
        private string _Check0;
        public string Check0
        {
            get { return _Check0; }
            set
            {
                _Check0 = value;
                OnPropertyChanged("Check0");
            }
        }
        private string _Check1;
        public string Check1
        {
            get { return _Check1; }
            set
            {
                _Check1 = value;
                OnPropertyChanged("Check1");
            }
        }
        private string _Check2;
        public string Check2
        {
            get { return _Check2; }
            set
            {
                _Check2 = value;
                OnPropertyChanged("Check2");
            }
        }
        public void Check()
        {
            if(Algorithm == 0)
            {
                Check0 = "★";
                Check1 = "";
                Check2 = "";
            }else if(Algorithm == 1)
            {
                Check0 = "";
                Check1 = "★";
                Check2 = "";
            }
            else
            {
                Check0 = "";
                Check1 = "";
                Check2 = "★";
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ComaList
    {//Comaのコレクションをプロパティにもつクラス

        public ObservableCollection<Coma> List { get; set; }
        public ComaList()
        {//コンストラクタ
            List = new ObservableCollection<Coma>();
        }
        public void Add(Coma c)
        {
            List.Add(c);
        }
        public void Remove(Coma c)
        {
            List.Remove(c);
        }
        public void Clear()
        {
            List.Clear();
        }
        public void Change(string tag)
        {
            string[] coma = tag.Split(':');

            foreach(Coma c in List)
            {
                string[] comama = c.Number0.Split(':');
                if(comama[0] == coma[0])//変更するコマ
                {//変更
                    c.Algorithm = int.Parse(coma[1]);
                    c.Check();
                    break;
                }
            }
        }
    }

    public class OpaqueClickableImage : Image//コマ練くん用，マウスオーバー判定をカスタムしたImageクラス
    {
        //Image.HitTestCoreメソッドの上書き
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            var source = (BitmapSource)Source;

            //get the pixel of the source that was hit
            var x = (int)(hitTestParameters.HitPoint.X / ActualWidth * source.PixelWidth);
            var y = (int)(hitTestParameters.HitPoint.Y / ActualHeight * source.PixelHeight);



            //copy the single pixel in to a new byte array representing RGBA
            var pixel = new byte[4];
            source.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            //check the alpha (transparency) of the pixel
            //- threshold can be adjusted from 0 to 255
            if (pixel[0] == 0)
            {
                //MessageBox.Show(pixel[0].ToString());//test
                return null;
            }
            //不透明な場合のみマウスが上にありますよと返す
            return new PointHitTestResult(this, hitTestParameters.HitPoint);

        }
    }
}