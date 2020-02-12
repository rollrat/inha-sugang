/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.INHA_API.Crpyto
{
    public class RSAHelper
    {
        /// <summary>
        /// Create new private key, pulibc key pair.
        /// </summary>
        /// <returns></returns>
        public static (string, string) CreateKey()
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var private_key = RSAKeyExtensions.ToXmlString(rsa, true);
            var public_key = RSAKeyExtensions.ToXmlString(rsa, false);
            return (private_key, public_key);
        }

        public static byte[] Encrypt(byte[] target, string public_key)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            RSAKeyExtensions.FromXmlString(rsa, public_key);
            return rsa.Encrypt(target, false);
        }

        public static byte[] Decrypt(byte[] target, string private_key)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            RSAKeyExtensions.FromXmlString(rsa, private_key);
            return rsa.Decrypt(target, false);
        }
    }
}
