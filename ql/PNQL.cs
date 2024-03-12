using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace Huangbo.RLPetri
{

    /// <summary>
    /// Base class for pathfinding nodes, it holds no actual information about the map. 
    /// An inherited class must be constructed from this class and all virtual methods must be 
    /// implemented. Note, that calling base() in the overridden methods is not needed.
    /// </summary>
 
    public class PNQLNode//A state node in the reachability graph of a Petri net
    {
        #region Properties

        //属性，通常首字母大写。外类对此类非静态属性可通过对象.属性访问；如果是静态static，则通过类.属性访问
        public PNQLNode Parent//parent node
        {
            set
            {
                _Parent = value;
            }
            get
            {
                return _Parent;
            }
        }
        private PNQLNode _Parent;//私有变量成员，通常前面加_，会被派生类继承


        public PNQLNode GoalNode //goal node
        {
            set
            {
                _GoalNode = value;
            }
            get
            {
                return _GoalNode;
            }
        }
        private PNQLNode _GoalNode;


        public decimal gValue //g value of a node (The accumulative cost of the path until now.)
        {
            set
            {
                _gValue = value;
            }
            get
            {
                return _gValue;
            }
        }
        private decimal _gValue;

      

       
        public int[] M //the marking of node in the reachability graph
        {
            get
            {
                return _M;
            }
        }
        private int[] _M;



        public decimal[,] R //the remaining token time matrix of a place-timed Petri net
        {
            get
            {
                return _R;
            }
        }
        private decimal[,] _R;



        public int tFireFrom //the transition whose firing generates this node
        {
            get
            {
                return _tFireFrom;
            }
            set
            {
                _tFireFrom = value;
            }
        }
        private int _tFireFrom;



        public int[] EnabledTransitions //the set of transitions that are enabled at the node
        {
            get
            {
                return _EnabledTransitions;
            }
            set
            {
                Array.Copy(value, _EnabledTransitions, value.Length);
            }
        }
        private int[] _EnabledTransitions;



        public int MarkingDepth //the depth of the node, i.e., the number of transition firings from the initial node to the curren node
        {
            get
            {
                return _MarkingDepth;
            }
            set
            {
                _MarkingDepth = value;
            }
        }
        private int _MarkingDepth;//the index number of the current node


        public int StateIndex//the index of a node in the state space
        {
            get
            {
                return _StateIndex;
            }
            set
            {
                _StateIndex = value;
            }
        }
        private int _StateIndex;

        

       

        public decimal Delt//从父标识到变迁发射得到本标识所用时间
        { //Delt denotes the time required for its parent node to fire a transition to generate this node. 
            set
            {
                _Delt = value;
            }
            get
            {
                return _Delt;
            }
        }
        private decimal _Delt;

        
        // 通过变迁的发射而放入输出库所中的托肯必须等待一定时间后才可利用，并且该时间等于该库所的操作时间
        // M(k)- 和 R(k)- 分别表示：变迁发射前那刻，"系统的标识" 和 "剩余托肯时间矩阵"；分别用_MF和_RF表示
        // M(k)+ 和 R(k)+ 分别表示：变迁发射后，输入托肯已移走但输出托肯还未加入时 "系统的标识" 和 "剩余托肯时间矩阵"；分别用_MZ和_RZ表示
        // M(k)- and R(k)- denote the marking and the remaining token time matrix before a transition fires, respectively. Denoted as _MF and _RF.
        // M(k)+ denotes the marking after tokens are removed from the preplaces of a fired transition and before tokens are added into the postplace of the transition. Denoted as _MZ.
        // R(k)+ denotes the remaining token time matrix after the transition fires. Denoted as _RZ.
        // 这些成员无需被访问，所以没定义相应属性。但private成员也会被派生类继承
        private int[] _MF = new int[PNQL.N_P];//M(k)-
        private int[] _MZ = new int[PNQL.N_P];//M(k)+
        private decimal[,] _RF = new decimal[PNQL.N_P, PNQL.MAX_TOK_PA];//R(k)- 
        public decimal[,] _RZ = new decimal[PNQL.N_P, PNQL.MAX_TOK_PA];//R(k)+	 

        #endregion


        #region Constructors

        //PNQLNode(父节点, 目标节点, g值, h值, f值, 节点的标志, 该标识下所有库所的托肯剩余时间矩阵, 产生本标识所发射的变迁,节点的序号，标识的深度,第二个h值，从父标识到变迁发射得到本标识所用时间)
        public PNQLNode(PNQLNode parent, PNQLNode goalNode,  int[] M, decimal[,] R, int tFireFrom,int stateindex,decimal markingDepth,decimal Delt)
        {
            _Parent = parent; 
            _GoalNode = goalNode;
            _M = new int[PNQL.N_P];
            Array.Copy(M, _M, M.Length);
            _R = new decimal[PNQL.N_P, PNQL.MAX_TOK_PA];
            Array.Copy(R, _R, R.Length);
            _tFireFrom = tFireFrom;
            _EnabledTransitions = new int[PNQL.N_T];
            _StateIndex= stateindex;
            
            _MarkingDepth = (int)markingDepth;
            _Delt = Delt;
        }

        #endregion



        #region Public Methods


        public virtual bool IsSameStateM(PNQLNode aNode)//判断某节点的标识M是否和本节点一致 //只判断M
        {//Decide whether or not this node is the same as the goal node only in terms of M.
            if (aNode == null)
                return false;
            if (_M.Length != aNode.M.Length)
                return false;
            for (int i = 0; i < _M.Length; ++i )
                if (_M[i] != aNode.M[i])
                    return false;
            return true;
        }

        

        public virtual bool IsSameStateM_R(PNQLNode aNode)//判断aNode节点的M和R是否和本节点一致//判断M和R
        {//Decide whether or not this node is the same as the goal node in terms of M and R.
            if (aNode == null)
                return false;
            if (_M.Length != aNode.M.Length)
                return false;
            for (int i = 0; i < _M.Length; ++i)
                if (_M[i] != aNode.M[i])
                    return false;
            for (int i = 0; i < PNQL.N_P; ++i)//zzx/////////////
                for (int j = 0; j < PNQL.MAX_TOK_PA; ++j) 
                {
                    if (_R[i, j] == 0 && aNode.R[i, j] == 0)//Huang202303:因为存在不含剩余托肯时间的非操作库所，且操作库所中的剩余托肯时间按非升序排列，所以无需比较库所中所有的剩余托肯时间;可提速
                        break;
                    if (_R[i, j] != aNode.R[i, j])
                        return false;
                }
            return true;
        }

        public virtual bool IsDeadLock()//判断本节点是否为Deadlock
        {//Decide whether this node is Deadlock
            if (this.EnabledTransitions.Sum() == 0)
            { return true; }
            else
            { return false; }
                    }

        public bool IsGoal()//判断本节点的M和R是否与GoalNode一致
        {//Decide whether or not this node is the same as the goal node in terms of M and R. 
            return IsSameStateM_R(_GoalNode);
        }

    public virtual void FindEnabledTransitions()//寻找当前标识（_M）下使能的变迁，并对EnabledTransitions赋值（1：使能，0：非使能）
            {//Find enabled transitions at a marking. It will assign values to EnabledTransitions such that 1 denotes its corresponding transition is enabled and 0 denotes not.
                int e;
                for (int j = 0; j < PNQL.N_T; ++j)
                {
                    e = 1;
                    for (int i = 0; i < PNQL.N_P; ++i)
                    {
                        if (PNQL.LMINUS[i, j] != 0 && _M[i] < PNQL.LMINUS[i, j]) //变迁使能的条件：当前输入库所的托肯数量大于变迁输入弧所需的输入托肯数量（ M(p) > I(p, t) ）
                        {
                            e = 0;
                            continue;
                        } 
                    }
                    _EnabledTransitions[j] = e; //_EnabledTransitions = new int[AStar.N_T];
                
            }
            
        }



       
        public virtual void GetSuccessors(string fileName, ArrayList Successors,ArrayList _States) //获得当前节点的所有子节点，并添加到Successors列表中
        {//Get the child nodes of the current node and handle them with OPEN and CLOSED. 
         //寻找当前标识下使能的变迁
            
            FindEnabledTransitions();//Find the enabled transitions at the current node.

            
            for (int i = 0; i < _EnabledTransitions.Length; ++i)
            {
               
                if (_EnabledTransitions[i] == 1) //对于每一个使能的变迁 if the transition is enabled
                {
                    //程序选择哪个变迁取决于OPEN中相同f值的节点如何排队，即tie-breaking规则
                    

                    //Calculate _Delt=max{R(pi,k-W(pi, tj)+1}; Because W(pi,tj)=1 in SC-nets, _Delt=max{R(pi,k)}. 
                    //R(pi,k) denotes the remaining token time of the last existing token in pi that is a pre-place of the transition selected to fire
                    _Delt = 0;
                    for (int x = 0; x < PNQL.N_P; ++x)//zzx
                    {
                        if (PNQL.LMINUS[x, i] > 0 && PNQL.operationTimes[x]>0) // if it is a operation pre-place of the transition
                        {//注意，对于每个库所而言，R里面的托肯按照剩余时间非递增排序。且变迁从每个非资源输入库所需要的托肯数为1
                            if (_Delt < _R[x, _M[x] - 1])//参看我的书delta的定义，-1是因为下标从0开始
                                _Delt = _R[x, _M[x] - 1]; 
                        }
                    }


                    //从变迁的输入库所中移去相应的托肯  Remove tokens from the pre-places of the transition
                    //M(k)+ = M(k)- - LMINUS*u(k) , M(k)+ 和M(k)- 分别表示托肯移走前后的系统标识
                    for (int n = 0; n < PNQL.N_P; ++n)
                    {
                        if (PNQL.LMINUS[n, i] > 0)
                            _MZ[n] = _M[n] - PNQL.LMINUS[n, i]; //_MZ：标识状态M(k)+; _MZ: denotes M(k)+
                        else
                            _MZ[n] = _M[n];
                    }

                    //向变迁的所有输出库所中添加相应托肯 Add tokens into the post-places of the transition
                    //M(k+1)- = M(k)+ + LPLUS*u(k)
                    for (int n = 0; n < PNQL.N_P; ++n)
                    {
                        if (PNQL.LPLUS[n, i] > 0)
                            _MF[n] = _MZ[n] + PNQL.LPLUS[n, i];
                        else
                            _MF[n] = _MZ[n];
                     }


                    //在剩余托肯时间向量(即R)中逐个元素地减去_Delt，若值小于0则赋值为0   ZZX
                    //Subtract _Delt from each element of R. If the result is below 0, then set the element to 0. 
                    //计算 R(k)+z = R(k)-z - _Delt(k)z
                    for (int n = 0; n < PNQL.N_P; ++n)
                    {
                        for (int m = 0; m < PNQL.MAX_TOK_PA; ++m)
                        {
                            _RZ[n, m] = _R[n, m] - _Delt;
                            if (_RZ[n, m] < 0)
                                _RZ[n, m] = 0;
                        }
                    }

                    //向变迁的所有输出库所的R中开头位置加入新托肯的操作时间，其它托肯的剩余时间向后移动。所以对于一个非资源库所，其托肯按剩余托肯时间由大到小排列 
                    //R(k+1)-z = R(k)+z + t(k)z
                    for (int n = 0; n < PNQL.N_P; ++n)
                    {
                        if (PNQL.LPLUS[n, i] > 0 && PNQL.operationTimes[n]>0)//if p_n is a post-operation place of the transition t_i
                        {
                            //All remaining token times in R(p_n, .) move one step to the right
                            for (int j = PNQL.MAX_TOK_PA  - 1; j > 0; --j)                   
                                    _RF[n, j] = _RZ[n, j - 1];

                            //Add the operation time of the place to the first entry of R(p_n,.).
                            _RF[n, 0] = PNQL.operationTimes[n];
                        }
                        else
                        {
                            for (int j = 0; j < PNQL.MAX_TOK_PA; ++j)
                                _RF[n, j] = _RZ[n, j];
                        }
                    }
                                       
                    PNQLNode newNode = new PNQLNode(this, GoalNode,_MF, _RF, i, 0, _MarkingDepth + 1, _Delt);
                    ArrayList statesCopy = (ArrayList)_States.Clone(); 
                    bool foundSameState = false;

                    //若生成的新节点未拓展，则将节点加入_States列表，且更新节点序号，反之则无需进行上述操作。
                    //If the generated new node is not expanded, add the node to the _States list and update the node serial number. Otherwise, there is no need to perform the above operations.
                    foreach (PNQLNode aNode in statesCopy)
                    {
                        if (aNode.IsSameStateM_R(newNode))
                        {
                            newNode.StateIndex = aNode.StateIndex;
                            foundSameState = true;
                            break;
                        }
                    }

                    if (!foundSameState)
                    {
                        newNode.StateIndex = ++PNQL._Index;
                        _States.Add(newNode);
                    }
                    Successors.Add(newNode);

                    //若当前得到的后继结点所对应状态动作对的Q值没有值，则初始化为0。
                    //If the Q value of the state-action pair corresponding to the currently obtained successor node has no value, it is initialized to 0.
                    if (!PNQL.Qtable[this.StateIndex].ContainsKey(newNode.StateIndex))
                    {
                        PNQL.Qtable.SetQValue(this, newNode.StateIndex, 0);
                    }
                }
            }
        }


        



        public virtual void PrintNodeInfo() //打印当前节点的信息  Print the info of the current node. 
        {
            Console.Write("tFireFrom:{0} Depth:{1}",  _tFireFrom + 1, _MarkingDepth);


            Console.Write(" tEnabled:");
            for (int n = 0; n < _EnabledTransitions.Length; ++n)
            {
                if (_EnabledTransitions[n] == 1)
                    Console.Write("{0} ", n + 1);
            }
            Console.Write(" M:");
            for (int i = 0; i < _M.Length; ++i)//输出M中为1的places
            {
                if (_M[i] == 1)
                    Console.Write("{0} ", i + 1);
                if (_M[i] > 1)
                    Console.Write("{0}({1}) ", i + 1, _M[i]);
            }
            Console.Write(" R:");
            for (int n = 0; n < PNQL.N_P; ++n)
                for (int m = 0; m < PNQL.MAX_TOK_PA; ++m)
                {
                    if (_R[n, m] != 0)
                        Console.Write("[{0}，{1}]:{2}  ", n + 1, m + 1, _R[n, m]);
                }
            Console.Write(" Index:" + StateIndex);
            Console.WriteLine();
        }

        #endregion

      
       

        
    }








    public sealed class PNQL//基于PN的QL运行所需的属性和行为,sealed表示此类不能被继承
    {
        #region Private Fields
        private PNQLNode _StartNode;//起始节点 the start node of the search
        private PNQLNode _GoalNode;//目标节点 the goal node
        private ArrayList _Successors;//子节点列表 the list to contain the child nodes
        private ArrayList _States;//所有得到的状态列表 the list contains the states space


        #endregion


        #region Properties
        //属性，通常首字母大写。外类对此类非静态属性可通过对象.属性访问；如果是静态static，则通过类.属性访问        

        public static decimal[] operationTimes;//各库所的操作时间（资源库所操作时间为0） Operation times of places (for any resource place and idle place, it equals zero); const常量需要定义时就赋值

        public static int[,] LPLUS;//后置关联矩阵L+ The post-incidence matrix L+
        public static int[,] LMINUS;//前置关联矩阵L- The pre-incidence matrix L-

        public static int N_P;//Petri网中库所数(含资源)  The number of places in the net (including resource places)
        public static int N_T;//Petri网中变迁数 The number of transitions in the net
        public static int N_P_R;//Petri网中资源库所数 The number of resource places
        public static int MAX_TOK_PA; //活动库所的最大容量 The maximal number of tokens in any activity place. It will be set automatically by analyzing the input files.

        public static int[] StartM;//开始节点的标识向量 The marking of the start node
        public static int[] GoalM;//目标节点的标识向量 The marking of the goal node
        public static decimal[,] StartR;//开始节点的剩余托肯时间矩阵 The remaining token time matrix of the start node
        public static decimal[,] GoalR;//目标节点的剩余托肯时间矩阵 The remaining token time matrix of the end node
        public static int _Index = 0;//状态空间中节点的编号 the index of a node in the state space
        public static QTable Qtable = new QTable();//Q-Table
        public static int makespan = 0;//makespan
        private Random rand;


        #endregion


        #region Constructors

        public PNQL(string initialMFile, string incidenceMatrixFile)//构造函数
        {
            _Successors = new ArrayList();
            _States = new ArrayList();
            rand = new Random();
            Read_initfile(initialMFile);
            Read_matrixfile(incidenceMatrixFile);



            Console.WriteLine("Petri网中库所数(含资源) The number of places (including resource places)：" + N_P);
            Console.WriteLine("Petri网中变迁数 The number of transitions：" + N_T);
            Console.WriteLine("Petri网中资源库所数 The number of resource places：" + N_P_R);
            Console.WriteLine("初始标识 The initial marking：");

            for (int i = 0; i < N_P; i++)
            {
                Console.Write(StartM[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("操作的时间 Operation times：");
            for (int i = 0; i < N_P; i++)
            {
                Console.Write(operationTimes[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("目标标识 The goal marking：");
            for (int i = 0; i < N_P; i++)
            {
                Console.Write(GoalM[i] + " ");
            }
            Console.WriteLine();
            Console.WriteLine("后置伴随矩阵 The post-incidence matrix L+：");
            for (int i = 0; i < N_P; ++i)
            {
                for (int j = 0; j < N_T; ++j)
                {
                    Console.Write(LPLUS[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("前置伴随矩阵 The pre-incidence matrix L-：");
            for (int i = 0; i < N_P; ++i)
            {
                for (int j = 0; j < N_T; ++j)
                {
                    Console.Write(LMINUS[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();


            StartR = new decimal[N_P, MAX_TOK_PA];//资源库所的所有R都为0
            GoalR = new decimal[N_P, MAX_TOK_PA];
            for (int i = 0; i < N_P; ++i)
                for (int j = 0; j < MAX_TOK_PA; ++j)
                {
                    StartR[i, j] = 0;
                    GoalR[i, j] = 0;
                }

        }

        #endregion




        #region Private Methods

        private static void Read_initfile(string fileName)
        {
            StreamReader SR;
            try
            {
                SR = File.OpenText(fileName);
            }
            catch (Exception ex)
            {
                Console.Write(fileName + " open failed! " + ex.Message);
                return;
            }
            string stringALine;
            string[] stringWords;

            //init文件的第一行  The first line of the init.txt
            {
                stringALine = SR.ReadLine();

                stringALine = stringALine.Trim();//去头尾空格
                while (stringALine == "")//去空行
                {
                    stringALine = SR.ReadLine();
                    stringALine = stringALine.Trim();
                }

                stringWords = System.Text.RegularExpressions.Regex.Split(stringALine, @"\s{1,}"); //以一个或多空格为分隔


                //Petri网中库所数(含资源)
                N_P = stringWords.Length;

                //初始marking
                StartM = new int[N_P];
                for (int i = 0; i < stringWords.Length; ++i)
                {
                    StartM[i] = Convert.ToInt32(stringWords[i]);
                }
            }


            //init文件的第二行 The second line of the init.txt
            {
                stringALine = SR.ReadLine(); //ReadLine可能返回空行

                stringALine = stringALine.Trim();
                while (stringALine == "")
                {
                    stringALine = SR.ReadLine();
                    stringALine = stringALine.Trim();
                }

                stringWords = System.Text.RegularExpressions.Regex.Split(stringALine, @"\s{1,}"); //以一个或多空格为分隔


                operationTimes = new decimal[N_P]; //各库所的操作时间
                for (int i = 0; i < stringWords.Length; ++i)
                    operationTimes[i] = Convert.ToInt32(stringWords[i]);

            }

            //init文件的第三行 the third line of the init.txt
            {
                stringALine = SR.ReadLine(); //ReadLine可能返回空行
                stringALine = stringALine.Trim();
                while (stringALine == "")
                {
                    stringALine = SR.ReadLine();
                    stringALine = stringALine.Trim();
                }

                stringWords = System.Text.RegularExpressions.Regex.Split(stringALine, @"\s{1,}"); //以一个或多空格为分隔

                //目标marking
                GoalM = new int[N_P];
                for (int i = 0; i < stringWords.Length; ++i)
                {
                    GoalM[i] = Convert.ToInt32(stringWords[i]);
                }
            }


            N_P_R = 0;//Petri网中资源库所数 the number of resource places in the net
            MAX_TOK_PA = 0; //活动库所的最大托肯容量 The maximal number of tokens in any activity place.                 
            int maxTokP_R = 0;//S_0中所有资源库所中的最大托肯数
            int maxTokP_Start = 0;//S_0中所有起始库所中的最大托肯数

            for (int i = 0; i < stringWords.Length; ++i) //Huang2023:纠正了MAX_TOK_PA的计算错误。并改进了MAX_TOK_PA=min{maxTokP_Start, maxTokP_R}
            {
                if (StartM[i] != 0 && GoalM[i] != 0 && StartM[i] == GoalM[i]) //在读取PN输入文件时，自动计算资源库所数量与非资源库所最大托肯容量; N_P_R and MAX_TOK_PA are automatically calculated by analyzing the input files.
                {
                    N_P_R++;
                    if (maxTokP_R < StartM[i])
                        maxTokP_R = StartM[i];
                }
                else if (StartM[i] != 0 && maxTokP_Start < StartM[i])
                    maxTokP_Start = StartM[i];
            }
            MAX_TOK_PA = (maxTokP_Start <= maxTokP_R) ? maxTokP_Start : maxTokP_R;

            SR.Close();

            return;
        }





        private static void Read_matrixfile(string fileName)
        {
            StreamReader SR;

            try
            {
                SR = File.OpenText(fileName);
            }
            catch
            {
                Console.Write(fileName + " open failed!");
                return;
            }


            string stringALine;

            //获取Petri网中变迁数N_T obtain the number of transitions in the net
            N_T = 0;
            stringALine = SR.ReadLine();
            while (stringALine != null)
            {
                stringALine = stringALine.Trim();//去头尾空格

                if (stringALine != "") //stringALine可能会是空行
                    ++N_T;
                stringALine = SR.ReadLine();
            }
            SR.Close();



            try
            {
                SR = File.OpenText(fileName);
            }
            catch
            {
                Console.Write(fileName + " open failed!");
                return;
            }


            int[,] temp = new int[N_T, N_P]; //临时矩阵 N_T x N_P,其中N_T需要上面的语句获取，而N_P通过前一个函数Read_initfile()获取
            string[] stringWords;
            int n = 0;

            stringALine = SR.ReadLine();
            while (stringALine != null)
            {
                stringALine = stringALine.Trim(); //去头尾空格

                if (stringALine != "")
                {
                    stringWords = System.Text.RegularExpressions.Regex.Split(stringALine, @"\s{1,}"); //以一个或多空格为分隔

                    for (int j = 0; j < N_P; ++j)
                        temp[n, j] = Convert.ToInt32(stringWords[j]);
                    n++;
                }

                stringALine = SR.ReadLine();
            }
            SR.Close();


            //关联矩阵L+
            LPLUS = new int[N_P, N_T];

            //关联矩阵L-
            LMINUS = new int[N_P, N_T];

            for (int i = 0; i < N_T; ++i)
            {
                for (int j = 0; j < N_P; ++j)
                {
                    if (temp[i, j] >= 1)
                        LPLUS[j, i] = temp[i, j];
                    else
                        LPLUS[j, i] = 0;


                    if (temp[i, j] <= -1)
                        LMINUS[j, i] = -temp[i, j];
                    else
                        LMINUS[j, i] = 0;
                }
            }

            return;
        }





        private void PrintNodeList(object aNodeList)//输出某列表中所有节点的信息  Print the info. of all nodes in a given list. 
        {
            Console.WriteLine("\nNode list:");
            int i = 0;
            foreach (PNQLNode n in (aNodeList as IEnumerable))
            {
                Console.Write("{0})\t", i + 1);
                i++;
                n.PrintNodeInfo();
            }
            Console.WriteLine("=============================================================");
        }



        #endregion




        #region Public Methods


        public class QTable
        {//Q-Table相关属性与方法
            //Q-Table related properties and functions
            private Dictionary<int, Dictionary<int, decimal>> _table;

            public QTable()
            {
                _table = new Dictionary<int, Dictionary<int, decimal>>();
            }

            public void SetQValue(PNQLNode currentState, int nextStateIndex, decimal qValue)
            { int currentStateIndex = currentState.StateIndex;
                if (!_table.ContainsKey(currentStateIndex))
                {
                    _table[currentStateIndex] = new Dictionary<int, decimal>();
                }

                _table[currentStateIndex][nextStateIndex] = qValue;
            }

            public decimal GetQValue(PNQLNode currentState, int nextStateIndex)
            {
                int currentStateIndex = currentState.StateIndex;
                if (!_table.ContainsKey(currentStateIndex))
                {
                    return 0;
                }

                if (!_table[currentStateIndex].ContainsKey(nextStateIndex))
                {
                    return 0;
                }

                return _table[currentStateIndex][nextStateIndex];
            }
            public decimal GetMaxQValue(PNQLNode currentState)
            {
                int currentStateIndex = currentState.StateIndex;
                decimal maxQValue = decimal.MinValue;

                if (_table.ContainsKey(currentStateIndex))
                {
                    var stateDict = _table[currentStateIndex];
                    foreach (var nextState in stateDict.Keys)
                    {
                        decimal qValue = stateDict[nextState];
                        if (qValue > maxQValue)
                        {
                            maxQValue = qValue;
                        }
                    }
                }
                else 
                {
                    maxQValue = 0;
                }
                return maxQValue;
            }
            public bool ContainsKey(int stateIndex)
            {
                return _table.ContainsKey(stateIndex);
            }

            public void Clear()
            { _table = new Dictionary<int, Dictionary<int, decimal>>(); }

            public decimal this[int stateIndex, int actionIndex]
            {
                get
                {
                    if (_table.ContainsKey(stateIndex) && _table[stateIndex].ContainsKey(actionIndex))
                    {
                        return _table[stateIndex][actionIndex];
                    }
                    return 0; 
                }
                set
                {
                    if (!_table.ContainsKey(stateIndex))
                    {
                        _table[stateIndex] = new Dictionary<int, decimal>();
                    }
                    _table[stateIndex][actionIndex] = value;
                }
            }
            public Dictionary<int, decimal> this[int stateIndex]
            {
                get
                {
                    if (_table.ContainsKey(stateIndex))
                    {
                        return _table[stateIndex];
                    }
                    return new Dictionary<int, decimal>();
                }
                set
                {
                    _table[stateIndex] = value;
                }
            }


        }

        public int GetNextStateWithMaxQValue(PNQLNode currentState)
        //根据当前状态的索引值，获取QTable中所有包含匹配索引值的所有 QValue 对象的集合。然后，我们遍历集合并找到具有最大Q值的对象。如果有多个 QValue 具有相同的最大 Q 值，则将相应的下一个状态索引值添加到 nextMaxQ 列表中。
        //接下来，我们从具有最大 Q 值的下一个索引值中随机选择一个返回。
        //Gets the collection of all QValue objects in the QTable that contain matching index values based on the current state's index value. We then iterate through the collection and find the object with the largest Q-value. If there are multiple QValues with the same maximum Q value, the corresponding next state index value is added to the nextMaxQ list.
        //Next, we randomly select one to return from the next index value with the largest Q value.
        {
            int MaxnextStateIndex = -2;
            decimal maxQValue = decimal.MinValue;
            int currentStateIndex = currentState.StateIndex;
            List<int> nextMaxQ = new List<int>();

            if (Qtable.ContainsKey(currentStateIndex))
            {
                foreach (var nextStateIndex in Qtable[currentStateIndex].Keys)
                {
                    decimal qValue = Qtable[currentStateIndex, nextStateIndex];
                   
                    if (qValue == 0)
                    { continue; }
                    if (qValue > maxQValue)
                    {
                        maxQValue = qValue;
                        nextMaxQ.Clear();
                        nextMaxQ.Add(nextStateIndex);
                    }
                    else if (qValue == maxQValue)
                    {
                        nextMaxQ.Add(nextStateIndex);
                    }
                }
            }

            if (nextMaxQ.Count > 0)
            {
                int index = rand.Next(nextMaxQ.Count);
                MaxnextStateIndex = nextMaxQ[index];
                return MaxnextStateIndex;
            }
            return GetRandomNextState(currentState);

        }




        public int GetRandomNextState(PNQLNode currentState)
        //根据当前状态的索引值，获取Q-Table中所有包含匹配索引值的所有 QValue 对象的集合。然后，我们随机选择一个返回其索引值。
        //Gets the collection of all QValue objects in the QTable that contain matching index values based on the current state's index value. Then, we randomly select one and return its index value.
        {
            int MaxnextStateIndex = -2;
            List<int> nextStates = new List<int>();
            int currentStateIndex = currentState.StateIndex;

            if (Qtable.ContainsKey(currentStateIndex))
            {
            foreach (var nextStateIndex in Qtable[currentStateIndex].Keys)
                {
                    nextStates.Add(nextStateIndex);
                }
            }

            if (nextStates.Count > 0)
            {
                int index = rand.Next(nextStates.Count);
                MaxnextStateIndex = nextStates[index];
            }
            return MaxnextStateIndex;
        }

        

        public double EpsilonGreedyStrategy(int episode, int episodeCount)
        {   //选择使用的epsilon。
            //Choose epsilon to use.
            double epsilon;  
            epsilon = Math.Pow(0.01, (double)episode / episodeCount);//epsilon1
            //epsilon = 1 - 0.99 * ((double)episode / episodeCount);//epsilon2
            //epsilon = -Math.Pow(0.01, ((double)(episodeCount - episode) / episodeCount)) + 1.01;//epsilon3
            return epsilon;
        }

        public PNQLNode TakeAction(string fileName, PNQLNode CurrentState, ArrayList Successors, ArrayList States, double epsilon)
        {   //代理根据epsilon greedy strategy采取动作
            //The agent takes actions based on the epsilon greedy strategy
            CurrentState.GetSuccessors(fileName, Successors, States);
            double randNum = new Random().NextDouble();

            if (randNum < epsilon) {
                int nextindex = GetRandomNextState(CurrentState);
                foreach (PNQLNode aNode in Successors)
                { if (aNode.StateIndex == nextindex)
                    {
                        return aNode;
                    }
                }
            }
            else {
                int nextindex = GetNextStateWithMaxQValue(CurrentState);
                foreach (PNQLNode aNode in Successors)
                { if (aNode.StateIndex == nextindex)
                    {
                        return aNode;
                    }
                }
            }
            return CurrentState;
        }

        public void UpdateQValue(PNQLNode currentState, PNQLNode nextState, int nextStateIndex, decimal reward, decimal learningRate, decimal discountFactor)
        {   //更新Q-value
            //update Q-value

            // 获取当前状态下采取指定行动到达下一个状态的 Q 值
            // Get the Q-value of taking the specified action in the current state to reach the next state
            decimal currentQValue = Qtable.GetQValue(currentState, nextStateIndex);

            // 获取下一个状态下采取所有可能行动中 Q 值的最大值
            // Get the maximum Q-value among all possible actions taken in the next state
            decimal maxNextQValue = Qtable.GetMaxQValue(nextState);

            // 更新当前状态和下一个状态之间(即状态 动作对)的 Q 值
            // Update the Q value between the current state and the next state (ie, state-action pair)
            decimal updatedQValue = (1 - learningRate) * currentQValue + learningRate * (reward + discountFactor * maxNextQValue);
            Qtable.SetQValue(currentState, nextStateIndex, updatedQValue);
        }

               
        public void Train(PNQLNode startNode, PNQLNode goalNode, string fileName, int episodeCount,decimal learningrate,decimal discountrate,decimal Pereward,int trainth) //训练我们的agent
        {//训练部分
          //Training part

            //初始化
            //Initialization
            _StartNode = startNode;
            _GoalNode = goalNode;
            _Successors.Clear();
            _States.Clear();
            Qtable.Clear();
            _States.Add(goalNode);
            _States.Add(startNode);
            _Index = 0;
            int Deadlockc = 0; //Deadlock counts
            List<decimal> aReward = new List<decimal>();//列表记录统计average accumulative reward  List record statistics average accumulative reward

            using (StreamWriter sw = new StreamWriter($"training process{trainth}.csv"))//将训练过程中的average accumulative reward与deadlock counts写入csv Write the average accumulative reward and deadlock counts during the training process to csv
            {
                for (int episode = 1; episode < episodeCount + 1; episode++)
                {// 对每个 episode 进行初始化
                    //Initialize each episode
                    PNQLNode CurrentState = _StartNode;
                    double epsilon = EpsilonGreedyStrategy(episode, episodeCount);
                    decimal AcReward = 0;//Average accumulative reward
                    while (!CurrentState.IsSameStateM_R(_GoalNode))
                    {// 训练核心流程
                        //Training core process
                        _Successors.Clear();
                        PNQLNode nextState = TakeAction(fileName, CurrentState, _Successors, _States, epsilon);
                        nextState.FindEnabledTransitions();
                        int nextStateIndex = nextState.StateIndex;
                        if (nextState.IsDeadLock() && !nextState.IsGoal())
                        {
                            UpdateQValue(CurrentState, nextState, nextStateIndex, Pereward, learningrate, discountrate);
                            AcReward = AcReward + Pereward;
                            Deadlockc++;
                            Console.WriteLine("Deadlcok encountered");
                            break;
                        }
                        else {
                            decimal reward = -nextState.Delt;
                            AcReward = AcReward + reward;
                            UpdateQValue(CurrentState, nextState, nextStateIndex, reward, learningrate , discountrate);
                        }
                         
                        CurrentState = nextState;
                        }
                     aReward.Add(AcReward);
                     int Statecount = _States.Count;
                     Console.WriteLine("episode: " + episode+ " The number of nodes in the state space is: " + Statecount + " epsilon: " + epsilon);
                     _Successors.Clear();
                    if (episode % 100 == 0) { 
                        sw.WriteLine($"{episode/100},{aReward.Average()},{Deadlockc}");
                        Deadlockc = 0;
                        aReward.Clear();
                        }
                }
            }
         }
           

        public void FindPath_PNQL(PNQLNode startNode, PNQLNode goalNode, string fileName)
        {//Execution part：construct a path from a start state to a goal state based on the QTable.
            _StartNode = startNode;
            _GoalNode = goalNode;
            makespan = 0;
            List<PNQLNode> path = new List<PNQLNode>();  // 用于存储路径节点的列表 List used to store path nodes
            PNQLNode currentNode = _StartNode;
            PNQLNode nextNode = _StartNode;
            Console.WriteLine("Training parts ends and execution parts begins：");
            while (!currentNode.IsSameStateM_R(_GoalNode))
            {
                path.Add(currentNode);
                Console.Write("Current Node： ");
                currentNode.PrintNodeInfo();
                _Successors.Clear();
                currentNode.GetSuccessors(fileName, _Successors, _States);
                if (_Successors.Count == 0)
                {
                    currentNode = _StartNode;
                    makespan = 999;
                    break;
                }
                
                int nextnodeindex = GetNextStateWithMaxQValue(currentNode);
                foreach (PNQLNode node in _Successors)
                {
                    if (node.StateIndex == nextnodeindex)
                    {
                        nextNode = node;
                        break;
                    }
                }
                Console.Write("Next node found: ");
                nextNode.PrintNodeInfo();

                makespan = (int)(makespan + nextNode.Delt);
                Console.WriteLine("current makespan： " + makespan);
                currentNode = nextNode;
                if (nextNode.IsSameStateM_R(_GoalNode))
                {
                    path.Add(nextNode);
                }
            }
            Console.WriteLine("PNQL Solution: ");
            foreach (var node in path)
            {
                node.PrintNodeInfo();

            }
            Console.WriteLine("Total Makespan: " + makespan);
        }
        

        #endregion
    }

}