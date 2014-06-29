using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using NXDO.Data.Factory;
using NXDO.Data.Reflection;
using NXDO.Data.Extension;

namespace NXDO.Data.MVCP
{
    public class PartialViewResult : ViewBaseResult
    {
        protected override bool IsPartial
        {
            get { return true; }
        }

        Type GetViewType(string[] viewNames)
        {
            Type t = null;
            foreach (string vn in viewNames)
            {
                t = MvcApplication.GetType(vn, GetTypeMode.PartialView, false);
                if (t != null)
                    break;
            }
            return t;
        }

        internal PartialViewResult(string[] viewPartialNames, Controller controller)
            : this(viewPartialNames, controller, null, null)
        {
        }

        internal PartialViewResult(string[] viewPartialNames, Controller controller, object model)
            : this(viewPartialNames, controller, model, null)
        {
        }

        internal PartialViewResult(string[] viewPartialNames, Controller controller, object model, IEnumerable<object> models)
            : this(viewPartialNames, controller, model, null, null)
        {
        }

        internal PartialViewResult(string[] viewPartialNames, Controller controller, object model, IEnumerable<object> models, System.Data.DataTable table)
        {
            var type = this.GetViewType(viewPartialNames);
            if (type == null)
                throw new TypeLoadException(string.Join(";", viewPartialNames) + "类型不存在！");

            dynamic dyHelper = controller.CurrentMvcHelper;
            var ocx = dyHelper.GetPartial(type);
            if (ocx == null)
                throw new ArgumentException("当前控制器不存在分部视图实例。如果确定存在分部视图，则在视图的 Register 方法中显示调用 this.Mvc.Partial(分部视图实例)。", "contoller");

            this.SetPartialViewHelper(ocx, type, controller, model, models, table);
        }

        #region for MvcHelper<TModel>.RenderPartial
        PartialViewResult()
        {
        }

        /// <summary>
        /// 获取用于在Form视图的 MvcHelper 中调用RenderPartial方法，实现初始化分部视图。
        /// </summary>
        /// <returns></returns>
        internal static PartialViewResult GetInstanceForRender()
        {
            return new PartialViewResult();
        }
        #endregion

        internal void SetPartialViewHelper(dynamic dyUserControl, Type viewType, Controller controller, object model, IEnumerable<object> models,System.Data.DataTable table)
        {
            if (dyUserControl.Mvc == null)
            {
                UserControl userControl = dyUserControl as UserControl;
                var propHelper = userControl.GetType().GetTypeHelper().GetProperty("Mvc");
                if (propHelper == null)
                    throw new TypeAccessException("请检查分部视图“" + viewType.Name + "”是否实现了接口 NXDO.Data.Factory.IViewControl<TModel>。");

                Type mvcHelperType = ((PropertyInfo)propHelper).PropertyType;
                var mvcHelperInstance = mvcHelperType.GetTypeHelper().
                                                    GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, 
                                                    typeof(Controller)).
                                                    Create(controller);

                //dyUserControl.Mvc = mvcHelperInstance;
                //dyUserControl.Mvc.Model = model;
                //dyUserControl.Mvc.Models = models;

                controller.CurrentPartialMvcHelper = mvcHelperInstance;                
                propHelper.SetValue(dyUserControl, mvcHelperInstance);
                //mvcHelperType.GetTypeHelper().GetProperty("LinkViewer").SetValue(dyUserControl.Mvc, dyUserControl); //仅需设置一次，当前所持有的控件
                //mvcHelperType.GetTypeHelper().GetProperty("Model").SetValue(mvcHelperInstance, model);
                //mvcHelperType.GetTypeHelper().GetProperty("Models").SetValue(mvcHelperInstance, models);
                var mvcHelper = mvcHelperInstance as IMvcHelper;
                mvcHelper.SetModelByController(model);
                mvcHelper.SetModelByController(models);
                mvcHelper.SetTableByController(table);
            }
            else
            {
                Type mvcHelperType = dyUserControl.Mvc.GetType();
                controller.CurrentPartialMvcHelper = dyUserControl.Mvc;
                //mvcHelperType.GetTypeHelper().GetProperty("IsRegistered").SetValue(dyUserControl.Mvc, true);
                //mvcHelperType.GetTypeHelper().GetProperty("Model").SetValue(dyUserControl.Mvc, model);
                //mvcHelperType.GetTypeHelper().GetProperty("Models").SetValue(dyUserControl.Mvc, models);
                var mvcHelper = dyUserControl.Mvc as IMvcHelper;
                mvcHelper.SetModelByController(model);
                mvcHelper.SetModelByController(models);
                mvcHelper.SetTableByController(table);
            }

            dyUserControl.Mvc.ViewBag = controller.ViewBag;
            dyUserControl.Register(dyUserControl.Mvc);
        }
    }
}
