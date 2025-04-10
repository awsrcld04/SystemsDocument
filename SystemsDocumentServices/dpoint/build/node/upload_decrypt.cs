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

namespace uploaddecrypt
{
    class UDMain
    {
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

                    //string instring = File.ReadAllText(inputfile);

                    byte[] encrypted = File.ReadAllBytes(inputfile);
                    //Console.WriteLine(encrypted.Length.ToString());

                    // Decrypt the bytes to a string.
                    string roundtrip = decryptStringFromBytes_AES(encrypted, myRijndael.Key, myRijndael.IV);

                    byte[] outbytes = Convert.FromBase64String(roundtrip);

                    File.WriteAllBytes(outputfile, outbytes);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
            else
            {
                Console.WriteLine("No decryption was done.");
            }
        }

        static string decryptStringFromBytes_AES(byte[] cipherText, byte[] Key, byte[] IV)
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

        static void Main(string[] args)
        {
            PerformDecryption(args[0], args[1], true);
        }


        
    } // class
} // namespace
