using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using NXDO.Data.Reflection;
using NXDO.Data.Extension;
using NXDO.Data.Factory;

namespace NXDO.Data.MVCP
{
    /// <summary>
    /// 视图结果
    /// </summary>
    public class ViewResult : ViewBaseResult
    {
        protected override bool IsPartial
        {
            get { return false; }
        }

        internal ViewResult(bool isIndexAction, string viewName, Controller contoller)
        {
            this.RenderView(isIndexAction, this.GetViewType(viewName), null, null, null, contoller);
        }

        internal ViewResult(bool isIndexAction, string viewName, Controller contoller, DataTable table)
        {
            this.RenderView(isIndexAction, this.GetViewType(viewName), null, null, table, contoller);
        }

        internal ViewResult(bool isIndexAction, string viewName, Controller contoller, object model)
        {
            this.RenderView(isIndexAction, this.GetViewType(viewName), model, null, null, contoller);
        }

        internal ViewResult(bool isIndexAction, string viewName, Controller contoller, object model, IEnumerable<object> models)
        {
            this.RenderView(isIndexAction, this.GetViewType(viewName), model, models, null, contoller);
        }

        //public object Model
        //{
        //    get;
        //    private set;
        //}
    }

    //public class ViewDialogResult : 
}
