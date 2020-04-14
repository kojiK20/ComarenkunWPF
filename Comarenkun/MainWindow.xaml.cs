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

namespace Comarenkun
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MatchingLogic mlogic;//ロジック部を実行してくれる人
        List<string[]> members;
        List<string> memberNames;
        List<string> groups;
        public GroupList groupList;

        int gridRow = 30;//疑似グリッド．列数
        int gridColumn = 30;//疑似グリッド．行数
        public double rowSize = 0;
        public double columnSize = 0;//列，行それぞれ1マス分のサイズ
        double windowWidth = 0;
        double windowHeight = 0;//ウィンドウのサイズ
        string format = "F2";//double->stringの丸めオプション
        

        //params配列は<座標列(x座標)><座標行(y座標)><列数(横幅)><行数(縦幅)>を格納
        //さらに多角形ならば，その長方形に対し，左上を基準とした
        //<左下頂点のx方向のずれ><y方向のずれ>
        //<右上頂点のx方向のずれ><y方向のずれ>
        //<右下頂点のx方向のずれ><y方向のずれ>を続いて格納(4角形の場合)
        double[] initialBackObjectParams1 = { 0, 0, 30, 11, 0, 0, 30, 0, 0, 10, -18, 6 };//五角形(2つ目の角は左上の角基準)
        double[] initialBackObjectParams2 = { 0, 11, 30, 19, 0, 0, 12, 4, 0, -1, 0, 0 };//五角形(2つ目の角は左上の角基準)
        double[] backObjectParams1 = { -6, 0, 12, 30, 0, 0, 0, 0, -10, 0 };//左上濃三角
        double[] backObjectParams2 = { 30, 5, 2, 26, -15, 0, 0, 0, 0, 0 };//右下濃三角
        double[] backObjectParams3 = { -6, 3, 25, 30, 0, 0, -25, 0, 0, 0 };//左下薄三角
        double[] backObjectParams4 = { 15, 0, 30, 20, 0, -20, 0, 0, 0, 0 };//右上薄三角        
        double[] backObjectParams5 = { 11.5, 0, 5, 30, 0, 0, 0, 0, 0, 0 };//中央白線
        double[] headerParams = { 0, 0, 30, 3, 0, 0, 0, 0, 0, -2 };//ヘッダの傾き2/30
        double[] headerAccentParams = { 0, 0, 30, 3.75, 0, 0, 0, 0, 0, -2.2 };
        double[] footerParams = { 0, 21 - 1 / 15, 30, 9, 0, 0, 0, 1, 0, 0 };
        double[] footerBottomParams = { 0, 22.2 - 1 / 15, 30 };//footerのテンプレートを利用するので座標のみでよい
        double[] footerAccentParams = { 0, 21.8 - 1 / 15, 30, 0.6, 0, 0.2, 0, 1, 0, 0.8 };
        double[] footerCircleParams = { 24, 19, 14, 18 };
        double[] footerCircleAccentParams = { 24, 20, 13, 17 };
        double[] footerCircleBlackParams = { 24.2, 20.8, 12, 16 };
        double[] comarenkunButtonParams = { 24, 19, 10, 10 };
        double[] memberButtonParams = { 1, 4.75 - 1.0 / 15.0, 12, 15, 0, 0, 0, -4.0 / 5.0, 1, 13.0 / 30.0 };
        double[] matchingButtonParams = { 14, 4.75 - 15.0 / 15.0, 14, 15.0 + 19.0 / 15.0, 1, 1.0 / 30.0, 0, -1, 0, 7.0 / 15.0 };//フッタの傾き1/30
        double[] toMenuButtonParams = { 0, 0, 5, 24, 0, 0, -2, 0, 0, 0 };
        double[] groupButtonsParams = { 11, 3, 17, 19 };
        double[] groupButtonParams = { 0, 0, 14, 4, 1, 0, 0, 0, 1, 0 };//メンバ画面で所属を選択するボタン
        //double[] shozokuLabelParams = { 4, 4, 6, 10 };//左上
        double[] talkLabelParams = { 0, 23, 24, 7, 0, 0, 0, 0, 0, 0 };

        int clickNumber = 0;//クリックのエフェクトに使用
        Storyboard s1 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s2 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s3 = new Storyboard();//クリックのエフェクトに使用
        bool memberButtonPush = false;
        bool matchingButtonPush = false;
        bool toMenuButtonPush = false;

        string menu = "Menu";
        bool nowMenu = false;
        string member = "Member";
        bool nowMember = false;
        string matching = "Matching";
        bool nowMatching = false;

        public MainWindow()
        {
            InitializeComponent();
            SetMode(menu);
            mlogic = new MatchingLogic(this);//ロジック部を実行してくれる人
            members = mlogic.ReadMemberFile();
            memberNames = mlogic.AllMemberNames(members);
            groups = mlogic.AllGroups(members);

            //XAMLからこのクラスのメンバ変数を参照可能にする
            this.DataContext = this;
            this.toMenuButton.Visibility = Visibility.Hidden;
            this.toMenuButton.Content = "モ\nド\nル";//行替えエスケープがxamlコードからは利用できない
            this.talkLabel.Content = "Hello,Comarenkun";

            groupList = new GroupList();
            this.groupButtons.DataContext = groupList.List;
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {//このタイミングでバインディングする必要がある
            
            //this.groupButtons.DataContext = groupNames;
        }
        public void SetMode(string mode)//モード遷移時にこのメソッドでフラグを管理する
        {
            if (mode == "Menu")
            {
                nowMenu = true;
                nowMember = false;
                nowMatching = false;
            }
            else if (mode == "Member")
            {
                nowMenu = false;
                nowMember = true;
                nowMatching = false;
            }
            else if (mode == "Matching")
            {
                nowMenu = false;
                nowMember = false;
                nowMatching = true;
            }
            else
            {
                throw new System.ArgumentException("undefinedMode", "original");
            }
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();

            if (this.initialBackObject1.Visibility != Visibility.Collapsed)
            {
                PolygonSet("InitialBackObject1", this.initialBackObject1, initialBackObjectParams1);
                PolygonSet("InitialBackObject2", this.initialBackObject2, initialBackObjectParams2);
            }

            PolygonSet("BackObject1", this.backObject1, backObjectParams1);
            PolygonSet("BackObject2", this.backObject2, backObjectParams2);
            PolygonSet("BackObject3", this.backObject3, backObjectParams3);
            PolygonSet("BackObject4", this.backObject4, backObjectParams4);
            PolygonSet("BackObject5", this.backObject5, backObjectParams5);
            PolygonSet("HeaderAccent", this.headerAccent, headerAccentParams);
            LabelSet("Header", this.header, headerParams);
            PolygonSet("Footer", this.footer, footerParams);
            //footerのStyleを再利用
            this.footerBottom.Margin = new Thickness(rowSize * footerBottomParams[0], columnSize * footerBottomParams[1], 0, 0);
            PolygonSet("FooterAccent", this.footerAccent, footerAccentParams);
            EllipseSet("FooterCircle", this.footerCircle, footerCircleParams);
            EllipseSet("FooterCircleAccent", this.footerCircleAccent, footerCircleAccentParams);
            EllipseSet("FooterCircleBlack", this.footerCircleBlack, footerCircleBlackParams);
            ComarenkunSet("ComarenkunButton", this.comarenkunButton, comarenkunButtonParams);

            //ComarenkunSet("ComarenkunButton", this.comarenkunButton, comarenkunButtonParams);
            //Visbillityはストーリーボードで管理できるのでモードによるテンプレートのセッティング分けはしないほうがいいか？
            //if (nowMenu)
            //{
            PolygonButtonSet("MemberButton", this.memberButton, memberButtonParams);
            PolygonButtonSet("MatchingButton", this.matchingButton, matchingButtonParams);
            //}
            //else if (nowMember)
            //{
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
            ListBoxSet("GroupButtons", this.groupButtons, groupButtonsParams);
            PolygonButtonSet("GroupButton", null, groupButtonParams);
            //GroupButtonsSet("GroupButton", groupButtonParams);
            //PolygonButtonSet("GroupButton", this.sampleGroup, groupButtonParams);
            //}
            //else if (nowMatching)
            //{
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            //}


        }

        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            beginClickEffect(e);
            //マウスボタンを離してもイベントは終了しないのでストーリーボードも消えない
        }
        private void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //マウスボタンを離した際に明示的にストーリーボードを消しておく
            removeClickEffect();
        }

        private void memberButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Storyboard enter = (Storyboard)this.FindResource("MemberButtonMouseEnter");
            if (!memberButtonPush && !matchingButtonPush)
            {//ボタンをクリックした際にはエンターイベントは発生させない
                ButtonBrighten(this.memberButton);
                enter.Begin();
            }
            else
            {
                enter.Stop();
            }

        }
        private void memberButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (!memberButtonPush && !matchingButtonPush)
            {//ボタンをクリックした際にはリーブイベントは発生させない
                Storyboard enter = (Storyboard)this.FindResource("MemberButtonMouseEnter");
                enter.Stop();
                Storyboard leave = (Storyboard)this.FindResource("MemberButtonMouseLeave");
                leave.Begin();
            }

        }

        private void memberButton_Click(object sender, RoutedEventArgs e)
        {

            memberButtonPush = true;
            Storyboard enter = (Storyboard)this.FindResource("MemberButtonMouseEnter");
            enter.Stop();
            Storyboard click = (Storyboard)this.FindResource("MemberButtonClick");
            click.Begin();
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            this.toMenuButton.Visibility = Visibility.Visible;

            //groupList.Clear();//newするとバインドが外れる

            this.talkLabel.Content = "所属を選択するコマああああああああああああ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
            this.groupButtons.Visibility = Visibility.Visible;
            foreach (string name in groups)
            {

                double contentSize = name.Replace("\n", "").Length;
                double fontSize = rowSize * groupButtonParams[2] * 1.0 / contentSize * 0.8;
                //MarginCenter(groupButtonParams, contentSize, "GroupButton");
                //GroupName n = new GroupName(name, fontSize);
                //groupNames.Add(n);
            }
            Group a = new Group();
            a.Name = " aaaa";
            a.FontSize = 30;
            a.Color = Brushes.LightGreen;
            a.IsHit = true;
            groupList.Add(a);
        }
        private void memberButtonClickCompleted(object sender, EventArgs e)
        {   //MemberButtonClidkストーリーボードが終了したときに発生するイベント
            memberButtonPush = false;
            //ストーリーボード実行中にウィンドウサイズが変わった時にはまだサイズ変更を反映するのでストーリーボード終了時にSetModeする
            SetMode(member);
            //ストーリーボード実行中にウィンドウサイズが変わった時，出てきたメニューボタンのサイズが反映できないので一度再描写しておく
        }

        private void matchingButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Storyboard enter = (Storyboard)this.FindResource("MatchingButtonMouseEnter");
            if (!memberButtonPush && !matchingButtonPush)
            {//ボタンをクリックした際にはエンターイベントは発生させない
                ButtonBrighten(this.matchingButton);
                enter.Begin();
            }
            else
            {
                enter.Stop();
            }

        }
        private void matchingButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (!memberButtonPush && !matchingButtonPush)
            {//ボタンをクリックした際にはリーブイベントは発生させない
                Storyboard enter = (Storyboard)this.FindResource("MatchingButtonMouseEnter");
                enter.Stop();
                Storyboard leave = (Storyboard)this.FindResource("MatchingButtonMouseLeave");
                leave.Begin();
            }

        }
        private void matchingButton_Click(object sender, RoutedEventArgs e)
        {
            matchingButtonPush = true;
            Storyboard enter = (Storyboard)this.FindResource("MatchingButtonMouseEnter");
            enter.Stop();
            Storyboard click = (Storyboard)this.FindResource("MatchingButtonClick");
            click.Begin();
            
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            this.toMenuButton.Visibility = Visibility.Visible;

            
        }
        private void matchingButtonClickCompleted(object sender, EventArgs e)
        {   //MatchingButtonClidkストーリーボードが終了したときに発生するイベント
            matchingButtonPush = false;
            SetMode(matching);
        }

        private void toMenuButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Storyboard enter = (Storyboard)this.FindResource("ToMenuButtonMouseEnter");
            if (!toMenuButtonPush)
            {//ボタンをクリックした際にはエンターイベントは発生させない
                ButtonBlueBrighten(this.toMenuButton);
                enter.Begin();
                
            }
            else
            {
                enter.Stop();
            }
        }
        private void toMenuButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (!toMenuButtonPush)
            {//ボタンをクリックした際にはリーブイベントは発生させない
                Storyboard enter = (Storyboard)this.FindResource("ToMenuButtonMouseEnter");
                enter.Stop();
                Storyboard leave = (Storyboard)this.FindResource("ToMenuButtonMouseLeave");
                leave.Begin();
            }
        }
        private void toMenuButton_Click(object sender, RoutedEventArgs e)
        {
            this.groupButtons.Visibility = Visibility.Hidden;

            toMenuButtonPush = true;
            memberButtonPush = false;
            matchingButtonPush = false;
            Storyboard enter = (Storyboard)this.FindResource("ToMenuButtonMouseEnter");
            enter.Stop();

            Storyboard click = (Storyboard)this.FindResource("ToMenuButtonClick");
            click.Begin();
            this.talkLabel.Content = "Hello,Comarenkun";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
        }
        private void toMenuButtonCompleted(object sender, EventArgs e)
        {   //ToMenuButtonClidkストーリーボードが終了したときに発生するイベント
            SetMode(menu);
            toMenuButtonPush = false;
        }

        
    }

    /*public class GroupName : INotifyPropertyChanged
    {//Nameプロパティに所属名を保持するクラス
        public string name { set; get; }
        public double fontSize { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public GroupName(string Name, double FontSize)
        {
            this.name = Name;
            this.fontSize = FontSize;
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if(value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double FontSize
        {
            get
            {
                return this.fontSize;
            }
            set
            {
                if(value != this.fontSize)
                {
                    this.fontSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
    }*/
    public class Group
    {
        public string Name { get; set; }
        public double FontSize { get; set; }
        public SolidColorBrush Color { get; set; }
        public bool IsHit { get; set; }
    }
    public class GroupList
    {
        public ObservableCollection<Group> List { get; set; }
        public GroupList()
        {
            List = new ObservableCollection<Group>
            {
                new Group {Name=" 部内", FontSize=50.0,Color=Brushes.LightBlue, IsHit = false },
                new Group {Name=" あああああああああ", FontSize=50.0,Color=Brushes.LightGreen, IsHit = true }
            };
        }
        public void Add(Group g)
        {
            List.Add(g);
        }
    }

    public class OpaqueClickableImage : Image
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
            if(pixel[0] == 0)
            {
                //MessageBox.Show(pixel[0].ToString());//test
                return null;
            }
            //不透明な場合のみマウスが上にありますよと返す
            return new PointHitTestResult(this, hitTestParameters.HitPoint);

        } 
    }

   // public event DependencyPropertyChangedEventHandler PropertyChanged = null;
   // protected void OnPropertyChanged(string info)
        //{
            //this.PropertyChanged?.Invoke(this, new DependencyPropertyChangedEventArgs(info));
        //}
    
}
