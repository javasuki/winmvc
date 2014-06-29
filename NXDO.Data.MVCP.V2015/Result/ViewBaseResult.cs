using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using NXDO.Data.Reflection;
using NXDO.Data.Extension;
using NXDO.Data.Factory;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 基类视图结果
    /// </summary>
    public abstract class ViewBaseResult : ActionResult
    {
        #region 获取视图类型与实例
        /// <summary>
        /// 获取视图实例
        /// </summary>
        /// <param name="viewFullName">视图类型名称</param>
        /// <returns></returns>
        internal object GetViewInstance(string viewName)
        {
            return Activator.CreateInstance(this.GetViewType(viewName));
        }

        /// <summary>
        /// 获取视图实例
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <returns></returns>
        internal object GetViewInstance(Type viewType)
        {
            return Activator.CreateInstance(viewType);
        }

        /// <summary>
        /// 获取视图类型
        /// </summary>
        /// <param name="viewName">视图类型名称</param>
        /// <returns>视图类型</returns>
        internal Type GetViewType(string viewName)
        {
            return MvcApplication.GetType(viewName, GetTypeMode.View, true);
        }
        #endregion

        protected abstract bool IsPartial
        {
            get;
        }

        internal virtual void RenderView(bool isIndexAction, Type viewType, object model, IEnumerable<object> models, DataTable table, Controller controller)
        {

            #region 设置视图
            Action<object, bool> actSetting = (vInstance, isAddCache) =>
            {
                #region IViewForm<TModel>
                dynamic dyFrmInstance = vInstance;

                var propHelper = vInstance.GetType().GetTypeHelper().GetProperty("Mvc");
                if (propHelper == null)
                    throw new TypeAccessException("请检查视图“" + viewType.Name + "”是否实现了接口 NXDO.Data.Factory.IViewForm<TModel>。");

                if (dyFrmInstance.Mvc == null)
                {
                    Type mvcHelperType = ((PropertyInfo)propHelper).PropertyType;
                    var mvcHelperInstance = mvcHelperType.GetTypeHelper().GetConstructor(
                                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                                        typeof(Controller))
                                            .Create(controller);
                    propHelper.SetValue(vInstance, mvcHelperInstance);

                    //dyFrmInstance.Mvc = mvcHelperInstance;
                    //dyFrmInstance.Mvc.Model = model;
                    //dyFrmInstance.Mvc.Models = models;                    
                    //mvcHelperType.GetTypeHelper().GetProperty("LinkViewer").SetValue(mvcHelperInstance, vInstance); //仅需设置一次，当前所持有的窗体
                    //mvcHelperType.GetTypeHelper().GetProperty("Model").SetValue(mvcHelperInstance, model);
                    //mvcHelperType.GetTypeHelper().GetProperty("Models").SetValue(mvcHelperInstance, models);
                    //mvcHelperType.GetTypeHelper().GetProperty("Table").SetValue(mvcHelperInstance, table);

                    controller.CurrentMvcHelper = mvcHelperInstance;
                    var mvcHelper = mvcHelperInstance as IMvcHelper;
                    mvcHelper.SetLinkViewer(vInstance);
                    mvcHelper.SetModelByController(model);
                    mvcHelper.SetModelByController(models);
                    mvcHelper.SetTableByController(table);
                    

                    //控制器添加缓存的视图实例
                    if (isAddCache)
                        Controller.ViewInstances.Add(viewType, vInstance);
                }
                else
                {
                    Type mvcHelperType = dyFrmInstance.Mvc.GetType();
                    controller.CurrentMvcHelper = dyFrmInstance.Mvc;
                    //mvcHelperType.GetTypeHelper().GetProperty("IsRegistered").SetValue(dyFrmInstance.Mvc, true);
                    //mvcHelperType.GetTypeHelper().GetProperty("Model").SetValue(dyFrmInstance.Mvc, model);
                    //mvcHelperType.GetTypeHelper().GetProperty("Models").SetValue(dyFrmInstance.Mvc, models);
                    //mvcHelperType.GetTypeHelper().GetProperty("Table").SetValue(dyFrmInstance.Mvc, table);

                    var mvcHelper = dyFrmInstance.Mvc as IMvcHelper;
                    mvcHelper.SetLinkViewer(vInstance);
                    mvcHelper.SetModelByController(model);
                    mvcHelper.SetModelByController(models);
                    mvcHelper.SetTableByController(table);
                }

                dyFrmInstance.Mvc.ViewBag = controller.ViewBag;
                vInstance.GetType().GetTypeHelper().GetMethod("Register").Invoke(vInstance, dyFrmInstance.Mvc);

                #endregion
            };
            #endregion

            controller.ModeBag.ModeDialogResult = -1;
            if (controller.ModeBag.CreateMode == ViewCreateFlag.FormApplication)
                this.ViewFirst(viewType, model, controller, actSetting);
            else if (controller.ModeBag.CreateMode == ViewCreateFlag.FormDialog)
                this.ViewDialog(viewType, model, controller, actSetting);
            else if (controller.ModeBag.CreateMode == ViewCreateFlag.Form || controller.ModeBag.CreateMode == ViewCreateFlag.FormMdi)
            {
                bool isMdiChildren = controller.ModeBag.CreateMode == ViewCreateFlag.FormMdi;

                if (controller.ModeBag.IsAlwaysCreate)
                    this.ViewCreate(viewType, model, controller, isMdiChildren, actSetting);
                else
                    this.ViewActive(isIndexAction, viewType, model, controller, isMdiChildren, actSetting);
                return;
            }

            #region 注释
            //int iTest = 100;
            //if (iTest > 0) return;

            //object cacheViewUI = Controller.ViewInstances.Get(viewType);
            //if (cacheViewUI != null)
            //{
            //    if (cacheViewUI is Form)
            //        (cacheViewUI as Form).Activate();
            //    return;
            //}

            //object viewInstance = null;
            //if (controller.ModeBag.MdiChildCreateOrActive == 2)
            //{
            //    viewInstance = Controller.ViewInstances.Get(viewType);
            //    if (viewInstance != null)
            //    {
            //        if (viewInstance is Form)
            //            (viewInstance as Form).Activate();
            //        return;
            //    }
            //}

            //viewInstance = this.GetViewInstance(viewType);

            //#region IViewForm<TModel>
            //////viewInstance 实现接口 IViewForm<TModel>
            //////viewInstance.View 的类型并实例化后，赋值给 View 属性
            //////viewInstance.View = new ViewHelper(model，controller);
            ////var propHelper = viewInstance.GetType().GetTypeHelper().GetProperty("View");
            ////if (propHelper == null)
            ////    throw new TypeAccessException("请检查视图“" + viewType.Name + "”是否实现了接口 NXDO.Data.Factory.IViewForm<TModel>。");

            ////var viewHelperInstance = ((PropertyInfo)propHelper).PropertyType.GetTypeHelper().GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, typeof(object), typeof(Controller)).Create(model, controller);
            ////propHelper.SetValue(viewInstance, viewHelperInstance);

            //////viewUI 实例方法 Register 的调用
            //////viewUI.Register(viewHelperInstance);
            ////viewInstance.GetType().GetTypeHelper().GetMethod("Register").Invoke(viewInstance, viewHelperInstance);

            //////控制器添加缓存的视图实例
            ////Controller.ViewInstances.Add(viewType, viewInstance);
            //#endregion

            //if (!(viewInstance is Form)) return;

            //var frm = viewInstance as Form;
            //frm.FormClosed += (sender, args) =>
            //{
            //    Controller.ViewInstances.Remove(viewType);
            //};

            //if (System.Windows.Forms.Application.OpenForms.Count == 0)
            //{
            //    System.Windows.Forms.Application.Run(frm);
            //    return;
            //}

            //if (controller.ModeBag.IsMode)
            //{
            //    controller.ModeBag.ModeDialogResult = Convert.ToInt32(frm.ShowDialog());
            //    dynamic dyFrm = frm;
            //    controller.ViewBag = dyFrm.View.ViewBag;
            //    return;
            //}

            //var mdiFrm = System.Windows.Forms.Application.OpenForms[0];
            //if (mdiFrm.IsMdiContainer && controller.ModeBag.MdiChildCreateOrActive > 0)
            //    frm.MdiParent = mdiFrm;
            //frm.Show();
            #endregion
        }

        #region ViewXXX方法
        /// <summary>
        /// 建立模态显示视图
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="model">模型</param>
        /// <param name="controller">控制器</param>
        /// <param name="actSetting">视图实现接口IViewForm&lt;TModel&gt;的属性与方法的设置</param>
        private void ViewDialog(Type viewType, object model, Controller controller, Action<object, bool> actSetting)
        {
            object viewInstance = this.GetViewInstance(viewType);
            actSetting.Invoke(viewInstance, false);//false:不需要添加到某一类型的缓存集合中

            Form frm = viewInstance as Form;
            controller.ModeBag.ModeDialogResult = Convert.ToInt32(frm.ShowDialog());

            dynamic dyFrm = frm;
            controller.ViewBag = dyFrm.Mvc.ViewBag;
        }

        /// <summary>
        /// 建立第一个视图,作为应用程序的入口视图
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="model">模型</param>
        /// <param name="controller">控制器</param>
        /// <param name="actSetting">视图实现接口IViewForm&lt;TModel&gt;的属性与方法的设置</param>
        private void ViewFirst(Type viewType, object model, Controller controller, Action<object, bool> actSetting)
        {
            object viewInstance = this.GetViewInstance(viewType);
            actSetting.Invoke(viewInstance, true);
            System.Windows.Forms.Application.Run(viewInstance as Form);
        }

        /// <summary>
        /// 始终创建视图
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="model">模型</param>
        /// <param name="controller">控制器</param>
        /// <param name="isMdiChild">是否为子视图</param>
        /// <param name="actSetting">子视图实现接口IViewForm&lt;TModel&gt;的属性与方法的设置</param>
        private void ViewCreate(Type viewType, object model, Controller controller,bool isMdiChild, Action<object, bool> actSetting)
        {
            object viewInstance = this.GetViewInstance(viewType);
            actSetting.Invoke(viewInstance, true);
            var frm = viewInstance as Form;
            if (isMdiChild)
            {
                if (frm.IsMdiContainer) frm.IsMdiContainer = false;
                //需要建立子视图,检查当前系统中的 MDI父容器窗体,如果存在,则设置建立子视图的MDI父窗体
                var mdiParent = from f in Application.OpenForms.OfType<Form>() where f.IsMdiContainer select f;
                if (mdiParent != null)
                    frm.MdiParent = mdiParent.First();
            }
            frm.FormClosed += (s, e) =>
            {
                Controller.ViewInstances.RemoveContent(viewType, s as Form);
            };
            frm.Show();
        }

        /// <summary>
        /// 视图激活,如果视图不存在则创建
        /// </summary>
        /// <param name="viewType">视图类型</param>
        /// <param name="model">模型</param>
        /// <param name="controller">控制器</param>
        /// <param name="isMdiChild">是否为子视图</param>
        /// <param name="actSetting">视图实现接口IViewForm&lt;TModel&gt;的属性与方法的设置</param>
        private void ViewActive(bool isIndexAction, Type viewType, object model, Controller controller, bool isMdiChild, Action<object, bool> actSetting)
        {
            List<object> views = Controller.ViewInstances.Get(viewType);

            if (views.Count == 0)
            {
                this.ViewCreate(viewType, model, controller, isMdiChild, actSetting);
                return;
            }
            else
            {
                //不是Index操作，则需要更新传入的数据到视图中。
                if (!isIndexAction)
                    actSetting.Invoke(views.Last(), false);
            }

            (views.Last() as Form).Activate();
        }
        #endregion
    }

}
