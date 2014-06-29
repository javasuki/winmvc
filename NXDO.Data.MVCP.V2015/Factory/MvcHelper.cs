using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

using NXDO.Data.Attribute;
using NXDO.Data.Extension;
using NXDO.Data.MVCP;

namespace NXDO.Data.Factory
{
    internal interface IMvcHelper
    {
        bool IsRegistered { get; set; }

        void Action(string actionName);
        void Action(string actionName, object anonymousValues);

        void SetModelByController(object oModel);
        void SetModelByController(IEnumerable<object> oModels);
        void SetTableByController(System.Data.DataTable table);

        void SetLinkViewer(object LinkViewer);
        Type GetModelType();
        bool IsGridMvc { get; set; }
        object GridFirstColumnCellValue { get; set; }
        Object GetDataSource();
    }

    /// <summary>
    /// 视图帮助器，该帮助器用于实现模型数据的交互
    /// </summary>
    /// <typeparam name="M">模型类型</typeparam>
    public class MvcHelper<M> : IMvcHelper
        where M : class, new()
    {
        internal MvcHelper(Controller controller)
        {
            this.ViewBag = new System.Dynamic.ExpandoObject();
            this.Controller = controller;
        }

        internal bool IsForWebApi
        {
            get;
            set;
        }

        /// <summary>
        /// 与控制器交互的动态数据包
        /// </summary>
        public dynamic ViewBag
        {
            get;
            internal set;
        }

        #region 内部使用相关方法或属性
        Type IMvcHelper.GetModelType()
        {
            return typeof(M);
        }


        bool isGridMvc;
        bool IMvcHelper.IsGridMvc
        {
            get { return this.isGridMvc; }
            set { this.isGridMvc = value; }
        }

        object gridFirstColumnCellValue;
        object IMvcHelper.GridFirstColumnCellValue
        {
            get { return this.gridFirstColumnCellValue; }
            set { this.gridFirstColumnCellValue = value; }
        }

        #region model/table
        internal M Model
        {
            get;
            set;
        }

        void IMvcHelper.SetModelByController(object oModel)
        {
            this.Model = (M)oModel;
        }

        internal IEnumerable<M> Models
        {
            get;
            set;
        }

        void IMvcHelper.SetModelByController(IEnumerable<object> oModels)
        {
            this.Models = oModels == null ? null : oModels.Cast<M>();
        }

        internal System.Data.DataTable Table
        {
            get;
            set;
        }

        void IMvcHelper.SetTableByController(System.Data.DataTable table)
        {
            this.Table = table;
        }
        #endregion

        /// <summary>
        /// 所在视图实现的接口中，是否已经调用过 Register 方法。
        /// </summary>
        internal bool IsRegistered
        {
            get;
            set;
        }

        bool IMvcHelper.IsRegistered
        {
            get
            {
                return this.IsRegistered;
            }
            set
            {
                this.IsRegistered = value;
            }
        }

        /// <summary>
        /// FillDataSource 调用，以设置类似 DataGridView 上单个 DataSource 属性
        /// </summary>
        /// <returns></returns>
        internal Object GetDataSource()
        {
            if (Models != null)
                return this.Models;
            if (this.Table != null)
                return this.Table;

            return new List<M>();
        }

        Object IMvcHelper.GetDataSource()
        {
            return this.GetDataSource();
        }

        /// <summary>
        /// 当前关联的控制器
        /// </summary>
        internal Controller Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前关联的窗体或控件
        /// </summary>
        internal object LinkViewer
        {
            get;
            set;
        }

        void IMvcHelper.SetLinkViewer(object LinkViewer)
        {
            this.LinkViewer = LinkViewer;
        }
        

        /// <summary>
        /// 从成员访问表达式获取成员
        /// </summary>
        /// <param name="body">表达式</param>
        /// <returns>成员</returns>
        private MemberInfo GetMemberExpression(Expression body)
        {
            var expr = body as MemberExpression;
            if (expr == null)
                throw new ArgumentException("必须是成员访问表达式。");

            return expr.Member;
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        /// <param name="member">成员</param>
        /// <returns>成员值</returns>
        private object GetMemberValue(MemberInfo member)
        {
            if (this.Model == null) return null;
            return 
                (member.MemberType == MemberTypes.Property) ?
                    (member as PropertyInfo).GetValue(this.Model, null) :
                    (member as FieldInfo).GetValue(this.Model);
        }
        #endregion

        #region fill数据源
        /// <summary>
        /// 设置控件的DataSource属性与指定数据源的绑定操作。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="TModel">关联模型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="dataSource">数据源</param>
        /// <param name="anonymousValueDisplay">指定数据源匿名类型，以便指示如何绑定到控件。
        /// <para>m =&gt; new { m.ID, m.Name }; //ID为ValueMember, Name为DisplayMember</para>
        /// <para>例如：ListBox控件具有ValueMember,DisplayMember属性。</para></param>
        public void FillDataList<TControl>(TControl control, System.Data.DataTable dataSource, object anonymousValueDisplay)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (anonymousValueDisplay == null)
                new ArgumentNullException("anonymousValueDisplay");

            var props = anonymousValueDisplay.GetType().GetProperties();
            if (props.Length < 2)
                throw new ArgumentException("无法确定绑定到控件的两个属性。", "anonymousValueDisplay");

            dynamic dyControl = control;
            dyControl.ValueMember = props[0].Name;
            dyControl.DisplayMember = props[1].Name;
            dyControl.DataSource = dataSource;
        }

        /// <summary>
        /// 设置控件的DataSource属性与指定数据源的绑定操作。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="TModel">关联模型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="dataSource">数据源</param>
        /// <param name="ValueDisplayMemberExpr">指定数据源匿名类型表达式，以便指示如何绑定到控件。
        /// <para>m =&gt; new { m.ID, m.Name }; //ID为ValueMember, Name为DisplayMember</para>
        /// <para>例如：ListBox控件具有ValueMember,DisplayMember属性。</para></param>
        public void FillDataList<TControl, TModel>(TControl control, IEnumerable<object> dataSource, Expression<Func<TModel, object>> ValueDisplayMemberExpr)
            where TModel : new()
        {
            if (control == null)
                throw new ArgumentNullException("control");

            if (ValueDisplayMemberExpr == null)
                new ArgumentNullException("ValueDisplayMemberExpr");

            TModel tm = new TModel();
            var anonymousObject = ValueDisplayMemberExpr.Compile().Invoke(tm);
            bool bIsAnonymous = anonymousObject.GetType().Name.StartsWith("<>f__AnonymousType", StringComparison.CurrentCultureIgnoreCase);
            if (!bIsAnonymous)
                throw new ArgumentException("必须保证返回类型为匿名类实例。", "ValueDisplayMemberExpr");

            var props = anonymousObject.GetType().GetProperties();
            if (props.Length < 2)
                throw new ArgumentException("返回的匿名类不具备两个属性。", "ValueDisplayMemberExpr");


            dynamic dyControl = control;
            //Type t = typeof(TControl);
            //t.GetPropertyHelper("ValueMember").SetValue(control, props[0].Name);
            //t.GetPropertyHelper("DisplayMember").SetValue(control, props[1].Name);
            dyControl.ValueMember = props[0].Name;
            dyControl.DisplayMember = props[1].Name;
            dyControl.DataSource = dataSource;
        }

        /// <summary>
        /// 设置控件的DataSource属性与指定枚举类型间的绑定操作。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="dataSource">枚举值数据源，将枚举值设置到控件的ValueMember上。</param>
        public void FillDataList<TControl>(TControl control, Enum dataSource)
        {
            Type enumType = dataSource.GetType();
            var names = Enum.GetNames(enumType);

            var ctor = (new { Text = "", Value = dataSource }).GetType().GetConstructor(new Type[] { typeof(string), enumType });

            List<object> lst = new List<object>();
            var flds = enumType.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static);
            foreach (var fld in flds)
            {
                string disName = DisplayAttribute.GetDisplayName(fld);
                if (string.IsNullOrWhiteSpace(disName))
                    disName = fld.Name;

                var a = ctor.Invoke(new object[] { disName, fld.GetValue(dataSource) });
                lst.Add(a);
            }

            dynamic dyControl = control;
            dyControl.ValueMember = "Value";
            dyControl.DisplayMember = "Text";
            dyControl.DataSource = lst;
        }

