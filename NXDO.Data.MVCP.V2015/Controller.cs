using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NXDO.Data.Factory;
using NXDO.Data.MVCP.Routing;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 控制器基类
    /// </summary>
    public abstract class Controller
    {
        static readonly object lockObject = new object();
        #region 映射建立的视图实例
        /// <summary>
        /// 控制器调用 new ViewResult() 后，缓存的视图实例
        /// </summary>
        internal static ControllerViewsInstance ViewInstances
        {
            get
            {
                if (_ViewInstances == null)
                {
                    lock (lockObject)
                    {
                        if (_ViewInstances == null)
                            _ViewInstances = new ControllerViewsInstance();
                    }
                }
                return _ViewInstances;
            }
        }static ControllerViewsInstance _ViewInstances;
        #endregion

        #region 获取视图的类型名
        string _viewTypePrefixName = "";
        bool bGetViewTypePrefixName;
        /// <summary>
        /// 获取视图的所在子目录的名称。
        /// </summary>
        /// <returns></returns>
        private string GetViewTypePrefixName()
        {
            if (bGetViewTypePrefixName)
                return _viewTypePrefixName;

            bGetViewTypePrefixName = true;
            var objs = this.GetType().GetCustomAttributes(typeof(NXDO.Data.Attribute.MvcViewNameAttribute), true);
            if (objs.Length == 0)
                return "";

            var attr = objs[0] as NXDO.Data.Attribute.MvcViewNameAttribute;
            _viewTypePrefixName = attr.PrefixName;
            return _viewTypePrefixName;
        }

        /// <summary>
        /// 获取视图的类型名
        /// </summary>
        /// <returns></returns>
        private string GetViewTypeName()
        {
            Type controllerType = this.GetType();
            string typeName = controllerType.Name;
            if (!typeName.EndsWith("Controller", StringComparison.CurrentCultureIgnoreCase))
                throw ControllerException.GetNameError(typeName);

            string viewName = typeName.Substring(0, typeName.Length - "Controller".Length);
            string s = this.GetViewTypePrefixName();
            return string.IsNullOrEmpty(s) ? viewName : s + "." + viewName;
        }

        private string[] GetPartialViewTypeName(string partialViewName)
        {
            //xx为viewName
            //搜索：xxPartialViewName　或　xxs.PartialViewName 或　xxs.xxPartialViewName
            //其中　xxs　为命名空间
            string viewName = this.GetViewTypeName();

            bool isStartView = partialViewName.StartsWith(viewName, StringComparison.CurrentCultureIgnoreCase);
            if (isStartView)
                partialViewName = partialViewName.Substring(viewName.Length, partialViewName.Length - viewName.Length);

            return new string[] { viewName + partialViewName, 
                                  viewName + "s." + partialViewName, 
                                  viewName + "s." + viewName + partialViewName
                                };
        }
        #endregion

        #region index
        /// <summary>
        /// View()方法是否在 Index 操作中被调用
        /// <para>除Index中创建View以外，其它子操作均为激活View，并传递交互数据。</para>
        /// </summary>
        internal object CurrentMvcHelper
        {
            get;
            set;
        }

        /// <summary>
        /// 当前对应的分部视图帮助器
        /// </summary>
        internal object CurrentPartialMvcHelper
        {
            get;
            set;
        }

        /// <summary>
        /// 是否在分部视图中提交到操作。
        /// </summary>
        internal bool IsInPartialView
        {
            get;
            set;
        }


        

        /// <summary>
        /// 控制器默认的入口操作。
        /// </summary>
        /// <returns></returns>
        public virtual ViewResult Index(params object[] values)
        {
            this.ViewBag.Values = values;
            return this.View();
        }
        #endregion

        #region 数据传递
        /// <summary>
        /// 模态数据包
        /// </summary>
        internal ViewFormBag ModeBag
        {
            get
            {
                if (_ModeBag == null)
                    _ModeBag = new ViewFormBag();
                return _ModeBag;
            }
        }ViewFormBag _ModeBag;

        /// <summary>
        /// 与视图交互的动态数据包
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                if (_ViewBag == null)
                    _ViewBag = new System.Dynamic.ExpandoObject();
                return _ViewBag;
            }
            internal set
            {
                _ViewBag = value;
            }
        }dynamic _ViewBag;
        #endregion

        #region 获取调用方的方法名称
        /// <summary>
        /// 获取View/PartialView调用方的Action名称
        /// </summary>
        /// <param name="checkIsPartialView">是否为检查PartialView的调用方</param>
        /// <param name="startFrame">起始帧</param>
        /// <returns>Action名称</returns>
        private string GetActionName(bool checkIsPartialView, int startFrame = 2)
        {
            int iNext = startFrame;
            System.Reflection.MethodBase method = null;
            do
            {
                System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(iNext++, true);
                method = frame.GetMethod();
                if (method == null) break;

                if (method.DeclaringType == null) break;
                if (method.DeclaringType.IsSubclassOf(typeof(Controller)) || method.DeclaringType == typeof(Controller))
                {
                    if (method.Name.CompareTo("PartialView") == 0 && checkIsPartialView) continue;
                    if (checkIsPartialView)
                        return method.Name;

                    if (method.Name.CompareTo("Index") == 0 && !checkIsPartialView)
                        return method.Name;
                }
                else
                {
                    if (!checkIsPartialView)
                        break;
                }
            }
            while (method != null);

            return string.Empty;
        }

        /// <summary>
        /// View的调用方是否为 Index 操作
        /// </summary>
        /// <returns></returns>
        bool checkIsIndexAction()
        {
            string name = this.GetActionName(false, 3);
            if (string.IsNullOrWhiteSpace(name)) return false;
            return "Index".CompareTo(name) == 0;
        }
        #endregion

        #region view
        /// <summary>
        /// 呈现视图
        /// </summary>
        /// <returns>视图结果</returns>
        protected internal ViewResult View()
        {
            bool isIndexAction = checkIsIndexAction();
            return new ViewResult(isIndexAction, this.GetViewTypeName(), this);
        }

        /// <summary>
        /// 呈现视图
        /// </summary>
        /// <param name="model">模型实例</param>
        /// <returns>视图结果</returns>
        protected internal ViewResult View(object model)
        {
            bool isIndexAction = checkIsIndexAction();
            return new ViewResult(isIndexAction, this.GetViewTypeName(), this, model);
        }

        /// <summary>
        /// 呈现视图
        /// </summary>
        /// <param name="dataTable">数据表</param>
        /// <returns>视图结果</returns>
        protected internal ViewResult View(System.Data.DataTable dataTable)
        {
            bool isIndexAction = checkIsIndexAction();
            return new ViewResult(isIndexAction, this.GetViewTypeName(), this, dataTable);
        }

        /// <summary>
        /// 呈现视图
        /// </summary>
        /// <param name="models">模型迭代器实例</param>
        /// <returns>视图结果</returns>
        protected internal ViewResult View(IEnumerable<object> models)
        {
            if (models == null)
                throw new ArgumentNullException("models");

            Type[] types = models.GetType().GetGenericArguments();
            if (types.Length==0)
                throw new ArgumentException("泛型枚举器缺失泛型类型，GenericType。", "models");
            var objModel = Activator.CreateInstance(types[0]);

            bool isIndexAction = checkIsIndexAction();
            return new ViewResult(isIndexAction, this.GetViewTypeName(), this, objModel, models);
        }
        #endregion

        #region partial view
        /// <summary>
        /// 呈现与Contoller对应视图名为前缀并和Action同名的分部视图
        /// </summary>
        /// <returns>分部视图结果</returns>
        public PartialViewResult PartialView()
        {
            string vpName = this.GetActionName(true);
            return new PartialViewResult(this.GetPartialViewTypeName(vpName), this, null);
        }

        /// <summary>
        /// 呈现与Contoller对应视图名为前缀并和Action同名的分部视图
        /// </summary>
        /// <param name="model">模型实例</param>
        /// <returns>分部视图结果</returns>
        public PartialViewResult PartialView(object model)
        {
            string vpName = this.GetActionName(true);
            return new PartialViewResult(this.GetPartialViewTypeName(vpName), this, model);
        }

        /// <summary>
        /// 呈现指定视图名称的分部视图
        /// </summary>
        /// <param name="viewName">分部视图名称</param>
        /// <returns>分部视图结果</returns>
        public PartialViewResult PartialView(string viewName)
        {
            return new PartialViewResult(this.GetPartialViewTypeName(viewName), this);
        }

        /// <summary>
        /// 呈现指定视图名称的分部视图
        /// </summary>
        /// <param name="viewName">分部视图名称</param>
        /// <param name="model">模型实例</param>
        /// <returns>分部视图结果</returns>
        public PartialViewResult PartialView(string viewName, object model)
        {
            return new PartialViewResult(this.GetPartialViewTypeName(viewName), this, model);
        }

        //protected internal PartialViewResult PartialView(System.Data.DataTable dataTable)
        //{
        //}

        //protected internal PartialViewResult PartialView(string viewName, System.Data.DataTable dataTable)
        //{
        //}

        /// <summary>
        /// 呈现与Contoller对应视图名为前缀并和Action同名的分部视图
        /// </summary>
        /// <param name="models">模型迭代器实例</param>
        /// <returns>分部视图结果</returns>
        protected internal PartialViewResult PartialView(IEnumerable<object> models)
        {
            string vpName = this.GetActionName(true);
            return this.PartialView(vpName, models);
        }

        /// <summary>
        /// 呈现指定视图名称的分部视图
        /// </summary>
        /// <param name="viewName">分部视图名称</param>
        /// <param name="models">模型迭代器实例</param>
        /// <returns>分部视图结果</returns>
        protected internal PartialViewResult PartialView(string viewName, IEnumerable<object> models)
        {
            if (models == null)
                throw new ArgumentNullException("models");

            Type[] types = models.GetType().GetGenericArguments();
            if (types.Length == 0)
                throw new ArgumentException("泛型枚举器缺失泛型类型，GenericType。", "models");
            var objModel = Activator.CreateInstance(types[0]);

            return new PartialViewResult(this.GetPartialViewTypeName(viewName), this, objModel, models);
        }
        #endregion

        #region RedirectToAction
        /// <summary>
        /// 使用操作名称和控制器名称重定向到指定的操作，并呈现重定向的视图。
        /// </summary>
        /// <param name="actionName">操作的名称</param>
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
        /// <returns>操作重定向结果</returns>
        protected internal RedirectToRouteResult RedirectToAction(string actionName, object anonymousValues = null)
        {
            return this.RedirectToAction(actionName, this.GetType().Name);
        }

        /// <summary>
        /// 使用操作名称和控制器名称重定向到指定的操作，并呈现重定向的视图。
        /// </summary>
        /// <param name="actionName">操作的名称</param>
        /// <param name="controllerName">控制器的名称</param>
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
        /// <returns>操作重定向结果</returns>
        protected internal RedirectToRouteResult RedirectToAction(string actionName, string controllerName, object anonymousValues = null)
        {
            Type type = MvcApplication.GetType(controllerName, GetTypeMode.Controller, true);
            return this.RedirectToAction(actionName, type);
        }

        /// <summary>
        /// 重定向到某一控制器的Index操作
        /// </summary>
        /// <param name="controllerType">控制器类型</param>
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
        /// <returns>操作重定向结果</returns>
        protected internal RedirectToRouteResult RedirectToAction(Type controllerType, object anonymousValues = null)
        {
            return this.RedirectToAction("Index", controllerType);
        }


        /// <summary>
        /// 重定向到某一控制器的子操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="controllerType">控制器类型</param>
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
        /// <returns>操作重定向结果</returns>
        protected internal RedirectToRouteResult RedirectToAction(string actionName, Type controllerType, object anonymousValues = null)
        {
            var pValues = anonymousValues ?? new { };
            var r = new RedirectToRouteResult(actionName, controllerType, this.ModeBag, this.ViewBag, pValues);
            this.ModeBag.ModeDialogResult = r.RedirectModeExecuteResult;
            this.ViewBag = r.ViewBag;
            return r;
        }
        #endregion

        #region empty
        EmptyResult emptyResylt;
        /// <summary>
        /// 呈现空视图，为满足对话框控件产生视图。
        /// </summary>
        /// <returns>空视图结果</returns>
        protected internal EmptyResult Empty()
        {
            if (emptyResylt == null)
                emptyResylt = new EmptyResult();
            return emptyResylt;
        }
        #endregion


        /// <summary>
        /// 更新指定的模型实例。
        /// </summary>
        /// <typeparam name="TModel">模型对象的类型</typeparam>
        /// <param name="model">模型实例</param>
        protected internal void UpdateModel<TModel>(TModel model)
        {
            dynamic dyMvc = this.IsInPartialView ? this.CurrentPartialMvcHelper : this.CurrentMvcHelper;
            Exception ex = dyMvc.UpdateModel(model);
            if (ex != null)
                throw ex;
        }

        /// <summary>
        /// 尝试更新指定的模型实例。
        /// </summary>
        /// <typeparam name="TModel">模型对象的类型</typeparam>
        /// <param name="model">模型实例</param>
        /// <returns>如果更新已成功，则为 true；否则为 false。</returns>
        protected internal bool TryUpdateModel<TModel>(TModel model)
        {
            bool b = true;
            try
            {
                this.UpdateModel(model);
            }
            catch
            {
                b = false;
            }
            return b;
        }
    }

    /// <summary>
    /// 控制器异常
    /// </summary>
    public class ControllerException : Exception
    {
        ControllerException(string Message) : base(Message)
        {
        }

        internal static ControllerException GetNameError(string controllerTypeName)
        {
            return new ControllerException(controllerTypeName + "类型定义必须以 Controller 为后缀。");
        }

        internal static ControllerException GetNoActionError(string controllerTypeName,string actionName)
        {
            return new ControllerException(controllerTypeName + "，缺少“" + actionName + "”方法。");
        }

        internal static ControllerException GetMoreActionError(string controllerTypeName, string actionName)
        {
            return new ControllerException(controllerTypeName + "，无法对应“" + actionName + "”多个重载方法，请明确匿名类中属性的类型。");
        }

        internal static ControllerException GetActionParamCountError(string controllerTypeName, string actionName)
        {
            return new ControllerException(controllerTypeName + "的“" + actionName + "”操作参数数量不匹配。");
        }

        internal static ControllerException GetActionParamTypeError(string controllerTypeName, string actionName)
        {
            return new ControllerException(controllerTypeName + "的“" + actionName + "”操作参数类型不匹配。");
        }
    }

    /// <summary>
    /// 控制器映射已经建立的Form视图实例
    /// </summary>
    internal class ControllerViewsInstance
    {
        ConcurrentDictionary<Type, List<object>> dics = new ConcurrentDictionary<Type, List<object>>();
        /// <summary>
        /// 添加一个视图实例到对应类型的集合中
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="viewInstance">视图实例</param>
        public void Add(Type viewType, object viewInstance)
        {
            if (viewInstance == null) return;
            if (dics.ContainsKey(viewType))
            {
                var lst = dics[viewType];
                if (!lst.Contains(viewInstance))
                    lst.Add(viewInstance);
                return;
            }
            this.dics.TryAdd(viewType, new List<object> { viewInstance });
        }

        /// <summary>
        /// 移除某一视图类型的所有实例
        /// </summary>
        /// <param name="viewType"></param>
        public void RemoveKey(Type viewType)
        {
            if (!dics.ContainsKey(viewType)) return;
            List<object> os = null;
            this.dics.TryRemove(viewType, out os);
        }

        /// <summary>
        /// 移除某一视图类型下,集合中的某一视图实例
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="viewsInstance">某一视图实例</param>
        public void RemoveContent(Type viewType, object viewsInstance)
        {
            if (!dics.ContainsKey(viewType)) return;
            var lst = dics[viewType];

            if (!lst.Contains(viewsInstance)) return;
            lst.Remove(viewsInstance);
        }

        /// <summary>
        /// 获取某一视图类型的多个实例集合
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <returns>同一类型视图实例集合,如果不存在该类型集合,则返回0个元素的List&lt;&gt;集合</returns>
        public List<object> Get(Type viewType)
        {
            List<object> os = null;
            if (!dics.ContainsKey(viewType)) return new List<object>();
            this.dics.TryGetValue(viewType, out os);
            return os;
        }
    }
}
