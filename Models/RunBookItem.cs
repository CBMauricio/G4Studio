using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace G4Studio.Models
{

    public class RunBookItem
    {
        public enum Status
        {
            INI,
            RUN,
            PAU,
            WAI,
            STP,
            FIN
        }

        private static ResourceLoader resourceLoader;

        public EnvironmentHandler.Type Environment { get; set; }
        public string EnvironmentDesc { get; set; }

        public D2CMessagesConfig.Category D2CMessagesCategory { get; set; }
        public D2CMessagesConfig.Kind D2CMessagesKind { get; set; }
        public string D2CMessagesKindDesc { get; set; }

        public Tenant Project { get; set; }
        public double NMessages { get; set; }
        public double NRuns { get; set; }
        public double NSeconds { get; set; }

        public DateTime DateCreated { get; set; }
        public List<Device> Devices { get; private set; }

        public RunBookItem()
            : this(EnvironmentHandler.Type.DEV, new Tenant(), D2CMessagesConfig.Category.REG, D2CMessagesConfig.Kind.REG, 0, 0, 0, new List<Device>(), DateTime.Now)
        {}

        public RunBookItem(EnvironmentHandler.Type environment, Tenant project, D2CMessagesConfig.Category category, D2CMessagesConfig.Kind kind, double nMessages, double nRuns, double nSeconds, List<Device> devices, DateTime dateCreated)
        {
            resourceLoader = ResourceLoader.GetForCurrentView();

            Environment = environment;
            EnvironmentDesc = GetEnvironmentString(environment);
            Project = project;
            D2CMessagesCategory = category;
            D2CMessagesKind = kind;
            D2CMessagesKindDesc = GetD2CCommandTypeString(kind);
            NMessages = nMessages;
            NRuns = nRuns;
            NSeconds = nSeconds;

            Devices = devices is null ? new List<Device>() : devices;
            DateCreated = dateCreated;
        }

        private static string GetEnvironmentString(EnvironmentHandler.Type environment)
        {
            var environmentString = string.Empty;

            switch (environment)
            {
                case EnvironmentHandler.Type.DEV:
                    environmentString = resourceLoader.GetString("CONFS_ENV_DEV_Desc");
                    break;
                case EnvironmentHandler.Type.TST:
                    environmentString = resourceLoader.GetString("CONFS_ENV_TST_Desc");
                    break;
                default:
                    environmentString = resourceLoader.GetString("CONFS_ENV_PRD_Desc");
                    break;
            }

            return environmentString;
        }

        private static string GetD2CCommandTypeString(D2CMessagesConfig.Kind commandType)
        {
            var commandKindString = string.Empty;

            switch (commandType)
            {
                case D2CMessagesConfig.Kind.REG:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_REG_Desc");
                    break;
                case D2CMessagesConfig.Kind.TEL:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_TEL_Desc");
                    break;
                case D2CMessagesConfig.Kind.ALR:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_ALR_Desc");
                    break;
                case D2CMessagesConfig.Kind.BREG:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_BREG_Desc");
                    break;
                case D2CMessagesConfig.Kind.BTEL:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_BTEL_Desc");
                    break;
                default:
                    commandKindString = resourceLoader.GetString("CONFS_C2D_BALR_Desc");
                    break;
            }

            return commandKindString;
        }
    }
}