        /// <summary>
        /// 设置控件DataSource属性值以呈现绑定数据。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="setting">控件相关的设置</param>
        /// <param name="isBeforeBinding">true：在绑定数据前执行setting，false：在绑定数据后执行setting。</param>
        public void FillDataSource<TControl>(TControl control, Action<TControl> setting, bool isBeforeBinding = false)
        {
            if (isBeforeBinding) setting.Invoke(control);
            this.FillDataSource(control);
            if (!isBeforeBinding) setting.Invoke(control);
        }

        /// <summary>
        /// 设置控件DataSource属性值以呈现绑定数据。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">控件实例</param>
        public void FillDataSource<TControl>(TControl control)
        {
            dynamic dyControl = control;
            var ds = this.GetDataSource();
            dyControl.DataSource = ds;

            if (control is Control && ds is IEnumerable<M>)
                dyControl.Refresh();
        }
        #endregion

        #region for属性
        Dictionary<object,Tuple<string, Expression>> dicMap = new Dictionary<object,Tuple<string,Expression>>();

        #region ForTitle/ForTitleText
        /// <summary>
        /// 模型DisplayAttribute的Name设置到控件指定的属性上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="controlPropertyName">控件属性名称</param>
        /// <param name="modelPropertyName">模型属性名称（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitle<TControl>(TControl control, string controlPropertyName, string modelPropertyName)
        {
            if (this.IsRegistered) return;

            if (control == null)
                throw new ArgumentNullException("control");
            if (string.IsNullOrWhiteSpace(controlPropertyName))
                throw new ArgumentNullException("controlPropertyName");
            if (string.IsNullOrWhiteSpace(modelPropertyName))
                throw new ArgumentNullException("modelPropertyName");

            var modelMember = typeof(M).GetMember(modelPropertyName)[0];
            string title = DisplayAttribute.GetDisplayName(modelMember);

            var ocxMember = control.GetType().GetProperty(controlPropertyName);
            (ocxMember as PropertyInfo).SetValue(control, title, null);
        }

