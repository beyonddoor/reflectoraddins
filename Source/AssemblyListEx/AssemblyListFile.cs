using System;
using System.Collections;
using System.Text;
using Reflector.CodeModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;

namespace Reflector.AssemblyListEx
{
    class AssemblyListFile
    {
        private string m_filename;
        private string[] m_assemblies;
        private bool m_automaticallyResolveReferences;
        private bool m_removeExisitingAssemblies;
        private bool m_supressResolveUI;

        internal AssemblyListFile(string filename)
        {
            m_filename = filename;
            _Load();
        }

        internal string Filename
        {
            get
            {
                return m_filename;
            }
        }

        internal string[] Assemblies
        {
            get
            {
                return m_assemblies;
            }
        }

        internal bool AutomaticallyResolveReferences
        {
            get
            {
                return m_automaticallyResolveReferences;
            }
        }

        internal bool RemoveExisitingAssemblies
        {
            get
            {
                return m_removeExisitingAssemblies;
            }
        }

        internal bool SupressResolveUI
        {
            get
            {
                return m_supressResolveUI;
            }
        }


        // FileFormat:
        //   <AssemblyList ResolveReferences="[yes|no]" RemoveExisitingAssemblies="[yes|no]" SupressResolveUI="[yes|no]">
        //     <Assembly Path="XXXXXX" />
        //     <Assembly Path="XXXXXX" />
        //   </AssemblyList>
        private void _Load()
        {
            string filename = m_filename;
            ArrayList returnValue = new ArrayList();
            bool automaticallyResolveReferences = false;
            bool removeExistingAssemblies = true;
            bool supressResolveUI = true;

            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    XmlTextReader reader = new XmlTextReader(fs);
                    reader.Read();

                    // Read in top level options
                    string resolveReferencesAttribute = reader.GetAttribute("ResolveReferences");
                    string removeExisitingAssembliesAttribute = reader.GetAttribute("RemoveExisitingAssemblies");
                    string supressResolveUIAttribute = reader.GetAttribute("SupressResolveUI");

                    if (resolveReferencesAttribute != null && resolveReferencesAttribute.ToLower(CultureInfo.InvariantCulture) == "yes")
                    {
                        automaticallyResolveReferences = true;
                    }

                    if (removeExisitingAssembliesAttribute != null && removeExisitingAssembliesAttribute.ToLower(CultureInfo.InvariantCulture) == "no")
                    {
                        removeExistingAssemblies = false;
                    }

                    if (supressResolveUIAttribute != null && supressResolveUIAttribute.ToLower(CultureInfo.InvariantCulture) == "no")
                    {
                        supressResolveUI = false;
                    }

                    reader.ReadStartElement("AssemblyList");

                    // Read assembly list
                    while (reader.Read() && reader.Name == "Assembly")
                    {
                        string assemblyFile = reader.GetAttribute("Path");
						if (assemblyFile != string.Empty && assemblyFile != null && File.Exists(assemblyFile))
						{
							returnValue.Add(assemblyFile);
						}
						else if (assemblyFile != string.Empty && assemblyFile != null)
						{
							// try looking in a directory relative to the .ref file
							assemblyFile = Path.Combine(Path.GetDirectoryName(filename), assemblyFile);
							if(File.Exists(assemblyFile))
							{
								returnValue.Add(assemblyFile);
							}

						}
                    }
                    
                }

            }
            catch (UnauthorizedAccessException securityException)
            {
                MessageBox.Show(String.Format("Access was denied when opening {0}.\r\n{1}", filename, securityException.ToString()));
            }

            // Set return values
            m_assemblies = new string[returnValue.Count];
            returnValue.CopyTo(m_assemblies);

            m_automaticallyResolveReferences = automaticallyResolveReferences;
            m_removeExisitingAssemblies = removeExistingAssemblies;
            m_supressResolveUI = supressResolveUI;
        }

        internal void Save(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
