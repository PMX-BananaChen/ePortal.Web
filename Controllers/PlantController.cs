using System;
using System.Web.Mvc;
using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data.Repositories;
using ePortal.Web.ViewModels;
using ePortal.Web.Helpers;
using ePortal.Domain.Commands;
using ePortal.Web.Core.ActionFilters;
using ePortal.Web.Core.Models;
using System.Web.Mvc.Result;
using AutoMapper;
using System.Data;
using System.Linq;
using ePortal.Models;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;
using ePortal.Core.Common;
using ePortal.Web.Core.Extensions;
using ePortal.Data.Infrastructure;
using System.Web;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using ePortal.Web.Core.Email;
using System.Configuration;
using Microsoft.Reporting.WebForms;
namespace ePortal.Web.Controllers
{
    /*
     * summary:AttendanceController車間門禁模塊Controller
     * 
     */

    [CompressResponse]
    public class PlantController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<Employee_Full> employeeRepository;
        private readonly IEntityRepository<Absence> absenceRepository;
        private readonly IEntityRepository<Overtime> overtimeRepository;
        private readonly IEntityRepository<Scheduling> schedulingRepository;
        private readonly IEntityRepository<AccessRecord> AccessRecordRepository;
        private readonly IEntityRepository<PlantFormMode> PlantFormModelRepository;
        private readonly IEntityRepository<Bide> BideRepository;
        private readonly IEntityRepository<MailingList> mailingListRepository;
        private readonly IUnitOfWork UnitOfWork;
        private readonly IEntityRepository<AbnormalAnalysis> analysisRepository;
        public PlantController(ICommandBus commandBus, IEntityRepository<Employee_Full> employeeRepository, IEntityRepository<Scheduling> schedulingRepository, IEntityRepository<PlantFormMode> PlantFormModelRepository, IEntityRepository<Absence> absenceRepository, IEntityRepository<Overtime> overtimeRepository, IEntityRepository<AbnormalAnalysis> analysisRepository, IEntityRepository<AccessRecord> AccessRecordRepository, IEntityRepository<Bide> BideRepository, IEntityRepository<MailingList> mailingListRepository, IUnitOfWork UnitOfWork)
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
            this.PlantFormModelRepository = PlantFormModelRepository;
        }
        /**/
        public ActionResult AttendanceList(int? ResultId)
        {
            ResultId = ResultId ?? 1;
            string TotalResult = "0";
            var DeptMai = employeeRepository.SqlQuery<AuditDeptMailViewModels>("sp_AuditDeptMail", out TotalResult, new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=ResultId.Value,ParameterName="pageIndex"},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=25,ParameterName="pageSize"}
                }).ToList();
            IEnumerate entityResult = new Enumerate<AuditDeptMailViewModels>(DeptMai, ResultId.Value, 25, Convert.ToInt32(TotalResult));

            //sp_AuditDeptMail
            return View(entityResult);
        }
        [HandleErrors]
        public ActionResult Index(int? ResultId)
        {
            ResultId = ResultId ?? 1;
            var factory = employeeRepository.ProcQuery<Factorys>("GetFactory", new SqlParameter[] { }).Select(l => new SelectListItem { Text = l.Factory, Value = l.Factory });
            ViewData["Factory"] = factory;
            string TotalResult = "0";
            var DeptMai = employeeRepository.SqlQuery<AuditDeptMailViewModels>("sp_AuditDeptMail", out TotalResult, new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=ResultId.Value,ParameterName="pageIndex"},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=25,ParameterName="pageSize"}
                }).ToList();


            IEnumerate entityResult = new Enumerate<AuditDeptMailViewModels>(DeptMai, ResultId.Value, 25, Convert.ToInt32(TotalResult));

            //sp_AuditDeptMail
            return View(entityResult);
        }

        public ActionResult DeptMailSave(MailingLists form)
        {
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<MailingLists, CreateOrUpdateEntityCommand<MailingList>>(form);
                if (form.AutoID == Guid.Empty)
                {
                    IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                    ModelState.AddModelErrors(errors);
                    if (ModelState.IsValid)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        //0:Insert ,1:Update ,3:Delete
                        var result = commandBus.Submit(command, 0);
                        if (result.Success)
                        {

                        }
                    }
                }
                else
                {
                    var result = commandBus.Submit(command, 1);

                    if (result.Success)
                    {

                    }
                }
            }

            return Json(true);
        }
        [HandleErrors]
        public ActionResult DeptMailEdit(Guid? Id)
        {
            //var factory = employeeRepository.GetAll().Select(n => new { Text = n.Factory, Value = n.Factory }).Distinct().Select(n => new SelectListItem { Text = n.Text, Value = n.Value });

            var factory = employeeRepository.ProcQuery<Factorys>("GetFactory", new SqlParameter[] { }).Select(s => s.Factory).Distinct().Select(l => new SelectListItem { Text = l, Value = l }).ToList();

            MailingList DeptMail;
            if (Id.HasValue)
            {
                DeptMail = mailingListRepository.Get(m => m.AutoID == Id);
            }
            else
            {
                DeptMail = new MailingList();
            }
            if (!string.IsNullOrEmpty(DeptMail.DeptNo))
            {
                factory.Find(s => s.Text == DeptMail.Factory).Selected = true;
                var deptEntity = employeeRepository.GetMany(m => m.Dept_No == DeptMail.DeptNo).Select(n => new { Text = n.Dept_Name, Value = n.Dept_No }).Distinct().Select(n => new SelectListItem { Text = n.Text, Value = n.Value });
                ViewData["department"] = deptEntity;
            }
            else
            {
                ViewData["department"] = new List<SelectListItem>();
            }
            ViewData["Factory"] = factory;
            var Entity = Mapper.Map<MailingList, MailingLists>(DeptMail);

            return PartialView("Edit", Entity);

        }
        [Authorizer]
        [HandleErrors]
        [OutputCache(CacheProfile = "Attendance/Audit")]
        public ActionResult Audit(int? ResultId, DateTime? startDate, DateTime? endDate, string factory, string department, string empno, string emptype, string control)
        {  //If date is not passed, take current month's first and last dte 

            DateTime dtNow;
            int PageSize = 30;
            int PageIndex = ResultId ?? 1;
            factory = factory ?? "";
            department = department ?? "";
            empno = empno ?? "";

            //var DeptIds = employeeRepository.SqlQuery<DepartmentFormModel>("exec GetDeptId {0}", new object[] { HttpContext.User.Identity.Name }).OrderBy(e => e.Dept_Name).ToList();
            //if (DeptIds.Count() > 0)
            //{
            //    ViewBag.False = "disabled";
            //    if (string.IsNullOrEmpty(department))
            //    {
            //        department = DeptIds.FirstOrDefault().Dept_No;
            //    }
            //}
            //else
            //{
            //    ViewBag.False = "";
            //}

            emptype = emptype ?? "";
            control = control ?? "";

            dtNow = DateTime.Now.ToLocalTime();
            if (!startDate.HasValue)
            {
                startDate = dtNow.AddDays(Convert.ToInt16(-8));               
                endDate = dtNow.AddDays(Convert.ToInt16(-3));
            }

            if (startDate.HasValue && !endDate.HasValue)
            {
                endDate = dtNow.AddDays(Convert.ToInt16(0 - dtNow.DayOfWeek));
            }
            //var expenses = expenseRepository.GetMany(exp => exp.Date >= startDate && exp.Date <= endDate);
            //if request is Ajax will return partial view
            //set start date and end date to ViewBag dictionary
            ViewBag.StartDate = startDate.Value.ToShortDateString();
            ViewBag.EndDate = endDate.Value.ToShortDateString();


            if (empno == "")
            {
               
                return View("Plant/Index");

            }
            else
            {

                //var fa = employeeRepository.GetAll().Select(l => l.Factory).Distinct().Select(e => new SelectListItem { Text = e, Value = e }).OrderBy(e => e.Text).ToList();
                //ViewData["factory"] = fa;
                //if (string.IsNullOrEmpty(factory))
                //{
                //    factory = fa.FirstOrDefault().Value;
                //}
                string TotalResult = "0";
                var entityModel = PlantFormModelRepository.SqlQuery<ePortal.Web.ViewModels.PlantFormModel>("sp_Plantwith", out TotalResult, new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=PageIndex,ParameterName="pageIndex"},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.Int32,Value=PageSize,ParameterName="pageSize"},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=startDate.GetValueOrDefault().ToString("yyyy-MM-dd"),ParameterName="startDate"} ,
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value= endDate.GetValueOrDefault().ToString("yyyy-MM-dd"),ParameterName="endDate"} ,
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=factory,ParameterName="factory",Size=50},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=department,ParameterName="department",Size=100},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=empno,ParameterName="empno",Size=50},
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=emptype,ParameterName="emptype",Size=50} ,
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=control,ParameterName="control",Size=50} 
                }).ToList();

                IEnumerate entityResult = new Enumerate<PlantFormModel>(entityModel, PageIndex, PageSize, Convert.ToInt32(TotalResult));
                //take last date of start date's month, if end date is not passed 
               
                if (Request.IsAjaxRequest())
                {
                    return PartialView("Plant/Plant", entityResult);
                }
                else
                {
                    return View("Plant/Index", entityResult);
                }

            }

        }
        
        /// <summary>
        /// 全部分析
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="factory"></param>
        /// <param name="department"></param>
        /// <param name="emptype"></param>
        /// <param name="DL"></param>
        /// <param name="anomalous"></param>
        /// <returns></returns>
        [HandleErrors]
        public  ActionResult AllAnalyse(DateTime? startDate, DateTime? endDate, string factory, string department, string emptype, string DL, string anomalous)
        {

            DateTime dtNow;
            factory = factory ?? "";
            department = department ?? "";
            emptype = emptype ?? "";
            Expression<Func<Employee_Full, bool>> Where;
            if (!string.IsNullOrEmpty(DL))
            {
                if (emptype != "")
                {
                    Where = predicate<Employee_Full>(m => m.Dept_No == department && m.Emp_Type == emptype);
                }
                else
                {
                    Where = predicate<Employee_Full>(m => m.Dept_No == department && (m.Emp_Type == "JO01" || m.Emp_Type == "JO02"));
                }
            }
            else
            {
                Where = predicate<Employee_Full>(m => m.Dept_No == department);
            }

            var Analyses = new AnalyseForm();

            dtNow = DateTime.Today;
            if (!startDate.HasValue)
            {
                startDate = dtNow.AddDays(Convert.ToInt16(-6 - dtNow.DayOfWeek));
                endDate = dtNow.AddDays(Convert.ToInt16(0 - dtNow.DayOfWeek));
            }
            string username = HttpContext.User.Identity.Name;
            //清空之前的用戶分析記錄
            EmptyAbnormalAnalysis(username);
            var employees = employeeRepository.GetMany(Where).Distinct().ToList();
            Analyses.Employee = employees;
            foreach (var item in employees)
            {
                //排班信息　異常信息　加班信息
                Analyses.Analyse = absenceRepository.SqlQuery<Analyse>("exec sp_analytical {0},{1},{2},{3}", new object[] { startDate.Value.ToString("yyyy/MM/dd"), endDate.Value.ToString("yyyy/MM/dd"), item.Emp_No, item.Factory }).ToList();
                foreach (var entity in Analyses.Analyse)
                {
                    if (!string.IsNullOrEmpty(anomalous))
                    {
                        //分析是否異常
                        audits(entity, item, true, username, employees.Count);
                    }
                    else
                    {
                        audits(entity, item, false, username, employees.Count);
                    }
                    entity.factory = item.Emp_ID;
                }
            }

            if (!string.IsNullOrEmpty(anomalous))
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name && (m.Abnormal_Flag == "1" || m.Abnormal_Flag == "2" || m.Abnormal_Flag == "3" || m.Abnormal_Flag == "4")).OrderBy(m => m.Create_Time).ToList();
            }
            else
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name).OrderBy(m => m.Create_Time).ToList();
            }

            //EmptyAbnormalAnalysis();

            return PartialView("Audit/_Analyse", Analyses);

        }
        

        //設置為自動發送郵件
        [HandleErrors]
        public ActionResult SendAllAnalyse()
        {
            // Departments(select distinct DeptNo,Factory from dbo.MailingList)
            var Departments = mailingListRepository.GetAll().Select(e => new { e.DeptNo, e.Factory }).Distinct().ToList();
            //取当天的前7天的一天的异常记录
            var dtNow = DateTime.Today;
            var DataTimeDay = ConfigurationManager.AppSettings["DataTimeDay"];//此参数程序中没有用到
            var DaySpace = ConfigurationManager.AppSettings["DaySpace"];
            var DayDefer = ConfigurationManager.AppSettings["DayDefer"];
            var startDate = dtNow.AddDays(Convert.ToInt16(-Convert.ToInt16(DayDefer) - Convert.ToInt16(DaySpace)));
            var endDate = dtNow.AddDays(-Convert.ToInt16(DayDefer));
            SMTPEmailHandler smtpmail = new SMTPEmailHandler();
            foreach (var Depts in Departments)
            {
                EmptyAbnormalAnalysis("AuitMail");
                //郵件地址
                try
                {
                    //EmpNoMail：邮件地址,
                    var EmpNoMail = mailingListRepository.GetMany(e => e.DeptNo == Depts.DeptNo && e.Factory == Depts.Factory).Select(e => new { e.EmpEmail });
                    var deptSplit = Depts.DeptNo.Split(',');
                    List<string> ListDept = new List<string>();

                    if (deptSplit.Length > 1)
                    {
                        //根據部門編號來分部門起止
                        ListDept = employeeRepository.SqlQuery<string>("exec sp_DeptBeginEnd {0},{1},{2}", new object[] { Depts.Factory, deptSplit[0], deptSplit[1] }).ToList();
                    }
                    else
                    {
                        ListDept.Add(deptSplit[0]);
                    }
                    /*
                    if (deptSplit.Length > 1)
                    {
                        //根據部門編號來分部門起止sp_DeptBeginEnd
                        ListDept = employeeRepository.SqlQuery<string>("exec getAuditFactorys {0},{1},{2},{3},{4}",
                            new object[] { Depts.Factory, deptSplit[0], deptSplit[1], startDate, endDate }).ToList();
                    }
                    else
                    {
                        ListDept = employeeRepository.SqlQuery<string>("exec getAuditFactorys {0},{1},{2},{3},{4}",
                            new object[] { Depts.Factory, deptSplit[0], deptSplit[0], startDate, endDate }).ToList();
                        //ListDept.Add(deptSplit[0]);
                    }*/

                    foreach (var DeptNo in ListDept)
                    {
                        if (!string.IsNullOrEmpty(DeptNo))
                        {
                            //員工信息

                            var employees = employeeRepository.SqlQuery<Employee_Full>("exec sp_GetEmployee {0},{1},{2},{3}",new object[] { startDate, endDate, Depts.Factory, DeptNo }).ToList();
                            //var employees = employeeRepository.GetMany(m => m.Dept_No == DeptNo && m.Factory == Depts.Factory && (m.Emp_Type == "JO01" || m.Emp_Type == "JO02")).Distinct().ToList();
                            // var employees = employeeRepository.GetMany(m => m.Dept_No == DeptNo && m.Factory == Depts.Factory).Distinct().ToList();
                            foreach (var item in employees)
                            {
                                //排班信息　異常信息　加班信息
                                var Analyse = absenceRepository.SqlQuery<Analyse>("exec sp_analytical {0},{1},{2},{3}", new object[] { startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"), item.Emp_No, item.Factory }).ToList();
                                foreach (var entity in Analyse)
                                {
                                    //分析是否為異常
                                    audits(entity, item, true, "AuitMail", employees.Count);
                                    //記錄員工ID
                                    entity.factory = item.Emp_ID;
                                }
                            }
                        }
                    }
                    var Analyses = new AnalyseForm();
                    var DataTable = analysisRepository.ProcQuery("sp_AbnormalAnalysis", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value="AuitMail",ParameterName="Create_By"}
                });
                    Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == "AuitMail").OrderBy(m => m.Dept_No).ToList();

                    //異常才發郵件
                    if (DataTable != null && DataTable.Rows.Count > 0)
                    {
                        Microsoft.Reporting.WebForms.ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                        //設置報表路徑
                        rv.LocalReport.ReportPath = @"Report\Report.rdlc";
                        //設置報表數據源
                        ReportDataSource reportDataSource = new ReportDataSource("DataSet", DataTable);
                        rv.LocalReport.DataSources.Add(reportDataSource);
                        rv.LocalReport.Refresh();
                        //生是ＥＸＥＣＬ文件字節碼
                        var result = rv.LocalReport.Render("EXCEL");
                        try
                        {
                            //將excel寫入文件

                            var tempfilename = Server.MapPath("~/temp/AttendanceAudit-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second);
                            var xlsfileName = tempfilename + ".xls";
                            System.IO.FileStream stream = new System.IO.FileStream(tempfilename + ".xls", System.IO.FileMode.OpenOrCreate);
                            stream.Write(result, 0, result.Length);
                            stream.Close();

                            //生成excel 字符串
                            System.Web.UI.HtmlControls.HtmlGenericControl htmlcol = new System.Web.UI.HtmlControls.HtmlGenericControl();
                             /* 
                             Excel.Application app = new Excel.Application();
                             app.Visible = false;
                             Object o = System.Reflection.Missing.Value;
                             Excel.Workbook xls = app.Application.Workbooks.Open(tempfilename + ".xls", o, o, o, o, o, o, o, o, o, o, o, o, o, o);
                             string fileName = tempfilename + ".html";
                             var format = Excel.XlFileFormat.xlHtml;//Html  
                             xls.SaveAs(fileName, format, o, o, o, o, Excel.XlSaveAsAccessMode.xlExclusive, o, o, o, o, o);
                             xls.Close(false, Type.Missing, Type.Missing);
                             app.Quit();
                             System.Diagnostics.Process[] myProcesses = System.Diagnostics.Process.GetProcessesByName("EXCEL");
                             foreach (System.Diagnostics.Process myProcess in myProcesses)
                             {
                                 myProcess.Kill();
                             }
                             htmlcol.InnerHtml = System.IO.File.OpenText(fileName).ReadToEnd(); 
                             */


                            using (System.IO.StringWriter writer = new System.IO.StringWriter())
                            {
                                //根據視圖文件生html table代碼的excel
                                IView view = ViewEngines.Engines.FindPartialView(ControllerContext, "Audit/_Analysexls").View;
                                var Partia = PartialView("Audit/_Analysexls", Analyses);
                                ViewContext viewContext = new ViewContext(ControllerContext, view, Partia.ViewData, Partia.TempData, writer);
                                viewContext.View.Render(viewContext, writer);
                                htmlcol.InnerHtml = writer.ToString();

                            }

                            //System.IO.StreamReader srhtml = System.IO.File.OpenText(fileName);
                            // string s = System.Text.Encoding.UTF8.GetString(img);
                            // var htmls = srhtml.ReadToEnd();
                            //System.IO.StreamReader srhtml = System.IO.File.OpenText(Server.MapPath(tempfilename));
                            //var html = srhtml.ReadToEnd();
                            string MailAddress = "";
                            //同一部門收件人一起發送郵件
                            foreach (var item in EmpNoMail)
                            {
                                if (!string.IsNullOrEmpty(MailAddress))
                                {
                                    MailAddress = MailAddress + ";" + item.EmpEmail;
                                }
                                else
                                {
                                    MailAddress = item.EmpEmail;
                                }
                            }
                            if (!string.IsNullOrEmpty(MailAddress))
                            {
                                //
                                var sub = ConfigurationManager.AppSettings["AuditEmailSub"];
                                //ConfigurationManager.AppSettings["AuditEmailBody"]

                                System.IO.StreamReader sr = System.IO.File.OpenText(Server.MapPath("~/temp/E-Mail.txt"));
                                var body = sr.ReadToEnd();
                                sr.Close();
                                //替換郵件發送時間
                                body = body.Replace("#date", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                //smtpmail.SendMail(MailAddress, sub, body + htmlcol.InnerHtml, "");
                                //發送郵件
                                string fil = Server.MapPath("~/temp/AttendanceAccessAuditDataAnalysisFlowChart.jpg");
                               
                                
                                smtpmail.SendMail(MailAddress, Depts.Factory + sub + "(" + DateTime.Now.ToString("yyyy/MM/dd") + ")" + "(共" + DataTable.Rows.Count + "筆異常)", htmlcol.InnerHtml + body, xlsfileName + "," + fil, "");

                               // LogHelper.WriteOperateLog("AuitJobMail", "考勤稽核", "定時發送異常郵件", "AuitJobMail");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog(ex);
                        }

                        //System.Web.UI.HtmlControls.HtmlGenericControl htmlcol = new System.Web.UI.HtmlControls.HtmlGenericControl();
                        //using (System.IO.StringWriter writer = new System.IO.StringWriter())
                        //{
                        //    IView view = ViewEngines.Engines.FindPartialView(ControllerContext, "Audit/_Analysexls").View;
                        //    var Partia = PartialView("Audit/_Analysexls", Analyses);
                        //    ViewContext viewContext = new ViewContext(ControllerContext, view, Partia.ViewData, Partia.TempData, writer);
                        //    viewContext.View.Render(viewContext, writer);
                        //    htmlcol.InnerHtml = writer.ToString();
                        //    var file = ToExcel(htmlcol, "~/temp/attendance" + DateTime.Now.Ticks.ToString());
                        //    string MailAddress = "";
                        //    foreach (var item in EmpNoMail)
                        //    {
                        //        if (!string.IsNullOrEmpty(MailAddress))
                        //        {
                        //            MailAddress = MailAddress + ";" + item.EmpEmail;
                        //        }
                        //        else
                        //        {
                        //            MailAddress = item.EmpEmail;
                        //        }
                        //    }
                        //    if (!string.IsNullOrEmpty(MailAddress))
                        //    {
                        //        var sub = ConfigurationManager.AppSettings["AuditEmailSub"];
                        //        //ConfigurationManager.AppSettings["AuditEmailBody"]

                        //        System.IO.StreamReader sr = System.IO.File.OpenText(Server.MapPath("~/temp/E-Mail.txt"));
                        //        var body = sr.ReadToEnd();
                        //        sr.Close();

                        //        //smtpmail.SendMail(MailAddress, sub, body + htmlcol.InnerHtml, "");
                        //        smtpmail.SendMail(MailAddress, sub, body + htmlcol.InnerHtml, file, "");
                        //        LogHelper.WriteOperateLog("AuitJobMail", "考勤稽核", "定時發送異常郵件", HttpContext.User.Identity.Name);
                        //    }
                        //}
                    }
                }
                catch (Exception Exc)
                {
                    LogHelper.WriteLog(Exc);
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);

        }


        //排班情況
        public ActionResult Arrange(string day, string empno)
        {
            var absenceEntity = absenceRepository.Get(a => a.Absence_Day == day && a.Emp_No == empno);
            var ememploye = employeeRepository.Get(e => e.Emp_No == empno);
            var overtimeEntity = overtimeRepository.Get(a => a.Overtime_Day == day && a.Emp_No == empno);
            var schedulingEntity = schedulingRepository.Get(a => a.Carding_Day == day && a.Emp_No == empno);
            var ArrangeFormModel = new ArrangeFormModel();
            ArrangeFormModel.day = day;
            ArrangeFormModel.empno = empno;
            ArrangeFormModel.Factory = ememploye.Factory;
            ArrangeFormModel.Department = ememploye.Dept_Name;
            ArrangeFormModel.Name = ememploye.Emp_Name;
            ArrangeFormModel.Absence = Mapper.Map<Absence, AbsenceFormModel>(absenceEntity);
            ArrangeFormModel.Overtime = Mapper.Map<Overtime, OvertimeFormModel>(overtimeEntity);
            ArrangeFormModel.Scheduling = Mapper.Map<Scheduling, SchedulingFormModel>(schedulingEntity);

            return PartialView("Audit/_Arrange", ArrangeFormModel);
        }

        //考勤門禁大門記錄
        public ActionResult ClockRecord(string StartDate, string endDate, string empno, string cardno)
        {
            var entity = employeeRepository.SqlQuery<ClockRecord>("exec sp_ClockRecord {0} , {1},{2},{3}", new object[] { StartDate, endDate, empno, cardno });
            return PartialView("Audit/_ClockRecord", entity);
        }
        
    
        /// <summary>
        /// 分析
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="endDate"></param>
        /// <param name="empno"></param>
        /// <param name="anomalous"></param>
        /// <returns></returns>
        [HandleErrors]
        public ActionResult Analyse(string StartDate, string endDate, string empno, string anomalous)
        {
            var Analyses = new AnalyseForm();
            string UserName = HttpContext.User.Identity.Name;
            //根据登陆的用户名清空原来的分析数据。重新分析
            EmptyAbnormalAnalysis(UserName);
            var ememploye = employeeRepository.Get(e => e.Emp_No == empno);

            //执行数据库的存储过程给Analyses.Analyse赋值         
            Analyses.Analyse = absenceRepository.SqlQuery<Analyse>("exec sp_analytical {0},{1},{2},{3}", new object[] { StartDate, endDate, empno, ememploye.Factory }).ToList();
            foreach (var entity in Analyses.Analyse)
            {
                if (!string.IsNullOrEmpty(anomalous))
                {
                    audits(entity, ememploye, true, UserName, 1);
                }
                else
                {
                    audits(entity, ememploye, false, UserName, 1);
                }
                entity.factory = ememploye.Emp_ID;
            }

            if (!string.IsNullOrEmpty(anomalous))
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name && (m.Abnormal_Flag == "1" || m.Abnormal_Flag == "2" || m.Abnormal_Flag == "3" || m.Abnormal_Flag == "4")).OrderBy(m => m.Create_Time).ToList();
            }
            else
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name).OrderBy(m => m.Create_Time).ToList();
            }

            //EmptyAbnormalAnalysis();
            return PartialView("Audit/_Analyse", Analyses);
        }
        [HandleErrors]
        public FileResult attendancexls(string anomalous)
        {
            Response.Clear();
            Response.AppendHeader("content-disposition", "attachment;filename=\"" + System.Web.HttpUtility.UrlEncode("attendance", System.Text.Encoding.UTF8) + ".xls\"");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            var Analyses = new AnalyseForm();
            if (!string.IsNullOrEmpty(anomalous))
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name && (m.Abnormal_Flag == "1" || m.Abnormal_Flag == "2" || m.Abnormal_Flag == "3" || m.Abnormal_Flag == "4")).OrderBy(m => m.Create_Time).ToList();
            }
            else
            {
                Analyses.AbnormalAnalysis = analysisRepository.GetMany(m => m.Create_By == HttpContext.User.Identity.Name).OrderBy(m => m.Create_Time).ToList();
            }
            System.Web.UI.HtmlControls.HtmlGenericControl htmlcol = new System.Web.UI.HtmlControls.HtmlGenericControl();
            using (System.IO.StringWriter writer = new System.IO.StringWriter())
            {
                IView view = ViewEngines.Engines.FindPartialView(ControllerContext, "Audit/_Analysexls").View;
                var Partia = PartialView("Audit/_Analysexls", Analyses);
                ViewContext viewContext = new ViewContext(ControllerContext, view, Partia.ViewData, Partia.TempData, writer);
                viewContext.View.Render(viewContext, writer);
                htmlcol.InnerHtml = writer.ToString();
                var file = ToExcel(htmlcol, "~/temp/attendance");
                return File(file, "application/ms-excel");
            }
        }
        public string ToExcel(System.Web.UI.HtmlControls.HtmlGenericControl divControl, string filename)
        {

            string style = @"<style> td { mso-number-format:\@; } </style>";

            System.IO.FileStream fs = new System.IO.FileStream(Server.MapPath(filename + ".xls"), System.IO.FileMode.OpenOrCreate);//创建文件

            System.IO.StreamWriter stringWrite = new System.IO.StreamWriter(fs);
            System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);
            stringWrite.WriteLine(style);
            divControl.RenderControl(htmlWrite);


            ////清空缓冲区
            stringWrite.Flush();
            ////关闭流
            stringWrite.Close();
            fs.Close();
            return Server.MapPath(filename + ".xls");

        }

        [HandleErrors]
        public void EmptyAbnormalAnalysis(string UserName)
        {
            try
            {
                analysisRepository.ProcQuery("sp_deleteAnalysis", new SqlParameter[]{ new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=UserName,ParameterName="Create_By"} });
                //LogHelper.WriteOperateLog("刪除", "考勤稽核", "根據用戶名刪除之前生成的異常分析記錄", UserName);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 此方法程序中没有用到
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Employee"></param>
        /// <param name="anomalous"></param>
        /// <param name="username"></param>
        private void audit(Analyse entity, Employee_Full Employee, bool anomalous, string username)
        {
            //設置員工信息
            var Analysis = new AbnormalAnalysis();
            Analysis.Factory = Employee.Factory;
            Analysis.Emp_No = Employee.Emp_No;
            Analysis.Emp_Name = Employee.Emp_Name;
            Analysis.Dept_Name = Employee.Dept_Name;
            Analysis.Dept_No = Employee.Dept_No;
            Analysis.Emp_Title = Employee.Title;
            Analysis.Emp_Type = Employee.Emp_Type_Desc;
            Analysis.Carding_Day = entity.carding_day;
            //上班時段
            Analysis.Carding_List = Convert.ToDateTime(entity.carding_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.carding_end_time).ToString("HH:mm");
            Analysis.Carding_Hours = entity.carding_hours;
            Analysis.Overtime_Hours = entity.overtime_hours;

            if (string.IsNullOrEmpty(entity.overtime_begin_time))
            {
                Analysis.Overtime_Apply = "N/A";
            }
            else
            {
                //加班時段
                Analysis.Overtime_Apply = Convert.ToDateTime(entity.overtime_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.overtime_end_time).ToString("HH:mm");
            }

            Analysis.Absence_List = Convert.ToDateTime(entity.absence_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.absence_end_time).ToString("HH:mm");
            Analysis.Totalhours_Access = entity.Totalhours_Access;
            Analysis.Totalhours_Plant = entity.Totalhours_Plant;


            var EmpType = Employee.Emp_Type_Desc;
            Analysis.Create_By = username;
            var cardId = entity.emp_id;
            var EmpNo = Employee.Emp_No;
            var carding_day = ToDateTime(entity.carding_day);
            var emptype = Employee.Emp_Type;
            //住宿情況
            var bide = BideRepository.Get(b => b.Emp_No == EmpNo && (b.startDate <= carding_day.Value && b.endDate >= carding_day.Value));
            if (bide == null)
            {
                bide = BideRepository.GetMany(b => b.Emp_No == EmpNo).OrderBy(b => b.endDate).FirstOrDefault();
            }

            if (bide != null)
            {
                if (bide.Status)
                {
                    Analysis.Card_Type = "外宿";
                }
                else { Analysis.Card_Type = "內宿"; }
            }
            else
            {
                Analysis.Card_Type = "外宿";
            }
            var daybeginaccesscheck1 = ToDateTime(entity.carding_actual_begin);
            var daybeginaccesscheck2 = daybeginaccesscheck1;
            List<string> empty = new List<string>();
            IEnumerable<string> attTime = empty;
            List<AtttimeFlag> attTimeFlag = new List<AtttimeFlag>();
            var AnalysisiL = createTo(Analysis);
            AnalysisiL.Abnormal_Flag = "0";
            AnalysisiL.Card_Type = AnalysisiL.Card_Type ?? "";
            AnalysisiL.Access_Card_List = "";
            AnalysisiL.Card_List = "";
            AnalysisiL.Clock_Type = "";
            AnalysisiL.AttType = "";

            //休息日不作稽核
            if (entity.carding_type != "N" && string.IsNullOrEmpty(entity.overtime_hours))
            {
                if (!anomalous)
                {
                    handle(OA(AnalysisiL, "R"));
                }
            }
            else
            {

                //免刷卡不作稽核
                if (entity.carding_flag == "N")
                {
                    if (!anomalous)
                    {
                        handle(OA(AnalysisiL, "A"));
                    }
                }
                else
                {
                    //正常請假出差
                    if (!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type))
                    {
                        if (!anomalous)
                        {
                            handle(OA(AnalysisiL, "R"));
                        }
                    }
                    else
                    {
                        /************* 上班廠區大門門禁記錄 ************/
                        attTime = empty;
                        daybeginaccesscheck1 = ToDateTime(entity.carding_begin_time);
                        if (daybeginaccesscheck1.HasValue)
                        {
                            daybeginaccesscheck2 = ToDateTime(entity.overtime_end_time);
                            if (!daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck2 = ToDateTime(entity.carding_end_time);
                            }
                            if (daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-90);
                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(90);
                                attTimeFlag = employeeRepository.SqlQuery<AtttimeFlag>("exec sp_AccessrecordTimeIO {0},{1},{2}", new object[] {
                                            daybeginaccesscheck1,
                                            daybeginaccesscheck2, 
                                            EmpNo
                                        }).ToList().OrderBy(e => e.att_time).ToList();
                            }
                        }
                        if (attTimeFlag.Count() == 0)
                        {

                            if ((entity.carding_flag != "N") && (emptype == "JO01" || emptype == "JO02") && (Analysis.Card_Type == "外宿") && !(!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type)))
                            {
                                AnalysisiL.AttType = "1";
                                AnalysisiL.Abnormal_Flag = "4";
                                AnalysisiL.Access_Card_List = "無記錄<br/>";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷廠區大門門禁卡<br/>";
                            }
                            else
                            {
                                if (!anomalous)
                                {
                                    AnalysisiL.Access_Card_List = "無記錄";
                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "廠區門禁無異常<br/>";
                                }
                            }
                        }
                        else
                        {
                            string io = "";
                            if (!anomalous)
                            {
                                for (int i = 0; i < attTimeFlag.Count; i++)
                                {

                                    if (attTimeFlag[i].Flag)
                                    {
                                        io = "出";
                                    }
                                    else
                                    {
                                        io = "進";
                                    }
                                    AnalysisiL.Access_Card_List = AnalysisiL.Access_Card_List + attTimeFlag[i].att_time.ToString("dd HH:mm") + io + "<br/>";

                                }
                            }
                            if (attTimeFlag.Count < 2)
                            {
                                if ((entity.carding_flag != "N") && (emptype == "JO01" || emptype == "JO02") && (Analysis.Card_Type == "外宿") && !(!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type)))
                                {
                                    AnalysisiL.AttType = "1";
                                    AnalysisiL.Abnormal_Flag = "4";
                                    if (attTimeFlag[0].Flag)
                                    {
                                        io = "出";
                                    }
                                    else
                                    {
                                        io = "進";
                                    }

                                    AnalysisiL.Access_Card_List = attTimeFlag[0].att_time.ToString("dd HH:mm") + io + "<br/>";
                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未落實廠區大門門禁<br/>";
                                }
                                else
                                {
                                    if (!anomalous)
                                    {
                                        AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "廠區大門門禁無異常<br/>";
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(entity.carding_actual_begin))
                        {
                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷上班考勤卡<br/>";
                            AnalysisiL.Card_List = "上班考勤<br/>";
                        }
                        else
                        {
                            if (!anomalous || (!WhetherLeave(entity.absence_type)))
                            {
                                var attime = ToDateTime(entity.carding_actual_begin);
                                var str = "";
                                if (attime.HasValue)
                                {
                                    str = attime.Value.ToString("dd HH:mm");
                                }
                                else
                                {
                                    str = entity.carding_actual_begin;
                                }
                                AnalysisiL.Card_List = str + "上班考勤<br/>";
                                //AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "上班考勤無異常<br/>";
                            } //上班卡 上班考勤
                        }
                        if (EmpType != "間接人員")
                        {
                            /************* 上班門禁記錄 ************/
                            daybeginaccesscheck1 = ToDateTime(entity.carding_actual_begin);
                            daybeginaccesscheck2 = ToDateTime(entity.carding_actual_end);
                            if (daybeginaccesscheck1.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-10);
                                if (daybeginaccesscheck2.HasValue)
                                {
                                    daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(10);
                                    attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime {0},{1},{2},{3}", new object[] {
                                daybeginaccesscheck1,
                                daybeginaccesscheck2, 
                                cardId, 
                                "AccessCheck" 
                                 }).ToList();
                                }
                            }
                            if (attTime.Count() == 0)
                            {
                                AnalysisiL.Clock_Type = "1";
                                AnalysisiL.Abnormal_Flag = "4";
                                AnalysisiL.Plant_List = AnalysisiL.Plant_List + "上班門禁<br/>";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷上班門禁卡<br/>";
                                //未刷上班門禁
                            }
                            else
                            {
                                if (!anomalous)
                                {
                                    var attime = ToDateTime(attTime.First());

                                    if (attime.HasValue)
                                    {
                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + attime.Value.ToString("dd HH:mm") + "上班門禁<br/>";
                                    }
                                    else
                                    {
                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + attTime.First() + "上班門禁<br/>";
                                    }

                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "上班門禁無異常<br/>";
                                }  //上班門禁
                            }
                        }
                        /************* 上班期間之門禁出入記錄 ************/
                        attTime = empty;
                        daybeginaccesscheck1 = ToDateTime(entity.carding_begin_time);
                        daybeginaccesscheck2 = ToDateTime(entity.carding_end_time);
                        if (daybeginaccesscheck1.HasValue)
                        {
                            daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(10);
                            if (daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(-10);
                                attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime17 {0},{1},{2},{3}", new object[] {
                                    daybeginaccesscheck1,
                                    daybeginaccesscheck2, 
                                    cardId, 
                                    "AccessCheck" 
                                    }).ToList();
                            }
                        }

                        if (attTime.Count() > 0)
                        {
                            List<string> ListTime = attTime.ToList();
                            foreach (var item in ListTime)
                            {
                                var attrtime = ToDateTime(item);
                                if (((attrtime >= entity.break_begin_a) && (attrtime <= entity.break_end_a)) ||
                                    ((attrtime >= entity.break_begin_b) && (attrtime <= entity.break_end_b)) ||
                                    ((attrtime >= entity.break_begin_c) && (attrtime <= entity.break_end_c)) ||
                                    ((attrtime >= entity.break_begin_d) && (attrtime <= entity.break_end_d))
                                    )
                                {
                                    string breakbeginendstring = "";

                                    if ((attrtime >= entity.break_begin_a) && (attrtime <= entity.break_end_a))
                                    {
                                        breakbeginendstring = entity.break_begin_a.Value.ToString("HH:mm") + " / " + entity.break_end_a.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_b) && (attrtime <= entity.break_end_b))
                                    {
                                        breakbeginendstring = entity.break_begin_b.Value.ToString("HH:mm") + " / " + entity.break_end_b.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_c) && (attrtime <= entity.break_end_c))
                                    {
                                        breakbeginendstring = entity.break_begin_c.Value.ToString("HH:mm") + " / " + entity.break_end_c.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_d) && (attrtime <= entity.break_end_d))
                                    {
                                        breakbeginendstring = entity.break_begin_d.Value.ToString("HH:mm") + " / " + entity.break_end_d.Value.ToString("HH:mm");
                                    }
                                    var attime = ToDateTime(item);
                                    var str = "";
                                    if (attime.HasValue)
                                    {
                                        str = attime.Value.ToString("dd HH:mm");
                                    }
                                    else
                                    {
                                        str = item;
                                    } if (!anomalous)
                                    {
                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "休息<br/>";
                                        AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "休息時段出入<br/>";
                                    }
                                    //休息時段出入
                                }
                                else
                                {
                                    var attime = ToDateTime(item);
                                    var str = "";
                                    if (attime.HasValue)
                                    {
                                        str = attime.Value.ToString("dd HH:mm");
                                    }
                                    else
                                    {
                                        str = item;
                                    }
                                    if ((!string.IsNullOrEmpty(entity.overtime_begin_time)) && (!string.IsNullOrEmpty(entity.overtime_end_time)))
                                    {
                                        if ((attrtime >= ToDateTime(entity.carding_end_time)) && (attrtime <= ToDateTime(entity.overtime_begin_time)))
                                        {
                                            if (!anomalous)
                                            {
                                                AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "下班<br/>";
                                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班時間出入<br/>";
                                            }
                                        }
                                        else
                                        {
                                            AnalysisiL.Absence_Apply = "1";
                                            AnalysisiL.Abnormal_Flag = "4";
                                            AnalysisiL.Plant_List = AnalysisiL.Plant_List + "" + str + "離崗<br/> ";
                                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "應在崗期間進出工作場所<br/>";
                                        }
                                    }
                                    else
                                    {
                                        AnalysisiL.Absence_Apply = "1";
                                        AnalysisiL.Abnormal_Flag = "4";
                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + "" + str + "離開<br/>";
                                        AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "應在崗期間進出工作場所<br/>";

                                    }
                                    //應在崗期間進出工作場所
                                }
                            }
                            //AnalysisiL.Absence_Apply = "1";
                            if (attTime.Count() == 2)
                            {
                                if (AnalysisiL.Absence_Apply == "1")
                                {
                                    var attrtime = ToDateTime(attTime.First());
                                    long end = attrtime.Value.Ticks / 10000000;
                                    attrtime = ToDateTime(attTime.Last());
                                    long begin = attrtime.Value.Ticks / 10000000;
                                    double tks = (end - begin);
                                    var levaeH = tks / 60;
                                    AnalysisiL.leave_Hours = levaeH.ToString();
                                }
                            }
                        }
                        /***************下班考勤*******************/
                        if (string.IsNullOrEmpty(entity.carding_actual_end))
                        {
                            AnalysisiL.Card_List = AnalysisiL.Card_List + "下班考勤<br/> ";
                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷下班考勤卡<br/>";
                        }
                        else
                        {
                            var attime = ToDateTime(entity.carding_actual_end);
                            var str = "";
                            if (attime.HasValue)
                            {
                                str = attime.Value.ToString("dd HH:mm");
                            }
                            else
                            {
                                str = entity.carding_actual_end;
                            }
                            if (!anomalous || !WhetherLeave(entity.absence_type))
                            {
                                AnalysisiL.Card_List = AnalysisiL.Card_List + str + "下班考勤<br/> ";
                                //AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班考勤無異常<br/>";
                            }
                        }
                        if (EmpType != "間接人員")
                        {
                            /************* 下班門禁記錄 ************/
                            attTime = empty;
                            daybeginaccesscheck1 = ToDateTime(entity.carding_actual_end);
                            daybeginaccesscheck2 = ToDateTime(entity.carding_actual_end);
                            if (daybeginaccesscheck1.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-10);
                                if (daybeginaccesscheck2.HasValue)
                                {
                                    daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(10);
                                    attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime {0},{1},{2},{3}", new object[] {
                                daybeginaccesscheck1,
                                daybeginaccesscheck2, 
                                cardId, 
                                "AccessCheck" 
                                }).ToList();
                                }
                            }
                            if (attTime.Count() == 0)
                            {
                                AnalysisiL.Clock_Type = "1";
                                AnalysisiL.Abnormal_Flag = "4";
                                AnalysisiL.Plant_List = AnalysisiL.Plant_List + "下班門禁<br/> ";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷下班門禁卡<br/>";
                            }
                            else
                            {
                                var attime = ToDateTime(attTime.First());
                                var str = "";
                                if (attime.HasValue)
                                {
                                    str = attime.Value.ToString("dd HH:mm");
                                }
                                else
                                {
                                    str = attTime.First();
                                }
                                if (!anomalous)
                                {
                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "下班門禁<br/> ";
                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班門禁無異常<br/>";
                                }
                            }
                        }
                        switch (entity.absence_type)
                        {
                            case "曠工":
                                //Card_List
                                AnalysisiL.Abnormal_Flag = "1";
                                break;
                            case "遲到":
                                AnalysisiL.Abnormal_Flag = "2";
                                break;

                            case "早退":
                                AnalysisiL.Abnormal_Flag = "3";
                                break;
                            default: break;
                        }
                        if (!anomalous)
                        {
                            handle(AnalysisiL);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(AnalysisiL.Abnormal_Flag) && AnalysisiL.Abnormal_Flag != "0")
                            {
                                handle(AnalysisiL);
                            }
                        }
                    }
                }


            }
        }


        /// <summary>
        /// 异常分析：不將廠區作為主要參考條件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Employee"></param>
        /// <param name="anomalous"></param>
        /// <param name="username"></param>
        /// <param name="count"></param>
        private void audits(Analyse entity, Employee_Full Employee, bool anomalous, string username, int count)
        {
            //設置員工信息
            var Analysis = new AbnormalAnalysis();
            var Factory = Employee.Factory;
            Analysis.Factory = Employee.Factory;
            Analysis.Emp_No = Employee.Emp_No;
            Analysis.Emp_Name = Employee.Emp_Name;
            Analysis.Dept_Name = Employee.Dept_Name;
            Analysis.Dept_No = Employee.Dept_No;
            Analysis.Emp_Title = Employee.Title;
            Analysis.Emp_Type = Employee.Emp_Type_Desc;
            Analysis.Carding_Day = entity.carding_day;

            //上班時段
            Analysis.Carding_List = Convert.ToDateTime(entity.carding_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.carding_end_time).ToString("HH:mm");
            Analysis.Carding_Hours = entity.carding_hours;
            Analysis.Overtime_Hours = entity.overtime_hours;

            if (string.IsNullOrEmpty(entity.overtime_begin_time))
            {
                Analysis.Overtime_Apply = "N/A";
            }
            else
            {
                //加班時段
                Analysis.Overtime_Apply = Convert.ToDateTime(entity.overtime_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.overtime_end_time).ToString("HH:mm");
            }

            if (string.IsNullOrEmpty(entity.absence_begin_time))
            {
                Analysis.Absence_List = "N/A";
            }
            else
            {
                 //请假時段
                if (entity.absence_type == "事假" || entity.absence_type == "年休假" || entity.absence_type == "出差假" || entity.absence_type == "產假")
                {
                    Analysis.Absence_List = Convert.ToDateTime(entity.absence_begin_time).ToString("HH:mm") + "~" + Convert.ToDateTime(entity.absence_end_time).ToString("HH:mm");
                }
                else{
                    Analysis.Absence_List = "N/A";
                }
            }         
            Analysis.Totalhours_Access = entity.Totalhours_Access;
            Analysis.Totalhours_Plant = entity.Totalhours_Plant;

            var EmpType = Employee.Emp_Type_Desc;
            Analysis.Create_By = username;
            var cardId = entity.emp_id;
            var EmpNo = Employee.Emp_No;
            var carding_day = ToDateTime(entity.carding_day);
            var emptype = Employee.Emp_Type;
            //住宿情況
            var bide = BideRepository.Get(b => b.Emp_No == EmpNo && (b.startDate <= carding_day.Value && b.endDate >= carding_day.Value));
            if (bide == null)
            {
                bide = BideRepository.GetMany(b => b.Emp_No == EmpNo).OrderBy(b => b.endDate).FirstOrDefault();
            }

            if (bide != null)
            {
                if (bide.Status)
                {
                    Analysis.Card_Type = "外宿";
                }
                else { Analysis.Card_Type = "內宿"; }
            }
            else
            {
                Analysis.Card_Type = "外宿";
            }
            var daybeginaccesscheck1 = ToDateTime(entity.carding_actual_begin);
            var daybeginaccesscheck2 = daybeginaccesscheck1;
            List<string> empty = new List<string>();
            IEnumerable<string> attTime = empty;
            List<AtttimeFlag> attTimeFlag = new List<AtttimeFlag>();
            var AnalysisiL = createTo(Analysis);
            AnalysisiL.Abnormal_Flag = "0";
            AnalysisiL.Card_Type = AnalysisiL.Card_Type ?? "";
            AnalysisiL.Access_Card_List = "";
            AnalysisiL.Card_List = "";
            AnalysisiL.Clock_Type = "";
            AnalysisiL.AttType = count.ToString();

            //休息日不作稽核
            if (entity.carding_type != "N" && string.IsNullOrEmpty(entity.overtime_hours))
            {
                if (!anomalous)
                {
                    handle(OA(AnalysisiL, "R"));
                }
            }
            else
            {

                //免刷卡不作稽核
                if (entity.carding_flag == "N")
                {
                    if (!anomalous)
                    {
                        handle(OA(AnalysisiL, "A"));
                    }
                }
                else
                {
                    //正常請假出差
                    if (!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type))
                    {
                        if (!anomalous)
                        {
                            handle(OA(AnalysisiL, "R"));
                        }
                    }
                    else
                    {
                        /************* 上班廠區大門門禁記錄 ************/
                        attTime = empty;
                        daybeginaccesscheck1 = ToDateTime(entity.carding_begin_time);
                        if (daybeginaccesscheck1.HasValue)
                        {
                            daybeginaccesscheck2 = ToDateTime(entity.overtime_end_time);
                            if (!daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck2 = ToDateTime(entity.carding_end_time);
                            }
                            if (daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-45);
                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(80);
                                attTimeFlag = employeeRepository.SqlQuery<AtttimeFlag>("exec sp_AccessrecordTimeIO {0},{1},{2}", new object[] {
                                            daybeginaccesscheck1,
                                            daybeginaccesscheck2, 
                                            EmpNo
                                        }).ToList().OrderBy(e => e.att_time).ToList();
                            }
                        }
                        /*
                        if (attTimeFlag.Count() == 0)
                        {

                            if ((entity.carding_flag != "N") && (emptype == "JO01" || emptype == "JO02") && (Analysis.Card_Type == "外宿") && !(!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type)))
                            {
                                AnalysisiL.AttType = "1";
                                AnalysisiL.Abnormal_Flag = "4";
                                AnalysisiL.Access_Card_List = "無記錄<br/>";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷廠區大門門禁卡<br/>";
                            }
                            else
                            {

                                AnalysisiL.Access_Card_List = "無記錄";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "廠區門禁無異常<br/>";

                            }
                        }
                        else
                        {*/
                        string io = "";

                        for (int i = 0; i < attTimeFlag.Count; i++)
                        {

                            if (attTimeFlag[i].Flag)
                            {
                                io = "出";
                            }
                            else
                            {
                                io = "進";
                            }
                            AnalysisiL.Access_Card_List = AnalysisiL.Access_Card_List + attTimeFlag[i].att_time.ToString("dd HH:mm") + io + "<br/>";

                        }
                        /*
                        if (attTimeFlag.Count < 2)
                        {
                            if ((entity.carding_flag != "N") && (emptype == "JO01" || emptype == "JO02") && (Analysis.Card_Type == "外宿") && !(!string.IsNullOrEmpty(entity.absence_type) && WhetherLeave(entity.absence_type)))
                            {
                                AnalysisiL.AttType = "1";
                                AnalysisiL.Abnormal_Flag = "4";
                                if (attTimeFlag[0].Flag)
                                {
                                    io = "出";
                                }
                                else
                                {
                                    io = "進";
                                }

                                AnalysisiL.Access_Card_List = attTimeFlag[0].att_time.ToString("dd HH:mm") + io + "<br/>";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未落實廠區大門門禁<br/>";
                            }
                            else
                            {
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "廠區大門門禁無異常<br/>";
                            }
                        }*/
                        /*   }*/

                        if (string.IsNullOrEmpty(entity.carding_actual_begin))
                        {
                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷上班考勤卡<br/>";
                            AnalysisiL.Card_List = "未刷上班考勤<br/>";
                        }
                        else
                        {

                            var attime = ToDateTime(entity.carding_actual_begin);
                            var str = "";
                            if (attime.HasValue)
                            {
                                str = attime.Value.ToString("dd HH:mm");
                            }
                            else
                            {
                                str = entity.carding_actual_begin;
                            }
                            AnalysisiL.Card_List = str + "上班考勤<br/>";
                            //AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "上班考勤無異常<br/>";
                            //上班卡  上班考勤
                        }
                        if (EmpType != "間接人員")
                        {
                            var FactorySpace = ConfigurationManager.AppSettings["Factory"];
                            var strFactory = FactorySpace.Split(',');
                            var nocard = true;
                           /* foreach (var item in strFactory)
                            {
                                if (Factory == item)
                                {
                                    nocard = false;
                                    break;
                                }
                            }
                            */

                            nocard = false;

                            /************* 上班門禁記錄 ************/

                            //取考勤实际打卡时间前后15分钟内，读取对应门禁记录
                            daybeginaccesscheck1 = ToDateTime(entity.carding_actual_begin);
                            daybeginaccesscheck2 = ToDateTime(entity.carding_actual_begin);
                            if (daybeginaccesscheck1.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-15);


                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(15);
                                attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime {0},{1},{2},{3}", new object[] {daybeginaccesscheck1,daybeginaccesscheck2, cardId, "AccessCheck" }).ToList();

                            }
                            if (attTime.Count() == 0 || (attTime.Count() == 1 && string.IsNullOrEmpty(attTime.First())))
                            {
                                if (nocard)
                                {
                                    AnalysisiL.Clock_Type = "1";
                                    AnalysisiL.Abnormal_Flag = "4";
                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + "未刷上班門禁<br/>";
                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷上班門禁卡<br/>";
                                }
                                //未刷上班門禁
                            }
                            else
                            {

                                var attime = ToDateTime(attTime.First());

                                if (attime.HasValue)
                                {
                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + attime.Value.ToString("dd HH:mm") + "上班門禁<br/>";
                                }
                                else
                                {
                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + attTime.First() + "上班門禁<br/>";
                                }

                                // AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "上班門禁無異常<br/>";
                                //上班門禁
                            }

                        }
                        /************* 上班期間之門禁出入記錄 ************/
                        attTime = empty;
                        daybeginaccesscheck1 = ToDateTime(entity.carding_actual_begin);
                        daybeginaccesscheck2 = ToDateTime(entity.carding_actual_end);
                        if (!daybeginaccesscheck1.HasValue)
                        {
                            daybeginaccesscheck1 = ToDateTime(entity.carding_begin_time);
                        }
                        if (!daybeginaccesscheck2.HasValue)
                        {
                            daybeginaccesscheck2 = ToDateTime(entity.overtime_end_time);
                        }
                        if (!daybeginaccesscheck2.HasValue)
                        {
                            daybeginaccesscheck2 = ToDateTime(entity.carding_end_time);
                        }
                        if (daybeginaccesscheck1.HasValue)
                        {
                            if (daybeginaccesscheck2.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(15);
                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(-15);
                                attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime17 {0},{1},{2},{3}", new object[] {
                                    daybeginaccesscheck1,
                                    daybeginaccesscheck2, 
                                    cardId, 
                                    "AccessCheck" 
                                    }).ToList();
                            }
                        }

                        if (attTime.Count() > 0)
                        {
                            List<string> ListTime = attTime.ToList();
                            foreach (var item in ListTime)
                            {
                                var attrtime = ToDateTime(item);
                                if (((attrtime >= entity.break_begin_a) && (attrtime <= entity.break_end_a)) ||
                                    ((attrtime >= entity.break_begin_b) && (attrtime <= entity.break_end_b)) ||
                                    ((attrtime >= entity.break_begin_c) && (attrtime <= entity.break_end_c)) ||
                                    ((attrtime >= entity.break_begin_d) && (attrtime <= entity.break_end_d))
                                    )
                                {
                                    string breakbeginendstring = "";

                                    if ((attrtime >= entity.break_begin_a) && (attrtime <= entity.break_end_a))
                                    {
                                        breakbeginendstring = entity.break_begin_a.Value.ToString("HH:mm") + " / " + entity.break_end_a.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_b) && (attrtime <= entity.break_end_b))
                                    {
                                        breakbeginendstring = entity.break_begin_b.Value.ToString("HH:mm") + " / " + entity.break_end_b.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_c) && (attrtime <= entity.break_end_c))
                                    {
                                        breakbeginendstring = entity.break_begin_c.Value.ToString("HH:mm") + " / " + entity.break_end_c.Value.ToString("HH:mm");
                                    }
                                    if ((attrtime >= entity.break_begin_d) && (attrtime <= entity.break_end_d))
                                    {
                                        breakbeginendstring = entity.break_begin_d.Value.ToString("HH:mm") + " / " + entity.break_end_d.Value.ToString("HH:mm");
                                    }
                                    var attime = ToDateTime(item);
                                    var str = "";
                                    if (attime.HasValue)
                                    {
                                        str = attime.Value.ToString("dd HH:mm");
                                    }
                                    else
                                    {
                                        str = item;
                                    }

                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "休息" + breakbeginendstring + "<br/>";
                                    // AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "休息時段出入<br/>";
                                    //休息時段出入
                                }
                                else
                                {
                                    var attime = ToDateTime(item);
                                    var str = "";
                                    if (attime.HasValue)
                                    {
                                        str = attime.Value.ToString("dd HH:mm");
                                    }
                                    else
                                    {
                                        str = item;
                                    }
                                    if ((!string.IsNullOrEmpty(entity.overtime_begin_time)) && (!string.IsNullOrEmpty(entity.overtime_end_time)))
                                    {
                                        if ((attrtime >= ToDateTime(entity.carding_end_time)) && (attrtime <= ToDateTime(entity.overtime_begin_time)))
                                        {
                                            string breakbeginendstring = ToDateTime(entity.carding_end_time).Value.ToString("HH:mm") + " / " + ToDateTime(entity.overtime_begin_time).Value.ToString("HH:mm");
                                            AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "下班" + breakbeginendstring + "<br/> ";
                                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班時間出入<br/>";
                                        }
                                        else
                                        {
                                            if ((attrtime > ToDateTime(entity.overtime_begin_time)) && (attrtime < ToDateTime(entity.overtime_end_time) && ToDateTime(entity.overtime_begin_time) > ToDateTime(entity.carding_end_time)))
                                            {
                                                var temptime = ToDateTime(entity.carding_actual_end);
                                                var overtime_begin_time = ToDateTime(entity.overtime_begin_time);
                                                if (item != ListTime.Last())
                                                {
                                                    if ((temptime.HasValue && attrtime < temptime.Value.AddMinutes(-10)) && (overtime_begin_time.Value.AddMinutes(10) < attrtime))
                                                    {
                                                        AnalysisiL.Absence_Apply = "1";
                                                        AnalysisiL.Abnormal_Flag = "4";
                                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + "" + str + "進出車間<br/> ";
                                                        AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "應在崗期間進出工作場所<br/>";
                                                    }
                                                    else
                                                    {
                                                        string breakbeginendstring = ToDateTime(entity.carding_end_time).Value.ToString("HH:mm") + " / " + ToDateTime(entity.overtime_begin_time).Value.ToString("HH:mm");

                                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "休息" + breakbeginendstring + "<br/>";

                                                    }
                                                }
                                                else
                                                {
                                                    if ((temptime.HasValue && attrtime < temptime.Value.AddMinutes(-10)) && (overtime_begin_time.Value.AddMinutes(10) < attrtime))
                                                    {
                                                        AnalysisiL.Absence_Apply = "1";
                                                        AnalysisiL.Abnormal_Flag = "4";
                                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + "" + str + "進出車間<br/> ";
                                                        AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "應在崗期間進出工作場所<br/>";
                                                    }
                                                    else
                                                    {

                                                        string breakbeginendstring = ToDateTime(entity.carding_end_time).Value.ToString("HH:mm") + " / " + ToDateTime(entity.overtime_begin_time).Value.ToString("HH:mm");
                                                        AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "休息" + breakbeginendstring + "<br/>";

                                                    }
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {

                                        if ((attrtime < ToDateTime(entity.carding_end_time).Value.AddMinutes(-10)) && (attrtime > ToDateTime(entity.carding_end_time).Value.AddMinutes(10)))
                                        {
                                            AnalysisiL.Absence_Apply = "1";
                                            AnalysisiL.Abnormal_Flag = "4";
                                            AnalysisiL.Plant_List = AnalysisiL.Plant_List + "" + str + "進出車間<br/> ";
                                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "應在崗期間進出工作場所<br/>";
                                        }
                                    }
                                    //應在崗期間進出工作場所
                                }
                            }
                            //AnalysisiL.Absence_Apply = "1";
                            if (attTime.Count() == 2)
                            {
                                if (AnalysisiL.Absence_Apply == "1")
                                {
                                    var attrtime = ToDateTime(attTime.First());
                                    long end = attrtime.Value.Ticks / 10000000;
                                    attrtime = ToDateTime(attTime.Last());
                                    long begin = attrtime.Value.Ticks / 10000000;
                                    double tks = (end - begin);
                                    var levaeH = tks / 60;
                                    AnalysisiL.leave_Hours = levaeH.ToString();
                                }
                            }
                        }
                        /***************下班考勤*******************/
                        if (string.IsNullOrEmpty(entity.carding_actual_end))
                        {
                            AnalysisiL.Card_List = AnalysisiL.Card_List + "未刷下班考勤<br/> ";
                            AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷下班考勤卡<br/>";
                        }
                        else
                        {
                            var attime = ToDateTime(entity.carding_actual_end);
                            var str = "";
                            if (attime.HasValue)
                            {
                                str = attime.Value.ToString("dd HH:mm");
                            }
                            else
                            {
                                str = entity.carding_actual_end;
                            }

                            //if (!anomalous || !WhetherLeave(entity.absence_type))

                            AnalysisiL.Card_List = AnalysisiL.Card_List + str + "下班考勤<br/> ";
                            // AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班考勤無異常<br/>";
                        }
                        if (EmpType != "間接人員")
                        {
                            var FactorySpace = ConfigurationManager.AppSettings["Factory"];
                            var strFactory = FactorySpace.Split(',');
                            var nocard = true;
                          /*  foreach (var item in strFactory)
                            {
                                if (Factory == item)
                                {
                                    nocard = false;
                                    break;
                                }
                            }
                            */
                            nocard = false;
                            /************* 下班門禁記錄 ************/
                            //有加班
                            var ovttime = ToDateTime(entity.overtime_end_time);
                            attTime = empty;
                            daybeginaccesscheck1 = ToDateTime(entity.carding_actual_end);
                            daybeginaccesscheck2 = ToDateTime(entity.carding_actual_end);
                            if (daybeginaccesscheck1.HasValue)
                            {
                                daybeginaccesscheck1 = daybeginaccesscheck1.Value.AddMinutes(-15);
                                daybeginaccesscheck2 = daybeginaccesscheck2.Value.AddMinutes(15);
                                attTime = employeeRepository.SqlQuery<string>("exec sp_recordTime {0},{1},{2},{3}", new object[] {
                                daybeginaccesscheck1,
                                daybeginaccesscheck2, 
                                cardId, 
                                "AccessCheck" 
                                }).ToList();

                            }
                            if (attTime.Count() == 0 || (attTime.Count() == 1 && string.IsNullOrEmpty(attTime.First())))
                            {
                                if (nocard)
                                {
                                    AnalysisiL.Clock_Type = "1";
                                    AnalysisiL.Abnormal_Flag = "4";
                                    AnalysisiL.Plant_List = AnalysisiL.Plant_List + "未刷下班門禁<br/> ";
                                    AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "未刷下班門禁卡<br/>";
                                }
                            }
                            else
                            {
                                var attime = ToDateTime(attTime.First());
                                var str = "";
                                if (attime.HasValue)
                                {
                                    str = attime.Value.ToString("dd HH:mm");
                                }
                                else
                                {
                                    str = attTime.First();
                                }

                                AnalysisiL.Plant_List = AnalysisiL.Plant_List + str + "下班門禁<br/> ";
                                //AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "下班門禁無異常<br/>";

                            }

                        }
                        switch (entity.absence_type)
                        {
                            case "曠工":
                                //Card_List
                                AnalysisiL.Abnormal_Flag = "1";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "曠工<br/>";
                                break;
                            case "遲到":
                                AnalysisiL.Abnormal_Flag = "2";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "遲到<br/>";
                                break;

                            case "早退":
                                AnalysisiL.Abnormal_Flag = "3";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "早退<br/>";
                                break;
                            case "事假":
                                AnalysisiL.Abnormal_Flag = "8";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "事假<br/>";
                                break;
                            case "年休假":
                                AnalysisiL.Abnormal_Flag = "8";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "年休假<br/>";
                                break;
                            case "國定假":
                                AnalysisiL.Abnormal_Flag = "8";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "國定假<br/>";
                                break;
                            case "產假":
                                AnalysisiL.Abnormal_Flag = "8";
                                AnalysisiL.Abnormal_Desc = AnalysisiL.Abnormal_Desc + "產假<br/>";
                                break;
                            default: break;
                        }
                        if (!anomalous)
                        {
                            handle(AnalysisiL);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(AnalysisiL.Abnormal_Flag) && AnalysisiL.Abnormal_Flag != "0")
                            {
                                handle(AnalysisiL);
                            }
                        }
                    }
                }


            }
        }


        private AbnormalAnalysis createTo(AbnormalAnalysis Analysis)
        {
            var tempAnalysis = new AbnormalAnalysis
            {
                Abnormal_Desc = Analysis.Abnormal_Desc,
                Abnormal_Flag = Analysis.Abnormal_Flag,
                Absence_Apply = Analysis.Absence_Apply,
                Access_Card_List = Analysis.Access_Card_List,
                AutoID = Guid.NewGuid(),
                Card_List = Analysis.Card_List,
                Card_Type = Analysis.Card_Type,
                Carding_Day = Analysis.Carding_Day,
                Carding_List = Analysis.Carding_List,
                Factory = Analysis.Factory,
                Create_By = Analysis.Create_By,
                Emp_Type = Analysis.Emp_Type,
                Emp_No = Analysis.Emp_No,
                Emp_Title = Analysis.Emp_Title,
                Carding_Hours = Analysis.Carding_Hours,
                leave_Hours = Analysis.leave_Hours,
                Overtime_Hours = Analysis.Overtime_Hours,
                Overtime_Apply = Analysis.Overtime_Apply,
                Clock_Type = Analysis.Clock_Type,
                Dept_No = Analysis.Dept_No,
                Dept_Name = Analysis.Dept_Name,
                Emp_Name = Analysis.Emp_Name,
                Create_Time = Analysis.Create_Time = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")),
                Absence_List = Analysis.Absence_List,
                Totalhours_Access = Analysis.Totalhours_Access,
                Totalhours_Plant = Analysis.Totalhours_Plant                

            };
            return tempAnalysis;
        }
        public DateTime? ToDateTime(string val)
        {  //
            if (!string.IsNullOrEmpty(val) && !string.IsNullOrWhiteSpace(val))
            {
                var time = Convert.ToDateTime(val);
                return time;
            }
            else
            {
                return null;
            }
        }

        public bool WhetherLeave(string type)
        {
            string[] abnormal = { "曠工", "遲到", "早退" };
            foreach (var item in abnormal)
            {
                if (item == type)
                {
                    return false;
                }
            }
            return true;
        }

        public AbnormalAnalysis OA(AbnormalAnalysis Analysis, string operate)
        {
            return OA(Analysis, operate, "");
        }

        public AbnormalAnalysis OA(AbnormalAnalysis Analysis, string operate, string cardList)
        {
            return OA(Analysis, operate, cardList, "");
        }

        public AbnormalAnalysis OA(AbnormalAnalysis Analysis, string operate, string cardList, string CardingList)
        {
            var tempAnalysis = new AbnormalAnalysis
            {
                Abnormal_Desc = Analysis.Abnormal_Desc,
                Abnormal_Flag = Analysis.Abnormal_Flag,
                Absence_Apply = Analysis.Absence_Apply,
                Access_Card_List = Analysis.Access_Card_List,
                AutoID = Guid.NewGuid(),
                leave_Hours = Analysis.leave_Hours,
                Carding_Hours = Analysis.Carding_Hours,
                Card_List = Analysis.Card_List,
                Card_Type = Analysis.Card_Type,
                Carding_Day = Analysis.Carding_Day,
                Carding_List = Analysis.Carding_List,
                Factory = Analysis.Factory,
                Create_By = Analysis.Create_By,
                Emp_Type = Analysis.Emp_Type,
                Emp_No = Analysis.Emp_No,
                Emp_Title = Analysis.Emp_Title,
                Overtime_Hours = Analysis.Overtime_Hours,
                Overtime_Apply = Analysis.Overtime_Apply,
                Clock_Type = Analysis.Clock_Type,
                Dept_No = Analysis.Dept_No,
                Dept_Name = Analysis.Dept_Name,
                Emp_Name = Analysis.Emp_Name,
                Create_Time = Analysis.Create_Time = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"))
            };

            switch (operate)
            {
                case "R"://休息日不加班不作稽核
                    tempAnalysis.Abnormal_Flag = "0";
                    tempAnalysis.Clock_Type = tempAnalysis.Clock_Type ?? "";
                    tempAnalysis.Card_Type = tempAnalysis.Card_Type ?? "";
                    tempAnalysis.Abnormal_Desc = "休息/差假";
                    break;
                case "A"://免刷卡不作稽核
                    tempAnalysis.Abnormal_Flag = "0";
                    tempAnalysis.Clock_Type = tempAnalysis.Clock_Type ?? "";
                    tempAnalysis.Card_Type = tempAnalysis.Card_Type ?? "";
                    tempAnalysis.Abnormal_Desc = "免刷卡";
                    break;
            }
            return tempAnalysis;
        }

        
        private void handle(AbnormalAnalysis entity)
        {
            //新增分析記錄
            ModelState.Clear();
            var command = Mapper.Map<AbnormalAnalysis, CreateOrUpdateEntityCommand<AbnormalAnalysis>>(entity);
            IEnumerable<ValidationResult> errors = commandBus.Validate(command);
            ModelState.AddModelErrors(errors);
            if (ModelState.IsValid)
            {
                //0:Insert ,1:Update ,3:Delete

                var result = commandBus.Submit(command, 0);
                if (result.Success)
                {
                    //操作成功
                }
            }
            else
            {
                ModelState.Clear();
            }
        }


        public ActionResult Inprescritvel(int? ResultId)
        {
            int PageSize = 30;
            int PageIndex = ResultId ?? 1;
            System.Data.DataTable dt = employeeRepository.ProcQuery("Inprescritvel", null);
            var src = GetPagedTable(dt, PageIndex, PageSize);
            var Result = src.AsEnumerable().AsEnumerable();
            int count = dt.Rows.Count;
            IEnumerate entityResult = new Enumerate<DataRow>(Result, PageIndex, PageSize, count);
            if (Request.IsAjaxRequest())
            {

                return PartialView("Inprescritvel/_UserList", entityResult);
            }
            return View("Inprescritvel/Index", entityResult);
        }

        private DataTable GetPagedTable(DataTable dt, int PageIndex, int PageSize)
        {
            if (PageIndex == 0) { return dt; }
            DataTable newdt = dt.Copy();
            newdt.Clear();
            int rowbegin = (PageIndex - 1) * PageSize;
            int rowend = PageIndex * PageSize;

            if (rowbegin >= dt.Rows.Count)
            { return newdt; }

            if (rowend > dt.Rows.Count)
            { rowend = dt.Rows.Count; }
            for (int i = rowbegin; i <= rowend - 1; i++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[i];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }
                newdt.Rows.Add(newdr);
            }
            return newdt;
        }

        private IEnumerable<DataRow> getDataRow(EnumerableRowCollection<DataRow> dtrow, int PageIndex, int PageSize)
        {

            var Result = dtrow.Skip(PageIndex - 1).Take(PageSize);



            return Result;

        }
        public ActionResult Departments(string id)
        {

            var department = employeeRepository.GetMany(m => m.Factory == id).OrderBy(e => e.Dept_Name).Select(d => new { name = d.Dept_Name, value = d.Dept_No }).Distinct();
            return Json(department, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Department(string id, string DL, string emptype)
        {
            var department = employeeRepository.SqlQuery<DepartmentFormModel>("exec GetDeptId {0}", new object[] { HttpContext.User.Identity.Name }).Select(d => new { name = d.Dept_Name, value = d.Dept_No }).Distinct();
            if (department.Count() < 1)
            {
                Expression<Func<Employee_Full, bool>> Where;
                if (!string.IsNullOrEmpty(DL))
                {
                    if (emptype != "")
                    {
                        Where = predicate<Employee_Full>(e => e.Factory == id && e.Emp_Type == emptype);
                    }
                    else
                    {
                        Where = predicate<Employee_Full>(e => e.Factory == id && (e.Emp_Type == "JO01" || e.Emp_Type == "JO02"));
                    }
                }
                else
                {
                    Where = predicate<Employee_Full>(e => e.Factory == id);
                }
                var dept = employeeRepository.GetMany(Where).OrderBy(e => e.Dept_Name).Select(d => new { name = d.Dept_Name, value = d.Dept_No }).Distinct();
                return Json(dept, JsonRequestBehavior.AllowGet);
            }
            return Json(department, JsonRequestBehavior.AllowGet);
        }
        private Expression<Func<TSource, bool>> predicate<TSource>(Expression<Func<TSource, bool>> dicate)
        {
            return dicate;
        }

        [HttpPost]
        public ActionResult Create(InprescritvelFromModel frm)
        {

            var d = new SqlParameter("empno", frm.Emp_No);
            if (!string.IsNullOrEmpty(frm.Emp_No))
            {
                int count = employeeRepository.ProcExec("CreateInprescritvel", d);
            }
            return Inprescritvel(1);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var d = new SqlParameter("empno", id);
            if (!string.IsNullOrEmpty(id))
            {
                int count = employeeRepository.ProcExec("delInprescritvel", d);
            }

            return Inprescritvel(1);
        }
    }
}
