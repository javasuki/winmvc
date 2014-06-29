using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NXDO.Data.MVCP;
using NXDO.Data.Attribute;

namespace EMP.MvcDemo.Controllers
{
    
    class DlgController : Controller
    {
        public override ViewResult Index(params object[] values)
        {
            var rs = base.Index(values);
            if(this.ViewBag== null);
            return rs;
        }


    }
}
