﻿/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using SharpTune;
using SharpTuneCore;
using SharpTune.Properties;

namespace SharpTune.RomMod
{
    public class RomMod
    {
        public const int Version = 9;
        public const int AuthVersion = 10000 + Version;

        /// <summary>
        /// Determines which command to run.
        /// </summary>
        public static bool Run(AvailableDevices availableDevices, string[] args)
        {
            if (args.Length == 2 && args[0] == "help")
            {
                PrintHelp(args[1]);
                return true;
            }
            else if (args.Length == 2 && args[0] == "dump")
            {
                return RomMod.TryDumpSRecordFile(args[1]);
            }
            else if (args.Length == 3 && args[0] == "test")
            {
                return RomMod.TryApply(args[1], args[2], true, false) || RomMod.TryApply(args[1], args[2], false, false);
            }
            else if (args.Length == 3 && args[0] == "apply")
            {
                return RomMod.TryApply(args[1], args[2], true, true);
            }
            else if (args.Length == 3 && args[0] == "applied")
            {
                return RomMod.TryApply(args[1], args[2], false, false);//TODO is this deprecated?
            }
            else if (args.Length == 3 && args[0] == "remove")
            {
                return RomMod.TryApply(args[1], args[2], false, true);
            }
            else if (args.Length == 4 && args[0] == "baseline")
            {
                return RomMod.TryGenerateBaseline(args[1], args[2], args[3]);
            }
            else if (args.Length == 4 && args[0] == "baselinedefine")
            {
                return RomMod.TryBaselineAndDefine(availableDevices, args[1], args[2], args[3],Settings.Default.EcuFlashDefRepoPath);
            }
            else if (args.Length == 4 && args[0] == "define")
            {
                return RomMod.TryDefine(availableDevices, args[1], args[2], args[3], Settings.Default.EcuFlashDefRepoPath);
            }
            else if (args.Length == 4 && args[0] == "hewbuild")
            {
                return RomMod.TryHewBuild(availableDevices, args[1], args[2], args[3], Settings.Default.EcuFlashDefRepoPath);
            }
            PrintHelp();
            return false;
        }

        /// <summary>
        /// Print generic usage instructions.
        /// </summary>
        private static void PrintHelp()
        {
            Trace.WriteLine("RomPatch Version " + RomMod.Version);
            Trace.WriteLine("Commands:");
            Trace.WriteLine("");
            Trace.WriteLine("test       - determine whether a patch is suitable for a ROM");
            Trace.WriteLine("apply      - apply a patch to a ROM file");
            Trace.WriteLine("applied    - determine whether a patch has been applied to a ROM");
            Trace.WriteLine("remove     - remove a patch from a ROM file");
            Trace.WriteLine("dump       - dump the contents of a patch file");
            Trace.WriteLine("baseline   - generate baseline data for a ROM and a partial patch");
            Trace.WriteLine("define     - define a patched rom");
            Trace.WriteLine("hewbuild   - perform hew build (baseline -> patch -> define)");
            Trace.WriteLine("");
            Trace.WriteLine("Use \"RomPatch help <command>\" to show help for that command.");
        }

        /// <summary>
        /// Print usage instructions for a particular command.
        /// </summary>
        private static void PrintHelp(string command)
        {
            switch (command)
            {
                case "test":
                    Trace.WriteLine("RomPatch test <patchfilename> <romfilename>");
                    Trace.WriteLine("Determines whether the given patch file matches the given ROM file.");
                    break;

                case "apply":
                    Trace.WriteLine("RomPatch apply <patchfilename> <romfilename>");
                    Trace.WriteLine("");
                    Trace.WriteLine("Verifies that a patch is suitable for the ROM file, then applies");
                    Trace.WriteLine("the patch to the ROM (or prints an error message).");
                    break;

                case "applied":
                    Trace.WriteLine("RomPatch applied <patchfilename> <romfilename>");
                    Trace.WriteLine("Determines whether the given patch file was applied to the given ROM file.");
                    break;

                case "remove":
                    Trace.WriteLine("RomPatch remove <patchfilename> <romfilename>");
                    Trace.WriteLine("");
                    Trace.WriteLine("Verifies that a patch was applied to the ROM file, then removes");
                    Trace.WriteLine("the patch from the ROM (or prints an error message).");
                    break;

                case "dump":
                    Trace.WriteLine("RomPatch dump <filename>");
                    Trace.WriteLine("Dumps the contents of the give patch file.");
                    break;

                case "baseline":
                    Trace.WriteLine("RomPatch baseline <patchfilename> <romfilename>");
                    Trace.WriteLine("Generates baseline SRecords for the given patch and ROM file.");
                    break;

                case "help":
                    Trace.WriteLine("You just had to try that, didn't you?");
                    break;
            }
        }

