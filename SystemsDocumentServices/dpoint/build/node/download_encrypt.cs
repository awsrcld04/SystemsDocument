using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Security;

namespace downloadencrypt
{
    class DEMain
    {
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
                    //TextReader tr = File.OpenText(inputfile);
                    //string sdfile = tr.ReadToEnd();
                    //tr.Close();
                    //string original = "Here is some data to encrypt!";

                    //byte[] key = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
                    //              15, 16, 17, 18, 19, 20, 21, 22, 23, 24};

                    //byte[] iv16Bit = { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 };

                    string str = "YMeFP,Ury\\^U;Ch'G,pmdo%#&er:t0Op";
                    //SecureString securekey = new SecureString();
                    //foreach (char k in "YMeFP,Ury\\^U;Ch'G,pmdo%#&er:t0Op")
                    //{
                    //    securekey.AppendChar(k);
                    //}
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    byte[] key = encoding.GetBytes(str);
                    //funcWriteToErrorLog(str.Length.ToString());
                    str = "00000000000000000000000000000000000000000000000000000000";

                    string str2 = "mOG+9s%$%\\;O+IDG";
                    //SecureString secureiv = new SecureString();
                    //foreach (char i in "mOG+9s%$%\\;O+IDG")
                    //{
                    //    secureiv.AppendChar(i);
                    //}
                    System.Text.UTF8Encoding encoding2 = new System.Text.UTF8Encoding();
                    byte[] iv16Bit = encoding2.GetBytes(str2);
                    //funcWriteToErrorLog(encoding2.GetByteCount(str2).ToString());
                    str2 = "00000000000000000000000000000000000000000000000000000000";

                    // Create a new instance of the RijndaelManaged
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    RijndaelManaged myRijndael = new RijndaelManaged();

                    myRijndael.Key = key;
                    myRijndael.IV = iv16Bit;
                    myRijndael.Padding = PaddingMode.PKCS7;

                    // Encrypt the string to an array of bytes.
                    //byte[] encrypted = encryptStringToBytes_AES(original, myRijndael.Key, myRijndael.IV);
                    byte[] encrypted = encryptStringToBytes_AES(inputstring, myRijndael.Key, myRijndael.IV);
                    //funcWriteToLog(encrypted.Length.ToString());

                    // string enc_out = Convert.ToBase64String(encrypted);

                    File.WriteAllBytes(outputfile, encrypted);

                    //FileStream fsOut = new FileStream(outputfile, FileMode.OpenOrCreate, FileAccess.Write);
                    //fsOut.Write(encrypted, 0, encrypted.Length - 1);
                    //fsOut.Close();

                    //funcWriteToLog("inside PerformEncryption: " + outputfile + " creation time: " + File.GetCreationTime(outputfile));
                    //funcWriteToLog("inside PerformEncryption: " + outputfile + " last write time: " + File.GetLastWriteTime(outputfile));

                    //File.WriteAllBytes(outputdatfile, encrypted);

                    //byte[] newencryp = File.ReadAllBytes(outputdatfile);
                    //Console.WriteLine(newencryp.Length.ToString());

                    // Decrypt the bytes to a string.
                    //string roundtrip = decryptStringFromBytes_AES(encrypted, myRijndael.Key, myRijndael.IV);

                    //File.WriteAllText(output_inputmirror, roundtrip);

                    //Display the original data and the decrypted data.
                    //Console.WriteLine("Original:   {0}", original);
                    //Console.WriteLine("Round Trip: {0}", roundtrip);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
            else
            {
                File.AppendAllText(outputfile, inputstring);
            }
        }

        static byte[] encryptStringToBytes_AES(string plainText, byte[] Key, byte[] IV)
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

        static void Main(string[] args)
        {
            byte[] inputbytes = File.ReadAllBytes(args[0]);

            //string inputstring = Encoding.Unicode.GetString(inputbytes);

            string inputstring = Convert.ToBase64String(inputbytes);

            PerformEncryption(inputstring, args[1], true);
        }


        
    } // class
} // namespace
