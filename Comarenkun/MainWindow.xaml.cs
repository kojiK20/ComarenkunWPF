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
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MatchingLogic mlogic;//ロジック部を実行してくれる人
        List<string[]> members;
        List<string> memberNames;
        List<string[]> currentMembersToShow;
        List<string> groups;
        public GroupList groupList;
        public MemberList memberList;

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
        double[] groupAddButtonParams = { 25.5, 1.5, 4.5, 20, 1, 0, 0, 0, 0, 0 };//groupNameChangeButtonの表示に合わせてパラメータを変える
        double[] groupAddButtonParams2 = { 26, 11.5, 4.5, 10, 0.5, 0, 0, 0, 0, 0 };//groupNameChangeButtonが表示されてるとき用
        double[] groupAddButtonParamsCopy = { 25.5, 1.5, 4.5, 20, 1, 0, 0, 0, 0, 0 };//↑から戻すとき用
        double[] groupNameChangeButtonParams = { 5.1, 13.5, 6.5, 6.5 };
        double[] groupDeleteButtonParams = { 25.5, 1.5, 4.5, 10, 0.5, 0, 0, 0, 0, 0 };
        double[] groupOpenButtonParams = { 4.2, 4, 7.5, 7.5 };
        double[] groupButtonsParams = { 11, 3.1, 14.5, 19 };
        double[] groupButtonParams = { 0, 0, 13, 3, 1, 0, 0, 0, 0, 0 };//メンバ画面で所属を選択するボタン
        double[] memberAddButtonParams = { 25.5, 1.5, 4.5, 20, 1, 0, 0, 0, 0, 0 };
        double[] memberGroupLabelParams = { 5, 3.75, 3, 17.25, 0, 0, 0, 0, 0, 0 };//メンバ画面で所属名を表示
        double[] memberButtonsParams = { 8, 5, 16.5, 16};//所属選択後にメンバ編集するためのボタン
        double[] memberRankButtonParams = { 0, 0, 2.2, 2, 0, 0, 0, 0, -0.2, 0 };
        double[] memberNameButtonParams = { 2.3, 0, 9.6, 2, -0.2, 0, 0, 0, 0, 0 };
        double[] memberDeleteButtonParams = { 12, 0, 2, 2, 0, 0, 0, 0, 0, 0 };
        double[] rankNameLabelParams = { 8, 3.5, 16.5, 1.5, 0, 0, 0, 0, 0, 0 };//「Rank/Name」
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
        string group = "Group";
        bool nowGroup = false;
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
            memberNames = mlogic.AllMemberNames();
            groups = mlogic.AllGroups();

            //XAMLからこのクラスのメンバ変数を参照可能にする
            this.DataContext = this;
            this.toMenuButton.Visibility = Visibility.Hidden;
            this.toMenuButton.Content = "モ\nド\nル";//行替えエスケープがxamlコードからは利用できない
            this.groupAddButton.Content = "追\n加\n＋";
            this.groupDeleteButton.Content = "削\n除";
            this.memberAddButton.Content = "追\n加\n＋";
            this.talkLabel.Content = "Hello,Comarenkun";

            groupList = new GroupList();
            this.groupButtons.DataContext = groupList.List;
            memberList = new MemberList();
            this.memberButtons.DataContext = memberList.List;
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
                nowGroup = false;
                nowMatching = false;
                nowMember = false;
            }
            else if (mode == "Group")
            {
                nowMenu = false;
                nowGroup = true;
                nowMatching = false;
                nowMember = false;
            }
            else if (mode == "Matching")
            {
                nowMenu = false;
                nowGroup = false;
                nowMatching = true;
                nowMember = false;
            }
            else if(mode == "Member")
            {
                nowMenu = false;
                nowGroup = false;
                nowMatching = false;
                nowMember = true;
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
            //else if (nowGroup)
            //{
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams);    
            PolygonButtonSet("GroupDeleteButton", this.groupDeleteButton, groupDeleteButtonParams);
            EllipseButtonSet("GroupNameChangeButton", this.groupNameChangeButton, groupNameChangeButtonParams);
            EllipseButtonSet("GroupOpenButton", this.groupOpenButton, groupOpenButtonParams);
            ListBoxSet("GroupButtons", this.groupButtons, groupButtonsParams);
            PolygonButtonSet("GroupButton", null, groupButtonParams);
            GroupFontSizeSet();

            PolygonButtonSet("MemberAddButton", this.memberAddButton, memberAddButtonParams);
            LabelSet("MemberGroupLabel", this.memberGroupLabel, memberGroupLabelParams);
            ListBoxSet("MemberButtons", this.memberButtons, memberButtonsParams);
            PolygonButtonSet("MemberRankButton", null, memberRankButtonParams);
            PolygonButtonSet("MemberNameButton", null, memberNameButtonParams);
            PolygonButtonSet("MemberDeleteButton", null, memberDeleteButtonParams);
            MemberFontSizeSet();
            LabelSet("RankNameLabel", this.rankNameLabel, rankNameLabelParams);

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
            SetMode(group);//Groupモードへ
            memberButtonPush = true;
            Storyboard enter = (Storyboard)this.FindResource("MemberButtonMouseEnter");
            enter.Stop();
            Storyboard click = (Storyboard)this.FindResource("MemberButtonClick");
            click.Begin();
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);

            MakeButtonsVisible();
            GroupsSetToListBox();

            this.talkLabel.Content = "所属を選択するコマ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
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

            this.groupDeleteButton.Visibility = Visibility.Visible;
            this.groupOpenButton.Visibility = Visibility.Visible;
            this.groupAddButton.Content = "追\n加";
            groupAddButtonParams = groupAddButtonParams2;
            PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams);//形が変わるのでテンプレート更新
            this.groupNameChangeButton.Visibility = Visibility.Visible;//3つのGroup操作ボタンを出現させる
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
        }
        private void Group_MouseEnter(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button != preSelectedGroup)
            {//選択していないボタンの場合
                ButtonBrighten(button);
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
                name = mlogic.NameDuplicateCheck(groups, name);//重複してるなら連番に
                if (name != "")
                {
                    mlogic.AddGroup(name);
                    groups = mlogic.AllGroups();
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
                        mlogic.ChangeGroup(pre, name);
                        groups = mlogic.AllGroups();
                        GroupsSetToListBox();
                    }
                    else
                    {//既に同盟の所属がある
                        for (int i = 2; true; i++)
                        {
                            if (groups.IndexOf(name + i.ToString()) == -1)
                            {//既に存在する所属名のときは連番にする
                                mlogic.ChangeGroup(pre, name + i.ToString());
                                groups = mlogic.AllGroups();
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
                    mlogic.DeleteGroup((string)preSelectedGroup.Content);
                    groups = mlogic.AllGroups();
                    GroupsSetToListBox();
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
            PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams);//形を戻しておく
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
            foreach(char c in preSelectedGroup.Content.ToString())
            {
                string cc = c.ToString();
                if(c == 'ー' || c == '-')
                {
                    cc = " |";
                }
                label = label + cc + "\n";
            }
            this.memberGroupLabel.Content = label;//選択している所属名の縦書き
            LabelSet("MemberGroupLabel", this.memberGroupLabel, memberGroupLabelParams);
            MembersSetToListBox(preSelectedGroup.Content.ToString());

            this.talkLabel.Content = "変更したい部分を押せば編集\nできるコマ";
            TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
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
                }
                string name = Interaction.InputBox("新しく作成するメンバーの名前を入力して下さい．");
                int rankNum;
                if (!int.TryParse(rank, out rankNum))
                {//ランクが整数値ではない
                    MessageBox.Show("ランクは整数値にして下さい．");
                }
                else
                {//ランクが整数値なら進める
                    name = name.Replace(":", "");//:はファイル処理に使用しているので消す
                    if (name.Length > 30)
                    {
                        name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
                    }
                    name = mlogic.NameDuplicateCheck(memberNames, name);//重複しているなら連番にする
                    if (name != "")
                    {
                        mlogic.AddMember(rank, name, preSelectedGroup.Content.ToString());//members.txtファイル更新
                        memberNames = mlogic.AllMemberNames();//更新
                        MembersSetToListBox(preSelectedGroup.Content.ToString());//members.txtファイル読んで指定のグループ名のメンバーをリストボックスにセット
                    }
                }   
            }
        }

        private void MemberRank_Click(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Tag.ToString();
            //ランクを上げる
            mlogic.MinusRank(name, preSelectedGroup.Content.ToString());
            memberNames = mlogic.AllGroups();
            MembersSetToListBox(preSelectedGroup.Content.ToString());
        }
        private void MemberRank_RightClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Tag.ToString();
            //ランクを下げる
            mlogic.PlusRank(name, preSelectedGroup.Content.ToString());
            memberNames = mlogic.AllGroups();
            MembersSetToListBox(preSelectedGroup.Content.ToString());
        }
        private void MemberName_Click(object sender, RoutedEventArgs e)
        {
            string preName = ((Button)sender).Content.ToString();
            string preRank = memberList.Rank(preName);//mamberListにはcurrentMembresToShowがはいってる
            string name = Interaction.InputBox("変更後のメンバーの名前を入力して下さい．", "Comarenkun", preName);
            string rank = Interaction.InputBox("変更後のランクを入力して下さい．\nランクの変更が不要なら空文字を入力もしくはキャンセルして下さい．", "Comarenkun", preRank);
            if (rank == "")
            {
                rank = preRank;
            }
            name = name.Replace(":", "");//:はファイル処理に使用しているので消す
            if (name.Length > 30)
            {
                name = name.Substring(0, 30);//文字数は上限30とする 知らんけど
            }
            name = mlogic.NameDuplicateCheck(memberNames, name);//重複しているなら連番にする
            if (name == "")
            {
                name = preName;
            }

            mlogic.ChangeMember(rank, preName, name, preSelectedGroup.Content.ToString());
            memberNames = mlogic.AllGroups();
            MembersSetToListBox(preSelectedGroup.Content.ToString());

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
                    mlogic.DeleteMember(((Button)sender).Tag.ToString(), preSelectedGroup.Content.ToString());
                    memberNames = mlogic.AllMemberNames();
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
            ButtonBrighten(button);
        }
        private void Member_MouseLeave(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string name = button.Content.ToString();
            if (name == "削除")
            {
                button.Background = new SolidColorBrush(Color.FromRgb(255,0,0));
            }
            else
            {
                button.Background = new SolidColorBrush(Color.FromRgb(255, 136, 34));
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
            
            PolygonButtonSet("ToMenuButton", this.toMenuButton, toMenuButtonParams);
            this.toMenuButton.Visibility = Visibility.Visible;

            
        }
        private void matchingButtonClickCompleted(object sender, EventArgs e)
        {   //MatchingButtonClidkストーリーボードが終了したときに発生するイベント
            matchingButtonPush = false;
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
        {//1階層モードを戻る
            if (nowGroup || nowMatching)
            {
                SetMode(menu);
                this.groupAddButton.Visibility = Visibility.Hidden;
                this.groupNameChangeButton.Visibility = Visibility.Hidden;
                this.groupDeleteButton.Visibility = Visibility.Hidden;
                this.groupOpenButton.Visibility = Visibility.Hidden;
                this.groupButtons.Visibility = Visibility.Hidden;//この辺はストーリーボードで後々
                this.groupAddButton.Content = "追\n加\n＋";
                groupAddButtonParams = groupAddButtonParamsCopy;
                PolygonButtonSet("GroupAddButton", this.groupAddButton, groupAddButtonParams);//形を戻しておく

                MakeButtonsVisible();

                toMenuButtonPush = true;
                memberButtonPush = false;
                matchingButtonPush = false;
                Storyboard enter = (Storyboard)this.FindResource("ToMenuButtonMouseEnter");
                enter.Stop();

                Storyboard click = (Storyboard)this.FindResource("ToMenuButtonClick");
                click.Begin();
                this.talkLabel.Content = "Hello,Comarenkun";
                TransParentLabelSet("talkLabel", this.talkLabel, talkLabelParams);
            }else if (nowMember)
            {
                SetMode(group);
                MakeButtonsVisible();
                this.memberAddButton.Visibility = Visibility.Hidden;
                this.memberGroupLabel.Visibility = Visibility.Hidden;
                this.memberButtons.Visibility = Visibility.Hidden;
                this.rankNameLabel.Visibility = Visibility.Hidden;
                preSelectedGroup = null;
                isGroupSelected = false;
                //this.groupOpenButton.Visibility = Visibility.Hidden;
                //this.groupButtons.Visibility = Visibility.Hidden;//この辺はストーリーボードで後々
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
                this.memberButtons.Visibility = Visibility.Visible;
                this.rankNameLabel.Visibility = Visibility.Visible;

                this.toMenuButton.Visibility = Visibility.Visible;
                
            }
        }
        
    }
   // public event DependencyPropertyChangedEventHandler PropertyChanged = null;
   // protected void OnPropertyChanged(string info)
        //{
            //this.PropertyChanged?.Invoke(this, new DependencyPropertyChangedEventArgs(info));
        //}
}
