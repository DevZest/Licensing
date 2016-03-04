using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace DevZest.Licensing
{
    /// <summary>Declares the public key used to verify the license.</summary>
    /// <remarks>
  	///  <para><see cref="FileIOPermission" /> is required to get the public key of the assembly. If you plan to distribute your
    ///  assembly running under partial trust, you need to declare the <see cref="LicensePublicKeyAttribute" /> for your assembly:</para>
    ///  	<code lang="C#">
	///		<![CDATA[
	///		[assembly: LicensePublicKey("0024000004800000940000000602000000240000525341310004000001000100ed58bddcb7bb199ed08c99bd83f732f26d49db4be3ea11c03a0c01bc0774bdcf5bbd3f00fd853f761598dd28489d9849a27e9eb901bb227d2c88b6644bd8e1b1453d021ea6b724995bdc5f839a608a5aa98f2ba6c602d25eaed7147e8046db369ad5ff0847423d926526176ff43902ee012d98f7010a5987448342107eb632b8")]
	///		]]>
   	///	</code>
   	///	<code lang="VB">
	///		<![CDATA[
	///		<assembly: LicensePublicKey("0024000004800000940000000602000000240000525341310004000001000100ed58bddcb7bb199ed08c99bd83f732f26d49db4be3ea11c03a0c01bc0774bdcf5bbd3f00fd853f761598dd28489d9849a27e9eb901bb227d2c88b6644bd8e1b1453d021ea6b724995bdc5f839a608a5aa98f2ba6c602d25eaed7147e8046db369ad5ff0847423d926526176ff43902ee012d98f7010a5987448342107eb632b8")>
	///		]]>
   	///	</code>
   	///  <para>Use the command line <application>sn.exe</application> utility to get the public key string from an signed
   	///  assembly:</para>
   	///  <command>sn -Tp <replacable>assemblyFile</replacable></command>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class LicensePublicKeyAttribute : Attribute
    {
        private string _publicKey;

        /// <summary>Initializes a new instance of <see cref="LicensePublicKeyAttribute" /> class.</summary>
        /// <param name="publicKey">
        /// <para>The public key hex string. You may use the following command to get the public key hex string:</para>
        /// <para><command>sn -Tp <replacable>assemblyFile</replacable></command></para>
        /// </param>
        public LicensePublicKeyAttribute(string publicKey)
        {
            _publicKey = publicKey;
        }

        /// <summary>Gets the public key hex string.</summary>
        /// <value>The public key hex string.</value>
        public string PublicKey
        {
            get { return _publicKey; }
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];

            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        internal byte[] GetPublicKey()
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.SetPublicKey(StringToByteArray(_publicKey));
            return assemblyName.GetPublicKey();
        }
    }
}
