namespace dotnet_bookish_starter.AuxiliaryClasses;
using System.Security.Cryptography;
using System.Text;

public class HashClass
{
    public static int HashPassword(string password)
    {
        byte[] passwordByte;
        byte[] hashByte;
        
        passwordByte = ASCIIEncoding.ASCII.GetBytes(password);
        hashByte = new MD5CryptoServiceProvider().ComputeHash(passwordByte);
        return BitConverter.ToInt32(hashByte, 0);
    }
}