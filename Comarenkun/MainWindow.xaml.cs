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
using Matching;
using System.ComponentModel;

namespace Comarenkun
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {       
        public MainWindow()
        {
            InitializeComponent();
            SetMode("Menu");

            //XAMLからこのクラスのメンバ変数が参照可能
            this.DataContext = this;
        }

        int gridRow = 30;//疑似グリッド．列数
        int gridColumn = 30;//疑似グリッド．行数
        public double rowSize = 0;
        public double columnSize = 0;//列，行それぞれ1マス分のサイズ
        double windowWidth = 0;
        double windowHeight = 0;//ウィンドウのサイズ
        PointsGenerater pGen;//ポリゴンやマージンを計算してくれる人
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
        double[] footerParams = { 0, 21-1/15, 30, 9, 0,0, 0,1, 0,0};
        double[] footerBottomParams = { 0, 22.2-1/15, 30 };//footerのテンプレートを利用するので座標のみでよい
        double[] footerAccentParams = { 0, 21.8-1/15, 30, 0.6, 0, 0.2, 0, 1, 0, 0.8 };
        double[] footerCircleParams = { 24, 19, 14, 18 };      
        double[] footerCircleAccentParams = { 24, 20, 13, 17 };
        double[] footerCircleBlackParams = { 24.2, 20.8, 12, 16 };
        double[] memberButtonParams = { 1, 4.75-1.0/15.0, 12, 15, 0, 0, 0, -4.0 / 5.0, 1, 13.0/30.0 };
        double[] matchingButtonParams = { 14, 4.75-15.0/15.0, 14, 15.0+19.0/15.0, 1, 1.0/30.0, 0, -1, 0, 7.0/15.0 };//フッタの傾き1/30
        double[] comarenkunButtonParams = { 24, 19, 10, 10};

        int clickNumber = 0;//クリックのエフェクトに使用
        Storyboard s1 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s2 = new Storyboard();//クリックのエフェクトに使用
        Storyboard s3 = new Storyboard();//クリックのエフェクトに使用
        bool memberButtonPush = false;
        bool matchingButtonPush = false;

        bool nowMenu = false;
        bool nowMember = false;
        bool nowMatching = false;
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
                pGen.ButtonBrighten(this.memberButton);
                enter.Begin();
            }
            else
            {
                enter.Stop();
            }
            
        }
        private void memberButton_MouseLeave(object sender, RoutedEventArgs e)
        {
            if(!memberButtonPush && !matchingButtonPush)
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
            //this.header.Content = "おした";
            //LabelSet("Header", this.header, headerParams);//Contentの変化をバインドしているテンプレートに反映する必要がある
        
        }
        private void matchingButton_MouseEnter(object sender, RoutedEventArgs e)
        {
            Storyboard enter = (Storyboard)this.FindResource("MatchingButtonMouseEnter");
            if (!memberButtonPush && !matchingButtonPush)
            {//ボタンをクリックした際にはエンターイベントは発生させない
                pGen.ButtonBrighten(this.matchingButton);
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
            this.matchingButton.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(0), Convert.ToByte(0), Convert.ToByte(0)));
            Storyboard click = (Storyboard)this.FindResource("MatchingButtonClick");
            click.Begin();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
             // デコードされたビットマップイメージのインスタンスを作る。
            //bitmap.BeginInit();
            //bitmap.UriSource = new Uri("../../Images/comarenkun.png", UriKind.Relative);
            //bitmap.EndInit();
            //comarenkunImage.Source = bitmap;

            windowWidth = ActualWidth;
            windowHeight = ActualHeight;
            rowSize = windowWidth / gridRow;//列幅(マスの横幅)
            columnSize = windowHeight / gridColumn;//行幅(マスの縦幅)          
            pGen = new PointsGenerater(windowWidth, windowHeight, rowSize, columnSize);

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
            if (nowMenu)
            {
                ButtonSet("MemberButton", this.memberButton, memberButtonParams);
                ButtonSet("MatchingButton", this.matchingButton, matchingButtonParams);
            }         

            
        }

        public void PolygonSet(string name, Polygon polygon, double[] p)
        {
            //Polygonオブジェクトのスタイルを設定する
            //スタイルの再利用を考えるとポリゴンのポジションは原点を左上に指定し，スタイル外からマージンにより全体の原点座標を設定する(スタイル内で座標を設定するとそのスタイルを利用する全コントロールが同じ位置になってしまう)
            Style polygonStyle = new Style(typeof(Polygon));
            PointCollection poi = new PointCollection();
            if (name == "InitialBackObject1" || name == "InitialBackObject2")
            {//この2つは五角形
                poi = pGen.PentaPoints(p);
            }
            else
            {
                poi = pGen.PolygonPoints(p);
            }
            Setter points = new Setter(Polygon.PointsProperty, poi);
            Setter mouse = new Setter(Polygon.IsHitTestVisibleProperty, false);
            polygonStyle.Setters.Add(points);
            polygonStyle.Setters.Add(mouse);
            App.Current.Resources[name] = polygonStyle;

            if (name == "BackObject3" || name == "BackObject4" )
            {   //背景の一部だけマージン変化の縮尺を変えて遠近感を出す
                polygon.Margin = new Thickness(columnSize * p[0], rowSize * p[1], 0, 0);
            }
            else 
            {
                polygon.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
            }

        }
        public void EllipseSet(string name, Ellipse circle, double[] p)
        {
            //Ellipceオブジェクトのスタイルを設定する
            //スタイルの再利用を考えるとサークルのポジションは原点を左上に指定し，スタイル外からマージンにより全体の原点座標を設定する(スタイル内で座標を設定するとそのスタイルを利用する全コントロールが同じ位置になってしまう)
            Style circleStyle = new Style(typeof(Ellipse));
            Setter wid = new Setter(Ellipse.WidthProperty, rowSize * p[2]);
            Setter hei = new Setter(Ellipse.HeightProperty, columnSize * p[3]);
            Setter mouse = new Setter(Ellipse.IsHitTestVisibleProperty, false);
            circleStyle.Setters.Add(wid);
            circleStyle.Setters.Add(hei);
            circleStyle.Setters.Add(mouse);
            App.Current.Resources[name] = circleStyle;
            circle.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
        }
        public void LabelSet(string name, Label label, double[] p)//header要素のコントロールテンプレートおよびプロパティ更新メソッド
        {   //文字を持っていたり多角形であったりするラベルのコントロールテンプレートを設定する
            //テンプレートの再利用を考えるとポリゴンのポジションは原点を左上に指定し，テンプレート外からマージンにより全体の原点座標を設定する(テンプレート内で座標を設定するとそのテンプレートを利用する全コントロールが同じ位置になってしまう)
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用

            ControlTemplate labelTemplate = new ControlTemplate(typeof(Label));

            FrameworkElementFactory gri = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory pol = new FrameworkElementFactory(typeof(Polygon));//ポリゴン用
            FrameworkElementFactory cir = new FrameworkElementFactory(typeof(Ellipse));//円用
            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));

            if (name == "Header")//多角形ラベル
            {
                pol = new FrameworkElementFactory(typeof(Polygon));
                TemplateBindingExtension b = new TemplateBindingExtension(Label.BackgroundProperty);
                pol.SetValue(Polygon.FillProperty, b);
                pol.SetValue(Polygon.PointsProperty, pGen.PolygonPoints(p));
            }else//円形ラベル
            {
                cir = new FrameworkElementFactory(typeof(Ellipse));
                TemplateBindingExtension b = new TemplateBindingExtension(Label.BackgroundProperty);
                cir.SetValue(Ellipse.FillProperty, b);
                cir.SetValue(Ellipse.WidthProperty, rowSize * p[2]);
                cir.SetValue(Ellipse.HeightProperty, columnSize * p[3]);             
            }
            
            if(name == "Header")//文字(コンテンツ)を持つラベル
            {
                con = new FrameworkElementFactory(typeof(ContentPresenter));
                con.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);//フォントサイズ＝ヘッダの縦幅の70%
                con.SetValue(ContentPresenter.MarginProperty, pGen.MarginLeftCenter(p));//マージンにより左端中央に配置

                conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);
                
                conShadow.SetValue(ContentPresenter.MarginProperty, pGen.MarginLeftCenterS(p));//マージンにより左端中央に配置 
            }        
            
            //Grid構築
            if(name == "Header")
            {
                gri.AppendChild(pol);
                gri.AppendChild(conShadow);
                gri.AppendChild(con);
            }else if(false)
            {
                gri.AppendChild(pol);
            }else if(false)
            {
                gri.AppendChild(cir);
            }
            
            //ビジュアルツリーに登録
            labelTemplate.VisualTree = gri;

            //コントロールテンプレート更新
            App.Current.Resources[name] = labelTemplate;

            label.IsHitTestVisible = false;//マウス判定を消す
            label.Margin = new Thickness(rowSize * p[0], columnSize * p[1], -windowWidth + rowSize * (p[0] + p[2]), -windowHeight + columnSize * (p[1] + p[3]));//Ellipseに対しては右と下のマージンを削らなくてよい？

        }

        public void ButtonSet(string name, Button button, double[] p)//Button要素のコントロールテンプレートおよびプロパティ更新メソッド
        {
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用
            double contentSize = button.Content.ToString().Length;

            double angle;
            if (name == "MemberButton")
            {
                angle = 5;
            } else if (name == "MatchingButton")
            {
                angle = -4;//だいたい文字数の逆比
            }
            else
            {
                angle = 0;
            }

            //Style buttonStyle = new Style(typeof(Button));
            ControlTemplate buttonTemplate = new ControlTemplate(typeof(Button));

            RotateTransform rotateTransform1 = new RotateTransform(angle);
            rotateTransform1.CenterX = (p[0] + (p[0] + p[2])) / 2.0;
            rotateTransform1.CenterY = (p[1] + (p[1] + p[3])) / 2.0;

            FrameworkElementFactory gri = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory pol = new FrameworkElementFactory(typeof(Polygon));
            TemplateBindingExtension b = new TemplateBindingExtension(Button.BackgroundProperty);
            pol.SetValue(Polygon.FillProperty, b);//pol要素のFillプロパティをbindingでバインドしますの意
            pol.SetValue(Polygon.PointsProperty, pGen.PolygonPoints(p));
            pol.SetValue(Polygon.CursorProperty, Cursors.Hand);
            //pol.SetValue(Polygon.NameProperty, "memberPolygon");//何故かセットされない

            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            con.SetValue(ContentPresenter.MarginProperty, pGen.MarginCenter(p, contentSize));//マージンにより左端中央に配置
            con.SetValue(ContentPresenter.CursorProperty, Cursors.Hand);
            con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
            con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
            conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
            conShadow.SetValue(ContentPresenter.MarginProperty, pGen.MarginCenterS(p, contentSize));//マージンにより左端中央に配置
            conShadow.SetValue(ContentPresenter.CursorProperty, Cursors.Hand);
            conShadow.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
            conShadow.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

            gri.AppendChild(pol);
            gri.AppendChild(conShadow);
            gri.AppendChild(con);

            pol.Name = "buttonPolygon";//この方法だとセットされる

            //Trigger trigger = new Trigger();//マウスオーバー時の挙動
            //trigger.Property = IsMouseOverProperty;
            //trigger.Value = true;
            //Setter buttonBackgroundSetter;
            //SolidColorBrush color = new SolidColorBrush(Color.FromRgb(10, 10, 10));
            //buttonBackgroundSetter = new Setter(Polygon.FillProperty, new SolidColorBrush(pGen.Brighten(((SolidColorBrush)button.Background).Color)), pol.Name);

            //trigger.Setters.Add(buttonBackgroundSetter);
            //buttonTemplate.Triggers.Add(trigger);
            //buttonStyle.Triggers.Add(trigger);
        
            buttonTemplate.VisualTree = gri;
            
            Setter buttonTemplateSetter = new Setter(TemplateProperty, buttonTemplate);
            //buttonStyle.Setters.Add(buttonTemplateSetter);

            App.Current.Resources[name] = buttonTemplate;//コントロールテンプレート更新
            button.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
            button.FontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;//フォントサイズ＝80% * ボタンの横幅/文字数
        }
        public void ComarenkunSet(string name, Button coma, double[] p)
        {
            //コマ練くんの画像ボタンのコントロールのコントロールテンプレートを設定する
            //スタイルの再利用を考えるとポジションは原点を左上に指定し，スタイル外からマージンにより全体の原点座標を設定する(スタイル内で座標を設定するとそのスタイルを利用する全コントロールが同じ位置になってしまう)
            ControlTemplate comaTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory gri = new FrameworkElementFactory(typeof(Grid));
            BitmapImage i = new BitmapImage(new Uri("pack://application:,,,/Comarenkun;component/Images/comarenkun.png", UriKind.Absolute));
            FrameworkElementFactory im = new FrameworkElementFactory(typeof(OpaqueClickableImage));
            im.SetValue(OpaqueClickableImage.SourceProperty, i);
            im.SetValue(OpaqueClickableImage.StretchProperty, Stretch.Fill);

            gri.AppendChild(im);
            comaTemplate.VisualTree = gri;

            App.Current.Resources[name] = comaTemplate;
            coma.Width = rowSize * p[2];
            coma.Height = columnSize * p[3];
            //常に右下が画面右下に来るように特殊に計算
            coma.Margin = new Thickness(rowSize * (gridRow + 3 - p[2]), columnSize * (gridColumn - p[3]), 0, 0);
        }

        public void beginClickEffect(MouseButtonEventArgs e)
        {
            if (clickNumber == 0)
            {
                clickNumber++;
                s1 = ((Storyboard)this.FindResource("ClickEffect")).Clone();
                Ellipse e1 = this.effect1;
                e1.Margin = new Thickness(e.GetPosition(this.grid).X - 10 / 2, e.GetPosition(this.grid).Y - 10 / 2, 0, 0);
                foreach (var child in s1.Children)
                {
                    Storyboard.SetTarget(child, e1);
                }
                s1.Begin();
            }
            else if (clickNumber == 1)
            {
                clickNumber++;
                s2 = ((Storyboard)this.FindResource("ClickEffect")).Clone();
                Ellipse e2 = this.effect2;
                e2.Margin = new Thickness(e.GetPosition(this.grid).X - 10 / 2, e.GetPosition(this.grid).Y - 10 / 2, 0, 0);
                foreach (var child in s2.Children)
                {
                    Storyboard.SetTarget(child, e2);
                }
                s2.Begin();
            }
            else
            {
                clickNumber = 0;
                s3 = ((Storyboard)this.FindResource("ClickEffect")).Clone();
                Ellipse e3 = this.effect3;
                e3.Margin = new Thickness(e.GetPosition(this.grid).X - 10 / 2, e.GetPosition(this.grid).Y - 10 / 2, 0, 0);
                foreach (var child in s3.Children)
                {
                    Storyboard.SetTarget(child, e3);
                }
                s3.Begin();
            }
        }
        public void removeClickEffect()
        {
            if (clickNumber == 0)
            {
                s2.Stop();
            }
            else if (clickNumber == 1)
            {
                s3.Stop();
            }
            else
            {
                s1.Stop();
            }
        }
    }

    public class PointsGenerater//windowのサイズからPolygonなどの各種座標を計算するクラス
    {
        string format = "F2";
        public double Width { set; get; }
        public double Height { set; get; } 
        public double Row { set; get; }
        public double Column { set; get; }
        public PointsGenerater(double Width, double Height, double Row, double Column)//コンストラクタ
        {
            this.Width = Width;
            this.Height = Height;
            this.Row = Row;
            this.Column = Column;
        }

        public PointCollection PentaPoints(double[] p)//Polygonの列幅行幅から,左上を原点とした適当なPoints文字列を返す
        {
            PointCollection result = new PointCollection();
            result.Add(new Point(Row * p[4], Column * (p[3] + p[5])));
            result.Add(new Point(0, 0));
            result.Add(new Point(Row * p[6], Column * p[7]));
            result.Add(new Point(Row * (p[2] + p[8]), Column * p[9]));
            result.Add(new Point(Row * (p[2] + p[10]), Column * (p[3] + p[11])));
            return result;
        }
        public PointCollection PolygonPoints(double[] p)//Polygonの列幅行幅から,左上を原点とした適当なPoints文字列を返す
        {
            //PolygonのPointsは左下左上右上右下の順
            string leftbottom = (Row * p[4]).ToString(format) + "," + (Column * (p[3] + p[5])).ToString(format) + " ";
            string lefttop =  "0,0 ";
            string righttop = (Row * (p[2] + p[6])).ToString(format) + "," + (Column * p[7]).ToString(format) + " ";
            string rightbottom = (Row * (p[2] + p[8])).ToString(format) + "," + (Column * (p[3] + p[9])).ToString(format);
            return PointCollection.Parse(leftbottom + lefttop + righttop + rightbottom);
        }
        public Thickness MarginLeftCenter(double[] p)
        {
            double fontSize = Column * p[3] * 0.7;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Left　のかわり(Gridを使用していないため)

            //Marginは左上右下の順
            double left = Row * 0.5;//Windowの横幅に応じて少しだけ左端を空ける
            double top = Column * p[3] / 2 - fontSize / 2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = - Width + Row * p[2];
            double bottom = - Height + Column * p[3];//親グリッドが画面全体なので右と下の負のマージンがないと画面一杯に範囲が広がってしまう<-IsHitTestVisible=falseで解決はした
            Thickness result = new Thickness(left, top, right, bottom);
            return result; 
        }
        public Thickness MarginLeftCenterS(double[] p)//ラベルなどの影用のちょっとずらしたマージン生成
        {
            double fontSize = Column * p[3] * 0.7;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Left　のかわり(Gridを使用していないため)

            //Marginは左上右下の順
            double left = Row * (0.5 + 0.1);
            double top = Column * p[3] / 2 - fontSize / 2 + Column * 0.2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = - Width + Row * p[2];
            double bottom = - Height + Column * p[3];//右と下のマージンがないと画面一杯に範囲が広がってしまう
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness MarginCenter(double[] p, double contentSize)
        {     
            //横方向のマージンに文字数contentSizeが必要
            double fontSize = Row * p[2] * 1.0 / contentSize * 0.8;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Center　のかわり(Gridを使用していないため)

            double trueWidth, trueHeight;
            if (p.Length > 4)
            {
                //多角形なので平均っぽい縦横の長さを上と左の辺を基準に計算
                trueWidth = p[2] + p[6];
                trueHeight = p[3] + p[5];
            }
            else
            {
                trueWidth = p[2];
                trueHeight = p[3];
            }
            //Marginは左上右下の順
            double left = Row * trueWidth / 2 - fontSize * contentSize / 2;
            double top = Column * trueHeight / 2 - fontSize / 2 - Column;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル),ボタンが右上がりなためy座標1つぶん上に配置
            double right = - Width + Row * p[2];
            double bottom = - Height + Column * p[3];//右と下のマージンがないと画面一杯に範囲が広がってしまう
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness MarginCenterS(double[] p, double contentSize)//ラベルなどの影用のちょっとずらしたマージン生成
        {
            double fontSize = Row * p[2] * 1.0 / contentSize * 0.8;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Left　のかわり(Gridを使用していないため)
        
            double trueWidth, trueHeight;
            if (p.Length > 4)
            {
                //多角形なので平均っぽい縦横の長さを上と左の辺を基準に計算
                trueWidth = p[2] + p[6];
                trueHeight = p[3] + p[5];
            }
            else
            {
                trueWidth = p[2];
                trueHeight = p[3];
            }
            //Marginは左上右下の順
            double left = Row * trueWidth / 2 - fontSize * contentSize / 2 + Row * (0.1);
            double top = Column * trueHeight / 2 - fontSize / 2 + Column * (- 1 + 0.2);//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = - Width + Row * p[2];
            double bottom = - Height + Column * p[3];
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Color Brighten(Color input)//カラーをすこし黄色めに明るくする
        {
            int[] rgb = { input.R, input.G, input.B };
            int r, g, b;
            if(rgb[0] > 255 - 16 * 3)
            {
                r = 255;
            }
            else
            {
                r = rgb[0] + 16 * 3;
            }
            if(rgb[1] > 255 - 16 * 2)
            {
                g = 255;
            }
            else
            {
                g = rgb[1] + 16 * 2;
            }
            if(rgb[2] < 16 * 1)
            {
                b = 0;
            }
            else
            {
                b = rgb[2] - 16 * 1;
            }
            return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b)); 
        }
        public void ButtonBrighten(Button b)
        {//渡されたボタンの色を↑の関数に通した値に変える
            Color currentColor = ((SolidColorBrush)b.Background).Color;
            b.Background = new SolidColorBrush(Brighten(currentColor));
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