        private static bool TryApply(string patchPath, string romPath, bool apply, bool commit)
        {
            Mod currentMod = new Mod(patchPath);//TODO: KING KLUDGE!
            return currentMod.TryCheckApplyMod(romPath, romPath, apply, commit);
        }
    
        /// <summary>
        /// Dump the contents of an SRecord file.  Mostly intended for development use.
        /// </summary>
        private static bool TryDumpSRecordFile(string path)
        {
            bool result = true;
            SRecord record;
            SRecordReader reader = new SRecordReader(path);
            reader.Open();
            BlobList list = new BlobList();
            while (reader.TryReadNextRecord(out record))
            {
                if (!record.IsValid)
                {
                    Trace.WriteLine(record.ToString());
                    result = false;
                    continue;
                }

                list.ProcessRecord(record);

                Trace.WriteLine(record.ToString());
            }

            Trace.WriteLine("Aggregated:");
            foreach (Blob blob in list.Blobs)
            {
                Trace.WriteLine(blob.ToString());
            }

            return result;
        }
        
        /// <summary>
        /// Extract data from an unpatched ROM file, for inclusion in a patch file.
        /// </summary>
        private static bool TryGenerateBaseline(string patchPath, string romPath, string build)
        {
            using (Stream romStream = File.OpenRead(romPath))
            {
                Mod patcher = new Mod(patchPath, build);

                if (!patcher.TryReadPatches())
                    return false;
                return patcher.TryPrintBaselines(patchPath, romStream);
            }
        }

        private static bool TryBaselineAndDefine(AvailableDevices ad, string patchPath, string romPath, string build, string defPath)
        {
            using (Stream romStream = File.OpenRead(romPath))
            {
                Mod patcher = new Mod(patchPath, build);

                if (!patcher.TryReadPatches()) 
                    return false;
                if (!patcher.TryDefinition(ad, defPath)) 
                    return false;
                return patcher.TryPrintBaselines(patchPath,romStream);
            }
        }

        public static bool TryDefine(AvailableDevices ad, string patchPath, string romPath, string bc, string defPath)
        {
            using (Stream romStream = File.OpenRead(romPath))
            {
                Mod mod = new Mod(patchPath, bc);
                return mod.TryDefinition(ad, defPath);
            }
        }

        public static bool TryHewBuild(AvailableDevices ad, string patchPath, string romPath, string bc, string defPath)
        {
            Mod mod = new Mod(patchPath, bc);
            using (Stream romStream = File.OpenRead(romPath))
            {
                Trace.WriteLine("Attempting to read patches");
                if (!mod.TryReadPatches())
                {
                    PrintError("READING PATCH");
                    return false;
                }
                Trace.WriteLine("Attempting to baseline patches");
                if (!mod.TryPrintBaselines(patchPath, romStream))
                {
                    PrintError("GENERATING BASELINES");
                    return false;
                }
            }

            // use baseline'd patch to create new mod
            mod = new Mod(mod.ModIdent + ".patch");
            File.Copy(romPath, "oem.bin", true);
            Trace.WriteLine("Attempting to test patches");
            if (!mod.TryCheckApplyMod(romPath, romPath, true, false)) //&& !mod.TryCheckApplyMod(romPath, romPath, false, false)) ;
            {
                PrintError("TESTING PATCH");
                return false;
            }
            Trace.WriteLine("Attempting to apply patches");
            if (!mod.TryCheckApplyMod(romPath, romPath, true, true))
            {
                PrintError("APPLYING PATCH");
                return false;
            }
            Trace.WriteLine("Attempting to remove patches");
            File.Copy(romPath, "reverted.bin", true);
            if (!mod.TryCheckApplyMod("reverted.bin", "reverted.bin", false, true))
            {
                PrintError("REMOVING PATCH");
                return false;
            }
            Trace.WriteLine("Attempting to verify patch removal");
            if (!Utils.FileCompare("reverted.bin", "oem.bin"))
            {
                PrintError("VERIFYING REMOVED PATCH");
                return false;
            }
            Trace.WriteLine("Attempting to define patch");
            if (!mod.TryDefinition(ad, defPath))
            {
                PrintError("WRITING DEFINITIONS");
                return false;
            }
            Trace.WriteLine("Attempting to copy patch to: "+ bc + "\\" + mod.InitialCalibrationId + "\\" + mod.FileName);
            string d = bc + "\\" + mod.InitialCalibrationId + "\\";
            Directory.CreateDirectory(d);
            string c = d + mod.FileName;
            File.Copy(mod.FilePath,c,true);
            Trace.WriteLine("HEW BUILD SUCCESS!!");
            return true;
        }

        public static void PrintError(string mess)
        {
            for (int i = 0; i < 5; i++)
                Trace.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ERROR " + mess + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}

    

