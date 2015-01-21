﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Runtime.Session;

namespace Abp.Web.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthorizationScriptManager : IAuthorizationScriptManager, ISingletonDependency
    {
        /// <inheritdoc/>
        public IAbpSession AbpSession { get; set; }

        private readonly IPermissionManager _permissionManager;

        public IPermissionChecker PermissionChecker { get; set; }

        /// <inheritdoc/>
        public AuthorizationScriptManager(IPermissionManager permissionManager)
        {
            AbpSession = NullAbpSession.Instance;
            PermissionChecker = NullPermissionChecker.Instance;

            _permissionManager = permissionManager;
        }

        /// <inheritdoc/>
        public string GetScript()
        {
            var allPermissionNames = _permissionManager.GetAllPermissions().Select(p => p.Name).ToList();
            var grantedPermissionNames =
                AbpSession.UserId.HasValue
                    ? allPermissionNames.Where(permissionName => PermissionChecker.IsGranted(AbpSession.UserId.Value, permissionName)).ToArray()
                    : new string[0];

            var script = new StringBuilder();

            script.AppendLine("(function(){");

            script.AppendLine();

            script.AppendLine("    abp.auth = abp.auth || {};");

            script.AppendLine();

            AppendPermissionList(script, "allPermissions", allPermissionNames);

            script.AppendLine();

            AppendPermissionList(script, "grantedPermissions", grantedPermissionNames);

            script.AppendLine();
            script.Append("})();");

            return script.ToString();
        }

        private static void AppendPermissionList(StringBuilder script, string name, IReadOnlyList<string> permissions)
        {
            script.AppendLine("    abp.auth." + name + " = {");

            for (var i = 0; i < permissions.Count; i++)
            {
                var permission = permissions[i];
                if (i < permissions.Count - 1)
                {
                    script.AppendLine("        '" + permission + "': true,");
                }
                else
                {
                    script.AppendLine("        '" + permission + "': true");
                }
            }

            script.AppendLine("    };");
        }
    }
}