        /// <summary>
        /// 模型DisplayAttribute的Name设置到控件指定的属性上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="controlPropertyExpr">访问控件属性表达式</param>
        /// <param name="modelPropertyName">模型属性名称（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitle<TControl>(TControl control, Expression<Func<TControl, string>> controlPropertyExpr, string modelPropertyName)
        {
            if (this.IsRegistered) return;
            if (control == null)
                throw new ArgumentNullException("control");
            if (controlPropertyExpr == null)
                throw new ArgumentNullException("controlPropertyExpr");
            if (string.IsNullOrWhiteSpace(modelPropertyName))
                throw new ArgumentNullException("modelPropertyName");

            var modelMember = typeof(M).GetMember(modelPropertyName)[0];
            string title = DisplayAttribute.GetDisplayName(modelMember);

            var ocxMember = this.GetMemberExpression(controlPropertyExpr.Body);
            (ocxMember as PropertyInfo).SetValue(control, title, null);
        }

        /// <summary>
        /// 模型DisplayAttribute的Name设置到控件指定的属性上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="controlPropertyExpr">访问控件属性表达式</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitle<TControl, MValue>(TControl control, Expression<Func<TControl, string>> controlPropertyExpr, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            if (this.IsRegistered) return;
            if (control == null)
                throw new ArgumentNullException("control");
            if (controlPropertyExpr == null)
                throw new ArgumentNullException("controlPropertyExpr");
            if (modelPropertyExpr == null)
                throw new ArgumentNullException("modelPropertyExpr");

