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

            if (name == "Header")//文字(コンテンツ)を持つラベル
            {
                con = new FrameworkElementFactory(typeof(ContentPresenter));
                con.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);//フォントサイズ＝ヘッダの縦幅の70%
                con.SetValue(ContentPresenter.MarginProperty, MarginLeftCenter(p));//マージンにより左端中央に配置

                conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(TextBlock.FontSizeProperty, columnSize * p[3] * 0.7);

                conShadow.SetValue(ContentPresenter.MarginProperty, MarginLeftCenterS(p));//マージンにより左端中央に配置 
            }

            //Grid構築
            if (name == "Header")
            {
                gri.AppendChild(pol);
                gri.AppendChild(conShadow);
                gri.AppendChild(con);
            }
            else if (false)
            {
                gri.AppendChild(pol);
            }
            else if (false)
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
        public void TransParentLabelSet(string name, Label label, double[] p)
        {
            /*if(rowSize * p[2] < columnSize * p[3])
            {
                if (name == "shozoku")
                {
                    label.FontSize = rowSize * p[2] / label.Content.ToString().Length;
                }
                else
                {
                    label.FontSize = rowSize * p[2] / label.Content.ToString().Length;
                }
            }
            else
            {
                if (name == "shozoku")
                {
                    label.FontSize = columnSize * p[3] / label.Content.ToString().Length;
                }
                else
                {
                    label.FontSize = columnSize * p[3] / label.Content.ToString().Length;
                }
            }
            */
            label.FontSize = rowSize * p[2] / label.Content.ToString().Length;
            //label.FontSize = columnSize * p[3] / label.Content.ToString().Length;

            label.Width = rowSize * p[2];
            label.Height = columnSize * p[3];
            label.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);// -windowWidth + rowSize * (p[0] + p[2]), -windowHeight + columnSize * (p[1] + p[3]));
        }
        public void PolygonButtonSet(string name, Button button, double[] p)//Button要素のコントロールテンプレートおよびプロパティ更新メソッド
        {
            //this.header.Content = windowWidth.ToString(format);//小数第二位まで テスト用
            double contentSize = 1;
            if (name != "GroupButton")
            {
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
            //pol.SetValue(Polygon.NameProperty, "memberPolygon");//何故かセットされない

            FrameworkElementFactory con = new FrameworkElementFactory(typeof(ContentPresenter));
            FrameworkElementFactory conShadow = new FrameworkElementFactory(typeof(ContentPresenter));
            if (name == "GroupButton")
            {//マージンを設定しない
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }
            else if (name == "ToMenuButton")
            {
                //縦書きなので別処理
                con.SetValue(ContentPresenter.MarginProperty, PortrateMarginCenter(p, contentSize));//マージンにより左端中央に配置
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(ContentPresenter.MarginProperty, PortrateMarginCenterS(p, contentSize));//マージンにより左端中央に配置
                conShadow.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                conShadow.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }
            else
            {
                con.SetValue(ContentPresenter.MarginProperty, MarginCenter(p, contentSize, name));//マージンにより左端中央に配置
                con.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                con.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない

                conShadow.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                conShadow.SetValue(ContentPresenter.MarginProperty, MarginCenterS(p, contentSize, name));//マージンにより左端中央に配置
                conShadow.SetValue(ContentPresenter.RenderTransformProperty, rotateTransform1);
                conShadow.SetValue(ContentPresenter.IsHitTestVisibleProperty, false);//テキストにはマウス判定を持たせない
            }      

            gri.AppendChild(pol);
            gri.AppendChild(conShadow);
            gri.AppendChild(con);

            //pol.Name = "buttonPolygon";//この方法だとセットされる
            buttonTemplate.VisualTree = gri;

            //Setter buttonTemplateSetter = new Setter(TemplateProperty, buttonTemplate);
            //buttonStyle.Setters.Add(buttonTemplateSetter);

            App.Current.Resources[name] = buttonTemplate;//コントロールテンプレート更新
            if(name != "GroupButton")
            {//GroupButtonsに関してはマージンもフォントサイズも各自で設定する
                button.Margin = new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
                if (name == "ToMenuButton")
                {
                    button.FontSize = columnSize * p[3] * 1.0 / contentSize * 0.3;//フォントサイズ＝80% * ボタンの縦幅/文字数
                }
                else
                {
                    button.FontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;//フォントサイズ＝80% * ボタンの横幅/文字数
                }
            }      
        }
        public void EllipseButtonSet(string name, Button button, double[] p)//Button要素のコントロールテンプレートおよびプロパティ更新メソッド
        {
            
        }
        public void ListBoxSet(string name, ListBox lb, double[] p)
        {
            lb.Width = rowSize * p[2];
            lb.Height = columnSize * p[3];
            lb.Margin=new Thickness(rowSize * p[0], columnSize * p[1], 0, 0);
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
}

