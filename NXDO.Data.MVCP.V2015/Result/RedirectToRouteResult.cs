using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 重定向结果
    /// </summary>
    public class RedirectToRouteResult : ViewBaseResult
    {
        protected override bool IsPartial
        {
            get { return false; }
        }

        /// <summary>
        /// 重定向后,如果是模态视图,则模态视图的返回结果
        /// <para>非-1时,可以转换成 ActionMode </para>
        /// </summary>
        internal int RedirectModeExecuteResult
        {
            get;
            private set;
        }

        internal dynamic ViewBag { get; set; }

        internal RedirectToRouteResult(string actionName, Type controllerType, ViewFormBag formBag, object viewBag, params object[] values)
        {
            Controller controller = Activator.CreateInstance(controllerType) as Controller;
            //controller.ModeBag.IsMode = formBag.IsMode;
            //controller.ModeBag.MdiChildCreateOrActive = formBag.MdiChildCreateOrActive;
            controller.ModeBag.IsAlwaysCreate = formBag.IsAlwaysCreate;
            controller.ModeBag.CreateMode = formBag.CreateMode;

            if (string.Compare(actionName, "Index", true) == 0)
            {
                controller.Index(values);
                this.ViewBag = controller.ViewBag;
            }
            else
                this.ViewBag = NXDO.Data.Factory.ActionHelper.ExecuteAction(actionName, controller, viewBag, values);

            this.RedirectModeExecuteResult = controller.ModeBag.ModeDialogResult;
        }
    }
}
