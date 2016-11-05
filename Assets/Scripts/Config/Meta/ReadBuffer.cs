using System;
using System.Collections.Generic;
using System.Net;

//namespace ReaderTest
//{
    public class ReadBuffer
    {
        public enum Encoding
        {
            Default,
            UTF8,
        }

        byte[] mBuffer = null;
        int    mLen = 0;
        int    mPosition = 0;
        bool   mIsNetEndian = false;
        Encoding mEncoding = Encoding.UTF8;

        public void SetEncoding(Encoding type)
        {
            mEncoding = type;
        }

        public void SetNetEndianEnable(bool enable)
        {
            mIsNetEndian = enable;
        }

        public ReadBuffer(ref byte[] buffer, int len,int position = 0)
        {
            Reset(ref buffer, len, position);
        }

        public void Reset(ref byte[] buffer, int len,int position = 0)
        {
            mBuffer = buffer;
            mLen = len;
            mPosition = position;
        }

        bool CanReadSize(int size)
        {
            if (size + mPosition > mLen)
                return false;

            return true;
        }

        public int ReadInt8(ref sbyte dest,bool isSkip = false)
        {
            byte temp = 0;
            int ret = ReadUInt8(ref temp, isSkip);
            if(ret == 0)
            {
                dest = (sbyte)temp;
            }
            return ret;
        }

        public int ReadUInt8(ref byte dest,bool isSkip = false)
        {
            int size = sizeof(byte);
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            dest = mBuffer[mPosition];
            mPosition += size;
            return 0;
        }

        public int ReadInt16(ref short dest,bool isSkip = false)
        {
            int size = sizeof(short);
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            dest = BitConverter.ToInt16(mBuffer, mPosition);
            mPosition += size;

            if (mIsNetEndian)
            {
                dest = IPAddress.NetworkToHostOrder(dest);
            }
            return 0;
        }

        public int ReadUInt16(ref ushort dest,bool isSkip = false)
        {
            short temp = 0;
            int ret = ReadInt16(ref temp);
            if (ret == 0)
            {
                dest = (ushort)temp;
            }
            return ret;
        }

        public int ReadInt32(ref int dest, bool isSkip = false)
        {
            int size = sizeof(int);
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            dest = BitConverter.ToInt32(mBuffer, mPosition);
            mPosition += size;

            if (mIsNetEndian)
            {
                dest = IPAddress.NetworkToHostOrder(dest);
            }
            return 0;
        }

        public int ReadUInt32(ref uint dest, bool isSkip = false)
        {
            int temp = 0;
            int ret = ReadInt32(ref temp);
            if (ret == 0)
            {
                dest = (uint)temp;
            }
            return ret;
        }

        public int ReadInt64(ref long dest, bool isSkip = false)
        {
            int size = sizeof(long);
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            dest = BitConverter.ToInt64(mBuffer, mPosition);
            mPosition += size;

            if (mIsNetEndian)
            {
                dest = IPAddress.NetworkToHostOrder(dest);
            }
            return 0;
        }

        public int ReadUInt64(ref ulong dest, bool isSkip = false)
        {
            long temp = 0;
            int ret = ReadInt64(ref temp);
            if (ret == 0)
            {
                dest = (ulong)temp;
            }
            return ret;
        }

        public int ReadFloat(ref float dest, bool isSkip = false)
        {
            int size = sizeof(int);
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            dest = BitConverter.ToSingle(mBuffer, mPosition);

            mPosition += size;

            if (mIsNetEndian)
            {
                dest = (float)IPAddress.NetworkToHostOrder((int)dest);
            }
            return 0;
        }

        public int Skip(int size)
        {
            if (!CanReadSize(size))
            {
                return -1;
            }
            mPosition += size;
            return 0;
        }

        public int ReadByte(ref byte[] dest,int size,bool isSkip = false)
        {
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            Array.Copy(mBuffer, mPosition, dest, 0, size);
            mPosition += size;
            return 0;
        }

        public int ReadString(ref string dest,int size,bool isSkip = false)
        {
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            byte[] temp = new byte[size];
           
            Array.Copy(mBuffer, mPosition, temp, 0, size);
            if (mEncoding == Encoding.UTF8)
            {
                dest = System.Text.Encoding.UTF8.GetString(temp);
            }
            else
            {
                dest = System.Text.Encoding.Default.GetString(temp);
            }
            mPosition += size;
            dest = dest.Replace("\0", "");
            
            return 0;
        }

        public int ReadEnum<T>(ref T dest, int size, bool isSkip = false) where T : struct, System.IConvertible
        {
            if (isSkip)
                return Skip(size);

            if (!CanReadSize(size))
            {
                return -1;
            }
            
            //TypeCode type = dest.GetTypeCode();
            System.Type type = dest.GetType();
            
            // Current just support value smaller than 128(8 bit)
            sbyte temp = 0;
            int ret = ReadInt8(ref temp, false);
            if (ret == 0 /* && Enum.IsDefined(type, temp) */)
            {
                dest = (T)Enum.ToObject(type, temp);
            }

            return ret;
        }
    }
//}
