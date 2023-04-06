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

namespace S7CommPlusDriver
{
    public class SetMultiVariablesResponse
    {
        public byte ProtocolVersion;
        public UInt16 SequenceNumber;
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public Dictionary<UInt32, UInt64> ErrorValues;      // ItemNumber, ReturnValue
        public UInt32 IntegrityId;

        public SetMultiVariablesResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            ErrorValues = new Dictionary<UInt32, UInt64>();
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            UInt32 itemnr = 0;
            UInt64 retval = 0;

            ret += S7p.DecodeUInt16(buffer, out SequenceNumber);
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            ErrorValues.Clear();
            ret += S7p.DecodeUInt32Vlq(buffer, out itemnr);
            while (itemnr > 0)
            {
                ret += S7p.DecodeUInt64Vlq(buffer, out retval);
                ErrorValues.Add(itemnr, retval);
                ret += S7p.DecodeUInt32Vlq(buffer, out itemnr); /// ????? Ist das richtig?
            }
            ret += S7p.DecodeUInt32Vlq(buffer, out IntegrityId);
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<SetMultiVariablesResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "<ErrorValueList>" + Environment.NewLine;
            foreach (var errval in ErrorValues)
            {
                s += "<ErrorValue>" + Environment.NewLine;
                s += "<ItemNr>" + errval.Key.ToString() + "</ItemNr>" + Environment.NewLine;
                s += "<ReturnValue>" + errval.Value.ToString() + "</ReturnValue>" + Environment.NewLine;
                s += "</ErrorValue>" + Environment.NewLine;
            }
            s += "</ErrorValueList>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "<IntegrityId>" + IntegrityId.ToString() + "</IntegrityId>" + Environment.NewLine;
            s += "</SetMultiVariablesResponse>" + Environment.NewLine;
            return s;
        }

        public static SetMultiVariablesResponse DeserializeFromPdu(Stream pdu)
        {
            byte protocolVersion;
            byte opcode;
            UInt16 function;
            UInt16 reserved;
            // ProtocolVersion wird vorab als ein Byte in den Stream geschrieben, Sonderbehandlung
            S7p.DecodeByte(pdu, out protocolVersion);
            S7p.DecodeByte(pdu, out opcode);
            if (opcode != Opcode.Response)
            {
                return null;
            }
            S7p.DecodeUInt16(pdu, out reserved);
            S7p.DecodeUInt16(pdu, out function);
            S7p.DecodeUInt16(pdu, out reserved);
            if (function != Functioncode.SetMultiVariables)
            {
                return null;
            }
            SetMultiVariablesResponse resp = new SetMultiVariablesResponse(protocolVersion);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
