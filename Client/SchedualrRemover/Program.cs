﻿using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SchedualrRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TaskService ts = new TaskService())
            {
                ts.GetFolder("ClassNet Client").DeleteTask("Run When Logon");
                ts.RootFolder.DeleteFolder("ClassNet Client");
            }


            Console.WriteLine("자동 실행 스케줄을 성공적으로 제거했습니다.");
            Thread.Sleep(500);
            

        }
    }
}