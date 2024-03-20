using System;
using System.IO;
using System.Collections;

namespace Huangbo.RLPetri.Test
{
    class MainClass
    {

        #region Public Methods

        [STAThread]

        static void Main(string[] args)
        {
            /*
             * This program is to search a scheduling path from an initial state to a goal state in the reachability graph of a place-timed Petri net for a resource allocation system by using the Q-learning algorithm.
             * April. 12, 2024
            */

            Huangbo.RLPetri.PNQL qL;
            Huangbo.RLPetri.PNQLNode goalNode;
            Huangbo.RLPetri.PNQLNode startNode;

            string fileName = "ChenFig5_11";  //The prefix of your input Petri net files.         fileName should be "XXXFigx_111_init.txt" or "XXXFigx_111_matrix.txt".   
            //string fileName = "ChenFig6_211";  //The prefix of your input files. 
            //string fileName = "Chen2011Big_11122";  //The prefix of your input files. `
            //string fileName = "ChenFig6Extended_121";  //The prefix of your input files.
            //string fileName = "Xiong98_1111";  //The prefix of your input files.
            //string fileName = "test" ;  //The prefix of your input files.
            //string fileName = "new4x3_2111";//The prefix of your input files.
            //string fileName = "Huang2012Fig1_1111"; //The prefix of your input files.
            string[] initialMFile = new string[] { "./" + fileName + "_init.txt" };//A file containing the initial state, goal state, and the time information in activity places. It is a file with 3 lines and |P| columns. 会自动计算获取Petri网的N_P, N_P_R, MAX_TOK_PA
            string[] incidenceMatrixFile = new string[] { "./" + fileName + "_matrix.txt" };//Petri net structure file containing its incidence matrix. It is a file of a |T|x|P| matrix. 我们在程序中会转化成库所x变迁；会获取Petri网的N_T和关联矩阵

            //统计表格及超参数初始化 Statistical tables and hyperparameter initialization
            List<int> Makespan = new List<int>();
            List<double> tTime = new List<double>();
            List<double> sTime = new List<double>();
            decimal learningrate = 0.9M;
            decimal discountrate = 0.3M;
            decimal PeReward = -10000;
            int Epi = 10000;

            qL = new Huangbo.RLPetri.PNQL(initialMFile[0], incidenceMatrixFile[0]);
            goalNode = new Huangbo.RLPetri.PNQLNode(null, null, PNQL.GoalM, PNQL.GoalR, 0, -1, 0, 0);
            startNode = new Huangbo.RLPetri.PNQLNode(null, goalNode, PNQL.StartM, PNQL.StartR, 0, 0, 0, 0);
            
            Console.WriteLine();
            
             using (StreamWriter sw2 = new StreamWriter("makespan result.csv"))//统计每次训练及执行的makespan等数据 Statistics of makespan and other data for each training and execution
            {
                 for (int i = 1; i <= 1; i++)
                 {
                     DateTime startTime1 = DateTime.Now;//训练部分开始时间  the start time of the training part
                    qL.Train(startNode, goalNode, fileName, Epi, learningrate, discountrate, PeReward,i);
                     DateTime endTime1 = DateTime.Now;//训练部分结束时间  the end time of the training part
                    TimeSpan trainTime = new TimeSpan(endTime1.Ticks - startTime1.Ticks);
                     double toMs1 = trainTime.TotalMilliseconds;
                     DateTime startTime2 = DateTime.Now;//执行部分开始时间  the start time of the execution part
                     qL.FindPath_PNQL(startNode, goalNode, fileName);
                     sw2.WriteLine($"{i},{PNQL.makespan}");
                     Makespan.Add(PNQL.makespan);
                     DateTime endTime2 = DateTime.Now;//执行部分结束时间 the end time of the search part
                    TimeSpan elapsedTime = new TimeSpan(endTime2.Ticks - startTime2.Ticks);//运行时间 the running time of the search
                     double toMs2 = elapsedTime.TotalMilliseconds;
                     tTime.Add(toMs1);
                     sTime.Add(toMs2);

                    Console.WriteLine("Search time：hours={0}, minutes={1}, seconds={2}, milliseconds={3},Epi={4}", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds, elapsedTime.Milliseconds, Epi);
                }
                double avgm = Makespan.Average();
                 double avgttime = tTime.Average();
                 double avgstime = sTime.Average();
                 sw2.Flush();
                 double bias = (avgm - 51) / 51;
                 Console.WriteLine("{4} Search result：average makespan={0}, average traintime={1}, average searchtime={2}, makespan bias={3}", avgm, avgttime, avgstime, bias, fileName);

                 //保持输出屏幕不被关闭

             }
                         
        }


        #endregion
    }
}