using System;
using System.Text;
using System.Collections;
using Microsoft.Win32;
using System.Security;
using System.Windows.Forms;

namespace Reflector.AssemblyListEx
{
    class FileTypeAssociation
    {
        internal static void Create(string extension, string contentType, string fullName, string properName, string iconPath, Hashtable commands)
        {
            // Remove any existing association
            Remove(extension, properName);

            try
            {
                RegistryKey extensionKey = Registry.ClassesRoot.CreateSubKey(extension);
                extensionKey.SetValue("", properName);
                extensionKey.SetValue("Content Type", contentType);
                extensionKey.Close();

                RegistryKey properNameKey = Registry.ClassesRoot.CreateSubKey(properName);
                properNameKey.SetValue("", fullName);
                
                RegistryKey iconKey = properNameKey.CreateSubKey("DefaultIcon");
                iconKey.SetValue("", iconPath);
                iconKey.Close();

                RegistryKey shellKey = properNameKey.CreateSubKey("Shell");
                
                foreach(DictionaryEntry de in commands)
                {
                    string commandName = de.Key as string;
                    string command = de.Value as string;

                    RegistryKey commandKey = shellKey.CreateSubKey(commandName);
                    RegistryKey cmdKey = commandKey.CreateSubKey("Command");
                    cmdKey.SetValue("", command);
                    cmdKey.Close();
                    commandKey.Close();
                }

                shellKey.Close();
                properNameKey.Close();
            }
            catch (SecurityException securityException)
            {
                MessageBox.Show(String.Format("File type association failed.\r\n{0}", securityException.ToString()));
            }


        }

        internal static void Remove(string extension, string properName)
        {
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(extension);
                Registry.ClassesRoot.DeleteSubKeyTree(properName);
            }
            catch (ArgumentException)
            {
                /* Keys do not exist */
            }
        }
    }
}
