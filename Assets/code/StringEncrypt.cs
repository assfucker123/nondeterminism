using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

public class StringEncrypt {

    private static byte[] key = { 123, 22, 19, 11, 24, 26, 85, 45, 114, 184, 27, 34, 37, 112, 222, 89, 241, 24, 175, 144, 173, 2, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 }; // <= 255
    private static byte[] vector = { 146, 78, 191, 111, 3, 3, 113, 119, 231, 121, 55, 112, 34, 32, 114, 156 }; // <= 255
    private static ICryptoTransform encryptor, decryptor;
    private static UTF8Encoding encoder;
    private static bool initialized = false;

    public static void initialize() {
        if (initialized) return;
        RijndaelManaged rm = new RijndaelManaged();
        encryptor = rm.CreateEncryptor(key, vector);
        decryptor = rm.CreateDecryptor(key, vector);
        encoder = new UTF8Encoding();
        initialized = true;
    }

    public static string encrypt(string unencryptedStr) {
        initialize();
        return Convert.ToBase64String(encrypt(encoder.GetBytes(unencryptedStr)));
    }
    public static string decrypt(string encrypted) {
        initialize();
        return encoder.GetString(decrypt(Convert.FromBase64String(encrypted)));
    }

    public static byte[] encrypt(byte[] buffer) {
        initialize();
        return transform(buffer, encryptor);
    }
    public static byte[] decrypt(byte[] buffer) {
        initialize();
        return transform(buffer, decryptor);
    }

    private static byte[] transform(byte[] buffer, ICryptoTransform transform) {
        MemoryStream stream = new MemoryStream();
        using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write)) {
            cs.Write(buffer, 0, buffer.Length);
        }
        return stream.ToArray();
    }

}
