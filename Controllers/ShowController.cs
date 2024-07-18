using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data;
using ePortal.Data.Infrastructure;
using ePortal.Data.Repositories;
using ePortal.Models;
using ePortal.Web.ViewModels;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace ePortal.Web.Controllers
{
    public class ShowController : Controller
    {
        private PortalDataContext db = new PortalDataContext();

        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<Employee_Full> employeeRepository;
        private readonly IEntityRepository<Absence> absenceRepository;
        private readonly IEntityRepository<Overtime> overtimeRepository;
        private readonly IEntityRepository<Scheduling> schedulingRepository;
        private readonly IEntityRepository<OthersFormMode> OthersFormModelRepository;
        private readonly IEntityRepository<AccessFormMode> AccessFormModelRepository;
        private readonly IEntityRepository<AccessRecord> AccessRecordRepository;
        private readonly IEntityRepository<Bide> BideRepository;
        private readonly IEntityRepository<MailingList> mailingListRepository;
        private readonly IUnitOfWork UnitOfWork;
        private readonly IEntityRepository<AbnormalAnalysis> analysisRepository;
        private readonly IEntityRepository<HCP> HCPRepository;

        public ShowController(ICommandBus commandBus, IEntityRepository<Employee_Full> employeeRepository, IEntityRepository<Scheduling> schedulingRepository, IEntityRepository<OthersFormMode> OthersFormModelRepository, IEntityRepository<AccessFormMode> AccessFormModelRepository, IEntityRepository<HCP> HCPRepository, IEntityRepository<Absence> absenceRepository, IEntityRepository<Overtime> overtimeRepository, IEntityRepository<AbnormalAnalysis> analysisRepository, IEntityRepository<AccessRecord> AccessRecordRepository, IEntityRepository<Bide> BideRepository, IEntityRepository<MailingList> mailingListRepository, IUnitOfWork UnitOfWork)
        {
            this.UnitOfWork = UnitOfWork;
            this.BideRepository = BideRepository;
            this.commandBus = commandBus;
            this.absenceRepository = absenceRepository;
            this.AccessRecordRepository = AccessRecordRepository;
            this.mailingListRepository = mailingListRepository;
            this.overtimeRepository = overtimeRepository;
            this.employeeRepository = employeeRepository;
            this.analysisRepository = analysisRepository;
            this.schedulingRepository = schedulingRepository;
            this.OthersFormModelRepository = OthersFormModelRepository;
            this.AccessFormModelRepository = AccessFormModelRepository;
            this.HCPRepository = HCPRepository;
        }


        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Index2()
        {

            return View();
        }
        public ActionResult Index3()
        {
           

            if (Request.QueryString["StartDate"] != null && Request.QueryString["EndDate"] != null)
            {
                DateTime StartDate = Convert.ToDateTime(Request.QueryString["StartDate"]);

                DateTime EndDate = Convert.ToDateTime(Request.QueryString["EndDate"]);

                ViewBag.StartDate = StartDate.ToShortDateString();
                ViewBag.EndDate = EndDate.ToShortDateString();

            }





            return View();
        }


        public string ShowCount(string types, string star, string end, string emp_no)
        {

            var dt = analysisRepository.ProcQuery("Audit_ShowCount", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=types,ParameterName="types"},
                      new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=star,ParameterName="stardate"},
                        new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=end,ParameterName="enddate"},
                             new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=emp_no,ParameterName="emp_no"}
                });


            string s = dt.Rows[0][0].ToString();
            //return Json(s);

            //return Json(s, JsonRequestBehavior.AllowGet);

            //return Content(Convert.ToString(s));

            return s;
        }


        public ActionResult Show(string types, string star, string end, string emp_no, Int32 page, Int32 pagecount)
        {


         



            DataTable dt = analysisRepository.ProcQuery("Audit_ShowData", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=types,ParameterName="types"},
                      new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=star,ParameterName="stardate"},
                        new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=end,ParameterName="enddate"},
                            new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=emp_no,ParameterName="emp_no"},
                                 new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=page,ParameterName="page"},
                                     new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=pagecount,ParameterName="pagecount"},
                });
            string s = DTtoJSON(dt);
            //return Json(s);

            //return Json(s, JsonRequestBehavior.AllowGet);

            //return Content(Convert.ToString(s));

            return Content(JsonConvert.SerializeObject(s).Replace("&nbsp;", ""));

        }


        public ActionResult ShowTop(string StartDate,  string EndDate)
        {

            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                ViewBag.StartDate = StartDate;
                ViewBag.EndDate = EndDate;
            }
            else
            {
                ViewBag.StartDate = null;
                ViewBag.EndDate = null;

            }





            DataTable dt = analysisRepository.ProcQuery("Audit_TOP50", new SqlParameter[]
                {
          
                      new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=StartDate,ParameterName="StartDate"},
                        new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=EndDate,ParameterName="EndDate"}
                });
            string s = DTtoJSON(dt);
            //return Json(s);

            //return Json(s, JsonRequestBehavior.AllowGet);

            //return Content(Convert.ToString(s));

            return Content(JsonConvert.SerializeObject(s).Replace("&nbsp;", ""));

        }



        public void excel(string StartDate, string EndDate)
        {



            var DataTable = analysisRepository.ProcQuery("Audit_Top50",
             new SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=StartDate,ParameterName="StartDate"} ,
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value= EndDate,ParameterName="EndDate"}


              });

            Microsoft.Reporting.WebForms.ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
            //設置報表路徑
            rv.LocalReport.ReportPath = @"Report\ReportG4.rdlc";

            rv.LocalReport.DataSources.Clear();

            //設置報表數據源
            ReportDataSource reportDataSource = new ReportDataSource("DataSet2", DataTable);
            rv.LocalReport.DataSources.Add(reportDataSource);
            rv.LocalReport.Refresh();
            //生是ＥＸＥＣＬ文件字節碼
            var result = rv.LocalReport.Render("EXCEL");


            //將excel寫入文件
            var tempfilename = Server.MapPath("~/temp/Attendance");
            var xlsfileName = tempfilename + ".xls";
            System.IO.FileStream stream = new System.IO.FileStream(tempfilename + ".xls", System.IO.FileMode.OpenOrCreate);
            stream.Write(result, 0, result.Length);
            stream.Close();

            // System.Diagnostics.Process.Start(Server.MapPath("~/temp/Attendance.xls")); 

            //string fileName = "Attendance.xls";//客户端保存的文件名
            //string filePath = Server.MapPath("~/temp/Attendance.xls");//路径
            //FileInfo fileInfo = new FileInfo(filePath);
            //Response.Clear();
            //Response.ClearContent();
            //Response.ClearHeaders();
            //Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            //Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            //Response.AddHeader("Content-Transfer-Encoding", "binary");
            //Response.ContentType = "application/ms-excel";
            //Response.ContentEncoding = System.Text.Encoding.GetEncoding("BIG5");
            //Response.WriteFile(fileInfo.FullName);
            //Response.Flush();
            //Response.End();

            string fileName = "Attendance.xls";//客户端保存的文件名
            string filePath = Server.MapPath("~/temp/Attendance.xls");//路径

            //以字符流的形式下载文件
            FileStream fs = new FileStream(filePath, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            Response.ContentType = "application/ms-excel";
            //通知浏览器下载文件而不是打开
            Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();

        }




        public static string DTtoJSON(DataTable dt)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ArrayList dic = new ArrayList();
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> drow = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    drow.Add(col.ColumnName, row[col.ColumnName]);
                }
                dic.Add(drow);
            }
            return jss.Serialize(dic);
        }


    }
}
