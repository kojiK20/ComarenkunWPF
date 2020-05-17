using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comarenkun
{
    class MatchingLogic
    {
        
        private MainWindow window;
        public MatchingLogic(MainWindow window)
        {
            this.window = window;
        }

        int leave = 0;
        int resetCounter;//マッチング中に選出状況をリセットした回数．1以上なら指定回数まで組みなおす
        int bridgeCounter;//マッチング中に橋を交換した回数．1以上なら指定回数まで組みなおす
        int sameGroupCounter;//マッチング中に所属制約を破った回数．1以上なら指定回数まで組みなおす
        int sameGroupCounterMin;
        int ignoreRankCounter;//マッチング中にランク制約を破った回数．1以上なら指定回数まで組みなおす
        int groupLoopCounter;//組みなおした回数
        int rankLoopCounter;
        int bridgeLoopCounter;
        int resetLoopCounter;
        int groupMax;
        int rankMax;
        int bridgeMax;
        int resetMax;
        int maxLoop;//指定回数
        int maxBridgeLoop;//指定回数
        int maxSameGroupLoop;//指定回数
        int maxIgnoreRankLoop;//指定回数
        int maxResetLoop;

        bool noForeigners = false;//部外者が誰もいない->true
        bool purupuru = false;

        public async void process(int s)
        {
            await Task.Run(() =>
            {//スレッドプールに進捗通知を投げる
                window.Dispatcher.Invoke((Action)(() =>
                {//windowスレッドのキューに投げる
                    string ta = window.talkLabel.Content.ToString();
                    string ok = "";
                    string wrong = "";
                    for (int i = 0; i < 11; i++)
                    {
                        if(i <= s)
                        {
                            ok = ok + "■";
                        }
                        else
                        {
                            wrong = wrong + "□";
                        }
                        
                    }
                    window.talkLabel.Content = ok + wrong;
                    if(s > 3)
                    {//progressバー>=4でプルプルしはじめる
                        if (!purupuru)
                        {
                            window.StoryBegin("ComarenkunPuruPuru");
                            purupuru = true;
                        }     
                    }
                }));
            });

        }

        public string Matching(List<List<string>> participants, List<List<string>> increase, List<List<string>> decrease, List<string> configs)
        {//マッチングする
            

            string result = "";
            List<ResultSet> set = new List<ResultSet>();
            int table = int.Parse(configs[0]);
            int coma = int.Parse(configs[1]);
            string[] alg = new string[configs.Count - 2];
            int fuga = 0;
            foreach(string s in configs)
            {
                if(fuga > 1)
                {
                    alg[fuga - 2] = s;
                }
                fuga++;
            }
            groupMax = 0;//;maxSameGroupLoop回組んだ際の最小所属被り回数，0回が発生した場合は即採用
            rankMax = 0;
            bridgeMax = 0;
            resetMax = 0;
            resetCounter = 0;//マッチング中に選出状況をリセットした回数．>resetMaxなら指定回数まで組みなおす←無駄なく選んだ際のリセットはカウントしないので，理想的にはresetMaxは0のままで組めるはず
            bridgeCounter = 0;//これも理想的には一度も発生しない，優先度も低い
            sameGroupCounter = 0;
            sameGroupCounterMin = -1;
            ignoreRankCounter = 0;//これは発生しやすいが優先度も低い
            groupLoopCounter = -1;//組みなおした回数
            rankLoopCounter = 0;
            bridgeLoopCounter = 0;
            resetLoopCounter = 0;
            maxBridgeLoop = 10;//指定回数
            maxIgnoreRankLoop = 50;
            maxSameGroupLoop = 100;
            maxResetLoop = 10000;


            FileLogic flogic = new FileLogic(window); 
            leave = 0;
            List<MemberNode> memberList = new List<MemberNode>();
            List<List<MemberNode>> eachMemberList = new List<List<MemberNode>>();//[i]:iコマ目の参加者のNode
            List<MemberNode> all = flogic.AllMemberNodes();//名簿内全員のノード集合
            int r;
            string n;
            string g;
            int foreignersCounter = 0;
            int progressPerComa = 0;
            int comaPerProgress = 0;
            for(int i = 0; i < participants.Count; i++)
            {//memberListに参加者全員のNodeをつっこむ
                foreach(string inc in increase[i])
                {
                    MemberNode m = all.Find(mm => mm.Name == inc);
                    if(m.Group == "部内" || m.Group == "所属ナシ")
                    {
                        m.Group = "";
                    }
                    else
                    {
                        foreignersCounter++;
                    }
                    if(memberList.IndexOf(m) == -1)
                    {
                        memberList.Add(m);
                    }
                }
            }
            if(foreignersCounter == 0)
            {
                noForeigners = true;
                
                if(coma < 10)
                {//progressPerComa(1コマ進めば何progressするか)を計算
                    progressPerComa = 10 / coma;
                }
                else
                {//comaPerProgress(何コマ進めば1progressするか)を計算
                    comaPerProgress = coma / 10;
                }
            }

            List<Matching> edges = new List<Matching>();//参加者間の枝集合＝マッチング，動的な変更が多いのでリスト

            //全参加者のノード集合memberListはできた，次はまず1コマ目時点での枝集合edgeを生成する，
            Matching edge;
            eachMemberList.Add(new List<MemberNode>());
            List<MemberNode> first = eachMemberList[0];//参照渡し
            foreach(string p in participants[0])
            {//1コマ目のMemberNode集合
                first.Add(memberList.Find(mm => mm.Name == p));
            }
            foreach(MemberNode m in first)
            {//1コマ目は完全グラフなので次数は計算で求まる
                m.Degree = first.Count - 1;
            }
            for (int i = 0; i < first.Count; i++)
            {
                for(int ii = i + 1; ii < first.Count; ii++)
                {
                    if(first[i].Group != first[ii].Group)
                    {//所属が異なる(＝このマッチングは無考慮に組んでOK)
                        edge = new Matching(first[i].Name, first[ii].Name, first[i].Rank, first[ii].Rank, true);
                    }
                    else
                    {
                        edge = new Matching(first[i].Name, first[ii].Name, first[i].Rank, first[ii].Rank, false);
                    }
                    edges.Add(edge);

                }
            }

            //これで，1コマ目時点での枝集合edgesができた
            //各コマでの参加者Node集合をつくる
            for(int i = 1;i < coma; i++)
            {
                eachMemberList.Add(new List<MemberNode>());
                foreach(string p in participants[i])
                {//2コマ目以降の次数は動的に定まるのでノータッチ
                    MemberNode m = memberList.Find(mm => mm.Name == p);
                    eachMemberList[i].Add(m);
                }
            }

            //コマ毎にアルゴリズムに従って操作していく
            //アルゴリズムに従った枝選定->アルゴリズムに合致する枝がなくなれば合致しない枝も選定
            //->それもなくなれば同所属間の枝も選定->それもなくなれば枝集合をリセット
            /*List<string> patNum1 = ((textBox1.Text).Split('　')).ToList();
            int patNum2 = patNum1.Count + add2.Length - dec2.Length;
            int patNum3 = patNum2 + add3.Length - dec3.Length;
            int patNum4 = patNum3 + add4.Length - dec4.Length;
            int patNum5 = patNum4 + add5.Length - dec5.Length;
            int table = 0;*/
            //bool duplicate = false;
            int loopStopper = 0;//例外時に1
            /*foreach (MemberNode m in memberList)
            {
                if (memberList.Count(mm => mm.Name == m.Name) > 1)
                {//参加者の名前に重複がある
                    duplicate = true;
                    //throw new System.ArgumentException("error", "original");
                }
            }
            if (duplicate)
            {
                MessageBox.Show("同じ名前の参加者が複数いるコマよ");
                roopStopper = 1;
            }*/
            for(int i = 0; i < coma; i++)
            {
                List<string> l = participants[i];
                if(l.Count <= 5)
                {
                    MessageBox.Show((i+1).ToString() + "コマ目の参加者が6人未満になっているコマ");
                    loopStopper = 1;
                    break;
                }else if(l.Count > table * 3)
                {
                    MessageBox.Show((i+1).ToString() + "コマ目の参加者が【使用可能台数×3】(人)を超えているコマ...");
                    loopStopper = 1;
                    break;
                }
            }

            List<Matching> chosenMatchs = new List<Matching>();//選んだ枝を保存，枝のリセットとともにリセット
            
            List<Matching> edgesOrigin = new List<Matching>(edges);//組みなおす際に使用する
            if (loopStopper == 0)//組み合わせ生成処理
            {
                process(-1);
                int progress = 0;
                List<string> takyuChosen = new List<string>();
                for(int i = 0; i <= coma; i++)
                {
                    if (noForeigners)
                    {//部外者がいない場合のプログレスバー(所属制約が破られることがないので到達コマ数で決定)
                        if(progressPerComa > 0)
                        {//コマ数が10以下
                            if(i > progress)
                            {
                                process(progress * progressPerComa);
                                progress++;
                            }
                        }
                        else if(comaPerProgress > 0)
                        {//コマ数が10以上
                            if(coma / comaPerProgress > progress)
                            {
                                process(progress);
                                progress++;
                            }
                        }
                    }
                    if( i < coma)
                    {
                        ResultSet s = new ResultSet(null, null, null);
                        set.Add(s);
                        if (alg[i] == "0")
                        {
                            set[i] = TopToBottom(eachMemberList[i], edges, takyuChosen, table, i, set, chosenMatchs);
                            //result = result +  (i + 1).ToString() + "コマ目：\n" + set.Result;
                            edges = set[i].Edges;//残っている枝
                            takyuChosen = set[i].TakyuChosen;//多球に選ばれた人
                        }
                        else if (alg[i] == "1")
                        {
                            set[i] = Near(eachMemberList[i], edges, takyuChosen, table, i, set, chosenMatchs);
                            //result = result +  (i + 1).ToString() + "コマ目：\n" + set.Result;
                            edges = set[i].Edges;//残っている枝
                            takyuChosen = set[i].TakyuChosen;//多球に選ばれた人
                        }
                        else
                        {
                            set[i] = Random(eachMemberList[i], edges, takyuChosen, table, i, set, chosenMatchs);
                            //result = result  + (i + 1).ToString() + "コマ目：\n" + set.Result;
                            edges = set[i].Edges;//残っている枝
                            takyuChosen = set[i].TakyuChosen;//多球に選ばれた人
                        }
                        if (set[i].Result == "ERROR:台が足りませんコマ")
                        {
                            MessageBox.Show(set[i].Result);
                            loopStopper = 1;
                            break;
                            //フラグ等を立てて処理を終了させる
                        }


                        if (i < coma - 1)
                        {
                            MemberNode[] incL = new MemberNode[increase[i+1].Count];
                            int increaseSize = increase[i + 1].Count;
                            for(int inc = 0; inc < increaseSize; inc++)
                            {//増分者のノード
                                MemberNode m = memberList.Find(mm => mm.Name == increase[i+1][inc]);
                                incL[inc] = m;
                            }
                            foreach (MemberNode am in incL)
                            {
                                foreach (MemberNode m in eachMemberList[i])//参加者それぞれにincreaseから枝をはる.ただもしchosenMatchs内の枝がある場合は無視
                                {
                                    Matching ade = new Matching(am.Name, m.Name, am.Rank, m.Rank, am.Group != m.Group);
                                    Matching ade2 = new Matching(m.Name, am.Name, m.Rank, am.Rank, am.Group != m.Group);
                                    if (!chosenMatchs.Contains(ade) && !chosenMatchs.Contains(ade2))
                                    {
                                        edges.Add(ade);
                                    }
                                }
                            }
                            int incLSize = incL.Length;
                            for (int ini = 0; ini < incL.Length; ini++)
                            {//追加参加者どうしの枝がまだはれてないのではる
                                for (int inj = ini + 1; inj < incL.Length; inj++)
                                {
                                    string inc2 = increase[i + 1][inj];
                                    MemberNode am1 = incL[ini];
                                    MemberNode am2 = incL[inj];
                                    Matching ade1 = new Matching(am1.Name, am2.Name, am1.Rank, am2.Rank, am1.Group != am2.Group);
                                    Matching ade2 = new Matching(am2.Name, am1.Name, am2.Rank, am1.Rank, am1.Group != am2.Group);
                                    if (!chosenMatchs.Contains(ade1) && !chosenMatchs.Contains(ade2))
                                    {
                                        edges.Add(ade1);
                                    }
                                }
                            }

                            foreach (string dc in decrease[i + 1])//脱退者から伸びる枝を全て消す
                            {
                                edges.RemoveAll(pyo => pyo.Name1 == dc || pyo.Name2 == dc);
                            }
                        }
                    }
                    
                    if ( groupLoopCounter < maxSameGroupLoop && i == coma)
                    {//i==coma...最終コマ後
                        //1000回の中から所属制約が破られた回数の最小値を取得
                        if (sameGroupCounter == 0)
                        {//所属被りが0回のままrank,bridge,reset制約を守りつつ最後まで組めたor所属が部内か所属ナシのみ＝＞即採用
                            process(10);
                            break;
                        }
                        else if(sameGroupCounter < sameGroupCounterMin || sameGroupCounterMin < 0)//初回)
                        {//所属被りが最小回数で組めた＝＞所属破り許容最小回数を更新
                            sameGroupCounterMin = sameGroupCounter;
                            
                        }
                        else
                        {
                        }
                        //1コマ目からループする
                        set = new List<ResultSet>();
                        edges = new List<Matching>(edgesOrigin);
                        chosenMatchs = new List<Matching>();
                        takyuChosen = new List<string>();
                        groupLoopCounter++;
                        sameGroupCounter = 0;
                        ignoreRankCounter = 0;
                        bridgeCounter = 0;
                        resetCounter = 0;
                        i = -1;

                        if(groupLoopCounter %(maxSameGroupLoop * 1/10) == 0)
                        {
                            process(groupLoopCounter / (maxSameGroupLoop * 1/10));
                        }
                    }
                    if (groupLoopCounter >= maxSameGroupLoop && sameGroupCounter > sameGroupCounterMin && i != -1)
                    {//1000回目以降，所属破り許容最小回数以上の所属破り回数となった場合に組みなおす
                        //所属破り許容最小回数での組み合わせが存在することは明らかなのでもうgroupLoopCounterは更新しない
                        set = new List<ResultSet>();
                        edges = new List<Matching>(edgesOrigin);
                        chosenMatchs = new List<Matching>();
                        takyuChosen = new List<string>();

                        sameGroupCounter = 0;
                        ignoreRankCounter = 0;
                        bridgeCounter = 0;
                        resetCounter = 0;
                        i = -1;
                        //MessageBox.Show("組みなおし回数：" + loopCounter.ToString() + "\nランク制約を無視したマッチングが検出されたので組みなおしてみたコマ");
                    }
                    if (ignoreRankCounter > rankMax && i != -1)
                    {
                        //ランク制約が破られていれば組みなおす
                        set = new List<ResultSet>();
                        edges = new List<Matching>(edgesOrigin);
                        chosenMatchs = new List<Matching>();
                        takyuChosen = new List<string>();
                        if (rankLoopCounter > maxIgnoreRankLoop)
                        {//ランク制約は破らざるを得ない可能性が一番高いので上限値の上昇ペースを指数的(*2^n)に速める
                            if(rankMax == 0)
                            {
                                rankMax = 1;
                            }
                            else
                            {
                                rankMax = rankMax * 2;
                            }
                            
                            rankLoopCounter = 0;
                        }
                        else
                        {
                            rankLoopCounter++;
                        }

                        sameGroupCounter = 0;
                        ignoreRankCounter = 0;
                        bridgeCounter = 0;
                        resetCounter = 0;
                        i = -1;
                        //MessageBox.Show("組みなおし回数：" + loopCounter.ToString() + "\n同所属間のマッチングが検出されたので組みなおしてみたコマ");
                    }
                    if (bridgeCounter > bridgeMax && i != -1)
                    {
                        //リセットされていれば組みなおす
                        set = new List<ResultSet>();
                        edges = new List<Matching>(edgesOrigin);
                        chosenMatchs = new List<Matching>();
                        takyuChosen = new List<string>();
                        if (bridgeLoopCounter > maxBridgeLoop)
                        {
                            bridgeMax++;
                            bridgeLoopCounter = 0;
                        }
                        else
                        {
                            bridgeLoopCounter++;
                        }

                        sameGroupCounter = 0;
                        ignoreRankCounter = 0;
                        bridgeCounter = 0;
                        resetCounter = 0;
                        i = -1;
                        //MessageBox.Show("組みなおし回数：" + loopCounter.ToString() + "\n橋の交換が検出されたので組みなおしてみたコマ");
                    }
                    if (resetCounter > resetMax && i != -1)
                    {//リセットされていれば組みなおす←絶対リセットが起こる(コマ数が多いなど)ときに，理想よりも早いリセットかどうか判別しないと時間の無駄(強制1000回ループがおきてる,早くない理想的なリセットなら検知しない)
                        set = new List<ResultSet>();
                        edges = new List<Matching>(edgesOrigin);
                        chosenMatchs = new List<Matching>();
                        takyuChosen = new List<string>();
                        if (resetLoopCounter > maxResetLoop)
                        {
                            resetMax++;
                            resetLoopCounter = 0;
                        }
                        else
                        {
                            resetLoopCounter++;
                        }

                        sameGroupCounter = 0;
                        ignoreRankCounter = 0;
                        bridgeCounter = 0;
                        resetCounter = 0;
                        i = -1;
                        //MessageBox.Show("組みなおし回数：" + loopCounter.ToString() + "\nリセットが検出されたので組みなおしてみたコマ");
                    }
                    /*else if (loopCounter >= maxLoop && i == coma - 1)
                    {
                        MessageBox.Show("あまり良くない組み合わせが出たかもしれないコマ...\n組みなおしを推奨するコマ");
                    }*/

                }
            }
            //throw new System.ArgumentException("error", "original");
            if (loopStopper == 0)
            {
                //MessageBox.Show(result);
            }
            else//例外
            {
                purupuru = false;
                return "ERROR";
            }

            for(int i = 0; i < set.Count; i++)
            {
                result = result + (i+1).ToString() + "コマ目：\n" + set[i].Result;
            }
            purupuru = false;
            return result;
        }
        public bool haveSameGroup(List<Matching> edges, string name)
        {
            bool b = false;
            foreach (Matching e in edges)
            {
                if (e.Name1 == name || e.Name2 == name)
                {
                    if (e.DifferentGroup == false)
                    {
                        b = true;
                        break;
                    }
                }
            }
            return b;
        }
        //枝集合から枝をもつノード数を返す
        public int NodeNumber(List<Matching> edges)
        {
            List<string> nodes = new List<string>();
            foreach (Matching e in edges)
            {
                if (!nodes.Contains(e.Name1))
                {
                    nodes.Add(e.Name1);
                }
                if (!nodes.Contains(e.Name2))
                {
                    nodes.Add(e.Name2);
                }
            }
            return nodes.Count;
        }
        //最小の次数を返す
        public int MinDegree(List<Matching> edges, List<MemberNode> nodes)
        {
            int degree = nodes.Count - 1;//最小の次数
            int degreePre = 0;
            foreach (MemberNode n in nodes)
            {
                foreach (Matching e in edges)
                {
                    if (e.Name1 == n.Name || e.Name2 == n.Name)
                    {
                        degreePre++;
                    }
                }
                if (degreePre < degree && degreePre != 0)
                {
                    degree = degreePre;
                }
                degreePre = 0;
            }
            return degree;
        }
        //最大の次数を返す<-使用していない
        public int MaxDegree(List<Matching> edges, List<MemberNode> nodes)
        {
            int degree = 0;//最大の次数
            int degreePre = 0;
            foreach (MemberNode n in nodes)
            {
                foreach (Matching e in edges)
                {
                    if (e.Name1 == n.Name || e.Name2 == n.Name)
                    {
                        degreePre++;
                    }
                }
                if (degreePre > degree && degreePre != 0)
                {
                    degree = degreePre;
                }
                degreePre = 0;
            }
            return degree;
        }
        //memberノードの次数を返す
        public int Degree(List<Matching> edges, MemberNode member)
        {
            int degree = 0;
            foreach (Matching e in edges)
            {
                if (e.Name1 == member.Name || e.Name2 == member.Name)
                {
                    degree++;
                }
            }
            return degree;
        }
        public List<MemberNode> HaveEdgeList(List<Matching> edges, List<MemberNode> nodes)
        {//枝を持つノード集合を返す
            List<MemberNode> result = new List<MemberNode>();
            foreach(Matching e in edges)
            {
                MemberNode m1 = nodes.Find(m => m.Name == e.Name1);
                MemberNode m2 = nodes.Find(m => m.Name == e.Name2);
                if(result.IndexOf(m1) == -1)
                {
                    result.Add(m1);
                }
                if(result.IndexOf(m2) == -1)
                {
                    result.Add(m2);
                }
            }
            return result;
        }
        public List<Matching> IsConnected(List<MemberNode> nodes, List<Matching> edges)
        {//現在のグラフが連結かどうか判定する(n^2で妥協)
         //連結ならばnull,非連結ならば橋候補(開始点とlabelが0の点の枝2通り)と開始点とその隣接点の2通りの枝を返す
            //まず，隣接行列を作成(nodesおよびedgesはランクで昇順になっている前提)
            int[,] mat = new int[nodes.Count, nodes.Count];//隣接行列,0で初期化
            foreach(Matching e in edges)
            {
                int indexX = nodes.FindIndex((n => n.Name == e.Name1));
                int indexY = nodes.FindIndex((n => n.Name == e.Name2));
                mat[indexX, indexY] = 1;
                mat[indexY, indexX] = 1;//無向グラフなので対称
            }
            //深さ優先で連結か判定
            int[] label = new int[nodes.Count];
            int nowIndex = 0;
            int indexForResult = 0;
            int flag = 0;
            while (true)
            {
                flag = 0;
                for(int i = 1; i < nodes.Count; i++)
                {//行を探索(i=0は開始点で自明に探索済みなので1から)
                    if(mat[nowIndex,i] == 1 && label[i] == 0)
                    {//未探索の隣接ノード発見
                        if (nowIndex == 0)
                        {//開始点からの隣接ノードを記憶しておく
                            indexForResult = i;
                        }
                        label[i] = label[nowIndex] + 1;
                        nowIndex = i;
                        flag = 1;
                        break;
                    }
                }
                if(flag == 0)
                {//新たな隣接ノードなし
                    if(label[nowIndex] == 0)
                    {//かつ，開始ノードまで戻っている→到達可能なノードを探索済み
                        break;
                    }
                    int nearLabel = 0;
                    int preIndex = 0;
                    for (int i = 0; i < nodes.Count; i++)
                    {//行を探索
                        if (mat[nowIndex, i] == 1 && label[i] < label[nowIndex])
                        {//探索済みの隣接ノードのうちlabelが最も近いものに戻る
                            if(label[i] > nearLabel)
                            {//更新
                                nearLabel = label[i];
                                preIndex = i;
                            }
                        }
                    }
                    nowIndex = preIndex;
                }
            }
            int count = 0;
            for(int i = 0; i < label.Length; i++)
            {//labelが0のノードが2つ以上残っていれば非連結
                if (label[i] == 0)
                {
                    count++;
                }
                if(count >= 2)
                {
                    Matching bridge1 = new Matching(nodes[0].Name, nodes[i].Name, nodes[0].Rank, nodes[i].Rank, nodes[0].Group != nodes[i].Group);
                    Matching bridge2 = new Matching(nodes[i].Name, nodes[0].Name, nodes[i].Rank, nodes[0].Rank, nodes[0].Group != nodes[i].Group);
                    Matching neighbor1 = new Matching(nodes[0].Name, nodes[indexForResult].Name, nodes[0].Rank, nodes[indexForResult].Rank, nodes[0].Group != nodes[indexForResult].Group);
                    Matching neighbor2 = new Matching(nodes[indexForResult].Name, nodes[0].Name, nodes[indexForResult].Rank, nodes[0].Rank, nodes[0].Group != nodes[indexForResult].Group);
                    List<Matching> result = new List<Matching>();
                    result.Add(bridge1);
                    result.Add(bridge2);
                    result.Add(neighbor1);
                    result.Add(neighbor2);
                    return result;
                }
            }
            return null;
        }
        //chosen内の枝を避けながら，アルゴリズムに従った枝選定->アルゴリズムに合致する枝がなくなれば合致しない枝も選定
        //->それもなくなれば同所属間の枝も選定->それもなくなればchosenをリセット
        public ResultSet TopToBottom(List<MemberNode> nodes, List<Matching> edges, List<string> takyuChosen, int table, int now, List<ResultSet> set, List<Matching> chosenMatchs)
        {
            //まず参加人数と台数から対人と多球の組数の方程式を解く
            int p = nodes.Count;//1コマ目の参加人数
            int taijin = -1;//対人組数
            int takyu = -1;//多球組数
            //2taijin+3takyu = p,taijin+takyu<=tをみたしtaijinが最大となるtaijinとtakyuの組み合わせを求める
            if (3 * table - p < 0)
            {
                List<string> er = new List<string>();
                ResultSet error = new ResultSet(edges, er, "error台が足りません");
                return error;
            }
            else if (3 * table - p < p / 2)//taijinの条件は，3table-p以下又はp/2以下　である．この場合3table-p以下．
            {

                for (int i = 3 * table - p; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)//この時点でのiをtaijinとしたとき，残りを多球にできるか.できるならこれがtaijinの最大値
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }
            else//taijinがp/2以下．
            {
                for (int i = p / 2; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }

            //もしも開始時点で,枝数がtaijin数よりも多いにもかかわらずグラフが非連結であった場合，特にメンバが偶数であれば各部分グラフ内で不足なくマッチングするのは不可能．
            //この場合，ある頂点から生える枝(bridge[2]or[3])と，以前に選ばれているはずのその頂点から生える橋(bridge[0]or[1])のマッチングを入れ替える
            if (edges.Count > taijin)
            {
                List<Matching> bridge = IsConnected(nodes, edges);
                if (bridge != null)
                {//非連結
                    //MessageBox.Show("出戻りしたコマお");
                    for (int i = now - 1; i >= 0; i--)
                    {//以前のコマで選ばれた枝と入れ替える
                        string bridge0 = bridge[0].Name1 + " - " + bridge[0].Name2;
                        string bridge1 = bridge[1].Name1 + " - " + bridge[1].Name2;
                        string exchange = bridge[2].Name1 + " - " + bridge[2].Name2;
                        if (set[i].Result.Contains(bridge0) || set[i].Result.Contains(bridge1))
                        {//このコマで橋を選択している
                            string[] sep1 = new string[1];
                            string[] sep2 = new string[1];
                            string[] sep3 = new string[1];
                            string m = null;
                            //exchangeのもう片方のノードと接続していたノードを取得しておく
                            sep1[0] = bridge[2].Name2 + " - ";
                            sep2[0] = " - " + bridge[2].Name2;
                            sep3[0] = "　\n";
                            if (set[i].Result.Contains(sep1[0]))
                            {
                                //増減者の関係で，exchangeのもう片方のノードと接続していたノードが過去に存在しないことなどがある
                                m = ((set[i].Result).Split(sep1, StringSplitOptions.None))[1].Split('\n')[0];
                                
                            }
                            else if (set[i].Result.Contains(sep2[0]))
                            {
                                string[] md = (set[i].Result).Split(sep2, StringSplitOptions.None)[0].Split(sep3, StringSplitOptions.None);
                                m = md[md.Length-1];

                            }
                            if (nodes.Find(mmm => mmm.Name == m) != null)
                            {
                                //まずbridgeと交換するぶんのResultの調整を行う
                                if (set[i].Result.Contains(bridge0))
                                {

                                    set[i].Result = set[i].Result.Replace(bridge0, exchange);
                                }
                                else
                                {

                                    set[i].Result = set[i].Result.Replace(bridge1, exchange);
                                }

                                set[i].Edges.Add(bridge[0]);//橋を消すのでEdgeには残る
                                set[i].Edges.RemoveAll(e => e.Equal(bridge[2]));//橋と入れ替える枝を加えるのでEdgeからは消す
                                edges.Add(bridge[0]);
                                edges.RemoveAll(e => e.Equal(bridge[2]));



                                MemberNode mm = nodes.Find(mmm => mmm.Name == m);
                                //Matchingを作るためにexchangeのもう片方のノードを取得
                                MemberNode em = nodes.Find(mmm => mmm.Name == bridge[2].Name2);
                                Matching exchangeNeighbor = new Matching(em.Name, m, em.Rank, mm.Rank, em.Group != mm.Group);
                                MemberNode bm = nodes.Find(mmm => mmm.Name == bridge[0].Name2);
                                Matching exchangeNeighborToBridge1 = new Matching(m, bm.Name, mm.Rank, bm.Rank, bm.Group != mm.Group);
                                Matching exchangeNeighborToBridge2 = new Matching(bm.Name, m, bm.Rank, mm.Rank, bm.Group != mm.Group);

                                string bridge2 = exchangeNeighbor.Name1 + " - " + exchangeNeighbor.Name2;
                                string bridge3 = exchangeNeighbor.Name2 + " - " + exchangeNeighbor.Name1;
                                string exchange2 = exchangeNeighborToBridge1.Name1 + " - " + exchangeNeighborToBridge1.Name2;
                                if (set[i].Result.Contains(bridge2))
                                {
                                    set[i].Result = set[i].Result.Replace(bridge2, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }
                                else
                                {
                                    set[i].Result = set[i].Result.Replace(bridge3, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }

                                set[i].Edges.Add(exchangeNeighbor);
                                set[i].Edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));
                                edges.Add(exchangeNeighbor);
                                edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));

                                bridgeCounter++;
                                break;
                            }
                            else
                            {//諦めてリセットする
                                //MessageBox.Show("諦めて非連結のまま進行したコマ");
                                break;
                            }
                            
                        }
                    }
                    
                }
            }
            
            string result = "";
            //まずランクの中央値を求める
            List<int> ranks = new List<int>();//中央値算出のため
            int midrank = -1;//ランクの中央値
            foreach (MemberNode s in nodes)
            {
                ranks.Add(s.Rank);
            }
            ranks.Sort();
            bool rankOdd = ranks.Count % 2 == 1;
            if (rankOdd)
            {
                midrank = ranks[(ranks.Count + 1) / 2 - 1];
            }
            else
            {
                midrank = ranks[ranks.Count / 2];
            }
            
            //枝をtaijin本選んだら，残り3takyu個のノードを選ぶ
            //Rank1>=midrank&&Rank2<=midrank || Rank1<=midrank&&Rank2>=midrankを満たす枝を，次数が低いものからランダムに抽出
            Random r = new Random();//乱数
            edges = edges.OrderBy(e => r.Next(edges.Count)).ToList();//edgesをシャッフル
            nodes = nodes.OrderBy(n => r.Next(nodes.Count)).ToList();//nodesをシャッフル
            List<Matching> origin = new List<Matching>(edges);//もとのedgesを値コピー->あとでmatchsの枝を消して返り値へ
            List<MemberNode> haveEdgeNodes = HaveEdgeList(edges, nodes);//開始時点で枝を持つノード，無駄なく選べたかの判定に使用 
            List<Matching> matchs = new List<Matching>();//得られたマッチングを格納->resultへ
            List<string[]> takyuMatchs = new List<string[]>();//得られた多球マッチングを格納->resultへ 
            takyuChosen = takyuChosen;//今までに多球に選ばれた人たち．↑の列挙版を加える


            if (takyu > 0)
            {//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                List<string> current = new List<string>();
                string[] t = new string[3];
                int flag = 0;
                for (int i = 0; i < takyu;)
                {
                    for(int ii = 0; ii < nodes.Count;)
                    {
                        if(takyuChosen.IndexOf(nodes[ii].Name) == -1)
                        {//多球に選ばれていないなら選ぶ
                            t[flag] = nodes[ii].Name;
                            takyuChosen.Add(nodes[ii].Name);
                            current.Add(nodes[ii].Name);
                            flag++;
                        }
                        
                        if(flag == 3)
                        {
                            
                            break;
                        }
                        ii++;
                    }
                    if (flag < 3)
                    {//列挙しても多球に3人選ばれていない＝＞既に多球に全員が選ばれている＝＞リセット
                        takyuChosen = new List<string>();
                        foreach (string s in t)
                        {
                            if (s != null)
                            {
                                takyuChosen.Add(s);
                            }

                        }
                    }
                    else
                    {
                        takyuMatchs.Add(t);
                        t = new string[3];
                        flag = 0;
                        i++;
                    }
                    
                }
                foreach (string s in current)
                {//選んだ人の枝を消す
                    edges.RemoveAll(ee => ee.Name1 == s || ee.Name2 == s);
                    //nodesも消す
                    haveEdgeNodes.RemoveAll(nn => nn.Name == s);
                }
            }
            //リセット時用の完全グラフをloopの外で作成しておく
            List<Matching> compEdges = new List<Matching>();
            Matching edge = new Matching(null, null, 0, 0, false);
            for (int j = 0; j < nodes.Count; j++)
            {
                for (int jj = j + 1; jj < nodes.Count; jj++)
                {
                    if (nodes[j].Group != nodes[jj].Group)
                    {//所属が異なる(＝このマッチングは無考慮に組んでOK)
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, true);
                    }
                    else
                    {
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, false);
                    }
                    compEdges.Add(edge);

                }
            }

            for (int i = 0; i < taijin;)//taijin組の対人が見つかるまで
            {
                int ii = i;//iがforeachを抜けても変化していないならば条件を緩める
                int minD = MinDegree(edges, nodes);//現在の枝の最小次数
                //int maxD = MaxDegree(edges, nodes);//現在の枝の最大次数

                foreach (Matching e in edges)
                {
                    MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                    MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                    int d1 = Degree(edges, node1);
                    int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                    if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                    {
                        //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                        //{
                        if (rankOdd)
                        {
                            if (((e.Rank1 >= midrank && e.Rank2 <= midrank) || (e.Rank1 <= midrank && e.Rank2 >= midrank))
                                && e.DifferentGroup)
                            //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除

                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                        }
                        else
                        {
                            if (((e.Rank1 >= midrank && e.Rank2 < midrank) || (e.Rank1 < midrank && e.Rank2 >= midrank))
                                && e.DifferentGroup)
                            //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                        }
                        //}
                    }
                }
                
                if (ii == i)//条件の枝が見つからない(条件を緩める，ランクの条件をとっぱらう)
                {
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                            //{
                            if (e.DifferentGroup)
                            //条件(所属制約）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                ignoreRankCounter++;

                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                            //}
                        }
                    }
                }
                if (ii == i)
                {//条件の枝が見つからない(部内or所属ナシを対象に再び探索)
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                            //{
                            if (rankOdd)
                            {
                                if (((e.Rank1 >= midrank && e.Rank2 <= midrank) || (e.Rank1 <= midrank && e.Rank2 >= midrank))
                                    && (node1.Name == "" || node2.Name == ""))
                                //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                                //条件はforループごとに徐々に緩める
                                {
                                    edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                    || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除

                                    haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                    matchs.Add(e);
                                    i++;
                                    break;
                                }
                            }
                            else
                            {
                                if (((e.Rank1 >= midrank && e.Rank2 < midrank) || (e.Rank1 < midrank && e.Rank2 >= midrank))
                                    && (node1.Name == "" || node2.Name == ""))
                                //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                                //条件はforループごとに徐々に緩める
                                {
                                    edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                    || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                    haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                    matchs.Add(e);
                                    i++;
                                    break;
                                }
                            }
                            //}
                        }
                    }
                }
                if (ii == i)//条件の枝が見つからない(条件を緩める，すべてとっぱらう)
                {
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            if (node1.Group != "" || node2.Group != "")
                            {
                                sameGroupCounter++;
                            }
                            ignoreRankCounter++;
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため,枝を持つノード数=処理していない人数
                            //{
                                //条件なし
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            //}
                        }
                    }
                }
                if (ii == i)//これでも見つからないならば枝の選定状況をリセット(完全グラフから，このコマで既に選んだ枝のみを除いたグラフにedgesを置き換える)
                            //次数の低いノードから選択しているのでこの状況になるのは単純に残る枝数が足りていないときだと思われる
                {
                    //MessageBox.Show("リセットしたコマ");
                    if(haveEdgeNodes.Count > 0)
                    {//開始時点で枝を持っていたノードが全て選択されている(Count==0)ならば無駄なく選んだうえでリセットせざるを得ないので許容
                        resetCounter++;
                    }
                    
                    //完全グラフに置き換え
                    edges = new List<Matching>(compEdges);
                    chosenMatchs = new List<Matching>();
                    //originも完全グラフに更新
                    origin = new List<Matching>(edges);

                    //このコマで既に選んだ枝の両端ノードが関与する枝を除く(同じコマで複数同じ人が出現することを防ぐため)
                    List<Matching> edgesCopy = new List<Matching>(edges);
                    foreach (Matching m in matchs)
                    {
                        edges.RemoveAll(e => e.Name1 == m.Name1 || e.Name1 == m.Name2 || e.Name2 == m.Name1 || e.Name2 == m.Name2);

                        chosenMatchs.Add(m);
                    }
                    //多球の選出状況もリセット
                    takyuChosen = new List<string>();
                    foreach(string[] t in takyuMatchs)
                    {
                        foreach(string tt in t)
                        {
                            takyuChosen.Add(tt);
                        }
                    }
                }
            }
            //nodesとorigin(返り値に使用)からマッチングしたメンバーを消す
            foreach (Matching m in matchs)
            {
                nodes.RemoveAll(hoge => hoge.Name == m.Name1 || hoge.Name == m.Name2);
                origin.RemoveAll(ed => ed.Equal(m));
            }
            //以下，返り値処理
            if (takyu == 0)
            {
                //多球がないときはそのまま返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
            }
            else
            {
                /*if (nodes.Count % 3 != 0)
                {
                    throw new System.ArgumentException("残った多球用ノードが3の倍数になってない", "original");
                }
                else//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                {
                    for (int i = 0; i < nodes.Count / 3; i = i++)
                    {
                        string[] t = new string[] { nodes[0].Name, nodes[1].Name, nodes[2].Name };
                        takyuMatchs.Add(t);
                        foreach (string n in t)
                        {
                            takyuChosen.Add(n);
                        }
                        nodes.RemoveRange(0, 3);
                    }
                }*/

                //返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
                foreach (string[] s in takyuMatchs)
                {
                    result = result + "    多球: " + s[0] + "-" + s[1] + "-" + s[2] + "\n";
                }
            }
            ResultSet outPut = new ResultSet(origin, takyuChosen, result);
            return outPut;

        }

        public ResultSet Near(List<MemberNode> nodes, List<Matching> edges, List<string> takyuChosen, int table, int now, List<ResultSet> set, List<Matching> chosenMatchs)
        {
            //まず参加人数と台数から対人と多球の組数の方程式を解く
            int p = nodes.Count;//1コマ目の参加人数
            int taijin = -1;//対人組数
            int takyu = -1;//多球組数
            //2taijin+3takyu = p,taijin+takyu<=tをみたしtaijinが最大となるtaijinとtakyuの組み合わせを求める
            if (3 * table - p < 0)
            {
                List<string> er = new List<string>();
                ResultSet error = new ResultSet(edges, er, "error台が足りません");
                return error;
            }
            else if (3 * table - p < p / 2)//taijinの条件は，3table-p以下又はp/2以下　である．この場合3table-p以下．
            {

                for (int i = 3 * table - p; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)//この時点でのiをtaijinとしたとき，残りを多球にできるか.できるならこれがtaijinの最大値
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }
            else//taijinがp/2以下．
            {
                for (int i = p / 2; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }

            //もしも開始時点で,枝数がtaijin数よりも多いにもかかわらずグラフが非連結であった場合，特にメンバが偶数であれば各部分グラフ内で不足なくマッチングするのは不可能．
            //この場合，ある頂点から生える枝(bridge[2]or[3])と，以前に選ばれているはずのその頂点から生える橋(bridge[0]or[1])のマッチングを入れ替える
            if (edges.Count > taijin)
            {
                List<Matching> bridge = IsConnected(nodes, edges);
                if (bridge != null)
                {//非連結
                    //MessageBox.Show("出戻りしたコマお");
                    for (int i = now - 1; i >= 0; i--)
                    {//以前のコマで選ばれた枝と入れ替える
                        string bridge0 = bridge[0].Name1 + " - " + bridge[0].Name2;
                        string bridge1 = bridge[1].Name1 + " - " + bridge[1].Name2;
                        string exchange = bridge[2].Name1 + " - " + bridge[2].Name2;
                        if (set[i].Result.Contains(bridge0) || set[i].Result.Contains(bridge1))
                        {//このコマで橋を選択している
                            string[] sep1 = new string[1];
                            string[] sep2 = new string[1];
                            string[] sep3 = new string[1];
                            string m = null;
                            //exchangeのもう片方のノードと接続していたノードを取得しておく
                            sep1[0] = bridge[2].Name2 + " - ";
                            sep2[0] = " - " + bridge[2].Name2;
                            sep3[0] = "　\n";
                            if (set[i].Result.Contains(sep1[0]))
                            {
                                //増減者の関係で，exchangeのもう片方のノードと接続していたノードが過去に存在しないことなどがある
                                m = ((set[i].Result).Split(sep1, StringSplitOptions.None))[1].Split('\n')[0];

                            }
                            else if (set[i].Result.Contains(sep2[0]))
                            {
                                string[] md = (set[i].Result).Split(sep2, StringSplitOptions.None)[0].Split(sep3, StringSplitOptions.None);
                                m = md[md.Length - 1];

                            }
                            if (nodes.Find(mmm => mmm.Name == m) != null)
                            {
                                //まずbridgeと交換するぶんのResultの調整を行う
                                if (set[i].Result.Contains(bridge0))
                                {

                                    set[i].Result = set[i].Result.Replace(bridge0, exchange);
                                }
                                else
                                {

                                    set[i].Result = set[i].Result.Replace(bridge1, exchange);
                                }

                                set[i].Edges.Add(bridge[0]);//橋を消すのでEdgeには残る
                                set[i].Edges.RemoveAll(e => e.Equal(bridge[2]));//橋と入れ替える枝を加えるのでEdgeからは消す
                                edges.Add(bridge[0]);
                                edges.RemoveAll(e => e.Equal(bridge[2]));



                                MemberNode mm = nodes.Find(mmm => mmm.Name == m);
                                //Matchingを作るためにexchangeのもう片方のノードを取得
                                MemberNode em = nodes.Find(mmm => mmm.Name == bridge[2].Name2);
                                Matching exchangeNeighbor = new Matching(em.Name, m, em.Rank, mm.Rank, em.Group != mm.Group);
                                MemberNode bm = nodes.Find(mmm => mmm.Name == bridge[0].Name2);
                                Matching exchangeNeighborToBridge1 = new Matching(m, bm.Name, mm.Rank, bm.Rank, bm.Group != mm.Group);
                                Matching exchangeNeighborToBridge2 = new Matching(bm.Name, m, bm.Rank, mm.Rank, bm.Group != mm.Group);

                                string bridge2 = exchangeNeighbor.Name1 + " - " + exchangeNeighbor.Name2;
                                string bridge3 = exchangeNeighbor.Name2 + " - " + exchangeNeighbor.Name1;
                                string exchange2 = exchangeNeighborToBridge1.Name1 + " - " + exchangeNeighborToBridge1.Name2;
                                if (set[i].Result.Contains(bridge2))
                                {
                                    set[i].Result = set[i].Result.Replace(bridge2, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }
                                else
                                {
                                    set[i].Result = set[i].Result.Replace(bridge3, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }

                                set[i].Edges.Add(exchangeNeighbor);
                                set[i].Edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));
                                edges.Add(exchangeNeighbor);
                                edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));

                                bridgeCounter++;
                                break;
                            }
                            else
                            {//諦めてリセットする
                                //MessageBox.Show("諦めて非連結のまま進行したコマ");
                                break;
                            }

                        }
                    }

                }
            }

            string result = "";
            //まずランクの中央値を求める
            List<int> ranks = new List<int>();//中央値算出のため
            int midrank = -1;//ランクの中央値
            foreach (MemberNode s in nodes)
            {
                ranks.Add(s.Rank);
            }
            ranks.Sort();
            bool rankOdd = ranks.Count % 2 == 1;
            if (rankOdd)
            {
                midrank = ranks[(ranks.Count + 1) / 2 - 1];
            }
            else
            {
                midrank = ranks[ranks.Count / 2];
            }

            //枝をtaijin本選んだら，残り3takyu個のノードを選ぶ
            //Rank1>=midrank&&Rank2<=midrank || Rank1<=midrank&&Rank2>=midrankを満たす枝を，次数が低いものからランダムに抽出
            Random r = new Random();//乱数
            edges = edges.OrderBy(e => r.Next(edges.Count)).ToList();//edgesをシャッフル
            nodes = nodes.OrderBy(n => r.Next(nodes.Count)).ToList();//nodesをシャッフル
            List<Matching> origin = new List<Matching>(edges);//もとのedgesを値コピー->あとでmatchsの枝を消して返り値へ
            List<MemberNode> haveEdgeNodes = HaveEdgeList(edges, nodes);//開始時点で枝を持つノード，無駄なく選べたかの判定に使用 
            List<Matching> matchs = new List<Matching>();//得られたマッチングを格納->resultへ
            List<string[]> takyuMatchs = new List<string[]>();//得られた多球マッチングを格納->resultへ 
            takyuChosen = takyuChosen;//今までに多球に選ばれた人たち．↑の列挙版を加える


            if (takyu > 0)
            {//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                List<string> current = new List<string>();
                string[] t = new string[3];
                int flag = 0;
                for (int i = 0; i < takyu;)
                {
                    for (int ii = 0; ii < nodes.Count;)
                    {
                        if (takyuChosen.IndexOf(nodes[ii].Name) == -1)
                        {//多球に選ばれていないなら選ぶ
                            t[flag] = nodes[ii].Name;
                            takyuChosen.Add(nodes[ii].Name);
                            current.Add(nodes[ii].Name);
                            flag++;
                        }

                        if (flag == 3)
                        {

                            break;
                        }
                        ii++;
                    }
                    if (flag < 3)
                    {//列挙しても多球に3人選ばれていない＝＞既に多球に全員が選ばれている＝＞リセット
                        takyuChosen = new List<string>();
                        foreach (string s in t)
                        {
                            if (s != null)
                            {
                                takyuChosen.Add(s);
                            }

                        }
                    }
                    else
                    {
                        takyuMatchs.Add(t);
                        t = new string[3];
                        flag = 0;
                        i++;
                    }

                }
                foreach (string s in current)
                {//選んだ人の枝を消す
                    edges.RemoveAll(ee => ee.Name1 == s || ee.Name2 == s);
                    //nodesも消す
                    haveEdgeNodes.RemoveAll(nn => nn.Name == s);
                }
            }
            //リセット時用の完全グラフをloopの外で作成しておく
            List<Matching> compEdges = new List<Matching>();
            Matching edge = new Matching(null, null, 0, 0, false);
            for (int j = 0; j < nodes.Count; j++)
            {
                for (int jj = j + 1; jj < nodes.Count; jj++)
                {
                    if (nodes[j].Group != nodes[jj].Group)
                    {//所属が異なる(＝このマッチングは無考慮に組んでOK)
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, true);
                    }
                    else
                    {
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, false);
                    }
                    compEdges.Add(edge);

                }
            }

            for (int i = 0; i < taijin;)//taijin組の対人が見つかるまで
            {
                int ii = i;//iがforeachを抜けても変化していないならば条件を緩める
                int minD = MinDegree(edges, nodes);//現在の枝の最小次数
                //int maxD = MaxDegree(edges, nodes);//現在の枝の最大次数

                foreach (Matching e in edges)
                {
                    MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                    MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                    int d1 = Degree(edges, node1);
                    int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                    if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                    {
                        //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                        //{
                        if (rankOdd)
                        {
                            if (((e.Rank1 >= midrank && e.Rank2 >= midrank) || (e.Rank1 <= midrank && e.Rank2 <= midrank))
                                && e.DifferentGroup)
                            //条件(近い,所属が違う）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除

                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                        }
                        else
                        {
                            if (((e.Rank1 >= midrank && e.Rank2 >= midrank) || (e.Rank1 < midrank && e.Rank2 < midrank))
                                && e.DifferentGroup)
                            //条件(近い,所属が違う）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                        }
                        //}
                    }
                }

                if (ii == i)//条件の枝が見つからない(条件を緩める，ランクの条件をとっぱらう)
                {
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                            //{
                            if (e.DifferentGroup)
                            //条件(所属制約）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                ignoreRankCounter++;

                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                            //}
                        }
                    }
                }
                if (ii == i)
                {//条件の枝が見つからない(部内or所属ナシを対象に再び探索)
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                            //{
                            if (rankOdd)
                            {
                                if (((e.Rank1 >= midrank && e.Rank2 >= midrank) || (e.Rank1 <= midrank && e.Rank2 <= midrank))
                                    && (node1.Name == "" || node2.Name == ""))
                                //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                                //条件はforループごとに徐々に緩める
                                {
                                    edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                    || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除

                                    haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                    matchs.Add(e);
                                    i++;
                                    break;
                                }
                            }
                            else
                            {
                                if (((e.Rank1 >= midrank && e.Rank2 >= midrank) || (e.Rank1 < midrank && e.Rank2 < midrank))
                                    && (node1.Name == "" || node2.Name == ""))
                                //条件(上位＜＝＞下位,所属が違う）に合う枝を見つけたら採用)
                                //条件はforループごとに徐々に緩める
                                {
                                    edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                    || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                    haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                    matchs.Add(e);
                                    i++;
                                    break;
                                }
                            }
                            //}
                        }
                    }
                }
                if (ii == i)//条件の枝が見つからない(条件を緩める，すべてとっぱらう)
                {
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            if (node1.Group != "" || node2.Group != "")
                            {
                                sameGroupCounter++;
                            }
                            ignoreRankCounter++;
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため,枝を持つノード数=処理していない人数
                            //{
                            //条件なし
                            edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                            || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                            haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                            matchs.Add(e);
                            i++;
                            break;
                            //}
                        }
                    }
                }
                if (ii == i)//これでも見つからないならば枝の選定状況をリセット(完全グラフから，このコマで既に選んだ枝のみを除いたグラフにedgesを置き換える)
                            //次数の低いノードから選択しているのでこの状況になるのは単純に残る枝数が足りていないときだと思われる
                {
                    //MessageBox.Show("リセットしたコマ");
                    if (haveEdgeNodes.Count > 0)
                    {//開始時点で枝を持っていたノードが全て選択されている(Count==0)ならば無駄なく選んだうえでリセットせざるを得ないので許容
                        resetCounter++;
                    }

                    //完全グラフに置き換え
                    edges = new List<Matching>(compEdges);
                    chosenMatchs = new List<Matching>();
                    //originも完全グラフに更新
                    origin = new List<Matching>(edges);

                    //このコマで既に選んだ枝の両端ノードが関与する枝を除く(同じコマで複数同じ人が出現することを防ぐため)
                    List<Matching> edgesCopy = new List<Matching>(edges);
                    foreach (Matching m in matchs)
                    {
                        edges.RemoveAll(e => e.Name1 == m.Name1 || e.Name1 == m.Name2 || e.Name2 == m.Name1 || e.Name2 == m.Name2);

                        chosenMatchs.Add(m);
                    }
                    //多球の選出状況もリセット
                    takyuChosen = new List<string>();
                    foreach (string[] t in takyuMatchs)
                    {
                        foreach (string tt in t)
                        {
                            takyuChosen.Add(tt);
                        }
                    }
                }
            }
            //nodesとorigin(返り値に使用)からマッチングしたメンバーを消す
            foreach (Matching m in matchs)
            {
                nodes.RemoveAll(hoge => hoge.Name == m.Name1 || hoge.Name == m.Name2);
                origin.RemoveAll(ed => ed.Equal(m));
            }
            //以下，返り値処理
            if (takyu == 0)
            {
                //多球がないときはそのまま返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
            }
            else
            {
                /*if (nodes.Count % 3 != 0)
                {
                    throw new System.ArgumentException("残った多球用ノードが3の倍数になってない", "original");
                }
                else//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                {
                    for (int i = 0; i < nodes.Count / 3; i = i++)
                    {
                        string[] t = new string[] { nodes[0].Name, nodes[1].Name, nodes[2].Name };
                        takyuMatchs.Add(t);
                        foreach (string n in t)
                        {
                            takyuChosen.Add(n);
                        }
                        nodes.RemoveRange(0, 3);
                    }
                }*/

                //返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
                foreach (string[] s in takyuMatchs)
                {
                    result = result + "    多球: " + s[0] + "-" + s[1] + "-" + s[2] + "\n";
                }
            }
            ResultSet outPut = new ResultSet(origin, takyuChosen, result);
            return outPut;
        }

        public ResultSet Random(List<MemberNode> nodes, List<Matching> edges, List<string> takyuChosen, int table, int now, List<ResultSet> set, List<Matching> chosenMatchs)
        {
            //まず参加人数と台数から対人と多球の組数の方程式を解く
            int p = nodes.Count;//1コマ目の参加人数
            int taijin = -1;//対人組数
            int takyu = -1;//多球組数
            //2taijin+3takyu = p,taijin+takyu<=tをみたしtaijinが最大となるtaijinとtakyuの組み合わせを求める
            if (3 * table - p < 0)
            {
                List<string> er = new List<string>();
                ResultSet error = new ResultSet(edges, er, "error台が足りません");
                return error;
            }
            else if (3 * table - p < p / 2)//taijinの条件は，3table-p以下又はp/2以下　である．この場合3table-p以下．
            {

                for (int i = 3 * table - p; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)//この時点でのiをtaijinとしたとき，残りを多球にできるか.できるならこれがtaijinの最大値
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }
            else//taijinがp/2以下．
            {
                for (int i = p / 2; i >= 0; i--)
                {
                    if ((p - 2 * i) % 3 == 0)
                    {
                        taijin = i;
                        takyu = (p - 2 * i) / 3;
                        break;
                    }
                }
            }

            //もしも開始時点で,枝数がtaijin数よりも多いにもかかわらずグラフが非連結であった場合，特にメンバが偶数であれば各部分グラフ内で不足なくマッチングするのは不可能．
            //この場合，ある頂点から生える枝(bridge[2]or[3])と，以前に選ばれているはずのその頂点から生える橋(bridge[0]or[1])のマッチングを入れ替える
            if (edges.Count > taijin)
            {
                List<Matching> bridge = IsConnected(nodes, edges);
                if (bridge != null)
                {//非連結
                    //MessageBox.Show("出戻りしたコマお");
                    for (int i = now - 1; i >= 0; i--)
                    {//以前のコマで選ばれた枝と入れ替える
                        string bridge0 = bridge[0].Name1 + " - " + bridge[0].Name2;
                        string bridge1 = bridge[1].Name1 + " - " + bridge[1].Name2;
                        string exchange = bridge[2].Name1 + " - " + bridge[2].Name2;
                        if (set[i].Result.Contains(bridge0) || set[i].Result.Contains(bridge1))
                        {//このコマで橋を選択している
                            string[] sep1 = new string[1];
                            string[] sep2 = new string[1];
                            string[] sep3 = new string[1];
                            string m = null;
                            //exchangeのもう片方のノードと接続していたノードを取得しておく
                            sep1[0] = bridge[2].Name2 + " - ";
                            sep2[0] = " - " + bridge[2].Name2;
                            sep3[0] = "　\n";
                            if (set[i].Result.Contains(sep1[0]))
                            {
                                //増減者の関係で，exchangeのもう片方のノードと接続していたノードが過去に存在しないことなどがある
                                m = ((set[i].Result).Split(sep1, StringSplitOptions.None))[1].Split('\n')[0];

                            }
                            else if (set[i].Result.Contains(sep2[0]))
                            {
                                string[] md = (set[i].Result).Split(sep2, StringSplitOptions.None)[0].Split(sep3, StringSplitOptions.None);
                                m = md[md.Length - 1];

                            }
                            if (nodes.Find(mmm => mmm.Name == m) != null)
                            {
                                //まずbridgeと交換するぶんのResultの調整を行う
                                if (set[i].Result.Contains(bridge0))
                                {

                                    set[i].Result = set[i].Result.Replace(bridge0, exchange);
                                }
                                else
                                {

                                    set[i].Result = set[i].Result.Replace(bridge1, exchange);
                                }

                                set[i].Edges.Add(bridge[0]);//橋を消すのでEdgeには残る
                                set[i].Edges.RemoveAll(e => e.Equal(bridge[2]));//橋と入れ替える枝を加えるのでEdgeからは消す
                                edges.Add(bridge[0]);
                                edges.RemoveAll(e => e.Equal(bridge[2]));



                                MemberNode mm = nodes.Find(mmm => mmm.Name == m);
                                //Matchingを作るためにexchangeのもう片方のノードを取得
                                MemberNode em = nodes.Find(mmm => mmm.Name == bridge[2].Name2);
                                Matching exchangeNeighbor = new Matching(em.Name, m, em.Rank, mm.Rank, em.Group != mm.Group);
                                MemberNode bm = nodes.Find(mmm => mmm.Name == bridge[0].Name2);
                                Matching exchangeNeighborToBridge1 = new Matching(m, bm.Name, mm.Rank, bm.Rank, bm.Group != mm.Group);
                                Matching exchangeNeighborToBridge2 = new Matching(bm.Name, m, bm.Rank, mm.Rank, bm.Group != mm.Group);

                                string bridge2 = exchangeNeighbor.Name1 + " - " + exchangeNeighbor.Name2;
                                string bridge3 = exchangeNeighbor.Name2 + " - " + exchangeNeighbor.Name1;
                                string exchange2 = exchangeNeighborToBridge1.Name1 + " - " + exchangeNeighborToBridge1.Name2;
                                if (set[i].Result.Contains(bridge2))
                                {
                                    set[i].Result = set[i].Result.Replace(bridge2, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }
                                else
                                {
                                    set[i].Result = set[i].Result.Replace(bridge3, exchange2);
                                    if (!exchangeNeighborToBridge1.DifferentGroup)
                                    {
                                        sameGroupCounter++;
                                    }
                                }

                                set[i].Edges.Add(exchangeNeighbor);
                                set[i].Edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));
                                edges.Add(exchangeNeighbor);
                                edges.RemoveAll(e => e.Equal(exchangeNeighborToBridge1));

                                bridgeCounter++;
                                break;
                            }
                            else
                            {//諦めてリセットする
                                //MessageBox.Show("諦めて非連結のまま進行したコマ");
                                break;
                            }

                        }
                    }

                }
            }

            string result = "";
            //まずランクの中央値を求める
            List<int> ranks = new List<int>();//中央値算出のため
            int midrank = -1;//ランクの中央値
            foreach (MemberNode s in nodes)
            {
                ranks.Add(s.Rank);
            }
            ranks.Sort();
            bool rankOdd = ranks.Count % 2 == 1;
            if (rankOdd)
            {
                midrank = ranks[(ranks.Count + 1) / 2 - 1];
            }
            else
            {
                midrank = ranks[ranks.Count / 2];
            }

            //枝をtaijin本選んだら，残り3takyu個のノードを選ぶ
            //Rank1>=midrank&&Rank2<=midrank || Rank1<=midrank&&Rank2>=midrankを満たす枝を，次数が低いものからランダムに抽出
            Random r = new Random();//乱数
            edges = edges.OrderBy(e => r.Next(edges.Count)).ToList();//edgesをシャッフル
            nodes = nodes.OrderBy(n => r.Next(nodes.Count)).ToList();//nodesをシャッフル
            List<Matching> origin = new List<Matching>(edges);//もとのedgesを値コピー->あとでmatchsの枝を消して返り値へ
            List<MemberNode> haveEdgeNodes = HaveEdgeList(edges, nodes);//開始時点で枝を持つノード，無駄なく選べたかの判定に使用 
            List<Matching> matchs = new List<Matching>();//得られたマッチングを格納->resultへ
            List<string[]> takyuMatchs = new List<string[]>();//得られた多球マッチングを格納->resultへ 
            takyuChosen = takyuChosen;//今までに多球に選ばれた人たち．↑の列挙版を加える


            if (takyu > 0)
            {//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                List<string> current = new List<string>();
                string[] t = new string[3];
                int flag = 0;
                for (int i = 0; i < takyu;)
                {
                    for (int ii = 0; ii < nodes.Count;)
                    {
                        if (takyuChosen.IndexOf(nodes[ii].Name) == -1)
                        {//多球に選ばれていないなら選ぶ
                            t[flag] = nodes[ii].Name;
                            takyuChosen.Add(nodes[ii].Name);
                            current.Add(nodes[ii].Name);
                            flag++;
                        }

                        if (flag == 3)
                        {

                            break;
                        }
                        ii++;
                    }
                    if (flag < 3)
                    {//列挙しても多球に3人選ばれていない＝＞既に多球に全員が選ばれている＝＞リセット
                        takyuChosen = new List<string>();
                        foreach (string s in t)
                        {
                            if (s != null)
                            {
                                takyuChosen.Add(s);
                            }

                        }
                    }
                    else
                    {
                        takyuMatchs.Add(t);
                        t = new string[3];
                        flag = 0;
                        i++;
                    }

                }
                foreach (string s in current)
                {//選んだ人の枝を消す
                    edges.RemoveAll(ee => ee.Name1 == s || ee.Name2 == s);
                    //nodesも消す
                    haveEdgeNodes.RemoveAll(nn => nn.Name == s);
                }
            }
            //リセット時用の完全グラフをloopの外で作成しておく
            List<Matching> compEdges = new List<Matching>();
            Matching edge = new Matching(null, null, 0, 0, false);
            for (int j = 0; j < nodes.Count; j++)
            {
                for (int jj = j + 1; jj < nodes.Count; jj++)
                {
                    if (nodes[j].Group != nodes[jj].Group)
                    {//所属が異なる(＝このマッチングは無考慮に組んでOK)
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, true);
                    }
                    else
                    {
                        edge = new Matching(nodes[j].Name, nodes[jj].Name, nodes[j].Rank, nodes[jj].Rank, false);
                    }
                    compEdges.Add(edge);

                }
            }

            for (int i = 0; i < taijin;)//taijin組の対人が見つかるまで
            {
                int ii = i;//iがforeachを抜けても変化していないならば条件を緩める
                int minD = MinDegree(edges, nodes);//現在の枝の最小次数
                                                   //int maxD = MaxDegree(edges, nodes);//現在の枝の最大次数

                foreach (Matching e in edges)
                {
                    MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                    MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                    int d1 = Degree(edges, node1);
                    int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                    if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                    {
                        //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                        //{
                        if (e.DifferentGroup)
                        //条件(所属制約）に合う枝を見つけたら採用)
                        //条件はforループごとに徐々に緩める
                        {
                            edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                            || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                            haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                            matchs.Add(e);
                            i++;
                            break;
                        }
                        //}
                    }
                }

                if (ii == i)
                {//条件の枝が見つからない(部内or所属ナシを対象に再び探索)
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため
                            //{
                            if ((node1.Name == "" || node2.Name == ""))
                            //条件(所属制約）に合う枝を見つけたら採用)
                            //条件はforループごとに徐々に緩める
                            {
                                edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                                || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                                haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                                matchs.Add(e);
                                i++;
                                break;
                            }
                            //}
                        }
                    }
                }
                if (ii == i)//条件の枝が見つからない(条件を緩める，すべてとっぱらう)
                {
                    foreach (Matching e in edges)
                    {
                        MemberNode node1 = nodes.Find(m => m.Name == e.Name1);
                        MemberNode node2 = nodes.Find(m => m.Name == e.Name2);

                        int d1 = Degree(edges, node1);
                        int d2 = Degree(edges, node2);//枝eの両端の次数を算出
                        if (d1 == minD || d2 == minD)//枝eが最小次数ならば
                        {
                            if (node1.Group != "" || node2.Group != "")
                            {
                                sameGroupCounter++;
                            }
                            //if (NodeNumber(edges) > 3 * takyu)//選んだ枝のノードが関与する枝はすべて削除するため,枝を持つノード数=処理していない人数
                            //{
                            //条件なし
                            edges.RemoveAll(hoge => hoge.Name1 == e.Name1 || hoge.Name2 == e.Name1
                                            || hoge.Name1 == e.Name2 || hoge.Name2 == e.Name2);//採用された枝の2ノードが関与する枝はすべて削除
                            haveEdgeNodes.RemoveAll(nn => nn.Name == e.Name1 || nn.Name == e.Name2);
                            matchs.Add(e);
                            i++;
                            break;
                            //}
                        }
                    }
                }
                if (ii == i)//これでも見つからないならば枝の選定状況をリセット(完全グラフから，このコマで既に選んだ枝のみを除いたグラフにedgesを置き換える)
                            //次数の低いノードから選択しているのでこの状況になるのは単純に残る枝数が足りていないときだと思われる
                {
                    //MessageBox.Show("リセットしたコマ");
                    if (haveEdgeNodes.Count > 0)
                    {//開始時点で枝を持っていたノードが全て選択されている(Count==0)ならば無駄なく選んだうえでリセットせざるを得ないので許容
                        resetCounter++;
                    }

                    //完全グラフに置き換え
                    edges = new List<Matching>(compEdges);
                    chosenMatchs = new List<Matching>();
                    //originも完全グラフに更新
                    origin = new List<Matching>(edges);

                    //このコマで既に選んだ枝の両端ノードが関与する枝を除く(同じコマで複数同じ人が出現することを防ぐため)
                    List<Matching> edgesCopy = new List<Matching>(edges);
                    foreach (Matching m in matchs)
                    {
                        edges.RemoveAll(e => e.Name1 == m.Name1 || e.Name1 == m.Name2 || e.Name2 == m.Name1 || e.Name2 == m.Name2);

                        chosenMatchs.Add(m);
                    }
                    //多球の選出状況もリセット
                    takyuChosen = new List<string>();
                    foreach (string[] t in takyuMatchs)
                    {
                        foreach (string tt in t)
                        {
                            takyuChosen.Add(tt);
                        }
                    }
                }
            }
            //nodesとorigin(返り値に使用)からマッチングしたメンバーを消す
            foreach (Matching m in matchs)
            {
                nodes.RemoveAll(hoge => hoge.Name == m.Name1 || hoge.Name == m.Name2);
                origin.RemoveAll(ed => ed.Equal(m));
            }
            //以下，返り値処理
            if (takyu == 0)
            {
                //多球がないときはそのまま返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
            }
            else
            {
                /*if (nodes.Count % 3 != 0)
                {
                    throw new System.ArgumentException("残った多球用ノードが3の倍数になってない", "original");
                }
                else//多球メンバを3人ずつえらぶ．既にシャッフルしてるので頭からでよい
                {
                    for (int i = 0; i < nodes.Count / 3; i = i++)
                    {
                        string[] t = new string[] { nodes[0].Name, nodes[1].Name, nodes[2].Name };
                        takyuMatchs.Add(t);
                        foreach (string n in t)
                        {
                            takyuChosen.Add(n);
                        }
                        nodes.RemoveRange(0, 3);
                    }
                }*/

                //返り値処理
                foreach (Matching m in matchs)
                {
                    result = result + m.Pair();
                    chosenMatchs.Add(m);
                }
                foreach (string[] s in takyuMatchs)
                {
                    result = result + "    多球: " + s[0] + "-" + s[1] + "-" + s[2] + "\n";
                }
            }
            ResultSet outPut = new ResultSet(origin, takyuChosen, result);
            return outPut;

        }
    }


    public class MemberNode//参加者を表すノードの構造体
    {
        public int Rank { set; get; }
        public string Name { set; get; }
        public string Group { set; get; }//部内者は"",外部参加者で所属分けする場合は末尾3文字を読みアルファベットを格納
        public int Degree { set; get; }

        public MemberNode(int Rank, string Name, string Group)
        {
            this.Rank = Rank;
            this.Name = Name;
            this.Group = Group;
        }

    }
    public class Matching//参加間のマッチングを表す枝の構造体
    {
        public string Name1 { set; get; }
        public string Name2 { set; get; }
        public int Rank1 { set; get; }
        public int Rank2 { set; get; }
        public bool DifferentGroup { set; get; }//2端の所属が異なるか，どちらかが""のときtrue

        public Matching(string Name1, string Name2, int Rank1, int Rank2, bool DifferentGroup)
        {
            this.Name1 = Name1;
            this.Name2 = Name2;
            this.Rank1 = Rank1;
            this.Rank2 = Rank2;
            this.DifferentGroup = DifferentGroup;
        }
        public bool Equal(Matching M)
        {
            if (this.Name1 == M.Name1 && this.Name2 == M.Name2 && this.Rank1 == M.Rank1 && this.Rank2 == M.Rank2 && this.DifferentGroup == M.DifferentGroup)
            {
                return true;
            }
            else if (this.Name1 == M.Name2 && this.Name2 == M.Name1 && this.Rank1 == M.Rank2 && this.Rank2 == M.Rank1 && this.DifferentGroup == M.DifferentGroup)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string Pair()
        {
            return "    " + this.Name1 + " - " + this.Name2 + "\n";
        }
    }
    public class ResultSet//アルゴリズムに沿った枝集合と組み合わせのstringをまとめた出力に使用
    {
        public List<Matching> Edges { set; get; }//枝を抜く前のもとの枝集合から，選んだマッチング枝のみを削除したもの
        public List<Matching> ChosenEdges { set; get; }//今までに選んだマッチング，枝のリセットとともにリセット
        public List<string> TakyuChosen { set; get; }//多球に選ばれたメンバ
        public string Result { set; get; }//そのコマの組み合わせ

        public ResultSet(List<Matching> Edges, List<string> TakyuChosen, string Result)
        {
            this.Edges = Edges;
            this.TakyuChosen = TakyuChosen;
            this.Result = Result;
        }
    }
}
