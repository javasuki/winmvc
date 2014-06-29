using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NXDO.Data.MVCP;
using EMP.MvcDemo.Models;

namespace EMP.MvcDemo.Views.vvc
{
    public partial class Form1 : Form, IViewForm<object>
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public NXDO.Data.Factory.MvcHelper<object> Mvc
        {
            get;
            set;
        }

        public void Register(NXDO.Data.Factory.MvcHelper<object> mvc)
        {
        }
    }
}
