using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;


namespace ChromeExtensionsIdCalculator
{
    public class ExtensionIdentifier
    {
        public string CalculateId(string path)
        {
            try
            {
                var keyParameter = ReadAsymmetricKeyParameter(path);
                var publicKey = ConvertPrivateKeyToPublicKey(keyParameter);
                var hashedKey = HashPublicKey(publicKey);
                var extensionId = GetExtensionId(hashedKey);

                return extensionId;
            }
            catch (Exception e)
            {
                throw new ExtensionCalculationException("Exception while calculating Id", e);
            }
        }

        private AsymmetricKeyParameter ReadAsymmetricKeyParameter(string path)
        {
            using (var fileStream = File.OpenText(path))
            {
                var pemReader = new PemReader(fileStream);
                var keyParameter = (AsymmetricKeyParameter) pemReader.ReadObject();

                return keyParameter;
            }
        }

        private AsymmetricKeyParameter ConvertPrivateKeyToPublicKey(AsymmetricKeyParameter keyParameter)
        {
            AsymmetricCipherKeyPair keyPair = null;

            if (keyParameter is RsaPrivateCrtKeyParameters rsaPrivateKey)
            {
                var pub = new RsaKeyParameters(false, rsaPrivateKey.Modulus, rsaPrivateKey.PublicExponent);
                keyPair = new AsymmetricCipherKeyPair(pub, rsaPrivateKey);
            }
            else
            {
                throw new ArgumentException("Not an RSA private key.");
            }

            return keyPair.Public;
        }

        private string HashPublicKey(AsymmetricKeyParameter publicKey)
        {
            var pubInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            byte[] derEncodedPubKey = pubInfo.ToAsn1Object().GetDerEncoded();

            var sha256 = SHA256.Create();
            var hashedKey = sha256.ComputeHash(derEncodedPubKey);

            return BitConverter.ToString(hashedKey).Replace("-", "");
        }

        private string GetExtensionId(string hash)
        {
            var truncatedHash = hash.Substring(0, 32).ToUpper();
            var result = truncatedHash.Select(c => _hexMapping[c]).ToArray();

            return string.Join("", result);
        }

        private readonly Dictionary<char, char> _hexMapping = new Dictionary<char, char>
        {
            {'0' , 'a'},
            {'1' , 'b'},
            {'2' , 'c'},
            {'3' , 'd'},
            {'4' , 'e'},
            {'5' , 'f'},
            {'6' , 'g'},
            {'7' , 'h'},
            {'8' , 'i'},
            {'9' , 'j'},
            {'A' , 'k'},
            {'B' , 'l'},
            {'C' , 'm'},
            {'D' , 'n'},
            {'E' , 'o'},
            {'F' , 'p'},
        };

        [Serializable]
        public class ExtensionCalculationException : Exception
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            public ExtensionCalculationException()
            {
            }

            public ExtensionCalculationException(string message) : base(message)
            {
            }

            public ExtensionCalculationException(string message, Exception inner) : base(message, inner)
            {
            }

            protected ExtensionCalculationException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }

    }
}
