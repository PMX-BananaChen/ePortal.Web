using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ePortal.Models;
using ePortal.Web.ViewModels;

namespace ePortal.Web.Helpers
{
    public static class ToMenuItemHelper
    {
        public static NestedMenuItem ToMenuItem(
              this IEnumerable<NestedMenuItem> menus)
        {
            NestedMenuItem TreeNestedMenuItemList = new NestedMenuItem { MenuItemID=""};
            TreeNestedMenuItemList.ExecutablePath = "#";
            getNestedChidlren(TreeNestedMenuItemList, menus);
            return TreeNestedMenuItemList;
        }

        private static void getNestedChidlren(NestedMenuItem pnmi, IEnumerable<NestedMenuItem> flatNestedMenuItemList)
        {
            var childItem = new List<NestedMenuItem>();
            foreach (NestedMenuItem cnmi in flatNestedMenuItemList.Where(nmi => nmi.ParentMenuID == pnmi.MenuItemID).ToList())
            {
                childItem.Add(cnmi);

                getNestedChidlren(cnmi, flatNestedMenuItemList);
            }
            if (childItem.Count > 0)
            {

                pnmi.Children = childItem;
            }
        }
    }
}