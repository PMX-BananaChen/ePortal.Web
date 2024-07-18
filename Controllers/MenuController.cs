using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ePortal.Domain.Commands;
using ePortal.Core.Common;
using ePortal.Web.Core.Extensions;
using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data.Repositories;
using ePortal.Web.Core.ActionFilters;
using AutoMapper;
using ePortal.Models;
using ePortal.Web.Models;
using ePortal.Web.Helpers;
using ePortal.Web.Core.Models;
using ePortal.Web.ViewModels;
using System;
namespace ePortal.Web.Controllers
{
    [Authorizer]
    [CompressResponse]
    public class MenuController : Controller
    {
        //
        // GET: /Default1/
        private readonly ICommandBus commandBus;
        private readonly IEntityRepository<MenuSecurity> menuSecurityRepository;
        private readonly IEntityRepository<MenuItem> menuItemRepository;
        private readonly IEntityRepository<SecurityGroup> securityGroupRepository;
        private readonly IEntityRepository<ExecutableProgram> programRepository;
        public MenuController(ICommandBus commandBus, IEntityRepository<MenuItem> menuItemRepository, IEntityRepository<MenuSecurity> menuSecurityRepository, IEntityRepository<ExecutableProgram> programRepository, IEntityRepository<SecurityGroup> securityGroupRepository)
        {
            this.securityGroupRepository = securityGroupRepository;
            this.commandBus = commandBus;
            this.menuSecurityRepository = menuSecurityRepository;
            this.menuItemRepository = menuItemRepository;
            this.programRepository = programRepository;
        }
        [OutputCache(CacheProfile = "Menu/Index")]
        public ActionResult Index()
        {
            var menus = menuItemRepository.GetAll();
            ViewBag.ExecutablePrograms = programRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.ExecutableProgramID });
            return View(menus);
        }

        #region ExecutablePrograms

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveExecutableProgram(ExecutableProgramsFormModel form)
        {
            if (ModelState.IsValid)
            {

                var command = Mapper.Map<ExecutableProgramsFormModel, CreateOrUpdateEntityCommand<ExecutableProgram>>(form);
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {


                    if (form.AutoID == Guid.Empty)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        form.AutoID = command.Entity.AutoID;
                        command.Entity.CompanyID = "primax";
                        command.Entity.ExecutableProgramTypeID = "";
                        command.Entity.Notes = command.Entity.Notes ?? "";
                        command.Entity.ExecutableProgramCodeID = "";
                        var result = commandBus.Submit(command, 0);
                        if (result.Success)
                        {

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
            }
            return ProgramList();
        }
        [OutputCache(CacheProfile = "Menu/ProgramList")]
        public ActionResult ProgramList()
        {
            var programs = programRepository.GetAll();
            return PartialView("Program/_ProgramList", programs);
        }
        [OutputCache(CacheProfile = "ExecutableProgram")]
        public ActionResult ExecutableProgram()
        {
            var programs = programRepository.GetAll();
            return View("Program/Index", programs);

        }

        [HttpPost]
        [HandleErrors]
        public ActionResult DeleteProgram(Guid Id)
        {
            var command = new CreateOrUpdateEntityCommand<ExecutableProgram> { id = Id };
            var result = commandBus.Submit(command, 3);
            var categories = programRepository.GetAll();
            if (ModelState.IsValid)
            {

            }
            return PartialView("_ProgramList", categories);
        }
        #endregion


        [OutputCache(CacheProfile = "Menu/MenuSecurity")]
        public ActionResult MenuSecurity()
        {
            var menus = menuSecurityRepository.SqlQuery<MenuSecurityFormModel>("select * from v_MenuSecurities", HttpContext.User.Identity.Name);
            ViewBag.MenuItems = menuItemRepository.GetAll().Select(a => new SelectListItem { Text = a.Description, Value = a.MenuItemID });
            ViewBag.SecurityGroups = securityGroupRepository.GetAll().Select(a => new SelectListItem { Text = a.Description, Value = a.SecurityGroupID });
            return View("Security/Index", menus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HandleErrors]
        public ActionResult SaveMenu(MenuItemFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<MenuItemFormModel, CreateOrUpdateEntityCommand<MenuItem>>(form);
                IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                ModelState.AddModelErrors(errors);
                if (ModelState.IsValid)
                {
                    if (form.AutoID == System.Guid.Empty)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        command.Entity.MenuItemCodeID = "";
                        command.Entity.ImageID = command.Entity.ImageID ?? "";
                        command.Entity.CompanyID = "primax";
                        command.Entity.MenuItemTypeID = "";
                        form.AutoID = command.Entity.AutoID;
                        var menumaxid = menuItemRepository.SqlQuery<string>("exec sp_menumaxid {0}", command.Entity.ParentMenuID ?? "").First() ?? "0";
                        command.Entity.ParentMenuID = command.Entity.ParentMenuID ?? "";
                        command.Entity.MenuItemID = command.Entity.ParentMenuID + menumaxid;
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
            return MenuList();
        }


        public ActionResult EditMenuSecurity(Guid Id)
        {
            var menu = menuSecurityRepository.Get(m => m.AutoID == Id);
            ViewBag.MenuItems = menuItemRepository.GetAll().Select(a => new SelectListItem { Text = a.Description, Value = a.MenuItemID });
            ViewBag.SecurityGroups = securityGroupRepository.GetAll().Select(a => new SelectListItem { Text = a.Description, Value = a.SecurityGroupID });
            var MenuModel = Mapper.Map<MenuSecurity, MenuSecurityFormModel>(menu);

            return PartialView("Security/Edit", MenuModel);
        }

        public ActionResult MenuEdit(Guid Id)
        {
            var menus = menuItemRepository.GetAll();
            var menu = menuItemRepository.Get(m => m.AutoID == Id);
            ViewBag.ExecutablePrograms = programRepository.GetAll().Select(a => new SelectListItem { Text = a.Name, Value = a.ExecutableProgramID });
            var MenuModel = Mapper.Map<MenuItem, MenuItemFormModel>(menu);
            MenuModel.SetSelectList(menus);
            return PartialView("Edit", MenuModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HandleErrors]
        public ActionResult SaveMenuSecurity(MenuSecurityFormModel form)
        {
            if (ModelState.IsValid)
            {
                var command = Mapper.Map<MenuSecurityFormModel, CreateOrUpdateEntityCommand<MenuSecurity>>(form);

                if (form.AutoID == System.Guid.Empty)
                {
                    IEnumerable<ValidationResult> errors = commandBus.Validate(command);
                    ModelState.AddModelErrors(errors);
                    if (ModelState.IsValid)
                    {
                        command.Entity.AutoID = Guid.NewGuid();
                        command.Entity.CompanyID = "primax";
                        form.AutoID = command.Entity.AutoID;
                        //0為新增頁面權限
                        var result = commandBus.Submit(command, 0);
                        if (result.Success) { }//return RedirectToAction("Index");
                    }
                }
                else
                {
                    //1為修改頁面權限
                    var result = commandBus.Submit(command, 1);
                    if (result.Success) { } //return RedirectToAction("Index");
                }

            }
            return MenuSecurityList();
        }

        [OutputCache(CacheProfile = "Menu/MenuSecurityList")]
        public ActionResult MenuSecurityList()
        {

            var menus = menuItemRepository.SqlQuery<MenuSecurityFormModel>("select * from v_MenuSecurities", HttpContext.User.Identity.Name);
            return PartialView("Security/_MenuSecurityList", menus);
        }
        [HttpPost]
        [HandleErrors]
        public ActionResult DeleteMenu(Guid Id)
        {
            var command = new CreateOrUpdateEntityCommand<MenuItem> { id = Id };
            var result = commandBus.Submit(command, 3);
            return Json(new { success = true, msc = "刪除成功!" });

        }
        //獲取菜單列表
        [OutputCache(CacheProfile = "Menu/MenuList")]
        public ActionResult MenuList()
        {
            var menus = menuItemRepository.GetAll();
            return PartialView("_MenuList", menus);
        }

        public ActionResult MenuItem()
        {
            var menus = menuItemRepository.SqlQuery<NestedMenuItem>("exec [sp_MenuItems] {0}", HttpContext.User.Identity.Name);

            return PartialView("_MenuItem", menus.ToMenuItem());
        }

    }
}
