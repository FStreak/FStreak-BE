using System;

namespace AgoraIO.Media
{
    public class ByteBuffer
    {
        private int CURRENT_LENGTH = 0;
        private int CURRENT_POSITION = 0;
        private byte[] TEMP_BYTE_ARRAY;

        public ByteBuffer() : this(1024)
        {
        }

        public ByteBuffer(int length)
        {
            TEMP_BYTE_ARRAY = new byte[length];
        }

        public ByteBuffer(byte[] data)
        {
            TEMP_BYTE_ARRAY = data;
            CURRENT_LENGTH = data.Length;
        }

        public void PushByte(byte b)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = b;
        }

        public void PushBytes(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = b;
            }
        }

        public void PushByteArray(byte[] bytes)
        {
            PushBytes(bytes);
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[CURRENT_LENGTH];
            Array.Copy(TEMP_BYTE_ARRAY, 0, result, 0, CURRENT_LENGTH);
            return result;
        }

        public void PushUInt16(UInt16 Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x00ff) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff00) >> 8) & 0xff);
        }

        public void PushInt(UInt32 Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x000000ff) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x0000ff00) >> 8) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x00ff0000) >> 16) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff000000) >> 24) & 0xff);
        }

        public void PushLong(long Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x000000ff) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x0000ff00) >> 8) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x00ff0000) >> 16) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff000000) >> 24) & 0xff);
        }

        public byte PopByte()
        {
            byte ret = TEMP_BYTE_ARRAY[CURRENT_POSITION++];
            return ret;
        }

        public UInt16 PopUInt16()
        {
            if (CURRENT_POSITION + 1 >= CURRENT_LENGTH)
            {
                return 0;
            }
            UInt16 ret = (UInt16)(TEMP_BYTE_ARRAY[CURRENT_POSITION] | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1] << 8);
            CURRENT_POSITION += 2;
            return ret;
        }

        public uint PopUInt()
        {
            if (CURRENT_POSITION + 3 >= CURRENT_LENGTH)
                return 0;
            uint ret = (uint)(TEMP_BYTE_ARRAY[CURRENT_POSITION] | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1] << 8 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 2] << 16 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 3] << 24);
            CURRENT_POSITION += 4;
            return ret;
        }

        public long PopLong()
        {
            if (CURRENT_POSITION + 3 >= CURRENT_LENGTH)
                return 0;
            long ret = (long)(TEMP_BYTE_ARRAY[CURRENT_POSITION] << 24 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1] << 16 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 2] << 8 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 3]);
            CURRENT_POSITION += 4;
            return ret;
        }

        public byte[] PopByteArray(int Length)
        {
            if (CURRENT_POSITION + Length > CURRENT_LENGTH)
            {
                return new byte[0];
            }
            byte[] ret = new byte[Length];
            Array.Copy(TEMP_BYTE_ARRAY, CURRENT_POSITION, ret, 0, Length);
            CURRENT_POSITION += Length;
            return ret;
        }

        public byte[] PopByteArray2(int Length)
        {
            if (CURRENT_POSITION <= Length)
            {
                return new byte[0];
            }
            byte[] ret = new byte[Length];
            Array.Copy(TEMP_BYTE_ARRAY, CURRENT_POSITION - Length, ret, 0, Length);
            CURRENT_POSITION -= Length;
            return ret;
        }
    }
}
