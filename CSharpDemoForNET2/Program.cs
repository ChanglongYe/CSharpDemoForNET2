using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CSharpDemoForNET2
{
    class Program
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void WriteToConsoleCallback(IntPtr str);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary", SetLastError = true)]
        public static extern IntPtr LoadLibrary(
           [MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule,
                   [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        public static extern bool FreeLibrary(IntPtr hModule);


        public delegate int InitPrinterManager(char[] url);
        public delegate void SetRecvDataCallback(WriteToConsoleCallback callback);
        private static WriteToConsoleCallback callback;
        public delegate int SendMessage(byte[] str);
        public delegate int ClosePrinterManager();

        static void onMessage(IntPtr str)
        {
            string message = Marshal.PtrToStringAnsi(str);
            Console.Write(message);
        }
        static void Main(string[] args)
        {
            //用于接口返回
            int Res = -1;
            //以下动态载入dll库
            IntPtr hLib = LoadLibrary("PrinterManager.dll");
            if (hLib == IntPtr.Zero)
            {
                Console.WriteLine("loadlibrary failed! ErrorNuber:"+Marshal.GetLastWin32Error().ToString());
                Console.Read();
                System.Environment.Exit(-1);
            }
            //以下载入初始化函数initPrinterManager
            IntPtr hapi = GetProcAddress(hLib, "initPrinterManager");
            if (hapi == IntPtr.Zero)
            {
                Console.WriteLine("load initPrinterManager failed! ErrorNuber:" + Marshal.GetLastWin32Error().ToString());
                Console.Read();
                System.Environment.Exit(-1);
            }
            //将初始化函数指针封装成委托，并调用
            InitPrinterManager initPrinterManager = 
                (InitPrinterManager)Marshal.GetDelegateForFunctionPointer(hapi, typeof(InitPrinterManager));
            string url = "ws://127.0.0.1:13528";
            Res = initPrinterManager(url.ToCharArray());
            if (Res!=0)
            {
                Console.WriteLine("run initPrinterManager failed! \n");
                Console.Read();
                System.Environment.Exit(-1);
            }

            //以下载入设定回调函数的函数，并调用
            IntPtr hapi2 = GetProcAddress(hLib, "setRecvDataCallback");
            if (hapi2 == IntPtr.Zero)
            {
                Console.WriteLine("load setRecvDataCallback failed! ErrorNuber:" + Marshal.GetLastWin32Error().ToString());
            }
            SetRecvDataCallback setRecvDataCallback =
                (SetRecvDataCallback)Marshal.GetDelegateForFunctionPointer(hapi2, typeof(SetRecvDataCallback));
            callback = new WriteToConsoleCallback(onMessage);
            setRecvDataCallback(callback);

            //以下载入发送数据函数，并发送数据
            string str = "{\"cmd\":\"getPrinters\",\"requestID\":\"123458976\",\"version\":\"1.0中文\"}";
            //str = "{\"cmd\":\"print\",\"requestid\":\"1866\",\"version\":\"1.0\",\"task\":{\"taskID\":\"2016/8/24 14:37:04_106123892\",\"preview\":true,\"printer\":\"Microsoft XPS Document Writer\",\"documents\":[{\"documentID\":\"SW-20160614-00003\",\"contents\":[{\"data\":{\"cpCode\":\"STO\",\"recipient\":{\"address\":{\"city\":\"安徽省\",\"detail\":\"禹航路360号\",\"district\":\"合肥市\",\"province\":\"其它区\",\"town\":\"\"},\"mobile\":\"15485875225\",\"name\":\"白同学\",\"phone\":\"13750005656\"},\"routingInfo\":{\"consolidation\":{\"name\":\"\",\"code\":\"\"},\"origin\":{\"code\":\"STANDARD_EXPRESS\",\"name\":\"标准快递\"},\"sortation\":{\"name\":\"\"},\"routeCode\":\"STO\"},\"sender\":{\"address\":{\"city\":\"古墩路122号\",\"detail\":\"古墩路122号\",\"district\":null,\"province\":null,\"town\":null},\"mobile\":\"13450434390\",\"name\":\"富润\",\"phone\":\"13450434390\"},\"shippingOption\":{\"code\":\"STANDARD_EXPRESS\",\"services\":null,\"title\":\"STANDARD_EXPRESS\"},\"waybillCode\":\"3300000079602\"},\"signature\":\"\",\"templateURL\":\"http://cloudprint.daily.taobao.net/template/standard/201/4\",\"ErrCode\":null,\"ErrMsg\":null,\"SubErrCode\":null,\"SubErrMsg\":null,\"TopForbiddenFields\":null,\"Body\":null,\"ReqUrl\":null,\"IsError\":false}]}]}}";
            IntPtr hapi3 = GetProcAddress(hLib, "sendMessage");
            if (hapi3 == IntPtr.Zero)
            {
                Console.WriteLine("load sendMessage failed! ErrorNuber:" + Marshal.GetLastWin32Error().ToString());
            }

            SendMessage sendMessage =
                (SendMessage)Marshal.GetDelegateForFunctionPointer(hapi3, typeof(SendMessage));
            byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
            Res = sendMessage(b);
            Console.WriteLine(str);
            if (Res != 0)
            {
                Console.WriteLine("sendMessage failed! \n");
                System.Threading.Thread.Sleep(2000);
                System.Environment.Exit(-1);
            }

            System.Threading.Thread.Sleep(1000);
            //以下载入关闭连接函数，并调用
            IntPtr hapi4 = GetProcAddress(hLib, "closePrinterManager");
            if (hapi4 == IntPtr.Zero)
            {
                Console.WriteLine("load closePrinterManager failed! ErrorNuber:" + Marshal.GetLastWin32Error().ToString());
            }
            ClosePrinterManager closePrinterManager =
                (ClosePrinterManager)Marshal.GetDelegateForFunctionPointer(hapi4, typeof(ClosePrinterManager));
            Res = closePrinterManager();
            if (Res != 0)
            {
                Console.WriteLine("closePrinterManager failed! \n");
                System.Threading.Thread.Sleep(2000);
                System.Environment.Exit(-1);
            }
            //释放动态链接库
            FreeLibrary(hLib);
            Console.Read(); 
        }

        //回调函数，打印出接收的数据
        static void printMesssage(string message)
        {
            Console.Write("\n" + message);
        }
    }
}
