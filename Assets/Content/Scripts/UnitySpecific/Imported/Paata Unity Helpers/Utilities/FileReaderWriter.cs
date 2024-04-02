using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Patik.Utilities.File
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using UnityEngine;

    /// <summary>
    /// Don't use This Class Directly , Use <see cref="ReadFrom"/> and <see cref="WriteTo"/>
    /// </summary>
    public static class FileReaderWriter
    {
        /// <summary>
        /// Underlying file format
        /// </summary>
        public enum FileFormat
        {
            /// <summary>
            /// Resolved using Unity's internal <see cref="JsonUtility"/>
            /// </summary>
            Json,
            /// <summary>
            /// Resolved using <see cref="BinaryFormatter"/>
            /// </summary>
            Binary,
            /// <summary>
            /// Resolved Using <see cref="DataContractSerializer"/>
            /// </summary>
            XML
        }
        /// <summary>
        /// Writes file to file path using custom format serializer and encrypter (or unencrypted , if encrypter is not provided)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">Full Path of File</param>
        /// <param name="format">Format infrastructure to use (different formats have different serializers)</param>
        /// <param name="objectToWrite">Object to Write</param>
        /// <param name="encrypter">If encrypter is provided , file will be encrypted using encrypter</param>
        internal static void WriteTo<T>(string filePath, FileFormat format, T objectToWrite,
            EncryptionInfo encrypter = default)
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (objectToWrite == null)
            {
                Debug.LogError("Object You are Trying to Save is Null");
                return;
            }

            bool isEncrypted = encrypter!=default;

            using (var fileStream = File.Create(filePath))
            {
                if (!isEncrypted)
                {
                    Internal.StreamReaderWriter.WriteToStream(fileStream, objectToWrite, format);
                }

                else
                {
                    using (var cryptoStream = new CryptoStream(fileStream, encrypter.Encryptor, CryptoStreamMode.Write))
                    {
                        Internal.StreamReaderWriter.WriteToStream(cryptoStream, objectToWrite, format);
                    }
                }
            }
        }

        /// <summary>
        /// Reads file from file path using custom format serializer and decrypter (leave decrypter empty if file is not encrypted)
        /// </summary>
        /// <typeparam name="T">Type of Class</typeparam>
        /// <param name="filePath">Full Path of File</param>
        /// <param name="format">Format infrastructure to use (different formats have different serializers)</param>
        /// <param name="decrypter">Special decrypter if file is Encrypted </param>
        /// <returns></returns>
        internal static T ReadFrom<T>(string filePath, FileFormat format,
            EncryptionInfo decrypter = default(EncryptionInfo))
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            bool isEncrypted = decrypter!=default;

            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                if (!isEncrypted)
                {
                    return Internal.StreamReaderWriter.ReadFromStream<T>(fileStream, format);
                }

                using (var cryptoStream = new CryptoStream(fileStream, decrypter.Decryptor, CryptoStreamMode.Read))
                {
                    return Internal.StreamReaderWriter.ReadFromStream<T>(cryptoStream, format);
                }


            }
        }

        /// <summary>
        /// Paths Used  by this Specific FileReaderWriter Class
        /// </summary>
        public static class PathFor
        {
            private const string JSONFilesFolder = "/PersistentData/JSONCF/";
            private const string BinaryFilesFolder = "/PersistentData/Binary/";
            private const string XMLFilesFolder = "/PersistentData/XFF/";

            public static string ClassSingletonFile<T>(FileFormat format)
            {
                return FileByName(typeof(T).Name, format);
            }

            public static string FileByName(string name, FileFormat format)
            {
                return $"{Directory(format)}{name}{Internal.ExtensionFor.Format(format)}";
            }

            public static string Directory(FileFormat format)
            {
                switch (format)
                {
                    case FileFormat.Binary:
                        {
                            return $"{Application.persistentDataPath}{BinaryFilesFolder}";
                        }

                    case FileFormat.Json:
                        {
                            return $"{Application.persistentDataPath}{JSONFilesFolder}";
                        }

                    case FileFormat.XML:
                        {
                            return $"{Application.persistentDataPath}{XMLFilesFolder}";
                        }
                }

                return "";
            }

            public static string FileNameWithExtension(string name, FileFormat format)
            {
                return name + Internal.ExtensionFor.Format(format);
            }
        }

        /// <summary>
        /// Internal data for FileReaderWriter Usage
        /// </summary>
        private static class Internal
        {
            internal static class ExtensionFor
            {
                private const string JSON = ".jff";
                private const string XML = ".xff";
                private const string Binary = ".bff";

                public static string Format(FileFormat format)
                {
                    switch (format)
                    {
                        case FileFormat.Binary: return Binary;
                        case FileFormat.Json: return JSON;
                        case FileFormat.XML: return XML;
                    }

                    return "";
                }
            }
            internal static class StreamReaderWriter
            {
                internal static void WriteToStream<T>(Stream stream, T objectToWrite, FileFormat format)
                {
                    switch (format)
                    {
                        case FileFormat.Binary:
                            {
                                new BinaryFormatter().Serialize(stream, objectToWrite);
                                break;
                            }

                        case FileFormat.Json:
                            {
                                bool prettyFormat=false;
#if UNITY_EDITOR
                                prettyFormat = true;
#endif
                                var data = Encoding.Unicode.GetBytes(JsonUtility.ToJson(objectToWrite,prettyFormat));
                                stream.Write(data, 0, data.Length);
                                break;
                            }


                        case FileFormat.XML:
                            {   
                                new DataContractSerializer(typeof(T)).WriteObject(stream, objectToWrite);
                                break;
                            }
                    }
                }

                internal static T ReadFromStream<T>(Stream stream, FileFormat format)
                {
                    switch (format)
                    {
                        case FileFormat.Binary:
                            {
                                return (T)new BinaryFormatter().Deserialize(stream);
                            }

                        case FileFormat.Json:
                            {
                                byte[] data = null;

                                if (stream is CryptoStream)
                                {
                                    List<byte> bytes = new List<byte>();
                                    int _readByte;
                                    while ((_readByte = stream.ReadByte()) != -1)
                                    {
                                        bytes.Add((byte)_readByte);
                                    }

                                    data = bytes.ToArray();
                                }

                                else
                                {
                                    data = new byte[stream.Length];
                                    stream.Read(data, 0, data.Length);
                                }

                                return JsonUtility.FromJson<T>(Encoding.Unicode.GetString(data));
                            }

                        case FileFormat.XML:
                            {
                                return (T)new DataContractSerializer(typeof(T)).ReadObject(stream);
                            }
                    }

                    return default(T);
                }
            }
        }

        /// <summary>
        /// Encryption Information
        /// </summary>
        public class EncryptionInfo
        {
            public ICryptoTransform Encryptor;
            public ICryptoTransform Decryptor;

            public EncryptionInfo(string password, string salt)
            {
                Rijndael algoritm = Rijndael.Create();

                if (salt.Length < 8)
                {
                    salt = salt.PadRight(8);
                }

                else if (salt.Length > 8)
                {
                    salt = salt.Substring(0, 8);
                }

                var saltfromstring = Encoding.Unicode.GetBytes(salt.ToCharArray(), 0, 8);

                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltfromstring);
                algoritm.Padding = PaddingMode.ISO10126;
                algoritm.Mode = CipherMode.CBC;

                var passBytes = key.GetBytes(algoritm.Key.Length);
                var saltBytes = key.GetBytes(algoritm.IV.Length);
                algoritm.Key = passBytes;
                algoritm.IV = saltBytes;

                Encryptor = algoritm.CreateEncryptor(passBytes, saltBytes);
                Decryptor = algoritm.CreateDecryptor(passBytes, saltBytes);
            }


        }
    }

    /// <summary>
    /// Shortcut Class for Writing data to disk
    /// </summary>
    public class WriteTo
    {
        public static class BinaryFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.Binary;

            /// <summary>
            /// Creates file with the same name as class(with automatically added .bff extension) in binary folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AsClassSingleton<T>(T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AsClassSingleton(objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates file with given name (with automatically added .bff extension) in binary folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">name of file (without extension) </param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void WithName<T>(string name, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.WithName(name, objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates binary file at custom file path
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filePath">full path to file including directories and extension</param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AtAdress<T>(string filePath, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AtAdress(filePath, objectToWrite, format, encryption);
            }
        }

        public class JSONFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.Json;

            /// <summary>
            /// Creates file with the same name as class(with automatically added .jff extension) in json folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AsClassSingleton<T>(T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AsClassSingleton(objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates file with given name (with automatically added .jff extension) in json folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">name of file (without extension) </param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void WithName<T>(string name, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.WithName(name, objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates json file at custom file path
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filePath">full path to file including directories and extension</param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AtAdress<T>(string filePath, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AtAdress(filePath, objectToWrite, format, encryption);
            }
        }

        public class XMLFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.XML;

            /// <summary>
            /// Creates file with the same name as class(with automatically added .xff extension) in xml folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AsClassSingleton<T>(T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AsClassSingleton(objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates file with given name (with automatically added .xff extension) in xml folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">name of file (without extension) </param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void WithName<T>(string name, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.WithName(name, objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates xml file at custom file path
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filePath">full path to file including directories and extension</param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AtAdress<T>(string filePath, T objectToWrite, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                GenericFile.AtAdress(filePath, objectToWrite, format, encryption);
            }
        }

        public class GenericFile
        {
            /// <summary>
            /// Creates file with the same name as class(with automatically added .*** extension) in corresponding format folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="format">format of file </param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AsClassSingleton<T>(T objectToWrite, FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                AtAdress(FileReaderWriter.PathFor.ClassSingletonFile<T>(format), objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates file with given name (with automatically added .*** extension) in corresponding format folder , or overwrites it
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">name of file (without extension) </param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="format">format of file </param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void WithName<T>(string name, T objectToWrite, FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                AtAdress(FileReaderWriter.PathFor.FileByName(name, format), objectToWrite, format, encryption);
            }

            /// <summary>
            /// Creates binary file at custom file path
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filePath">full path to file including directories and extension</param>
            /// <param name="objectToWrite">class instance to be saved</param>
            /// <param name="format">format of file </param>
            /// <param name="encryption">if provided , file will be encrypted using provided encrypter , otherwise it will be written unencrypted</param>
            public static void AtAdress<T>(string filePath, T objectToWrite, FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                FileReaderWriter.WriteTo(filePath, format, objectToWrite, encryption);
            }
        }
    }

    /// <summary>
    /// Shortcut class for Reading data from disk
    /// </summary>
    public class ReadFrom
    {
        public class BinaryFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.Binary;

            /// <summary>
            /// Reads binary serialized class file with same name as class from binary folder  if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns>null</returns>
            public static T ClassSingleton<T>(FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {    
                return GenericFile.ClassSingleton<T>(format, encryption);
            }

            /// <summary>
            /// Reads binary serialized class file from binary folder if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">Name of file without extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T WithName<T>(string name, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.WithName<T>(name, format, encryption);
            }

            /// <summary>
            /// Reads binary serialized class file from custom file path if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filepath">full file path including directories and extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T AtAdress<T>(string filepath, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.AtAdress<T>(filepath, format, encryption);
            }
        }

        public class JSONFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.Json;

            /// <summary>
            /// Reads JSON serialized class file with same name as class from JSON folder  if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns>null</returns>
            public static T ClassSingleton<T>(FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.ClassSingleton<T>(format, encryption);
            }

            /// <summary>
            /// Reads JSON serialized class file from JSON folder if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">Name of file without extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T WithName<T>(string name, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.WithName<T>(name, format, encryption);
            }

            /// <summary>
            /// Reads JSON serialized class file from custom file path if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filepath">full file path including directories and extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T AtAdress<T>(string filepath, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.AtAdress<T>(filepath, format, encryption);
            }
        }

        public class XMLFile
        {
            private const FileReaderWriter.FileFormat format = FileReaderWriter.FileFormat.XML;

            /// <summary>
            /// Reads XML serialized class file with same name as class from XML folder  if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns>null</returns>
            public static T ClassSingleton<T>(FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.ClassSingleton<T>(format, encryption);
            }

            /// <summary>
            /// Reads XML serialized class file from XML folder if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">Name of file without extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T WithName<T>(string name, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.WithName<T>(name, format, encryption);
            }

            /// <summary>
            /// Reads XML serialized class file from custom file path if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filepath">full file path including directories and extension</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T AtAdress<T>(string filepath, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return GenericFile.AtAdress<T>(filepath, format, encryption);
            }
        }

        public class GenericFile
        {
            /// <summary>
            /// Reads serialized class file with same name as class from corresponding format folder if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="format">format of file</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns>null</returns>
            public static T ClassSingleton<T>(FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return AtAdress<T>(FileReaderWriter.PathFor.ClassSingletonFile<T>(format), format, encryption);
            }

            /// <summary>
            /// Reads serialized class file from corresponding format folder if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="name">Name of file without extension</param>
            /// <param name="format">format of file</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T WithName<T>(string name, FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return AtAdress<T>(FileReaderWriter.PathFor.FileByName(name, format), format, encryption);
            }

            /// <summary>
            /// Reads serialized class file from custom file path if such file exists , or returns null
            /// </summary>
            /// <typeparam name="T">type of class</typeparam>
            /// <param name="filepath">full file path including directories and extension</param>
            /// <param name="format">format of file</param>
            /// <param name="encryption">encrypter of file , leave empty if file is unencrypted</param>
            /// <returns></returns>
            public static T AtAdress<T>(string filepath, FileReaderWriter.FileFormat format, FileReaderWriter.EncryptionInfo encryption = default(FileReaderWriter.EncryptionInfo))
            {
                return FileReaderWriter.ReadFrom<T>(filepath, format, encryption);
            }
        }
    }

    /// <summary>
    /// Custom Serialization Instructions For File
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false, Inherited =true)]
    public class SerializationInstructionsAttribute : Attribute
    {
        private FileReaderWriter.EncryptionInfo encryption;
        /// <inheritdoc cref="FileReaderWriter.FileFormat"/>
        public FileReaderWriter.FileFormat Format;
        /// <summary>
        /// Password for Encryption (If left Empty , File Will Be Unencrypted)
        /// </summary>
        public string Password;
        /// <summary>
        /// Salt For Encryption (Has no effect if password is empty)
        /// <para>Takes First 8 Chars of passed String </para>
        /// </summary>
        public string Salt;
        public FileReaderWriter.EncryptionInfo EncryptionInfo
        {
            get
            {
                if (encryption == default)
                {
                    if (string.IsNullOrEmpty(Password)) return default;
                    if (string.IsNullOrEmpty(Salt)) Salt = Password;
                    encryption = new FileReaderWriter.EncryptionInfo(password: Password, salt: Salt);
                }

                return encryption;
            }
        }
    }
}