using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

using NXDO.Data.Attribute;
using NXDO.Data.Extension;
using NXDO.Data.MVCP;

namespace NXDO.Data.Factory
{
    /// <summary>
    /// 视图帮助器，该帮助器用于实现模型数据的交互
    /// </summary>
    /// <typeparam name="M">模型类型</typeparam>
    public class ViewHelper<M> where M : class, new()
    {
        private M model;
        internal ViewHelper(object oModel,Controller controller)
        {
            this.ViewBag = new System.Dynamic.ExpandoObject();
            this.model = (M)oModel;
            this.Controller = controller;
        }

        public M GetModel()
        {
            var m = new M();
            return m;
        }

        internal IEnumerable<M> Models
        {
            get;
            set;
        }

        /// <summary>
        /// 当前关联的控制器
        /// </summary>
        internal Controller Controller
        {
            get;
            private set;
        }
        
        public dynamic ViewBag
        {
            get;
            internal set;
        }

        private MemberInfo GetMemberExpression(Expression body)
        {
            var expr = body as MemberExpression;
            if (expr == null)
                throw new ArgumentException("必须是成员访问表达式。");

            return expr.Member;
        }

        private object GetMemberValue(MemberInfo member)
        {
            if (this.model == null) return null;
            return 
                (member.MemberType == MemberTypes.Property) ?
                    (member as PropertyInfo).GetValue(this.model, null) :
                    (member as FieldInfo).GetValue(this.model);
        }


        #region for属性
        /// <summary>
        /// 模型属性上的标题设置到控件上
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型属性类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="controlPropertyExpr">访问控件属性表达式</param>
        /// <param name="modelPropertyExpr">访问模型属性表达式（并获取模型属性上DisplayAttribute的Name值）</param>
        public void ForTitle<TControl, MValue>(TControl control, Expression<Func<TControl, string>> controlPropertyExpr, Expression<Func<M, MValue>> modelPropertyExpr)
        {
            var modelMember = this.GetMemberExpression(modelPropertyExpr.Body);
            string title = DisplayAttribute.GetDisplayName(modelMember);
            var ocxMember = this.GetMemberExpression(controlPropertyExpr.Body);
            (ocxMember as PropertyInfo).SetValue(control, title, null);
        }

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
            if (this.model != null)
                val = modelPropertyExpr.Compile().Invoke(this.model);
            //var prop = control.GetType().GetTypeHelper().GetProperty("Text");
            //prop.SetValue(control, val);
            dynamic dyControl = control;
            dyControl.Text = val==null ? string.Empty : val.ToString();
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
            if (this.model != null)
                val = modelPropertyExpr.Compile().Invoke(this.model);
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
            if (this.model != null)
                val = modelPropertyExpr.Compile().Invoke(this.model);

            var prop = control.GetType().GetTypeHelper().GetProperty("Value");
            if (val != null)
            {                
                PropertyInfo pinfo = prop;
                Type pType = pinfo.PropertyType;
                val = Convert.ChangeType(val, pType);
            }
            prop.SetValue(control, val);
        }

        /// <summary>
        /// 设置控件的DataSource属性与模型数据源绑定的操作
        /// </summary>
        /// <typeparam name="TControl">控件类型</typeparam>
        /// <typeparam name="MValue">模型数据源匿名类型</typeparam>
        /// <param name="control">控件实例</param>
        /// <param name="ValueDisplayMemberExpr">模型数据源匿名类型表达式
        /// <para>m =&gt; new { m.ID, m.Name }; //ID为ValueMember, Name为DisplayMember</para>
        /// <para>例如：ListBox控件具有ValueMember,DisplayMember属性。</para></param>
        public void ForDataSource<TControl, MValue>(TControl control, Expression<Func<M, MValue>> ValueDisplayMemberExpr)
        {

        }

        public void ForDataSource<TControl>(TControl control, Action<TControl> setting)
        {

        }

        public void ForKnown<TControl, MValue>(TControl control, Expression<Func<TControl, object>> expr, Expression<Func<M, MValue>> expr2)
        {
        }
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

        class EventWrapper<TEventArgs>
        {
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

        #region action动作
        #region public ActionLinkClick
        /// <summary>
        /// 处理控件的Click事件以执行控制器指定操作。
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="actionName">操作名称</param>
        /// <param name="toAction">操作执行类型</param>
        /// <param name="actionDialog">如操作类型为Dioalog时，可处理子操作的返回值。</param>
        public void ActionLinkClick(object control, string actionName, ToActionFlag toAction = ToActionFlag.ViewActived, Action<ActionMode> actionDialogMode = null)
        {
            if (toAction == ToActionFlag.View || toAction == ToActionFlag.ViewActived)
                this.ForEvent(control, "Click", (s, e) => this.Action(actionName, toAction == ToActionFlag.View));
            else if (toAction == ToActionFlag.Child || toAction == ToActionFlag.ChildActived)
                this.ForEvent(control, "Click", (s, e) => this.ActionChild(actionName, toAction == ToActionFlag.Child));
            else if (toAction == ToActionFlag.Dialog)
            {
                this.ForEvent(control, "Click",
                    (s, e) =>
                    {
                        var rMode = this.ActionDialog(actionName);
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
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
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
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
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
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
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
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
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
            this.Action(actionName, false, isAlwaysCreate);
        }

        /// <summary>
        /// 执行控制器的操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        /// <param name="isAlwaysCreate">true:始终创建视图，反之为激活视图。</param>
        /// <param name="anonymousValues">方法参数，必须提供匿名类型。</param>
        public void Action(string actionName, bool isAlwaysCreate, object anonymousValues)
        {
            object oValues = anonymousValues ?? new { };
            bool bIsAnonymous = oValues.GetType().Name.StartsWith("<>f__AnonymousType", StringComparison.CurrentCultureIgnoreCase);
            if (!bIsAnonymous)
                throw new ArgumentException("必须保证传入参数为匿名类实例。", "anonymousValues");

            this.ExecuteAction(actionName, new object[] { oValues });
        }

        private void ExecuteAction(string actionName, params object[] values)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("actionName");

            dynamic viewBag = NXDO.Data.Factory.ActionHelper.ExecuteAction(actionName, this.Controller, this.ViewBag, values);
            this.ViewBag = viewBag;
        }
        #endregion

        #endregion

        public void Partial<TControl>(TControl control)
        {

        }
    }
}
