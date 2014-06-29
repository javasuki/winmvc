using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NXDO.Data.MVCP;

namespace EMP.MvcDemo.Controllers
{
    class HomeController : Controller
    {
        public ActionResult LoadEmps()
        {
            return this.RedirectToAction("Index", "EmpController");
        }

        public ActionResult LoadSubView()
        {
            return this.RedirectToAction(typeof(Form1Controller));
        }
    }
}
