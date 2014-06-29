using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NXDO.Data.MVCP;
using NXDO.Data.Attribute;

using EMP.MvcDemo.Models;

namespace EMP.MvcDemo.Controllers
{
    class EmpController : Controller
    {
        public override ViewResult Index(params object[] values)
        {
            List<EmpModel> lst = new List<EmpModel>()
            {
                new EmpModel{ ID=1, Name="x1", Birthday=DateTime.Now.AddDays(-2), Sex=true},
                new EmpModel{ ID=2, Name="x2", Birthday=DateTime.Now.AddDays(-3), Sex=true},
                new EmpModel{ ID=3, Name="x3", Birthday=DateTime.Now.AddDays(-4), Sex=false},
                new EmpModel{ ID=4, Name="x4", Birthday=DateTime.Now.AddDays(-5), Sex=false},
                new EmpModel{ ID=5, Name="x5", Birthday=DateTime.Now.AddDays(-6), Sex=false}
            };

            return this.View(lst);
        }

        public ViewResult Refresh()
        {
            List<EmpModel> lst = new List<EmpModel>()
            {
                new EmpModel{ ID=1, Name="y1", Birthday=DateTime.Now, Sex=true},
                new EmpModel{ ID=2, Name="y2", Birthday=DateTime.Now, Sex=true},
                new EmpModel{ ID=3, Name="y3", Birthday=DateTime.Now, Sex=false},
                new EmpModel{ ID=4, Name="y4", Birthday=DateTime.Now, Sex=false},
                new EmpModel{ ID=5, Name="y5", Birthday=DateTime.Now, Sex=false}
            };

            return this.View(lst);
        }

        public ViewResult qEdit(int ID)
        {
            var model = new EmpModel { ID = 1, Name = "x1", Birthday = DateTime.Now, Sex = true };
            return this.View(model);
        }

        public ActionResult New()
        {
            return this.PartialView("Edit");
        }


        public ActionResult Edit(EmpModel model)
        {
            this.ViewBag.ListData = SexModel.Data;
            return this.PartialView(model);
        }

        //[HttpPost]
        public ActionResult Save(/*EmpModel model*/)
        {
            EmpModel model = new EmpModel();
            this.UpdateModel(model);

            return this.PartialView("Edit");
        }

        public ActionResult Dlg()
        {
            var rs = RedirectToAction("Index", "Dlg");
            return rs;
        }
    }
}
