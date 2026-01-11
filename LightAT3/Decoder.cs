using System.Linq;

namespace LightAT3
{
    public sealed class DecoderChnACCData_Entry
    {
        public int num_acc;
        public int[] data0 = new int[7];
        public uint[] data1 = new uint[7];
        public int acc_now;
        public int[] reserved = new int[0x16];
    }

    public sealed class DecoderChnACCData
    {
        public DecoderChnACCData_Entry[] table = CreateTable();
        public DecoderPackTable0 acc = new DecoderPackTable0();

        static public DecoderChnACCData_Entry[] CreateTable()
        {
            return Enumerable.Range(0, 0x10).Select(Index => new DecoderChnACCData_Entry()).ToArray();
        }

        public void tableMemset()
        {
            table = CreateTable();
        }
    }

    unsafe public sealed class DecoderChnACCTableTable
    {
        public int[] unk = new int[8];
        public int num_uk;
        public ManagedPointer<DecoderChnACCTableData> ptr0;
    }

    public sealed class DecoderChnACCTable
    {
        public DecoderChnACCTableInner inner;
        public DecoderChnACCTableTable[] table = CreateTable();

        static public DecoderChnACCTableTable[] CreateTable()
        {
            return Enumerable.Range(0, 0x10).Select(Index => new DecoderChnACCTableTable()).ToArray();
        }

        public void tableReset()
        {
            table = CreateTable();
        }
    }

    unsafe public sealed class DecoderChnInfo
    {
        public int chn_flag;
        public DecoderJointChnInfo joint_chn_info = new DecoderJointChnInfo();
        public DecoderChnACCData acc_data_now = new DecoderChnACCData();
        public DecoderChnACCData acc_data_old = new DecoderChnACCData();
        public DecoderChnACCTable acc_table_old = new DecoderChnACCTable();
        public DecoderChnACCTable acc_table_now = new DecoderChnACCTable();
        public DecoderChnInfo chn_ref;

        public uint var1034;
        public uint[] check_table0 = new uint[0x20];

        public int table0_flag_ex;
        public int table0_flag_data_num;
        public uint table0_data_num0;
        public uint table0_data_num1;

        public int uk1c718;
        public int uk1c714;

        public uint uk1b450;

        public uint[] table0 = new uint[0x20];
        public uint[] table1 = new uint[0x20];
        public uint[] table2 = new uint[0x20];
        public short[] table3 = new short[0x800];
        public uint[] table4 = new uint[0x10];

        public uint[] inner_pack_table0_check_table = new uint[0x10];
    }

    public sealed class DecoderChnACCTableData
    {
        public int unk0;
        public int unk1;
        public int unk2;
        public int unk3;
    }

    public sealed class DecoderChnACCTableInner
    {
        public int unk0;
        public int unk1;
        public int unk2;
        public DecoderChnACCTableData[] data = new DecoderChnACCTableData[0x30];
        public ManagedPointer<DecoderChnACCTableData> ptr_to_use_now;
        public DecoderPackTable0 table_unk0;
        public DecoderPackTable0 table_unk1;
        public DecoderPackTable0 table_unk2;
    }

    public sealed class DecoderPackTable0
    {
        public int check_data0;
        public int check_data1;
        public int[] data = new int[0x10];
    }

    public sealed class DecoderJointChnInfo
    {
        public DecoderPackTable0 table00 = new DecoderPackTable0();
        public DecoderPackTable0 table48 = new DecoderPackTable0();

        public uint num_band_splited_declared;
        public uint num_band_splited_used;

        public uint num_band_declared;
        public uint num_band_used;

        public uint joint_flag;
        public uint chns;

        public uint var90;
        public int var94;
        public int var98;
        public int var9c;
        public int var118;
        public int[] table11c = new int[0x100];
    }

    public sealed partial class Decoder
    {
        DecoderChnInfo[] chn_info = new DecoderChnInfo[2];

        float[][] kyou_buf = { new float[0x800], new float[0x800] };

