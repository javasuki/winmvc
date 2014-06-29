using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using NXDO.Data.MVCP;

namespace NXDO.Data.Factory
{
    internal enum GetTypeMode
    {
        Controller,
        View,
        PartialView
    }

    /// <summary>
    /// MVC应用程序
    /// </summary>
    public static class MvcApplication
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal static Assembly EntryAssembly
        {
            get;
            private set;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static string NamespaceController { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static string NamespaceView { get; set; }
        internal static Type GetType(string TypeName, GetTypeMode gtmFlag, bool isThrowNull)
        {
            string exPrefixName = "";
            string typeFullName = string.Empty;
            if (gtmFlag == GetTypeMode.Controller)
            {
                typeFullName = NamespaceController + TypeName;
                exPrefixName = "控制器 ";
            }
            else if (gtmFlag == GetTypeMode.View)
            {
                typeFullName = NamespaceView + TypeName;
                exPrefixName = "视图 ";
            }
            else if (gtmFlag == GetTypeMode.PartialView)
            {
                typeFullName = NamespaceView + TypeName;
                exPrefixName = "分部视图 ";
            }

            if (gtmFlag == GetTypeMode.Controller && !typeFullName.EndsWith("Controller", StringComparison.CurrentCultureIgnoreCase))
                typeFullName += "Controller";

            Type type = MvcApplication.EntryAssembly != null ?
                MvcApplication.EntryAssembly.GetType(typeFullName, false, true) :
                Type.GetType(typeFullName, false, true);

            if (isThrowNull && type == null)
                throw new NullReferenceException(typeFullName + " 类型，装载失败。");
            return type;
        }

        /// <summary>
        /// 启动MVC应用程序，并执行默认控制器上的 Index() 子操作。
        /// <para>默认控制器：DefaultController 或 HomeController。</para>
        /// </summary>
        public static void Run(params object[] values)
        {
            var asm = MvcApplication.EntryAssembly = Assembly.GetEntryAssembly();
            if (asm == null)
                throw new PlatformNotSupportedException("不是一个有效的(Dotnet)Windows应用程序。");

            var mi = asm.EntryPoint;
            if (mi == null)
                throw new NullReferenceException("应用程序缺少入口方法。");

            MvcApplication.NamespaceController = mi.DeclaringType.Namespace + ".Controllers.";
            MvcApplication.NamespaceView = mi.DeclaringType.Namespace + ".Views.";


            Type type = MvcApplication.GetType("DefaultController", GetTypeMode.Controller, false);
            type = type ?? MvcApplication.GetType("HomeController", GetTypeMode.Controller, false);
            if (type == null)
                throw new NullReferenceException("未找到默认的(DefaultController或HomeController)控制器。");

            MvcApplication.Run(type, values);
        }


        public static void Run(string baseUrl, params object[] values)
        {
            //远程 reset api
        }


        /// <summary>
        /// 运行MVCP应用程序
        /// </summary>
        /// <param name="controllerType">控制器类型</param>
        static void Run(Type controllerType, params object[] values)
        {
            var controller = Activator.CreateInstance(controllerType) as Controller;
            controller.Index(values);
        }



        //public static void Route(string actionName, params object[] values)
        //{

        //}

        //public static void Route<TController>(Expression<Func<TController,ActionResult>> actionExpr)
        //{

        //}
    }
}