            var modelMember = this.GetMemberExpression(modelPropertyExpr.Body);
            string title = DisplayAttribute.GetDisplayName(modelMember);
            var ocxMember = this.GetMemberExpression(controlPropertyExpr.Body);
            (ocxMember as PropertyInfo).SetValue(control, title, null);
        }

        /// <summary>
        /// 模型DisplayAttribute的Name设置到控件的Text属性上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitleText<TControl, MValue>(TControl control, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            if (this.IsRegistered) return;
            if (control == null)
                throw new ArgumentNullException("control");
            if (modelPropertyExpr == null)
                throw new ArgumentNullException("modelPropertyExpr");

            var modelMember = this.GetMemberExpression(modelPropertyExpr.Body);
            string title = DisplayAttribute.GetDisplayName(modelMember);
            ((dynamic)control).Text = title;
        }

        /// <summary>
        /// 模型DisplayAttribute的Name设置到控件的Text属性上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="modelPropertyName">模型属性名称（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitleText<TControl, MValue>(TControl control, string modelPropertyName)
        {
            if (this.IsRegistered) return;
            if (control == null)
                throw new ArgumentNullException("control");
            if (string.IsNullOrWhiteSpace(modelPropertyName))
                throw new ArgumentNullException("modelPropertyName");

            var modelMember = typeof(M).GetProperty(modelPropertyName);
            string title = DisplayAttribute.GetDisplayName(modelMember);
            ((dynamic)control).Text = title;
        }
        #endregion

        #region ForText/ForTag/ForValue/ForKnown
        /// <summary>
        /// 设置控件的Text属性与模型关联
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式</param>
        public void ForText<TControl, MValue>(TControl control, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            object val = null;
            if (this.Model != null)
                val = modelPropertyExpr.Compile().Invoke(this.Model);
            //var prop = control.GetType().GetTypeHelper().GetProperty("Text");
            //prop.SetValue(control, val);
            dynamic dyControl = control;
            dyControl.Text = val==null ? string.Empty : val.ToString();

            if (!dicMap.ContainsKey(control))
                this.dicMap.Add(control, new Tuple<string, Expression>("Text", modelPropertyExpr.Body));
        }

        /// <summary>
        /// 设置控件的Tag属性与模型关联
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式</param>
        public void ForTag<TControl, MValue>(TControl control, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            object val = null;
            if (this.Model != null)
                val = modelPropertyExpr.Compile().Invoke(this.Model);
            dynamic dyControl = control;
            dyControl.Tag = val;
        }

        /// <summary>
        /// 设置控件的Value属性与模型关联
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式</param>
        public void ForValue<TControl, MValue>(TControl control, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            object val = null;
            if (this.Model != null)
                val = modelPropertyExpr.Compile().Invoke(this.Model);

            var prop = control.GetType().GetTypeHelper().GetProperty("Value");
            if (val != null)
            {                
                PropertyInfo pinfo = prop;
                Type pType = pinfo.PropertyType;
                val = Convert.ChangeType(val, pType);
            }
            prop.SetValue(control, val);

            if (!dicMap.ContainsKey(control))
                this.dicMap.Add(control, new Tuple<string, Expression>("Value", modelPropertyExpr.Body));
        }

        

        /// <summary>
        /// 设置控件已知属性与模型关联。
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue"></typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="expr">访问控件属性表达式</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式</param>
        /// <param name="funcConvertTo">模型属性值是否需要转换成其它类型。</param>
        public void ForKnown<TControl, MValue>(TControl control, Expression<Func<TControl, object>> controlPropertyExpr, Expression<Func<M, MValue>> modelPropertyExpr, Func<M, object> funcConvertTo = null)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (controlPropertyExpr == null)
                throw new ArgumentNullException("controlPropertyExpr");
            if (modelPropertyExpr == null)
                throw new ArgumentNullException("modelPropertyExpr");


            var ocxMember = this.GetMemberExpression(controlPropertyExpr.Body);
            if (this.Model != null)
            {
                object val = funcConvertTo == null ?
                                    modelPropertyExpr.Compile().Invoke(this.Model) :
                                    funcConvertTo.Invoke(this.Model);
                (ocxMember as PropertyInfo).SetValue(control, val, null);
            }

            if (!dicMap.ContainsKey(control))
                this.dicMap.Add(control, new Tuple<string, Expression>(ocxMember.Name, modelPropertyExpr.Body));
        }
        #endregion
        #endregion

        #region for事件
        /// <summary>
        /// 将事件处理程序添加到指定控件的事件源。
        /// <para> view.ForEvent(控件,"Click",(s,e)=&gt;事件处理代码);</para>
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventAction">事件处理程序</param>
        public void ForEvent(object control, string eventName, Action<object, EventArgs> eventAction)
        {
            this.ForEvent<EventArgs>(control, eventName, eventAction);
        }

        /// <summary>
        /// 将事件处理程序添加到指定控件的事件源。
        /// <para> view.ForEvent(控件,"Click",(s,e)=&gt;事件处理代码);</para>
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型</typeparam>
        /// <param name="control">控件</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventAction">事件处理程序</param>
        public void ForEvent<TEventArgs>(object control, string eventName, Action<object, TEventArgs> eventAction)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException("eventName");
            if (control == null)
                throw new ArgumentNullException("control");
            if (eventAction == null)
                throw new ArgumentNullException("eventAction");


            var eventInfo = control.GetType().GetEvent(eventName);
            if (eventInfo == null)
                throw new ArgumentException("未找到" + eventName + "事件。", "eventName");
            var eventDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, new EventWrapper<TEventArgs>(eventAction), "Invoke");
            eventInfo.AddEventHandler(control, eventDelegate);
        }        
        #endregion

        #region action动作
        #region public ActionClick
        /// <summary>
        /// 处理控件的Click事件以执行控制器指定操作。
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialog">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionClick(object control, string actionName, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            this.ActionClick(control, actionName, null, toAction, actionDialogMode);
        }

        /// <summary>
        /// 处理控件的Click事件以执行控制器指定操作。
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialogMode">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionClick(object control, string actionName, object anonymousValues, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            this.ActionByEvent(control, "Click", actionName, anonymousValues, toAction, actionDialogMode);
        }
        #endregion

        #region public Action ByEvent
        /// <summary>
        /// 处理控件指定事件以执行控制器指定操作。
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="eventName">指定事件名称</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialogMode">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionByEvent(object control, string eventName, string actionName, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            this.ActionByEvent(control, eventName, actionName, null, toAction, actionDialogMode);
        }

        /// <summary>
        /// 处理控件指定事件以执行控制器指定操作。
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="eventName">指定事件名称</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialogMode">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionByEvent(object control, string eventName, string actionName, object anonymousValues, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            this.ActionByEvent<EventArgs>(control, eventName, actionName, anonymousValues, toAction, actionDialogMode);
        }

        /// <summary>
        /// 处理控件指定事件（非EventArgs的参数）以执行控制器指定操作。
        /// </summary>
        /// <typeparam name="TEventArgs">事件的参数类型</typeparam>
        /// <param name="control">控件</param>
        /// <param name="eventName">指定事件名称</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialogMode">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionByEvent<TEventArgs>(object control, string eventName, string actionName, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            this.ActionByEvent<TEventArgs>(control, eventName, actionName, null, toAction, actionDialogMode);
        }

        /// <summary>
        /// 处理控件指定事件（非EventArgs的参数）以执行控制器指定操作。
        /// </summary>
        /// <typeparam name="TEventArgs">事件的参数类型</typeparam>
        /// <param name="control">控件</param>
        /// <param name="eventName">指定事件名称</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialogMode">如果操作类型为Dialog时，可处理子操作的返回值。</param>
        public void ActionByEvent<TEventArgs>(object control, string eventName, string actionName, object anonymousValues, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            if (toAction == ToActionFlag.View || toAction == ToActionFlag.ViewActived)
                this.ForEvent<TEventArgs>(control, eventName, (s, e) => this.Action(actionName, toAction == ToActionFlag.View, anonymousValues));
            else if (toAction == ToActionFlag.Child || toAction == ToActionFlag.ChildActived)
                this.ForEvent<TEventArgs>(control, eventName, (s, e) => this.ActionChild(actionName, toAction == ToActionFlag.Child, anonymousValues));
            else if (toAction == ToActionFlag.Dialog)
            {
                this.ForEvent<TEventArgs>(control, eventName,
                    (s, e) =>
                    {
                        var rMode = this.ActionDialog(actionName, anonymousValues);
                        if (actionDialogMode != null)
                            actionDialogMode.Invoke(rMode);
                    });
            }
        }
        #endregion

        #region Action
        /// <summary>
        /// 操作返回以模态方式呈现视图
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        /// <returns>模态视图的返回值</returns>
        public ActionMode ActionDialog(string actionName, object anonymousValues = null)
        {
            this.Controller.ModeBag.CreateMode = ViewCreateFlag.FormDialog;
            this.Action(actionName, false, anonymousValues);
            return this.Controller.ModeBag.ModeDialogResult.GetActionMode();
        }

        /// <summary>
        /// 操作返回以Mdi子视图方式呈现视图 (如果存在子视图则激活)
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        /// <param name="actionName">操作名称</param>
        public void ActionChild(string actionName)
        {
            this.ActionChild(actionName, false, null);
        }

        /// <summary>
        /// 操作返回以Mdi子视图方式呈现视图 (如果存在子视图则激活)
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        public void ActionChild(string actionName, object anonymousValues)
        {
            this.ActionChild(actionName, false, anonymousValues);
        }

        /// <summary>
        /// 操作返回以Mdi子视图方式呈现视图
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="isAlwaysCreate">true:始终创建子视图，反之为激活子视图。</param>
        public void ActionChild(string actionName, bool isAlwaysCreate)
        {
            this.ActionChild(actionName, isAlwaysCreate, null);
        }

        /// <summary>
        /// 操作返回以Mdi子视图方式呈现视图
        /// <para>不存在Mdi父视图时，作为普通视图呈现。</para>
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="isAlwaysCreate">true:始终创建子视图，反之为激活子视图。</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        public void ActionChild(string actionName, bool isAlwaysCreate, object anonymousValues)
        {
            this.Controller.ModeBag.CreateMode = ViewCreateFlag.FormMdi;
            this.Controller.ModeBag.IsAlwaysCreate = isAlwaysCreate;
            this.Action(actionName, isAlwaysCreate, anonymousValues);
        }

        /// <summary>
        /// 执行控制器的操作(如果存在视图则激活)
        /// </summary>
        /// <param name="actionName">操作名称</param>
        public void Action(string actionName)
        {
            this.Action(actionName, false, null);
        }

        /// <summary>
        /// 执行控制器的操作(如果存在视图则激活)
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        public void Action(string actionName, object anonymousValues)
        {
            this.Action(actionName, false, anonymousValues);
        }

        /// <summary>
        /// 执行控制器的操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="isAlwaysCreate">true:始终创建视图，反之为激活视图。</param>
        public void Action(string actionName, bool isAlwaysCreate)
        {
            this.Action(actionName, isAlwaysCreate, null);
        }

        /// <summary>
        /// 执行控制器的操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="isAlwaysCreate">true:始终创建视图，反之为激活视图。</param>
        /// <param name="anonymousValues">方法参数，必须提供模型类型或匿名类型。</param>
        public void Action(string actionName, bool isAlwaysCreate, object anonymousValues)
        {
            object oValues = anonymousValues ?? new { };
            bool bIsAnonymous = oValues.GetType().Name.StartsWith("<>f__AnonymousType", StringComparison.CurrentCultureIgnoreCase);
            if (!bIsAnonymous)
            {
                if(!(anonymousValues is M))
                    throw new ArgumentException("必须保证传入参数为匿名类实例或模型实例。", "anonymousValues");
            }

            this.ExecuteAction(actionName, new object[] { oValues });
        }

        private void ExecuteAction(string actionName, params object[] values)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            this.Controller.IsInPartialView = this.LinkViewer is UserControl;
            dynamic viewBag = NXDO.Data.Factory.ActionHelper.ExecuteAction(actionName, this.Controller, this.ViewBag, values);
            this.ViewBag = viewBag;
        }
        #endregion

        #endregion

        #region Partial
        Dictionary<Type, System.Windows.Forms.UserControl> dicPartials = new Dictionary<Type, System.Windows.Forms.UserControl>();
        /// <summary>
        /// 设定视图中存在指定的分部视图
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <param name="control">作为分部视图的控件实例</param>
        public void Partial<TControl>(TControl control)
            where TControl : System.Windows.Forms.UserControl
        {
            if (control == null)
                throw new ArgumentNullException("control");

            Type type = typeof(TControl);
            if (!dicPartials.ContainsKey(type))
                dicPartials.Add(type, control);
        }

        /// <summary>
        /// 初始化设置指定的分部视图
        /// </summary>
        /// <typeparam name="TControl">分部视图类型</typeparam>
        /// <param name="control">分部视图实例</param>
        public void RenderPartial<TControl>(TControl control)
            where TControl : System.Windows.Forms.UserControl
        {
            this.RenderPartial(control, this.Model ?? new M());
        }

        /// <summary>
        /// 初始化设置指定的分部视图
        /// </summary>
        /// <typeparam name="TControl">分部视图类型</typeparam>
        /// <param name="control">分部视图实例</param>
        /// <param name="model">模型实例</param>
        public void RenderPartial<TControl>(TControl control, M model)
            where TControl : System.Windows.Forms.UserControl
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Partial(control);
            this.Controller.ViewBag = this.ViewBag;
            PartialViewResult.GetInstanceForRender().SetPartialViewHelper(control, typeof(TControl), this.Controller, model, this.Models ?? new List<M> { }, this.Table);
        }

        /// <summary>
        /// 获取分部视图实例
        /// </summary>
        /// <param name="typeControl">控件类型</param>
        /// <returns>分部视图实例</returns>
        internal System.Windows.Forms.UserControl GetPartial(Type typeControl)
        {
            if (!dicPartials.ContainsKey(typeControl)) return null;
            return dicPartials[typeControl];
        }
        #endregion

        #region UpdateModel,在Controller中实现UpdateModel/TryUpdateModel,都调用到本方法
        /// <summary>
        /// 更新模型数据
        /// </summary>
        /// <param name="m">传入的模型</param>
        internal Exception UpdateModel(M m)
        {
            if (m == null)
                throw new ArgumentNullException("m");

            Exception ex = null;
            if (!this.isGridMvc)
            {
                #region 分部视图的编辑器
                foreach (Control control in this.dicMap.Keys)
                {
                    var tuple = this.dicMap[control];

                    object ocxVal = control.GetType().GetPropertyHelper(tuple.Item1).GetValue(control);
                    var mPropInfo = this.GetMemberExpression(tuple.Item2) as PropertyInfo;

                    Type type = Nullable.GetUnderlyingType(mPropInfo.PropertyType) ?? mPropInfo.PropertyType;
                    if (ocxVal != null)
                    {
                        if (ocxVal.GetType() != type)
                            ocxVal = Convert.ChangeType(ocxVal, type);

                        try
                        {
                            mPropInfo.SetValueEx(m, ocxVal);
                        }
                        catch (Exception exx)
                        {
                            if (ex == null)
                                ex = exx.InnerException ?? exx;
                        }
                    }
                }
                return ex;
                #endregion
            }

            if (this.gridFirstColumnCellValue == null) return ex;

            dynamic dyCell = this.gridFirstColumnCellValue;
            object keyValue = null;
            string keyName = string.Empty; 
            bool isModelType = this.gridFirstColumnCellValue.GetType() == typeof(M);
            if (!isModelType)
            {
                keyValue = dyCell.Value;
                keyName = dyCell.Name;
            }

            if (!isModelType && keyValue == null) return ex;
            var dt = this.GetDataSource() as System.Data.DataTable;
            if (dt != null)
            {
                #region 数据源为 DataTable
                var rows = dt.Select(keyName + "=" + keyValue.ToString());
                if (rows.Length == 0) return ex;
                var row = rows[0];

                foreach (var prop in typeof(M).GetProperties())
                {
                    object oVal = row[prop.Name];
                    if (oVal == null) continue;
                    if (oVal == DBNull.Value) continue;
                    try
                    {
                        prop.SetValueEx(m, oVal);
                    }
                    catch (Exception exx)
                    {
                        if (ex == null)
                            ex = exx.InnerException ?? exx;
                    }
                }
                #endregion
            }
            else
            {
                #region 数据源为迭代器
                try
                {
                    M singleModel = null;
                    if (!isModelType)
                    {
                        //建立比较表达式 m.KeyMame == keyValue
                        var ec = Expression.Constant(keyValue, keyValue.GetType());
                        var instance = Expression.Parameter(typeof(M), "model");
                        var epi = Expression.MakeMemberAccess(instance, typeof(M).GetProperty(keyName));
                        var exp = Expression.Equal(epi, ec);
                        var lambda = Expression.Lambda<Func<M, bool>>(exp, instance);


                        var lstData = this.Models.ToList();
                        var ms = lstData.Where(lambda.Compile()).Select(mm => mm);
                        if (ms.Count() == 0) return ex;
                        singleModel = ms.First();
                    }
                    else
                        singleModel = (M)this.gridFirstColumnCellValue;

                    foreach (var pp in singleModel.GetType().GetProperties())
                    {
                        pp.SetValueEx(m, pp.GetValueEx(singleModel));
                    }
                }
                catch (Exception exx)
                {
                    if (ex == null)
                        ex = exx.InnerException ?? exx;
                }
                #endregion
            }

            return ex;
        }
        #endregion
    }

    #region 事件包装
    /// <summary>
    /// 事件包装类
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    internal class EventWrapper<TEventArgs>
    {
        public static object GetWrapper(Delegate delg)
        {
            return new EventWrapper<TEventArgs>((Action<object, TEventArgs>)delg);
        }

        Action<object, TEventArgs> EventAction;
        public EventWrapper(Action<object, TEventArgs> eventAction)
        {
            this.EventAction = eventAction;
        }

        public void Invoke(object sender, TEventArgs e)
        {
            if (EventAction == null) return;
            this.EventAction.Invoke(sender, e);
        }
    }
    #endregion
}
