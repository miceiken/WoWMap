using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;

namespace WoWMap
{
    public class ADT
    {
        public ADT(string filename)
        {
            Chunks = new List<IChunk>();
            Filename = filename;
        }

        public string Filename
        {
            get;
            private set;
        }

        public List<IChunk> Chunks { get; private set; }

        public void Read()
        {
            using (var file = File.Open(Filename, FileMode.Open))
            using (var br = new BinaryReader(file))
            {
                var bytesRead = 0L;
                while (bytesRead < file.Length)
                {
                    file.Position = bytesRead;
                    var header = new ChunkHeader(br);
                    header.Flip();
                    bytesRead = file.Position + header.Size;

                    Console.WriteLine("{0} - {1} bytes", header.Name, header.Size);
                    // We should probably just read what we need
                    IChunk cr = null;
                    switch (header.Name)
                    {
                        case "MVER":
                            cr = new MVER();
                            break;
                        case "MHDR":
                            cr = new MHDR();
                            break;
                        case "MCIN":
                            cr = new MCIN();
                            break;
                        case "MTEX":
                            cr = new MTEX();
                            break;
                        case "MMDX":
                            cr = new MMDX();
                            break;
                        case "MWMO":
                            cr = new MWMO();
                            break;
                        case "MWID":
                            cr = new MWID();
                            break;
                        case "MDDF":
                            cr = new MDDF();
                            break;
                        case "MODF":
                            cr = new MODF();
                            break;
                        case "MH2O":
                            cr = new MH2O();
                            break;
                        case "MCNK":
                            cr = new MCNK();
                            break;
                        case "MFBO":
                            cr = new MFBO();
                            break;
                        case "MTXF":
                        case "MTXP":
                            break;
                    }

                    // Unknown chunk?
                    if (cr == null) continue;
                    cr.Read(header, br);
                    Chunks.Add(cr);
                }
            }
        }
    }
}
