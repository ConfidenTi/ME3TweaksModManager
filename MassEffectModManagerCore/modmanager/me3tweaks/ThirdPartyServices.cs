﻿using MassEffectModManager.modmanager;
using System;
using System.Collections.Generic;
using System.Text;

namespace MassEffectModManagerCore.modmanager.me3tweaks
{
    public class ThirdPartyServices
    {
        /// <summary>
        /// Looks up importing information for mods through the third party mod importing service. This returns all candidates, the client code must determine which is the appropriate value.
        /// </summary>
        /// <param name="archiveSize">Size of archive being checked for information</param>
        /// <returns>List of candidates</returns>
        public static List<ThirdPartyImportingInfo> GetImportingInfosBySize(long archiveSize)
        {
            if (App.ThirdPartyImportingService == null) return new List<ThirdPartyImportingInfo>(); //Not loaded
            if (App.ThirdPartyImportingService.TryGetValue(archiveSize, out var result))
            {
                return result;
            }

            return new List<ThirdPartyImportingInfo>();
        }

        /// <summary>
        /// Looks up information about a DLC mod through the third party identification service
        /// </summary>
        /// <param name="dlcName"></param>
        /// <param name="game">Game to look in database for</param>
        /// <returns>Third party mod info about dlc folder, null if not found</returns>
        public static ThirdPartyModInfo GetThirdPartyModInfo(string dlcName, Mod.MEGame game)
        {
            if (App.ThirdPartyIdentificationService == null) return null; //Not loaded
            if (App.ThirdPartyIdentificationService.TryGetValue(game.ToString(), out var infosForGame))
            {
                if (infosForGame.TryGetValue(dlcName, out var info))
                {
                    return info;
                }
            }

            return null;
        }


        public class ThirdPartyImportingInfo
        {
            public string md5 { get; set; }
            public string inarchivepathtosearch { get; set; }
            public string filename { get; set; }
            public string subdirectorydepth { get; set; }
            public string servermoddescname { get; set; }
            public Mod.MEGame game { get; set; }
            public string version { get; set; }
        }


        public class ThirdPartyModInfo
        {
            public string modname { get; set; }
            public string moddev { get; set; }
            public string modsite { get; set; }
            public string moddesc { get; set; }
            public string mountpriority { get; set; }
            public string preventimport { get; set; }
            public string updatecode { get; set; } //has to be string I guess
        }

    }
}