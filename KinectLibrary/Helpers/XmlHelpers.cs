using System;
using System.IO;
using System.Xml.Serialization;

namespace KinectLibrary.Helpers
{
    public static class XmlHelpers
    {
        public static bool ReadFromFile<T>(string path, out T readData)
        {
            readData = default(T);

            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    readData = (T) xmlSerializer.Deserialize(streamReader);
                }

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }

        public static bool WriteToFile<T>(string path, T dataToWrite)
        {
            string backupFilePath = Path.ChangeExtension(path, ".bak");

            try
            {
                CreateBackupFile(path, backupFilePath);

                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(streamWriter, dataToWrite);
                }

                File.Delete(backupFilePath);
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);

                RestoreBackupFile(path, backupFilePath);
                return false;
            }
        }

        private static bool RestoreBackupFile(string corruptFilePath, string backupFilePath)
        {
            if (File.Exists(corruptFilePath) && File.Exists(backupFilePath))
            {
                string secondBackupPath = Path.ChangeExtension(backupFilePath, ".tmp");
                File.Replace(backupFilePath, corruptFilePath, secondBackupPath);
                File.Delete(secondBackupPath);

                return true;
            }

            return false;
        }

        private static bool CreateBackupFile(string currentPath, string newPath)
        {
            if (File.Exists(currentPath))
            {
                File.Copy(currentPath, newPath);
                return true;
            }

            return false;
        }
    }
}