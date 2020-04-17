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

//オブジェクトのイベントに関係なく，オブジェクトの座標などを設定するメソッドはここにおく
namespace Comarenkun
{
    public partial class MainWindow : Window
    {
        public PointCollection PentaPoints(double[] p)//Polygonの列幅行幅から,左上を原点とした適当なPoints文字列を返す
        {
            PointCollection result = new PointCollection();
            result.Add(new Point(rowSize * p[4], columnSize * (p[3] + p[5])));
            result.Add(new Point(0, 0));
            result.Add(new Point(rowSize * p[6], columnSize * p[7]));
            result.Add(new Point(rowSize * (p[2] + p[8]), columnSize * p[9]));
            result.Add(new Point(rowSize * (p[2] + p[10]), columnSize * (p[3] + p[11])));
            return result;
        }
        public PointCollection PolygonPoints(double[] p)//Polygonの列幅行幅から,左上を原点とした適当なPoints文字列を返す
        {
            //PolygonのPointsは左下左上右上右下の順
            string leftbottom = (rowSize * p[4]).ToString(format) + "," + (columnSize * (p[3] + p[5])).ToString(format) + " ";
            string lefttop = "0,0 ";
            string righttop = (rowSize * (p[2] + p[6])).ToString(format) + "," + (columnSize * p[7]).ToString(format) + " ";
            string rightbottom = (rowSize * (p[2] + p[8])).ToString(format) + "," + (columnSize * (p[3] + p[9])).ToString(format);
            return PointCollection.Parse(leftbottom + lefttop + righttop + rightbottom);
        }
        public Thickness MarginLeftCenter(double[] p)
        {
            double fontSize = columnSize * p[3] * 0.7;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Left　のかわり(Gridを使用していないため)

            //Marginは左上右下の順
            double left = rowSize * 0.5;//Windowの横幅に応じて少しだけ左端を空ける
            double top = columnSize * p[3] / 2 - fontSize / 2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];//親グリッドが画面全体なので右と下の負のマージンがないと画面一杯に範囲が広がってしまう<-IsHitTestVisible=falseで解決はした
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness MarginLeftCenterS(double[] p)//ラベルなどの影用のちょっとずらしたマージン生成
        {
            double fontSize = columnSize * p[3] * 0.7;
            //原点を左上として，ラベルなどの座標をマージンで指定する.
            //VerticalAlignment=Center,HorizontalAlignment=Left　のかわり(Gridを使用していないため)

            //Marginは左上右下の順
            double left = rowSize * (0.5 + 0.1);
            double top = columnSize * p[3] / 2 - fontSize / 2 + columnSize * 0.2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];//右と下のマージンがないと画面一杯に範囲が広がってしまう
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness MarginCenter(double[] p, double contentSize, string name)
        {
            //横方向のマージンに文字数contentSizeが必要
            double fontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;
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
            double left = rowSize * trueWidth / 2 - fontSize * contentSize / 2;
            double top;
            if(name == "MemberButton")
            {
                top = columnSize * trueHeight / 2 - fontSize / 2 - columnSize;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル),Memberボタンは右上がりなためy座標1つぶん上に配置
            }
            else
            {
                top = columnSize * trueHeight / 2 - fontSize / 2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            }
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];//右と下のマージンがないと画面一杯に範囲が広がってしまう
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness MarginCenterS(double[] p, double contentSize, string name)//ラベルなどの影用のちょっとずらしたマージン生成
        {
            double fontSize = rowSize * p[2] * 1.0 / contentSize * 0.8;
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
            double left = rowSize * trueWidth / 2 - fontSize * contentSize / 2 + rowSize * (0.1);
            double top;
            if (name == "MemberButton")
            {
                top = columnSize * trueHeight / 2 - fontSize / 2 + columnSize * (-1 + 0.2);//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル),Matchingボタンは右上がりなためy座標1つぶん上に配置
            }
            else
            {
                top = columnSize * trueHeight / 2 - fontSize / 2 + columnSize * 0.2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            }
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness CircleMarginCenter(double[] p, double contentSize, string name)
        {//真円用
            Thickness result = new Thickness();
            if (rowSize > columnSize)
            {
                double fontSize = columnSize * p[2] * 1.0 / contentSize * 0.8;
                result = new Thickness(columnSize * p[2] / 2 - fontSize * contentSize / 2, columnSize * p[2] / 2 - fontSize * 1 / 2, 0, 0);
            }
            else
            {
                double fontSize = rowSize * p[2] * 1.0 / contentSize * 0.8; 
                result = new Thickness(rowSize * p[2] / 2 - fontSize * contentSize / 2, rowSize * p[2] / 2 - fontSize * 1 / 2, 0, 0);
            }
            
        
            return result;
        }
        public Thickness PortrateMarginCenter(double[] p, double contentSize, string name)
        {//コンテンツが縦書きの場合
         //縦方向のマージンに文字数contentSizeが必要
            double fontSize = 1;
            if (name == "GroupNameChangeButton" || name == "GroupAddButton" || name == "MemberAddButton")
            {
                fontSize = columnSize * p[3] * 1.0 / contentSize * 0.4;
            }
            else
            {
                fontSize = columnSize * p[3] * 1.0 / contentSize * 0.3;
            }
            
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
            double left = rowSize * trueWidth / 2 - fontSize / 2;
            double top = columnSize * trueHeight / 2 - fontSize * contentSize / 2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル),ボタンが右上がりなためy座標1つぶん上に配置
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];//右と下のマージンがないと画面一杯に範囲が広がってしまう
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Thickness PortrateMarginCenterS(double[] p, double contentSize)//ラベルなどの影用のちょっとずらしたマージン生成
        {
            double fontSize = columnSize * p[3] * 1.0 / contentSize * 0.3;
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
            double left = rowSize * trueWidth / 2 - fontSize / 2 + rowSize * (0.1);
            double top = columnSize * trueHeight / 2 - fontSize * contentSize / 2 + columnSize * 0.2;//縦のマージンはy座標の中央値-フォントサイズ/2(WPFではwindowの座標もフォントサイズもピクセル)
            double right = -windowWidth + rowSize * p[2];
            double bottom = -windowHeight + columnSize * p[3];
            Thickness result = new Thickness(left, top, right, bottom);
            return result;
        }
        public Color Brighten(Color input)//カラーをすこし黄色めに明るくする
        {
            int[] rgb = { input.R, input.G, input.B };
            int r, g, b;
            if (rgb[0] > 255 - 16 * 3)
            {
                r = 255;
            }
            else
            {
                r = rgb[0] + 16 * 3;
            }
            if (rgb[1] > 255 - 16 * 2)
            {
                g = 255;
            }
            else
            {
                g = rgb[1] + 16 * 2;
            }
            if (rgb[2] < 16 * 1)
            {
                b = 0;
            }
            else
            {
                b = rgb[2] - 16 * 1;
            }
            return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
        public Color BlueBrighten(Color input)//カラーをすこし青めに明るくする
        {
            int[] rgb = { input.R, input.G, input.B };
            int r, g, b;
            if (rgb[0] < 16 * 1)
            {
                r = 0;
            }
            else
            {
                r = rgb[0] - 16 * 1;
            }
            if (rgb[1] > 255 - 16 * 2)
            {
                g = 255;
            }
            else
            {
                g = rgb[1] + 16 * 2;
            }
            if (rgb[2] > 255 - 16 * 3)
            {
                b = 255;
            }
            else
            {
                b = rgb[2] + 16 * 3;
            }
            return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
        public Color Darken(Color input)//カラーをすこし暗くする(Brighten2回分の真逆)
        {
            int[] rgb = { input.R, input.G, input.B };
            int r, g, b;
            if (rgb[0] < 16 * 6)
            {
                r = 0;
            }
            else
            {
                r = rgb[0] - 16 * 6;
            }
            if (rgb[1] < 16 * 4)
            {
                g = 0;
            }
            else
            {
                g = rgb[1] - 16 * 4;
            }
            if (rgb[2] > 255 - 16 * 2)
            {
                b = 255;
            }
            else
            {
                b = rgb[2] + 16 * 2;
            }
            return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }
        public void ButtonBrighten(Button b)
        {//渡されたボタンの色を↑の関数に通した値に変える
            Color currentColor = ((SolidColorBrush)b.Background).Color;
            b.Background = new SolidColorBrush(Brighten(currentColor));
        }

        public void ButtonBlueBrighten(Button b)
        {//渡されたボタンの色を↑の関数に通した値に変える
            Color currentColor = ((SolidColorBrush)b.Background).Color;
            b.Background = new SolidColorBrush(BlueBrighten(currentColor));
        }

        public void ButtonDarken(Button b)
        {//渡されたボタンの色を暗くする
            Color currentColor = ((SolidColorBrush)b.Background).Color;
            b.Background = new SolidColorBrush(Darken(currentColor));
        }
    }
}
