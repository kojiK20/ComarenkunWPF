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

//オブジェクトのイベントに関係なく，オブジェクトのテンプレートやスタイルを設定するメソッドはここにおく
namespace Comarenkun
{
    public partial class MainWindow : Window
    {
        public void UpdateSize()
        {
            windowWidth = ActualWidth;
            windowHeight = ActualHeight;
            rowSize = windowWidth / gridRow;
            columnSize = windowHeight / gridColumn;
        }
        public void PolygonSet(string name, Polygon polygon, double[] p)
        {
            //Polygonオブジェクトのスタイルを設定する
            //スタイルの再利用を考えるとポリゴンのポジションは原点を左上に指定し，スタイル外からマージンにより全体の原点座標を設定する(スタイル内で座標を設定するとそのスタイルを利用する全コントロールが同じ位置になってしまう)
            Style polygonStyle = new Style(typeof(Polygon));
            PointCollection poi = new PointCollection();
            if (name == "InitialBackObject1" || name == "InitialBackObject2")
            {//この2つは五角形
                poi = PentaPoints(p);
            }
            else
            {
                poi = PolygonPoints(p);
            }
            Setter points = new Setter(Polygon.PointsProperty, poi);
            Setter mouse = new Setter(Polygon.IsHitTestVisibleProperty, false);
            polygonStyle.Setters.Add(points);
            polygonStyle.Setters.Add(mouse);
            App.Current.Resources[name] = polygonStyle;

            if (name == "BackObject3" || name == "BackObject4")
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
        public void LabelSet(string name, Label label, double[] p, bool sh)//header要素のコントロールテンプレートおよびプロパティ更新メソッド
        {   //文字を持っていたり多角形であったりするラベルのコントロールテンプレートを設定する
            //テンプレートの再利用を考えるとポリゴンのポジションは原点を左上に指定し，テンプレート外からマージンにより全体の原点座標を設定する(テンプレート内で座標を設定するとそのテンプレートを利用する全コントロールが同じ位置になってしまう)
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用
            double contentSize = 1;

            if (label != null)
            {//ListBoxのItems(nullを渡している)のフォントサイズはこのメソッドでは指定しない(形状は同じだが文字数が異なるため)
                contentSize = label.Content.ToString().Replace("\n", "").Length;
            }

            ControlTemplate labelTemplate = new ControlTemplate(typeof(Label));

            FrameworkElementFactory gri = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory pol = new FrameworkElementFactory(typeof(Polygon));//ポリゴン用
            FrameworkElementFactory cir = new FrameworkElementFactory(typeof(Ellipse));//円用
            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));

            if (true)//name == "Header" || name == "RankNameLabel" || name == "MemberGroupLabel")//多角形ラベル
            {
                pol = new FrameworkElementFactory(typeof(Polygon));
                TemplateBindingExtension b = new TemplateBindingExtension(Label.BackgroundProperty);
                pol.SetValue(Polygon.FillProperty, b);
                pol.SetValue(Polygon.PointsProperty, PolygonPoints(p));
            }
            else//円形ラベル
            {
                cir = new FrameworkElementFactory(typeof(Ellipse));
                TemplateBindingExtension b = new TemplateBindingExtension(Label.BackgroundProperty);
                cir.SetValue(Ellipse.FillProperty, b);
                cir.SetValue(Ellipse.WidthProperty, rowSize * p[2]);
                cir.SetValue(Ellipse.HeightProperty, columnSize * p[3]);
            }

            if (name != "MemberGroupLabel")//name == "Header" || name == "RankNameLabel" || name == "ComaAlgorithmLabel")//文字(コンテンツ)を持つラベル
            {
                con = new FrameworkElementFactory(typeof(ContentPresenter));
                con.SetValue(ContentPresenter.MarginProperty, MarginLeftCenter(p, contentSize));//マージンにより左端中央に配置
                if(label == null)
                {//ComaAlgorithmLabelのとき
                    con.SetValue(TextBlock.FontSizeProperty, rowSize * p[2] / 4);//[i][コ][マ][目]
                }
                else if(name == "AlgorithmLabel" || name == "AlgorithmLabel0" || name == "AlgorithmLabel1" || name == "AlgorithmLabel2")
                {
                    double fontSize = rowSize * p[2] / contentSize * 0.8;
                    if(fontSize > columnSize * p[3])
                    {
                        fontSize = columnSize * p[3];
                    }
                    con.SetValue(TextBlock.FontSizeProperty, fontSize);
                }
                else
                {
                    con.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);//フォントサイズ＝ヘッダの縦幅の70%
                }

                conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);

                conShadow.SetValue(ContentPresenter.MarginProperty, MarginLeftCenterS(p, contentSize));//マージンにより左端中央に配置 
            }
            else if (name == "MemberGroupLabel")
            {//縦書き
                con = new FrameworkElementFactory(typeof(ContentPresenter));
                double fontSize = rowSize * p[2] * 0.7;//フォントサイズ＝横幅の70%
                if (fontSize * contentSize > columnSize * p[3])//フォントサイズが縦幅を超えたとき
                {
                    fontSize = columnSize * p[3] / contentSize;//フォントサイズ＝縦幅/文字数の70%
                }
                con.SetValue(TextBlock.FontSizeProperty, fontSize);//フォントサイズ＝横幅の70%
                con.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                con.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Top);
           
            }
            
            //Grid構築
            if (sh)//name == "Header")
            {
                gri.AppendChild(pol);
                gri.AppendChild(conShadow);
                gri.AppendChild(con);
            }
            else if (!sh)//name == "RankNameLabel" || name == "MemberGroupLabel")
            {
                gri.AppendChild(pol);
                gri.AppendChild(con);
            }
            else if (false)
            {
                gri.AppendChild(cir);
            }

            //ビジュアルツリーに登録
            labelTemplate.VisualTree = gri;

            //コントロールテンプレート更新
            App.Current.Resources[name] = labelTemplate;

            if(label != null)
            {
                label.IsHitTestVisible = false;//マウス判定を消す
                label.Margin = new Thickness(rowSize * p[0], columnSize * p[1], -windowWidth + rowSize * (p[0] + p[2]), -windowHeight + columnSize * (p[1] + p[3]));//Ellipseに対しては右と下のマージンを削らなくてよい？
            }        

        }
        public void TransParentLabelSet(string name, Label label, double[] p, bool sh)
        {
          
            if (name == "talkLabel")
            {
                if(rowSize * p[2] / 16 < columnSize * p[3] / 2 - 5)
                {
                    label.FontSize = rowSize * p[2] / 16;//セリフによらず一定の割合の大きさの方がよい？
                }
                else
                {
                    label.FontSize = columnSize * p[3] / 2 - 5;//セリフによらず一定の割合の大きさの方がよい？
                }
                
            }
            else
            {
                label.FontSize = rowSize * p[2] / label.Content.ToString().Length;
            }
            
            //label.FontSize = columnSize * p[3] / label.Content.ToString().Length;

            label.Width = rowSize * p[2];
            label.Height = columnSize * p[3];
            label.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);// -windowWidth + rowSize * (p[0] + p[2]), -windowHeight + columnSize * (p[1] + p[3]));
        }
        public void PolygonButtonSet(string name, Button button, double[] p, bool sh)//Button要素のコントロールテンプレートおよびプロパティ更新メソッド
        {
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用
            double contentSize = 1;
            if (button != null)
            {//ListBoxのItems(nullを渡している)のフォントサイズはこのメソッドでは指定しない
                contentSize = button.Content.ToString().Replace("\n", "").Length;
            }

            double angle;
            if (name == "MemberButton")
            {
                angle = 5;
            }
            else if (name == "MatchingButton")
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
            pol.SetValue(Polygon.PointsProperty, PolygonPoints(p));
            pol.SetValue(Polygon.CursorProperty, Cursors.Hand);
            if (name == "MemberNameButton" || name == "MemberDeleteButton" || name == "ComaAlgorithmButton0" || name == "ComaAlgorithmButton1" || name == "ComaAlgorithmButton2")
            {//ListBoxでボタンを横に配置するため
                pol.SetValue(ContentPresenter.MarginProperty, new Thickness(rowSize * p[0], 0, 0, 0));
            }
            //pol.SetValue(Polygon.NameProperty, "memberPolygon");//何故かセットされない
            if (button == null || name == "GroupAddButton" || name == "GroupDeleteButton"
                || name == "MemberAddButton" || name=="TableButton" || name == "ComaButton"
                || name == "ComaAlgorithm0" || name == "ComaAlgorithm1" || name == "ComaAlgorithm2")//!shadowでも可？
            {//ボタンの枠線表示する
                pol.SetValue(Polygon.StrokeProperty, Brushes.Black);
                //pol.SetValue(Polygon.StrokeThicknessProperty, Shape.StrokeThickness(1));
            }

            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
            if (button == null)
            {//ListBoxItems
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
                con.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                if(name == "MemberNameButton" || name == "MemberDeleteButton" || name == "ComaAlgorithmButton0" || name == "ComaAlgorithmButton1" || name == "ComaAlgorithmButton2")
                {//ListBoxでボタンを横に配置するため
                    con.SetValue(ContentPresenter.MarginProperty, new Thickness(rowSize * (0.2 + p[0]), 0, 0, 0));
                }
                else if(name == "MemberRankButton")
                {
                    con.SetValue(ContentPresenter.MarginProperty, new Thickness(rowSize * 0.2, 0, 0, 0));
                }
                else
                {
                    con.SetValue(ContentPresenter.MarginProperty, new Thickness(rowSize * 1, 0, 0, 0));
                }
            }else if(name == "GroupAddButton" || name == "GroupDeleteButton" || name == "MemberAddButton")
            {   //縦書きなので別処理,かつ，影をもたせない
                con.SetValue(ContentPresenter.MarginProperty, PortrateMarginCenter(p, contentSize, name));//マージンにより左端中央に配置
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }
            else if (name == "ToMenuButton")
            {
                //縦書きなので別処理
                con.SetValue(ContentPresenter.MarginProperty, PortrateMarginCenter(p, contentSize, name));//マージンにより左端中央に配置
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(ContentPresenter.MarginProperty, PortrateMarginCenterS(p, contentSize));//マージンにより左端中央に配置
                conShadow.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                conShadow.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }
            else
            {
                Thickness c;
                Thickness s;
                if(name == "ConfigButton")
                {
                    c = MarginLeftCenter(p, contentSize);
                    s = MarginLeftCenterS(p, contentSize);
                }
                else
                {
                    c = MarginCenter(p, contentSize, name);
                    s = MarginCenterS(p, contentSize, name);
                }
                con.SetValue(ContentPresenter.MarginProperty, c);//マージンにより左端中央に配置
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(ContentPresenter.MarginProperty, s);//マージンにより左端中央に配置
                conShadow.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                conShadow.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }
            if (sh)
            {
                gri.AppendChild(pol);
                gri.AppendChild(conShadow);
                gri.AppendChild(con);
            }
            else
            {
                gri.AppendChild(pol);
                gri.AppendChild(con);
            }

            //pol.Name = "buttonPolygon";//この方法だとセットされる
            buttonTemplate.VisualTree = gri;

            //Setter buttonTemplateSetter = new Setter(TemplateProperty, buttonTemplate);
            //buttonStyle.Setters.Add(buttonTemplateSetter);

            App.Current.Resources[name] = buttonTemplate;//コントロールテンプレート更新

            if(button != null)
            {//ListBoxのItemsに関してはフォントサイズを各自で設定する(全てのItemが同じテンプレートを参照するため)
                //が，MemberButtonsのNameボタンに関してはRankボタンの横に配置するためにマージンを設定する
                button.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
                if (name == "ToMenuButton")// || name == "GroupAddButton" || name == "GroupDeleteButton" || name == "MemberAddButton")
                {//縦書き
                    button.FontSize = columnSize * p[3] * 1.0 / contentSize * 0.3;//フォントサイズ＝80% * ボタンの縦幅/文字数
                }
                else if (name == "GroupDeleteButton" || name == "GroupAddButton" || name == "MemberAddButton")
                {//縦書き(文字数の関係でボタンによって比率を変える)
                    button.FontSize = columnSize * p[3] * 1.0 / contentSize * 0.4;//フォントサイズ＝80% * ボタンの縦幅/文字数
                }
                else
                {
                    double fontSize =  rowSize * p[2] * 1.0 / contentSize * 0.8;
                    if (fontSize > columnSize * p[3] * 0.8)
                    {//縦幅を超えてしまわないように
                        fontSize = columnSize * p[3] * 0.8;
                    }
                    button.FontSize = fontSize;//フォントサイズ＝80% * ボタンの横幅/文字数
                }
            }      
        }
        public void EllipseButtonSet(string name, Button button, double[] p, bool sh)//Button要素のコントロールテンプレートおよびプロパティ更新メソッド
        {
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用
            double contentSize = 1;
            contentSize = button.Content.ToString().Replace("\n", "").Length;

            ControlTemplate buttonTemplate = new ControlTemplate(typeof(Button));

            FrameworkElementFactory gri = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory el = new FrameworkElementFactory(typeof(Ellipse));
            TemplateBindingExtension b = new TemplateBindingExtension(Button.BackgroundProperty);
            el.SetValue(Ellipse.FillProperty, b);//pol要素のFillプロパティをbindingでバインドしますの意
            if (name == "GroupNameChangeButton" || name == "GroupOpenButton")
            {//真円
                el.SetValue(Ellipse.StrokeProperty, Brushes.Black);
                if(rowSize > columnSize)
                {
                    el.SetValue(Ellipse.WidthProperty, columnSize * p[2]);
                    el.SetValue(Ellipse.HeightProperty, columnSize * p[3]);
                }
                else
                {
                    el.SetValue(Ellipse.WidthProperty, rowSize * p[2]);
                    el.SetValue(Ellipse.HeightProperty, rowSize * p[3]);
                }
                
            }
            else
            {
                el.SetValue(Ellipse.WidthProperty, rowSize * p[2]);
                el.SetValue(Ellipse.HeightProperty, columnSize * p[3]);
            }        
            el.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            el.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Top);
            el.SetValue(Ellipse.CursorProperty, Cursors.Hand);
            //pol.SetValue(Polygon.NameProperty, "memberPolygon");//何故かセットされない

            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
            con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            if(name == "GroupNameChangeButton" || name == "GroupOpenButton")
            {
                con.SetValue(ContentPresenter.MarginProperty, CircleMarginCenter(p, contentSize, name));
            }
            else
            {
                con.SetValue(ContentPresenter.MarginProperty, MarginCenter(p, contentSize, name));
            }

            if (sh)
            {
                gri.AppendChild(el);
                gri.AppendChild(con);
            }
            else
            {
                gri.AppendChild(el);
                gri.AppendChild(con);
            }
            

            //pol.Name = "buttonPolygon";//この方法だとセットされる
            buttonTemplate.VisualTree = gri;

            App.Current.Resources[name] = buttonTemplate;//コントロールテンプレート更新
            button.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
            if(name == "GroupNameChangeButton" || name == "GroupOpenButton")
            {
                if(rowSize > columnSize)
                {
                    button.FontSize = columnSize * p[2] * 1.0 / contentSize * 0.8;//フォントサイズ＝80% * ボタンの縦幅/文字数
                }
                else
                {
                    button.FontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;//フォントサイズ＝80% * ボタンの横幅/文字数
                }
            }
            else
            {
                button.FontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;//フォントサイズ＝80% * ボタンの横幅/文字数
            }
            
        }
        public void ListBoxSet(string name, ListBox lb, double[] p)
        {//長方形ですよね
            lb.Width = rowSize * p[2];
            lb.Height = columnSize * p[3];
            lb.Margin=new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
            if(name == "ComaListBox")
            {//文字数が一定なのでフォントサイズを指定する
                lb.FontSize = columnSize * comaAlgorithmButton0Params[3];
            }
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

        
        public void GroupsSetToListBox()
        {//グループリストの更新
            groupList.Clear();
            foreach (string name in groups)
            {
                double contentSize = name.Replace("\n", "").Length;
                double fontSize = GroupFontSize(name,"Group");
                SolidColorBrush color;
                if (name == "部内")
                {
                    color = Brushes.LightBlue;
                }
                else if (name == "所属ナシ")
                {
                    color = Brushes.Pink;
                }
                else
                {
                    color = Brushes.LightGreen;
                }
                //MarginCenter(groupButtonParams, contentSize, "GroupButton");
                Group n = new Group { Name = name, FontSize = fontSize, Color = color };
                groupList.Add(n);
            }
        }
        public void GroupFontSizeSet()
        {//グループリストのフォントサイズのみを更新　WindowSizeChanged用
            for (int i = 0; i < groupList.List.Count; i++)
            {
                string n = "";
                Group g = groupList.List[i];
                try
                {
                    n = preSelectedGroup.Content.ToString();
                }
                catch
                {//何も選択されていない場合に例外が返るので
                }
                if (groupList.List[i].Name == n)
                {//このメソッドはWindowSizeChange時に行うが，選択しているグループの色は白色になっているのでこのタイミングでリストに反映しておく
                    groupList.List[i].Color = Brushes.White;
                }
                groupList.List[i].FontSize = GroupFontSize(g.Name,"Group");
            }
        }
        public double GroupFontSize(string name,string obj)
        {//GroupというかリストボックスItemのフォントサイズを返す
            double[] para = { };
            double contentSize = name.Replace("\n", "").Length;
            if(obj == "Group")
            {
                para = groupButtonParams;
            }else if(obj == "MemberName")
            {
                para = memberNameButtonParams;
            }else if(obj == "MemberRank")
            {
                para = memberRankButtonParams;
            }else if(obj == "MemberDelete")
            {
                para = memberDeleteButtonParams;
            }
            double result;
            if (rowSize * para[2] * 1.0 / contentSize * 0.8 < columnSize * para[3])
            {
                result = rowSize * para[2] * 1.0 / contentSize * 0.8;
            }
            else
            {
                result = columnSize * para[3];
            }
            return result;
        }

        public void MembersSetToListBox(string groupName)
        {//選んだグループに属するメンバーのリストの更新
            memberList.Clear();
            currentMembersToShow = mlogic.MembersOfGroup(groupName);
            foreach (string[] member in currentMembersToShow)
            {
                //double contentSize = member[1].Replace("\n", "").Length;//名前の文字数
                double nameFontSize = GroupFontSize(member[1],"MemberName");//Groupのものを再利用
                double rankFontSize = GroupFontSize(member[0],"MemberRank");//Groupのものを再利用
                double deleteFontSize = GroupFontSize("削除", "MemberDelete");//Groupのものを再利用
                Member n = new Member { Rank = member[0], Name = member[1], NameFontSize = nameFontSize, RankFontSize = rankFontSize, DeleteFontSize = deleteFontSize };
                memberList.Add(n);
                if(groupName == "部内")
                {
                    memberList.Sort();//ランク順ソート
                }
            }
        }
        public void MemberFontSizeSet()
        {//Itemsのフォントサイズのみを更新　WindowSizeChanged用
            for (int i = 0; i < memberList.List.Count; i++)
            {
                //string n = "";
                Member m = memberList.List[i];
                //try
                //{
                //   n = preSelectedGroup.Content.ToString();
                //}
                //catch
                //{//何も選択されていない場合に例外が返るので
                //}
                //if (groupList.List[i].Name == n)
                //{//このメソッドはWindowSizeChange時に行うが，選択しているグループの色は白色になっているのでこのタイミングでリストに反映しておく
                //    groupList.List[i].Color = Brushes.White;
                //}
                memberList.List[i].RankFontSize = GroupFontSize(m.Rank,"MemberRank");
                memberList.List[i].NameFontSize = GroupFontSize(m.Name, "MemberName");
                memberList.List[i].DeleteFontSize = GroupFontSize("削除", "MemberDelete");
            }
        }
        public void ComaSetToListBox()
        {//ListBoxの更新
            comaList.Clear();
            int i = -1;
            foreach (string config in configs)
            {
                if(i < 1){
                    //台数とコマ数の情報なので無視
                    i++;
                }
                else
                {
                    int number = i;//iコマ目
                    int alg = int.Parse(config);//アルゴリズム
                    string label = i.ToString() + "コマ目";

                    Coma c = new Coma { Number0 = number.ToString() + ":0", Number1 = number.ToString() + ":1", Number2 = number.ToString() + ":2", Algorithm = alg, Label = label };
                    c.Check();//Algorithmプロパティに合わせて☑する
                    comaList.Add(c);
                    i++;
                }
                
            }
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
            else if (clickNumber == 2)
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
            else if (clickNumber == 3)
            {
                clickNumber = 0;
                s4 = ((Storyboard)this.FindResource("ClickEffect")).Clone();
                Ellipse e4 = this.effect4;
                e4.Margin = new Thickness(e.GetPosition(this.grid).X - 10 / 2, e.GetPosition(this.grid).Y - 10 / 2, 0, 0);
                foreach (var child in s4.Children)
                {
                    Storyboard.SetTarget(child, e4);
                }
                s4.Begin();
            }
        }
        public void removeClickEffect()
        {
            if (clickNumber == 0)
            {
                s1.Stop();
                s2.Stop();
                s3.Stop();
            }
            else if (clickNumber == 1)
            {
                s2.Stop();
                s3.Stop();
                s4.Stop();
            }
            else if (clickNumber == 2)
            {//左クリック中に右クリックしてから離すとここに来る，起動しているのはs1とs2
                s3.Stop();
                s4.Stop();
                s5.Stop();
            }
            else
            {
                s4.Stop();
                s5.Stop();
                s1.Stop();
            }
        }
    }
}

