using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NXDO.Data.MVCP;
using NXDO.Data.Extension;

namespace NXDO.Data.Factory
{
    /// <summary>
    /// 操作执行的帮助器
    /// </summary>
    internal class ActionHelper
    {
        /// <summary>
        /// 执行操作，并返回 ViewBag 动态对象
        /// </summary>
        /// <typeparam name="TController">控制器类型</typeparam>
        /// <param name="actionName">操作名称</param>
        /// <param name="controller">控制器实例</param>
        /// <param name="ViewBag">视图或控制器中的ViewBag</param>
        /// <param name="values">操作所需的参数值</param>
        /// <returns>ViewBag</returns>
        public static dynamic ExecuteAction<TController>(string actionName, TController controller, dynamic ViewBag, params object[] values)
            where TController : NXDO.Data.MVCP.Controller
        {
            var pvs = ActionHelper.GetParams(values);
            int prmLength = pvs.Count;
            var methods = controller.GetType().GetTypeHelper().GetMethods(mi => mi.Name == actionName && mi.GetParameters().Length == prmLength);
            if (methods.Count() == 0)
            {
                methods = controller.GetType().GetTypeHelper().GetMethods(mi => mi.Name.ToLower() == actionName.ToLower() && mi.GetParameters().Length == prmLength);
                if(methods.Count() == 0)
                    throw ControllerException.GetNoActionError(controller.GetType().Name, actionName);
            }

            //执行子操作前,将VIEW中的ViewBag传递给控制器
            controller.ViewBag = ViewBag;

            #region 一个方法时，假定它是参数类型匹配的方法
            if (methods.Count() == 1)
            {
                var singleMethod = methods.First();
                ActionHelper.CheckActionParamter(pvs, singleMethod, controller, actionName);
                
                //找到一个方法，可能参数类型不匹配，此处不处理，调用时会引发异常
                if (values.Length == 0)
                    singleMethod.Invoke(controller);
                else if (values.Length > 0)
                    singleMethod.Invoke(controller, pvs.ToArray());

                controller.ModeBag.CreateMode = ViewCreateFlag.Form;
                controller.ModeBag.IsAlwaysCreate = false;
                return controller.ViewBag; //执行子操作后,将控制器中的ViewBag返回给 View
            }
            #endregion

            //多个方法时，如果参数值含有null,无法判断控制器的参数类型是否匹配
            int iCount = (from p in pvs where p == null select p).Count();
            if (iCount > 0)
                throw ControllerException.GetMoreActionError(controller.GetType().Name, actionName);


            #region 找到多个方法时，进行参数类型的匹配
            NXDO.Data.Reflection.AMethodHelper miMatch = null;
            foreach (var m in methods)
            {
                var lParam = ((MethodInfo)m).GetParameters();
                bool isMatch = true;
                for (int i = 0; i < pvs.Count; i++)
                {
                    if (lParam[i].ParameterType != pvs[i].GetType()) isMatch = false;
                    if (!isMatch) break;
                }

                if (!isMatch) continue;
                miMatch = m;
                break;
            }
            #endregion

            if (miMatch == null)
                throw ControllerException.GetMoreActionError(controller.GetType().Name, actionName);
            miMatch.Invoke(controller, pvs.ToArray());

            controller.ModeBag.CreateMode = ViewCreateFlag.Form;
            controller.ModeBag.IsAlwaysCreate = false;

            //执行子操作后,将控制器中的ViewBag返回设置给 View
            return controller.ViewBag;
        }

        #region action执行时，需要解析参数对象
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<object> GetParams(params object[] values)
        {
            var lst = new List<object>();
            if (values == null) return lst;
            if (values.Length == 0) return lst;

            foreach (var o in values)
            {
                if (o == null)
                {
                    lst.Add(o);
                    continue;
                }

                string tName = o.GetType().FullName;
                bool bIsAnonymous = tName.StartsWith("<>f__AnonymousType", StringComparison.CurrentCultureIgnoreCase);
                if (bIsAnonymous)
                {
                    var pinfos = o.GetType().GetProperties();
                    foreach (var pinfo in pinfos)
                    {
                        lst.Add(pinfo.GetValue(o, null));
                    }
                    //Console.WriteLine(tName);
                    continue;
                }

                lst.Add(o);
            }

            return lst;
        }
        #endregion


        internal static Type ModelType
        {
            get;
            set;
        }

        /// <summary>
        /// 检查参数的合法性
        /// </summary>
        /// <param name="pvs"></param>
        /// <param name="method"></param>
        /// <param name="controller"></param>
        /// <param name="actionName"></param>
        private static void CheckActionParamter(List<object> pvs, MethodInfo method, Controller controller, string actionName)
        {
            var lParam = method.GetParameters();
            if (lParam.Length != pvs.Count)
                throw ControllerException.GetActionParamCountError(controller.GetType().Name, actionName);

            var mvcHelper = (controller.IsInPartialView ? controller.CurrentPartialMvcHelper : controller.CurrentMvcHelper) as IMvcHelper;

            bool isMatch = true;
            //int iPrmModelIndex = -1;
            for (int i = 0; i < pvs.Count; i++)
            {
                Type pvType = pvs[i].GetType(); //传入参数值的类型
                Type pdType = lParam[i].ParameterType; //方法所定义的参数类型
                if (pdType == pvType) continue;
                isMatch = false;
                break;
                //if (pdType == mvcHelper.GetModelType() && pvType.IsValueType)
                //    iPrmModelIndex = i;
            }

            if (!isMatch)
                throw ControllerException.GetActionParamTypeError(controller.GetType().Name, actionName);

            //if (iPrmModelIndex < 0 && !isMatch)
            //    throw ControllerException.GetActionParamTypeError(controller.GetType().Name, actionName);

            //mvcHelper.GetModelByKey(pvs[iPrmModelIndex]);
        }
    }
}
