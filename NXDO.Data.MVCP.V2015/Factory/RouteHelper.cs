using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using NXDO.Data.MVCP;

namespace NXDO.Data.Factory
{
    //public static class RouteHelper
    //{
    //    public static void Runner()
    //    {
    //        var asm = Assembly.GetCallingAssembly();
    //        var mi = asm.EntryPoint;
    //        if (mi == null)
    //            throw new NullReferenceException("应用程序缺少入口方法。");

    //        string nameSpace = mi.DeclaringType.Namespace + ".Controllers.";


    //        Type type = asm.GetType(nameSpace + "DefaultController", false);
    //        type = type ?? asm.GetType(nameSpace + "HomeController", false);
    //        if (type == null)
    //            throw new NullReferenceException("未找到默认(DefaultController或HomeController)控制器类型。");

    //        RouteHelper.Runner(type);
    //    }

    //    public static void Runner(Type controllerType)
    //    {
    //        var controller = Activator.CreateInstance(controllerType) as Controller;
    //        controller.Index();
    //    }
    //}

    internal static class RunStackFrame
    {
        public static Type GetInvokeType(int frameIndex)
        {
            int iNext = 1;
            MethodBase method = null;
            do
            {
                StackFrame frame = new StackFrame(iNext++, true);
                method = frame.GetMethod();
                if (method == null) break;
            }
            while (method != null);

            return null;
        }

        public static MethodInfo GetInvokeMethod(int frameIndex)
        {
            return null;
        }
    }
}
