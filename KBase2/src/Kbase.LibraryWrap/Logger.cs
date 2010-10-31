/*
This file is part of TheKBase Desktop
A Multi-Hierarchical  Information Manager
Copyright (C) 2004-2010 Daniel Rosenstark

TheKBase Desktop is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
("GPL") version 2 as published by the Free Software Foundation.
See the file LICENSE.TXT for the full text of the GNU GPL, or see
http://www.gnu.org/licenses/gpl.txt

For using TheKBase Desktop with software that can not be combined with 
the GNU GPL or any other queries, please contact Daniel Rosenstark 
(license@thekbase.com).
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Kbase.LibraryWrap
{
    class Logger
    {
        internal static void Init() {
            ConfusionUtilities.FileLogger.SetFileName("thekbase.log");
            string pId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            ConfusionUtilities.FileLogger.Init(Universe.Instance.Settings.tracing, pId);
            ConfusionUtilities.Logger.Log("Starting TheKBase", ConfusionUtilities.Logger.EventType.Information);
        }

        internal static void ShutDown()
        {
            ConfusionUtilities.Logger.ShutDown();
        }

        internal static void Log(string text)
        {
            ConfusionUtilities.Logger.Log(text);
        }

        internal static void Log(Exception e)
        {
            ConfusionUtilities.Logger.Log(e);
        }


        internal static void LogTimer(DateTime marker, string text)
        {
            ConfusionUtilities.Logger.LogTimer(marker,text);
        }

        internal static bool IsInitialized
        {
            get
            {
                return ConfusionUtilities.Logger.IsInitialized;
            }

        }
    }
}
