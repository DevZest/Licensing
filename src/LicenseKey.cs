using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.Reflection;

namespace DevZest.Licensing
{
    /// <summary>Represents a license key that can be used to get <see cref="License" /> from <see cref="LicensePublisher" />.</summary>
    /// <remarks><para>A license key is a 25 digits string that can be used to get a <see cref="License" /> from <see cref="LicensePublisher" />.
    /// The first 24 digits are generated randomly and the last digit is checksum. Use <see cref="NewLicenseKey">NewLicenseKey</see> to generate
    /// a new license key; Use <see cref="ToString">ToString</see> method to convert <see cref="LicenseKey"/> into a string formatted as
    /// "xxxxx-xxxxx-xxxxx-xxxxx-xxxxx"; Use <see cref="CanConvertFrom">CanConvertFrom</see>to determine if the provided string has correct
    /// format and checksum.</para>
    /// <para><see cref="LicenseKey" /> is also used to encrypt the reponse sent from <see cref="LicensePublisher" /> to
    /// <see cref="LicenseClient" />.</para>
    /// <para>Since <see cref="LicenseKey" /> can be used to get <see cref="License" /> from <see cref="LicensePublisher" />, it should be
    /// stored as encrypted and never be displayed as clear text by your application.</para></remarks>
    public struct LicenseKey
    {
        private const int KeyLength = 25;
        private static Random s_random;
        private static char[] s_digits = new char[] { '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };
        private static char[] s_keyChars = new char[KeyLength];

        private string _key;

        /// <summary>Initializes a new instance of <see cref="LicenseKey" /> structure.</summary>
        /// <param name="key">The string representation of the key.</param>
        /// <exception cref="ArgumentException"><paramref name="key"/> is invalid.</exception>
        public LicenseKey(string key)
        {
            if (!CanConvertFrom(key))
                throw new ArgumentException(ExceptionMessages.FormatInvalidLicenseKey(key), "key");

            if (key.Length == KeyLength + 4)
                key = key.Substring(0, 5) + key.Substring(6, 5) + key.Substring(12, 5) + key.Substring(18, 5) + key.Substring(24, 5);

            _key = key;
        }

        /// <summary>Gets an empty <see cref="LicenseKey" />.</summary>
        /// <value>An empty <see cref="LicenseKey" />.</value>
        public static LicenseKey Empty
        {
            get { return new LicenseKey(); }
        }

        /// <summary>Determines whether this license key is empty.</summary>
        /// <value><see langword="true" /> if this license key is empty. Otherwise <see langword="false"/>.</value>
        public bool IsEmpty
        {
            get { return _key == null; }
        }

        /// <summary>Initializes a new instance of <see cref="LicenseKey" />.</summary>
        /// <returns>A new <see cref="LicenseKey"/> object.</returns>
        public static LicenseKey NewLicenseKey()
        {
            string strKey = GenerateKey();

            while (TripleDES.IsWeakKey(GetTripleDesKey(strKey)))
                strKey = GenerateKey();

            return new LicenseKey(strKey);
        }

        internal string EncryptToString(Rsa rsaKey)
        {
            if (rsaKey == null)
                throw new ArgumentNullException("rsaKey");

            if (IsEmpty)
                return null;

            return EncryptString(rsaKey, ToString());
        }

        private static string EncryptString(Rsa rsaKey, string value)
        {
            Debug.Assert(rsaKey != null);
            Debug.Assert(!string.IsNullOrEmpty(value));

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] encryptedBytes = rsaKey.Encrypt(bytes);
            return LicenseManager.BytesToString(encryptedBytes);
        }

        internal static LicenseKey DecryptFromString(Rsa rsaKey, string encryptedLicenseKey)
        {
            if (string.IsNullOrEmpty(encryptedLicenseKey))
                return LicenseKey.Empty;

            return new LicenseKey(DecryptString(rsaKey, encryptedLicenseKey));
        }

