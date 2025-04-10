// Copyright (C) 2025 Akil Woolfolk Sr. 
// All Rights Reserved
// All the changes released under the MIT license as the original code.

using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using SystemsDocument.Library;
using SystemsDocument.Utility;

namespace SystemsDocument
{
    class SDMain
    {
        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int NetShareGetInfo(
            [MarshalAs(UnmanagedType.LPWStr)] string serverName,
            [MarshalAs(UnmanagedType.LPWStr)] string netName,
            Int32 level,
            out IntPtr bufPtr);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetSecurityDescriptorDacl(
            IntPtr pSecurityDescriptor,
            [MarshalAs(UnmanagedType.Bool)] out bool bDaclPresent,
            ref IntPtr pDacl,
            [MarshalAs(UnmanagedType.Bool)] out bool bDaclDefaulted
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetAclInformation(
            IntPtr pAcl,
            ref ACL_SIZE_INFORMATION pAclInformation,
            uint nAclInformationLength,
            ACL_INFORMATION_CLASS dwAclInformationClass
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetAce(
            IntPtr aclPtr,
            int aceIndex,
            out IntPtr acePtr
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetLengthSid(
            IntPtr pSID
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ConvertSidToStringSid(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSID,
            out IntPtr ptrSid
        );

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int NetApiBufferFree(
            IntPtr buffer
        );

        enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupAccountSid(
           string lpSystemName,
           [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
           System.Text.StringBuilder lpName,
           ref uint cchName,
           System.Text.StringBuilder ReferencedDomainName,
           ref uint cchReferencedDomainName,
           out SID_NAME_USE peUse);

        [StructLayout(LayoutKind.Sequential)]
        struct SHARE_INFO_502
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_netname;
            public uint shi502_type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_remark;
            public Int32 shi502_permissions;
            public Int32 shi502_max_uses;
            public Int32 shi502_current_uses;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_path;
            public IntPtr shi502_passwd;
            public Int32 shi502_reserved;
            public IntPtr shi502_security_descriptor;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ACL_SIZE_INFORMATION
        {
            public uint AceCount;
            public uint AclBytesInUse;
            public uint AclBytesFree;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ACE_HEADER
        {
            public byte AceType;
            public byte AceFlags;
            public short AceSize;
        }

        [StructLayout(LayoutKind.Sequential)]

        struct ACCESS_ALLOWED_ACE
        {
            public ACE_HEADER Header;
            public int Mask;
            public int SidStart;
        }

        enum ACL_INFORMATION_CLASS
        {
            AclRevisionInformation = 1,
            AclSizeInformation
        }

        enum RegWow64Options
        {
            None = 0,
            KEY_WOW64_64KEY = 0x0100,
            KEY_WOW64_32KEY = 0x0200
        }

        enum RegistryRights
        {
            ReadKey = 131097,
            WriteKey = 131078
        }

        /// <summary>
        /// Open a registry key using the Wow64 node instead of the default 32-bit node.
        /// </summary>
        /// <param name="parentKey">Parent key to the key to be opened.</param>
        /// <param name="subKeyName">Name of the key to be opened</param>
        /// <param name="writable">Whether or not this key is writable</param>
        /// <param name="options">32-bit node or 64-bit node</param>
        /// <returns></returns>
        static RegistryKey _openSubKey(RegistryKey parentKey, string subKeyName, bool writable, RegWow64Options options)
        {
            //Sanity check
            if (parentKey == null || _getRegistryKeyHandle(parentKey) == IntPtr.Zero)
            {
                return null;
            }

            //Set rights
            int rights = (int)RegistryRights.ReadKey;
            if (writable)
                rights = (int)RegistryRights.WriteKey;

            //Call the native function >.<
            int subKeyHandle, result = RegOpenKeyEx(_getRegistryKeyHandle(parentKey), subKeyName, 0, rights | (int)options, out subKeyHandle);

            //If we errored, throw an exception
            if (result != 0)
            {
                throw new Exception("Exception encountered opening registry key.", new System.ComponentModel.Win32Exception(result));
            }

            //Get the key represented by the pointer returned by RegOpenKeyEx
            RegistryKey subKey = _pointerToRegistryKey((IntPtr)subKeyHandle, writable, true);
            return subKey;
        }

        /// <summary>
        /// Get a pointer to a registry key.
        /// </summary>
        /// <param name="registryKey">Registry key to obtain the pointer of.</param>
        /// <returns>Pointer to the given registry key.</returns>
        static IntPtr _getRegistryKeyHandle(RegistryKey registryKey)
        {
            //Get the type of the RegistryKey
            Type registryKeyType = typeof(RegistryKey);

            //Get the FieldInfo of the 'hkey' member of RegistryKey
            System.Reflection.FieldInfo fieldInfo =
                registryKeyType.GetField("hkey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Get the handle held by hkey
            SafeHandle handle = (SafeHandle)fieldInfo.GetValue(registryKey);

            //Get the unsafe handle
            IntPtr dangerousHandle = handle.DangerousGetHandle();
            return dangerousHandle;
        }

        /// <summary>
        /// Get a registry key from a pointer.
        /// </summary>
        /// <param name="hKey">Pointer to the registry key</param>
        /// <param name="writable">Whether or not the key is writable.</param>
        /// <param name="ownsHandle">Whether or not we own the handle.</param>
        /// <returns>Registry key pointed to by the given pointer.</returns>
        static RegistryKey _pointerToRegistryKey(IntPtr hKey, bool writable, bool ownsHandle)
        {
            //Get the BindingFlags for private contructors
            System.Reflection.BindingFlags privateConstructors = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

            //Get the Type for the SafeRegistryHandle
            Type safeRegistryHandleType =
                typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");

            //Get the array of types matching the args of the ctor we want
            Type[] safeRegistryHandleCtorTypes = new Type[] { typeof(IntPtr), typeof(bool) };

            //Get the constructorinfo for our object
            System.Reflection.ConstructorInfo safeRegistryHandleCtorInfo = safeRegistryHandleType.GetConstructor(
                privateConstructors, null, safeRegistryHandleCtorTypes, null);

            //Invoke the constructor, getting us a SafeRegistryHandle
            Object safeHandle = safeRegistryHandleCtorInfo.Invoke(new Object[] { hKey, ownsHandle });

            //Get the type of a RegistryKey
            Type registryKeyType = typeof(RegistryKey);

            //Get the array of types matching the args of the ctor we want
            Type[] registryKeyConstructorTypes = new Type[] { safeRegistryHandleType, typeof(bool) };

            //Get the constructorinfo for our object
            System.Reflection.ConstructorInfo registryKeyCtorInfo = registryKeyType.GetConstructor(
                privateConstructors, null, registryKeyConstructorTypes, null);

            //Invoke the constructor, getting us a RegistryKey
            RegistryKey resultKey = (RegistryKey)registryKeyCtorInfo.Invoke(new Object[] { safeHandle, writable });

            //return the resulting key
            return resultKey;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, out int phkResult);


        // structs

        struct SDArguments
        {
            public bool bValidCmgArd; // flag valid command-line arguments
            public bool bUploadFlag; // flag for gathered data to be uploaded
            public bool bVerboseFlag; // flag for writing output to the screen during program execution
            public bool bEncryptionFlag; // flag for encrypting data
            public int intSourceFlag;
        }

        // end structs

        static Dictionary<string, string> funcGetComputerSystem(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_ComputerSystem          
            //Console.WriteLine("Win32_ComputerSystem");
            //Console.WriteLine(currentQueryCollection.Count.ToString());
            Dictionary<string, string> tmpComputerSystem = new Dictionary<string, string>();

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Caption","Domain","TotalPhysicalMemory","NumberOfProcessors"
                string[] strElementBag = new string[] { "Caption", "Domain", "TotalPhysicalMemory", "NumberOfProcessors", "SystemType" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "servername";
                    //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                    tmpComputerSystem.Add(strElementTemp, oReturn[strElement].ToString().Trim());
                }
            }
            return tmpComputerSystem;
        }

        static Dictionary<string, string>[] funcGetProcessors(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Processor
            //Console.WriteLine("Win32_Processor");
            //Console.WriteLine(currentQueryCollection.Count.ToString());
            Dictionary<string, string>[] tmpProcessors = new Dictionary<string, string>[currentQueryCollection.Count];
            int countProcs = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Name"
                tmpProcessors[countProcs] = new Dictionary<string, string>();
                string[] strElementBag = new string[] { "Name" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "name")
                        strElementTemp = "cpu";
                    //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                    tmpProcessors[countProcs].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    countProcs++;
                }
            }
            //Console.WriteLine("Add newProcessors");
            return tmpProcessors;
        }

        static Dictionary<string, string> funcGetOperatingSystem(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_OperatingSystem
            Dictionary<string, string> tmpOperatingSystem = new Dictionary<string, string>();
            bool bOtherTypeDescription = false;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Caption","CSDVersion","Version","BootDevice","SystemDevice","SystemDirectory","WindowsDirectory","InstallDate"
                string[] strElementBag = new string[] { "Caption", "CSDVersion", "Version", "BootDevice", "OtherTypeDescription", "SystemDevice", "SystemDirectory", "WindowsDirectory", "InstallDate", "CurrentTimeZone", "Organization", "RegisteredUser", "BuildNumber" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    //Console.WriteLine(strElementTemp);
                    if (strElementTemp == "caption")
                        strElementTemp = "osproduct";
                    if (strElementTemp == "csdversion")
                        strElementTemp = "osservicepack";
                    if (strElementTemp == "version")
                        strElementTemp = "osversionnumber";
                    if (strElementTemp == "installdate")
                        strElementTemp = "osinstalldate";

                    //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                    if (strElementTemp == "osproduct")
                    {
                        string strOSProduct = oReturn[strElement].ToString().Trim().Replace("\u00ae", "&#x00AE");
                        tmpOperatingSystem.Add(strElementTemp, strOSProduct);
                    }
                    else if (strElementTemp == "othertypedescription")
                    {
                        try
                        {
                            // capture R2 for Windows 2003 R2
                            tmpOperatingSystem.Add(strElementTemp, oReturn[strElement].ToString().Trim());
                            bOtherTypeDescription = true;
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        tmpOperatingSystem.Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                }
            }

            if (bOtherTypeDescription)
            {
                // append R2 to product name for Windows 2003 R2
                string strOSProduct = tmpOperatingSystem["osproduct"].Trim();
                string strOtherTypeDescription = tmpOperatingSystem["othertypedescription"].Trim();
                string strR2String = " R2,";
                if (strOtherTypeDescription == "R2")
                {
                    tmpOperatingSystem.Remove("osproduct");
                    strOSProduct = strOSProduct.Replace(",", strR2String);
                    tmpOperatingSystem.Add("osproduct", strOSProduct);
                }
            }

            return tmpOperatingSystem;
        }

        static Dictionary<string, string>[] funcGetDisks(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_LogicalDisk
            //Console.WriteLine("Get the disks");
            //Console.WriteLine("Win32_LogicalDisk");
            //Console.WriteLine(currentQueryCollection.Count.ToString());
            Dictionary<string, string>[] tmpDisks = new Dictionary<string, string>[currentQueryCollection.Count];
            int countDisks = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Caption","VolumeName","DiskSize","DriveType","FileSystem"
                // "Caption","VolumeName","Size","DriveType","FileSystem","FreeSpace"
                //Console.WriteLine("Get a disk {0}", countDisks);
                tmpDisks[countDisks] = new Dictionary<string, string>();
                // InstallDate does not retrieve a value
                string[] strElementBag = new string[] { "Caption", "VolumeName", "Size", "DriveType", "FileSystem", "FreeSpace", "VolumeSerialNumber" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    //Console.WriteLine(strElementTemp);
                    if (strElementTemp == "caption")
                        strElementTemp = "driveletter";
                    if (strElementTemp == "volumename")
                        strElementTemp = "drivelabel";
                    if (strElementTemp == "volumeserialnumber")
                        strElementTemp = "driveserialnumber";
                    if (strElementTemp == "size")
                        strElementTemp = "drivesize";
                    if (strElementTemp == "filesystem")
                        strElementTemp = "drivefilesystem";
                    if (strElementTemp == "freespace")
                        strElementTemp = "drivefreespace";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpDisks[countDisks].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpDisks[countDisks].Add(strElementTemp, "<na>");
                    }
                }
                //Console.WriteLine("Current disk number {0}", countDisks);
                countDisks++;
                //Console.WriteLine("Next disk number {0}", countDisks);
                //Console.WriteLine("Incremented");
            }

            return tmpDisks;

            //* Notes on DriveType 
            // * 2 - Removable disk
            // * 3 - Local disk
            // * 4 - Network drive
            // * 5 - Compact Disc
            // * 6 - RAM disk
        }

        static Dictionary<string, string> funcGetComputerSystemProduct(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_ComputerSystemProduct
            Dictionary<string, string> tmpComputerSystemProduct = new Dictionary<string, string>();

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Name", "Version"
                string[] strElementBag = new string[] { "Name", "Version", "Caption" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "name")
                        strElementTemp = "systemname";
                    if (strElementTemp == "version")
                        strElementTemp = "systemversion";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpComputerSystemProduct.Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpComputerSystemProduct.Add(strElementTemp, "<na>");
                    }
                }

            }
            return tmpComputerSystemProduct;
        }