        public Decoder()
        {
            for (int i = 0; i < 0x10; i++)
            {
                l2080_array[i] = new float[0x80];
            }

            DecoderJointChnInfo joint_chn_info = new DecoderJointChnInfo();

            DecoderChnACCTableInner inner0 = new DecoderChnACCTableInner();
            DecoderChnACCTableInner inner1 = new DecoderChnACCTableInner();

            for (int a0 = 0; a0 < 2; a0++)
            {
                chn_info[a0] = new DecoderChnInfo();

                chn_info[a0].chn_flag = a0;
                chn_info[a0].joint_chn_info = joint_chn_info;

                chn_info[a0].acc_data_old = new DecoderChnACCData();
                chn_info[a0].acc_data_now = new DecoderChnACCData();
                chn_info[a0].acc_table_old = new DecoderChnACCTable();
                chn_info[a0].acc_table_old.inner = inner0;

                chn_info[a0].acc_table_now = new DecoderChnACCTable();
                chn_info[a0].acc_table_now.inner = inner1;

                chn_info[a0].chn_ref = chn_info[0];
            }

            c900 = 3;

            for (int a0 = 0; a0 < 2; a0++)
            {
                for (int a1 = 0; a1 < 0x1000; a1++)
                {
                    syn_buf[a0][a1] = 0.0f;
                }
            }

            for (int a0 = 0; a0 < 2; a0++)
            {
                for (int a1 = 0; a1 < 0x800; a1++)
                {
                    kyou_buf[a0][a1] = 0.0f;
                }
            }
        }

        ~Decoder()
        {
            //heap0.free(chn_info[0].acc_table_now.inner);
            //heap0.free(chn_info[0].acc_table_old.inner);
            //heap0.free(chn_info[0].joint_chn_info);
            //for (int a0 = 0; a0 < 2; a0++)
            //{
            //	heap0.free(chn_info[a0].acc_table_now);
            //	heap0.free(chn_info[a0].acc_table_old);
            //	heap0.free(chn_info[a0].acc_data_now);
            //	heap0.free(chn_info[a0].acc_data_old);
            //	heap0.free(chn_info[a0]);
            //}
        }


