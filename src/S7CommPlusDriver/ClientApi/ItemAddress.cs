﻿#region License
/******************************************************************************
 * S7CommPlusDriver
 * 
 * Copyright (C) 2023 Thomas Wiens, th.wiens@gmx.de
 *
 * This file is part of S7CommPlusDriver.
 *
 * S7CommPlusDriver is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 /****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace S7CommPlusDriver
{
    public class ItemAddress : IS7pSerialize
    {
        public UInt32 SymbolCrc;
        public UInt32 AccessArea;
        public UInt32 AccessSubArea;
        public List<UInt32> LID = new List<uint>();

        public ItemAddress() : this (0, Ids.DB_ValueActual)
        {
        }

        public ItemAddress(UInt32 area, UInt32 subArea)
        {
            SymbolCrc = 0;
            AccessArea = area;
            AccessSubArea = subArea;
        }

        public ItemAddress(string variableAccessString)
        {
            // Verwendet einen kompletten Zugriffsstring bestehend aus Hexadezimalstrings getrennt durch einen Punkt.
            // Gibt eine Liste mit den daraus extrahierten IDs zurück.
            // Beispiele: 8A0E0001.A oder 52.A
            List<UInt32> ids = new List<UInt32>();
            foreach (string p in variableAccessString.Split('.'))
            {
                ids.Add(UInt32.Parse(p, System.Globalization.NumberStyles.HexNumber));
            }
            // TODO: Prüfen ob es einen Fehler gab, Feldlänge mindestens 2
            SymbolCrc = 0;
            AccessArea = ids[0];
            // Zugriffsbereich mit passend setzen
            if (AccessArea >= 0x8A0E0000)   // Datenbausteine
            {
                   AccessSubArea = Ids.DB_ValueActual;
            } 
            else if ((AccessArea == Ids.NativeObjects_theS7Timers_Rid) || 
                       (AccessArea == Ids.NativeObjects_theS7Counters_Rid) || 
                       (AccessArea == Ids.NativeObjects_theIArea_Rid) ||
                       (AccessArea == Ids.NativeObjects_theQArea_Rid) ||
                       (AccessArea == Ids.NativeObjects_theMArea_Rid))
            {
                AccessSubArea = Ids.ControllerArea_ValueActual;
            }
            foreach (var i in ids.Skip(1))
            {
                LID.Add(i);
            }
        }

        public UInt32 GetNumberOfFields()
        {
            return (UInt32)(4 + LID.Count);
        }

        public void SetAccessAreaToDatablock(UInt32 number)
        {
            AccessArea = (UInt16)number + 0x8a0e0000;
        }

        public int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeUInt32Vlq(buffer, SymbolCrc);
            ret += S7p.EncodeUInt32Vlq(buffer, AccessArea);
            ret += S7p.EncodeUInt32Vlq(buffer, (UInt32)LID.Count + 1);
            ret += S7p.EncodeUInt32Vlq(buffer, AccessSubArea);
            foreach (UInt32 id in LID)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, id);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<ItemAddress>" + Environment.NewLine;
            s += "<SymbolCrc>" + SymbolCrc.ToString() + "</SymbolCrc>" + Environment.NewLine;
            s += "<AccessArea>" + AccessArea.ToString() + "</AccessArea>" + Environment.NewLine;
            s += "<NumberOfIDs>" + (LID.Count + 1).ToString() + "</NumberOfIDs>" + Environment.NewLine;
            s += "<AccessSubArea>" + AccessSubArea.ToString() + "</AccessSubArea>" + Environment.NewLine;
            foreach (UInt32 id in LID)
            {
                s += "<LIDvalue>" + id.ToString() + "</LIDvalue>" + Environment.NewLine;
            }
            s += "</ItemAddress>" + Environment.NewLine;
            return s;
        }
    }
}
