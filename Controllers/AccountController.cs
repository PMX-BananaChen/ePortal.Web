using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ePortal.Web.ViewModels;
using ePortal.Domain.Commands;
using ePortal.Web.Core.Models;
using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data.Repositories;
using ePortal.Core.Common;
using ePortal.Web.Core.Extensions;
using ePortal.Web.Core.Authentication;
using ePortal.Models;
using ePortal.Web.Core.ActionFilters;
using AutoMapper;
using ePortal.Web.Models;
using System.Data.SqlClient;
namespace ePortal.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<SystemUser> userRepository;
        private readonly IEntityRepository<SecurityGroup> securityGroupRepository;
        private readonly IEntityRepository<Employee_Full> employeeRepository;
        private readonly IEntityRepository<SystemUserSecurity> userSecurityRepository;
        private readonly IFormsAuthentication formAuthentication;

        public AccountController(ICommandBus commandBus, IEntityRepository<SystemUser> userRepository, IEntityRepository<SecurityGroup> securityGroupRepository, IEntityRepository<SystemUserSecurity> userSecurityRepository, IEntityRepository<Employee_Full> employeeRepository, IFormsAuthentication formAuthentication)
        {
            this.commandBus = commandBus;
            this.securityGroupRepository = securityGroupRepository;
            this.employeeRepository = employeeRepository;
            this.userSecurityRepository = userSecurityRepository;
            this.userRepository = userRepository;
            this.formAuthentication = formAuthentication;
        }

        //
        // GET: /Account/LogOff
        /// <summary>
        ///  退出後重新定向到登錄頁面  ReturnUrl記錄退出時所在頁面的URL
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            formAuthentication.Signout();
            return RedirectToAction("Login", "Account", new { ReturnUrl = Request.QueryString["ReturnUrl"] });
        }
        [Authorizer]
        public ActionResult Index()
        {
            var userlist = userRepository.GetAll();
            return View("User/Index", userlist);
        }


        [Authorizer]
        public ActionResult Security()
        {
            var userlist = userSecurityRepository.GetAll();
            var userSecuritiesModel = new UserSecuritiesModel();
            userSecuritiesModel.SecurityGroup = securityGroupRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SecurityGroupID });
            userSecuritiesModel.SystemUser = userRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SystemUserID });
            ViewBag.UserSecuritiesModel = userSecuritiesModel;

            return View("Security/Index", userlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUserSecurity(UserSecuritiesModel form)
        {
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<UserSecuritiesModel, CreateOrUpdateEntityCommand<SystemUserSecurity>>(form);
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    if (form.AutoID == System.Guid.Empty)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        command.Entity.CompanyID = "primax";
                        form.AutoID = command.Entity.AutoID;
                        //0為新增菜單
                        var result = commandBus.Submit(command, 0);
                        if (result.Success) { }//return RedirectToAction("Index");
                    }
                    else
                    {
                        //1為修改菜單
                        var result = commandBus.Submit(command, 1);
                        if (result.Success) { } //return RedirectToAction("Index");
                    }
                }
            }
            return SecurityList();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveGroupSecurity(GroupFormModel form)
        {
            if (ModelState.IsValid)
            {
                form.DeptId = Request.Form["DeptId"];
                var command = Mapper.Map<GroupFormModel, CreateOrUpdateEntityCommand<SecurityGroup>>(form);

                if (form.AutoID == System.Guid.Empty)
                {
                    IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                    ModelState.AddModelErrors(errors);
                    if (ModelState.IsValid)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        command.Entity.CompanyID = "primax";
                        form.AutoID = command.Entity.AutoID;
                        //0為新增組
                        var result = commandBus.Submit(command, 0);
                        if (result.Success) { }//return RedirectToAction("Index");
                    }
                }
                else
                {
                    //1為修改組
                    var result = commandBus.Submit(command, 1);
                    if (result.Success) { } //return RedirectToAction("Index");
                }
            }

            return GroupList();
        }

        [Authorizer]
        public ActionResult SecurityList()
        {
            var userlist = userSecurityRepository.GetAll();
            var userSecuritiesModel = new UserSecuritiesModel();
            userSecuritiesModel.SecurityGroup = securityGroupRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SecurityGroupID });
            userSecuritiesModel.SystemUser = userRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SystemUserID });
            ViewBag.UserSecuritiesModel = userSecuritiesModel;
            return PartialView("Security/_SecurityList", userlist);
        }
        [Authorizer]
        public ActionResult GroupList()
        {
            var securityGroup = securityGroupRepository.GetAll().AsEnumerable();
            ViewData["Factory"] = employeeRepository.ProcQuery<Factorys>("GetFactory", new SqlParameter[] { }).Select(l => new SelectListItem { Text = l.Factory, Value = l.Factory });
            return PartialView("Group/_GroupList", securityGroup);
        }
        public ActionResult Group()
        {
            var securityGroup = securityGroupRepository.GetAll().AsEnumerable();
            var factory = employeeRepository.ProcQuery<Factorys>("GetFactory", new SqlParameter[] { }).Select(l => new SelectListItem { Text = l.Factory, Value = l.Factory });
            ViewData["Factory"] = factory;
            var department = employeeRepository.ProcQuery<DepartmentFormModel>("GetDept", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=factory.FirstOrDefault().Value,ParameterName="Factory"}
                }).Select(d => new SelectListItem { Text = d.Dept_Name, Value = d.Dept_No });
            ViewData["department"] = department;
            return View("Group/Index", securityGroup);

        }

        public ActionResult Department(string factory)
        {
            factory = factory ?? "";
            var department = employeeRepository.ProcQuery<DepartmentFormModel>("GetDept", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=factory,ParameterName="Factory"}
                }).Select(d => new { name = d.Dept_Name, value = d.Dept_No });
            return Json(department, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 編輯權限組
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult GroupEdit(string Id)
        {
            Guid Gid;
            if (string.IsNullOrEmpty(Id))
            {
                Gid = Guid.NewGuid();
            }
            else { Gid = new Guid(Id); }
            var Group = securityGroupRepository.Get(m => m.AutoID == Gid);
            if (Group == null) { Group = new SecurityGroup(); }
            var GroupModel = Mapper.Map<SecurityGroup, GroupFormModel>(Group);
            var factory = employeeRepository.ProcQuery<Factorys>("GetFactory", new SqlParameter[] { }).Select(l => new SelectListItem { Text = l.Factory, Value = l.Factory }).ToList();
            ViewData["Factory"] = factory;
            var department = employeeRepository.ProcQuery<DepartmentFormModel>("GetDept", new SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter{DbType=System.Data.DbType.String,Value=factory.FirstOrDefault().Value,ParameterName="Factory"}
                }).Select(l => new SelectListItem { Text = l.Dept_Name, Value = l.Dept_No }).ToList();
            ViewData["department"] = department;
            return PartialView("Group/Edit", GroupModel);

        }


        public ActionResult SecurityEdit(Guid Id)
        {
            var UserSecurity = userSecurityRepository.Get(m => m.AutoID == Id);
            var UserSecurityModel = Mapper.Map<SystemUserSecurity, UserSecuritiesModel>(UserSecurity);
            UserSecurityModel.SecurityGroup = securityGroupRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SecurityGroupID });
            UserSecurityModel.SystemUser = userRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.SystemUserID });
            return PartialView("Security/Edit", UserSecurityModel);

        }

        public ActionResult UserList()
        {
            var userlist = userRepository.GetAll();
            return PartialView("User/_UserList", userlist);

        }
        private bool ValidatePassword(SystemUser user, string password)
        {
            var encoded = Md5Encrypt.Md5EncryptPassword(password);
            return user.Password.Equals(encoded);
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return ContextDependentView();
        }
        [HttpPost]
        public ActionResult Login(LogOnFormModel form, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                SystemUser user = userRepository.Get(u => (u.EmailAddress == form.UserName || u.Name == form.UserName || u.SystemUserID == form.UserName));
                if (user != null)
                {
                    if (ValidatePassword(user, form.Password))
                    {
                        formAuthentication.SetAuthCookie(this.HttpContext,UserAuthenticationTicketBuilder.CreateAuthenticationTicket(user));
                        if (!Url.IsLocalUrl(ReturnUrl))
                        {
                            ReturnUrl = "/Home/Index";
                            // return RedirectToAction("Index", "Home");return Redirect(ReturnUrl);
                        }
                        else
                        {
                            ReturnUrl = "/Home/Index";

                        }
                        LogHelper.WriteOperateLog("登錄", "用戶管理", "用戶登錄", user.Name);
                        return Json(new { success = true, redirect = ReturnUrl });
                    }
                    else
                    {
                        ModelState.AddModelError("錯誤", "用戶密碼不正確");
                    }
                }
                else
                {
                    ModelState.AddModelError("錯誤", "用戶: " + form.UserName + " 不存在!");
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        [HttpPost]
        public JsonResult JsonLogin(LogOnFormModel form, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                SystemUser user = userRepository.Get(u => u.EmailAddress == form.UserName && u.Active == true);
                if (user != null)
                {
                    if (ValidatePassword(user, form.Password))
                    {
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                                 UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                                     user));

                        return Json(new { success = true, redirect = returnUrl });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        //
        // POST: /Account/JsonRegister

        [AllowAnonymous]
        [HttpPost]
        public ActionResult JsonRegister(UserFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = new CreateOrUpdateEntityCommand<SystemUser>
                {
                    Entity = new SystemUser
                    {
                        Active = true,
                        Name = form.Name,
                        EmailAddress = form.Email,
                         //加密密碼
                    Password = Md5Encrypt.Md5EncryptPassword(form.Password),
                       
                        SystemManager = false //UserRoles.User
                    }
                };
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    var result = commandBus.Submit(command, 0);
                    if (result.Success)
                    {
                        SystemUser user = userRepository.Get(u => u.EmailAddress == form.Email);
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                          UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                              user));
                        return Json(new { success = true });
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred.");
                    }
                }
                // If we got this far, something failed
                return Json(new { errors = GetErrorsFromModelState() });
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }
        public JsonResult ValidName(string Name)
        {
            SystemUser user = userRepository.Get(u => u.Name == Name);
            bool result = true;
            if (user != null)
            {
                result = false;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckEmail(string Email)
        {
            SystemUser user = userRepository.Get(u => u.EmailAddress.ToLower() == Email.ToLower());
            bool result = true;
            if (user != null)
            {
                result = false;

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(UserFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<UserFormModel, CreateOrUpdateEntityCommand<SystemUser>>(form);
                //加密密碼
                command.Entity.Password = Md5Encrypt.Md5EncryptPassword(command.Entity.Password);
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    command.Entity.AutoID = Guid.NewGuid();
                    var result = commandBus.Submit(command, 0);
                   
                    if (result.Success)
                    {
                        //LogHelper.WriteOperateLog("註冊", "用戶管理", "註冊用戶", HttpContext.User.Identity.Name);
                        SystemUser user = userRepository.Get(u => u.EmailAddress == form.Email);
                        formAuthentication.SetAuthCookie(this.HttpContext,
                                                          UserAuthenticationTicketBuilder.CreateAuthenticationTicket(
                                                              user));

                    }
                    else
                    {
                        ModelState.AddModelError("", "An unknown error occurred.");
                    }
                }
                // If we got this far, something failed, redisplay form

            }

            // If we got this far, something failed
            return UserList();
            //Json(new { errors = GetErrorsFromModelState() });
        }
        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
        private ActionResult ContextDependentView()
        {
            string actionName = ControllerContext.RouteData.GetRequiredString("action");
            if (Request.QueryString["content"] != null)
            {
                ViewBag.FormAction = "Json" + actionName;

                return PartialView();
            }
            else
            {
                ViewBag.FormAction = actionName;
                return View();
            }
        }
        public ActionResult ChangePassword()
        {
            return PartialView("ChangePassword");
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordFormModel form)
        {
            if (ModelState.IsValid)
            {
                var UserId = HttpContext.User.Identity.Name;
                var command = new ChangePasswordCommand
                {
                    SystemUserID = UserId,
                    OldPassword = form.OldPassword,
                    NewPassword = form.NewPassword
                };
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                command.NewPassword = Md5Encrypt.Md5EncryptPassword(command.NewPassword);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    var result = commandBus.Submit(command, 1);
                    if (result.Success)
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            return View(form);
        }
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
    }
}
