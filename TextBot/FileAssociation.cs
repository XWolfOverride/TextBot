﻿//MIT License
//
//Copyright(c) 2018 XWolf Override
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;

namespace XWolf
{

    public class FileAssociation
    {
        public static bool AssociateMe(string extension, string progID, string description)
        {
            return AssociateMe(extension, progID, description, 0, null);
        }

        public static bool AssociateMe(string extension, string progID, string description, int iicon)
        {
            return AssociateMe(extension, progID, description, iicon, null);
        }

        public static bool AssociateMe(string extension, string progID, string description, int iicon, string applicationParams)
        {
            try
            {
                Assembly me = Assembly.GetExecutingAssembly();
                string path = me.Location;
                if (extension.StartsWith("*"))
                    extension = extension.Substring(1);
                if (!extension.StartsWith("."))
                    extension = "." + extension;
                if (IsAssociatedWithMe(extension))
                    return true;
                if (applicationParams == null)
                    applicationParams = "\"%1\"";
                string icon;
                if (iicon == -1)
                    icon = "%1";
                else
                    icon = path + "," + iicon;
                string application = path + " " + applicationParams;
                Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
                if (progID != null && progID.Length > 0)
                {
                    RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID);
                    if (description != null)
                        key.SetValue("", description);
                    if (icon != null)
                        key.CreateSubKey("DefaultIcon").SetValue("", icon);
                    if (application != null)
                        key.CreateSubKey(@"Shell\Open\Command").SetValue("", application);
                }
                ShellNotification.NotifyOfChange();
                return true;
            }
            catch { return false; }

        }

        // Return true if extension already associated in registry
        public static bool IsAssociated(string extension)
        {
            return (Registry.ClassesRoot.OpenSubKey(extension, false) != null);
        }

        // Return true if extension already associated in registry
        public static bool IsAssociatedWithMe(string extension)
        {
            Assembly me = Assembly.GetExecutingAssembly();
            string path = me.Location;
            try
            {
                RegistryKey rkey = Registry.ClassesRoot.OpenSubKey(extension, false);
                if (rkey == null)
                    return false;
                string appId = rkey.GetValue("").ToString();
                rkey = Registry.ClassesRoot.OpenSubKey(appId, false);
                rkey = rkey.OpenSubKey("Shell", false);
                rkey = rkey.OpenSubKey("Open", false);
                rkey = rkey.OpenSubKey("Command", false);
                return (rkey.GetValue("").ToString().ToLower().StartsWith(path.ToLower()));
            }
            catch
            {
                return false;
            }
        }

        public static void AddShellNew(string extension, string command)
        {
            Assembly me = Assembly.GetExecutingAssembly();
            string path = me.Location;
            AddShellNewFull(extension, path + ", " + command);
        }

        public static void AddShellNewFull(string extension, string command)
        {
            try
            {
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(extension, true).CreateSubKey("ShellNew");
                rk.SetValue("ItemName", command);
                rk.SetValue("NullFile", "");
                rk.Close();
                ShellNotification.NotifyOfChange();
            }
            catch { };
        }

        public static void AddShellNewFilename(string extension, string filename)
        {
            try
            {
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(extension, true).CreateSubKey("ShellNew");
                rk.SetValue("FileName", filename);
                rk.Close();
                ShellNotification.NotifyOfChange();
            }
            catch { };
        }

        //[DllImport("Kernel32.dll")]
        //private static extern uint GetShortPathName(string lpszLongPath,
        //    [Out] StringBuilder lpszShortPath, uint cchBuffer);

