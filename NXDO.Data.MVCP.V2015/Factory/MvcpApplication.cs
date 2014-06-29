using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

using NXDO.Data.MVCP;

namespace NXDO.Data.Factory
{
    public static class MvcpApplication
    {
        internal static Assembly EntryAssembly
        {
            get;
            private set;
        }
        private static string NamespaceController { get; set; }
        private static string NamespaceView { get; set; }
        internal static Type GetType(string TypeName,bool isController, bool isThrowNull)
        {
            string typeFullName = isController ? 
                                        NamespaceController + TypeName :
                                        NamespaceView + TypeName;

            if (isController && !typeFullName.EndsWith("Controller", StringComparison.CurrentCultureIgnoreCase))
                typeFullName += "Controller";

            Type type = MvcpApplication.EntryAssembly != null ?
                MvcpApplication.EntryAssembly.GetType(typeFullName, false, true) :
                Type.GetType(typeFullName, false, true);

            if (isThrowNull && type == null)
                throw new NullReferenceException(typeFullName + "类型装载失败。");
            return type;
        }

        /// <summary>
        /// 启动MVC应用程序，并执行默认控制器上的 Index() 子操作。
        /// <para>默认控制器：DefaultController 或 HomeController。</para>
        /// </summary>
        public static void Run()
        {
            var asm = MvcpApplication.EntryAssembly = Assembly.GetEntryAssembly();
            if (asm == null)
                throw new PlatformNotSupportedException("不是一个有效的(Dotnet)Windows应用程序。");

            var mi = asm.EntryPoint;
            if (mi == null)
                throw new NullReferenceException("应用程序缺少入口方法。");

            MvcpApplication.NamespaceController = mi.DeclaringType.Namespace + ".Controllers.";
            MvcpApplication.NamespaceView = mi.DeclaringType.Namespace + ".Views.";


            Type type = MvcpApplication.GetType("DefaultController", true, false);
            type = type ?? MvcpApplication.GetType("HomeController", true, false);
            if (type == null)
                throw new NullReferenceException("未找到默认的(DefaultController或HomeController)控制器。");

            MvcpApplication.Run(type);
        }

        //public static void Run(string controllerName)
        //{

        //}

        /// <summary>
        /// 运行MVCP应用程序
        /// </summary>
        /// <param name="controllerType">控制器类型</param>
        static void Run(Type controllerType)
        {
            var controller = Activator.CreateInstance(controllerType) as Controller;
            controller.Index();
        }



        //public static void Route(string actionName, params object[] values)
        //{

        //}

        //public static void Route<TController>(Expression<Func<TController,ActionResult>> actionExpr)
        //{

        //}
    }
}
