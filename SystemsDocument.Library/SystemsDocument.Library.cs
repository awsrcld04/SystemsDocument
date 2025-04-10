// Copyright (C) 2025 Akil Woolfolk Sr. 
// All Rights Reserved
// All the changes released under the MIT license as the original code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemsDocument.Library
{
    public class DataConstruct
    {
        // structs

        public struct sServer
        {
            public string sVersion;
            public string strCustID;
            public string strScanID;
            public string strServerHash;
            public string strConfigScanDateTime;
            public string strTimeZone;
            public Dictionary<string, string> ComputerSystem;
            public Dictionary<string, string>[] Processors;
            public Dictionary<string, string> OperatingSystem;
            //public Dictionary<string, string> PageFileSetting;
            public Dictionary<string, string>[] Disks;
            public sDriveRoot[] DriveRoots;
            public sProgramFilesRoot[] ProgramFilesRoots;
            public Dictionary<string, string> ComputerSystemProduct;
            public Dictionary<string, string> BIOS;
            public Dictionary<string, string>[] NetworkAdapters;
            public Dictionary<string, string>[] NetworkAdapterConfigurations;
            public Dictionary<string, string>[] EventLogFiles;
            public Dictionary<string, string>[] Shares;
            public List<string> SharePermissions;
            public string PrintSpoolerLocation;
            public Dictionary<string, string>[] Printers;
            public List<string> Products;
            public Dictionary<string, string>[] SystemDataSources;
            public Dictionary<string, string>[] ServerFeatures;
            public Dictionary<string, string>[] Services;
            public Dictionary<string, string>[] UserAccounts;
            public Dictionary<string, string>[] Groups;
            public Dictionary<string, string>[] Accounts;
            public Dictionary<string, string>[] Volumes;
        }

        public struct sDriveRoot
        {
            public string DriveLetter;
            public List<string> DriveRootFolders;
        }

        public struct sProgramFilesRoot
        {
            public string ProgramFilesPath;
            public List<string> ProgramFilesFolders;
        }

        // end structs
    }
}