        //// Return short path format of a file name
        //private static string ToShortPathName(string longName)
        //{
        //    StringBuilder s = new StringBuilder(1000);
        //    uint iSize = (uint)s.Capacity;
        //    uint iRet = GetShortPathName(longName, s, iSize);
        //    return s.ToString();
        //}
    }

    class ShellNotification
    {
        /// <summary>
        /// Notifies the system of an event that an application has performed. An application should use this function if it performs an action that may affect the Shell. 
        /// </summary>
        /// <param name="wEventId">Describes the event that has occurred. The ShellChangeNotificationEvents enum contains a list of options.</param>
        /// <param name="uFlags">Flags that indicate the meaning of the dwItem1 and dwItem2 parameters.</param>
        /// <param name="dwItem1">First event-dependent value.</param>
        /// <param name="dwItem2">Second event-dependent value.</param>
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(
            UInt32 wEventId,
            UInt32 uFlags,
            IntPtr dwItem1,
            IntPtr dwItem2);

        /// <summary>
        /// Notify shell of change of file associations.
        /// </summary>
        public static void NotifyOfChange()
        {
            SHChangeNotify(
                (uint)ShellChangeNotificationEvents.SHCNE_ASSOCCHANGED,
                (uint)(ShellChangeNotificationFlags.SHCNF_IDLIST | ShellChangeNotificationFlags.SHCNF_FLUSHNOWAIT),
                IntPtr.Zero,
                IntPtr.Zero);
        }


        [Flags]
        private enum ShellChangeNotificationEvents : uint
        {
            /// <summary>
            /// The name of a nonfolder item has changed. SHCNF_IDLIST or  SHCNF_PATH must be specified in uFlags. dwItem1 contains the  previous PIDL or name of the item. dwItem2 contains the new PIDL or name of the item. 
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,
            /// <summary>
            /// A nonfolder item has been created. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that was created. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_CREATE = 0x00000002,
            /// <summary>
            /// A nonfolder item has been deleted. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that was deleted. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_DELETE = 0x00000004,
            /// <summary>
            /// A folder has been created. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that was created. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_MKDIR = 0x00000008,
            /// <summary>
            /// A folder has been removed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that was removed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_RMDIR = 0x00000010,
            /// <summary>
            /// Storage media has been inserted into a drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that contains the new media. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,
            /// <summary>
            /// Storage media has been removed from a drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive from which the media was removed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,
            /// <summary>
            /// A drive has been removed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was removed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,
            /// <summary>
            /// A drive has been added. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was added. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,
            /// <summary>
            /// A folder on the local computer is being shared via the network. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that is being shared. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,
            /// <summary>
            /// A folder on the local computer is no longer being shared via the network. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that is no longer being shared. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,
            /// <summary>
            /// The attributes of an item or folder have changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item or folder that has changed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,
            /// <summary>
            /// The contents of an existing folder have changed, but the folder still exists and has not been renamed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the folder that has changed. dwItem2 is not used and should be NULL. If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or SHCNE_RENAMEFOLDER, respectively, instead.
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,
            /// <summary>
            /// An existing nonfolder item has changed, but the item still exists and has not been renamed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the item that has changed. dwItem2 is not used and should be NULL. If a nonfolder item has been created, deleted, or renamed, use SHCNE_CREATE, SHCNE_DELETE, or SHCNE_RENAMEITEM, respectively, instead.
            /// </summary>
            SHCNE_UPDATEITEM = 0x00002000,
            /// <summary>
            /// The computer has disconnected from a server. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the server from which the computer was disconnected. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,
            /// <summary>
            /// An image in the system image list has changed. SHCNF_DWORD must be specified in uFlags. dwItem1 contains the index in the system image list that has changed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000,
            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive that was added. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,
            /// <summary>
            /// The name of a folder has changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the previous pointer to an item identifier list (PIDL) or name of the folder. dwItem2 contains the new PIDL or name of the folder.
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,
            /// <summary>
            /// The amount of free space on a drive has changed. SHCNF_IDLIST or SHCNF_PATH must be specified in uFlags. dwItem1 contains the root of the drive on which the free space changed. dwItem2 is not used and should be NULL.
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,
            /// <summary>
            /// Not currently used.
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,
            /// <summary>
            /// A file type association has changed. SHCNF_IDLIST must be specified in the uFlags parameter. dwItem1 and dwItem2 are not used and must be NULL.
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,
            /// <summary>
            /// Specifies a combination of all of the disk event identifiers.
            /// </summary>
            SHCNE_DISKEVENTS = 0x0002381F,
            /// <summary>
            /// Specifies a combination of all of the global event identifiers. 
            /// </summary>
            SHCNE_GLOBALEVENTS = 0x0C0581E0,
            /// <summary>
            /// All events have occurred.
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,
            /// <summary>
            /// The specified event occurred as a result of a system interrupt. As this value modifies other event values, it cannot be used alone.
            /// </summary>
            SHCNE_INTERRUPT = 0x80000000
        }

        private enum ShellChangeNotificationFlags
        {
            /// <summary>
            /// dwItem1 and dwItem2 are the addresses of ITEMIDLIST structures that represent the item(s) affected by the change. Each ITEMIDLIST must be relative to the desktop folder. 
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// dwItem1 and dwItem2 are the addresses of null-terminated strings of maximum length MAX_PATH that contain the full path names of the items affected by the change.
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// dwItem1 and dwItem2 are the addresses of null-terminated strings that represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// The dwItem1 and dwItem2 parameters are DWORD values.
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// like SHCNF_PATHA but unicode string
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// like SHCNF_PRINTERA but unicode string
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// 
            /// </summary>
            SHCNF_TYPE = 0x00FF,
            /// <summary>
            /// The function should not return until the notification has been delivered to all affected components. As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components but should return as soon as the notification process has begun. As this flag modifies other data-type flags, it cannot by used  by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }
    }
}
