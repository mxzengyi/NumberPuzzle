using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

//namespace ReaderTest
//{
    /* struct of the Head of Resource File */
    public class tagTResHead
    {
        public  const int TRES_TRANSLATE_METALIB_HASH_LEN = 36;
        public  const int TRES_ENCORDING_LEN              = 32;

        public int iMagic;                                   	/*   Magic number of Resource file */
        public int iVersion;                                 	/*   version of the Resource file */
        public int iUnit;                                    	/*   size of each resource calced by Byte */
        public int iCount;                                   	/*   total count of resources */
        public byte[] szMetalibHash = new byte[TRES_TRANSLATE_METALIB_HASH_LEN];	/*  Ver.2 hash of the resource metalib */
        public int iResVersion;                              	/*  Ver.3 version of the Resource struct */
        public long tCreateTime;                       	        /*  Ver.4 create time of resource file */
        public byte[] szResEncording = new byte[TRES_ENCORDING_LEN];        	/*  Ver.4 encording of the rescource data */
        public byte[] szContentHash = new byte[TRES_TRANSLATE_METALIB_HASH_LEN];	/*  Ver.5 hash of the content */
    }

    /* struct of the Head Extension of Resource File */
    public class tagTResHeadExt
    {
        public int iDataOffset;                              	/*   真正的资源的偏移位置 */
        public int iBuff;                                    	/*  Ver.7 字符串缓冲区的长度 */
    }

    public class tagTResHeadAll
    {
        public tagTResHead stResHead = new tagTResHead();
        public tagTResHeadExt stResHeadExt = new tagTResHeadExt();
    }

    public interface IResLoader
    {
        int Load(ReadBuffer buffer, bool skip = false);
    }

    public class ResLoaderHelper<T> where T : IResLoader, new()
    {
        public T[] mConfigs;
        public tagTResHeadAll resHeadAll;

        public int LoadFromBin(ref byte[] buffer)
        {
            if (buffer == null) return -1;

            int dataOffset = 0;
            int ret = BinReader.LoadFromMemory(ref buffer, ref resHeadAll, ref dataOffset);
            if (ret != 0) return ret;

            int configCount = resHeadAll.stResHead.iCount;
            mConfigs = new T[configCount];

            ReadBuffer readBuffer = new ReadBuffer(ref buffer, buffer.Length, dataOffset);
            for (int i = 0; i < configCount; ++i)
            {
                T res = new T();

                ret = res.Load(readBuffer, false);
                if (ret != 0)
                {
                    return ret;
                }

                mConfigs[i] = res;
            }
            return 0;
        }
    }

    public class BinReader
    {
        public static int LoadFromFile(string file,ref tagTResHeadAll head,ref byte[] buffer,ref int dataOffset)
        {
            dataOffset = 0;

            if(!File.Exists(file))
            {
                return -1;
            }

            FileStream fs = new FileStream(file, FileMode.Open);
            if(fs.Length <= 0)
            {
                fs.Close();
                return -2;
            }
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            fs.Close();
         
            return LoadFromMemory(ref buffer,ref head,ref dataOffset);
        }
        public static int LoadFromMemory(ref byte[] buffer,ref tagTResHeadAll head,ref int dataOffset)
        {
            if (buffer == null)
            {
                return -3;
            }

            head = new tagTResHeadAll();
            dataOffset = 0;

            int buffLength = buffer.Length;
            ReadBuffer readbuf = new ReadBuffer(ref buffer, buffLength);
            readbuf.ReadInt32(ref head.stResHead.iMagic);
            readbuf.ReadInt32(ref head.stResHead.iVersion);

            //check vision
            if (head.stResHead.iVersion != 7)
            {
                return -4;
            }

            readbuf.ReadInt32(ref head.stResHead.iUnit);
            readbuf.ReadInt32(ref head.stResHead.iCount);
            readbuf.ReadByte(ref head.stResHead.szMetalibHash, tagTResHead.TRES_TRANSLATE_METALIB_HASH_LEN);
            readbuf.ReadInt32(ref head.stResHead.iResVersion);
            readbuf.ReadInt64(ref head.stResHead.tCreateTime);
            readbuf.ReadByte(ref head.stResHead.szResEncording, tagTResHead.TRES_ENCORDING_LEN);
            readbuf.ReadByte(ref head.stResHead.szContentHash, tagTResHead.TRES_TRANSLATE_METALIB_HASH_LEN);

            readbuf.ReadInt32(ref head.stResHeadExt.iDataOffset);
            readbuf.ReadInt32(ref head.stResHeadExt.iBuff);
       
            //check length
            int countLength = head.stResHeadExt.iDataOffset + head.stResHeadExt.iBuff +
                head.stResHead.iUnit * head.stResHead.iCount;

            if (countLength != buffLength)
            {
                return -5;
            }
            dataOffset = head.stResHeadExt.iDataOffset + head.stResHeadExt.iBuff;

            return 0;
        }
    }

    public class XMLReader
    {
        public static int LoadFromFile(string file, ref tagTResHeadAll head, ref XmlDocument xmlDoc)
        {
            if (!File.Exists(file))
            {
                return -1;
            }

            FileStream fs = new FileStream(file, FileMode.Open);
            if (fs.Length <= 0)
            {
                fs.Close();
                return -2;
            }
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            fs.Close();

            return LoadFromMomory(ref buffer,ref head,ref xmlDoc);
        }

        public static int LoadFromMomory(ref byte[] buffer, ref tagTResHeadAll head, ref XmlDocument xmlDoc)
        {
            if (buffer == null)
            {
                return -3;
            }
            head = new tagTResHeadAll();
            xmlDoc = new XmlDocument();
            string xml = Encoding.UTF8.GetString(buffer);
            xmlDoc.LoadXml(xml);

            foreach (XmlNode node in xmlDoc.ChildNodes)
            {
                if(!node.HasChildNodes) continue;

                XmlNode resHeadAll = node.SelectSingleNode("TResHeadAll");
                if (resHeadAll == null)
                {
                    continue;
                }

                XmlNode resHead = resHeadAll.SelectSingleNode("resHead");
                if (resHead == null)
                {
                    continue;
                }

                foreach (XmlNode childNode in resHead.ChildNodes)
                {
                    switch(childNode.Name)
                    {
                        case "Magic":
                            head.stResHead.iMagic = int.Parse(childNode.InnerText);
                            break;

                        case "Version":
                            head.stResHead.iVersion = int.Parse(childNode.InnerText);

                            break;

                        case "Unit":
                            head.stResHead.iUnit = int.Parse(childNode.InnerText);
                            break;

                        case "Count":
                            head.stResHead.iCount = int.Parse(childNode.InnerText);
                            break;

                        case "MetalibHash":
                            {
                                byte[] temp = Encoding.Default.GetBytes(childNode.InnerText);
                                for (int i = 0; i < temp.Length;++i )
                                { 
                                    head.stResHead.szMetalibHash[i] = temp[i];
                                }
                            
                            }
                            break;

                        case "ResVersion":
                            head.stResHead.iResVersion = int.Parse(childNode.InnerText);
                            break;

                        case "CreateTime":
                            head.stResHead.tCreateTime = 0;//int.Parse(childNode.InnerText);
                            break;

                        case "ResEncording":
                            {
                                byte[] temp = Encoding.Default.GetBytes(childNode.InnerText);
                                for (int i = 0; i < temp.Length;++i )
                                { 
                                    head.stResHead.szResEncording[i] = temp[i];
                                }
                            }
                            break;

                        case "ContentHash":
                            {
                                byte[] temp = Encoding.Default.GetBytes(childNode.InnerText);
                                for (int i = 0; i < temp.Length; ++i)
                                {
                                    head.stResHead.szContentHash[i] = temp[i];
                                }
                            }
                            break;
                    }
                    
                }

                //check vision
                if (head.stResHead.iVersion != 7)
                {
                    return -4;
                }

                XmlNode resHeadExt = resHeadAll.SelectSingleNode("resHeadExt");
                foreach (XmlNode childNode in resHeadExt.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case "DataOffset":
                            head.stResHeadExt.iDataOffset = int.Parse(childNode.InnerText);
                            break;

                        case "Buff":
                            head.stResHeadExt.iBuff = int.Parse(childNode.InnerText);

                            break;
                    }
                }

                break;  // just olny one
            }
            return 0;
        }
    }
//}