        static Dictionary<string, string> funcGetBIOS(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_BIOS
            Dictionary<string, string> tmpBIOS = new Dictionary<string, string>();

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Manufacturer","Version","SerialNumber"
                string[] strElementBag = new string[] { "Manufacturer", "Version", "SerialNumber", "Caption", "Name", "SMBIOSBIOSVersion" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "manufacturer")
                        strElementTemp = "hardwaremanufacturer";
                    if (strElementTemp == "version")
                        strElementTemp = "hardwareversion";
                    if (strElementTemp == "serialnumber")
                        strElementTemp = "hardwareserialnumber";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpBIOS.Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpBIOS.Add(strElementTemp, "<na>");
                    }
                }
            }
            return tmpBIOS;
        }

        static Dictionary<string, string>[] funcGetNetworkAdapter(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_NetworkAdapter
            Dictionary<string, string>[] tmpNetworkAdapters = new Dictionary<string, string>[currentQueryCollection.Count];
            int countNetworkAdapters = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpNetworkAdapters[countNetworkAdapters] = new Dictionary<string, string>();
                // "Caption","Manufacturer","AdapterType","MACAddress","NetworkAddresses","ProductName"
                string[] strElementBag = new string[] { "Caption", "Manufacturer", "AdapterType", "MACAddress", "NetworkAddresses", "ProductName" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "niclabel";
                    if (strElementTemp == "manufacturer")
                        strElementTemp = "nicmanufacturer";
                    if (strElementTemp == "adaptertype")
                        strElementTemp = "nicadaptertype";
                    if (strElementTemp == "macaddress")
                        strElementTemp = "nicmacaddress";
                    if (strElementTemp == "networkaddresses")
                        strElementTemp = "nicnetworkaddresses";
                    if (strElementTemp == "productname")
                        strElementTemp = "nicproductname";

                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpNetworkAdapters[countNetworkAdapters].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpNetworkAdapters[countNetworkAdapters].Add(strElementTemp, "<na>");
                    }
                }
                countNetworkAdapters++;
            }
            return tmpNetworkAdapters;
        }

        static Dictionary<string, string>[] funcGetNetworkAdapterConfigurations(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_NetworkAdapterConfiguration
            Dictionary<string, string>[] tmpNetworkAdapterConfigurations = new Dictionary<string, string>[currentQueryCollection.Count];
            int countNetworkAdapterConfigurations = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                // "Caption","IPAddress","IPSubnet","DefaultIPGateway","MACAddress"
                tmpNetworkAdapterConfigurations[countNetworkAdapterConfigurations] = new Dictionary<string, string>();
                string[] strElementBag = new string[] { "Caption", "IPAddress", "IPSubnet", "DefaultIPGateway", "MACAddress" };
                foreach (string strElement in strElementBag)
                {
                    if (strElement == "IPAddress" | strElement == "IPSubnet" | strElement == "DefaultIPGateway")
                    {
                        string[] strElementArray = (string[])oReturn[strElement];
                        if (strElementArray != null)
                        {
                            string strElementArrayItem = String.Join(",", strElementArray);
                            string strElementArrayName = strElement.ToLower(new CultureInfo("en-US", false));
                            if (strElementArrayName == "ipaddress")
                                strElementArrayName = "nicconfigipaddress";
                            if (strElementArrayName == "ipsubnet")
                                strElementArrayName = "nicconfigipsubnet";
                            if (strElementArrayName == "defaultipgateway")
                                strElementArrayName = "nicconfigdefaultipgateway";
                            strElementArrayItem = strElementArrayItem.Replace(",", ", ");
                            tmpNetworkAdapterConfigurations[countNetworkAdapterConfigurations].Add(strElementArrayName, strElementArrayItem);
                        }
                        else
                        {
                            string strElementArrayName = strElement.ToLower(new CultureInfo("en-US", false));
                            if (strElementArrayName == "ipaddress")
                                strElementArrayName = "nicconfigipaddress";
                            if (strElementArrayName == "ipsubnet")
                                strElementArrayName = "nicconfigipsubnet";
                            if (strElementArrayName == "defaultipgateway")
                                strElementArrayName = "nicconfigdefaultipgateway";
                            tmpNetworkAdapterConfigurations[countNetworkAdapterConfigurations].Add(strElementArrayName, "<na>");
                        }
                    }
                    else
                    {
                        string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                        if (strElementTemp == "caption")
                            strElementTemp = "nicconfiglabel";
                        if (strElementTemp == "macaddress")
                            strElementTemp = "nicconfigmacaddress";
                        try
                        {
                            tmpNetworkAdapterConfigurations[countNetworkAdapterConfigurations].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                        }
                        catch
                        {
                            tmpNetworkAdapterConfigurations[countNetworkAdapterConfigurations].Add(strElementTemp, "<na>");
                        }
                    }
                }
                countNetworkAdapterConfigurations++;
            }
            return tmpNetworkAdapterConfigurations;
        }

        static Dictionary<string, string>[] funcGetShares(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Share
            Dictionary<string, string>[] tmpShares = new Dictionary<string, string>[currentQueryCollection.Count];
            int countShares = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpShares[countShares] = new Dictionary<string, string>();
                // "Name","Caption","Description","Path","Type"
                string[] strElementBag = new string[] { "Name", "Caption", "Description", "Path", "Type" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "name")
                        strElementTemp = "sharename";
                    if (strElementTemp == "caption")
                        strElementTemp = "sharelabel";
                    if (strElementTemp == "description")
                        strElementTemp = "sharedescription";
                    if (strElementTemp == "path")
                        strElementTemp = "sharepath";
                    if (strElementTemp == "type")
                        strElementTemp = "sharetype";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpShares[countShares].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpShares[countShares].Add(strElementTemp, "<na>");
                    }
                }
                countShares++;
            }
            return tmpShares;
        }

        static Dictionary<string, string>[] funcGetPrinters(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Share
            Dictionary<string, string>[] tmpPrinters = new Dictionary<string, string>[currentQueryCollection.Count];
            int countPrinters = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpPrinters[countPrinters] = new Dictionary<string, string>();
                // "Name","Caption","Description","Path","Type"
                string[] strElementBag = new string[] { "Name", "Caption", "Description", "DriverName", "Location", "PortName", "ServerName", "ShareName", "Status" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "name")
                        strElementTemp = "printername";
                    if (strElementTemp == "caption")
                        strElementTemp = "printerlabel";
                    if (strElementTemp == "description")
                        strElementTemp = "printerdescription";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpPrinters[countPrinters].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpPrinters[countPrinters].Add(strElementTemp, "<na>");
                    }
                }
                countPrinters++;
            }
            return tmpPrinters;
        }

        static List<string> funcGetProducts()
        {
            // 0 - HKCR = HKEY_CLASSES_ROOT
            // 1 - HKCU = HKEY_CURRENT_USER
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            List<string> tmpProductList = new List<string>();

            RegistryKey objProductRegKey;
            object objRegKeyValue;
            RegistryKey objProductInfo;
            string[] arrProfileList;

            // HKEY_CLASSES_ROOT\Installer\Products\[Key]\ProductName
            // 0 - HKCR = HKEY_CLASSES_ROOT

            try
            {
                //Console.WriteLine("HKEY_CLASSES_ROOT\\Installer\\Products\\[Key]\\ProductName");
                objProductInfo = funcGetRegistryData(0, "Installer\\Products");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(0, "Installer\\Products" + "\\" + strTemp);
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("ProductName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\[Key]\DisplayName
            // 1 - HKCU = HKEY_CURRENT_USER

            try
            {
                //Console.WriteLine("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\[Key]\\DisplayName");
                objProductInfo = funcGetRegistryData(1, "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(1, "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + strTemp);
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("DisplayName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // HKLM\SOFTWARE\Classes\Installer\Products\[Key]\ProductName
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            try
            {
                //Console.WriteLine("HKLM\\SOFTWARE\\Classes\\Installer\\Products\\[Key]\\ProductName");
                objProductInfo = funcGetRegistryData(2, "SOFTWARE\\Classes\\Installer\\Products");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(2, "SOFTWARE\\Classes\\Installer\\Products" + "\\" + strTemp);
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("ProductName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\[Key]\InstallProperties\DisplayName
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            try
            {
                //Console.WriteLine("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products\\[Key]\\InstallProperties\\DisplayName");
                objProductInfo = funcGetRegistryData(2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products" + "\\" + strTemp + "\\InstallProperties");
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("DisplayName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\[Key]\DisplayName
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            try
            {
                //Console.WriteLine("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\[Key]\\DisplayName");
                objProductInfo = funcGetRegistryData(2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(2, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + strTemp);
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("DisplayName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\[Key]\DisplayName
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            try
            {
                //Console.WriteLine("HKLM\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\[Key]\\DisplayName");
                objProductInfo = funcGetRegistryData(2, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

                if (objProductInfo != null)
                {
                    arrProfileList = objProductInfo.GetSubKeyNames();

                    foreach (string strTemp in arrProfileList)
                    {
                        objProductRegKey = funcGetRegistryData(2, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" + "\\" + strTemp);
                        // [DebugLine] Console.WriteLine(strTemp);
                        objRegKeyValue = objProductRegKey.GetValue("DisplayName");
                        if (objRegKeyValue != null)
                        {
                            //  [DebugLine] Console.WriteLine("DisplayName does not exist for: {0}", strTemp);
                            try
                            {
                                //  [DebugLine] Console.WriteLine("{0} :: {1}", strTemp, objRegKeyValue.ToString());
                                if (!tmpProductList.Contains(objRegKeyValue.ToString()))
                                {
                                    tmpProductList.Add(objRegKeyValue.ToString());
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            tmpProductList.Sort();

            return tmpProductList;
        }

        static Dictionary<string, string>[] funcGetServerFeatures(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_ServerFeature
            Dictionary<string, string>[] tmpServerFeatures = new Dictionary<string, string>[currentQueryCollection.Count];
            int countServerFeatures = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpServerFeatures[countServerFeatures] = new Dictionary<string, string>();
                // "Name"
                string[] strElementBag = new string[] { "ID", "Name" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "id")
                        strElementTemp = "roleorfeature_id";
                    if (strElementTemp == "name")
                        strElementTemp = "roleorfeaturename";
                    try
                    {
                        tmpServerFeatures[countServerFeatures].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        tmpServerFeatures[countServerFeatures].Add(strElementTemp, "<na>");
                    }
                }
                countServerFeatures++;
            }
            return tmpServerFeatures;
        }

        static Dictionary<string, string>[] funcGetServices(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Service
            Dictionary<string, string>[] tmpServices = new Dictionary<string, string>[currentQueryCollection.Count];
            int countServices = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpServices[countServices] = new Dictionary<string, string>();
                // "Caption","Description","PathName","Status","State","StartMode","StartName"
                string[] strElementBag = new string[] { "Caption", "Description", "PathName", "Status", "State", "StartMode", "StartName" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "servicename";
                    if (strElementTemp == "description")
                        strElementTemp = "servicedescription";
                    if (strElementTemp == "pathname")
                        strElementTemp = "servicepathname";
                    if (strElementTemp == "status")
                        strElementTemp = "servicestatus";
                    if (strElementTemp == "state")
                        strElementTemp = "servicestate";
                    if (strElementTemp == "startmode")
                        strElementTemp = "servicestartmode";
                    if (strElementTemp == "startname")
                        strElementTemp = "servicestartname";
                    try
                    {
                        //Console.WriteLine(strElementTemp + "\t" + oReturn[strElement].ToString().Trim());
                        tmpServices[countServices].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        //Console.WriteLine(strElementTemp + "\t" + "<na>");
                        tmpServices[countServices].Add(strElementTemp, "<na>");
                    }
                }
                countServices++;
            }
            return tmpServices;
        }

        static Dictionary<string, string>[] funcGetUserAccounts(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_UserAccount
            Dictionary<string, string>[] tmpUserAccounts = new Dictionary<string, string>[currentQueryCollection.Count];
            int countUserAccounts = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpUserAccounts[countUserAccounts] = new Dictionary<string, string>();
                // "Caption", "Description", "Domain", "FullName", "Name"
                string[] strElementBag = new string[] { "Caption", "Description", "Domain", "FullName", "Name" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "uacaption";
                    if (strElementTemp == "description")
                        strElementTemp = "uadescription";
                    if (strElementTemp == "domain")
                        strElementTemp = "uadomain";
                    if (strElementTemp == "fullname")
                        strElementTemp = "uafullname";
                    if (strElementTemp == "name")
                        strElementTemp = "uaname";
                    tmpUserAccounts[countUserAccounts].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                }
                countUserAccounts++;
            }
            return tmpUserAccounts;
        }

        static Dictionary<string, string>[] funcGetGroups(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Group
            Dictionary<string, string>[] tmpGroups = new Dictionary<string, string>[currentQueryCollection.Count];
            int countGroups = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpGroups[countGroups] = new Dictionary<string, string>();
                // "Caption", "Description", "Domain", "Name"
                string[] strElementBag = new string[] { "Caption", "Description", "Domain", "Name" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "grpcaption";
                    if (strElementTemp == "description")
                        strElementTemp = "grpdescription";
                    if (strElementTemp == "domain")
                        strElementTemp = "grpdomain";
                    if (strElementTemp == "name")
                        strElementTemp = "grpname";
                    tmpGroups[countGroups].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                }
                countGroups++;
            }
            return tmpGroups;
        }

        static Dictionary<string, string>[] funcGetAccounts(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Account
            Dictionary<string, string>[] tmpAccounts = new Dictionary<string, string>[currentQueryCollection.Count];
            int countAccounts = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpAccounts[countAccounts] = new Dictionary<string, string>();
                // "Caption", "Description", "Domain", "Name"
                string[] strElementBag = new string[] { "Caption", "Description", "Domain", "Name" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "acctcaption";
                    if (strElementTemp == "description")
                        strElementTemp = "acctdescription";
                    if (strElementTemp == "domain")
                        strElementTemp = "acctdomain";
                    if (strElementTemp == "name")
                        strElementTemp = "acctname";
                    tmpAccounts[countAccounts].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                }
                countAccounts++;
            }
            return tmpAccounts;
        }

        static Dictionary<string, string>[] funcGetVolumes(ManagementObjectCollection currentQueryCollection)
        {
            //Win32_Volume
            Dictionary<string, string>[] tmpVolumes = new Dictionary<string, string>[currentQueryCollection.Count];
            int countVolumes = 0;

            foreach (ManagementObject oReturn in currentQueryCollection)
            {
                tmpVolumes[countVolumes] = new Dictionary<string, string>();
                // InstallDate does not retrieve a value
                string[] strElementBag = new string[] { "Caption", "Name", "DriveLetter", "Label" };
                foreach (string strElement in strElementBag)
                {
                    string strElementTemp = strElement.ToLower(new CultureInfo("en-US", false));
                    if (strElementTemp == "caption")
                        strElementTemp = "volcaption";
                    if (strElementTemp == "name")
                        strElementTemp = "volname";
                    if (strElementTemp == "driveletter")
                        strElementTemp = "voldriveletter";
                    if (strElementTemp == "label")
                        strElementTemp = "vollabel";
                    try
                    {
                        tmpVolumes[countVolumes].Add(strElementTemp, oReturn[strElement].ToString().Trim());
                    }
                    catch
                    {
                        tmpVolumes[countVolumes].Add(strElementTemp, "<na>");
                    }
                }
                countVolumes++;
            }
            return tmpVolumes;
            //* Notes on DriveType 
            // * 2 - Removable disk
            // * 3 - Local disk
            // * 4 - Network drive
            // * 5 - Compact Disc
            // * 6 - RAM disk
        }

        static DataConstruct.sDriveRoot[] funcGetDriveRoots(int tmpDriveCount)
        {
            DataConstruct.sDriveRoot[] tmpDriveRoots = new DataConstruct.sDriveRoot[tmpDriveCount];
            int intDriveCounter = 0;

            tmpDriveRoots[intDriveCounter] = new DataConstruct.sDriveRoot();
            string[] drives = Environment.GetLogicalDrives();
            foreach (string drive in drives)
            {
                DriveInfo currentdrive = new DriveInfo(drive);
                if (currentdrive.IsReady)
                {
                    //Console.WriteLine(currentdrive.Name);
                    tmpDriveRoots[intDriveCounter].DriveLetter = currentdrive.Name;
                    DirectoryInfo currdriveroot = currentdrive.RootDirectory;
                    DirectoryInfo[] rootdirs = currdriveroot.GetDirectories();
                    List<string> tmpRootFolders = new List<string>();
                    foreach (DirectoryInfo rootdir in rootdirs)
                    {
                        //Console.WriteLine(rootdir.FullName);
                        tmpRootFolders.Add(rootdir.FullName);
                    }
                    tmpDriveRoots[intDriveCounter].DriveRootFolders = tmpRootFolders;
                }
                intDriveCounter++;
            }

            return tmpDriveRoots;
        }

        static DataConstruct.sProgramFilesRoot[] funcGetProgramFilesRoots()
        {
            string[] drives = Environment.GetLogicalDrives();
            List<string> tmpProgamFilesRootList = new List<string>();
            //Console.WriteLine(drives.Length.ToString());
            foreach (string drive in drives)
            {
                DriveInfo currentdrive = new DriveInfo(drive);
                //Console.WriteLine(currentdrive.Name);
                if (currentdrive.IsReady & currentdrive.DriveType == DriveType.Fixed)
                {
                    //Console.WriteLine("Ready and Fixed");
                    DirectoryInfo currdriveroot = currentdrive.RootDirectory;
                    DirectoryInfo[] rootdirs = currdriveroot.GetDirectories();
                    foreach (DirectoryInfo rootdir in rootdirs)
                    {
                        if(rootdir.FullName.ToUpper().Contains("PROGRAM FILES"))
                            tmpProgamFilesRootList.Add(rootdir.FullName);
                    }
                }
            }
            //Console.WriteLine(tmpProgamFilesRootList.Count.ToString());

            DataConstruct.sProgramFilesRoot[] tmpProgramFilesRoots = new DataConstruct.sProgramFilesRoot[tmpProgamFilesRootList.Count];
            int intProgramFilesRootCounter = 0;

            foreach (string strProgramFilesDir in tmpProgamFilesRootList)
            {
                //Console.WriteLine(strProgramFilesDir);
                DirectoryInfo programfilesroot = new DirectoryInfo(strProgramFilesDir);
                tmpProgramFilesRoots[intProgramFilesRootCounter] = new DataConstruct.sProgramFilesRoot();
                tmpProgramFilesRoots[intProgramFilesRootCounter].ProgramFilesPath = strProgramFilesDir;
                tmpProgramFilesRoots[intProgramFilesRootCounter].ProgramFilesFolders = new List<string>();
                DirectoryInfo[] programfilesdirs = programfilesroot.GetDirectories();
                //Console.WriteLine(programfilesdirs.Length.ToString());
                foreach (DirectoryInfo programfilesdir in programfilesdirs)
                {
                    //Console.WriteLine(programfilesdir.FullName);
                    tmpProgramFilesRoots[intProgramFilesRootCounter].ProgramFilesFolders.Add(programfilesdir.FullName);
                }
                intProgramFilesRootCounter++;
            }

            return tmpProgramFilesRoots;
        }

        static Dictionary<string, string>[] funcGetODBCDataSources_x64(int intValueCount)
        {
            string strODBCDataSourcesPath = "Software\\ODBC\\ODBC.INI\\ODBC Data Sources";
            RegistryKey regODBCDataSources = _openSubKey(Registry.LocalMachine, strODBCDataSourcesPath, false, RegWow64Options.KEY_WOW64_64KEY);

            Dictionary<string, string>[] tmpODBCDataSources = new Dictionary<string, string>[intValueCount];

            int intDataSourceCount = 0;

            string[] strODBCDataSources = regODBCDataSources.GetValueNames();
            object objValue;
            //object objDatabase;
            //object objServer;

            foreach (string strValueName in strODBCDataSources)
            {
                tmpODBCDataSources[intDataSourceCount] = new Dictionary<string, string>();
                //Console.WriteLine("DataSource: " + strValueName);
                tmpODBCDataSources[intDataSourceCount].Add("DataSource", strValueName);
                objValue = regODBCDataSources.GetValue(strValueName);
                tmpODBCDataSources[intDataSourceCount].Add("Driver", objValue.ToString());
                //Console.WriteLine("Driver: " + objValue.ToString());

                // ****** next section was removed due to differences in DSN details between
                //        different drivers
                //string strRegSubKeyPath = "Software\\ODBC\\ODBC.INI\\" + strValueName;
                //RegistryKey regODBCINIKey = _openSubKey(Registry.LocalMachine, strRegSubKeyPath, false, RegWow64Options.KEY_WOW64_64KEY);
                //if (regODBCINIKey != null)
                //{
                //    objServer = regODBCINIKey.GetValue("Server");
                //    objDatabase = regODBCINIKey.GetValue("Database");
                //    tmpODBCDataSources[intDataSourceCount].Add("Server", objServer.ToString());
                //    //Console.WriteLine("Server : " + objServer.ToString());
                //    tmpODBCDataSources[intDataSourceCount].Add("Database", objDatabase.ToString());
                //    //Console.WriteLine("Database : " + objDatabase.ToString());
                //}

                //Console.WriteLine("Close registry key");
                //regODBCINIKey.Close();

                intDataSourceCount++;
            }
            //Console.WriteLine("Close registry key");
            //regODBCDataSources.Close();

            return tmpODBCDataSources;

        }

        static Dictionary<string, string>[] funcGetODBCDataSources_x86(int intValueCount)
        {
            string strODBCDataSourcesPath = "Software\\ODBC\\ODBC.INI\\ODBC Data Sources";

            RegistryKey regODBCDataSources = Registry.LocalMachine.OpenSubKey(strODBCDataSourcesPath);

            Dictionary<string, string>[] tmpODBCDataSources = new Dictionary<string, string>[intValueCount];

            int intDataSourceCount = 0;

            string[] strODBCDataSources = regODBCDataSources.GetValueNames();
            object objValue;
            //object objDatabase;
            //object objServer;

            foreach (string strValueName in strODBCDataSources)
            {
                tmpODBCDataSources[intDataSourceCount] = new Dictionary<string, string>();
                //Console.WriteLine("DataSource: " + strValueName);
                tmpODBCDataSources[intDataSourceCount].Add("DataSource", strValueName);
                objValue = regODBCDataSources.GetValue(strValueName);
                tmpODBCDataSources[intDataSourceCount].Add("Driver", objValue.ToString());
                //Console.WriteLine("Driver: " + objValue.ToString());

                // ****** next section was removed due to differences in DSN details between
                //        different drivers
                //string strRegSubKeyPath = "Software\\ODBC\\ODBC.INI\\" + strValueName;
                //RegistryKey regODBCINIKey = Registry.LocalMachine.OpenSubKey(strRegSubKeyPath);
                //if (regODBCINIKey != null)
                //{
                //    objServer = regODBCINIKey.GetValue("Server");
                //    objDatabase = regODBCINIKey.GetValue("Database");
                //    tmpODBCDataSources[intDataSourceCount].Add("Server", objServer.ToString());
                //    //Console.WriteLine("Server : " + objServer.ToString());
                //    tmpODBCDataSources[intDataSourceCount].Add("Database", objDatabase.ToString());
                //    //Console.WriteLine("Database : " + objDatabase.ToString());
                //}

                //Console.WriteLine("Close registry key");
                //regODBCINIKey.Close();

                intDataSourceCount++;
            }
            //Console.WriteLine("Close registry key");
            //regODBCDataSources.Close();

            return tmpODBCDataSources;

        }

        static RegistryKey funcGetRegistryData(int intRegistryBase, string strRegPath)
        {
            // valid values for intRegistryBase
            // 0 - HKCR = HKEY_CLASSES_ROOT
            // 1 - HKCU = HKEY_CURRENT_USER
            // 2 - HKLM = HKEY_LOCAL_MACHINE

            // Default initialization of objRootKey to HKEY_LOCAL_MACHINE
            RegistryKey objRootKey = Microsoft.Win32.Registry.ClassesRoot;

            if (intRegistryBase == 0)
            {
                objRootKey = Microsoft.Win32.Registry.ClassesRoot;
            }
            else if (intRegistryBase == 1)
            {
                objRootKey = Microsoft.Win32.Registry.CurrentUser;
            }
            else if (intRegistryBase == 2)
            {
                objRootKey = Microsoft.Win32.Registry.LocalMachine;
            }
            else
            {
                objRootKey = Microsoft.Win32.Registry.LocalMachine;
            }

            RegistryKey objProfileKey = objRootKey.OpenSubKey(strRegPath);

            return objProfileKey;
        }

        static List<string> funcGetSharePermissions(string strServerName, string strShareName)
        {
            List<string> tmpSharePermissionsList = new List<string>();
            //Console.WriteLine(strShareName);

            if (strServerName == ".")
                strServerName = Environment.MachineName;

            IntPtr bufptr = IntPtr.Zero;
            int err = NetShareGetInfo(strServerName, strShareName, 502, out bufptr);
            if (0 == err)
            {
                SHARE_INFO_502 shareInfo = (SHARE_INFO_502)Marshal.PtrToStructure(bufptr, typeof(SHARE_INFO_502));

                bool bDaclPresent;
                bool bDaclDefaulted;
                IntPtr pAcl = IntPtr.Zero;
                GetSecurityDescriptorDacl(shareInfo.shi502_security_descriptor, out bDaclPresent, ref pAcl, out bDaclDefaulted);
                if (bDaclPresent)
                {
                    ACL_SIZE_INFORMATION AclSize = new ACL_SIZE_INFORMATION();
                    GetAclInformation(pAcl, ref AclSize, (uint)Marshal.SizeOf(typeof(ACL_SIZE_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);
                    for (int i = 0; i < AclSize.AceCount; i++)
                    {
                        IntPtr pAce;
                        err = GetAce(pAcl, i, out pAce);
                        ACCESS_ALLOWED_ACE ace = (ACCESS_ALLOWED_ACE)Marshal.PtrToStructure(pAce, typeof(ACCESS_ALLOWED_ACE));

                        IntPtr iter = (IntPtr)((long)pAce + (long)Marshal.OffsetOf(typeof(ACCESS_ALLOWED_ACE), "SidStart"));
                        byte[] bSID = null;
                        int size = (int)GetLengthSid(iter);
                        bSID = new byte[size];
                        Marshal.Copy(iter, bSID, 0, size);
                        IntPtr ptrSid;
                        ConvertSidToStringSid(bSID, out ptrSid);
                        string strSID = Marshal.PtrToStringAuto(ptrSid);

                        //Console.WriteLine("The details of ACE number {0} are: ", i + 1);

                        StringBuilder name = new StringBuilder();

                        uint cchName = (uint)name.Capacity;
                        StringBuilder referencedDomainName = new StringBuilder();
                        uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
                        SID_NAME_USE sidUse;

                        LookupAccountSid(null, bSID, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse);

                        //Console.WriteLine("Trustee Name: " + name);

                        //Console.WriteLine("Domain Name: " + referencedDomainName);

                        string strSecurityPrincipal = "";

                        if (referencedDomainName.ToString() != String.Empty)
                        {
                            strSecurityPrincipal = referencedDomainName.ToString() + "\\" + name.ToString();
                        }
                        else
                        {
                            strSecurityPrincipal = name.ToString();
                        }

                        //Console.WriteLine(strSecurityPrincipal);

                        string strSharePermission = "";

                        if ((ace.Mask & 0x1F01FF) == 0x1F01FF)
                        {
                            //Console.WriteLine("Permission: Full Control");
                            strSharePermission = "Full Control";
                        }
                        else if ((ace.Mask & 0x1301BF) == 0x1301BF)
                        {
                            //Console.WriteLine("Permission: READ and CHANGE");
                            strSharePermission = "Change";
                        }
                        else if ((ace.Mask & 0x1200A9) == 0x1200A9)
                        {
                            //Console.WriteLine("Permission: READ only");
                            strSharePermission = "Read Only";
                        }
                        //Console.WriteLine(strSharePermission);
                        //Console.WriteLine("SID: {0} \nHeader AceType: {1} \nAccess Mask: {2} \nHeader AceFlag: {3}", strSID, ace.Header.AceType.ToString(), ace.Mask.ToString(), ace.Header.AceFlags.ToString());
                        //Console.WriteLine("\n");
                        //Console.WriteLine("{0} : {1} : {2}",strShareName, strSecurityPrincipal, strSharePermission);

                        // Server Operators only exist domain controllers
                        if (strSecurityPrincipal == "" & strSID == "S-1-5-32-549")
                        {
                            strSecurityPrincipal = "Server Operators";
                        }

                        tmpSharePermissionsList.Add(strShareName + ":" + strSecurityPrincipal + ":" + strSharePermission);
                    }
                }
                err = NetApiBufferFree(bufptr);
            }

            return tmpSharePermissionsList;
        }

        static List<string> funcGatherSharePermissions(string strServerName, Dictionary<string, string>[] tmpShares)
        {
            List<string> listRestrictedShares = new List<string>();
            listRestrictedShares.Add("ADMIN$");
            listRestrictedShares.Add("IPC$");
            listRestrictedShares.Add("SYSVOL");
            listRestrictedShares.Add("NETLOGON");
            listRestrictedShares.Add("print$");

            string tmpShareName = "";
            bool bDefaultDriveAdminShare = false;

            List<string> listAllPermissions = new List<string>();

            foreach (Dictionary<string, string> tmpShare in tmpShares)
            {
                tmpShareName = tmpShare["sharename"].Trim();

                if (tmpShareName.Length == 2 & tmpShareName.Contains("$"))
                    bDefaultDriveAdminShare = true;

                if (!bDefaultDriveAdminShare & !listRestrictedShares.Contains(tmpShareName.Trim()))
                {
                    List<string> listPermissions = funcGetSharePermissions(strServerName, tmpShareName);

                    foreach (string item in listPermissions)
                    {
                        listAllPermissions.Add(item);
                    }
                }

                // reset
                bDefaultDriveAdminShare = false;
                tmpShareName = String.Empty;
            }
            return listAllPermissions;
        }

        static void funcSDParseArgs(string[] progargs)
        {
            //control passed from Main()

            SDArguments tmpFlags = new SDArguments();

            // initialize tmpFlags members
            tmpFlags.bValidCmgArd = false; // flag valid command-line arguments
            tmpFlags.bUploadFlag = true; // flag for gathered data to be uploaded
            tmpFlags.bVerboseFlag = false; // flag for writing output to the screen during program execution
            tmpFlags.bEncryptionFlag = true; // flag for encrypting data
            tmpFlags.intSourceFlag = 0;

            foreach (string argItem in progargs)
            {
                if (argItem == "-run" & tmpFlags.intSourceFlag == 0)
                {
                    tmpFlags.intSourceFlag = 3;
                    tmpFlags.bValidCmgArd = true;
                }
                if (argItem == "-nu")
                {
                    tmpFlags.bUploadFlag = false;
                }
                if (argItem == "-v")
                {
                    tmpFlags.bVerboseFlag = true;
                }
                if (argItem == "-ne")
                {
                    tmpFlags.bEncryptionFlag = false;
                }
            }

            if (tmpFlags.bValidCmgArd)
            {
                funcRun(tmpFlags);
            }

            if (!tmpFlags.bValidCmgArd & progargs[0] != "-?")
            {
                UtilityConstruct.funcPrintParameterWarning("SystemsDocument");
            }
        }

        static void funcRun(SDArguments runflags)
        {
            try
            {
                // control passed from funcSDParseArgs

                // currently need the next line because the upload is being sent over SSL
                // however the SSL cert on the back-end is self-signed and weak
                // modifying the server certification allows the upload to succeed
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (runflags.bValidCmgArd)
                {
                    string strNodeMgrurl = "https://sdocnm.systemsdocument.com";
                    string strNodeurl = "";

                    string strProgramUserAgent = "SystemsDocument 1.0";

                    string tmpsKey = UtilityConstruct.funcGetsKey("SystemsDocument");
                    string serverhash = UtilityConstruct.funcGetServerHashRIPEMD160(Environment.MachineName + tmpsKey);
                    funcWriteToLog(Environment.MachineName + ":" + serverhash);

                    // initially using round-robin DNS to point clients at different servers using nm.systemsdocument
                    string statresponse = funcStatusCheck(strNodeMgrurl + "/status", tmpsKey, serverhash);
                    funcWriteToLog(statresponse);


                    if (statresponse.Split(':')[0] == "STAT100")
                    {
                        funcWriteToLog("SystemsDocument [Running]");
                        funcWriteToEventLog("SystemsDocument [Running]", 100);
                        if (runflags.bVerboseFlag)
                            Console.WriteLine("Running...");

                        DateTime current = DateTime.Now;
                        string dtFormat = "MMddyyyyHHmmss"; // for output file creation
                        string outputfile = "sdinf" + current.ToString(dtFormat) + ".dat";

                        Dictionary<string, string> dictServerData = new Dictionary<string, string>();

                        if (runflags.bVerboseFlag)
                            Console.WriteLine("Gathering data...");

                        // build dictionary with initial data
                        dictServerData.Add("Node", statresponse.Split(':')[1].Trim());
                        dictServerData.Add("OutputFile", outputfile);
                        dictServerData.Add("ServerHash", serverhash);
                        dictServerData.Add("ServerName", Environment.MachineName);

                        if (runflags.bVerboseFlag)
                            Console.WriteLine(dictServerData["ServerName"] + ":" + dictServerData["ServerHash"]);

                        //funcWriteToErrorLog("outputfilename before calling funcGatherSystemData: " + outputfile);
                        funcGatherSystemData(dictServerData["ServerName"], dictServerData["OutputFile"], runflags.bVerboseFlag, runflags.bEncryptionFlag, dictServerData["ServerHash"]);
                        //funcWriteToErrorLog("outputfilename after calling funcGatherSystemData: " + outputfile);

                        if (runflags.bUploadFlag & File.Exists(dictServerData["OutputFile"]))
                        {
                            strNodeurl = "https://" + dictServerData["Node"] + ".systemsdocument.com";

                            string uploadresponse = "";
                            string uploadurl = strNodeurl + "/stage";

                            funcWriteToLog("Uploading " + dictServerData["OutputFile"]);

                            uploadresponse = UtilityConstruct.funcHttpFileUpload(uploadurl, dictServerData["OutputFile"], strProgramUserAgent);

                            if (uploadresponse.StartsWith("UPLOAD GOOD"))
                            {
                                funcWriteToLog("Upload for " + dictServerData["OutputFile"] + " was successful");

                                File.Delete(dictServerData["OutputFile"]);

                                dictServerData.Add("md5Hash", uploadresponse.Split(':')[1].Trim());

                                if (runflags.bVerboseFlag)
                                {
                                    Console.WriteLine(uploadresponse);
                                    Console.WriteLine("Retrieving file...");
                                }
                                
                                System.Threading.Thread.Sleep(5000); // small pause before attempting download(s)

                                funcPDFRetrieve(dictServerData, strNodeurl);
                            }
                            else
                            {
                                funcWriteToLog("Please see the error log.");
                                funcWriteToErrorLog("Error during upload process.");
                            }

                        }

                        funcWriteToLog("SystemsDocument [End]");
                        funcWriteToEventLog("SystemsDocument [End]", 101);

                        if (runflags.bVerboseFlag)
                            Console.WriteLine("Finished.");
                    }
                    else
                    {
                        if (statresponse == "STAT400")
                        {
                            funcWriteToLog("SystemsDocument service is not available");
                        }
                        else if (statresponse == "STAT401")
                        {
                            funcWriteToLog("SystemsDocument: " + "There was an provisioning issue for the skey. Please contact support.");
                        }
                        else if (statresponse == "STAT402")
                        {
                            funcWriteToLog("SystemsDocument: " + "Server limit has been reached for the skey being used");
                        }
                        else if (statresponse == "STAT403")
                        {
                            funcWriteToLog("SystemsDocument: " + Environment.MachineName + " has been previously documented");
                        }

                        funcWriteToLog("No data will be gathered");
                    }
                }
            }
            catch (WebException e)
            {
                funcWriteToErrorLog("Error with a call to the SystemsDocument service.");
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, e);
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static void funcPDFRetrieve(Dictionary<string, string> dictServerInfo, string strRetrieveurl)
        {
            try
            {
                bool bFileReady = false;
                int intFileReadyCtr = 0;

                string strProgramUserAgent = "SystemsDocument 1.0";

                string strSaveLocation = "";

                funcWriteToLog("Checking for SystemsDocument.cfg");
                //Console.WriteLine("Checking for SystemsDocument.cfg");
                if (File.Exists("SystemsDocument.cfg"))
                {
                    funcWriteToLog("SystemsDocument.cfg exists");
                    //Console.WriteLine("SystemsDocument.cfg exists");
                    string[] arrConfigOptions = File.ReadAllLines("SystemsDocument.cfg");
                    //Console.WriteLine(arrConfigOptions.Length.ToString());
                    if (arrConfigOptions != null & arrConfigOptions.Length > 0)
                    {
                        //Console.WriteLine("Checking for SaveLocation");
                        //Console.WriteLine(arrConfigOptions[0].Split('|')[1].Trim());
                        funcWriteToLog("Checking for SaveLocation");
                        if (Directory.Exists(arrConfigOptions[0].Split('|')[1].Trim()))
                        {
                            //Console.WriteLine("SaveLocation exists");
                            funcWriteToLog("SaveLocation exists");
                            strSaveLocation = arrConfigOptions[0].Split('|')[1].Trim();
                            //Console.WriteLine(strSaveLocation);
                        }
                        else
                        {
                            strSaveLocation = "";
                        }
                    }
                }
                else
                {
                    strSaveLocation = String.Empty;
                }

                string requestresponse = "";
                string responseurl = strRetrieveurl + "/status2";

                do
                {
                    funcWriteToLog("Requesting file for: " + dictServerInfo["md5Hash"]);
                    requestresponse = UtilityConstruct.funcHttpPutRequest(responseurl, dictServerInfo["md5Hash"], strProgramUserAgent);
                    //Console.WriteLine("Status: {0}", requestresponse);
                    if (requestresponse != "<retry>")
                    {
                        bFileReady = true;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(5000); // 5 seconds

                        // each increment of intFileReadyCtr means 5 seconds have elapsed
                        intFileReadyCtr += 1;
                    }

                } while (!bFileReady & intFileReadyCtr < 4);

                if (bFileReady)
                {
                    string fileToDownload = requestresponse.Split(':')[1];
                    string pickupurl = strRetrieveurl + "/pickup/" + fileToDownload;
                    //Console.WriteLine(fileToDownload);
                    //Console.WriteLine(pickupurl);

                    string strDAT = fileToDownload.Replace(fileToDownload.Split('_')[1], dictServerInfo["ServerName"]);
                    //Console.WriteLine(strDAT);

                    string downloadresponse = UtilityConstruct.funcHttpFileDownload(pickupurl, strDAT);
                    //Console.WriteLine(downloadresponse);

                    //funcWriteToLog("Download result: " + strDAT + " " + downloadresponse);
                    funcWriteToLog("Download result: " + downloadresponse);
                    string strPDF = strDAT.Replace(".dat", ".pdf");
                    if (strSaveLocation != "" & strSaveLocation != null)
                        strPDF = strSaveLocation + "\\" + strPDF;
                    PerformDecryption(strDAT, strPDF, true);
                    if (File.Exists(strPDF))
                    {
                        //Console.WriteLine("PDF {0} exists", strPDF);
                        funcWriteToLog("PDF: " + strPDF);
                        File.Delete(strDAT);
                    }
                }
                else
                {
                    funcWriteToLog("Download result: Data for " + dictServerInfo["ServerName"] + " failed to download");
                    //funcWriteToLog("Download result: Data for " + dictServerList[requestresponse.Split('_')[1]] + " failed to download");
                }

                // reset after each download or download attempt
                bFileReady = false;
                intFileReadyCtr = 0;
            }
            catch (Exception ex)
            {
                funcWriteToErrorLog("Error retrieving PDF file");
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static string funcStatusCheck(string URI, string strsKey, string tmpMachineName)
        {
            funcWriteToLog("Checking for service status");

            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(URI);
            //req.Proxy = new System.Net.WebProxy(ProxyString, true);

            //Add these, as we're doing a POST
            //req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "*/*";
            //req.ContentType = "application/octet-stream";
            req.Method = "PUT";
            req.UserAgent = "SystemsDocument 1.0";

            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);

            string strstatcheck = "<status>" + ":" + strsKey + ":" +tmpMachineName;
            //funcWriteToLog(strstatcheck);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(strstatcheck);

            req.ContentLength = bytes.Length;

            //Console.WriteLine(req.Headers.Count.ToString());
            //Console.WriteLine(req.Headers.ToString());

            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();

            //Console.WriteLine(req.Headers.Count.ToString());
            //Console.WriteLine(req.Headers.ToString());

            System.Net.WebResponse resp = req.GetResponse();

            if (resp == null) return null;

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        static void PerformEncryption(string inputstring, string outputfile, bool bEncrypt)
        {
            //string outputdatfile = outputprefix + ".dat";
            //string output_inputmirror = outputprefix + ".out";

            //string strLogMessage = "";

            //strLogMessage = "inside PerformEncryption, inputstring length: " + inputstring.Length.ToString();
            //funcWriteToLog(strLogMessage);
            //strLogMessage = "inside PerformEncryption, outputfile: " + outputfile;
            //funcWriteToLog(strLogMessage);

            if (bEncrypt)
            {
                try
                {
                    ///string str = "YMeFP,Ury\\^U;Ch'G,pmdo%#&er:t0Op";
                    SecureString securekey = new SecureString();
                    securekey.AppendChar('Y');
                    securekey.AppendChar('M');
                    securekey.AppendChar('e');
                    securekey.AppendChar('F');
                    securekey.AppendChar('P');
                    securekey.AppendChar(',');
                    securekey.AppendChar('U');
                    securekey.AppendChar('r');
                    securekey.AppendChar('y');
                    securekey.AppendChar('\\');
                    securekey.AppendChar('^');
                    securekey.AppendChar('U');
                    securekey.AppendChar(';');
                    securekey.AppendChar('C');
                    securekey.AppendChar('h');
                    securekey.AppendChar('\'');
                    securekey.AppendChar('G');
                    securekey.AppendChar(',');
                    securekey.AppendChar('p');
                    securekey.AppendChar('m');
                    securekey.AppendChar('d');
                    securekey.AppendChar('o');
                    securekey.AppendChar('%');
                    securekey.AppendChar('#');
                    securekey.AppendChar('&');
                    securekey.AppendChar('e');
                    securekey.AppendChar('r');
                    securekey.AppendChar(':');
                    securekey.AppendChar('t');
                    securekey.AppendChar('0');
                    securekey.AppendChar('O');
                    securekey.AppendChar('p');
                    securekey.MakeReadOnly();
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    IntPtr ptr = Marshal.SecureStringToBSTR(securekey);
                    byte[] key = encoding.GetBytes(Marshal.PtrToStringUni(ptr));
                    securekey.Dispose();

                    //string str2 = "mOG+9s%$%\\;O+IDG";
                    SecureString secureiv = new SecureString();
                    secureiv.AppendChar('m');
                    secureiv.AppendChar('O');
                    secureiv.AppendChar('G');
                    secureiv.AppendChar('+');
                    secureiv.AppendChar('9');
                    secureiv.AppendChar('s');
                    secureiv.AppendChar('%');
                    secureiv.AppendChar('$');
                    secureiv.AppendChar('%');
                    secureiv.AppendChar('\\');
                    secureiv.AppendChar(';');
                    secureiv.AppendChar('O');
                    secureiv.AppendChar('+');
                    secureiv.AppendChar('I');
                    secureiv.AppendChar('D');
                    secureiv.AppendChar('G');
                    secureiv.MakeReadOnly();
                    System.Text.UTF8Encoding encoding2 = new System.Text.UTF8Encoding();
                    IntPtr ptr2 = Marshal.SecureStringToBSTR(secureiv);
                    byte[] iv16Bit = encoding2.GetBytes(Marshal.PtrToStringUni(ptr2));
                    secureiv.Dispose();

                    // Create a new instance of the RijndaelManaged
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    RijndaelManaged myRijndael = new RijndaelManaged();

                    myRijndael.Key = key;
                    myRijndael.IV = iv16Bit;

                    // Encrypt the string to an array of bytes.
                    UtilityConstruct.funcEncryptStringToBytes_AES(inputstring, myRijndael.Key, myRijndael.IV);
                    byte[] encrypted = UtilityConstruct.funcEncryptStringToBytes_AES(inputstring, myRijndael.Key, myRijndael.IV);
                    //funcWriteToLog(encrypted.Length.ToString());

                    File.WriteAllBytes(outputfile, encrypted);

                    //funcWriteToLog("inside PerformEncryption: " + outputfile + " creation time: " + File.GetCreationTime(outputfile));
                    //funcWriteToLog("inside PerformEncryption: " + outputfile + " last write time: " + File.GetLastWriteTime(outputfile));
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Error: {0}", e.Message);
                    funcWriteToLog("Please see the error log");
                    funcWriteToErrorLog("Error during encryption");
                    funcWriteToErrorLog(e.Message);
                }
            }
            else
            {
                File.AppendAllText(outputfile, inputstring);
            }
        }

        static void PerformDecryption(string inputfile, string outputfile, bool bDecrypt)
        {
            //string outputdatfile = outputprefix + ".dat";
            //string output_inputmirror = outputprefix + ".out";

            //string strLogMessage = "";

            //strLogMessage = "inside PerformEncryption, inputstring length: " + inputstring.Length.ToString();
            //funcWriteToLog(strLogMessage);
            //strLogMessage = "inside PerformEncryption, outputfile: " + outputfile;
            //funcWriteToLog(strLogMessage);

            if (bDecrypt)
            {
                try
                {
                    //string str = "YMeFP,Ury\\^U;Ch'G,pmdo%#&er:t0Op";
                    SecureString securekey = new SecureString();
                    securekey.AppendChar('Y');
                    securekey.AppendChar('M');
                    securekey.AppendChar('e');
                    securekey.AppendChar('F');
                    securekey.AppendChar('P');
                    securekey.AppendChar(',');
                    securekey.AppendChar('U');
                    securekey.AppendChar('r');
                    securekey.AppendChar('y');
                    securekey.AppendChar('\\');
                    securekey.AppendChar('^');
                    securekey.AppendChar('U');
                    securekey.AppendChar(';');
                    securekey.AppendChar('C');
                    securekey.AppendChar('h');
                    securekey.AppendChar('\'');
                    securekey.AppendChar('G');
                    securekey.AppendChar(',');
                    securekey.AppendChar('p');
                    securekey.AppendChar('m');
                    securekey.AppendChar('d');
                    securekey.AppendChar('o');
                    securekey.AppendChar('%');
                    securekey.AppendChar('#');
                    securekey.AppendChar('&');
                    securekey.AppendChar('e');
                    securekey.AppendChar('r');
                    securekey.AppendChar(':');
                    securekey.AppendChar('t');
                    securekey.AppendChar('0');
                    securekey.AppendChar('O');
                    securekey.AppendChar('p');
                    securekey.MakeReadOnly();
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    IntPtr ptr = Marshal.SecureStringToBSTR(securekey);
                    byte[] key = encoding.GetBytes(Marshal.PtrToStringUni(ptr));
                    securekey.Dispose();

                    //string str2 = "mOG+9s%$%\\;O+IDG";
                    SecureString secureiv = new SecureString();
                    secureiv.AppendChar('m');
                    secureiv.AppendChar('O');
                    secureiv.AppendChar('G');
                    secureiv.AppendChar('+');
                    secureiv.AppendChar('9');
                    secureiv.AppendChar('s');
                    secureiv.AppendChar('%');
                    secureiv.AppendChar('$');
                    secureiv.AppendChar('%');
                    secureiv.AppendChar('\\');
                    secureiv.AppendChar(';');
                    secureiv.AppendChar('O');
                    secureiv.AppendChar('+');
                    secureiv.AppendChar('I');
                    secureiv.AppendChar('D');
                    secureiv.AppendChar('G');
                    secureiv.MakeReadOnly();
                    System.Text.UTF8Encoding encoding2 = new System.Text.UTF8Encoding();
                    IntPtr ptr2 = Marshal.SecureStringToBSTR(secureiv);
                    byte[] iv16Bit = encoding2.GetBytes(Marshal.PtrToStringUni(ptr2));
                    secureiv.Dispose();

                    // Create a new instance of the RijndaelManaged
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    RijndaelManaged myRijndael = new RijndaelManaged();

                    myRijndael.Key = key;
                    myRijndael.IV = iv16Bit;
                    myRijndael.Padding = PaddingMode.PKCS7;

                    byte[] encrypted = File.ReadAllBytes(inputfile);
                    //Console.WriteLine(encrypted.Length.ToString());

                    // Decrypt the bytes to a string.
                    string roundtrip = UtilityConstruct.funcDecryptStringFromBytes_AES(encrypted, myRijndael.Key, myRijndael.IV);

                    byte[] outbytes = Convert.FromBase64String(roundtrip);

                    File.WriteAllBytes(outputfile, outbytes);
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Error: {0}", e.Message);
                    funcWriteToLog("Please see the error log");
                    funcWriteToErrorLog("Error during decryption on " + inputfile);
                    funcWriteToErrorLog(e.Message);
                }
            }
            else
            {
                //Console.WriteLine("No decryption was done.");
                funcWriteToLog("Decryption was not performed for " + inputfile);
            }
        }

        static ManagementObjectCollection funcSysQueryData(string sysQueryString, string sysServerName)
        {

            // [Comment] Connect to the server via WMI
            System.Management.ConnectionOptions objConnOptions = new System.Management.ConnectionOptions();
            string strServerNameforWMI = "\\\\" + sysServerName + "\\root\\cimv2";

            // [DebugLine] Console.WriteLine("Construct WMI scope...");
            System.Management.ManagementScope objManagementScope = new System.Management.ManagementScope(strServerNameforWMI, objConnOptions);

            // [DebugLine] Console.WriteLine("Construct WMI query...");
            System.Management.ObjectQuery objQuery = new System.Management.ObjectQuery(sysQueryString);
            //if (objQuery != null)
            //    Console.WriteLine("objQuery was created successfully");

            // [DebugLine] Console.WriteLine("Construct WMI object searcher...");
            System.Management.ManagementObjectSearcher objSearcher = new System.Management.ManagementObjectSearcher(objManagementScope, objQuery);
            //if (objSearcher != null)
            //    Console.WriteLine("objSearcher was created successfully");

            // [DebugLine] Console.WriteLine("Get WMI data...");

            System.Management.ManagementObjectCollection objReturnCollection = null;

            try
            {
                objReturnCollection = objSearcher.Get();
                return objReturnCollection;
            }
            catch (SystemException ex)
            {
                // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
                string strRPCUnavailable = "The RPC server is unavailable. (Exception from HRESULT: 0x800706BA)";
                // [DebugLine] System.Console.WriteLine(ex.Message);
                if (ex.Message == strRPCUnavailable)
                {
                    //Console.WriteLine("WMI: Server unavailable");
                    funcWriteToErrorLog("Check availability and firewall settings for: " + sysServerName);
                }

                // Next line will return an object that is equal to null
                return objReturnCollection;
            }
        }

        static void funcGatherSystemData(string strHostName, string outputfilename, bool bVerboseOutput, bool bEncryptData, string strServerHash)
        {
            try
            {
                // Win32_ComputerSystem
                // Win32_OperatingSystem
                // Win32_Processor
                // Win32_LogicalDisk
                // Win32_ComputerSystemProduct
                // Win32_BIOS
                // Win32_NetworkAdapter
                // Win32_NetworkAdapterConfiguration
                // Win32_Printer
                // Win32_Share
                // Win32_Service
                // Win32_ServerFeature (Windows Server 2008 and up only)
                // Win32_UserAccount
                // Win32_Group
                // Win32_Account
                // Win32_Volume

                // the following classes below have yet to be fully implemented
                // Win32_PrinterShare

                // The following class(es) will never be implemented
                //
                // Whenever Win32_Product is used the application state on the machine 
                //   can be changed inadvertently
                // Win32_Product *****NEVER USE*****
                //
                // Win32_PageFile - deprecated

                //funcWriteToErrorLog("inside funcGatherSystemData, outputfilename string: " + outputfilename);

                DateTime current = DateTime.Now;

                string dtFormat = "MMddyyyyHHmmss"; // for output file creation

                bool bServerAvailable = false;

                DataConstruct.sServer newServer = new DataConstruct.sServer();

                ManagementObjectCollection oQueryCollection = null;

                oQueryCollection = funcSysQueryData("select * from Win32_OperatingSystem", strHostName);
                if (oQueryCollection != null)
                {
                    bServerAvailable = true;
                }
                else
                {
                    if (bVerboseOutput)
                    {
                        Console.WriteLine();
                        Console.WriteLine(">>> No data to gather for: {0} ({1})", strHostName, current);
                        Console.WriteLine();
                    }

                    funcWriteToLog("Please see the error log.");
                    funcWriteToErrorLog("Error gathering data for " + strHostName);
                }

                // set this flag to write output to the console for troubleshooting
                bool bProgramDebug = false;

                if (bServerAvailable)
                {
                    funcWriteToLog("Gathering data for: " + strHostName);

                    oQueryCollection = null;
                    //Win32_ComputerSystem
                    oQueryCollection = funcSysQueryData("select * from Win32_ComputerSystem", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.ComputerSystem = funcGetComputerSystem(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i0");
                    }

                    oQueryCollection = null;
                    //Win32_Processor
                    oQueryCollection = funcSysQueryData("select * from Win32_Processor", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Processors = funcGetProcessors(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i1");
                    }

                    oQueryCollection = null;
                    //Win32_OperatingSystem
                    oQueryCollection = funcSysQueryData("select * from Win32_OperatingSystem", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.OperatingSystem = funcGetOperatingSystem(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i2");
                    }

                    oQueryCollection = null;
                    //Win32_LogicalDisk
                    oQueryCollection = funcSysQueryData("select * from Win32_LogicalDisk", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Disks = funcGetDisks(oQueryCollection);
                        if (strHostName == Environment.MachineName)
                        {
                            newServer.DriveRoots = funcGetDriveRoots(newServer.Disks.Length);
                            if (bProgramDebug)
                                Console.WriteLine("i3.1");
                            newServer.ProgramFilesRoots = funcGetProgramFilesRoots();
                            if (bProgramDebug)
                                Console.WriteLine("i3.2");
                        }
                    }

                    oQueryCollection = null;
                    //Win32_ComputerSystemProduct
                    oQueryCollection = funcSysQueryData("select * from Win32_ComputerSystemProduct", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.ComputerSystemProduct = funcGetComputerSystemProduct(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i4");
                    }

                    oQueryCollection = null;
                    //Win32_BIOS
                    oQueryCollection = funcSysQueryData("select * from Win32_BIOS", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.BIOS = funcGetBIOS(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i5");
                    }

                    oQueryCollection = null;
                    //Win32_NetworkAdapter
                    oQueryCollection = funcSysQueryData("select * from Win32_NetworkAdapter", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.NetworkAdapters = funcGetNetworkAdapter(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i6");
                    }

                    oQueryCollection = null;
                    //Win32_NetworkAdapterConfiguration
                    oQueryCollection = funcSysQueryData("select * from Win32_NetworkAdapterConfiguration", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.NetworkAdapterConfigurations = funcGetNetworkAdapterConfigurations(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i7");
                    }

                    oQueryCollection = null;
                    //Win32_Share
                    oQueryCollection = funcSysQueryData("select * from Win32_Share", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Shares = funcGetShares(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i8");
                        if (strHostName == Environment.MachineName)
                            newServer.SharePermissions = funcGatherSharePermissions(strHostName, newServer.Shares);
                        if (bProgramDebug)
                            Console.WriteLine("i8.2");
                    }

                    // Products
                    newServer.Products = funcGetProducts();
                    if (bProgramDebug)
                        Console.WriteLine("i9");

                    // ODBC
                    if (newServer.ComputerSystem["systemtype"].Trim() == "X86-based PC")
                    {
                        string strODBCINIPath = "Software\\ODBC\\ODBC.INI";
                        RegistryKey regODBCINI = Registry.LocalMachine.OpenSubKey(strODBCINIPath);
                        if (regODBCINI.SubKeyCount > 0)
                        {
                            string[] strSubKeyNames = regODBCINI.GetSubKeyNames();
                            bool bODBCDataSourcesKey = false;
                            foreach (string strSubKeyName in strSubKeyNames)
                            {
                                if (strSubKeyName == "ODBC Data Sources")
                                    bODBCDataSourcesKey = true;
                            }
                            if (bODBCDataSourcesKey)
                            {
                                string strODBCDataSourcesPath = "Software\\ODBC\\ODBC.INI\\ODBC Data Sources";
                                RegistryKey regODBCDataSources = Registry.LocalMachine.OpenSubKey(strODBCDataSourcesPath);
                                if (regODBCDataSources != null)
                                {
                                    if (regODBCDataSources.ValueCount > 0)
                                    {
                                        newServer.SystemDataSources = funcGetODBCDataSources_x86(regODBCDataSources.ValueCount);
                                        if (bProgramDebug)
                                            Console.WriteLine("i10-1");
                                    }
                                }
                            }
                        }
                    }
                    if (newServer.ComputerSystem["systemtype"].Trim() == "x64-based PC")
                    {
                        string strODBCINIPath = "Software\\ODBC\\ODBC.INI";
                        RegistryKey regODBCINI = _openSubKey(Registry.LocalMachine, strODBCINIPath, false, RegWow64Options.KEY_WOW64_64KEY);
                        if (regODBCINI.SubKeyCount > 0)
                        {
                            string[] strSubKeyNames = regODBCINI.GetSubKeyNames();
                            bool bODBCDataSourcesKey = false;
                            foreach (string strSubKeyName in strSubKeyNames)
                            {
                                if (strSubKeyName == "ODBC Data Sources")
                                    bODBCDataSourcesKey = true;
                            }
                            if (bODBCDataSourcesKey)
                            {
                                string strODBCDataSourcesPath = "Software\\ODBC\\ODBC.INI\\ODBC Data Sources";
                                RegistryKey regODBCDataSources = _openSubKey(Registry.LocalMachine, strODBCDataSourcesPath, false, RegWow64Options.KEY_WOW64_64KEY);
                                if (regODBCDataSources != null)
                                {
                                    if (regODBCDataSources.ValueCount > 0)
                                    {
                                        newServer.SystemDataSources = funcGetODBCDataSources_x64(regODBCDataSources.ValueCount);
                                        if (bProgramDebug)
                                            Console.WriteLine("i10-2");
                                    }
                                }
                            }
                        }
                    }

                    if (newServer.OperatingSystem["osproduct"].Contains("2008"))
                    {
                        oQueryCollection = null;
                        //Win32_ServerFeature
                        oQueryCollection = funcSysQueryData("select * from Win32_ServerFeature", strHostName);
                        if (oQueryCollection != null)
                        {
                            newServer.ServerFeatures = funcGetServerFeatures(oQueryCollection);
                            if (bProgramDebug)
                                Console.WriteLine("i11");
                        }
                    }

                    oQueryCollection = null;
                    //Win32_Service
                    oQueryCollection = funcSysQueryData("select * from Win32_Service", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Services = funcGetServices(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i12");
                    }

                    oQueryCollection = null;
                    //Win32_UserAccount
                    oQueryCollection = funcSysQueryData("select * from Win32_UserAccount Where LocalAccount = True", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.UserAccounts = funcGetUserAccounts(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i13");
                    }

                    oQueryCollection = null;
                    //Win32_Group
                    oQueryCollection = funcSysQueryData("select * from Win32_Group", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Groups = funcGetGroups(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i14");
                    }

                    oQueryCollection = null;
                    //Win32_Account
                    oQueryCollection = funcSysQueryData("select * from Win32_Account", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Accounts = funcGetAccounts(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i15");
                    }

                    oQueryCollection = null;
                    //Win32_Volume
                    if (!newServer.OperatingSystem["osproduct"].Contains("Windows XP"))
                        oQueryCollection = funcSysQueryData("select * from Win32_Volume", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Volumes = funcGetVolumes(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i16");
                    }

                    oQueryCollection = null;
                    //Win32_Printer
                    oQueryCollection = funcSysQueryData("select * from Win32_Printer", strHostName);
                    if (oQueryCollection != null)
                    {
                        newServer.Printers = funcGetPrinters(oQueryCollection);
                        if (bProgramDebug)
                            Console.WriteLine("i17");
                    }

                    // ---------------------------------------------------------------------------
                    // ---------------------------------------------------------------------------

                    //Console.WriteLine("Finish setting newServer");
                    newServer.sVersion = "s1.0.0";
                    newServer.strCustID = UtilityConstruct.funcGetsKey("SystemsDocument");
                    // SCAN0001-001 - Normal scan
                    // SCAN0001-002 - Planned for MSP scan
                    newServer.strScanID = "SCAN0001-001";
                    newServer.strServerHash = strServerHash;
                    newServer.strConfigScanDateTime = current.ToString(dtFormat);

                    string serverjson = JsonConvert.SerializeObject(newServer);

                    //Console.WriteLine(serverjson);
                    //Console.WriteLine();
                    //Console.WriteLine(serverjson.Length.ToString());
                    //File.WriteAllText("server.json", serverjson);

                    if (bEncryptData)
                    {
                        // bEncryptData = true
                        byte[] setupbytes = ASCIIEncoding.ASCII.GetBytes(serverjson);
                        string setupstring = Convert.ToBase64String(setupbytes);
                        PerformEncryption(setupstring, outputfilename, bEncryptData);
                    }
                    else
                    {
                        // bEncryptData = false
                        PerformEncryption(serverjson, outputfilename, bEncryptData);
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        } // GatherSystemData

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("SystemsDocument (c) 2016 systemsdocument.com");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following parameter:");
            Console.WriteLine("-run                Run SystemsDocument");
            Console.WriteLine();
            Console.WriteLine("Optionally use the following parameter:");
            Console.WriteLine("-v                  Verbose mode - show data gathering operations");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("SystemsDocument -run");
            Console.WriteLine("SystemsDocument -run -v");
        }

        static void funcWriteToErrorLog(string strMessage)
        {
            UtilityConstruct.funcWriteToErrorLog("SystemsDocumentError_" + Environment.MachineName + ".Log", strMessage);
        }

        static void funcWriteToEventLog(string strEventMsg, int intEventType)
        {
            UtilityConstruct.funcWriteToEventLog("SystemsDocument", strEventMsg, intEventType);
        }  

        static void funcWriteToLog(string strMessage)
        {
            UtilityConstruct.funcWriteToLog("SystemsDocument_" + Environment.MachineName + ".Log", strMessage);
        }

        static bool funcOSCheck()
        {
            bool bValidOS = false;
            string strOS = "";
            ManagementObjectCollection oQueryCollection = null;
            oQueryCollection = funcSysQueryData("select * from Win32_OperatingSystem", Environment.MachineName);
            foreach (ManagementObject oReturn in oQueryCollection)
            {
                strOS = oReturn["Caption"].ToString().Trim();
                if (strOS.Contains("Server"))
                {
                    if (strOS.Contains("2000") | strOS.Contains("2003") | strOS.Contains("2008"))
                        bValidOS = true;
                }
            }
            return bValidOS;
        }

        static void funcGetFuncCatchCode(string strFunctionName, Exception currentex)
        {
            string strCatchCode = "";

            Dictionary<string, string> dCatchTable = new Dictionary<string, string>();
            dCatchTable.Add("funcGatherSharePermissions", "f0");
            dCatchTable.Add("funcGatherSystemData", "f1");
            dCatchTable.Add("funcGetAccounts", "f2");
            dCatchTable.Add("funcGetBIOS", "f3");
            dCatchTable.Add("funcGetComputerSystem", "f4");
            dCatchTable.Add("funcGetComputerSystemProduct", "f5");
            dCatchTable.Add("funcGetDisks", "f6");
            dCatchTable.Add("funcGetDriveRoots", "f7");
            dCatchTable.Add("funcGetGroups", "f8");
            dCatchTable.Add("funcGetNetworkAdapter", "f9");
            dCatchTable.Add("funcGetNetworkAdapterConfigurations", "f10");
            dCatchTable.Add("funcGetODBCDataSources_x64", "f11");
            dCatchTable.Add("funcGetODBCDataSources_x86", "f12");
            dCatchTable.Add("funcGetOperatingSystem", "f13");
            dCatchTable.Add("funcGetPrinters", "f14");
            dCatchTable.Add("funcGetProcessors", "f15");
            dCatchTable.Add("funcGetProducts", "f16");
            dCatchTable.Add("funcGetProgramFilesRoots", "f17");
            dCatchTable.Add("funcGetRegistryData", "f18");
            dCatchTable.Add("funcGetServerFeatures", "f19");
            dCatchTable.Add("funcGetServices", "f20");
            dCatchTable.Add("funcGetSharePermissions", "f21");
            dCatchTable.Add("funcGetShares", "f22");
            dCatchTable.Add("funcGetUserAccounts", "f23");
            dCatchTable.Add("funcGetVolumes", "f24");
            dCatchTable.Add("funcOSCheck", "f25");
            dCatchTable.Add("funcPDFRetrieve", "f26");
            dCatchTable.Add("funcPrintParameterSyntax", "f27");
            dCatchTable.Add("funcRun", "f28");
            dCatchTable.Add("funcSDParseArgs", "f29");
            dCatchTable.Add("funcStatusCheck", "f30");
            dCatchTable.Add("funcSysQueryData", "f31");
            dCatchTable.Add("funcWriteToErrorLog", "f32");
            dCatchTable.Add("funcWriteToEventLog", "f33");
            dCatchTable.Add("funcWriteToLog", "f34");
            dCatchTable.Add("PerformDecryption", "f35");
            dCatchTable.Add("PerformEncryption", "f36");

            if (dCatchTable.ContainsKey(strFunctionName))
            {
                strCatchCode = "err" + dCatchTable[strFunctionName] + ": ";
            }

            //[DebugLine] Console.WriteLine(strCatchCode + currentex.GetType().ToString());
            //[DebugLine] Console.WriteLine(strCatchCode + currentex.Message);
            funcWriteToErrorLog(strCatchCode + currentex.GetType().ToString());
            funcWriteToErrorLog(strCatchCode + currentex.Message);
            funcWriteToLog("Please see the error log.");
        }

        static bool funcSupportFilesCheck()
        {
            bool bSupportFiles = false;
            int intSupportFiles = 0;

            if(File.Exists("Newtonsoft.Json.dll"))
                intSupportFiles++;
            if (File.Exists("SystemsDocument.Library.dll"))
                intSupportFiles++;
            if (File.Exists("SystemsDocument.Utility.dll"))
                intSupportFiles++;

            if (intSupportFiles == 3)
                bSupportFiles = true;

            return bSupportFiles;
        }

        //Program Main
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                UtilityConstruct.funcPrintParameterWarning("SystemsDocument");
                funcWriteToErrorLog("Missing parameter(s). Run SystemsDocument -?");
            }
            else
            {
                if (args[0] == "-?")
                {
                    funcPrintParameterSyntax();
                }
                else
                {
                    bool bOSCheck = funcOSCheck();
                    bool bSupportFilesCheck = funcSupportFilesCheck();
                    if (bOSCheck)
                    {
                        if (bSupportFilesCheck)
                        {
                            if (File.Exists("SystemsDocument.skey"))
                            {
                                funcSDParseArgs(args);
                            }
                            else
                            {
                                funcWriteToErrorLog("SystemsDocument.skey does not exist");
                            }
                        }
                        else
                        {
                            funcWriteToErrorLog("All files needed to run SystemsDocument are not present.");
                            funcWriteToErrorLog("Verify files are present or re-install SystemsDocument.");
                        }
                    }
                    else
                    {
                        funcWriteToErrorLog("This operating system is not supported.");
                    }
                }
            }

        } // Main function

    } // class SDMain
}
