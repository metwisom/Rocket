﻿using System;
using System.IO;
using Rocket.API.Configuration;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.Core.Configuration;
using Rocket.Core.Configuration.Xml;
using Rocket.Core.Logging;
using Rocket.Core.Migration.LegacyPermissions;
using Rocket.Core.Permissions;

namespace Rocket.Core.Migration
{
    public class PermissionsMigration : IMigrationStep
    {
        public void Migrate(IDependencyContainer container, string basePath)
        {
            var permissions = container.Resolve<IPermissionProvider>("default_permissions");
            var logger = container.Resolve<ILogger>();
            var xmlConfiguration = (XmlConfiguration) container.Resolve<IConfiguration>("xml");

            var context = new ConfigurationContext(basePath, "Permissions.config");
            if (!xmlConfiguration.Exists(context))
            {
                logger.LogError("Permissions migration failed: Permissions.config.xml was not found in: " + basePath);
                return;
            }

            xmlConfiguration.Load(context);

            RocketPermissions legacyPermissions = xmlConfiguration.Get<RocketPermissions>();
            foreach (var group in legacyPermissions.Groups)
            {
                PermissionGroup newGroup = new PermissionGroup
                {
                    Name = @group.DisplayName,
                    Id = @group.Id,
                    Priority = 0
                };

                if (!permissions.CreateGroup(newGroup))
                {
                    logger.LogWarning($"Failed to migrate group: {@group.DisplayName} (Id: {group.Id})");
                    continue;
                }

                foreach (var permission in group.Permissions)
                {
                    permissions.AddPermission(newGroup, permission.Name);
                }
            }

            // restore parent groups
            foreach (var group in legacyPermissions.Groups)
            {
                if (string.IsNullOrEmpty(@group.ParentGroup))
                    continue;

                var sourceGroup = permissions.GetGroup(group.Id);
                if (sourceGroup == null)
                    continue;

                var targetGroup = permissions.GetGroup(group.ParentGroup);
                if(targetGroup == null)
                    continue;

                permissions.AddGroup(sourceGroup, targetGroup);
            }
        }
    }
}