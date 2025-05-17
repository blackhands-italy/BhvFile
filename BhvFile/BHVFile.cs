using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BHVEditor
{
    public class BHVFile
    {
        // Header and data sections
        public Header Header { get; set; } = new Header();
        public List<State> States { get; set; } = new List<State>();
        public List<StructB> StructBs { get; set; } = new List<StructB>();
        public List<List<byte>> StructCs { get; set; } = new List<List<byte>>();
        public List<StructD> StructDs { get; set; } = new List<StructD>();
        public List<string> Strings { get; set; } = null;

        private enum BHVType { BASENORMAL, W, WEAPON }
        private BHVType fileType;
        private const int DATA_START = 0x20;

        public static BHVFile Load(string path)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                BHVFile file = new BHVFile();
                file.DetectFileType(Path.GetFileName(path));
                file.Parse(br);
                return file;
            }
        }

        public void Save(string path)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Create(path)))
            {
                Write(bw);
            }
        }

        private void DetectFileType(string fileName)
        {
            string nameLower = fileName.ToLower();
            if (nameLower == "basenormal.bhv")
                fileType = BHVType.BASENORMAL;
            else if (nameLower == "weapon.bhv")
                fileType = BHVType.WEAPON;
            else
                fileType = BHVType.W;
        }

        private void Parse(BinaryReader br)
        {
            // Read Header
            Header.Version = br.ReadInt16();
            Header.Unk02 = br.ReadInt16();
            Header.FileSize = br.ReadInt32();
            // skip unknown ints 0x08-0x1C (assert zero)
            Header.UnknownHeader = br.ReadBytes(6 * 4);
            Header.StatesOffset = br.ReadInt32();
            Header.StateCount = br.ReadInt32();
            Header.OffsetB = br.ReadInt32();
            Header.CountB = br.ReadInt32();
            Header.OffsetC = br.ReadInt32();
            Header.CountC = br.ReadInt16();
            Header.SizeC = br.ReadInt16();
            Header.OffsetD = br.ReadInt32();
            Header.CountD = br.ReadInt32();

            // Mystery block
            int mysterySize = 0;
            if (fileType == BHVType.BASENORMAL) mysterySize = 0xE0;
            else if (fileType == BHVType.W) mysterySize = 0x40;
            else if (fileType == BHVType.WEAPON) mysterySize = 0;
            if (mysterySize > 0)
            {
                Header.MysteryBlock = br.ReadBytes(mysterySize);
            }

            // States
            States = new List<State>();
            if (Header.StateCount > 0)
            {
                br.BaseStream.Seek(DATA_START + Header.StatesOffset, SeekOrigin.Begin);
                for (int i = 0; i < Header.StateCount; i++)
                {
                    State st = new State();
                    st.Index = i;
                    st.Unk00 = br.ReadInt16();
                    st.Unk02 = br.ReadInt16();
                    st.Offset04 = br.ReadInt32();
                    st.TransitionsOffset = br.ReadInt32();
                    st.TransitionCount = br.ReadInt32();
                    st.StructBid = br.ReadInt16();
                    st.Unk10 = br.ReadInt16();
                    st.Unk14 = br.ReadInt32();
                    st.Unk18 = br.ReadInt32();
                    st.Unk1C = br.ReadInt32();
                    st.Unk20 = br.ReadInt32();
                    st.Unk24 = br.ReadInt32();
                    st.Unk28 = br.ReadInt32();
                    st.Unk2C = br.ReadInt32();
                    st.Unk30 = br.ReadInt32();
                    st.Unk34 = br.ReadInt32();
                    st.Unk38 = br.ReadInt32();
                    st.Unk3C = br.ReadInt32();
                    st.Unk40 = br.ReadSingle();
                    st.Unk44 = br.ReadSingle();
                    st.Unk48 = br.ReadSingle();
                    st.RootmotionStatus = br.ReadSingle();
                    st.Unk50 = br.ReadInt32();
                    st.Unk54 = br.ReadInt32();
                    st.Unk58 = br.ReadInt32();
                    st.Unk5C = br.ReadInt32();
                    st.Unk60 = br.ReadInt32();
                    st.StructBsid2 = br.ReadInt16();
                    st.UnkBsControlId = br.ReadInt16();
                    st.Unk68 = br.ReadInt32();
                    st.Unk6C = br.ReadInt32();
                    st.BladeHomingControl = br.ReadInt32();
                    st.Unk74 = br.ReadInt32();
                    st.Unk78 = br.ReadInt32();
                    st.Unk7C = br.ReadInt32();
                    if (Header.Version >= 10)
                    {
                        st.Unk80 = br.ReadInt32();
                        st.WeaponRailAnimationCallingId = br.ReadInt32();
                        st.Unk88 = br.ReadInt32();
                        st.Unk8C = br.ReadInt32();
                        st.Unk90 = br.ReadInt32();
                        st.Unk94 = br.ReadInt32();
                        st.Unk98 = br.ReadInt32();
                        st.Unk9C = br.ReadInt32();
                    }
                    States.Add(st);
                }
            }

            // StructB
            StructBs = new List<StructB>();
            if (Header.CountB > 0)
            {
                br.BaseStream.Seek(DATA_START + Header.OffsetB, SeekOrigin.Begin);
                for (int i = 0; i < Header.CountB; i++)
                {
                    StructB sb = new StructB();
                    sb.Unk00 = br.ReadInt32();
                    sb.Unk04 = br.ReadInt16();
                    sb.IsLooped = br.ReadInt16();
                    sb.Unk08 = br.ReadByte();
                    sb.Unk09 = br.ReadByte();
                    sb.Unk0A = br.ReadByte();
                    sb.Unk0B = br.ReadByte();
                    sb.Unk0C = br.ReadInt32();
                    sb.Unk10 = br.ReadInt32();
                    sb.Unk14 = br.ReadInt32();
                    sb.Unk18 = br.ReadInt32();
                    sb.Unk1C = br.ReadInt32();
                    sb.Unk20 = br.ReadSingle();
                    sb.FacingLocked = br.ReadByte();
                    sb.Unk25 = br.ReadByte();
                    sb.Unk26 = br.ReadByte();
                    sb.Unk27 = br.ReadByte();
                    sb.Unk28 = br.ReadSingle();
                    sb.Unk2C = br.ReadSingle();
                    sb.Unk30 = br.ReadInt32();
                    sb.Unk34 = br.ReadInt32();
                    sb.Unk38 = br.ReadInt32();
                    sb.Unk3C = br.ReadInt32();
                    if (Header.Version >= 6)
                    {
                        sb.Unk40 = br.ReadInt32();
                        sb.Unk44 = br.ReadInt16();
                        sb.Unk46 = br.ReadInt16();
                        sb.Unk48 = br.ReadInt16();
                        sb.Unk4A = br.ReadInt16();
                        sb.Unk4C = br.ReadInt16();
                        sb.Unk4E = br.ReadInt16();
                    }
                    if (Header.Version >= 10)
                    {
                        sb.Unk50 = br.ReadInt16();
                        sb.Unk52 = br.ReadInt16();
                        sb.Unk54 = br.ReadInt16();
                        sb.Unk56 = br.ReadInt16();
                        int dummy58 = br.ReadInt32(); // assert 0
                        int dummy5C = br.ReadInt32(); // assert 0
                    }
                    StructBs.Add(sb);
                }
            }

            // StructC
            StructCs = new List<List<byte>>();
            if (Header.CountC > 0)
            {
                br.BaseStream.Seek(DATA_START + Header.OffsetC, SeekOrigin.Begin);
                for (int i = 0; i < Header.CountC; i++)
                {
                    byte[] data = br.ReadBytes(Header.SizeC);
                    StructCs.Add(new List<byte>(data));
                }
            }

            // StructD and StructDA
            StructDs = new List<StructD>();
            if (Header.CountD > 0)
            {
                br.BaseStream.Seek(DATA_START + Header.OffsetD, SeekOrigin.Begin);
                for (int i = 0; i < Header.CountD; i++)
                {
                    StructD sd = new StructD();
                    sd.Offset00 = br.ReadInt32();
                    sd.Count04 = br.ReadInt32();
                    sd.Unk08 = br.ReadInt32();
                    sd.Unk0C = br.ReadInt32();
                    sd.Unk10 = br.ReadInt32();
                    sd.Unk14 = br.ReadInt32();
                    sd.Unk18 = br.ReadInt32();
                    sd.Unk1C = br.ReadInt32();
                    sd.StructDAs = new List<StructDA>();
                    // parse DA list if count04 > 0
                    if (sd.Count04 > 0)
                    {
                        long returnPos = br.BaseStream.Position;
                        br.BaseStream.Seek(DATA_START + sd.Offset00, SeekOrigin.Begin);
                        for (int j = 0; j < sd.Count04; j++)
                        {
                            StructDA da = new StructDA();
                            da.Unk00 = br.ReadInt16();
                            da.Unk02 = br.ReadInt16();
                            da.Unk04 = br.ReadInt32();
                            da.Unk08 = br.ReadSingle();
                            da.Unk0C = br.ReadSingle();
                            da.Unk10 = br.ReadSingle();
                            da.Unk14 = br.ReadSingle();
                            da.Unk18 = br.ReadSingle();
                            da.Unk1C = br.ReadSingle();
                            if (Header.Version >= 10)
                            {
                                da.Unk20 = br.ReadInt32();
                                da.Unk24 = br.ReadInt32();
                                da.Unk28 = br.ReadInt32();
                                da.Unk2C = br.ReadInt32();
                            }
                            sd.StructDAs.Add(da);
                        }
                        br.BaseStream.Seek(returnPos, SeekOrigin.Begin);
                    }
                    StructDs.Add(sd);
                }
            }

            // Prepare transitions and conditions parsing
            foreach (var st in States) st.Transitions = new List<Transition>();
            

            foreach (var st in States)
            {
                if (st.TransitionCount > 0)
                {
                    br.BaseStream.Seek(DATA_START + st.TransitionsOffset, SeekOrigin.Begin);
                    for (int t = 0; t < st.TransitionCount; t++)
                    {
                        Transition tr = new Transition();
                        tr.StateIndex = br.ReadInt32();
                        tr.ConditionsOffset = br.ReadInt32();
                        tr.ConditionCount = br.ReadInt32();
                        tr.Offset0C = br.ReadInt32();
                        tr.Unk10 = br.ReadInt32();
                        tr.Unk14 = br.ReadInt32();
                        tr.Unk18 = br.ReadInt32();
                        tr.Unk1C = br.ReadInt32();
                        tr.Conditions = new List<Condition>();
                        st.Transitions.Add(tr);
                    }
                }
            }

            // 2. 读取所有 Condition 头部，并累积到一个平面列表
            var allConds = new List<Condition>();
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    if (tr.ConditionCount <= 0) continue;
                    br.BaseStream.Seek(DATA_START + tr.ConditionsOffset, SeekOrigin.Begin);
                    for (int c = 0; c < tr.ConditionCount; c++)
                    {
                        var cond = new Condition
                        {
                            Id = br.ReadByte(),
                            Unk01 = br.ReadByte(),
                            Unk02 = br.ReadByte(),
                            Unk03 = br.ReadByte(),
                            DataOffset = br.ReadInt32(),
                            Unk08 = br.ReadByte(),
                            Unk09 = br.ReadByte(),
                            Unk0A = br.ReadByte(),
                            Unk0B = br.ReadByte(),
                            Unk0C = br.ReadInt32(),
                            Data = new List<byte>()
                        };
                        tr.Conditions.Add(cond);
                        allConds.Add(cond);
                    }
                }
            }
            // 3. 按 DataOffset 升序读取变长 Data 区
            allConds = allConds.OrderBy(c => c.DataOffset).ToList();
            for (int i = 0; i < allConds.Count; i++)
            {
                var cond = allConds[i];
                // 结束偏移：下一条的 DataOffset 或 B 段起始
                int nextOffset = (i < allConds.Count - 1)
                    ? allConds[i + 1].DataOffset
                    : Header.OffsetB;
                int length = nextOffset - cond.DataOffset;
                if (length < 0) length = 0;
                cond.DataLength = length;  // 可选：保存长度便于调试或 UI

                // 真正读取
                br.BaseStream.Seek(DATA_START + cond.DataOffset, SeekOrigin.Begin);
                cond.Data = br.ReadBytes(length).ToList();
            }
            // StructABB
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    br.BaseStream.Seek(DATA_START + tr.Offset0C, SeekOrigin.Begin);
                    StructABB abb = new StructABB();
                    abb.Unk00 = br.ReadByte();
                    abb.Unk01 = br.ReadByte();
                    abb.Unk02 = br.ReadByte();
                    abb.Unk03 = br.ReadByte();
                    if (abb.Unk01 == 1)
                    {
                        abb.BehaviorMatrixParam_f = br.ReadSingle();
                        abb.Unk08_int = br.ReadInt32();
                        abb.Unk0C = br.ReadInt32();
                        abb.Unk10_int = br.ReadInt32();
                        abb.Unk14_int = br.ReadInt32();
                        abb.Unk18_int = br.ReadInt32();
                        abb.Type = 1;
                    }
                    else if (abb.Unk01 == 3)
                    {
                        abb.BehaviorMatrixParam_i = br.ReadInt32();
                        abb.Unk08_f = br.ReadSingle();
                        abb.Unk0C = br.ReadInt32();
                        abb.Unk10_int = br.ReadInt32();
                        abb.Unk14_int = br.ReadInt32();
                        abb.Unk18_int = br.ReadInt32();
                        abb.Unk1C_int = br.ReadInt32();
                        abb.Type = 3;
                    }
                    else if (abb.Unk01 == 4)
                    {
                        abb.Unk04 = br.ReadInt32();
                        abb.Unk08_f = br.ReadSingle();
                        abb.Unk0C = br.ReadInt32();
                        abb.Unk10_int = br.ReadInt32();
                        abb.Unk14_int = br.ReadInt32();
                        abb.Unk18_int = br.ReadInt32();
                        abb.Unk1C_int = br.ReadInt32();
                        abb.Unk20_int = br.ReadInt32();
                        abb.Unk24_int = br.ReadInt32();
                        abb.Unk28_int = br.ReadInt32();
                        abb.Unk2C_int = br.ReadInt32();
                        abb.Unk30_int = br.ReadInt32();
                        abb.Type = 4;
                    }
                    else
                    {
                        abb.Type = 0;
                    }
                    tr.StructAbb = abb;
                }
            }

            // Condition data bytes
            if (allConds.Count > 0)
            {
                var sortedConds = allConds.OrderBy(c => c.DataOffset).ToList();
                for (int i = 0; i < sortedConds.Count; i++)
                {
                    int startRel = sortedConds[i].DataOffset;
                    int length;
                    if (i < sortedConds.Count - 1)
                        length = sortedConds[i + 1].DataOffset - sortedConds[i].DataOffset;
                    else
                        length = (int)Header.OffsetB - sortedConds[i].DataOffset;
                    if (length < 0) length = 0;
                    if (length > 0)
                    {
                        br.BaseStream.Seek(DATA_START + startRel, SeekOrigin.Begin);
                        byte[] bytes = br.ReadBytes(length);
                        sortedConds[i].Data = new List<byte>(bytes);
                    }
                    else
                    {
                        sortedConds[i].Data = new List<byte>();
                    }
                }
            }

            // Strings for basenormal
            if (fileType == BHVType.BASENORMAL)
            {
                Strings = new List<string>();
                long stringsStart = DATA_START + Header.OffsetD;
                if (Header.CountD == 0) stringsStart = DATA_START + Header.OffsetD;
                if (stringsStart < br.BaseStream.Length)
                {
                    br.BaseStream.Seek(stringsStart, SeekOrigin.Begin);
                    short strCount = br.ReadInt16();
                    if (strCount < 0) strCount = 0;
                    short[] offsets = new short[strCount];
                    for (int i = 0; i < strCount; i++) offsets[i] = br.ReadInt16();
                    long strContentStart = br.BaseStream.Position;
                    for (int i = 0; i < strCount; i++)
                    {
                        long pos = strContentStart + offsets[i];
                        br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        List<byte> buf = new List<byte>();
                        byte ch;
                        while ((ch = br.ReadByte()) != 0)
                        {
                            buf.Add(ch);
                        }
                        string s = Encoding.UTF8.GetString(buf.ToArray());
                        Strings.Add(s);
                    }
                }
            }
        }

        private void Write(BinaryWriter bw)
        {
            // Recalculate dynamic header fields
            Header.StateCount = (States != null ? States.Count : 0);
            Header.CountB = (StructBs != null ? StructBs.Count : 0);
            Header.CountC = (short)(StructCs != null ? StructCs.Count : 0);
            // Header.SizeC remains as is unless modified
            Header.CountD = (StructDs != null ? StructDs.Count : 0);

            // Prepare to write header
            long headerPos = bw.BaseStream.Position;
            bw.Write(Header.Version);
            bw.Write(Header.Unk02);
            // fileSize (placeholder)
            bw.Write(0);
            // 写回 parse 时读到的 24 字节 UnknownHeader
            if (Header.UnknownHeader != null && Header.UnknownHeader.Length == 24)
            {
                bw.Write(Header.UnknownHeader);
            }
            else
            {
                // 兜底填零
                for (int i = 0; i < 24; i++) bw.Write((byte)0);
            }
            // offsets and counts placeholders
            bw.Write(0); // statesOffset
            bw.Write(Header.StateCount);
            bw.Write(0); // offsetB
            bw.Write(Header.CountB);
            bw.Write(0); // offsetC
            bw.Write(Header.CountC);
            bw.Write(Header.SizeC);
            bw.Write(0); // offsetD
            bw.Write(Header.CountD);

            // 写版本依赖的 mystery block
            if (fileType == BHVType.BASENORMAL)
            {
                var mb = Header.MysteryBlock ?? new byte[0xE0];
                if (mb.Length != 0xE0) Array.Resize(ref mb, 0xE0);
                bw.Write(mb);
            }
            else if (fileType == BHVType.W)
            {
                var mb = Header.MysteryBlock ?? new byte[0x40];
                if (mb.Length != 0x40) Array.Resize(ref mb, 0x40);
                bw.Write(mb);
            }
            // if WEAPON, no mystery to write

            // Calculate offsets for states and transitions
            long statesStartPos = bw.BaseStream.Position;
            Header.StatesOffset = (int)(statesStartPos - DATA_START);
            int stateEntrySize = (Header.Version >= 10 ? 0xA0 : 0x80);
            // Allocate state data blocks
            bool anyStateData = States.Any(s => s.Data != null && s.Data.Count > 0);
            long dataPointerRel = Header.StatesOffset + Header.StateCount * stateEntrySize;
            foreach (var st in States)
            {
                if (st.Data != null && st.Data.Count > 0)
                {
                    st.Offset04 = (int)dataPointerRel;
                    dataPointerRel += st.Data.Count;
                }
                else
                {
                    st.Offset04 = -1; // mark to set later
                }
            }
            long transitionsStartRel = Header.StatesOffset + Header.StateCount * stateEntrySize + (int)(dataPointerRel - (Header.StatesOffset + Header.StateCount * stateEntrySize));
            // transitionsStartRel = Header.StatesOffset + state definitions length + totalStateDataLength
            foreach (var st in States)
            {
                if (st.Offset04 == -1)
                {
                    st.Offset04 = (int)transitionsStartRel;
                }
            }
            long transitionsPointerRel = transitionsStartRel;
            foreach (var st in States)
            {
                if (st.Transitions == null) st.Transitions = new List<Transition>();
                st.TransitionCount = st.Transitions.Count;
                if (st.TransitionCount > 0)
                {
                    st.TransitionsOffset = (int)transitionsPointerRel;
                    transitionsPointerRel += st.TransitionCount * 32;
                }
                else
                {
                    st.TransitionsOffset = (int)transitionsPointerRel;
                }
            }
            long structAbbStartRel = transitionsPointerRel;
            long structAbbPointerRel = structAbbStartRel;
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    tr.Offset0C = (int)structAbbPointerRel;
                    // determine size and pad
                    int abbSize;
                    if (tr.StructAbb == null)
                    {
                        abbSize = 0;
                    }
                    else if (tr.StructAbb.Type == 1)
                    {
                        abbSize = 0x1C; // 28 bytes
                    }
                    else if (tr.StructAbb.Type == 3)
                    {
                        abbSize = 0x20; // 32 bytes
                    }
                    else if (tr.StructAbb.Type == 4)
                    {
                        abbSize = 0x34; // 52 bytes
                    }
                    else
                    {
                        // fallback
                        abbSize = 0x20;
                    }
                    int alloc = ((abbSize + 31) / 32) * 32;
                    structAbbPointerRel += alloc;
                }
            }
            long conditionsStartRel = structAbbPointerRel;
            long conditionsPointerRel = conditionsStartRel;
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    if (tr.Conditions == null) tr.Conditions = new List<Condition>();
                    tr.ConditionCount = tr.Conditions.Count;
                    if (tr.ConditionCount > 0)
                    {
                        tr.ConditionsOffset = (int)conditionsPointerRel;
                        conditionsPointerRel += tr.ConditionCount * 16;
                    }
                    else
                    {
                        tr.ConditionsOffset = 0;
                    }
                }
            }
            long condDataStartRel = conditionsPointerRel;
            long condDataPointerRel = condDataStartRel;
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    foreach (var cond in tr.Conditions)
                    {
                        int dataLen = (cond.Data != null ? cond.Data.Count : 0);
                        cond.DataOffset = (int)condDataPointerRel;
                        if (dataLen > 0)
                        {
                            condDataPointerRel += dataLen;
                        }
                        // if dataLen=0, we do not advance pointer (no data to store)
                    }
                }
            }
            Header.OffsetB = (int)condDataPointerRel;
            Header.OffsetC = Header.OffsetB + Header.CountB * (Header.Version >= 10 ? 0x60 : (Header.Version == 6 ? 0x50 : 0x40));
            // If no StructC entries, we keep offsetC as computed end-of-B, which is fine.
            if (Header.CountD > 0)
            {
                Header.OffsetD = Header.OffsetC + Header.CountC * Header.SizeC;
            }
            else
            {
                // If no D, allow 0 or offsetC depending on type
                if (fileType == BHVType.BASENORMAL)
                    Header.OffsetD = Header.OffsetC;
                else
                    Header.OffsetD = 0;
            }

            // Write States definitions
            bw.BaseStream.Seek(statesStartPos, SeekOrigin.Begin);
            foreach (var st in States)
            {
                bw.Write(st.Unk00);
                bw.Write(st.Unk02);
                bw.Write(st.Offset04);
                bw.Write(st.TransitionsOffset);
                bw.Write(st.TransitionCount);
                bw.Write(st.StructBid);
                bw.Write(st.Unk10);
                bw.Write(st.Unk14);
                bw.Write(st.Unk18);
                bw.Write(st.Unk1C);
                bw.Write(st.Unk20);
                bw.Write(st.Unk24);
                bw.Write(st.Unk28);
                bw.Write(st.Unk2C);
                bw.Write(st.Unk30);
                bw.Write(st.Unk34);
                bw.Write(st.Unk38);
                bw.Write(st.Unk3C);
                bw.Write(st.Unk40);
                bw.Write(st.Unk44);
                bw.Write(st.Unk48);
                bw.Write(st.RootmotionStatus);
                bw.Write(st.Unk50);
                bw.Write(st.Unk54);
                bw.Write(st.Unk58);
                bw.Write(st.Unk5C);
                bw.Write(st.Unk60);
                bw.Write(st.StructBsid2);
                bw.Write(st.UnkBsControlId);
                bw.Write(st.Unk68);
                bw.Write(st.Unk6C);
                bw.Write(st.BladeHomingControl);
                bw.Write(st.Unk74);
                bw.Write(st.Unk78);
                bw.Write(st.Unk7C);
                if (Header.Version >= 10)
                {
                    bw.Write(st.Unk80);
                    bw.Write(st.WeaponRailAnimationCallingId);
                    bw.Write(st.Unk88);
                    bw.Write(st.Unk8C);
                    bw.Write(st.Unk90);
                    bw.Write(st.Unk94);
                    bw.Write(st.Unk98);
                    bw.Write(st.Unk9C);
                }
            }
            // Write state Data blocks
            foreach (var st in States)
            {
                if (st.Data != null && st.Data.Count > 0)
                {
                    bw.Write(st.Data.ToArray());
                }
            }
            // Write Transitions
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    bw.Write(tr.StateIndex);
                    bw.Write(tr.ConditionsOffset);
                    bw.Write(tr.ConditionCount);
                    bw.Write(tr.Offset0C);
                    bw.Write(tr.Unk10);
                    bw.Write(tr.Unk14);
                    bw.Write(tr.Unk18);
                    bw.Write(tr.Unk1C);
                }
            }
            // Write StructABB data
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    StructABB abb = tr.StructAbb;
                    if (abb == null) abb = new StructABB();
                    bw.Write(abb.Unk00);
                    bw.Write(abb.Unk01);
                    bw.Write(abb.Unk02);
                    bw.Write(abb.Unk03);
                    if (abb.Unk01 == 1)
                    {
                        bw.Write(abb.BehaviorMatrixParam_f);
                        bw.Write(abb.Unk08_int);
                        bw.Write(abb.Unk0C);
                        bw.Write(abb.Unk10_int);
                        bw.Write(abb.Unk14_int);
                        bw.Write(abb.Unk18_int);
                        // padding to 32 bytes
                        bw.Write(0);
                    }
                    else if (abb.Unk01 == 3)
                    {
                        bw.Write(abb.BehaviorMatrixParam_i);
                        bw.Write(abb.Unk08_f);
                        bw.Write(abb.Unk0C);
                        bw.Write(abb.Unk10_int);
                        bw.Write(abb.Unk14_int);
                        bw.Write(abb.Unk18_int);
                        bw.Write(abb.Unk1C_int);
                    }
                    else if (abb.Unk01 == 4)
                    {
                        bw.Write(abb.Unk04);
                        bw.Write(abb.Unk08_f);
                        bw.Write(abb.Unk0C);
                        bw.Write(abb.Unk10_int);
                        bw.Write(abb.Unk14_int);
                        bw.Write(abb.Unk18_int);
                        bw.Write(abb.Unk1C_int);
                        bw.Write(abb.Unk20_int);
                        bw.Write(abb.Unk24_int);
                        bw.Write(abb.Unk28_int);
                        bw.Write(abb.Unk2C_int);
                        bw.Write(abb.Unk30_int);
                        // padding to 64 bytes (we wrote 52, pad 12):
                        for (int p = 0; p < 3; p++) bw.Write(0);
                    }
                    else
                    {
                        // Unknown ABB type, write nothing beyond first 4 bytes (we already wrote 4 bytes).
                    }
                }
            }
            // Write Condition structures
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    foreach (var cond in tr.Conditions)
                    {
                        bw.Write(cond.Id);
                        bw.Write(cond.Unk01);
                        bw.Write(cond.Unk02);
                        bw.Write(cond.Unk03);
                        bw.Write(cond.DataOffset);
                        bw.Write(cond.Unk08);
                        bw.Write(cond.Unk09);
                        bw.Write(cond.Unk0A);
                        bw.Write(cond.Unk0B);
                        bw.Write(cond.Unk0C);
                    }
                }
            }
            // 写 Condition 数据区
            foreach (var st in States)
            {
                foreach (var tr in st.Transitions)
                {
                    foreach (var cond in tr.Conditions)
                    {
                        if (cond.DataLength > 0 && cond.Data.Count >= cond.DataLength)
                        {
                            bw.Write(cond.Data.ToArray(), 0, cond.DataLength);
                        }
                    }
                }
            }
            // Write StructB entries
            if (StructBs != null)
            {
                foreach (var sb in StructBs)
                {
                    bw.Write(sb.Unk00);
                    bw.Write(sb.Unk04);
                    bw.Write(sb.IsLooped);
                    bw.Write(sb.Unk08);
                    bw.Write(sb.Unk09);
                    bw.Write(sb.Unk0A);
                    bw.Write(sb.Unk0B);
                    bw.Write(sb.Unk0C);
                    bw.Write(sb.Unk10);
                    bw.Write(sb.Unk14);
                    bw.Write(sb.Unk18);
                    bw.Write(sb.Unk1C);
                    bw.Write(sb.Unk20);
                    bw.Write(sb.FacingLocked);
                    bw.Write(sb.Unk25);
                    bw.Write(sb.Unk26);
                    bw.Write(sb.Unk27);
                    bw.Write(sb.Unk28);
                    bw.Write(sb.Unk2C);
                    bw.Write(sb.Unk30);
                    bw.Write(sb.Unk34);
                    bw.Write(sb.Unk38);
                    bw.Write(sb.Unk3C);
                    if (Header.Version >= 6)
                    {
                        bw.Write(sb.Unk40);
                        bw.Write(sb.Unk44);
                        bw.Write(sb.Unk46);
                        bw.Write(sb.Unk48);
                        bw.Write(sb.Unk4A);
                        bw.Write(sb.Unk4C);
                        bw.Write(sb.Unk4E);
                    }
                    if (Header.Version >= 10)
                    {
                        bw.Write(sb.Unk50);
                        bw.Write(sb.Unk52);
                        bw.Write(sb.Unk54);
                        bw.Write(sb.Unk56);
                        bw.Write(0);
                        bw.Write(0);
                    }
                }
            }
            // Write StructC entries
            if (StructCs != null)
            {
                foreach (var cdata in StructCs)
                {
                    bw.Write(cdata.ToArray());
                }
            }
            // Write StructD entries
            long structDStartPos = bw.BaseStream.Position;
            if (StructDs != null)
            {
                // We should update Header.OffsetD in case it changed after writing C
                if (Header.CountD > 0 && Header.OffsetD == 0)
                    Header.OffsetD = Header.OffsetC + Header.CountC * Header.SizeC;
                // Write D array
                List<long> daSectionOffsets = new List<long>();
                long daStartRel = Header.OffsetD + Header.CountD * 32;
                long daPointerRel = daStartRel;
                foreach (var sd in StructDs)
                {
                    if (sd.Count04 > 0)
                    {
                        sd.Offset00 = (int)daPointerRel;
                        daPointerRel += sd.Count04 * (Header.Version >= 10 ? 48 : 32);
                    }
                    else
                    {
                        sd.Offset00 = 0;
                    }
                }
                // Actually write D structs
                foreach (var sd in StructDs)
                {
                    bw.Write(sd.Offset00);
                    bw.Write(sd.Count04);
                    bw.Write(sd.Unk08);
                    bw.Write(sd.Unk0C);
                    bw.Write(sd.Unk10);
                    bw.Write(sd.Unk14);
                    bw.Write(sd.Unk18);
                    bw.Write(sd.Unk1C);
                }
                // Write StructDA sections
                foreach (var sd in StructDs)
                {
                    if (sd.Count04 > 0)
                    {
                        foreach (var da in sd.StructDAs)
                        {
                            bw.Write(da.Unk00);
                            bw.Write(da.Unk02);
                            bw.Write(da.Unk04);
                            bw.Write(da.Unk08);
                            bw.Write(da.Unk0C);
                            bw.Write(da.Unk10);
                            bw.Write(da.Unk14);
                            bw.Write(da.Unk18);
                            bw.Write(da.Unk1C);
                            if (Header.Version >= 10)
                            {
                                bw.Write(da.Unk20);
                                bw.Write(da.Unk24);
                                bw.Write(da.Unk28);
                                bw.Write(da.Unk2C);
                            }
                        }
                    }
                }
            }
            // Write Strings (basenormal)
            if (fileType == BHVType.BASENORMAL)
            {
                int count = (Strings != null ? Strings.Count : 0);
                bw.Write((short)count);
                long offsetsPos = bw.BaseStream.Position;
                // Reserve space for offsets
                for (int i = 0; i < count; i++)
                {
                    bw.Write((short)0);
                }
                List<short> strOffsets = new List<short>();
                for (int i = 0; i < count; i++)
                {
                    string s = Strings[i];
                    // record offset from start of strings content (current position - offsetsPosEnd)
                    long curPos = bw.BaseStream.Position;
                    // Calculate offset relative to after offsets array
                    long contentStartPos = offsetsPos + count * 2;
                    short offsetVal = (short)(curPos - contentStartPos);
                    strOffsets.Add(offsetVal);
                    // write string bytes and null terminator
                    byte[] strBytes = Encoding.UTF8.GetBytes(s);
                    bw.Write(strBytes);
                    bw.Write((byte)0);
                }
                // Go back and write offsets
                long endPos = bw.BaseStream.Position;
                bw.BaseStream.Seek(offsetsPos, SeekOrigin.Begin);
                foreach (short off in strOffsets)
                {
                    bw.Write(off);
                }
                bw.BaseStream.Seek(endPos, SeekOrigin.Begin);
            }

            // Now patch header fileSize and offsets
            long fileEndPos = bw.BaseStream.Position;
            int fileSizeValue = (int)fileEndPos;
            // fileSize might exclude header of 0x20? But in original they store full file length.
            // So yes full length.
            Header.FileSize = fileSizeValue;
            // Seek and patch
            // FileSize at headerPos+0x04
            bw.BaseStream.Seek(headerPos + 0x04, SeekOrigin.Begin);
            bw.Write(Header.FileSize);
            // statesOffset at headerPos+0x20
            bw.BaseStream.Seek(headerPos + 0x20, SeekOrigin.Begin);
            bw.Write(Header.StatesOffset);
            // offsetB at 0x28
            bw.BaseStream.Seek(headerPos + 0x28, SeekOrigin.Begin);
            bw.Write(Header.OffsetB);
            // offsetC at 0x30
            bw.BaseStream.Seek(headerPos + 0x30, SeekOrigin.Begin);
            bw.Write(Header.OffsetC);
            // offsetD at 0x38
            bw.BaseStream.Seek(headerPos + 0x38, SeekOrigin.Begin);
            bw.Write(Header.OffsetD);
            // Done
        }
    }

    public class Header
    {
        public short Version { get; set; }
        public short Unk02 { get; set; }
        public int FileSize { get; set; }

        // 新增：保存 0x08-0x1C 这 6 个 int
        [JsonIgnore]
        public byte[] UnknownHeader { get; set; }
        // 把上面原始字节以 HEX 字符串形式输出到 JSON
        [JsonProperty("UnknownHeaderHex")]
        public string UnknownHeaderHex
        {
            get => UnknownHeader != null
                ? BitConverter.ToString(UnknownHeader).Replace("-", " ")
                : null;
            set => UnknownHeader = ParseHexString(value, expectedLength: 24);
        }
        public int StatesOffset { get; set; }
        public int StateCount { get; set; }
        public int OffsetB { get; set; }
        public int CountB { get; set; }
        public int OffsetC { get; set; }
        public short CountC { get; set; }
        public short SizeC { get; set; }
        public int OffsetD { get; set; }
        public int CountD { get; set; }

        // 原有：保存版本依赖的 “mystery block”
        [JsonIgnore]
        public byte[] MysteryBlock { get; set; }
        // 也公开为 HEX 让 JSON 能看到
        [JsonProperty("MysteryBlockHex")]
        public string MysteryBlockHex
        {
            get => MysteryBlock != null
                ? BitConverter.ToString(MysteryBlock).Replace("-", " ")
                : null;
            set
            {
                // 长度不固定，解析所有给定的字节
                MysteryBlock = ParseHexString(value, expectedLength: -1);
            }
        }
        /// <summary>
        /// 工具：把 "AA BB CC" 样式的 HEX 字符串转成字节数组
        /// 如果 expectedLength&gt;=0，则校验长度，否则不校验
        /// </summary>
        private static byte[] ParseHexString(string hex, int expectedLength)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return expectedLength >= 0
                    ? new byte[expectedLength]
                    : Array.Empty<byte>();

            // 支持空格或连字符分隔
            var parts = hex
                .Trim()
                .Replace("-", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var bytes = parts.Select(s => Convert.ToByte(s, 16)).ToArray();
            if (expectedLength >= 0 && bytes.Length != expectedLength)
            {
                // 如果长度不符，自动填零或截断
                var tmp = new byte[expectedLength];
                Array.Copy(bytes, tmp, Math.Min(bytes.Length, expectedLength));
                return tmp;
            }
            return bytes;
        }
    }


    public class State
    {
        public int Index { get; set; }
        public short Unk00 { get; set; }
        public short Unk02 { get; set; }
        public int Offset04 { get; set; }
        public int TransitionsOffset { get; set; }
        public int TransitionCount { get; set; }
        public short StructBid { get; set; }
        public short Unk10 { get; set; }
        public int Unk14 { get; set; }
        public int Unk18 { get; set; }
        public int Unk1C { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }
        public int Unk28 { get; set; }
        public int Unk2C { get; set; }
        public int Unk30 { get; set; }
        public int Unk34 { get; set; }
        public int Unk38 { get; set; }
        public int Unk3C { get; set; }
        public float Unk40 { get; set; }
        public float Unk44 { get; set; }
        public float Unk48 { get; set; }
        public float RootmotionStatus { get; set; }
        public int Unk50 { get; set; }
        public int Unk54 { get; set; }
        public int Unk58 { get; set; }
        public int Unk5C { get; set; }
        public int Unk60 { get; set; }
        public short StructBsid2 { get; set; }
        public short UnkBsControlId { get; set; }
        public int Unk68 { get; set; }
        public int Unk6C { get; set; }
        public int BladeHomingControl { get; set; }
        public int Unk74 { get; set; }
        public int Unk78 { get; set; }
        public int Unk7C { get; set; }
        public int Unk80 { get; set; }
        public int WeaponRailAnimationCallingId { get; set; }
        public int Unk88 { get; set; }
        public int Unk8C { get; set; }
        public int Unk90 { get; set; }
        public int Unk94 { get; set; }
        public int Unk98 { get; set; }
        public int Unk9C { get; set; }
        public List<Transition> Transitions { get; set; } = new List<Transition>();
        public List<byte> Data { get; set; } = new List<byte>();
    }

    public class Transition
    {
        public int StateIndex { get; set; }
        public int ConditionsOffset { get; set; }
        public int ConditionCount { get; set; }
        public int Offset0C { get; set; }
        public int Unk10 { get; set; }
        public int Unk14 { get; set; }
        public int Unk18 { get; set; }
        public int Unk1C { get; set; }
        public List<Condition> Conditions { get; set; } = new List<Condition>();
        public StructABB StructAbb { get; set; } = new StructABB();
    }

    public class Condition
    {
        public byte Id { get; set; }
        public byte Unk01 { get; set; }
        public byte Unk02 { get; set; }
        public byte Unk03 { get; set; }
        public int DataOffset { get; set; }
        // 新增：记录每条 Data 的长度
        public int DataLength { get; set; }
        public byte Unk08 { get; set; }
        public byte Unk09 { get; set; }
        public byte Unk0A { get; set; }
        public byte Unk0B { get; set; }
        public int Unk0C { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();

    }

    public class StructABB
    {
        public byte Unk00 { get; set; }
        public byte Unk01 { get; set; }
        public byte Unk02 { get; set; }
        public byte Unk03 { get; set; }
        public int Type { get; set; } = 0;
        public float BehaviorMatrixParam_f { get; set; }
        public int BehaviorMatrixParam_i { get; set; }
        public int Unk08_int { get; set; }
        public float Unk08_f { get; set; }
        public int Unk0C { get; set; }
        public int Unk10_int { get; set; }
        public int Unk14_int { get; set; }
        public int Unk18_int { get; set; }
        public int Unk1C_int { get; set; }
        public int Unk04 { get; set; }
        public int Unk20_int { get; set; }
        public int Unk24_int { get; set; }
        public int Unk28_int { get; set; }
        public int Unk2C_int { get; set; }
        public int Unk30_int { get; set; }
    }

    public class StructB
    {
        public int Unk00 { get; set; }
        public short Unk04 { get; set; }
        public short IsLooped { get; set; }
        public byte Unk08 { get; set; }
        public byte Unk09 { get; set; }
        public byte Unk0A { get; set; }
        public byte Unk0B { get; set; }
        public int Unk0C { get; set; }
        public int Unk10 { get; set; }
        public int Unk14 { get; set; }
        public int Unk18 { get; set; }
        public int Unk1C { get; set; }
        public float Unk20 { get; set; }
        public byte FacingLocked { get; set; }
        public byte Unk25 { get; set; }
        public byte Unk26 { get; set; }
        public byte Unk27 { get; set; }
        public float Unk28 { get; set; }
        public float Unk2C { get; set; }
        public int Unk30 { get; set; }
        public int Unk34 { get; set; }
        public int Unk38 { get; set; }
        public int Unk3C { get; set; }
        public int Unk40 { get; set; }
        public short Unk44 { get; set; }
        public short Unk46 { get; set; }
        public short Unk48 { get; set; }
        public short Unk4A { get; set; }
        public short Unk4C { get; set; }
        public short Unk4E { get; set; }
        public short Unk50 { get; set; }
        public short Unk52 { get; set; }
        public short Unk54 { get; set; }
        public short Unk56 { get; set; }
    }

    public class StructD
    {
        public int Offset00 { get; set; }
        public int Count04 { get; set; }
        public int Unk08 { get; set; }
        public int Unk0C { get; set; }
        public int Unk10 { get; set; }
        public int Unk14 { get; set; }
        public int Unk18 { get; set; }
        public int Unk1C { get; set; }
        public List<StructDA> StructDAs { get; set; } = new List<StructDA>();
    }

    public class StructDA
    {
        public short Unk00 { get; set; }
        public short Unk02 { get; set; }
        public int Unk04 { get; set; }
        public float Unk08 { get; set; }
        public float Unk0C { get; set; }
        public float Unk10 { get; set; }
        public float Unk14 { get; set; }
        public float Unk18 { get; set; }
        public float Unk1C { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }
        public int Unk28 { get; set; }
        public int Unk2C { get; set; }
    }
}
