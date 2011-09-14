// ----------------------------------------------------------------------------
// Printer.cs (Utility)
// Copyright (c) 2011 Adam Nofsinger <adam.nofsinger@gmail.com>
//
// Permission to use, copy, modify, and distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;

namespace Utility
{
    /* device capabilities indices */
    [FlagsAttribute]
    public enum DeviceCapabilitiesFlags : short
    {
        DC_FIELDS = 1,
        DC_PAPERS = 2,
        DC_PAPERSIZE = 3,
        DC_MINEXTENT = 4,
        DC_MAXEXTENT = 5,
        DC_BINS = 6,
        DC_DUPLEX = 7,
        DC_SIZE = 8,
        DC_EXTRA = 9,
        DC_VERSION = 10,
        DC_DRIVER = 11,
        DC_BINNAMES = 12,
        DC_ENUMRESOLUTIONS = 13,
        DC_FILEDEPENDENCIES = 14,
        DC_TRUETYPE = 15,
        DC_PAPERNAMES = 16,
        DC_ORIENTATION = 17,
        DC_COPIES = 18,
        DC_BINADJUST = 19,
        DC_EMF_COMPLIANT = 20,
        DC_DATATYPE_PRODUCED = 21,
        DC_COLLATE = 22,
        DC_MANUFACTURER = 23,
        DC_MODEL = 24,
        DC_PERSONALITY = 25,
        DC_PRINTRATE = 26,
        DC_PRINTRATEUNIT = 27,
        DC_PRINTERMEM = 28,
        DC_MEDIAREADY = 29,
        DC_STAPLE = 30,
        DC_PRINTRATEPPM = 31,
        DC_COLORDEVICE = 32,
        DC_NUP = 33
    }

    public class PrinterUtils
    {
        [DllImport("winspool.drv", SetLastError = true)]
        static extern Int32 DeviceCapabilities(string device, string port, DeviceCapabilitiesFlags capability,
                                               IntPtr outputBuffer, IntPtr deviceMode);

        public static bool GetProperties(string strDeviceName, string strPort, out bool bDuplex, out bool bColor, out string strError)
        {
            strError = "";
            bDuplex = false;
            bColor = false;

            // Duplex
            int nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_DUPLEX, IntPtr.Zero, (IntPtr)null);
            if (nRes < 0)
            {
                strError = new Win32Exception(Marshal.GetLastWin32Error()).Message + "[" + strDeviceName + ": " + strPort + "]";
                return false;
            }
            bDuplex = nRes == 1;

            // Color
            nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_COLORDEVICE, IntPtr.Zero, (IntPtr)null);
            if (nRes < 0)
            {
                strError = new Win32Exception(Marshal.GetLastWin32Error()).Message + "[" + strDeviceName + ": " + strPort + "]";
                return false;
            }
            bColor = nRes == 1;

            return true;
        }

        public static bool GetBins(string strDeviceName, string strPort, out ArrayList BinNr, out ArrayList BinName, out string strError)
        {
            strError = "";
            BinNr = new ArrayList();
            BinName = new ArrayList();

            // Bins
            int nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_BINS, (IntPtr)null, (IntPtr)null);
            IntPtr pAddr = Marshal.AllocHGlobal((int)nRes * 2);
            nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_BINS, pAddr, (IntPtr)null);
            if (nRes < 0)
            {
                strError = new Win32Exception(Marshal.GetLastWin32Error()).Message + "[" + strDeviceName + ": " + strPort + "]";
                return false;
            }
            short[] bins = new short[nRes];
            int offset = pAddr.ToInt32();
            for (int i = 0; i < nRes; i++)
            {
                BinNr.Add(Marshal.ReadInt16(new IntPtr(offset + i * 2)));
            }
            Marshal.FreeHGlobal(pAddr);

            // BinNames
            nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_BINNAMES, (IntPtr)null, (IntPtr)null);
            pAddr = Marshal.AllocHGlobal((int)nRes * 24);
            nRes = DeviceCapabilities(strDeviceName, strPort, DeviceCapabilitiesFlags.DC_BINNAMES, pAddr, (IntPtr)null);
            if (nRes < 0)
            {
                strError = new Win32Exception(Marshal.GetLastWin32Error()).Message + "[" + strDeviceName + ": " + strPort + "]";
                return false;
            }

            offset = pAddr.ToInt32();
            for (int i = 0; i < nRes; i++)
            {
                BinName.Add(Marshal.PtrToStringAnsi(new IntPtr(offset + i * 24)));
            }
            Marshal.FreeHGlobal(pAddr);

            return true;
        }

    }
}
