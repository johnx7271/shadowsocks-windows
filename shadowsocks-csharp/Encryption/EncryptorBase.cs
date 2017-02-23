using System.Security.Cryptography;
using System.Text;

namespace Shadowsocks.Encryption
{
	public struct EncryptorInfo
	{
		public int KeySize;
		public int IvSize;
		public int Type;
		public int CtxSize;
		public string InnerLibName;

		// For those who make use of internal crypto method name
		// e.g. mbed TLS
		public EncryptorInfo(string innerLibName, int keySize, int ivSize, int type, int ctxsize)
		{
			this.KeySize = keySize;
			this.IvSize = ivSize;
			this.Type = type;
			this.CtxSize = ctxsize;
			this.InnerLibName = innerLibName;
		}

		public EncryptorInfo(int keySize, int ivSize, int type)
		{
			this.KeySize = keySize;
			this.IvSize = ivSize;
			this.Type = type;
			this.CtxSize = 0;
			this.InnerLibName = string.Empty;
		}
	}
	public abstract class EncryptorBase
        : IEncryptor
    {
        public const int MAX_INPUT_SIZE = 32768;

        protected EncryptorBase(string method, string password, bool onetimeauthon)
        {
            Method = method;
            Password = password;
			OnetimeAuth = onetimeauthon;
        }

        protected string Method;
        protected string Password;
		protected bool OnetimeAuth;


		protected byte[] GetPasswordHash()
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(Password);
            return MD5.Create().ComputeHash(inputBytes);
        }

        public abstract void Encrypt(byte[] buf, int length, byte[] outbuf, out int outlength);

        public abstract void Decrypt(byte[] buf, int length, byte[] outbuf, out int outlength);

        public abstract void Dispose();
    }
}
