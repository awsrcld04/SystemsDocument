// Copyright (C) 2025 Akil Woolfolk Sr. 
// All Rights Reserved
// All the changes released under the MIT license as the original code.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SystemsDocument.Utility
{
    public class sKeyException : Exception
    {
        public sKeyException() 
        { 
        }
        public sKeyException(string message) 
            : base(message) 
        { 
        }
        public sKeyException(string message, Exception inner)
            : base(message, inner)
        { 
        }
    }

    public class UtilityConstruct
    {
        //static ManagementObjectCollection funcSysQueryData(string sysQueryString, string sysServerName)
        //{

        //    // [Comment] Connect to the server via WMI
        //    System.Management.ConnectionOptions objConnOptions = new System.Management.ConnectionOptions();
        //    string strServerNameforWMI = "\\\\" + sysServerName + "\\root\\cimv2";

        //    // [DebugLine] Console.WriteLine("Construct WMI scope...");
        //    System.Management.ManagementScope objManagementScope = new System.Management.ManagementScope(strServerNameforWMI, objConnOptions);

        //    // [DebugLine] Console.WriteLine("Construct WMI query...");
        //    System.Management.ObjectQuery objQuery = new System.Management.ObjectQuery(sysQueryString);
        //    //if (objQuery != null)
        //    //    Console.WriteLine("objQuery was created successfully");

        //    // [DebugLine] Console.WriteLine("Construct WMI object searcher...");
        //    System.Management.ManagementObjectSearcher objSearcher = new System.Management.ManagementObjectSearcher(objManagementScope, objQuery);
        //    //if (objSearcher != null)
        //    //    Console.WriteLine("objSearcher was created successfully");

        //    // [DebugLine] Console.WriteLine("Get WMI data...");

        //    System.Management.ManagementObjectCollection objReturnCollection = null;

        //    try
        //    {
        //        objReturnCollection = objSearcher.Get();
        //        return objReturnCollection;
        //    }
        //    catch (SystemException ex)
        //    {
        //        // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
        //        string strRPCUnavailable = "The RPC server is unavailable. (Exception from HRESULT: 0x800706BA)";
        //        // [DebugLine] System.Console.WriteLine(ex.Message);
        //        if (ex.Message == strRPCUnavailable)
        //        {
        //            //Console.WriteLine("WMI: Server unavailable");
        //            funcWriteToErrorLog("Check availability and firewall settings for: " + sysServerName);
        //        }

        //        // Next line will return an object that is equal to null
        //        return objReturnCollection;
        //    }
        //}

        //static string[] funcBuildServerList(int strSourceFlag)
        //{
        //    if (strSourceFlag == 1)
        //    {
        //        funcWriteToLog("Searching Directory Services as source for target server(s)");

        //        string strFilterAllServers = "(&(&(&(sAMAccountType=805306369)(objectCategory=computer)(|(operatingSystem=Windows Server 2008*)(operatingSystem=Windows Server 2003*)(operatingSystem=Windows 2000 Server*)(operatingSystem=Windows NT*)(operatingSystem=*2008*)))))";

        //        // [Comment] Get local domain context
        //        string rootDSE;

        //        System.DirectoryServices.DirectorySearcher objrootDSESearcher = new System.DirectoryServices.DirectorySearcher();
        //        rootDSE = objrootDSESearcher.SearchRoot.Path;
        //        // [DebugLine]Console.WriteLine(rootDSE);

        //        // [Comment] Construct DirectorySearcher object using rootDSE string
        //        System.DirectoryServices.DirectoryEntry objrootDSEentry = new System.DirectoryServices.DirectoryEntry(rootDSE);
        //        System.DirectoryServices.DirectorySearcher objComputerObjectSearcher = new System.DirectoryServices.DirectorySearcher(objrootDSEentry);
        //        // [DebugLine]Console.WriteLine(objComputerObjectSearcher.SearchRoot.Path);

        //        // [Comment] Add filter to DirectorySearcher object
        //        objComputerObjectSearcher.Filter = (strFilterAllServers);

        //        // [Comment] Execute query, return results, display name and path values
        //        System.DirectoryServices.SearchResultCollection objComputerResults = objComputerObjectSearcher.FindAll();
        //        // [DebugLine]Console.WriteLine(objComputerResults.Count.ToString());

        //        string[] arrADFileServers = new string[objComputerResults.Count];

        //        // string objComputerDEvalues;
        //        string objComputerNameValue;
        //        int intStrPosFirst = 3;
        //        int intStrPosLast;
        //        int intADFileServers = 0;

        //        foreach (System.DirectoryServices.SearchResult objComputer in objComputerResults)
        //        {
        //            System.DirectoryServices.DirectoryEntry objComputerDE = new System.DirectoryServices.DirectoryEntry(objComputer.Path);
        //            intStrPosLast = objComputerDE.Name.Length;
        //            objComputerNameValue = objComputerDE.Name.Substring(intStrPosFirst, intStrPosLast - intStrPosFirst);
        //            arrADFileServers[intADFileServers] = objComputerNameValue;
        //            intADFileServers++;
        //        }

        //        //Console.WriteLine(intADFileServers.ToString());
        //        funcWriteToLog("Number of servers: " + intADFileServers.ToString());

        //        return arrADFileServers;
        //    }
        //    else if (strSourceFlag == 2)
        //    {
        //        // string strLicenseString = "";
        //        // bool bValidLicense = false;


        //        try
        //        {
        //            funcWriteToLog("Using file sdservers.txt as source for target server(s)");

        //            if (File.Exists("sdservers.txt"))
        //            {
        //                string[] arrTxtFileServers = File.ReadAllLines("sdservers.txt");
        //                funcWriteToLog("Number of servers: " + arrTxtFileServers.Length.ToString());
        //                return arrTxtFileServers;
        //            }
        //            else
        //            {
        //                string[] arrTxtFileServers = { "<exception>" };
        //                return arrTxtFileServers;
        //            }

        //        } // end of try block on new StreamReader("sotfwlic.dat")

        //        catch (System.Exception ex)
        //        {
        //            // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());

        //            // [DebugLine] System.Console.WriteLine(ex.Message);

        //            if (ex.Message.StartsWith("Could not find file"))
        //            {
        //                Console.WriteLine("Input file not found.");
        //            }
        //            string[] arrException = new string[2];
        //            arrException[0] = "<exception>";
        //            return arrException;

        //        } // end of catch block on new StreamReader("sdservers.txt")
        //    }
        //    else if (strSourceFlag == 3)
        //    {
        //        funcWriteToLog("Gathering data from the Localhost only");

        //        string[] arrLocalhost = new string[1];
        //        arrLocalhost[0] = Environment.MachineName;
        //        return arrLocalhost;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public static string funcDecryptStringFromBytes_AES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        public static byte[] funcEncryptStringToBytes_AES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encrypto to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return msEncrypt.ToArray();
        }

        public static string funcGetServerHashRIPEMD160(string strServerName)
        {
            string strServerHash = "";

            RIPEMD160 ripemd160ServerHash = RIPEMD160.Create();

            byte[] tmpripemd160ServerHash;
            tmpripemd160ServerHash = ripemd160ServerHash.ComputeHash(Encoding.ASCII.GetBytes(strServerName));

            int i;
            StringBuilder sOutput = new StringBuilder(tmpripemd160ServerHash.Length);
            for (i = 0; i < tmpripemd160ServerHash.Length; i++)
            {
                sOutput.Append(tmpripemd160ServerHash[i].ToString("X2"));
            }

            strServerHash = sOutput.ToString();

            return strServerHash;
        }

        public static string funcGetsKey(string strProgramName)
        {
            string skey = String.Empty;

            if (File.Exists(strProgramName + ".skey"))
            {
                skey = File.ReadAllText(strProgramName + ".skey");
                if (skey.Length != 64)
                {
                    string strErrorMessage = strProgramName + ".skey" + " is incorrect.";
                    sKeyException ex = new sKeyException(strErrorMessage);
                    throw ex;
                }
            }
            else
            {
                string strErrorMessage = strProgramName + ".skey" + " does not exist.";
                sKeyException ex = new sKeyException(strErrorMessage);
                throw ex;
            }

            return skey;
        }

        public static string funcHttpFileDownload(string URI, string filename)
        {
            WebClient fileWebClient = new WebClient();

            fileWebClient.DownloadFile(URI, filename);

            if (File.Exists(filename))
            {
                return "DownloadOK";
            }
            else
            {
                return "DownloadFailed";
            }
        }

        public static string funcHttpFileUpload(string URI, string filename, string strUserAgent)
        {
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(URI);
            //req.Proxy = new System.Net.WebProxy(ProxyString, true);

            //Add these, as we're doing a POST
            //req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "*/*";
            //req.ContentType = "application/octet-stream";
            req.Method = "PUT";
            //req.UserAgent = "SystemsDocument 1.0";
            req.UserAgent = strUserAgent;

            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);

            byte[] bytes = File.ReadAllBytes(filename);

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

        public static string funcHttpPutRequest(string URI, string strData, string strUserAgent)
        {
            System.Net.HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(URI);
            //req.Proxy = new System.Net.WebProxy(ProxyString, true);

            //Add these, as we're doing a POST
            //req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "*/*";
            //req.ContentType = "application/octet-stream";
            req.Method = "PUT";
            req.UserAgent = strUserAgent;

            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(strData);

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

        public static void funcPrintParameterWarning(string strProgramName)
        {
            Console.WriteLine("Check parameters.");
            Console.WriteLine("Run " + strProgramName + " -? to get the parameter syntax.");
        }

        public static void funcWriteToErrorLog(string strLogFileName, string strMessage)
        {
            DateTime current = DateTime.Now;

            string dtFormat = "MM/d/yyyy HH:mm:ss"; // for output file creation

            File.AppendAllText(strLogFileName, current.ToString(dtFormat) + "\t" + strMessage + "\r\n");
        }

        public static void funcWriteToEventLog(string strAppName, string strEventMsg, int intEventType)
        {
            string strLogName;

            strLogName = "Application";

            if (!EventLog.SourceExists(strAppName))
                EventLog.CreateEventSource(strAppName, strLogName);

            //EventLog.WriteEntry(strAppName, strEventMsg);
            EventLog.WriteEntry(strAppName, strEventMsg, EventLogEntryType.Information, intEventType);
        }

        public static void funcWriteToLog(string strLogFileName, string strMessage)
        {
            DateTime current = DateTime.Now;

            string dtFormat = "MM/d/yyyy HH:mm:ss"; // for output file creation

            File.AppendAllText(strLogFileName, current.ToString(dtFormat) + "\t" + strMessage + "\r\n");
        }
    }
}