        public int parseStream(BitReader mbr0, uint chns, uint joint_flag)
        {
            int rs = 0;

            chn_info[0].joint_chn_info.joint_flag = joint_flag;
            chn_info[0].joint_chn_info.chns = chns;

            while (true)
            {
                if (0 != (rs = decodeBandNum(mbr0, chn_info))) break;
                if (0 != (rs = decodeTable0(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeTable1(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeTable2(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeTable3(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeACC2Pre(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeACC2Main(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeACC6Inner(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;
                if (0 != (rs = decodeTailInfo(mbr0, chn_info, chn_info[0].joint_chn_info.chns))) break;

                break;
            }

            return rs;
        }


        int decodeBandNum(BitReader mbr0, DecoderChnInfo[] chn_infos)
        {
            chn_infos[0].joint_chn_info.num_band_splited_declared =
                (uint)mbr0.getWithI32Buffer(5) + 1;


            chn_infos[0].joint_chn_info.num_band_declared =
                (uint)Decoder_StaticData.MAPCDSD_band_num_table0[chn_infos[0].joint_chn_info.num_band_splited_declared] + 1;

            chn_infos[0].joint_chn_info.var118 =
                mbr0.getWithI32Buffer(1);

            return 0;
        }

        int decodeTable0(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                    chn_infos[a0].table0[a1] = 0;

                if ((rs = MAPCDSF_decodeTable0_func_list0[chn_infos[a0].chn_flag * 4 + mbr0.getWithI32Buffer(2)]
                    (mbr0, chn_infos[a0])
                    ) != 0)
                    return rs;
            }

            chn_infos[0].joint_chn_info.num_band_splited_used = chn_infos[0].joint_chn_info.num_band_splited_declared; //[B0]

            if (chns == 2)
            {
                while ((0 != chn_infos[0].joint_chn_info.num_band_splited_used) &&
                    (0 == chn_infos[0].table0[chn_infos[0].joint_chn_info.num_band_splited_used - 1]) &&
                    (0 == chn_infos[1].table0[chn_infos[0].joint_chn_info.num_band_splited_used - 1]))
                    chn_infos[0].joint_chn_info.num_band_splited_used--;
            }
            else
            {
                while ((0 != chn_infos[0].joint_chn_info.num_band_splited_used) &&
                    (0 == chn_infos[0].table0[chn_infos[0].joint_chn_info.num_band_splited_used - 1]))
                    chn_infos[0].joint_chn_info.num_band_splited_used--;
            }

            chn_infos[0].joint_chn_info.num_band_used =
                (uint)Decoder_StaticData.MAPCDSD_band_num_table0[chn_infos[0].joint_chn_info.num_band_splited_used] + 1;

            //check
            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                {
                    if ((chn_infos[a0].table0[a1] < 0) || (chn_infos[a0].table0[a1] >= 8)) return -0x10B;
                }
            }

            return rs;
        }

        int decodeTable1(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            if (chn_infos[0].joint_chn_info.num_band_splited_used != 0) //
            {
                for (uint a0 = 0; a0 < chns; a0++)
                {
                    for (uint a1 = 0; a1 < 0x20; a1++)
                        chn_infos[a0].table1[a1] = 0;

                    if (0 != (rs = MAPCDSF_decodeTable1_func_list0[chn_infos[a0].chn_flag * 4 + mbr0.getWithI32Buffer(2)]
                        (mbr0, chn_infos[a0])
                        ))
                        return rs;
                }
            }

            //check
            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x20; a1++)
                {
                    if ((chn_infos[a0].table1[a1] < 0) ||
                        (chn_infos[a0].table1[a1] >= 0x40)
                        )
                        return -0x110;
                }
            }

            return rs;
        }

        int decodeTable2(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            if (chn_infos[0].joint_chn_info.num_band_splited_used != 0) //
            {
                chn_infos[0].joint_chn_info.var90 = (uint)mbr0.getWithI32Buffer(1);  //[90] tmp4 [arg1]

                for (uint a0 = 0; a0 < chns; a0++)
                {
                    for (uint a1 = 0; a1 < 0x20; a1++)
                        chn_infos[a0].table2[a1] = 0;

                    chn_infos[a0].var1034 = (uint)mbr0.getWithI32Buffer(1);  //[1034] tmp5

                    MAPCDSF_makeTable0CheckTable(chn_infos[a0], chn_infos[a0].check_table0); //check

                    if (0 != (rs = MAPCDSF_decodeTable2_func_list0[chn_infos[a0].chn_flag * 4 + mbr0.getWithI32Buffer(2)]
                        (mbr0, chn_infos[a0])
                        ))
                        return rs;
                }

                //comp
            }

            return rs;
        }

        int decodeTable3(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                for (uint a1 = 0; a1 < 0x800; a1++) chn_infos[a0].table3[a1] = 0;
                for (uint a1 = 0; a1 < 0x5; a1++) chn_infos[a0].table4[a1] = 0x0F;

                for (uint a1 = 0; a1 < chn_infos[0].joint_chn_info.num_band_splited_used; a1++)
                {
                    if (chn_infos[a0].table0[a1] == 0)
                    {
                        for (uint a2 = 0; a2 < Decoder_StaticData.MAPCDSD_bind_table1[a1]; a2++) chn_infos[a0].table3[Decoder_StaticData.MAPCDSD_bind_table0[a1] + a2] = 0;
                    }
                    else
                    {
                        uint atmp0 = 0;

                        if (0 == chn_infos[a0].joint_chn_info.var90) atmp0 = Decoder_StaticData.MAPCDSD_bind_table2[(chn_infos[a0].var1034 * 7 + chn_infos[a0].table0[a1]) * 4 + chn_infos[a0].table2[a1]]; //tmp4 5 6
                        else atmp0 = chn_infos[a0].table2[a1];

                        DecoderSearchTableDes huff_table_now = Decoder_StaticData.MAPCDSD_huff_table3[(chn_infos[a0].var1034 * 8 + atmp0) * 7 + chn_infos[a0].table0[a1]]; //tmp5 6

                        MAPCDSF_decodeTable3Sub0(mbr0, chn_infos[a0].table3.GetPointer((int)Decoder_StaticData.MAPCDSD_bind_table0[a1]), Decoder_StaticData.MAPCDSD_bind_table1[a1], huff_table_now);
                    }
                }

                if (chn_infos[0].joint_chn_info.num_band_splited_used > 2)
                {
                    for (uint a1 = 0; a1 < (uint)(Decoder_StaticData.MAPCDSD_band_num_table1[Decoder_StaticData.MAPCDSD_band_num_table0[chn_infos[0].joint_chn_info.num_band_splited_used] + 1] + 1); a1++)
                    {
                        chn_infos[a0].table4[a1] = (uint)mbr0.getWithI32Buffer(4);
                    }
                }
            }

            if (chns == 2)
            {
                MAPCDSF_readPackTable0(mbr0, (chn_infos[0].joint_chn_info.table48), chn_infos[0].joint_chn_info.num_band_used);
                MAPCDSF_readPackTable0(mbr0, (chn_infos[0].joint_chn_info.table00), chn_infos[0].joint_chn_info.num_band_used);
            }


            return rs;
        }

        int decodeACC2Pre(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                MAPCDSF_readPackTable0(mbr0, (chn_infos[a0].acc_data_now.acc), chn_infos[0].joint_chn_info.num_band_declared);
            }

            return rs;
        }

        int decodeACC2Main(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            for (uint a0 = 0; a0 < chns; a0++)
            {
                chn_infos[a0].acc_data_now.tableMemset();

                uint uk1b444 = (uint)mbr0.getWithI32Buffer(1);

                if (uk1b444 != 0)
                {
                    chn_infos[a0].uk1b450 = (uint)mbr0.getWithI32Buffer(4) + 1;
                    uint uk1b448 = (uint)mbr0.getWithI32Buffer(1);

                    uint uk1b44c = chn_infos[a0].uk1b450;
                    if (uk1b448 != 0) uk1b44c = (uint)mbr0.getWithI32Buffer(4) + 1;

                    //call 478200
                    if ((rs = MAPCDSF_decodeACC2MainSub0(mbr0, chn_infos[a0])) != 0)
                        break;

                    //call 478270
                    if ((rs = MAPCDSF_decodeACC2MainSub1(mbr0, chn_infos[a0])) != 0)
                        break;

                    //call 478330
                    if ((rs = MAPCDSF_decodeACC2MainSub2(mbr0, chn_infos[a0])) != 0)
                        break;

                    if (uk1b448 != 0)
                    {
                        for (uint b0 = chn_infos[a0].uk1b450; b0 < uk1b44c; b0++)
                        {
                            chn_infos[a0].acc_data_now.table[b0].num_acc =
                                chn_infos[a0].acc_data_now.table[b0 - 1].num_acc;
                            for (uint b1 = 0; b1 < (uint)chn_infos[a0].acc_data_now.table[b0].num_acc; b1++)
                            {
                                chn_infos[a0].acc_data_now.table[b0].data1[b1] =
                                    chn_infos[a0].acc_data_now.table[b0 - 1].data1[b1];
                                chn_infos[a0].acc_data_now.table[b0].data0[b1] =
                                    chn_infos[a0].acc_data_now.table[b0 - 1].data0[b1];
                            }
                        }
                    }
                }
                //else
                //{
                //	uint uk1b44c = 0;
                //}

            }

            return rs;
        }

        //extern DecoderSearchTableDes[] MAPCDSD_huff_table_global_11;

        static void Mai_memcpy<T>(out T dst, ref T src)
        {
            dst = src;
        }

        static int decodeACC6Inner(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            if (chns == 2)
            {
                chn_infos[0].acc_table_now.inner.table_unk0 = new DecoderPackTable0();
                chn_infos[0].acc_table_now.inner.table_unk1 = new DecoderPackTable0();
                chn_infos[0].acc_table_now.inner.table_unk2 = new DecoderPackTable0();
            }

            for (uint a0 = 0; a0 < chns; a0++)
            {
                chn_infos[a0].acc_table_now.tableReset();
                for (uint a1 = 0; a1 < 0x10; a1++)
                {
                    chn_infos[a0].acc_table_now.table[a1].unk[7] = 0x20;
                    chn_infos[a0].acc_table_now.table[a1].unk[6] = 0;
                }
            }

            chn_infos[0].acc_table_now.inner.ptr_to_use_now =
                chn_infos[0].acc_table_now.inner.data;

            chn_infos[0].acc_table_now.inner.unk0 =
                mbr0.getWithI32Buffer(1);

            if (chn_infos[0].acc_table_now.inner.unk0 != 0)
            {
                chn_infos[0].acc_table_now.inner.unk1 =
                    mbr0.getWithI32Buffer(1);

                chn_infos[0].acc_table_now.inner.unk2 = (int)(
                    MAPCDSF_getHuffValue(Decoder_StaticData.MAPCDSD_huff_table_global_11[0], mbr0)
                    + 1
                );


                if (chns == 2)
                {
                    MAPCDSF_readPackTable0(mbr0, chn_infos[0].acc_table_now.inner.table_unk0, (uint)chn_infos[0].acc_table_now.inner.unk2);
                    MAPCDSF_readPackTable0(mbr0, chn_infos[0].acc_table_now.inner.table_unk2, (uint)chn_infos[0].acc_table_now.inner.unk2);
                    MAPCDSF_readPackTable0(mbr0, chn_infos[0].acc_table_now.inner.table_unk1, (uint)chn_infos[0].acc_table_now.inner.unk2);
                }

                for (uint a0 = 0; a0 < chns; a0++)
                {
                    //call 477e60
                    if ((rs = MAPCDSF_decodeACC6InnerSub0(mbr0, chn_infos[a0])) != 0)
                        break;
                }

                if (chns == 2)
                {
                    for (int a0 = 0; a0 < chn_infos[0].acc_table_now.inner.unk2; a0++)
                    {
                        if (chn_infos[0].acc_table_now.inner.table_unk0.data[a0] != 0)
                        {
                            /*for (int a1 = 0; a1 < 0xA; a1++)
							{
								chn_infos[1].acc_table_now.table[a0].unk[a1] = 
									chn_infos[0].acc_table_now.table[a0].unk[a1];
								//memcpy?
							}*/
                            Mai_memcpy(
                                out chn_infos[1].acc_table_now.table[a0],
                                ref chn_infos[0].acc_table_now.table[a0]
                            );

                            //left to right acc5 copy 0x10 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x14 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x18 + 0x28 * a0 0x4 add zumi
                            //left to right acc5 copy 0x1C + 0x28 * a0 0x4 add zumi
                        }

                        if (chn_infos[0].acc_table_now.inner.table_unk2.data[a0] != 0)
                        {
                            //swap?
                            DecoderChnACCTableTable tmpbuf0;

                            Mai_memcpy(
                                out tmpbuf0,
                                ref chn_infos[1].acc_table_now.table[a0]
                            );

                            Mai_memcpy(
                                out chn_infos[1].acc_table_now.table[a0],
                                ref chn_infos[0].acc_table_now.table[a0]
                            );

                            Mai_memcpy(
                                out chn_infos[0].acc_table_now.table[a0],
                                ref tmpbuf0
                            );
                            /*
							int tmpbuf0[0xA];
							for (int a1 = 0; a1 < 0xA; a1++)
							{
								tmpbuf0[a1] = *(int*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4];
							}
							for (int a1 = 0; a1 < 0xA; a1++)
							{
								*(int*)&infos.acc_struct_6_1.table0[a0 * 0x28 + a1 * 0x4] = 
									*(int*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4];
							}
							for (int a1 = 0; a1 < 0xA; a1++)
							{
								*(int*)&infos.acc_struct_6_0.table0[a0 * 0x28 + a1 * 0x4] = 
									tmpbuf0[a1];
							}*/
                        }
                    }
                }
            }
            return rs;
        }

        int decodeTailInfo(BitReader mbr0, DecoderChnInfo[] chn_infos, uint chns)
        {
            int rs = 0;

            chn_infos[0].joint_chn_info.var94 = mbr0.getWithI32Buffer(1);

            if (chn_infos[0].joint_chn_info.var94 != 0)
            {
                chn_infos[0].joint_chn_info.var98 = mbr0.getWithI32Buffer(4);
                chn_infos[0].joint_chn_info.var9c = mbr0.getWithI32Buffer(4);
            }

            return rs;
        }
    }
}
