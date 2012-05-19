using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Io
{
    static class UTF8Decoder
    {
        public static byte[] ReadUtf8UInt(FlacStreamReader reader, out UInt32 decodedValue)
        {
            const int MAX_BITS_IN_STREAM = 6;
            byte[] retVal = new byte[MAX_BITS_IN_STREAM];

            UInt32 v = 0;
            UInt32 x;
            uint i;

            x = reader.ReadByte();
            retVal[0] = (byte)x;

            if ((x & 0x80) == 0)
            { /* 0xxxxxxx */
                v = x;
                i = 0;
            }
            else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
            { /* 110xxxxx */
                v = x & 0x1F;
                i = 1;
            }
            else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
            { /* 1110xxxx */
                v = x & 0x0F;
                i = 2;
            }
            else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
            { /* 11110xxx */
                v = x & 0x07;
                i = 3;
            }
            else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
            { /* 111110xx */
                v = x & 0x03;
                i = 4;
            }
            else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
            { /* 1111110x */
                v = x & 0x01;
                i = 5;
            }
            else
            {
                decodedValue = 0xffffffff;
                goto exit;
            }
            for (; i > 0; i--)
            {
				x = reader.ReadByte();
                retVal[MAX_BITS_IN_STREAM - i] = (byte)x;

                if ((x & 0x80) == 0 || (x & 0x40) != 0)
                { /* 10xxxxxx */
                    decodedValue = 0xffffffff;
                    goto exit;
                }
                v <<= 6;
                v |= (x & 0x3F);
            }
            decodedValue = v;
        exit:
            return retVal;
        }

        public static byte[] ReadUtf8ULong(FlacStreamReader reader, out UInt64 decodedValue)
        {
            const int MAX_BITS_IN_STREAM = 7;
            byte[] retVal = new byte[MAX_BITS_IN_STREAM];

            UInt64 v = 0;
            UInt32 x;
            uint i;

			x = reader.ReadByte();
            retVal[0] = (byte)x;

            if ((x & 0x80) == 0)
            { /* 0xxxxxxx */
                v = x;
                i = 0;
            }
            else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
            { /* 110xxxxx */
                v = x & 0x1F;
                i = 1;
            }
            else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
            { /* 1110xxxx */
                v = x & 0x0F;
                i = 2;
            }
            else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
            { /* 11110xxx */
                v = x & 0x07;
                i = 3;
            }
            else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
            { /* 111110xx */
                v = x & 0x03;
                i = 4;
            }
            else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
            { /* 1111110x */
                v = x & 0x01;
                i = 5;
            }
            else if ((x & 0xFE) != 0 && (x & 0x01) == 0)
            { /* 11111110 */
                v = 0;
                i = 6;
            }
            else
            {
                decodedValue = 0xffffffffffffffff;
                goto exit;
            }

            for (; i > 0; i--)
            {
                x = reader.ReadByte();
                retVal[MAX_BITS_IN_STREAM - i] = (byte)x;

                if ((x & 0x80) == 0 || (x & 0x40) != 0)
                { /* 10xxxxxx */
                    decodedValue = 0xffffffffffffffff;
                    goto exit;
                }
                v <<= 6;
                v |= (x & 0x3F);
            }
            decodedValue = v;

        exit:
            return retVal;
        }
    }
}
