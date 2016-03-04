/*
 *  RSA Public-Key encryption for the .Net framework
 *  Copyright (C) 2007, Paul Sanders.
 *  http://www.alpinesoft.co.uk
 *
 *  Based on original work in C by Christophe Devine, which is 
 *  Copyright (C) 2006, 2007, Christopher Devine, and is available from:
 *  http://xyssl.org/code/source/rsa/
 *
 *  Also requires Chew Keong TAN's most excellent BigInteger class:
 *  http://www.codeproject.com/csharp/BigInteger.asp
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License, version 2.1 as published by the Free Software Foundation.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 *  MA  02110-1301  USA
 *
 *  RSA was designed by Ron Rivest, Adi Shamir and Len Adleman, and is
 *  the cat's pyjamas as far as I am concerned:
 *  http://theory.lcs.mit.edu/~rivest/rsapaper.pdf
 *  http://www.cacr.math.uwaterloo.ca/hac/about/chap8.pdf
 */

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace DevZest.Licensing
{
    [Obfuscation(Exclude = false)]
    internal sealed partial class Rsa : AsymmetricAlgorithm
    {

        [Obfuscation(Exclude = false)]
        private enum HASH_ALGORITHM { RSA_RAW = 0, RSA_SHA1 = 1, RSA_MD2 = 2, RSA_MD4 = 4, RSA_MD5 = 5 };

        public Rsa(string xml)
        {
            LegalKeySizesValue = new KeySizes[1];
            Init(xml);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void Init(string xmlString)
        {
            using (StringReader sr = new StringReader(xmlString))
            {
                using (XmlTextReader reader = new XmlTextReader(sr))
                {
                    this.N = this.E = this.P = this.Q = this.DP = this.DQ = this.QP = this.D = null;

                    for (; ; )
                    {
                        XmlNodeType node_type = reader.MoveToContent();
                        switch (node_type)
                        {
                            case XmlNodeType.Element:
                                {
                                    string element_name = reader.Name;
                                    if (!CheckXmlElement(reader, element_name, "Modulus", ref this.N) &&
                                        !CheckXmlElement(reader, element_name, "Exponent", ref this.E) &&
                                        !CheckXmlElement(reader, element_name, "P", ref this.P) &&
                                        !CheckXmlElement(reader, element_name, "Q", ref this.Q) &&
                                        !CheckXmlElement(reader, element_name, "DP", ref this.DP) &&
                                        !CheckXmlElement(reader, element_name, "DQ", ref this.DQ) &&
                                        !CheckXmlElement(reader, element_name, "InverseQ", ref this.QP) &&
                                        !CheckXmlElement(reader, element_name, "D", ref this.D))
                                    {
                                        reader.ReadString();
                                    }
                                    break;
                                }

                            case XmlNodeType.EndElement:
                                reader.ReadEndElement();
                                break;

                            case XmlNodeType.None:
                                return;

                            default:
                                throw new ArgumentException("something unexpected in xmlString", "xmlString");
                        }
                    }
                }
            }
        }

        public override void FromXmlString(string xmlString)
        {
            Init(xmlString);
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<RSAKeyValue>");
            builder.Append("<Modulus>" + BigIntToB64(N) + "</Modulus>");
            builder.Append("<Exponent>" + BigIntToB64(E) + "</Exponent>");
            if (includePrivateParameters)
            {
                builder.Append("<P>" + BigIntToB64(P) + "</P>");
                builder.Append("<Q>" + BigIntToB64(Q) + "</Q>");
                builder.Append("<DP>" + BigIntToB64(DP) + "</DP>");
                builder.Append("<DQ>" + BigIntToB64(DQ) + "</DQ>");
                builder.Append("<InverseQ>" + BigIntToB64(QP) + "</InverseQ>");
                builder.Append("<D>" + BigIntToB64(D) + "</D>");
            }
            builder.Append("</RSAKeyValue>");
            return builder.ToString();
        }

        // Perform an RSA private key operation on input
        private byte[] DoPublic(byte[] input)
        {
            Debug.Assert(input.Length == this.KeyBytes, "input.Length does not match KeyBytes");

            if (ReferenceEquals(this.N, null))
                throw new ArgumentException("no key set!");

            BigInteger T = new BigInteger(input);
            if (T >= this.N)
                throw new ArgumentException("input exceeds modulus");

            T = T.modPow(this.E, this.N);

            byte[] b = T.GetBytes();
            return PadBytes(b, this.KeyBytes);
        }


        // Perform an RSA private key operation on input
        private byte[] DoPrivate(byte[] input)
        {
            Debug.Assert(input.Length == this.KeyBytes, "input.Length does not match KeyBytes");

            if (ReferenceEquals(this.D, null))
                throw new ArgumentException("no private key set!");

            BigInteger T = new BigInteger(input);
            if (T >= this.N)
                throw new ArgumentException("input exceeds modulus");

            T = T.modPow(this.D, this.N);

            byte[] b = T.GetBytes();
            return PadBytes(b, this.KeyBytes);
        }

        // Encrypt a message and pack it up into PKCS#1 v1.5 format
        // Plug compatible with RsaCryptoServiceProvider.Encrypt
        public byte[] Encrypt(byte[] input)
        {
            int input_len = input.Length;
            int n_pad = this.KeyBytes - 3 - input_len;
            if (n_pad < 11)
                throw new ArgumentException("input too long");

            byte[] encrypt_me = new byte[this.KeyBytes];
            encrypt_me[0] = 0;
            encrypt_me[1] = RSA_CRYPT;

            byte[] padding = new byte[n_pad];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(padding);

            for (int i = 0; i < n_pad; ++i)             // padding bytes must not be zero
            {
                if (padding[i] == 0)
                    padding[i] = (byte)i;
            }

            Array.Copy(padding, 0, encrypt_me, 2, n_pad);
            encrypt_me[n_pad + 2] = 0;
            Array.Copy(input, 0, encrypt_me, n_pad + 3, input_len);

            return DoPublic(encrypt_me);
        }


        // Decrypt a message in PKCS#1 v1.5 format
        // Plug compatible with RsaCryptoServiceProvider.Decrypt
        public byte[] Decrypt(byte[] input)
        {
            byte[] decrypted = DoPrivate(input);

            if (decrypted[0] != 0 || decrypted[1] != RSA_CRYPT)
                throw new ArgumentException("invalid signature bytes");

            int decrypted_len = decrypted.Length;               // = KeyBytes
            for (int i = 2; i < decrypted_len - 1; ++i)
            {
                if (decrypted[i] == 0)
                {
                    ++i;
                    int output_len = decrypted_len - i;
                    byte[] output = new byte[output_len];
                    Array.Copy(decrypted, i, output, 0, output_len);
                    return output;
                }
            }

            throw new ArgumentException("invalid padding");
        }

        // Sign a message digest and pack it up into PKCS#1 format
        private byte[] SignHash(byte[] sign_me, HASH_ALGORITHM hash_algorithm)
        {
            int input_len = sign_me.Length;

            int n_pad = 0;
            switch (hash_algorithm)
            {
                case HASH_ALGORITHM.RSA_RAW:
                    n_pad = this.KeyBytes - 3 - input_len;
                    break;

                case HASH_ALGORITHM.RSA_MD2:
                case HASH_ALGORITHM.RSA_MD4:
                case HASH_ALGORITHM.RSA_MD5:
                    if (input_len != 16)
                        throw new ArgumentException("MDx hashes must be 16 bytes long");
                    n_pad = this.KeyBytes - 3 - 34;
                    break;

                case HASH_ALGORITHM.RSA_SHA1:
                    if (input_len != 20)
                        throw new ArgumentException("SHA1 hashes must be 20 bytes long");
                    n_pad = this.KeyBytes - 3 - 35;
                    break;
            }

            if (n_pad < 8)
                throw new ArgumentException("input too long");

            byte[] encrypt_me = new byte[this.KeyBytes];
            encrypt_me[0] = 0;
            encrypt_me[1] = RSA_SIGN;

            for (int i = 0; i < n_pad; ++i)
                encrypt_me[i + 2] = 0xFF;

            encrypt_me[n_pad + 2] = 0;

            switch (hash_algorithm)
            {
                case HASH_ALGORITHM.RSA_RAW:
                    Array.Copy(sign_me, 0, encrypt_me, n_pad + 3, input_len);
                    break;

                case HASH_ALGORITHM.RSA_MD2:
                case HASH_ALGORITHM.RSA_MD4:
                case HASH_ALGORITHM.RSA_MD5:
                    Array.Copy(ASN1_HASH_MDX, 0, encrypt_me, n_pad + 3, 18);
                    encrypt_me[n_pad + 3 + 13] = (byte)hash_algorithm;
                    Array.Copy(sign_me, 0, encrypt_me, n_pad + 3 + 18, input_len);
                    break;

                case HASH_ALGORITHM.RSA_SHA1:
                    Array.Copy(ASN1_HASH_SHA1, 0, encrypt_me, n_pad + 3, 15);
                    Array.Copy(sign_me, 0, encrypt_me, n_pad + 3 + 15, input_len);
                    break;
            }

            return DoPrivate(encrypt_me);
        }

        // Verify a signed PKCS#1 message digest
        private bool VerifyHash(byte[] hash, byte[] signature, HASH_ALGORITHM hash_algorithm)
        {
            int sig_len = signature.Length;
            if (sig_len != this.KeyBytes)
                return false;

            byte[] decrypted = DoPublic(signature);
            if (decrypted[0] != 0 || decrypted[1] != RSA_SIGN)
                return false;

            int decrypted_len = decrypted.Length;           // = KeyBytes

            for (int i = 2; i < decrypted_len - 1; ++i)
            {
                byte b = decrypted[i];
                if (b == 0)                                 // end of padding
                {
                    ++i;
                    int bytes_left = decrypted_len - i;

                    if (bytes_left == 34)                   // MDx
                    {
                        if (decrypted[i + 13] != (byte)hash_algorithm)
                            return false;
                        decrypted[i + 13] = 0;
                        if (!CompareBytes(decrypted, i, ASN1_HASH_MDX, 0, 18))
                            return false;
                        return CompareBytes(decrypted, i + 18, hash, 0, 16);
                    }

                    if (bytes_left == 35 && hash_algorithm == HASH_ALGORITHM.RSA_SHA1)
                    {
                        if (!CompareBytes(decrypted, i, ASN1_HASH_SHA1, 0, 15))
                            return false;
                        return CompareBytes(decrypted, i + 15, hash, 0, 20);
                    }

                    if (bytes_left == hash.Length && hash_algorithm == HASH_ALGORITHM.RSA_RAW)
                        return CompareBytes(decrypted, i, hash, 0, bytes_left);

                    return false;
                }

                if (b != 0xFF)
                    break;
            }

            return false;
        }


        // SignData - plug compatible with RsaCryptoServiceProvider.SignData,
        // but only this one override provided
        public byte[] SignData(byte[] data, HashAlgorithm hasher)
        {
            HASH_ALGORITHM ha = MapHashAlgorithm(hasher);
            byte[] hash = hasher.ComputeHash(data);
            byte[] signed_hash = SignHash(hash, ha);
            return signed_hash;
        }


        // VerifyData - plug compatible with RsaCryptoServiceProvider.VerifyData
        public bool VerifyData(byte[] data, HashAlgorithm hasher, byte[] signature)
        {
            HASH_ALGORITHM ha = MapHashAlgorithm(hasher);
            byte[] hash = hasher.ComputeHash(data);
            return VerifyHash(hash, signature, ha);
        }


        // Required by AssymetricAlgorithm base class
        public override string KeyExchangeAlgorithm
        {
            get { return "RSA-PKCS1-KeyEx"; }
        }


        // Required by AssymetricAlgorithm base class
        public override string SignatureAlgorithm
        {
            get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
        }

        // ----------------------------------------------------------------------------
        // Protected methods

        // Required by AssymetricAlgorithm base class
        protected override void Dispose(bool disposing)
        {
            this.N = this.E = this.P = this.Q = this.DP = this.DQ = this.QP = this.D = null;            
        }


        // ----------------------------------------------------------------------------
        // Private methods

        // Compare byte arrays
        private static bool CompareBytes(byte[] b1, int i1, byte[] b2, int i2, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                if (b1[i + i1] != b2[i + i2])
                    return false;
            }

            return true;
        }


        // pad b with leading 0's to make it n bytes long
        private static byte[] PadBytes(byte[] b, int n)
        {
            int len = b.Length;
            if (len >= n)
                return b;

            byte[] result = new byte[n];
            int padding = n - len;
            for (int i = 0; i < padding; ++i)
                result[i] = 0;
            Array.Copy(b, 0, result, padding, len);
            return result;
        }


        // Convert a big integer to a base 64 string
        private static string BigIntToB64(BigInteger bi)
        {
            byte[] b = bi.GetBytes();
            return Convert.ToBase64String(b);
        }


        // Initialise bi from XML element if element_name matches
        // Used by FromXmlString
        private bool CheckXmlElement(XmlReader reader, string element_name, string element_name_required, ref BigInteger bi_out)
        {
            if (element_name != element_name_required)
                return false;

            string s = reader.ReadString();
            byte[] b = Convert.FromBase64String(s);
            if (element_name == "Modulus")
            {
                KeySizeValue = b.Length * 8;
                LegalKeySizesValue[0] = new KeySizes(KeySizeValue, KeySizeValue, 0);
            }
            BigInteger bi = new BigInteger(b);
            bi_out = bi;
            return true;
        }


        // Map a HashAlgorithm object to our HASH_ALGORITHM enumeration
        private static HASH_ALGORITHM MapHashAlgorithm(HashAlgorithm hasher)
        {
            Type t = hasher.GetType();

            if (Object.ReferenceEquals(t, typeof(MD5CryptoServiceProvider)))
                return HASH_ALGORITHM.RSA_MD5;
            if (Object.ReferenceEquals(t, typeof(SHA1CryptoServiceProvider)))
                return HASH_ALGORITHM.RSA_SHA1;
            throw new ArgumentException("unknown HashAlgorithm");
        }


        // ----------------------------------------------------------------------------
        // Instance data and constants

        private const int RSA_SIGN = 0x01;
        private const int RSA_CRYPT = 0x02;

        private static byte[] ASN1_HASH_MDX =
        { 0x30, 0x20, 0x30, 0x0C, 0x06, 0x08, 0x2A, 0x86, 0x48,
          0x86, 0xF7, 0x0D, 0x02, 0x00, 0x05, 0x00, 0x04, 0x10 };
        private static byte[] ASN1_HASH_SHA1 =
        { 0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03,
          0x02, 0x1A, 0x05, 0x00, 0x04, 0x14 };

        private int KeyBytes
        {
            get { return KeySize / 8; }
        }

        private BigInteger N;
        private BigInteger E;
        private BigInteger P;
        private BigInteger Q;
        private BigInteger DP;
        private BigInteger DQ;
        private BigInteger QP;
        private BigInteger D;
    }
}