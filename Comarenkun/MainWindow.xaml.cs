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
using System.Net;
using System.Web;

namespace Comarenkun
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        FileLogic flogic;//ロジック部を実行してくれる人
        MatchingLogic mlogic;//
        List<string[]> members;
        List<string> memberNames;
        List<string[]> currentMembersToShow;
        List<string> configs;//台数とコマ数とアルゴリズムがとれる
        List<List<string>> participants = new List<List<string>>();//インデックス＋1コマ目の参加者string
        List<List<string>> increase = new List<List<string>>();//increace[i]:i-1 -> iコマ目での参加者 [0] = participants
        List<List<string>> decrease = new List<List<string>>();//decrease[i]:i-1 -> iコマ目での退出者 [0] = null
        List<string> groups;
        GroupList groupList;
        MemberList memberList;
        ComaList comaList;
        int tableNum = 10;
        int comaNum = 5;

        int gridRow = 30;//疑似グリッド．列数
        int gridColumn = 30;//疑似グリッド．行数
        public double rowSize = 0;
        public double columnSize = 0;//列，行それぞれ1マス分のサイズ
        double windowWidth = 0;
        double windowHeight = 0;//ウィンドウのサイズ
        bool shadow = true;
        bool noShadow = false;
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

        double[] groupAddButtonParams = { 25.5, 1.5, 4.5, 20, 1, 0, 0, 0, 0, 0 };//groupNameChangeButtonの表示に合わせてパラメータを変える
        double[] groupAddButtonParams2 = { 26, 11.5, 4.5, 10, 0.5, 0, 0, 0, 0, 0 };//groupNameChangeButtonが表示されてるとき用
        double[] groupAddButtonParamsCopy = { 25.5, 1.5, 4.5, 20, 1, 0, 0, 0, 0, 0 };//↑から戻すとき用
        double[] groupNameChangeButtonParams = { 5.1, 13.5, 6.5, 6.5 };
        double[] groupDeleteButtonParams = { 25.5, 1.5, 4.5, 10, 0.5, 0, 0, 0, 0, 0 };
        double[] groupOpenButtonParams = { 4.2, 4, 7.5, 7.5 };
        double[] groupButtonsParams = { 11, 3.1, 14.5, 19 };
        double[] groupButtonParams = { 0, 0, 13, 3, 1, 0, 0, 0, 0, 0 };//メンバ画面で所属を選択するボタン

        double[] memberAddButtonParams = { 25.6, 1.5, 4.4, 20, 1, 0, 0, 0, 0, 0 };
        double[] memberGroupLabelParams = { 23, 3.75, 3, 13.25, 0, 0, 0, 0, 0, 0 };//メンバ画面で所属名を表示
        double[] memberSortButtonParams = { 22.5, 17, 3.5, 3, 0, 0, 0, 0, 3.0/20, 0 };
        double[] memberButtonsParams = { 7, 5, 15, 16};//所属選択後にメンバ編集するためのボタン
        double[] memberRankButtonParams = { 0, 0, 2.2, 2, 0, 0, 0, 0, -0.2, 0 };
        double[] memberNameButtonParams = { 2.3, 0, 9.6, 2, -0.2, 0, 0, 0, 0, 0 };
        double[] memberDeleteButtonParams = { 12, 0, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] rankNameLabelParams = { 8, 3.5, 16.5, 1.5, 0, 0, 0, 0, 0, 0 };//「Rank/Name」

        double[] participantGroupButtonsParams = { 5, 5, 10, 17.75 };//マッチング画面でのグループListBox
        double[] participantGroupButtonParams = { 0, 0, 9, 3, 1, 0, 0, 0, 0, 0 };
        double[] participantMemberButtonsParams = { 15, 5, 10.5, 17.75 };//マッチング画面でのメンバーListBox
        double[] participantRankLabelParams = { 0, 0, 1, 2, 0, 0, 1.4, 0, 1, 0.5, 0.6, 0 };
        double[] participantMemberButtonParams = { 2, 0, 7.5, 2, 0, 0, 0, 0, 0, 0 };
        double[] nextButtonParams = { 26, 1, 4, 20, 0.5, 0, 0, 0, 0, 0 };
        double[] ParticipantNamesTextBoxParams = { 4, 3.6, 21, 1.2 };//選択された参加者を表示
        double[] ParticipantNamesSumTextBoxParams = { 5, 3.5, 20, 17 };//選択された参加者のまとめを表示
        double[] configButtonParams = { 22.5, 0, 7.5, 1.55, 0, 0.55, 0, 0, 0, 0 };
        double[] tableButtonParams = { 5, 3.75, 10, 8, 1, 0, 0, 0, 0, 0 };
        double[] comaButtonParams = { 6, 12.75, 9, 8, 1, 0, 0, 0, 0, 0 };
        double[] algorithmLabelParams = { 20, 2, 10, 2, 0, 0, 0, 0, 0, 0 };
        double[] algorithmLabel0Params = { 20, 3.75, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] algorithmLabel1Params = { 23, 3.75, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] algorithmLabel2Params = { 26, 3.75, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] comaListBoxParams = { 16, 5.75, 13, 16, 0, 0, 0, 0, 0, 0 };
        double[] comaAlgorithmLabelParams = { 0, 0, 3, 2, 0, 0, 0, 0, 0, 0 };
        double[] comaAlgorithmButton0Params = { 4, 0, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] comaAlgorithmButton1Params = { 7, 0, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] comaAlgorithmButton2Params = { 10, 0, 2, 2, 0, 0, 0, 0, 0, 0 };

        double[] talkLabelParams = { 0, 23, 24, 7, 0, 0, 0, 0, 0, 0 };

        int clickNumber = 0;//クリックのエフェクトに使用
        Storyboard s1 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s2 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s3 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s4 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s5 = new Storyboard();//クリックのエフェクトに使用
        bool memberButtonPush = false;
        bool matchingButtonPush = false;
        bool toMenuButtonPush = false;

        string menu = "Menu";
        bool nowMenu = false;
        string group = "Group";
        bool nowGroup = false;
        string member = "Member";
        bool nowMember = false;
        string matching = "Matching";
        bool nowMatching = false;
        int coma = 1;//現在設定中のコマ,Matchingモード＋何コマ目かで管理
        string config = "Config";
        bool nowConfig = false;

        public MainWindow()
        {
            InitializeComponent();
            SetMode(menu);
            flogic = new FileLogic(this);//ロジック部を実行してくれる人
            mlogic = new MatchingLogic(this);
            members = flogic.ReadMemberFile();
            memberNames = flogic.AllMemberNames();
            groups = flogic.AllGroups();

            //XAMLからこのクラスのメンバ変数を参照可能にする
            this.DataContext = this;
            this.toMenuButton.Visibility = Visibility.Hidden;
            this.toMenuButton.Content = "モ\nド\nル";//行替えエスケープがxamlコードからは利用できない
            this.groupAddButton.Content = "追\n加\n＋";
            this.groupDeleteButton.Content = "削\n除";
            this.memberAddButton.Content = "追\n加\n＋";
            this.participantNamesTextBox.Text = "コマ目：";
            this.nextButton.Content = "2\nコ\nマ\n目\n→";
            this.tableButton.Content = "台数：" + tableNum.ToString();
            this.comaButton.Content = "コマ数：" + comaNum.ToString();
            this.talkLabel.Content = "Hello,Comarenkun";
           

            groupList = new GroupList();
            this.groupButtons.DataContext = groupList.List;
            memberList = new MemberList();
            this.memberButtons.DataContext = memberList.List;
            this.participantMemberButtons.DataContext = memberList.List;
            comaList = new ComaList();
            this.comaListBox.DataContext = comaList.List;
            configs = flogic.ReadConfigFile();//configsを更新

            for (int i = 0; i < int.Parse(configs[1]); i++)
            {//あるコマまで，空Listで宣言しておく
                participants.Add(new List<string>());
                increase.Add(new List<string>());
                decrease.Add(new List<string>());
            }
            increase[0] = participants[0];//参照渡し
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {//このタイミングでバインディングする必要がある（ない）
            
            //this.groupButtons.DataContext = groupNames;
        }
        public void SetMode(string mode)//モード遷移時にこのメソッドでフラグを管理する
        {
            if (mode == "Menu")
            {
                nowMenu = true;
                nowGroup = false;
                nowMatching = false;
                nowMember = false;
                nowConfig = false;
            }
            else if (mode == "Group")
            {
                nowMenu = false;
                nowGroup = true;
                nowMatching = false;
                nowMember = false;
                nowConfig = false;
            }
            else if (mode == "Matching")
            {
                nowMenu = false;
                nowGroup = false;
                nowMatching = true;
                nowMember = false;
                nowConfig = false;
            }
            else if(mode == "Member")
            {
                nowMenu = false;
                nowGroup = false;
                nowMatching = false;
                nowMember = true;
                nowConfig = false;
            }
            else if(mode == "Config")
            {
                nowMenu = false;
                nowGroup = false;
                nowMatching = false;
                nowMember = false;
                nowConfig = true;
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
            //モードで場合分けするならば，「2階層(2モード)以上離れたコントロールのテンプレートは更新しない」例：MatchingモードではGroupモードのコントロールは更新しない．

            PolygonSet("BackObject1", this.backObject1, backObjectParams1);
            PolygonSet("BackObject2", this.backObject2, backObjectParams2);
            PolygonSet("BackObject3", this.backObject3, backObjectParams3);
            PolygonSet("BackObject4", this.backObject4, backObjectParams4);
            PolygonSet("BackObject5", this.backObject5, backObjectParams5);
            PolygonSet("HeaderAccent", this.headerAccent, headerAccentParams);
            LabelSet("Header", this.header, headerParams, shadow);
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
            PolygonButtonSet("MemberButton", this.memberButton, memberButtonParams, shadow);
            PolygonButtonSet("MatchingButton", this.matchingButton, matchingButtonParams, shadow);

            //}
            //else if (nowGroup)
            //{
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams, shadow);
            PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams, noShadow);    
            PolygonButtonSet("GroupDeleteButton", this.groupDeleteButton, groupDeleteButtonParams, noShadow);
            EllipseButtonSet("GroupNameChangeButton", this.groupNameChangeButton, groupNameChangeButtonParams, noShadow);
            EllipseButtonSet("GroupOpenButton", this.groupOpenButton, groupOpenButtonParams, noShadow);
            if (nowGroup || nowMember)
            {
                ListBoxSet("GroupButtons", this.groupButtons, groupButtonsParams);
                PolygonButtonSet("GroupButton", null, groupButtonParams, noShadow);
                ListBoxSet("MemberButtons", this.memberButtons, memberButtonsParams);
                PolygonButtonSet("MemberRankButton", null, memberRankButtonParams, noShadow);
                PolygonButtonSet("MemberNameButton", null, memberNameButtonParams, noShadow);
                PolygonButtonSet("MemberDeleteButton", null, memberDeleteButtonParams, noShadow);
            }
            GroupFontSizeSet();
            MemberFontSizeSet();

            PolygonButtonSet("MemberAddButton", this.memberAddButton, memberAddButtonParams, noShadow);
            LabelSet("MemberGroupLabel", this.memberGroupLabel, memberGroupLabelParams, noShadow);
            PolygonButtonSet("MemberSortButton", this.memberSortButton, memberSortButtonParams, noShadow);    
            LabelSet("RankNameLabel", this.rankNameLabel, rankNameLabelParams, noShadow);

            PolygonButtonSet("ConfigButton", this.configButton, configButtonParams, shadow);
            PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
            
            if (nowMatching || nowConfig )
            {
                ListBoxSet("GroupButtons", this.groupButtons, participantGroupButtonsParams);
                PolygonButtonSet("GroupButton", null, participantGroupButtonParams, noShadow);
                ListBoxSet("ParticipantMemberButtons", this.participantMemberButtons, participantMemberButtonsParams);
                PolygonButtonSet("ParticipantMemberButton", null, participantMemberButtonParams, noShadow);
                LabelSet("ParticipantRankLabel", null, participantRankLabelParams, noShadow);
                if ((string)this.nextButton.Content != "G\nO\n!" || (string)this.nextButton.Content != "オ\nカ\nワ\nリ")
                {
                    TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                }
                else
                {
                    TextBoxSet("ParticipantNamesSumTextBox", this.participantNamesTextBox, ParticipantNamesSumTextBoxParams);
                }

                PolygonButtonSet("TableButton", this.tableButton, tableButtonParams, noShadow);
                PolygonButtonSet("ComaButton", this.comaButton, comaButtonParams, noShadow);
                LabelSet("AlgorithmLabel", this.algorithmLabel, algorithmLabelParams, noShadow);
                LabelSet("AlgorithmLabel0", this.algorithmLabel0, algorithmLabel0Params, noShadow);
                LabelSet("AlgorithmLabel1", this.algorithmLabel1, algorithmLabel1Params, noShadow);
                LabelSet("AlgorithmLabel2", this.algorithmLabel2, algorithmLabel2Params, noShadow);
                ListBoxSet("ComaListBox", this.comaListBox, comaListBoxParams);
                LabelSet("ComaAlgorithmLabel", null, comaAlgorithmLabelParams, noShadow);
                PolygonButtonSet("ComaAlgorithmButton0", null, comaAlgorithmButton0Params, noShadow);
                PolygonButtonSet("ComaAlgorithmButton1", null, comaAlgorithmButton1Params, noShadow);
                PolygonButtonSet("ComaAlgorithmButton2", null, comaAlgorithmButton2Params, noShadow);
            }
            


            

            //GroupButtonsSet("GroupButton", groupButtonParams);
            //PolygonButtonSet("GroupButton", this.sampleGroup, groupButtonParams);
            //}
            //else if (nowMatching)
            //{
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams, shadow);
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
            SetMode(group);//Groupモードへ
            memberButtonPush = true;
            Storyboard enter = (Storyboard)this.FindResource("MemberButtonMouseEnter");
            enter.Stop();
            Storyboard click = (Storyboard)this.FindResource("MemberButtonClick");
            click.Begin();
            //PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);

            ListBoxSet("GroupButtons", this.groupButtons, groupButtonsParams);
            PolygonButtonSet("GroupButton", null, groupButtonParams, noShadow);
            ListBoxSet("MemberButtons", this.memberButtons, memberButtonsParams);
            
            MakeButtonsVisible();
            GroupsSetToListBox();
            //GroupFontSizeSet();

            this.talkLabel.Content = "所属を選択するコマ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
        }
        private void memberButtonClickCompleted(object sender, EventArgs e)
        {   //MemberButtonClidkストーリーボードが終了したときに発生するイベント
            memberButtonPush = false;
            //ストーリーボード実行中にウィンドウサイズが変わった時にはまだサイズ変更を反映するのでストーリーボード終了時にSetModeする
            
            //ストーリーボード実行中にウィンドウサイズが変わった時，出てきたメニューボタンのサイズが反映できないので一度再描写しておく←隣接モードのコントロールの更新はし続けることで解決？
        }

        bool isGroupSelected = false;
        Button preSelectedGroup = new Button();//選択済みのボタン(1つのみ保持)
        private void Group_Click(object sender, RoutedEventArgs e)
        {
            //senderに押されたボタンが格納される
            Button button = (Button)sender;
            //var o = (string)((Button)sender).Content;
            //MessageBox.Show(o);

            //色の変更処理//
            if (!isGroupSelected)
            {//どのボタンも押していない
                isGroupSelected = true;
                //ButtonDarken(button);
                button.Background = Brushes.White;
                //button.Foreground = Brushes.Black;
                preSelectedGroup = button;
                //this.groupDeleteButton.Visibility = Visibility.Visible;
                //this.groupOpenButton.Visibility = Visibility.Visible;
            }
            else
            {//いずれかのボタンを既に選択している
                string name = preSelectedGroup.Content.ToString();
                if (name == "部内")
                {
                    preSelectedGroup.Background = Brushes.LightBlue;
                }
                else if (name == "所属ナシ")
                {
                    preSelectedGroup.Background = Brushes.Pink;
                }
                else
                {
                    preSelectedGroup.Background = Brushes.LightGreen;
                }
                //ButtonDarken(button);//同じボタンを選択した際は打ち消される
                button.Background = Brushes.White;
                //preSelectedGroup.Foreground = new SolidColorBrush(Color.FromRgb(96, 96, 96));
                //button.Foreground = Brushes.Black;  
                preSelectedGroup = button;
            }

            if (nowGroup)
            {
                this.groupDeleteButton.Visibility = Visibility.Visible;
                this.groupOpenButton.Visibility = Visibility.Visible;
                this.groupAddButton.Content = "追\n加";
                groupAddButtonParams = groupAddButtonParams2;
                PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams, noShadow);//形が変わるのでテンプレート更新
                this.groupNameChangeButton.Visibility = Visibility.Visible;//3つのGroup操作ボタンを出現させる
            }
            else if (nowMatching)
            {
                if(participants[coma - 1] == null)
                {
                    participants[coma - 1] = new List<string>();
                }
                this.participantMemberButtons.Visibility = Visibility.Visible;
                MembersSetToListBox(preSelectedGroup.Content.ToString());

                PolygonButtonSet("ParticipantMemberButton", null, participantMemberButtonParams, noShadow);
                LabelSet("ParticipantRankLabel", null, participantRankLabelParams, noShadow);
            }
        }
        ContextMenu c = new ContextMenu();
        private void Group_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button != preSelectedGroup)
            {//選択していないボタンの場合
                ButtonBrighten(button);
            }
            if (nowGroup)
            {//マッチングモード以外ではコンテキストメニューを表示しない(MouseLeaveするまでnullにかえておく)
                c = button.ContextMenu;
                button.ContextMenu = null;
            }
        }
        private void Group_MouseLeave(object sender, RoutedEventArgs e)
        {//選択されていなければボタンの色を戻す
            Button button = (Button)sender;
            if(button != preSelectedGroup)
            {
                string name = button.Content.ToString();
                if (name == "部内")
                {
                    button.Background = Brushes.LightBlue;
                }
                else if (name == "所属ナシ")
                {
                    button.Background = Brushes.Pink;
                }
                else
                {
                    button.Background = Brushes.LightGreen;
                }
            }
            if (nowGroup)
            {
                button.ContextMenu = c;
            }
        }

        private void groupButton_Context1_Click(object sender, RoutedEventArgs e)
        {//そのグループ内の全メンバを一括選択
            List<string[]> mems = flogic.MembersOfGroup(((MenuItem)sender).Tag.ToString());
            if (coma == 1)
            {//1コマ目
                foreach (string[] mem in mems)
                {
                    if(participants[coma - 1].IndexOf(mem[1]) == -1)
                    {//選択されていないメンバ全てに対してクリック時の処理を行う
                        ParticipantNamesTextBoxControl(mem[1]);
                        //MouseLeave時の色処理を行う
                        MemberLeaveColorFunc(mem[1]);
                    }
                    
                }
            }
            else
            {//2コマ目以降
                foreach (string[] mem in mems)
                {
                    if (participants[coma - 1].IndexOf(mem[1]) == -1)
                    {//選択されていないメンバ全てに対してクリック時の処理を行う
                        ParticipantNamesTextBoxControl(mem[1], coma);
                        //MouseLeave時の色処理を行う
                        MemberLeaveColorFunc(mem[1]);
                    }
                }
            }
        }
        private void groupButton_Context2_Click(object sender, RoutedEventArgs e)
        {//そのグループ内の全メンバを一括選択解除
            List<string[]> mems = flogic.MembersOfGroup(((MenuItem)sender).Tag.ToString());
            if (coma == 1)
            {//1コマ目
                foreach (string[] mem in mems)
                {
                    if (participants[coma - 1].IndexOf(mem[1]) != -1)
                    {//選択されているメンバ全てに対してクリック時の処理を行う
                        ParticipantNamesTextBoxControl(mem[1]);
                        //MouseLeave時の色処理を行う
                        MemberLeaveColorFunc(mem[1]);
                    }
                }
            }
            else
            {//2コマ目以降
                foreach (string[] mem in mems)
                {
                    if (participants[coma - 1].IndexOf(mem[1]) != -1)
                    {//選択されていないメンバ全てに対してクリック時の処理を行う
                        ParticipantNamesTextBoxControl(mem[1], coma);
                        //MouseLeave時の色処理を行う
                        MemberLeaveColorFunc(mem[1]);
                    }
                }
            }
        }
        
        private void GroupAdd_Click(object sender, RoutedEventArgs e)
        {
            if (groups.Count >= 100)
            {
                MessageBox.Show("所属が多すぎます．");
            }
            else
            {
                string name = Interaction.InputBox("新しく作成する所属の名前を入力して下さい．");
                name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                if (name.Length > 30)
                {
                    name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                }
                name = flogic.NameDuplicateCheck(groups, name);//重複してるなら連番に
                if (name != "")
                {
                    flogic.AddGroup(name);
                    groups = flogic.AllGroups();
                    GroupsSetToListBox();
                }
            }
        }
        private void GroupNameChange_Click(object sender, RoutedEventArgs e)
        {
            string pre = preSelectedGroup.Content.ToString();
            bool cont = true;
            if (pre == "部内" || pre.ToString() == "所属ナシ")
            {
                MessageBox.Show("その所属の名前は変更できません．");
                cont = false;
            }
            if (cont)
            {
                string name = Interaction.InputBox("変更後の所属の名前を入力して下さい．", "Comarenkun", pre);
                name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                if (name.Length > 30)
                {
                    name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                }
                if (name != "")
                {
                    if (groups.IndexOf(name) == -1)
                    {//ユニークな所属名
                        flogic.ChangeGroup(pre, name);
                        groups = flogic.AllGroups();
                        GroupsSetToListBox();
                    }
                    else
                    {//既に同盟の所属がある
                        for (int i = 2; true; i++)
                        {
                            if (groups.IndexOf(name + i.ToString()) == -1)
                            {//既に存在する所属名のときは連番にする
                                flogic.ChangeGroup(pre, name + i.ToString());
                                groups = flogic.AllGroups();
                                GroupsSetToListBox();
                                break;
                            }
                            if (i > 30)
                            {//連番は30まで　知らんけど
                                MessageBox.Show("同名の所属が多すぎます.");
                                break;
                            }
                        }
                    }
                }
                //else if(name != null)
                //{
                //    MessageBox.Show("空文字は入力できません");
                //}
            } 
        }

        private void GroupDelete_Click(object sender, RoutedEventArgs e)
        {
            if(preSelectedGroup.Content.ToString() == "部内" || preSelectedGroup.Content.ToString() == "所属ナシ")
            {
                MessageBox.Show("その所属は削除できません．");
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("この所属を削除しますか?\n(メンバーは所属ナシに移動します．)", "確認", MessageBoxButton.YesNo);

                if (dr == MessageBoxResult.Yes)
                {
                    flogic.DeleteGroup((string)preSelectedGroup.Content);
                    groups = flogic.AllGroups();
                    GroupsSetToListBox();
                    preSelectedGroup = null;
                    isGroupSelected = false;
                    //グループ選択状態がなくなる
                    this.groupDeleteButton.Visibility = Visibility.Hidden;
                    this.groupNameChangeButton.Visibility = Visibility.Hidden;
                    this.groupOpenButton.Visibility = Visibility.Hidden;
                    this.groupAddButton.Content = "追\n加\n＋";
                    groupAddButtonParams = groupAddButtonParamsCopy;
                    PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams, noShadow);//形を戻しておく

                }
                else if (dr == MessageBoxResult.No)
                { }
                else
                { }
            }
            
        }

        private void GroupOpen_Click(object sender, RoutedEventArgs e)
        {
            SetMode(member);//Memberモードへ
            this.groupAddButton.Visibility = Visibility.Hidden;
            this.groupNameChangeButton.Visibility = Visibility.Hidden;
            this.groupDeleteButton.Visibility = Visibility.Hidden;
            this.groupButtons.Visibility = Visibility.Hidden;
            this.groupOpenButton.Visibility = Visibility.Hidden;//この辺はストーリーボードで後々
            this.groupAddButton.Content = "追\n加\n＋";
            groupAddButtonParams = groupAddButtonParamsCopy;
            PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams, noShadow);//形を戻しておく
            string name = preSelectedGroup.Content.ToString();
            if (name == "部内")
            {
                preSelectedGroup.Background = Brushes.LightBlue;
            }
            else if (name == "所属ナシ")
            {
                preSelectedGroup.Background = Brushes.Pink;
            }
            else
            {
                preSelectedGroup.Background = Brushes.LightGreen;
            }

            MakeButtonsVisible();
            string label = "";
            foreach (char c in preSelectedGroup.Content.ToString())
            {
                string cc = c.ToString();
                if (c == 'ー' || c == '-')
                {
                    cc = " |";
                }
                label = label + cc + "\n";
            }
            this.memberGroupLabel.Content = label;//選択している所属名の縦書き
            LabelSet("MemberGroupLabel", this.memberGroupLabel, memberGroupLabelParams, noShadow);
            MembersSetToListBox(preSelectedGroup.Content.ToString());
            PolygonButtonSet("MemberRankButton", null, memberRankButtonParams, noShadow);
            PolygonButtonSet("MemberNameButton", null, memberNameButtonParams, noShadow);
            PolygonButtonSet("MemberDeleteButton", null, memberDeleteButtonParams, noShadow);
            //MemberFontSizeSet();

            this.talkLabel.Content = "変更したい部分を押せば編集\nできるコマ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
            //this.memberButtons.Visibility = Visibility.Visible;
            //選択されているボタンの所属名を渡してその所属に属するメンバー集合をmemberListに格納等諸々する

            //this.rankNameLabel.Visibility = Visibility.Visible;
        



        }

        private void MemberAdd_Click(object sender, RoutedEventArgs e)
        {
            if (members.Count >= 10000)
            {
                MessageBox.Show("メンバーが多すぎます．");
            }
            else
            {
                string rank = "";
                if (preSelectedGroup.Content.ToString() == "部内")
                {
                    rank = Interaction.InputBox("新しく作成するメンバーのランクを入力して下さい．");
                    int rankNum;
                    if (!int.TryParse(rank, out rankNum))
                    {//ランクが整数値ではない
                        MessageBox.Show("ランクは整数値にして下さい．");
                    }
                }
                
                else
                {//ランクが整数値なら進める
                    string name = Interaction.InputBox("新しく作成するメンバーの名前を入力して下さい．");
                    name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                    if (name.Length > 30)
                    {
                        name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                    }
                    name = flogic.NameDuplicateCheck(memberNames, name);//重複しているなら連番にする
                    if (name != "")
                    {
                        flogic.AddMember(rank, name, preSelectedGroup.Content.ToString());//members.txtファイル更新
                        memberNames = flogic.AllMemberNames();//更新
                        MembersSetToListBox(preSelectedGroup.Content.ToString());//members.txtファイル読んで指定のグループ名のメンバーをリストボックスにセット
                    }
                }   
            }
        }
        private void memberSortButton_Click(object sender, RoutedEventArgs e)
        {
            memberList.Sort();
        }
        private void memberSortButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            ButtonBrighten((Button)sender);
        }
        private void memberSortButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            ButtonBrighten((Button)sender);
            ButtonDarken((Button)sender);
        }

        private void MemberRank_Click(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Tag.ToString();
            //ランクを上げる
            flogic.MinusRank(name, preSelectedGroup.Content.ToString());
            if(preSelectedGroup.Content.ToString() == "部内")
            {
                string rank = ((Button)sender).Content.ToString();
                int minus = int.Parse(rank) - 1;
                if(minus < 0)
                {
                    minus = 0;
                }
                memberList.Find(name).Rank = minus.ToString();
                //((Button)sender).Content = minus.ToString();
            }
            else
            {
                string rank = ((Button) sender).Content.ToString();
                if(rank == "")
                {
                    rank = "↑";
                }else if(rank == "↑")
                {
                    rank = "↓";
                }
                else
                {
                    rank = "";
                }
                memberList.Find(name).Rank = rank.ToString();
            }
            //MembersSetToListBox(preSelectedGroup.Content.ToString());表示に反映はしない
        }
        private void MemberRank_RightClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Tag.ToString();
            //ランクを下げる
            flogic.PlusRank(name, preSelectedGroup.Content.ToString());
            if (preSelectedGroup.Content.ToString() == "部内")
            {
                string rank = ((Button)sender).Content.ToString();
                int plus = int.Parse(rank) + 1;
                if (plus > 1000)
                {
                    plus = 1000;
                }
                memberList.Find(name).Rank = plus.ToString();
                //((Button)sender).Content = plus.ToString();
            }
            else
            {
                string rank = ((Button)sender).Content.ToString();
                if (rank == "")
                {
                    rank = "↓";
                }
                else if (rank == "↑")
                {
                    rank = "";
                }
                else
                {
                    rank = "↑";
                }
                memberList.Find(name).Rank = rank.ToString();
            }
            //MembersSetToListBox(preSelectedGroup.Content.ToString());表示に反映はしない
        }
        private void MemberName_Click(object sender, RoutedEventArgs e)
        {
            if (nowMember)
            {//メンバ編集時
                string preName = ((Button)sender).Content.ToString();
                string preRank = memberList.Rank(preName);//mamberListにはcurrentMembresToShowがはいってる
                string name = Interaction.InputBox("変更後のメンバーの名前を入力して下さい．", "Comarenkun", preName);
                if (name == "")
                {
                    //キャンセル
                }
                else
                {
                    string rank;
                    if (preSelectedGroup.Content.ToString() == "部内")
                    {//部内者はランクの変更も受け付ける
                        rank = Interaction.InputBox("変更後のランクを入力して下さい．\nランクの変更が不要なら空文字を入力もしくはキャンセルして下さい．", "Comarenkun", preRank);
                        if (rank == "")
                        {
                            rank = preRank;
                        }
                        int rankNum;
                        if (!int.TryParse(rank, out rankNum))
                        {//ランクが整数値ではない
                            MessageBox.Show("ランクは整数値にして下さい．");
                        }
                        else
                        {
                            name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                            if (name.Length > 30)
                            {
                                name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                            }
                            name = flogic.NameDuplicateCheck(memberNames, name);//重複しているなら連番にする
                            if (name == "")
                            {
                                name = preName;
                            }

                            flogic.ChangeMember(rank, preName, name, preSelectedGroup.Content.ToString());
                            memberNames = flogic.AllMemberNames();
                            MembersSetToListBox(preSelectedGroup.Content.ToString());
                        }

                    }
                    else
                    {//foreigner
                        name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                        if (name.Length > 30)
                        {
                            name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                        }
                        name = flogic.NameDuplicateCheck(memberNames, name);//重複しているなら連番にする
                        if (name == "")
                        {
                            name = preName;
                        }

                        flogic.ChangeMember(preRank, preName, name, preSelectedGroup.Content.ToString());
                        memberNames = flogic.AllGroups();
                        MembersSetToListBox(preSelectedGroup.Content.ToString());
                    }

                }
            }else if (nowMatching)
            {//メンバ選択時
                if (coma == 1)
                {//1コマ目
                    ParticipantNamesTextBoxControl((string)((Button)sender).Content);
                }
                else
                {//2コマ目以降
                    ParticipantNamesTextBoxControl((string)((Button)sender).Content, coma);
                }
            }

        }
        public void ParticipantNamesTextBoxControl(string name)//, Button button)
        {
            for(int i = 1; i < int.Parse(configs[1]); i++)
            {//メンバに変更があった場合それ以降のコマの参加者情報はリセット
                participants[i].Clear();
                increase[i].Clear();
                decrease[i].Clear();
            }
            if (participants[coma - 1].IndexOf(name) == -1)
            {//まだ選ばれていない場合→選択
                participants[coma - 1].Add(name);
                
                if (this.participantNamesTextBox.Text.EndsWith("："))
                {
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + name;
                }
                else
                {
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + "/ " + name;
                }
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                //ClickはMouseEnter中に完結するためボタンの色が明るくならないので，明るくなった色にしておく(MouseLeaveでもとに戻る)
                MemberSetColor(name, Brighten(Color.FromRgb(255, 55, 100)));
            }
            else
            {//選択済み→キャンセル
                participants[coma -1].Remove(name);
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("/ " + name, "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace(name + "/ ", "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace(name, "");
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                MemberSetColor(name, Brighten(Color.FromRgb(255, 128, 34)));
            }
        }
        public void ParticipantNamesTextBoxControl(string name, int coma)//, Button button, int coma)
        {
            for (int i = coma; i < int.Parse(configs[1]); i++)
            {//メンバに変更があった場合それ以降のコマの参加者情報はリセット
                participants[i].Clear();
                increase[i].Clear();
                decrease[i].Clear();
            }
            if (participants[coma - 2].IndexOf(name) == -1 && increase[coma -1].IndexOf(name) == -1)
            {//前回参加しておらず，まだ選択していない場合→選択し，participantsとincreaseを更新
                participants[coma - 1].Add(name);
                increase[coma - 1].Add(name);

                if (this.participantNamesTextBox.Text.EndsWith("："))
                {//increaseなので＋をつける
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + "＋" + name;
                }
                else
                {
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + "/ " + "＋" + name;
                }
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                //ClickはMouseEnter中に完結するためボタンの色が明るくならないので，明るくなった色にしておく(MouseLeaveでもとに戻る)
                MemberSetColor(name, Brighten(Color.FromRgb(255, 55, 100)));
            }
            else if(participants[coma - 2].IndexOf(name) != -1 && decrease[coma - 1].IndexOf(name) == -1)
            {//前回参加しており，まだ選択していない場合→選択し，participantsとdecreaseを更新
                participants[coma - 1].Remove(name);
                decrease[coma - 1].Add(name);

                if (this.participantNamesTextBox.Text.EndsWith("："))
                {//decreaseなので―をつける
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + "ー" + name;
                }
                else
                {
                    this.participantNamesTextBox.Text = this.participantNamesTextBox.Text + "/ " + "ー" + name;
                }
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                //ClickはMouseEnter中に完結するためボタンの色が明るくならないので，明るくなった色にしておく(MouseLeaveでもとに戻る)
                MemberSetColor(name, Brighten(Color.FromRgb(10, 10, 255)));
            }
            else if(participants[coma-2].IndexOf(name) == -1 && increase[coma - 1].IndexOf(name) != -1)
            {//前回参加しておらず，選択済み→＋をキャンセル
                participants[coma - 1].Remove(name);
                increase[coma - 1].Remove(name);
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("/ " + "＋" + name, "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("＋" + name + "/ ", "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("＋" + name, "");
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                MemberSetColor(name, Brighten(Color.FromRgb(255, 128, 34)));
            }
            else
            {//前回参加しており，選択済み→ーをキャンセル
                participants[coma - 1].Add(name);
                decrease[coma - 1].Remove(name);
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("/ " + "ー" + name, "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("ー" + name + "/ ", "");
                this.participantNamesTextBox.Text = this.participantNamesTextBox.Text.Replace("ー" + name, "");
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                MemberSetColor(name, Brighten(Color.FromRgb(100, 10, 200)));
            }
        }
        public void ParticipantNamesTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {//横方向にスクロールしたい
            ScrollViewer scrollviewer = (ScrollViewer)sender;
            if (e.Delta > 0)
            {
                scrollviewer.LineLeft();
            }
            else
            {
                scrollviewer.LineRight();
            }
            e.Handled = true;
        }
        private void MemberDelete_Click(object sender, RoutedEventArgs e)
        {
            if (currentMembersToShow.Count == 1)
            {
                MessageBox.Show("メンバーを0名にはできません．");
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("このメンバーを削除しますか?", "確認", MessageBoxButton.YesNo);

                if (dr == MessageBoxResult.Yes)
                {
                    flogic.DeleteMember(((Button)sender).Tag.ToString(), preSelectedGroup.Content.ToString());
                    memberNames = flogic.AllMemberNames();
                    MembersSetToListBox(preSelectedGroup.Content.ToString());
                }
                else if (dr == MessageBoxResult.No)
                { }
                else
                { }
            }
        }
        private void Member_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (nowMember)
            {
                ButtonBrighten(button);
            }
            else
            {
                MemberSetColor((string)button.Content, Brighten(((SolidColorBrush)button.Background).Color));
            }
        }
        private void Member_MouseLeave(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string name = button.Content.ToString();
            if (nowMember)
            {
                if (name == "削除")
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
                else
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(255, 128, 34));
                }
            }
            else
            {
                MemberLeaveColorFunc(name);
                
            }
            
        }
        public void MemberLeaveColorFunc(string name)
        {//メンバ―ボタンMouseLeave時の色処理
            if (coma > 1 && participants[coma - 2].IndexOf(name) != -1 && decrease[coma - 1].IndexOf(name) == -1)
            {//前のコマで参加しており，現在ーに選択されていない
                MemberSetColor(name, (Color.FromRgb(100, 10, 200)));
            }
            else if (coma > 1 && participants[coma - 2].IndexOf(name) != -1 && decrease[coma - 1].IndexOf(name) != -1)
            {//前のコマで参加しており，現在ーに選択されている
                MemberSetColor(name, (Color.FromRgb(10, 10, 255)));
            }
            else if (participants[coma - 1].IndexOf(name) != -1)
            {//1コマ目および前のコマで参加していない場合で，選択されている
                MemberSetColor(name, (Color.FromRgb(255, 55, 100)));
            }
            else
            {
                MemberSetColor(name, (Color.FromRgb(255, 128, 34)));
            }
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
            SetMode(matching);
            matchingButtonPush = true;
            Storyboard enter = (Storyboard)this.FindResource("MatchingButtonMouseEnter");
            enter.Stop();
            Storyboard click = (Storyboard)this.FindResource("MatchingButtonClick");
            click.Begin();

            this.talkLabel.Content = "右上のボタンから組み方など\n設定できるコマ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
            //PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            ListBoxSet("GroupButtons", this.groupButtons, participantGroupButtonsParams);
            PolygonButtonSet("GroupButton", null, participantGroupButtonParams, noShadow);
            //GroupFontSizeSet();
            ListBoxSet("MemberButtons", this.participantMemberButtons, participantMemberButtonsParams);
            GroupsSetToListBox();

            coma = 1;
            if(int.Parse(configs[1]) > 1)
            {//コマ数が2以上なら2コマ目へボタン
                this.nextButton.Content = (coma + 1).ToString() + "\nコ\nマ\n目\n→";
            }
            else
            {//コマ数が1
                this.nextButton.Content = "確\n認\n→";
            }
            PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
            //string[] pt = (this.participantNamesTextBox.Text).Split(':');
            //this.participantNamesTextBox.Text = coma.ToString() + "コマ目：" + pt[1];
            this.participantNamesTextBox.Text = ParticipantNameTextBoxText(coma);
            TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);

            MakeButtonsVisible();


        }
        private void matchingButtonClickCompleted(object sender, EventArgs e)
        {   //MatchingButtonClidkストーリーボードが終了したときに発生するイベント
            matchingButtonPush = false;
        }

        private void nextButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
        }
        private void nextButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
            ButtonDarken(button);
        }
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if(coma < int.Parse(configs[1]))
            {
                coma++;
            }



            if ((string)this.nextButton.Content == "確\n認\n→")
            {//まとめ
                this.participantMemberButtons.Visibility = Visibility.Hidden;
                this.participantNamesTextBox.Text = ParticipantNameTextBoxText();//マッチングするメンバ一覧
                TextBoxSet("ParticipantNamesSumTextBox", this.participantNamesTextBox, ParticipantNamesSumTextBoxParams);

                this.groupButtons.Visibility = Visibility.Hidden;
                this.talkLabel.Content = "このメンバーでマッチングして\nいいコマか？";
                TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                this.nextButton.Content = "G\nO\n!";//GOボタンを別にellipseで作るかも(その場合は↓を移植)
            }
            else if ((string)this.nextButton.Content == "G\nO\n!" || (string)this.nextButton.Content == "オ\nカ\nワ\nリ")
            {//ParticipantNamesTextBoxのContentに組み合わせ結果を記述する．
                string result = mlogic.Matching(participants, increase, decrease, configs);//マッチングする
                if (result != "ERROR")
                {
                    this.participantNamesTextBox.Text = result;//マッチングする
                    TextBoxSet("ParticipantNamesSumTextBox", this.participantNamesTextBox, ParticipantNamesSumTextBoxParams);
                    flogic.SendToLINE(result);//LINEに送信

                    this.groupButtons.Visibility = Visibility.Hidden;
                    this.talkLabel.Content = "マッチングしたコマ！";
                    TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                    this.nextButton.Content = "オ\nカ\nワ\nリ";
                    PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);

                    MessageBox.Show(result);
                }
                else
                {
                    /*this.participantNamesTextBox.Text = ParticipantNameTextBoxText();//マッチングする
                    TextBoxSet("ParticipantNamesSumTextBox", this.participantNamesTextBox, ParticipantNamesSumTextBoxParams);

                    this.groupButtons.Visibility = Visibility.Hidden;
                    this.talkLabel.Content = "このメンバーでマッチングして\nいいコマか？";
                    TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                    this.nextButton.Content = "G\nO\n!";//GOボタンを別にellipseで作るかも*/
                }
            }
            else if (configs[1] != coma.ToString())
            {
                MakeButtonsVisible();
                this.configButton.Visibility = Visibility.Hidden;
                this.participantMemberButtons.Visibility = Visibility.Hidden;
                this.participantNamesTextBox.Text = ParticipantNameTextBoxText(coma);
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                this.talkLabel.Content = "途中参加者(赤)と途中脱退者(青)を\n選ぶコマ";
                TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);

                this.nextButton.Content = (coma + 1).ToString() + "\nコ\nマ\n目\n→";
                foreach (string s in participants[coma - 2])
                {
                    participants[coma - 1].Add(s);
                }

            }
            else
            {
                this.participantMemberButtons.Visibility = Visibility.Hidden;
                this.participantNamesTextBox.Text = ParticipantNameTextBoxText(coma);
                TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                this.nextButton.Content = "確\n認\n→";
                foreach (string s in participants[coma - 2])
                {
                    participants[coma - 1].Add(s);
                }
            }


            PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
            isGroupSelected = false;
            preSelectedGroup = null;
            ListBoxSet("GroupButtons", this.groupButtons, participantGroupButtonsParams);
            PolygonButtonSet("GroupButton", null, participantGroupButtonParams, noShadow);
            ListBoxSet("MemberButtons", this.participantMemberButtons, participantMemberButtonsParams);
            GroupsSetToListBox();

            //PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
            


        }

        private void configButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
        }
        private void configButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
            ButtonDarken(button);//Brighten2回分
        }
        private void configButton_Click(object sender, RoutedEventArgs e)
        {
            SetMode(config);

            this.configButton.Visibility = Visibility.Hidden;
            this.groupButtons.Visibility = Visibility.Hidden;
            this.participantMemberButtons.Visibility = Visibility.Hidden;
            this.nextButton.Visibility = Visibility.Hidden;
            this.participantNamesScrollViewer.Visibility = Visibility.Hidden;

            configs = flogic.ReadConfigFile();//configsを更新
            this.tableButton.Content = "台数：" + configs[0];
            this.comaButton.Content = "コマ数：" + configs[1];
            PolygonButtonSet("TableButton", this.tableButton, tableButtonParams, noShadow);
            PolygonButtonSet("ComaButton", this.comaButton, comaButtonParams, noShadow);
            ComaSetToListBox();//LisstBoxを更新
            MakeButtonsVisible();
        }
        private void AlgorithmEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
        }
        private void AlgorithmLeave(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ButtonBrighten(button);
            ButtonDarken(button);//Brighten2回分
        }
        private void AlgorithmClick(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;
            flogic.SetAlgorithm(tag);//config.txtに反映
            configs = flogic.ReadConfigFile();//config.txtを再読み込み
            comaList.Change(tag);//プロパティを変更しリアルタイムに反映
        }
        private void TableMouseEnter(object sender, RoutedEventArgs e)
        {
            ButtonBrighten((Button)sender);
        }
        private void TableMouseLeave(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            ButtonBrighten(b);
            ButtonDarken(b);
        }
        private void TableClick(object sender, RoutedEventArgs e)
        {//1増やす
            flogic.PlusTable();
            configs = flogic.ReadConfigFile();
            string table = int.Parse(configs[0]).ToString();
            ((Button)sender).Content = "台数：" + table;
            
        }
        private void TableRightClick(object sender, RoutedEventArgs e)
        {//1減らす
            flogic.MinusTable();
            configs = flogic.ReadConfigFile();
            string table = int.Parse(configs[0]).ToString();
            ((Button)sender).Content = "台数：" + table;
        }
        private void ComaMouseEnter(object sender, RoutedEventArgs e)
        {
            ButtonBrighten((Button)sender);
        }
        private void ComaMouseLeave(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            ButtonBrighten(b);
            ButtonDarken(b);
        }
        private void ComaClick(object sender, RoutedEventArgs e)
        {//コマ数を増やし，ListBoxにも反映する
            flogic.PlusComa();
            configs = flogic.ReadConfigFile();
            string coma = int.Parse(configs[1]).ToString();
            ((Button)sender).Content = "コマ数：" + coma;
            ComaSetToListBox();

            participants.Add(new List<string>());
            increase.Add(new List<string>());
            decrease.Add(new List<string>());
        }
        private void ComaRightClick(object sender, RoutedEventArgs e)
        {//コマ数を減らし，ListBoxにも反映する
            flogic.MinusComa();
            configs = flogic.ReadConfigFile();
            string coma = int.Parse(configs[1]).ToString();
            ((Button)sender).Content = "コマ数：" + coma;
            ComaSetToListBox();

            participants.RemoveAt(participants.Count - 1);
            increase.RemoveAt(increase.Count - 1);
            decrease.RemoveAt(decrease.Count - 1);
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

        public string ParticipantNameTextBoxText(int c)
        {
            string p = c.ToString() + "コマ目：";
            // increase/decreaseから参加者をつくる
            int ii = 0;
            if (increase[c - 1] != null)
            {
                foreach (string s in increase[c - 1])
                {
                    string ss;
                    if (c == 1)
                    {
                        ss = s;
                    }
                    else
                    {
                        ss = "＋" + s;
                    }

                    if (ii == 0)
                    {
                        p = p + ss;
                        ii++;
                    }
                    else
                    {
                        p = p + "/ " + ss;
                    }

                }
            }
            if (decrease[coma - 1] != null)
            {
                foreach (string s in decrease[c - 1])
                {//1コマ目にはdecreaseはないので無視
                    if (c > 1)
                    {

                        string ss = "ー" + s;


                        if (ii == 0)
                        {
                            p = p + ss;
                            ii++;
                        }
                        else
                        {
                            p = p + "/ " + ss;
                        }
                    }

                }
            }

            return p;
        }
        public string ParticipantNameTextBoxText()
        {//確認画面で総まとめ
            string result = "台数：" + configs[0] + "台\n\n";
            for (int i = 1; i <= int.Parse(configs[1]); i++)
            {//再帰は改行などややこいのでループで
                string alg = "";
                if (configs[i + 1] == "0")
                {
                    alg = "ランクのトオサ優先";
                }
                else if (configs[i + 1] == "1")
                {
                    alg = "ランクのチカサ優先";
                }
                else
                {
                    alg = "ランダム";
                }
                result = result + ParticipantNameTextBoxText(i) + "\nアルゴリズム：" + alg + "\n\n";
            }
            return result;
        }

        private void toMenuButton_Click(object sender, RoutedEventArgs e)
        {//1階層モードを戻る
            if (nowGroup || nowMatching && coma == 1)
            {
                SetMode(menu);
                this.groupAddButton.Visibility = Visibility.Hidden;
                this.groupNameChangeButton.Visibility = Visibility.Hidden;
                this.groupDeleteButton.Visibility = Visibility.Hidden;
                this.groupOpenButton.Visibility = Visibility.Hidden;
                this.groupButtons.Visibility = Visibility.Hidden;//この辺はストーリーボードで後々
                this.groupAddButton.Content = "追\n加\n＋";
                this.configButton.Visibility = Visibility.Hidden;
                this.participantMemberButtons.Visibility = Visibility.Hidden;
                this.nextButton.Visibility = Visibility.Hidden;
                this.participantNamesScrollViewer.Visibility = Visibility.Hidden;
                groupAddButtonParams = groupAddButtonParamsCopy;
                PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams, noShadow);//形を戻しておく

                MakeButtonsVisible();

                toMenuButtonPush = true;
                memberButtonPush = false;
                matchingButtonPush = false;
                Storyboard enter = (Storyboard)this.FindResource("ToMenuButtonMouseEnter");
                enter.Stop();

                Storyboard click = (Storyboard)this.FindResource("ToMenuButtonClick");
                click.Begin();
                this.talkLabel.Content = "Hello,Comarenkun";
                TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
            } else if (nowMember)
            {
                SetMode(group);
                MakeButtonsVisible();
                this.memberAddButton.Visibility = Visibility.Hidden;
                this.memberGroupLabel.Visibility = Visibility.Hidden;
                this.memberButtons.Visibility = Visibility.Hidden;
                this.rankNameLabel.Visibility = Visibility.Hidden;
                this.memberSortButton.Visibility = Visibility.Hidden;
                preSelectedGroup = null;
                isGroupSelected = false;
                //this.groupOpenButton.Visibility = Visibility.Hidden;
                //this.groupButtons.Visibility = Visibility.Hidden;//この辺はストーリーボードで後々
            } else if (nowMatching && coma > 1)
            {
                if((string)this.nextButton.Content != "G\nO\n!" && (string)this.nextButton.Content != "オ\nカ\nワ\nリ")
                {
                    coma--;
                    
                }
                MakeButtonsVisible();
                this.participantMemberButtons.Visibility = Visibility.Hidden;

                for (int i = coma; i < int.Parse(configs[1]); i++)
                {//コマの参加者情報をリセット
                    participants[i].Clear();
                }

                if ((string)this.nextButton.Content == "オ\nカ\nワ\nリ")
                {
                    this.participantNamesTextBox.Text = ParticipantNameTextBoxText();//マッチングする
                    TextBoxSet("ParticipantNamesSumTextBox", this.participantNamesTextBox, ParticipantNamesSumTextBoxParams);

                    this.groupButtons.Visibility = Visibility.Hidden;
                    this.talkLabel.Content = "このメンバーでマッチングして\nいいコマか？";
                    TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                    this.nextButton.Content = "G\nO\n!";//GOボタンを別にellipseで作るかも
                }
                else if (coma == int.Parse(configs[1]))
                {//GO
                    this.nextButton.Content = "確\n認\n→";
                    TextBoxSet("ParticipantNamesTextBox", this.participantNamesTextBox, ParticipantNamesTextBoxParams);
                    this.participantNamesTextBox.Text = ParticipantNameTextBoxText();
                    this.talkLabel.Content = "途中参加者(赤)と途中脱退者(青)を\n選ぶコマ";
                    TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                    this.participantNamesTextBox.Text = ParticipantNameTextBoxText(coma);
                }
                else
                {
                    this.nextButton.Content = (coma + 1).ToString() + "\nコ\nマ\n目\n→";
                    this.participantNamesTextBox.Text = ParticipantNameTextBoxText(coma);
                }
                
                
                if (coma == 1)
                {
                    this.talkLabel.Content = "右上のボタンから組み方など\n設定できるコマ";
                    TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams, noShadow);
                }

                PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
                GroupsSetToListBox();
                preSelectedGroup = null;
                isGroupSelected = false;
            }
            else if (nowConfig)
            {
                SetMode(matching);
                MakeButtonsVisible();
                if (configs[1] == "1")
                {//1コマのみのとき，nextButtonの文字は確認へになる
                    this.nextButton.Content = "確\n認\n→";
                    PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
                }
                else
                {
                    this.nextButton.Content = "2\nコ\nマ\n目\n→";
                    PolygonButtonSet("NextButton", this.nextButton, nextButtonParams, shadow);
                }
                this.tableButton.Visibility = Visibility.Hidden;
                this.comaButton.Visibility = Visibility.Hidden;
                this.algorithmLabel.Visibility = Visibility.Hidden;
                this.algorithmLabel0.Visibility = Visibility.Hidden;
                this.algorithmLabel1.Visibility = Visibility.Hidden;
                this.algorithmLabel2.Visibility = Visibility.Hidden;
                this.comaListBox.Visibility = Visibility.Hidden;

                GroupsSetToListBox();
                preSelectedGroup = null;
                isGroupSelected = false;
            }

        }
        private void toMenuButtonCompleted(object sender, EventArgs e)
        {   //ToMenuButtonClidkストーリーボードが終了したときに発生するイベント
            toMenuButtonPush = false;
        }

        
        public void MakeButtonsVisible()
        {//現在のモードに合わせて，そのモードに表示されるボタンをまとめて表示するメソッド
            //非表示にする分はストーリーボードに任す．
            if (nowMenu)
            {//Menuモードに必要なコントロールをVisibleにする
                this.memberButton.Visibility = Visibility.Visible;
                this.matchingButton.Visibility = Visibility.Visible;
            }else if (nowGroup)
            {//Groupモード(の初期状態)ひ必要なコントロールをVisibleにする
                this.groupButtons.Visibility = Visibility.Visible;
                this.groupAddButton.Visibility = Visibility.Visible;
                this.toMenuButton.Visibility = Visibility.Visible;
            }else if (nowMember)
            {
                this.memberAddButton.Visibility = Visibility.Visible;
                this.memberGroupLabel.Visibility = Visibility.Visible;
                this.memberSortButton.Visibility = Visibility.Visible;
                this.memberButtons.Visibility = Visibility.Visible;
                this.rankNameLabel.Visibility = Visibility.Visible;

                this.toMenuButton.Visibility = Visibility.Visible;
                
            }
            else if (nowMatching)
            {
                this.nextButton.Visibility = Visibility.Visible;
                this.participantNamesScrollViewer.Visibility = Visibility.Visible;
                if (coma == 1)
                {
                    this.configButton.Visibility = Visibility.Visible;
                }
                this.groupButtons.Visibility = Visibility.Visible;

                this.toMenuButton.Visibility = Visibility.Visible;
            }
            else if (nowConfig)
            {
                this.tableButton.Visibility = Visibility.Visible;
                this.comaButton.Visibility = Visibility.Visible;
                this.algorithmLabel.Visibility = Visibility.Visible;
                this.algorithmLabel0.Visibility = Visibility.Visible;
                this.algorithmLabel1.Visibility = Visibility.Visible;
                this.algorithmLabel2.Visibility = Visibility.Visible;
                this.comaListBox.Visibility = Visibility.Visible;

                this.toMenuButton.Visibility = Visibility.Visible;

                PolygonButtonSet("TableButton", this.tableButton, tableButtonParams, noShadow);
                PolygonButtonSet("ComaButton", this.comaButton, comaButtonParams, noShadow);
                LabelSet("AlgorithmLabel", this.algorithmLabel, algorithmLabelParams, noShadow);
                LabelSet("AlgorithmLabel0", this.algorithmLabel0, algorithmLabel0Params, noShadow);
                LabelSet("AlgorithmLabel1", this.algorithmLabel1, algorithmLabel1Params, noShadow);
                LabelSet("AlgorithmLabel2", this.algorithmLabel2, algorithmLabel2Params, noShadow);
                ListBoxSet("ComaListBox", this.comaListBox, comaListBoxParams);
                LabelSet("ComaAlgorithmLabel", null, comaAlgorithmLabelParams, noShadow);
                PolygonButtonSet("ComaAlgorithmButton0", null, comaAlgorithmButton0Params, noShadow);
                PolygonButtonSet("ComaAlgorithmButton1", null, comaAlgorithmButton1Params, noShadow);
                PolygonButtonSet("ComaAlgorithmButton2", null, comaAlgorithmButton2Params, noShadow);
            }
        }
        
    }
   // public event DependencyPropertyChangedEventHandler PropertyChanged = null;
   // protected void OnPropertyChanged(string info)
        //{
            //this.PropertyChanged?.Invoke(this, new DependencyPropertyChangedEventArgs(info));
        //}
}
