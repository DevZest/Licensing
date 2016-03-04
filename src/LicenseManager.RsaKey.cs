using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Security;
using System.Security.Permissions;

namespace DevZest.Licensing
{
    partial class LicenseManager
    {
        [Obfuscation(Exclude=false)]
        private static class RsaKey
        {
            private enum MagicIndex
            {
                RSA1 = 20,
                RSA2 = 8
            }

            // Check that RSA1 is in header.
            //                                                R     S     A     1
            private static byte[] s_rsa1Check = new byte[] { 0x52, 0x53, 0x41, 0x31 };
            // Check that RSA2 is in header.
            //                                                R     S     A     2
            private static byte[] s_rsa2Check = new byte[] { 0x52, 0x53, 0x41, 0x32 };
            private const int MagicSize = 4;

            private static byte[] Copy(byte[] src, int index, int size)
            {
                if ((src == null) || (src.Length < (index + size)))
                    return null;

                byte[] dest = new byte[size];
                Array.Copy(src, index, dest, 0, size);
                return dest;
            }

            private static bool Check(byte[] bytes, MagicIndex magicIndex)
            {
                Debug.Assert(bytes != null);

                byte[] check = magicIndex == MagicIndex.RSA1 ? s_rsa1Check : s_rsa2Check;
                int index = (int)magicIndex;

                if (bytes.Length < index + check.Length)
                    return false;

                for (int i = 0; i < check.Length; i++)
                {
                    if (bytes[i + index] != check[i])
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Returns RSAParameters from byte[].
            /// Example to get rsa public key from assembly:
            /// byte[] pubkey = System.Reflection.Assembly.GetExecutingAssembly().GetName().GetPublicKey();
            /// RSAParameters p = SnkUtil.GetRSAParameters(pubkey);
            /// </summary>
            private static RSAParameters? GetRsaParameters(byte[] bytes)
            {
                Debug.Assert(bytes != null);

                bool publicOnly = (bytes.Length == 160);

                if (publicOnly && !Check(bytes, MagicIndex.RSA1))
                    return null;

                if (!publicOnly && !Check(bytes, MagicIndex.RSA2))
                    return null;

                RSAParameters parameters = new RSAParameters();

                int index = publicOnly ? (int)MagicIndex.RSA1 : (int)MagicIndex.RSA2;
                index += MagicSize + 4;
                int size = 4;
                parameters.Exponent = Copy(bytes, index, size);
                Array.Reverse(parameters.Exponent);

                index += size;
                size = 128;
                parameters.Modulus = Copy(bytes, index, size);
                Array.Reverse(parameters.Modulus);

                if (publicOnly)
                    return parameters;

                // Figure private params
                // Must reverse order (little vs. big endian issue)

                index += size;
                size = 64;
                parameters.P = Copy(bytes, index, size);
                Array.Reverse(parameters.P);

                index += size;
                size = 64;
                parameters.Q = Copy(bytes, index, size);
                Array.Reverse(parameters.Q);

                index += size;
                size = 64;
                parameters.DP = Copy(bytes, index, size);
                Array.Reverse(parameters.DP);

                index += size;
                size = 64;
                parameters.DQ = Copy(bytes, index, size);
                Array.Reverse(parameters.DQ);

                index += size;
                size = 64;
                parameters.InverseQ = Copy(bytes, index, size);
                Array.Reverse(parameters.InverseQ);

                index += size;
                size = 128;
                parameters.D = Copy(bytes, index, size);
                Array.Reverse(parameters.D);

                return parameters;
            }

            internal static string GetXmlString(byte[] bytes, bool includePrivateParameters)
            {
                if (bytes == null)
                    return null;

                RSAParameters? parameters = GetRsaParameters(bytes);
                if (!parameters.HasValue)
                    return null;

                StringBuilder builder = new StringBuilder();
                builder.Append("<RSAKeyValue>");
                builder.Append("<Modulus>" + Convert.ToBase64String(parameters.Value.Modulus) + "</Modulus>");
                builder.Append("<Exponent>" + Convert.ToBase64String(parameters.Value.Exponent) + "</Exponent>");
                if (includePrivateParameters)
                {
                    builder.Append("<P>" + Convert.ToBase64String(parameters.Value.P) + "</P>");
                    builder.Append("<Q>" + Convert.ToBase64String(parameters.Value.Q) + "</Q>");
                    builder.Append("<DP>" + Convert.ToBase64String(parameters.Value.DP) + "</DP>");
                    builder.Append("<DQ>" + Convert.ToBase64String(parameters.Value.DQ) + "</DQ>");
                    builder.Append("<InverseQ>" + Convert.ToBase64String(parameters.Value.InverseQ) + "</InverseQ>");
                    builder.Append("<D>" + Convert.ToBase64String(parameters.Value.D) + "</D>");
                }
                builder.Append("</RSAKeyValue>");
                return builder.ToString();
            }

            public static string XmlStringFromAssembly(string assemblyFile)
            {
                byte[] bytes = AssemblyName.GetAssemblyName(assemblyFile).GetPublicKey();
                return GetXmlString(bytes, false);
            }

            /// <summary>Returns RSA object from .snk key file.</summary>
            /// Path to snk file.
            /// <returns>RSACryptoServiceProvider</returns>
            public static string XmlStringFromSnkFile(string snkFilePath)
            {
                if (snkFilePath == null)
                    throw new ArgumentNullException("snkFilePath");

                byte[] bytes = GetFileBytes(snkFilePath);
                return GetXmlString(bytes, true);
            }

            public static string XmlStringFromSnkFile(Stream stream)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");

                byte[] bytes = GetStreamBytes(stream);
                return GetXmlString(bytes, true);
            }

            private static byte[] GetFileBytes(string path)
            {
                Stream stream = File.OpenRead(path);
                using (stream)
                {
                    return GetStreamBytes(stream);
                }
            }

            private static byte[] GetStreamBytes(Stream stream)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}