        private static string DecryptString(Rsa rsaKey, string encryptedText)
        {
            Debug.Assert(rsaKey != null);
            Debug.Assert(!string.IsNullOrEmpty(encryptedText));

            byte[] encryptedBytes = LicenseManager.BytesFromString(encryptedText);
            byte[] decryptedBytes = rsaKey.Decrypt(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        internal string Encrypt(string text)
        {
            byte[] input = Encoding.UTF8.GetBytes(text);

            if (IsEmpty)
                return Convert.ToBase64String(input);

            byte[] output;
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                output = Transform(input, des.CreateEncryptor(GetTripleDesKey(_key), GetTripleDesIv(_key)));
            }
            return LicenseManager.BytesToString(output);
        }

        internal string Decrypt(string encryptedText)
        {
            if (IsEmpty)
            {
                Byte[] bytes = Convert.FromBase64String(encryptedText);
                return Encoding.UTF8.GetString(bytes);
            }

            byte[] input = LicenseManager.BytesFromString(encryptedText);
            byte[] output;
            using (TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider())
            {
                output = Transform(input, des.CreateDecryptor(GetTripleDesKey(_key), GetTripleDesIv(_key)));
            }
            return Encoding.UTF8.GetString(output);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
        {
            // create the necessary streams
            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(memStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    // transform the bytes as requested
                    cryptStream.Write(input, 0, input.Length);
                    cryptStream.FlushFinalBlock();

                    // Read the memory stream and
                    // convert it back into byte array
                    memStream.Position = 0;
                    byte[] result = memStream.ToArray();

                    // close and release the streams
                    memStream.Close();
                    cryptStream.Close();

                    // hand back the encrypted buffer
                    return result;
                }
            }
        }

        private static byte[] GetTripleDesKey(string licenseKey)
        {
            Debug.Assert(!string.IsNullOrEmpty(licenseKey));
            byte[] bytes = Encoding.UTF8.GetBytes(licenseKey);
            Debug.Assert(bytes.Length >= 24);

            byte[] key = new byte[16];
            for (int i = 0; i < key.Length; i++)
                key[i] = bytes[i];
            return key;
        }

        private static byte[] GetTripleDesIv(string licenseKey)
        {
            Debug.Assert(!string.IsNullOrEmpty(licenseKey));
            byte[] bytes = Encoding.UTF8.GetBytes(licenseKey);
            Debug.Assert(bytes.Length >= 24);

            byte[] iv = new byte[8];
            for (int i = 0; i < iv.Length; i++)
                iv[i] = bytes[i + 16];

            return iv;
        }

        private static Random Random
        {
            get
            {
                if (s_random == null)
                    s_random = new Random();

                return s_random;
            }
        }

        private static string GenerateKey()
        {
            int sum = 0;

            for (int i = 0; i < KeyLength - 1; i++)
            {
                s_keyChars[i] = GetDigit();
                int index = GetIndexOfDigit(s_keyChars[i]);
                Debug.Assert(index >= 0 && index < s_digits.Length);
                sum += index;
            }

            s_keyChars[s_keyChars.Length - 1] = GetCheckSum(sum);
            return new string(s_keyChars);
        }

        /// <summary>Determines the specified string can be converted to a <see cref="LicenseKey" />.</summary>
        /// <param name="value">The specified string.</param>
        /// <returns><see langword="true" /> if the string can be converted, otherwise <see langword="false" />.</returns>
        public static bool CanConvertFrom(string value)
        {
            if (value == null)
                return false;

            if (value.Length == KeyLength + 4)
            {
                if (value[5] != '-' || value[11] != '-' || value[17] != '-' || value[23] != '-')
                    return false;

                value = value.Substring(0, 5) + value.Substring(6, 5) + value.Substring(12, 5) + value.Substring(18, 5) + value.Substring(24, 5);
            }

            if (value.Length != KeyLength)
                return false;

            if (TripleDES.IsWeakKey(GetTripleDesKey(value)))
                return false;

            int sum = 0;
            for (int i = 0; i < KeyLength - 1; i++)
            {
                int index = GetIndexOfDigit(value[i]);
                if (index == -1)
                    return false;

                sum += index;
            }
            return value[value.Length - 1] == GetCheckSum(sum);
        }

        private static char GetCheckSum(int sum)
        {
            int checkSumIndex = s_digits.Length - (sum % s_digits.Length);
            if (checkSumIndex == s_digits.Length)
                checkSumIndex = 0;
            return GetDigit(checkSumIndex);
        }

        private static char GetDigit()
        {
            return s_digits[Random.Next(s_digits.Length)];
        }

        private static char GetDigit(int index)
        {
            Debug.Assert(index >= 0 && index < s_digits.Length);
            return s_digits[index];
        }

        private static int GetIndexOfDigit(char digit)
        {
            for (int i = 0; i < s_digits.Length; i++)
            {
                if (s_digits[i] == digit)
                    return i;
            }

            return -1;
        }

        /// <exclude/>
        public override bool Equals(object obj)
        {
            return obj is LicenseKey ? Equals(this, (LicenseKey)obj) : false;
        }

        /// <exclude/>
        public static bool Equals(LicenseKey value1, LicenseKey value2)
        {
            return value1._key == value2._key;
        }

        /// <exclude/>
        public static bool operator ==(LicenseKey value1, LicenseKey value2)
        {
            return Equals(value1, value2);
        }

        /// <exclude/>
        public static bool operator !=(LicenseKey value1, LicenseKey value2)
        {
            return !Equals(value1, value2);
        }

        /// <exclude/>
        public override int GetHashCode()
        {
            return _key == null ? 0 : _key.GetHashCode();
        }

        /// <exclude/>
        public override string ToString()
        {
            return _key == null ? string.Empty : string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}-{4}", _key.Substring(0, 5), _key.Substring(5, 5), _key.Substring(10, 5), _key.Substring(15, 5), _key.Substring(20, 5));
        }
    }
}
