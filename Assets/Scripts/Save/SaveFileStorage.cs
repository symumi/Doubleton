using System;
using System.IO;
using System.Text;

namespace BalartroLike.Save
{
    public static class SaveFileStorage
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public static bool TryWrite(string path, string content, out string error)
        {
            string temporaryPath = path + ".tmp";
            string backupPath = path + ".bak";
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(temporaryPath, content, Utf8NoBom);
                if (File.Exists(path))
                {
                    File.Replace(temporaryPath, path, backupPath, true);
                }
                else
                {
                    File.Move(temporaryPath, path);
                }

                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static bool TryRead(string path, out string content, out string error)
        {
            try
            {
                content = File.ReadAllText(path, Encoding.UTF8);
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                content = string.Empty;
                error = exception.Message;
                return false;
            }
        }
    }
}