using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;


namespace ChromeExtensionIdCalculatorConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            var extFinder = new ExtensionFinder();

            var keyParameter = extFinder.ReadAsymmetricKeyParameter("test.pem");
            var publicKey = extFinder.ConvertPrivateKeyToPublicKey(keyParameter);
            var hashedKey = extFinder.HashPublicKey(publicKey);
            var extensionId = extFinder.GetExtensionId(hashedKey);

            Console.WriteLine(extensionId);
        }
    }

    class ExtensionFinder
    {
        public AsymmetricKeyParameter ReadAsymmetricKeyParameter(string pemFilename)
        {
            var fileStream = File.OpenText(pemFilename);
            var pemReader = new PemReader(fileStream);
            var keyParameter = (AsymmetricKeyParameter) pemReader.ReadObject();

            return keyParameter;
        }

        public AsymmetricKeyParameter ConvertPrivateKeyToPublicKey(AsymmetricKeyParameter keyParameter)
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

        public string HashPublicKey(AsymmetricKeyParameter publicKey)
        {
            var pubInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            byte[] derEncodedPubKey = pubInfo.ToAsn1Object().GetDerEncoded();

            var sha256 = SHA256.Create();
            var hashedKey = sha256.ComputeHash(derEncodedPubKey);

            return BitConverter.ToString(hashedKey).Replace("-", "");
        }

        public string GetExtensionId(string hash)
        {
            var truncatedHash = hash.Substring(0, 32).ToUpper();

            var result = truncatedHash.Select(c => mapping[c]).ToArray();

            return string.Join("", result);
        }

        private Dictionary<char, char> mapping = new Dictionary<char, char>
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

    }
}
