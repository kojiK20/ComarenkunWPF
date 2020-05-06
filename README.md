# コマ練くん made with WPF
![comarenkun](https://user-images.githubusercontent.com/50648545/81155129-96d09900-8fbf-11ea-9440-903a4d7cdd71.png)

## 概要

『コマ練くん』は，卓球の練習相手の組み合わせを自動でマッチングしてくれるWindowsアプリケーションです．

部内だけでなく，部外者の参加にも対応します．メンバの所属の認識，ランクの認識，ランクに基づいたマッチングアルゴリズムの選択，マッチング結果のLINEへの送信が可能です．

全ての操作を視覚的に行えます．マッチング部分は全てマウスのみで操作可能です．

## 詳細

マッチングの入力は，1コマ目の参加者情報と，2コマ目以降での途中参加者/途中退出者情報となります．

マッチング結果(出力)は，対人の組と多球(3人)の組で構成されます．全て対人で組める場合はそうします．人数が奇数であったり，台数の関係で対人で入れない場合は多球の組ができます．

出力のLINEグループへの送信にはLINE Notifyを利用します．

マッチングアルゴリズムは最適な解を導くものではまったくありませんが，余程運が悪くない限りは制約に従います．

アルゴリズムは，

マッチングが被らず，無駄がないこと＞所属が被らないこと＞指定したランク制約を満たさないこと

の順に優先度を置いています．ので，全コマ同じアルゴリズムに指定した場合などは途中からランク制約が破られる可能性が高いです．(ランク制約を守り続けると，以前のコマで既出のマッチングを選ばざるを得なくなるため)

## 使い方

### 起動

ReleaseからComarenkun_Binary.zipをダウンロード&解凍して下さい．内部に実行ファイルComarenkun.exeがあります．(https://github.com/kojiK20/ComarenkunWPF/releases/latest/download/Comarenkun_Binary.zip)

### メンバ/所属

#### 管理

Comarenkun.exeと同ディレクトリにあるText/members.txtおよびTexts/foreigners.txtに対して読み書きを行うことでメンバを管理します．ファイルおよびフォルダが存在しない場合は自動的に作成されます．

#### 編集

起動後，「メンバー」ボタンから所属編集画面に遷移できます．

さらに，所属を選択後，「開ク」ボタンからその所属に属するメンバの編集画面に遷移できます．「部内」メンバには部内ランクが整数値で与えられます．「部外」メンバには，ランクに整数値の代わりにマッチング時に参加者の中で上位(↑)/下位(↓)/ランダム(空文字)として扱われることを示す文字を与えます．メンバの名前を押すと名前の変更が，ランクを左クリックするとランクを1上げ，右クリックすると1下げることができます．

「部内」および「所属ナシ」メンバは所属に関係なくマッチングされます．それ以外の所属のメンバは，同じ所属どうしではなるべく当たらないようにマッチングされます．

### オプション

#### 管理

Comarenkun.exeと同ディレクトリにあるText/configs.txtおよびTexts/LINEtoken.txtに対して読み書きを行うことでオプションを管理します．ファイルおよびフォルダが存在しない場合は自動的に作成されます．

起動後，「マッチング」ボタンから1コマ目の設定画面に遷移し，右上の「オプション」ボタンを押すことでオプション画面に遷移できます．

#### 台数/コマ数/アルゴリズム

オプション画面のそれぞれ対応するボタンから設定できます．

台数/コマ数は左クリックで増え，右クリックで減ります．

アルゴリズムは「近(ランクが近い人が優先して当たる)」/「遠い(ランクが遠い人が優先して当たる)」/「乱(ランダム)」の３種類あります．

#### LINE

「LINE」ボタンから，事前に利用者が発行したLINE Notify アクセストークンを1つ登録できます．登録しておくことで，LINE Notifyを利用してマッチング結果をLINEグループに送信できます．

### マッチング

起動後に「マッチング」ボタンからマッチング画面に遷移します．

各所属を左クリックすることでメンバを表示できるほか，所属を右クリックして表示されるコンテキストメニューからメンバの一括選択/解除が可能です．

1コマ目は参加者をクリックすることで選んでいきます．

2コマ目以降は，途中参加/途中退出する人を選んでいきます．1つ前のコマで参加していた人のボタンの色が青く表示されます．青いボタンを選択すると，その人は途中退出ということになります．通常の色のボタンを選択すると，その人は途中参加ということになります．

最後に「GO!」ボタンを押すことで組み合わせが出力されます．LINE Notifyアクセストークンを登録している場合，右上の「LINE」ボタンからLINEグループに結果を通知できます．

## デモ

まだ
