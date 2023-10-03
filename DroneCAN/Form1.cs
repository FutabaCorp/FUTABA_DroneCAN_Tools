/* libcanard (https://github.com/UAVCAN/libcanard)                                      */
/* Copyright (c) 2016-2020 UAVCAN Development Team                                      */
/* Licensed under MIT (https://https://github.com/UAVCAN/libcanard/blob/master/LICENSE) */

using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DroneCAN
{
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    struct FP32
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public uint u;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public float f;
    }


    public partial class Form1 : Form
    {
        bool servo_flag = false;
        bool servo_timer_flag = false;
        bool update_flag = false;
        bool update_complete_flag = false;

        byte readByte;
        byte pos_ = 0;
        byte[] buf_ = new byte[200];

        uint id;
        byte data_len;
        byte[] data = new byte[8];
        ushort payload_pos = 0;
        byte[] rx_payload = new byte[264];

        byte[] received_unique_id = new byte[16 + 1];
        byte received_unique_id_len = 1;
        byte PreferredNodeID = 125;
        byte ServoNodeID = 125;

        byte priority;
        byte local_node_id = 127;
        ushort data_type_id;
        ulong data_type_signature;
        ushort crc;
        byte[] payload = new byte[264];
        ushort payload_len;
        byte transfer_id;
        byte transfer_id_dna = 0;
        byte transfer_id_aac = 0;
        byte transfer_id_pgs = 0;
        byte transfer_id_peo = 0;
        byte transfer_id_pfr = 0;

        ushort tx_data_len;
        byte[] tx_data = new byte[8];

        byte node_health;

        byte actuator_id;
        float position_deg;
        float position_deg2;
        float position_deg3 = 0;
        int[] position_buf = new int[120];
        int[] speed_buf = new int[120];
        int[] trque_buf = new int[120];
        int[] temprature_buf = new int[120];
        int[] voltage_buf = new int[120];

        int parameter_tab = 0;
        int parameter_tab3 = 0;

        byte Command_counter = 0;

        int parameter_read_counter = 0;
        int present_position;
        int present_speed;
        int present_torque;
        int present_temperature;
        int present_voltage;

        uint Info_Total;
        uint Info_Notice;
        uint Info_Warning;
        uint Info_Fault;

        byte Configuration_counter = 0;
        byte Configuration_button_clicked = 0;
        byte Enable_Torque;
        byte Enable_Soft_Start;
        byte Enable_Smoothing;
        byte Enable_Reverse;
        byte Enable_MultiTurn;
        byte Enable_Speed_Torque_Control;
        byte No_command_Operation;
        ushort No_command_Time;
        short OC_Protection;

        byte Control_counter = 0;
        byte Control_button_clicked = 0;
        ushort Angle_Prop_Gain;
        ushort Angle_Diff_Gain;
        ushort Angle_Dead_band;
        ushort Speed_Prop_Gain;
        ushort Speed_Intg_Gain;
        ushort Speed_Dead_band;
        uint Speed_Intg_Limit;
        ushort PWMIN_PulseWidth_Neutral;
        ushort PWMIN_PulseWidth_Range;
        ushort PWMIN_PulseWidth_Target;
        byte PWMIN_Target_Mode;
        ushort Angle_Boost;
        int comboBox4_Selected_Now;

        byte Limit_counter = 0;
        byte Limit_button_clicked = 0;
        int Limit_Angle_CW;
        int Limit_Angle_CCW;
        short Limit_Speed_CW;
        short Limit_Speed_CCW;
        short Limit_Torque_CW;
        short Limit_Torque_CCW;
        sbyte Limit_Temperature_High;
        sbyte Limit_Temperature_Low;
        short Limit_Voltage_High;
        short Limit_Voltage_Low;

        byte Option_counter = 0;
        short Origin_Position;
        byte Actuator_ID;
        byte DroneCAN_Node_ID;

        byte Manufacture_counter = 0;
        byte Model_Number;
        uint Unique_Number;
        ushort Firmware_Version;
        ushort Hardware_Version;
        ushort Manufacture_year;
        byte Manufacture_month;
        byte Manufacture_date;
        byte Manufacture_hours;
        byte Manufacture_minutes;
        byte[] BinFileBytes = new byte[0xA000];
        ushort offset = 0;


        /********************************************************************************/
        /*  ウィンドウ起動                                                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        public Form1()
        {
            InitializeComponent();
        }


        /********************************************************************************/
        /*  COM選択                                                                     */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }


        /********************************************************************************/
        /*  シリアルポート接続                                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "") // COM未接続時、エラー表示
            {
                MessageBox.Show("COM Port Error",
                   "Error",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
            }
            else
            {
                if (serialPort1.IsOpen)
                { }
                else
                {
                    serialPort1.BaudRate = 1000000;
                    serialPort1.Parity = Parity.None;
                    serialPort1.DataBits = 8;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Handshake = Handshake.None;
                    serialPort1.PortName = comboBox1.Text;

                    try
                    {
                        serialPort1.Open();
                        button1.Enabled = false; // 接続ボタン無効
                        button2.Enabled = true; // 切断ボタン有効
                        lbLed1.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Adapterランプ点灯
                        serialPort1.Write("O\r"); // O:CAN通信有効(SLCAN変換器)
                        // 構成パラメータ取得カウンタ初期化
                        Command_counter = 1;
                        Configuration_counter = 1;
                        Control_counter = 1;
                        Limit_counter = 1;
                        Option_counter = 1;
                        Manufacture_counter = 1;
                    }
                    catch
                    {
                        MessageBox.Show("COM Port Access Error",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        /********************************************************************************/
        /*  シリアルポート切断                                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("C\r"); // C:CAN通信無効(SLCAN変換器)
                serialPort1.Close();
                button1.Enabled = true; // 接続ボタン有効
                button2.Enabled = false; // 切断ボタン無効
                servo_flag = false; // サーボ操作無効
                lbLed1.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Adapterランプ消灯
                lbLed2.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Servoランプ消灯
                button3.Enabled = false; // writeボタン無効
                button4.Enabled = false; // defaultボタン無効
                button5.Enabled = false; // resetボタン無効

                Command_counter = 0; // Command取得カウンタ停止
                button6.Enabled = false; // Target Angleボタン無効
                button7.Enabled = false; // Target Angleボタン無効
                button8.Enabled = false; // Target Angleボタン無効
                button9.Enabled = false; // Target Speedボタン無効
                button10.Enabled = false; // Target Speedボタン無効
                button11.Enabled = false; // Target Speedボタン無効
                button12.Enabled = false; // Target Torqueボタン無効
                button13.Enabled = false; // Target Torqueボタン無効
                button14.Enabled = false; // Target Torqueボタン無効

                button15.Enabled = false; // Initializeボタン無効
                button16.Enabled = false; // Rebootボタン無効
                button17.Enabled = false; // WriteROMボタン無効

                Configuration_counter = 0; // Configuration取得カウンタ停止
                button18.Enabled = false; // No command Timeボタン無効
                button42.Enabled = false; // OC Protection ボタン無効

                Control_counter = 0; // Control取得カウンタ停止
                button21.Enabled = false; // Angle Prop Gainボタン無効
                button22.Enabled = false; // Angle Diff Gainボタン無効
                button23.Enabled = false; // Angle Dead bandボタン無効
                button24.Enabled = false; // Speed Prop Gainボタン無効
                button25.Enabled = false; // Speed Intg Gainボタン無効
                button26.Enabled = false; // Speed Dead bandボタン無効
                button27.Enabled = false; // Speed Intg Limitボタン無効
                button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
                button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
                button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効

                Limit_counter = 0; // Limit取得カウンタ停止
                button28.Enabled = false; // Limit Angle CWボタン無効
                button29.Enabled = false; // Limit Angle CCWボタン無効
                button30.Enabled = false; // Limit Speed CWボタン無効
                button31.Enabled = false; // Limit Speed CCWボタン無効
                button32.Enabled = false; // Limit Torque CWボタン無効
                button33.Enabled = false; // Limit Torque CCWボタン無効
                button34.Enabled = false; // Limit Temperature Highボタン無効
                button35.Enabled = false; // Limit Temperature Lowボタン無効
                button36.Enabled = false; // Limit Voltage Highボタン無効
                button37.Enabled = false; // Limit Voltage Lowボタン無効

                Option_counter = 0; // Option取得カウンタ停止
                button38.Enabled = false; // Origin Positionボタン無効
                button39.Enabled = false; // Actuator IDボタン無効
                button40.Enabled = false; // DroneCAN Node IDボタン無効

                Manufacture_counter = 0; // Manufacture取得カウンタ停止
            }
        }


        /********************************************************************************/
        /*  受信データ読み込み                                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (true)
            {
                readByte = (byte)serialPort1.ReadByte();
                if ((readByte >= 32 && readByte <= 126)) // 印字可能なASCIIコードの場合
                {
                    if (pos_ < 200) // バッファサイズ
                    {
                        buf_[pos_] = readByte;
                        pos_ += 1;
                    }
                    else
                    {
                        pos_ = 0;
                        serialPort1.DiscardInBuffer(); // バッファの初期化
                        break;
                    }
                }
                else if (readByte == (byte)'\r') // CR:CANフレーム終端(SLCAN変換器)
                {
                    buf_[pos_] = (byte)'\0';
                    command_identification(buf_); // SLCANコマンド判別
                    pos_ = 0;
                    break;
                }
                else
                {
                    pos_ = 0;
                    serialPort1.DiscardInBuffer(); // バッファの初期化
                    break;
                }
            }
        }


        /********************************************************************************/
        /*  SLCANコマンド識別                                                           */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void command_identification(byte[] cmd)
        {
            if (cmd[0] == 'T')
            {
                handle_FrameDataExt(cmd);
            }
        }


        /********************************************************************************/
        /*  拡張IDデータフレームCANデータ受信                                           */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_FrameDataExt(byte[] cmd)
        {
            // ID
            id = (uint)((1 << 31) |
                          (hex2nibble(cmd[1]) << 28) |
                          (hex2nibble(cmd[2]) << 24) |
                          (hex2nibble(cmd[3]) << 20) |
                          (hex2nibble(cmd[4]) << 16) |
                          (hex2nibble(cmd[5]) << 12) |
                          (hex2nibble(cmd[6]) << 8) |
                          (hex2nibble(cmd[7]) << 4) |
                          (hex2nibble(cmd[8]) << 0));

            // DLC
            data_len = hex2nibble(cmd[9]);

            // Data
            byte p = 10;
            for (byte i = 0; i < data_len; i++)
            {
                data[i] = (byte)((hex2nibble(cmd[p]) << 4) | hex2nibble(cmd[p + 1]));
                p += 2;
            }

            if (((data[data_len - 1] >> 6) & 0x3) == 0x3) // シングルフレーム
            {
                payload_pos = 0;
                Array.Clear(rx_payload, 0, 264);
                for (ushort i = 0; i < (data_len - 1); i++)
                {
                    rx_payload[payload_pos] = data[i];
                    payload_pos++;
                }
                transfer_id = data[data_len - 1];
                Data_Type_ID_identification(id, rx_payload, payload_pos, transfer_id);
                payload_pos = 0;
                serialPort1.DiscardInBuffer(); // COMバッファの初期化
            }
            else if (((data[data_len - 1] >> 6) & 0x3) == 0x2) // マルチフレーム(先頭)
            {
                payload_pos = 0;
                Array.Clear(rx_payload, 0, 264);
                for (ushort i = 2; i < (data_len - 1); i++) // 先頭2バイトのCRCは無視
                {
                    rx_payload[payload_pos] = data[i];
                    payload_pos++;
                }
            }
            else if (((data[data_len - 1] >> 6) & 0x3) == 0x1) // マルチフレーム(末尾)
            {
                for (ushort i = 0; i < (data_len - 1); i++)
                {
                    rx_payload[payload_pos] = data[i];
                    payload_pos++;
                }
                transfer_id = data[data_len - 1];
                Data_Type_ID_identification(id, rx_payload, payload_pos, transfer_id);
                payload_pos = 0;
                serialPort1.DiscardInBuffer(); // COMバッファの初期化
            }
            else // マルチフレーム(中間)
            {
                for (ushort i = 0; i < (data_len - 1); i++)
                {
                    rx_payload[payload_pos] = data[i];
                    payload_pos++;
                }
            }
        }


        /********************************************************************************/
        /*  Data Type ID識別                                                            */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void Data_Type_ID_identification(uint id, byte[] data, ushort payload_pos, byte transfer_id)
        {
            if (((id >> 7) & 0x1) == 0) // TransferTypeBroadcast
            {
                if ((id & 0x7F) == 0x00) // BroadcastNodeID
                {
                    data_type_id = (ushort)((id >> 8) & 0x3);

                    if (data_type_id == 1) // uavcan.protocol.dynamic_node_id.Allocation
                    {
                        handle_allocation_node(data, payload_pos);
                    }
                }
                else // NodeID:1～127
                {
                    data_type_id = (ushort)((id >> 8) & 0xFFFF);

                    if (data_type_id == 341) // uavcan.protocol.NodeStatus
                    {
                        handle_node_status(data);
                    }
                    else if (data_type_id == 1011) // uavcan.equipment.actuator.Status
                    {
                        handle_actuator_status(id, data);
                    }
                }
            }
            else if (((id >> 15) & 0x1) == 1) // TransferTypeRequest
            {
                data_type_id = (ushort)((id >> 16) & 0xFF);

                if (data_type_id == 48) // uavcan.protocol.file.Read
                {
                    handle_file_read(id, data, transfer_id);
                }
            }
            else // TransferTypeResponse
            {
                data_type_id = (ushort)((id >> 16) & 0xFF);

                if (data_type_id == 11) // uavcan.protocol.param.GetSet
                {
                    handle_param_getset(data);
                }
            }
        }


        /********************************************************************************/
        /*  uavcan.protocol.dynamic_node_id.Allocation受信データ処理                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_allocation_node(byte[] data, ushort payload_pos)
        {
            byte first_part_of_unique_id = 0;
            timer1.Stop();
            timer1.Start();

            // first_part_of_unique_idで1フレーム目か識別
            first_part_of_unique_id = (byte)(data[0] & 0x01);
            if (first_part_of_unique_id == 1)
            {
                received_unique_id_len = 1;
                Array.Clear(received_unique_id, 0, 16 + 1); // バッファの初期化
            }
            else if (received_unique_id_len == 1)
            {
                return; // 対象DNAリクエスト以外の割り当てメッセージを無視
            }

            for (byte o = 0; o < (payload_pos - 1); o++)
            {
                received_unique_id[received_unique_id_len] = data[o + 1];
                received_unique_id_len++;
            }

            if (received_unique_id_len < 16 + 1) // 3フレーム未満の場合(固有IDは6/6/4バイトの3フレームに分けて送信する)
            {
                received_unique_id[0] = 0x00;
            }
            else if (received_unique_id_len == 16 + 1) // 3フレーム受信完了時
            {
                received_unique_id[0] = (byte)(PreferredNodeID << 1);
                /*                PreferredNodeID--; // Node IDを125から降順に設定
                                if (PreferredNodeID < 1)
                                    PreferredNodeID = 125;*/
            }
            else // 固有IDが16Byteを超える場合
            {
                received_unique_id_len = 1;
                Array.Clear(received_unique_id, 0, 16 + 1); // バッファの初期化
                return;
            }

            priority = 24; // ノード優先度(低[24])
            payload = received_unique_id;
            payload_len = received_unique_id_len;

            data_type_id = 1;
            data_type_signature = 0x0b2a812620a11d40; // uavcan.protocol.dynamic_node_id.Allocation
            RequestOrRespond(data_type_id, data_type_signature, transfer_id_dna, priority, payload, payload_len);
            transfer_id_dna = incrementTransferID(transfer_id_dna);
        }


        /********************************************************************************/
        /*  uavcan.protocol.dynamic_node_id.Allocationタイムアウト                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void timer1_Tick(object sender, EventArgs e)
        {
            received_unique_id_len = 1;
            Array.Clear(received_unique_id, 0, 16 + 1); // バッファの初期化
        }


        /********************************************************************************/
        /*  servoランプ タイマ処理                                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void timer4_Tick(object sender, EventArgs e)
        {
            if (update_flag == false)
            {
                if (update_complete_flag == true)
                {
                    update_complete_flag = false;
                    MessageBox.Show("Turn the power on again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    button2.Enabled = true; // 切断ボタン有効
                }
                if (servo_timer_flag == true)
                {
                    servo_timer_flag = false;
                    if (servo_flag == true)
                    {
                        if (node_health == 0)
                            lbLed2.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green;
                        else if (node_health == 1)
                            lbLed2.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Yellow;
                        else
                            lbLed2.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Red;

                        button3.Enabled = true; // writeボタン有効
                        button4.Enabled = true; // defaultボタン有効
                        button5.Enabled = true; // resetボタン有効
                    }
                }
                else
                {
                    servo_flag = false; // サーボ操作無効
                    lbLed2.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Servoランプ消灯

                    Command_counter = 0; // Command取得カウンタ停止
                    button6.Enabled = false; // Target Angleボタン無効
                    button7.Enabled = false; // Target Angleボタン無効
                    button8.Enabled = false; // Target Angleボタン無効
                    button9.Enabled = false; // Target Speedボタン無効
                    button10.Enabled = false; // Target Speedボタン無効
                    button11.Enabled = false; // Target Speedボタン無効
                    button12.Enabled = false; // Target Torqueボタン無効
                    button13.Enabled = false; // Target Torqueボタン無効
                    button14.Enabled = false; // Target Torqueボタン無効

                    button15.Enabled = false; // Initializeボタン無効
                    button16.Enabled = false; // Rebootボタン無効
                    button17.Enabled = false; // WriteROMボタン無効

                    Configuration_counter = 0; // Configuration取得カウンタ停止
                    button18.Enabled = false; // No command Timeボタン無効
                    button42.Enabled = false; // OC Protection ボタン無効

                    Control_counter = 0; // Control取得カウンタ停止
                    button21.Enabled = false; // Angle Prop Gainボタン無効
                    button22.Enabled = false; // Angle Diff Gainボタン無効
                    button23.Enabled = false; // Angle Dead bandボタン無効
                    button24.Enabled = false; // Speed Prop Gainボタン無効
                    button25.Enabled = false; // Speed Intg Gainボタン無効
                    button26.Enabled = false; // Speed Dead bandボタン無効
                    button27.Enabled = false; // Speed Intg Limitボタン無効
                    button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
                    button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
                    button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効
                    button46.Enabled = false; // Angle_Boostボタン無効

                    Limit_counter = 0; // Limit取得カウンタ停止
                    button28.Enabled = false; // Limit Angle CWボタン無効
                    button29.Enabled = false; // Limit Angle CCWボタン無効
                    button30.Enabled = false; // Limit Speed CWボタン無効
                    button31.Enabled = false; // Limit Speed CCWボタン無効
                    button32.Enabled = false; // Limit Torque CWボタン無効
                    button33.Enabled = false; // Limit Torque CCWボタン無効
                    button34.Enabled = false; // Limit Temperature Highボタン無効
                    button35.Enabled = false; // Limit Temperature Lowボタン無効
                    button36.Enabled = false; // Limit Voltage Highボタン無効
                    button37.Enabled = false; // Limit Voltage Lowボタン無効

                    Option_counter = 0; // Option取得カウンタ停止
                    button38.Enabled = false; // Origin Positionボタン無効
                    button39.Enabled = false; // Actuator IDボタン無効
                    button40.Enabled = false; // DroneCAN Node IDボタン無効

                    Manufacture_counter = 0; // Manufacture取得カウンタ停止
                }
            }
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.GetSet受信データ処理                                  */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_param_getset(byte[] data)
        {
            byte tag;
            long read_value;
            byte[] name = new byte[4];

            tag = data[0]; // 0:write、1:read
            read_value = data[1] | ((long)data[2] << 8) | ((long)data[3] << 16) | ((long)data[4] << 24)
                       | ((long)data[5] << 32) | ((long)data[6] << 40) | ((long)data[7] << 48) | ((long)data[8] << 54); // レジスタ値
            name[0] = data[12]; // 0x**:レジスタアドレス
            name[1] = data[13];
            name[2] = data[14];
            name[3] = data[15];

            if (serialPort1.IsOpen)
            {
                servo_flag = true; // サーボ操作有効
                servo_timer_flag = true;
            }

            if ((tag == 1) && (name[0] == 0x30) && (name[1] == 0x78)) // 0x
            {
                if ((name[2] == 0x30) && (name[3] == 0x38)) // 0x08:Present Position
                {
                    present_position = (int)read_value;
                }
                else if ((name[2] == 0x30) && (name[3] == 0x39)) // 0x09:Present Speed
                {
                    present_speed = (int)read_value;
                }
                else if ((name[2] == 0x30) && (name[3] == 0x41)) // 0x0A:Present Torque
                {
                    present_torque = (int)read_value;
                }
                else if ((name[2] == 0x30) && (name[3] == 0x42)) // 0x0B:Present Temprature
                {
                    present_temperature = (int)read_value;
                }
                else if ((name[2] == 0x30) && (name[3] == 0x43)) // 0x0C:Present Voltage
                {
                    present_voltage = (int)read_value;
                }
                else if ((name[2] == 0x31) && (name[3] == 0x38)) // 0x18:Info Total
                {
                    Info_Total = (uint)read_value;
                }
                else if ((name[2] == 0x31) && (name[3] == 0x39)) // 0x19:Info Notice
                {
                    Info_Notice = (uint)read_value;
                }
                else if ((name[2] == 0x31) && (name[3] == 0x41)) // 0x1A:Info Warning
                {
                    Info_Warning = (uint)read_value;
                }
                else if ((name[2] == 0x31) && (name[3] == 0x42)) // 0x1B:Info Fault
                {
                    Info_Fault = (uint)read_value;
                }
                if ((name[2] == 0x32) && (name[3] == 0x30)) // 0x20:Enable Torque
                {
                    if (Configuration_counter == 1)
                    {
                        Enable_Torque = (byte)read_value;
                        Configuration_counter = 2;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x31)) // 0x21:Enable Soft Start
                {
                    if (Configuration_counter == 2)
                    {
                        Enable_Soft_Start = (byte)read_value;
                        Configuration_counter = 3;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x32)) // 0x22:Enable Smoothing
                {
                    if (Configuration_counter == 3)
                    {
                        Enable_Smoothing = (byte)read_value;
                        Configuration_counter = 4;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x33)) // 0x23:Enable Reverse
                {
                    if (Configuration_counter == 4)
                    {
                        Enable_Reverse = (byte)read_value;
                        Configuration_counter = 5;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x34)) // 0x24:Enable MultiTurn
                {
                    if (Configuration_counter == 5)
                    {
                        Enable_MultiTurn = (byte)read_value;
                        Configuration_counter = 6;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x35)) // 0x25:Enable Speed/Torque Control
                {
                    if (Configuration_counter == 6)
                    {
                        Enable_Speed_Torque_Control = (byte)read_value;
                        Configuration_counter = 7;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x36)) // 0x26:No command Operation
                {
                    if (Configuration_counter == 7)
                    {
                        No_command_Operation = (byte)read_value;
                        Configuration_counter = 8;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x37)) // 0x27:No command Time
                {
                    if (Configuration_counter == 8)
                    {
                        No_command_Time = (ushort)read_value;
                        Configuration_counter = 9;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x41)) // 0x2A:OC Protection 
                {
                    if (Configuration_counter == 9)
                    {
                        OC_Protection = (short)read_value;
                        Configuration_counter = 10;
                    }
                    else if (Configuration_button_clicked == 10)
                    {
                        OC_Protection = (short)read_value;
                        Configuration_button_clicked = 110;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x43)) // 0x2C:Angle Prop Gain
                {
                    if (Control_counter == 1)
                    {
                        Angle_Prop_Gain = (ushort)read_value;
                        Control_counter = 2;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x44)) // 0x2D:Angle Diff Gain
                {
                    if (Control_counter == 2)
                    {
                        Angle_Diff_Gain = (ushort)read_value;
                        Control_counter = 3;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x45)) // 0x2E:Angle Dead band
                {
                    if (Control_counter == 3)
                    {
                        Angle_Dead_band = (ushort)read_value;
                        Control_counter = 4;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x30)) // 0x30:Speed Prop Gain
                {
                    if (Control_counter == 4)
                    {
                        Speed_Prop_Gain = (ushort)read_value;
                        Control_counter = 5;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x31)) // 0x31:Speed Intg Gain
                {
                    if (Control_counter == 5)
                    {
                        Speed_Intg_Gain = (ushort)read_value;
                        Control_counter = 6;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x32)) // 0x32:Speed Dead band
                {
                    if (Control_counter == 6)
                    {
                        Speed_Dead_band = (ushort)read_value;
                        Control_counter = 7;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x33)) // 0x33:Speed Intg Limit
                {
                    if (Control_counter == 7)
                    {
                        Speed_Intg_Limit = (uint)read_value;
                        Control_counter = 8;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x34)) // 0x34:PWMIN_PulseWidth_Neutral
                {
                    if (Control_counter == 8)
                    {
                        PWMIN_PulseWidth_Neutral = (ushort)read_value;
                        Control_counter = 9;
                    }
                    else if (Control_button_clicked == 8)
                    {
                        PWMIN_PulseWidth_Neutral = (ushort)read_value;
                        Control_button_clicked = 108;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x35)) // 0x35:PWMIN_PulseWidth_Range
                {
                    if (Control_counter == 9)
                    {
                        PWMIN_PulseWidth_Range = (ushort)read_value;
                        Control_counter = 10;
                    }
                    else if (Control_button_clicked == 9)
                    {
                        PWMIN_PulseWidth_Range = (ushort)read_value;
                        Control_button_clicked = 109;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x36)) // 0x36:PWMIN_PulseWidth_Target
                {
                    if (Control_counter == 10)
                    {
                        PWMIN_PulseWidth_Target = (ushort)read_value;
                        Control_counter = 11;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x37)) // 0x37:PWMIN_Target_Mode
                {
                    if (Control_counter == 11)
                    {
                        PWMIN_Target_Mode = (byte)read_value;
                        Control_counter = 12;
                    }
                }
                else if ((name[2] == 0x32) && (name[3] == 0x46)) // 0x2F:Angle Boost
                {
                    if (Control_counter == 12)
                    {
                        Angle_Boost = (ushort)read_value;
                        Control_counter = 13;
                    }
                    else if (Control_button_clicked == 13)
                    {
                        Angle_Boost = (ushort)read_value;
                        Control_button_clicked = 113;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x38)) // 0x38:Limit Angle CW
                {
                    if (Limit_counter == 1)
                    {
                        Limit_Angle_CW = (int)read_value;
                        Limit_counter = 2;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x39)) // 0x39:Limit Angle CCW
                {
                    if (Limit_counter == 2)
                    {
                        Limit_Angle_CCW = (int)read_value;
                        Limit_counter = 3;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x41)) // 0x3A:Limit Speed CW
                {
                    if (Limit_counter == 3)
                    {
                        Limit_Speed_CW = (short)read_value;
                        Limit_counter = 4;
                    }
                    else if (Limit_button_clicked == 3)
                    {
                        Limit_Speed_CW = (short)read_value;
                        Limit_button_clicked = 103;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x42)) // 0x3B:Limit Speed CCW
                {
                    if (Limit_counter == 4)
                    {
                        Limit_Speed_CCW = (short)read_value;
                        Limit_counter = 5;
                    }
                    else if (Limit_button_clicked == 4)
                    {
                        Limit_Speed_CCW = (short)read_value;
                        Limit_button_clicked = 104;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x43)) // 0x3C:Limit Torque CW
                {
                    if (Limit_counter == 5)
                    {
                        Limit_Torque_CW = (short)read_value;
                        Limit_counter = 6;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x44)) // 0x3D:Limit Torque CCW
                {
                    if (Limit_counter == 6)
                    {
                        Limit_Torque_CCW = (short)read_value;
                        Limit_counter = 7;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x45)) // 0x3E:Limit Temperature High
                {
                    if (Limit_counter == 7)
                    {
                        Limit_Temperature_High = (sbyte)read_value;
                        Limit_counter = 8;
                    }
                }
                else if ((name[2] == 0x33) && (name[3] == 0x46)) // 0x3F:Limit Temperature Low
                {
                    if (Limit_counter == 8)
                    {
                        Limit_Temperature_Low = (sbyte)read_value;
                        Limit_counter = 9;
                    }
                }
                else if ((name[2] == 0x34) && (name[3] == 0x30)) // 0x40:Limit Voltage High
                {
                    if (Limit_counter == 9)
                    {
                        Limit_Voltage_High = (short)read_value;
                        Limit_counter = 10;
                    }
                    else if (Limit_button_clicked == 9)
                    {
                        Limit_Voltage_High = (short)read_value;
                        Limit_button_clicked = 109;
                    }
                }
                else if ((name[2] == 0x34) && (name[3] == 0x31)) // 0x41:Limit Voltage Low
                {
                    if (Limit_counter == 10)
                    {
                        Limit_Voltage_Low = (short)read_value;
                        Limit_counter = 11;
                    }
                    else if (Limit_button_clicked == 10)
                    {
                        Limit_Voltage_Low = (short)read_value;
                        Limit_button_clicked = 110;
                    }
                }
                else if ((name[2] == 0x34) && (name[3] == 0x34)) // 0x44:Origin Position
                {
                    if (Option_counter == 1)
                    {
                        Origin_Position = (short)read_value;
                        Option_counter = 2;
                    }
                }
                else if ((name[2] == 0x34) && (name[3] == 0x35)) // 0x45:Actuator ID
                {
                    if (Option_counter == 2)
                    {
                        Actuator_ID = (byte)read_value;
                        Option_counter = 3;
                    }
                }
                else if ((name[2] == 0x34) && (name[3] == 0x36)) // 0x46:DroneCAN Node ID
                {
                    if (Option_counter == 3)
                    {
                        DroneCAN_Node_ID = (byte)read_value;
                        Option_counter = 4;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x30)) // 0x50:Model Number
                {
                    if (Manufacture_counter == 1)
                    {
                        Model_Number = (byte)read_value;
                        Manufacture_counter = 2;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x31)) // 0x51:Unique Number
                {
                    if (Manufacture_counter == 2)
                    {
                        Unique_Number = (uint)read_value;
                        Manufacture_counter = 3;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x32)) // 0x52:Firmware Version
                {
                    if (Manufacture_counter == 3)
                    {
                        Firmware_Version = (ushort)read_value;
                        Manufacture_counter = 4;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x33)) // 0x53:Hardware Version
                {
                    if (Manufacture_counter == 4)
                    {
                        Hardware_Version = (ushort)read_value;
                        Manufacture_counter = 5;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x38)) // 0x58:Manufacture year
                {
                    if (Manufacture_counter == 5)
                    {
                        Manufacture_year = (ushort)read_value;
                        Manufacture_counter = 6;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x39)) // 0x59:Manufacture month
                {
                    if (Manufacture_counter == 6)
                    {
                        Manufacture_month = (byte)read_value;
                        Manufacture_counter = 7;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x41)) // 0x5A:Manufacture date
                {
                    if (Manufacture_counter == 7)
                    {
                        Manufacture_date = (byte)read_value;
                        Manufacture_counter = 8;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x42)) // 0x5B:Manufacture hours
                {
                    if (Manufacture_counter == 8)
                    {
                        Manufacture_hours = (byte)read_value;
                        Manufacture_counter = 9;
                    }
                }
                else if ((name[2] == 0x35) && (name[3] == 0x43)) // 0x5C:Manufacture minutes
                {
                    if (Manufacture_counter == 9)
                    {
                        Manufacture_minutes = (byte)read_value;
                        Manufacture_counter = 10;
                    }
                }
            }
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.ExecuteOpcode ROM一括保存処理                         */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button3_Click(object sender, EventArgs e)
        {
            byte opcode = 0; // ROM一括保存
            ulong argument = 0; // 48bits

            //    Array.Clear(payload, 0, 7); // バッファの初期化
            payload[0] = opcode;
            payload[1] = (byte)(argument & 0xff);
            payload[2] = (byte)((argument >> 8) & 0xff);
            payload[3] = (byte)((argument >> 16) & 0xff);
            payload[4] = (byte)((argument >> 24) & 0xff);
            payload[5] = (byte)((argument >> 32) & 0xff);
            payload[6] = (byte)((argument >> 40) & 0xff);

            priority = 16; // ノード優先度(中[16])
            payload_len = 7;

            data_type_id = 10;
            data_type_signature = 0x3b131ac5eb69d2cd; // uavcan.protocol.param.ExecuteOpcode
            Request(ServoNodeID, data_type_signature, data_type_id, transfer_id_peo, priority, payload, payload_len);
            transfer_id_peo = incrementTransferID(transfer_id_peo);
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.ExecuteOpcode ROM初期化処理                           */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button4_Click(object sender, EventArgs e)
        {
            byte opcode = 1; // デフォルト
            ulong argument = 0; // 48bits

            //   Array.Clear(payload, 0, 7); // バッファの初期化
            payload[0] = opcode;
            payload[1] = (byte)(argument & 0xff);
            payload[2] = (byte)((argument >> 8) & 0xff);
            payload[3] = (byte)((argument >> 16) & 0xff);
            payload[4] = (byte)((argument >> 24) & 0xff);
            payload[5] = (byte)((argument >> 32) & 0xff);
            payload[6] = (byte)((argument >> 40) & 0xff);

            priority = 16; // ノード優先度(中[16])
            payload_len = 7;

            data_type_id = 10;
            data_type_signature = 0x3b131ac5eb69d2cd; // uavcan.protocol.param.ExecuteOpcode
            Request(ServoNodeID, data_type_signature, data_type_id, transfer_id_peo, priority, payload, payload_len);
            transfer_id_peo = incrementTransferID(transfer_id_peo);

            Command_counter = 0; // Command取得カウンタ停止
            button6.Enabled = false; // Target Angleボタン無効
            button7.Enabled = false; // Target Angleボタン無効
            button8.Enabled = false; // Target Angleボタン無効
            button9.Enabled = false; // Target Speedボタン無効
            button10.Enabled = false; // Target Speedボタン無効
            button11.Enabled = false; // Target Speedボタン無効
            button12.Enabled = false; // Target Torqueボタン無効
            button13.Enabled = false; // Target Torqueボタン無効
            button14.Enabled = false; // Target Torqueボタン無効

            button15.Enabled = false; // Initializeボタン無効
            button16.Enabled = false; // Rebootボタン無効
            button17.Enabled = false; // WriteROMボタン無効

            Configuration_counter = 0; // Configuration取得カウンタ停止
            button18.Enabled = false; // No command Timeボタン無効
            button42.Enabled = false; // OC Protection ボタン無効

            Control_counter = 0; // Control取得カウンタ停止
            button21.Enabled = false; // Angle Prop Gainボタン無効
            button22.Enabled = false; // Angle Diff Gainボタン無効
            button23.Enabled = false; // Angle Dead bandボタン無効
            button24.Enabled = false; // Speed Prop Gainボタン無効
            button25.Enabled = false; // Speed Intg Gainボタン無効
            button26.Enabled = false; // Speed Dead bandボタン無効
            button27.Enabled = false; // Speed Intg Limitボタン無効
            button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
            button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
            button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効
            button46.Enabled = false; // Angle_Boostボタン無効

            Limit_counter = 0; // Limit取得カウンタ停止
            button28.Enabled = false; // Limit Angle CWボタン無効
            button29.Enabled = false; // Limit Angle CCWボタン無効
            button30.Enabled = false; // Limit Speed CWボタン無効
            button31.Enabled = false; // Limit Speed CCWボタン無効
            button32.Enabled = false; // Limit Torque CWボタン無効
            button33.Enabled = false; // Limit Torque CCWボタン無効
            button34.Enabled = false; // Limit Temperature Highボタン無効
            button35.Enabled = false; // Limit Temperature Lowボタン無効
            button36.Enabled = false; // Limit Voltage Highボタン無効
            button37.Enabled = false; // Limit Voltage Lowボタン無効

            Option_counter = 0; // Option取得カウンタ停止
            button38.Enabled = false; // Origin Positionボタン無効
            button39.Enabled = false; // Actuator IDボタン無効
            button40.Enabled = false; // DroneCAN Node IDボタン無効

            Manufacture_counter = 0; // Manufacture取得カウンタ停止
        }


        /********************************************************************************/
        /*  uavcan.protocol.RestartNode 再起動処理                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button5_Click(object sender, EventArgs e)
        {
            ulong magic_number = 0xACCE551B1E; // マジックナンバー

            payload[0] = (byte)(magic_number & 0xff);
            payload[1] = (byte)((magic_number >> 8) & 0xff);
            payload[2] = (byte)((magic_number >> 16) & 0xff);
            payload[3] = (byte)((magic_number >> 24) & 0xff);
            payload[4] = (byte)((magic_number >> 32) & 0xff);

            priority = 16; // ノード優先度(中[16])
            payload_len = 5;

            data_type_id = 5;
            data_type_signature = 0x569e05394a3017f0; // uavcan.protocol.RestartNode
            Request(ServoNodeID, data_type_signature, data_type_id, transfer_id_peo, priority, payload, payload_len);
            transfer_id_peo = incrementTransferID(transfer_id_peo);

            Command_counter = 0; // Command取得カウンタ停止
            button6.Enabled = false; // Target Angleボタン無効
            button7.Enabled = false; // Target Angleボタン無効
            button8.Enabled = false; // Target Angleボタン無効
            button9.Enabled = false; // Target Speedボタン無効
            button10.Enabled = false; // Target Speedボタン無効
            button11.Enabled = false; // Target Speedボタン無効
            button12.Enabled = false; // Target Torqueボタン無効
            button13.Enabled = false; // Target Torqueボタン無効
            button14.Enabled = false; // Target Torqueボタン無効

            button15.Enabled = false; // Initializeボタン無効
            button16.Enabled = false; // Rebootボタン無効
            button17.Enabled = false; // WriteROMボタン無効

            Configuration_counter = 0; // Configuration取得カウンタ停止
            button18.Enabled = false; // No command Timeボタン無効
            button42.Enabled = false; // OC Protection ボタン無効

            Control_counter = 0; // Control取得カウンタ停止
            button21.Enabled = false; // Angle Prop Gainボタン無効
            button22.Enabled = false; // Angle Diff Gainボタン無効
            button23.Enabled = false; // Angle Dead bandボタン無効
            button24.Enabled = false; // Speed Prop Gainボタン無効
            button25.Enabled = false; // Speed Intg Gainボタン無効
            button26.Enabled = false; // Speed Dead bandボタン無効
            button27.Enabled = false; // Speed Intg Limitボタン無効
            button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
            button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
            button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効
            button46.Enabled = false; // Angle_Boostボタン無効

            Limit_counter = 0; // Limit取得カウンタ停止
            button28.Enabled = false; // Limit Angle CWボタン無効
            button29.Enabled = false; // Limit Angle CCWボタン無効
            button30.Enabled = false; // Limit Speed CWボタン無効
            button31.Enabled = false; // Limit Speed CCWボタン無効
            button32.Enabled = false; // Limit Torque CWボタン無効
            button33.Enabled = false; // Limit Torque CCWボタン無効
            button34.Enabled = false; // Limit Temperature Highボタン無効
            button35.Enabled = false; // Limit Temperature Lowボタン無効
            button36.Enabled = false; // Limit Voltage Highボタン無効
            button37.Enabled = false; // Limit Voltage Lowボタン無効

            Option_counter = 0; // Option取得カウンタ停止
            button38.Enabled = false; // Origin Positionボタン無効
            button39.Enabled = false; // Actuator IDボタン無効
            button40.Enabled = false; // DroneCAN Node IDボタン無効

            Manufacture_counter = 0; // Manufacture取得カウンタ停止
        }


        /********************************************************************************/
        /*  uavcan.protocol.file.Read受信データ処理                                     */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_file_read(uint id, byte[] data, byte transfer_id)
        {
            offset = (ushort)(data[0] | (data[1] << 8));
            ushort error = 0x0000;
            byte[] file_payload = new byte[264];

            file_payload[0] = (byte)(error & 0xff);
            file_payload[1] = (byte)((error >> 8) & 0xff);
            payload_len = 2;
            for (ushort i = 0; i < 256; i++)
            {
                file_payload[(ushort)(i + 2)] = BinFileBytes[(ushort)(i + offset)];
                payload_len++;
                if ((i + offset) == (ushort)(BinFileBytes.Length - 1))
                    break;
            }

            priority = (byte)((byte)(id >> 24) & 0x7F); // ノード優先度

            data_type_id = 48;
            data_type_signature = 0x8dcdca939f33f678; // uavcan.protocol.file.Read
            transfer_id_pfr = (byte)((transfer_id + 1) & 0x1F);
            Respond(ServoNodeID, data_type_signature, data_type_id, transfer_id_pfr, priority, file_payload, payload_len);
            if (payload_len < 258)
            {
                if (update_flag == true)
                {
                    update_complete_flag = true;
                    update_flag = false;
                }
            }
        }


        /********************************************************************************/
        /*  uavcan.protocol.NodeStatus受信データ処理                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_node_status(byte[] data)
        {
            if (serialPort1.IsOpen)
            {
                servo_flag = true; // サーボ操作有効
                servo_timer_flag = true;
                node_health = (byte)((data[4] & 0xC0) >> 6);
            }
        }


        /********************************************************************************/
        /*  uavcan.equipment.actuator.Status受信データ処理                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void handle_actuator_status(uint id, byte[] data)
        {
            ushort position;

            ServoNodeID = (byte)(id & 0x7F);
            actuator_id = data[0]; // チャンネル番号
            position = (ushort)(data[1] | (data[2] << 8)); // 現在角度
            position_deg = ConvertFloat16ToNativeFloat(position);
            position_deg2 = position_deg * 100;

            if ((position_deg2 >= -2147483648) && (position_deg2 <= 2147483647))
                position_deg3 = position_deg2;

            if (serialPort1.IsOpen)
            {
                servo_flag = true; // サーボ操作有効
                servo_timer_flag = true;

                // サーボ再接続時、構成パラメータ取得開始
                if (Command_counter == 0)
                    Command_counter = 1;
                if (Configuration_counter == 0)
                    Configuration_counter = 1;
                if (Control_counter == 0)
                    Control_counter = 1;
                if (Limit_counter == 0)
                    Limit_counter = 1;
                if (Option_counter == 0)
                    Option_counter = 1;
                if (Manufacture_counter == 0)
                    Manufacture_counter = 1;
            }
        }


        /********************************************************************************/
        /*  uavcan.equipment.actuator.ArrayCommand送信処理                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void makeEquipmentActuatorArrayCommand(byte command_type, float command_position)
        {
            ushort command_value = ConvertNativeFloatToFloat16(command_position);

            payload[0] = actuator_id;
            payload[1] = command_type; // 0:単位無し、1:position、2:force、3:speed
            payload[2] = (byte)(command_value & 0xFF);
            payload[3] = (byte)((command_value >> 8) & 0xFF);

            priority = 16; // ノード優先度(中[16])
            payload_len = 4;

            data_type_id = 1010;
            data_type_signature = 0xd8a7486238ec3af3; // uavcan.equipment.actuator.ArrayCommand
            Broadcast(data_type_id, data_type_signature, transfer_id_aac, priority, payload, payload_len);
            transfer_id_aac = incrementTransferID(transfer_id_aac);
        }


        /********************************************************************************/
        /*  uavcan.equipment.actuator.ArrayCommnadタブ切替                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // タブ切替時に他ページの入力値を初期化
            if (tabControl2.SelectedIndex == 0) // UNITLESS
            {
                numericUpDown2.Value = 0; // POSITION 数値入力
                trackBar2.Value = 0;      // POSITION スライダー操作
                numericUpDown3.Value = 0; // FORCE 数値入力
                trackBar3.Value = 0;      // FORCE スライダー操作
                numericUpDown4.Value = 0; // SPEED 数値入力
                trackBar4.Value = 0;      // SPEED スライダー操作
            }
            else if (tabControl2.SelectedIndex == 1) // POSITION
            {
                numericUpDown1.Value = 0; // UNITLESS 数値入力
                trackBar1.Value = 0;      // UNITLESS スライダー操作
                numericUpDown3.Value = 0; // FORCE 数値入力
                trackBar3.Value = 0;      // FORCE スライダー操作
                numericUpDown4.Value = 0; // SPEED 数値入力
                trackBar4.Value = 0;      // SPEED スライダー操作
            }
            else if (tabControl2.SelectedIndex == 2) // FORCE
            {
                numericUpDown1.Value = 0; // UNITLESS 数値入力
                trackBar1.Value = 0;      // UNITLESS スライダー操作
                numericUpDown2.Value = 0; // POSITION 数値入力
                trackBar2.Value = 0;      // POSITION スライダー操作
                numericUpDown4.Value = 0; // SPEED 数値入力
                trackBar4.Value = 0;      // SPEED スライダー操作
            }
            else if (tabControl2.SelectedIndex == 3) // SPEED
            {
                numericUpDown1.Value = 0; // UNITLESS 数値入力
                trackBar1.Value = 0;      // UNITLESS スライダー操作
                numericUpDown2.Value = 0; // POSITION 数値入力
                trackBar2.Value = 0;      // POSITION スライダー操作
                numericUpDown3.Value = 0; // FORCE 数値入力
                trackBar3.Value = 0;      // FORCE スライダー操作
            }
        }


        /********************************************************************************/
        /*  スライダー角度指令(UNITLESS)                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                numericUpDown1.Value = (decimal)trackBar1.Value / 1000;
                float unitless_trackbar_val = (float)trackBar1.Value / 1000;
                makeEquipmentActuatorArrayCommand(0, unitless_trackbar_val);
            }
        }


        /********************************************************************************/
        /*  入力角度指令(UNITLESS)                                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                trackBar1.Value = (short)(numericUpDown1.Value * 1000);
                float unitless_updown_val = (float)numericUpDown1.Value;
                makeEquipmentActuatorArrayCommand(0, unitless_updown_val);
            }
        }


        /********************************************************************************/
        /*  スライダー角度指令(POSITION)                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                numericUpDown2.Value = trackBar2.Value;
                float position_val = (float)trackBar2.Value / 100;
                makeEquipmentActuatorArrayCommand(1, (float)position_val);
            }
        }

        /********************************************************************************/
        /*  入力角度指令(POSITION)                                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                trackBar2.Value = (short)numericUpDown2.Value;
                float position_val = (float)numericUpDown2.Value / 100;
                makeEquipmentActuatorArrayCommand(1, (float)position_val);
            }
        }


        /********************************************************************************/
        /*  スライダー角度指令(TORQUE)                                                  */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if ((-10 < trackBar3.Value) && (trackBar3.Value < 10))
                {
                    numericUpDown3.Value = 0;
                    float torque_val = 0;
                    makeEquipmentActuatorArrayCommand(2, (float)torque_val);
                }
                else
                {
                    numericUpDown3.Value = trackBar3.Value;
                    float torque_val = trackBar3.Value;
                    makeEquipmentActuatorArrayCommand(2, (float)torque_val);
                }
            }
        }

        /********************************************************************************/
        /*  入力角度指令(TORQUE)                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                trackBar3.Value = (short)numericUpDown3.Value;

                if (numericUpDown3.Value == 1)
                {
                    numericUpDown3.Value = 10;
                }
                else if (numericUpDown3.Value == -1)
                {
                    numericUpDown3.Value = -10;
                }
                else if ((-10 < numericUpDown3.Value) && (numericUpDown3.Value < 10))
                {
                    numericUpDown3.Value = 0;
                }

                float torque_val = (float)numericUpDown3.Value;
                makeEquipmentActuatorArrayCommand(2, (float)torque_val);
            }
        }


        /********************************************************************************/
        /*  スライダー角度指令(SPEED)                                                   */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if ((-10 < trackBar4.Value) && (trackBar4.Value < 10))
                {
                    numericUpDown4.Value = 0;
                    float speed_val = 0;
                    makeEquipmentActuatorArrayCommand(3, (float)speed_val);
                }
                else
                {
                    numericUpDown4.Value = trackBar4.Value;
                    float speed_val = trackBar4.Value;
                    makeEquipmentActuatorArrayCommand(3, (float)speed_val);
                }
            }
        }

        /********************************************************************************/
        /*  入力角度指令(SPEED)                                                         */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                trackBar4.Value = (short)numericUpDown4.Value;

                if (numericUpDown4.Value == 1)
                {
                    numericUpDown4.Value = 10;
                }
                else if (numericUpDown4.Value == -1)
                {
                    numericUpDown4.Value = -10;
                }
                else if ((-10 < numericUpDown4.Value) && (numericUpDown4.Value < 10))
                {
                    numericUpDown4.Value = 0;
                }

                trackBar4.Value = (short)numericUpDown4.Value;
                float speed_val = (float)numericUpDown4.Value;
                makeEquipmentActuatorArrayCommand(3, (float)speed_val);
            }
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.GetSet書込送信処理                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void makeProtocolParamGetSetWrite(string address, Int64 value)
        {
            byte tag = 0x01; // integer_value

            payload[0] = 0x00;
            payload[1] = tag;
            payload[2] = (byte)(value & 0xff);
            payload[3] = (byte)((value >> 8) & 0xff);
            payload[4] = (byte)((value >> 16) & 0xff);
            payload[5] = (byte)((value >> 24) & 0xff);
            payload[6] = (byte)((value >> 32) & 0xff);
            payload[7] = (byte)((value >> 40) & 0xff);
            payload[8] = (byte)((value >> 48) & 0xff);
            payload[9] = (byte)((value >> 56) & 0xff); // value
            payload[10] = System.Text.Encoding.ASCII.GetBytes(address.Substring(0, 1))[0]; // 0
            payload[11] = System.Text.Encoding.ASCII.GetBytes(address.Substring(1, 1))[0]; // x
            payload[12] = System.Text.Encoding.ASCII.GetBytes(address.Substring(2, 1))[0]; // *
            payload[13] = System.Text.Encoding.ASCII.GetBytes(address.Substring(3, 1))[0]; // *

            priority = 30; // ノード優先度(中[16])
            payload_len = 14;

            data_type_id = 11;
            data_type_signature = 0xa7b622f939d1a4d5; // uavcan.protocol.param.GetSet
            Request(ServoNodeID, data_type_signature, data_type_id, transfer_id_pgs, priority, payload, payload_len);
            transfer_id_pgs = incrementTransferID(transfer_id_pgs);
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.GetSet読出送信処理                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void makeProtocolParamGetSetRead(string address)
        {
            byte tag = 0x00; // empty

            payload[0] = 0x00;
            payload[1] = tag;
            payload[2] = System.Text.Encoding.ASCII.GetBytes(address.Substring(0, 1))[0]; // 0
            payload[3] = System.Text.Encoding.ASCII.GetBytes(address.Substring(1, 1))[0]; // x
            payload[4] = System.Text.Encoding.ASCII.GetBytes(address.Substring(2, 1))[0]; // *
            payload[5] = System.Text.Encoding.ASCII.GetBytes(address.Substring(3, 1))[0]; // *

            priority = 30; // ノード優先度(中[16])
            payload_len = 6;

            data_type_id = 11;
            data_type_signature = 0xa7b622f939d1a4d5; // uavcan.protocol.param.GetSet
            Request(ServoNodeID, data_type_signature, data_type_id, transfer_id_pgs, priority, payload, payload_len);
            transfer_id_pgs = incrementTransferID(transfer_id_pgs);
        }


        /********************************************************************************/
        /*  uavcan.protocol.param.GetSetタブ切替                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            parameter_tab = tabControl1.SelectedIndex;
        }


        /********************************************************************************/
        /*  Errorタブ切替                                                               */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            parameter_tab3 = tabControl3.SelectedIndex;
        }


        /********************************************************************************/
        /*  Target Angle [CCW]ボタン                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button6_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown5.Value * 10);
                makeProtocolParamGetSetWrite("0x00", val);
            }
        }


        /********************************************************************************/
        /*  Target Angle [Center]ボタン                                                 */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button7_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown6.Value * 10);
                makeProtocolParamGetSetWrite("0x00", val);
            }
        }


        /********************************************************************************/
        /*  Target Angle [CW]ボタン                                                     */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button8_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown7.Value * 10);
                makeProtocolParamGetSetWrite("0x00", val);
            }
        }


        /********************************************************************************/
        /*  Target Torque [CCW]ボタン                                                   */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button9_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown8.Value;
                makeProtocolParamGetSetWrite("0x02", val);
            }
        }


        /********************************************************************************/
        /*  Target Torque [Center]ボタン                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button10_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown9.Value;
                makeProtocolParamGetSetWrite("0x02", val);
            }
        }


        /********************************************************************************/
        /*  Target Torque [CW]ボタン                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button11_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown10.Value;
                makeProtocolParamGetSetWrite("0x02", val);
            }
        }


        /********************************************************************************/
        /*  Target Speed [CCW]ボタン                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button12_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown11.Value;
                makeProtocolParamGetSetWrite("0x01", val);
            }
        }


        /********************************************************************************/
        /*  Target Speed [Center]ボタン                                                 */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button13_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown12.Value;
                makeProtocolParamGetSetWrite("0x01", val);
            }
        }


        /********************************************************************************/
        /*  Target Speed [CW]ボタン                                                     */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button14_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown13.Value;
                makeProtocolParamGetSetWrite("0x01", val);
            }
        }


        /********************************************************************************/
        /*  Initializeボタン                                                            */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button15_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                makeProtocolParamGetSetWrite("0x10", 1);

                Command_counter = 0; // Command取得カウンタ停止
                button6.Enabled = false; // Target Angleボタン無効
                button7.Enabled = false; // Target Angleボタン無効
                button8.Enabled = false; // Target Angleボタン無効
                button9.Enabled = false; // Target Speedボタン無効
                button10.Enabled = false; // Target Speedボタン無効
                button11.Enabled = false; // Target Speedボタン無効
                button12.Enabled = false; // Target Torqueボタン無効
                button13.Enabled = false; // Target Torqueボタン無効
                button14.Enabled = false; // Target Torqueボタン無効

                button15.Enabled = false; // Initializeボタン無効
                button16.Enabled = false; // Rebootボタン無効
                button17.Enabled = false; // WriteROMボタン無効

                Configuration_counter = 0; // Configuration取得カウンタ停止
                button18.Enabled = false; // No command Timeボタン無効
                button42.Enabled = false; // OC Protection ボタン無効

                Control_counter = 0; // Control取得カウンタ停止
                button21.Enabled = false; // Angle Prop Gainボタン無効
                button22.Enabled = false; // Angle Diff Gainボタン無効
                button23.Enabled = false; // Angle Dead bandボタン無効
                button24.Enabled = false; // Speed Prop Gainボタン無効
                button25.Enabled = false; // Speed Intg Gainボタン無効
                button26.Enabled = false; // Speed Dead bandボタン無効
                button27.Enabled = false; // Speed Intg Limitボタン無効
                button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
                button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
                button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効
                button46.Enabled = false; // Angle_Boostボタン無効

                Limit_counter = 0; // Limit取得カウンタ停止
                button28.Enabled = false; // Limit Angle CWボタン無効
                button29.Enabled = false; // Limit Angle CCWボタン無効
                button30.Enabled = false; // Limit Speed CWボタン無効
                button31.Enabled = false; // Limit Speed CCWボタン無効
                button32.Enabled = false; // Limit Torque CWボタン無効
                button33.Enabled = false; // Limit Torque CCWボタン無効
                button34.Enabled = false; // Limit Temperature Highボタン無効
                button35.Enabled = false; // Limit Temperature Lowボタン無効
                button36.Enabled = false; // Limit Voltage Highボタン無効
                button37.Enabled = false; // Limit Voltage Lowボタン無効

                Option_counter = 0; // Option取得カウンタ停止
                button38.Enabled = false; // Origin Positionボタン無効
                button39.Enabled = false; // Actuator IDボタン無効
                button40.Enabled = false; // DroneCAN Node IDボタン無効

                Manufacture_counter = 0; // Manufacture取得カウンタ停止
            }
        }


        /********************************************************************************/
        /*  Rebootボタン                                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button16_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                makeProtocolParamGetSetWrite("0x11", 1);

                Command_counter = 0; // Command取得カウンタ停止
                button6.Enabled = false; // Target Angleボタン無効
                button7.Enabled = false; // Target Angleボタン無効
                button8.Enabled = false; // Target Angleボタン無効
                button9.Enabled = false; // Target Speedボタン無効
                button10.Enabled = false; // Target Speedボタン無効
                button11.Enabled = false; // Target Speedボタン無効
                button12.Enabled = false; // Target Torqueボタン無効
                button13.Enabled = false; // Target Torqueボタン無効
                button14.Enabled = false; // Target Torqueボタン無効

                button15.Enabled = false; // Initializeボタン無効
                button16.Enabled = false; // Rebootボタン無効
                button17.Enabled = false; // WriteROMボタン無効

                Configuration_counter = 0; // Configuration取得カウンタ停止
                button18.Enabled = false; // No command Timeボタン無効
                button42.Enabled = false; // OC Protection ボタン無効

                Control_counter = 0; // Control取得カウンタ停止
                button21.Enabled = false; // Angle Prop Gainボタン無効
                button22.Enabled = false; // Angle Diff Gainボタン無効
                button23.Enabled = false; // Angle Dead bandボタン無効
                button24.Enabled = false; // Speed Prop Gainボタン無効
                button25.Enabled = false; // Speed Intg Gainボタン無効
                button26.Enabled = false; // Speed Dead bandボタン無効
                button27.Enabled = false; // Speed Intg Limitボタン無効
                button43.Enabled = false; // PWMIN_PulseWidth_Neutralボタン無効
                button44.Enabled = false; // PWMIN_PulseWidth_Rangeボタン無効
                button45.Enabled = false; // PWMIN_PulseWidth_Targetボタン無効
                button46.Enabled = false; // Angle_Boostボタン無効

                Limit_counter = 0; // Limit取得カウンタ停止
                button28.Enabled = false; // Limit Angle CWボタン無効
                button29.Enabled = false; // Limit Angle CCWボタン無効
                button30.Enabled = false; // Limit Speed CWボタン無効
                button31.Enabled = false; // Limit Speed CCWボタン無効
                button32.Enabled = false; // Limit Torque CWボタン無効
                button33.Enabled = false; // Limit Torque CCWボタン無効
                button34.Enabled = false; // Limit Temperature Highボタン無効
                button35.Enabled = false; // Limit Temperature Lowボタン無効
                button36.Enabled = false; // Limit Voltage Highボタン無効
                button37.Enabled = false; // Limit Voltage Lowボタン無効

                Option_counter = 0; // Option取得カウンタ停止
                button38.Enabled = false; // Origin Positionボタン無効
                button39.Enabled = false; // Actuator IDボタン無効
                button40.Enabled = false; // DroneCAN Node IDボタン無効

                Manufacture_counter = 0; // Manufacture取得カウンタ停止
            }
        }


        /********************************************************************************/
        /*  WriteROMボタン                                                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button17_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                makeProtocolParamGetSetWrite("0x12", 1);
            }
        }


        /********************************************************************************/
        /*  Enable Torque選択                                                           */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (comboBox2.SelectedIndex == 0) // Torque ON
                {
                    makeProtocolParamGetSetWrite("0x20", 0);
                }
                else if (comboBox2.SelectedIndex == 1) // Torque OFF
                {
                    makeProtocolParamGetSetWrite("0x20", 1);
                }
                else if (comboBox2.SelectedIndex == 2) // Brake
                {
                    makeProtocolParamGetSetWrite("0x20", 2);
                }
            }
        }


        /********************************************************************************/
        /*  Enable Soft Start選択                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (checkBox2.Checked == true)
                {
                    makeProtocolParamGetSetWrite("0x21", 1); // ON
                }
                else
                {
                    makeProtocolParamGetSetWrite("0x21", 0); // OFF
                }
            }
        }


        /********************************************************************************/
        /*  Enable Smoothing選択                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (checkBox3.Checked == true)
                {
                    makeProtocolParamGetSetWrite("0x22", 1); // ON
                }
                else
                {
                    makeProtocolParamGetSetWrite("0x22", 0); // OFF
                }
            }
        }


        /********************************************************************************/
        /*  Enable Reverse選択                                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (checkBox4.Checked == true)
                {
                    makeProtocolParamGetSetWrite("0x23", 1); // ON
                }
                else
                {
                    makeProtocolParamGetSetWrite("0x23", 0); // OFF
                }
            }
        }


        /********************************************************************************/
        /*  Enable MultiTurn選択                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (checkBox5.Checked == true)
                {
                    makeProtocolParamGetSetWrite("0x24", 1); // ON
                }
                else
                {
                    makeProtocolParamGetSetWrite("0x24", 0); // OFF
                }
            }
        }


        /********************************************************************************/
        /*  Enable Speed/Torque Control選択                                                  */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (checkBox6.Checked == true)
                {
                    makeProtocolParamGetSetWrite("0x25", 1); // ON
                }
                else
                {
                    makeProtocolParamGetSetWrite("0x25", 0); // OFF
                }
            }
        }

        /********************************************************************************/
        /*  No command Operation選択                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                if (comboBox3.SelectedIndex == 0) // Hold
                {
                    makeProtocolParamGetSetWrite("0x26", 0);
                }
                else if (comboBox3.SelectedIndex == 1) // Free
                {
                    makeProtocolParamGetSetWrite("0x26", 1);
                }
                else if (comboBox3.SelectedIndex == 2) // Brake
                {
                    makeProtocolParamGetSetWrite("0x26", 2);
                }
            }
        }


        /********************************************************************************/
        /*  No command Timeボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button18_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown14.Value;
                makeProtocolParamGetSetWrite("0x27", val);
            }
        }

        /********************************************************************************/
        /*  OC Protection ボタン                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button42_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown37.Value * 10);
                makeProtocolParamGetSetWrite("0x2A", val);
                Configuration_button_clicked = 10;
            }
        }

        /********************************************************************************/
        /*  Angle Prop Gainボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button21_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown17.Value;
                makeProtocolParamGetSetWrite("0x2C", val);
            }
        }


        /********************************************************************************/
        /*  Angle Diff Gainボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button22_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown18.Value;
                makeProtocolParamGetSetWrite("0x2D", val);
            }
        }


        /********************************************************************************/
        /*  Angle Dead bandボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button23_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown19.Value;
                makeProtocolParamGetSetWrite("0x2E", val);
            }
        }


        /********************************************************************************/
        /*  Angle Boostボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button46_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown38.Value * 10);
                makeProtocolParamGetSetWrite("0x2F", val);
                Control_button_clicked = 13;
            }
        }

        /********************************************************************************/
        /*  Speed Prop Gainボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button24_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown20.Value;
                makeProtocolParamGetSetWrite("0x30", val);
            }
        }


        /********************************************************************************/
        /*  Speed Intg Gainボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button25_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown21.Value;
                makeProtocolParamGetSetWrite("0x31", val);
            }
        }


        /********************************************************************************/
        /*  Speed Dead bandボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button26_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown22.Value;
                makeProtocolParamGetSetWrite("0x32", val);
            }
        }


        /********************************************************************************/
        /*  Speed Intg Limitボタン                                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button27_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown23.Value;
                makeProtocolParamGetSetWrite("0x33", val);
            }
        }


        /********************************************************************************/
        /*  PWMIN_PulseWidth_Neutralボタン                                              */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button43_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown39.Value;
                makeProtocolParamGetSetWrite("0x34", val);
                Control_button_clicked = 8;
            }
        }


        /********************************************************************************/
        /*  PWMIN_PulseWidth_Rangeボタン                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button44_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown40.Value;
                makeProtocolParamGetSetWrite("0x35", val);
                Control_button_clicked = 9;
            }
        }


        /********************************************************************************/
        /*  PWMIN_PulseWidth_Targetボタン                                               */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button45_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = 0;

                if (comboBox4.SelectedIndex == 0)
                {
                    val = (long)(float)(numericUpDown41.Value * 10);
                }
                else if (comboBox4.SelectedIndex == 1)
                {
                    val = (long)numericUpDown41.Value;
                }
                makeProtocolParamGetSetWrite("0x36", val);
            }
        }




        /********************************************************************************/
        /*  PWMIN_Target_Mode選択　　　　　                                             */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal val = numericUpDown41.Value;

            if (servo_flag == true)
            {
                if ((comboBox4.SelectedIndex == 0) && (comboBox4_Selected_Now != comboBox4.SelectedIndex))// Angle
                {
                    makeProtocolParamGetSetWrite("0x37", 0);
                    numericUpDown41.Value = Decimal.Divide(val, 10);
                    numericUpDown41.DecimalPlaces = 1;
                    numericUpDown41.Increment = 0.1m;
                    numericUpDown41.Maximum = 360;
                    label94.Text = "deg";
                }
                else if ((comboBox4.SelectedIndex == 1) && (comboBox4_Selected_Now != comboBox4.SelectedIndex))// Speed
                {
                    makeProtocolParamGetSetWrite("0x37", 1);
                    numericUpDown41.Maximum = 3600;
                    numericUpDown41.Value = (decimal)(float)(val * 10);
                    numericUpDown41.DecimalPlaces = 0;
                    numericUpDown41.Increment = 1;
                    label94.Text = "rpm";
                }
            }
            else
            {
                if ((comboBox4.SelectedIndex == 0) && (comboBox4_Selected_Now != comboBox4.SelectedIndex))// Angle
                {
                    numericUpDown41.Value = Decimal.Divide(val, 10);
                    numericUpDown41.DecimalPlaces = 1;
                    numericUpDown41.Increment = 0.1m;
                    numericUpDown41.Maximum = 360;
                    label94.Text = "deg";
                }
                else if ((comboBox4.SelectedIndex == 1) && (comboBox4_Selected_Now != comboBox4.SelectedIndex))// Speed
                {
                    numericUpDown41.Maximum = 3600;
                    numericUpDown41.Value = (decimal)(float)(val * 10);
                    numericUpDown41.DecimalPlaces = 0;
                    numericUpDown41.Increment = 1;
                    label94.Text = "rpm";
                }
            }

            comboBox4_Selected_Now = comboBox4.SelectedIndex;
        }


        /********************************************************************************/
        /*  Limit Angle CWボタン                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button28_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown24.Value * 10);
                makeProtocolParamGetSetWrite("0x38", val);
            }
        }


        /********************************************************************************/
        /*  Limit Angle CCWボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button29_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown25.Value * 10);
                makeProtocolParamGetSetWrite("0x39", val);
            }
        }


        /********************************************************************************/
        /*  Limit Speed CWボタン                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button30_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown26.Value;
                makeProtocolParamGetSetWrite("0x3A", val);
                Limit_button_clicked = 3;
            }
        }


        /********************************************************************************/
        /*  Limit Speed CCWボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button31_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown27.Value;
                makeProtocolParamGetSetWrite("0x3B", val);
                Limit_button_clicked = 4;
            }
        }


        /********************************************************************************/
        /*  Limit Torque CWボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button32_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown28.Value;
                makeProtocolParamGetSetWrite("0x3C", val);
            }
        }


        /********************************************************************************/
        /*  Limit Torque CCWボタン                                                      */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button33_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown29.Value;
                makeProtocolParamGetSetWrite("0x3D", val);
            }
        }


        /********************************************************************************/
        /*  Limit Temperature Highボタン                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button34_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown30.Value;
                makeProtocolParamGetSetWrite("0x3E", val);
            }
        }


        /********************************************************************************/
        /*  Limit Temperature Lowボタン                                                 */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button35_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown31.Value;
                makeProtocolParamGetSetWrite("0x3F", val);
            }
        }


        /********************************************************************************/
        /*  Limit Voltage Highボタン                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button36_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown32.Value * 10);
                makeProtocolParamGetSetWrite("0x40", val);
                Limit_button_clicked = 9;
            }
        }


        /********************************************************************************/
        /*  Limit Voltage Lowボタン                                                     */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button37_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown33.Value * 10);
                makeProtocolParamGetSetWrite("0x41", val);
                Limit_button_clicked = 10;
            }
        }


        /********************************************************************************/
        /*  Origin Positionボタン                                                       */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button38_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)(float)(numericUpDown34.Value * 10);
                makeProtocolParamGetSetWrite("0x44", val);
            }
        }


        /********************************************************************************/
        /*  Actuator IDボタン                                                           */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button39_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown35.Value;
                makeProtocolParamGetSetWrite("0x45", val);
            }
        }


        /********************************************************************************/
        /*  DroneCAN Node IDボタン                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void button40_Click(object sender, EventArgs e)
        {
            if (servo_flag == true)
            {
                long val = (long)numericUpDown36.Value;
                makeProtocolParamGetSetWrite("0x46", val);
            }
        }


        /********************************************************************************/
        /*  ブロードキャスト処理                                                        */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void Broadcast(ushort data_type_id, ulong data_type_signature, byte transfer_id, byte priority, byte[] payload, ushort payload_len)
        {
            crc = 0xFFFF;
            // ローカルノードID：0の処理は削除
            uint tx_id = ((uint)priority << 24) | ((uint)data_type_id << 8) | local_node_id;
            if (payload_len > 7)
            {
                crc = crcAddSignature(crc, data_type_signature);
                crc = crcAdd(crc, payload, payload_len);
            }
            enqueueTxFrames(tx_id, transfer_id, crc, payload, payload_len);
        }


        /********************************************************************************/
        /*  要求処理                                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void Request(byte destination_node_id, ulong data_type_signature, ushort data_type_id, byte transfer_id, byte priority, byte[] payload, ushort payload_len)
        {
            byte kind = 1;
            crc = 0xFFFF;
            // ローカルノードID：0の処理は削除
            uint tx_id = ((uint)priority << 24) | ((uint)data_type_id << 16) | ((uint)kind << 15) | ((uint)destination_node_id << 8) | (1 << 7) | local_node_id;
            if (payload_len > 7)
            {
                crc = crcAddSignature(crc, data_type_signature);
                crc = crcAdd(crc, payload, payload_len);
            }
            enqueueTxFrames(tx_id, transfer_id, crc, payload, payload_len);
        }


        /********************************************************************************/
        /*  応答処理                                                                    */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void Respond(byte destination_node_id, ulong data_type_signature, ushort data_type_id, byte transfer_id, byte priority, byte[] payload, ushort payload_len)
        {
            byte kind = 0;
            crc = 0xFFFF;
            // ローカルノードID：0の処理は削除
            uint tx_id = ((uint)priority << 24) | ((uint)data_type_id << 16) | ((uint)kind << 15) | ((uint)destination_node_id << 8) | (1 << 7) | local_node_id;
            if (payload_len > 7)
            {
                crc = crcAddSignature(crc, data_type_signature);
                crc = crcAdd(crc, payload, payload_len);
            }
            enqueueTxFrames(tx_id, transfer_id, crc, payload, payload_len);
        }


        /********************************************************************************/
        /*  UAVCANデータ変換(ushort → float）                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        float ConvertFloat16ToNativeFloat(ushort value)
        {
            FP32 magic = new FP32() { u = (254 - 15) << 23 };
            FP32 was_inf_nan = new FP32() { u = (127 + 16) << 23 };
            FP32 out__ = new FP32();

            out__.u = (uint)((value & 0x7FFF) << 13);
            out__.f *= magic.f;

            if (out__.f >= was_inf_nan.f)
            {
                out__.u |= 255 << 23;
            }
            out__.u |= (uint)((value & 0x8000) << 16);

            return out__.f;
        }


        /********************************************************************************/
        /*  UAVCANデータ変換(float → ushort）                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        ushort ConvertNativeFloatToFloat16(float value)
        {
            FP32 f32inf = new FP32 { u = 255 << 23 };
            FP32 f16inf = new FP32 { u = 31 << 23 };
            FP32 magic = new FP32 { u = 15 << 23 };
            uint sign_mask = 0x80000000;
            uint round_mask = ~0xFFFU;

            FP32 in_ = new FP32();
            in_.f = value;
            uint sign = in_.u & sign_mask;
            in_.u ^= sign;

            ushort out___ = 0;

            if (in_.u >= f32inf.u)
            {
                out___ = (in_.u > f32inf.u) ? (ushort)0x7FFF : (ushort)0x7C00;
            }
            else
            {
                in_.u &= round_mask;
                in_.f *= magic.f;
                in_.u -= round_mask;
                if (in_.u > f16inf.u)
                {
                    in_.u = f16inf.u;
                }
                out___ = (ushort)(in_.u >> 13);
            }
            out___ |= (ushort)(sign >> 16);

            return out___;
        }


        /********************************************************************************/
        /*  要求/応答処理                                                               */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void RequestOrRespond(ushort data_type_id, ulong data_type_signature, byte transfer_id, byte priority, byte[] payload, ushort payload_len)
        {
            crc = 0xFFFF;
            uint tx_id = ((uint)priority << 24) | ((uint)data_type_id << 8) | local_node_id;
            if (payload_len > 7)
            {
                crc = crcAddSignature(crc, data_type_signature);
                crc = crcAdd(crc, payload, payload_len);
            }
            enqueueTxFrames(tx_id, transfer_id, crc, payload, payload_len);
        }


        /********************************************************************************/
        /*  送信フレーム生成                                                            */
        /*      [注記]                                                                  */
        /********************************************************************************/
        void enqueueTxFrames(uint tx_id, byte transfer_id, ushort crc, byte[] payload, ushort payload_len)
        {
            if (payload_len < 8)
            {
                for (uint m = 0; m < payload_len; m++)
                {
                    tx_data[m] = payload[m];
                }
                tx_data_len = (ushort)(payload_len + 1);
                tx_data[payload_len] = (byte)(0xC0 | (transfer_id & 31));
                reportFrame(tx_id, tx_data_len, tx_data);
            }
            else
            {
                ushort data_index = 0;
                byte toggle = 0;
                byte sot_eot = 0x80;

                while (payload_len - data_index != 0)
                {
                    byte n = 0;
                    if (data_index == 0)
                    {
                        // add crc
                        tx_data[0] = (byte)(crc);
                        tx_data[1] = (byte)(crc >> 8);
                        n = 2;
                    }
                    else
                    {
                        n = 0;
                    }
                    for (; n < (8 - 1) && data_index < payload_len; n++, data_index++)
                    {
                        tx_data[n] = payload[data_index];
                    }

                    // tail byte
                    sot_eot = (data_index == payload_len) ? (byte)0x40 : sot_eot;

                    tx_data[n] = (byte)(sot_eot | (toggle << 5) | (transfer_id & 31));
                    tx_data_len = (byte)(n + 1);

                    reportFrame(tx_id, tx_data_len, tx_data);
                    System.Threading.Thread.Sleep(3);

                    toggle ^= 1;
                    sot_eot = 0;
                }
            }
        }


        /********************************************************************************/
        /*  transfer_id関数                                                             */
        /*      [注記]                                                                  */
        /********************************************************************************/
        byte incrementTransferID(byte transfer_id)
        {
            transfer_id++;
            if (transfer_id >= 32)
            {
                transfer_id = 0;
            }
            return transfer_id;
        }


        /********************************************************************************/
        /*  CRC関数①                                                                   */
        /*      [注記]                                                                  */
        /********************************************************************************/
        ushort crcAddByte(ushort crc_val, byte byte_)
        {
            crc_val ^= (ushort)(byte_ << 8);
            for (byte l = 0; l < 8; l++)
            {
                if ((crc_val & 0x8000) == 0x8000)
                {
                    crc_val = (ushort)((ushort)(crc_val << 1) ^ 0x1021);
                }
                else
                {
                    crc_val = (ushort)(crc_val << 1);
                }
            }
            return crc_val;
        }


        /********************************************************************************/
        /*  CRC関数②                                                                   */
        /*      [注記]                                                                  */
        /********************************************************************************/
        ushort crcAddSignature(ushort crc_val, ulong data_type_signature)
        {
            for (ushort shift_val = 0; shift_val < 64; shift_val = (ushort)(shift_val + 8))
            {
                crc_val = crcAddByte(crc_val, (byte)(data_type_signature >> shift_val));
            }
            return crc_val;
        }


        /********************************************************************************/
        /*  CRC関数③                                                                   */
        /*      [注記]                                                                  */
        /********************************************************************************/
        ushort crcAdd(ushort crc_val, byte[] bytes, ushort len)
        {
            ushort k = 0;
            while (len > 0)
            {
                crc_val = crcAddByte(crc_val, bytes[k++]);
                len--;
            }
            return crc_val;
        }


        /********************************************************************************/
        /*  SLCAN送信                                                                   */
        /*      [注記]      <type> <id> <dlc> <data>                                    */
        /********************************************************************************/
        void reportFrame(uint tx_id, ushort tx_data_len, byte[] tx_data)
        {
            byte[] buffer = new byte[40];
            byte q = 0;

            // Frame type
            buffer[q++] = (byte)'T';

            // ID
            buffer[q++] = nibble2hex((byte)((tx_id >> 28) & 0x01));
            buffer[q++] = nibble2hex((byte)((tx_id >> 24) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 20) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 16) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 12) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 8) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 4) & 0xFF));
            buffer[q++] = nibble2hex((byte)((tx_id >> 0) & 0xFF));

            // dlc
            buffer[q++] = nibble2hex((byte)(tx_data_len));

            // Data
            for (byte i = 0; i < tx_data_len; i++)
            {
                buffer[q++] = nibble2hex((byte)((tx_data[i] >> 4) & 0xF));
                buffer[q++] = nibble2hex((byte)((tx_data[i]) & 0xF));
            }

            // Finalization
            buffer[q++] = (byte)'\r';

            // Tx
            serialPort1.Write(buffer, 0, q);
        }


        /********************************************************************************/
        /*  ASCII→Hex変換                                                              */
        /*      [注記]      4bitずつ                                                    */
        /********************************************************************************/
        byte hex2nibble(byte c)
        {
            byte[] NumConversionTable = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            byte[] AlphaConversionTable = { 10, 11, 12, 13, 14, 15 };

            byte out_ = 255;

            if (c >= '0' && c <= '9')
            {
                out_ = NumConversionTable[c - '0'];
            }
            else if (c >= 'a' && c <= 'f')
            {
                out_ = AlphaConversionTable[c - 'a'];
            }
            else if (c >= 'A' && c <= 'F')
            {
                out_ = AlphaConversionTable[c - 'A'];
            }

            if (out_ == 255)
            {
                //hex2nibble_error = true;
            }
            return out_;
        }


        /********************************************************************************/
        /*  Hex→ASCII変換                                                              */
        /*      [注記]      4bitずつ                                                    */
        /********************************************************************************/
        byte nibble2hex(byte x)
        {
            byte[] ConversionTable = { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 };
            return ConversionTable[x & 0x0F];
        }


        /********************************************************************************/
        /*  100ms汎用タイマ処理                                                         */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void timer2_Tick(object sender, EventArgs e)
        {
            float present_position2, present_voltage2;
            short present_speed2, present_torque2, present_temperature2;
            if (update_flag == false)
            {
                if ((position_deg2 <= 180) && (-180 <= position_deg2)) // Positionタコメータ
                    aGauge1.Value = position_deg2;
                else if ((position_deg2 > 0) && (position_deg2 <= 2147483647))  // マルチターン(プラス)
                {
                    if ((position_deg2 % 360) <= 180)
                        aGauge1.Value = position_deg2 % 360;
                    else
                        aGauge1.Value = (position_deg2 % 360) - 360;
                }
                else if (position_deg2 >= -2147483648) // マルチターン(マイナス)
                {
                    if ((-position_deg2 % 360) <= 180)
                        aGauge1.Value = -(-position_deg2 % 360);
                    else
                        aGauge1.Value = 360 - (-position_deg2 % 360);
                }
                textBox7.Text = position_deg3.ToString("F1");

                if ((servo_flag == true) && (parameter_tab == 0)) // Command
                {
                    if (Command_counter == 1)
                    {
                        Command_counter = 2;
                        button6.Enabled = true; // Target Angleボタン有効
                        button7.Enabled = true; // Target Angleボタン有効
                        button8.Enabled = true; // Target Angleボタン有効
                        button9.Enabled = true; // Target Speedボタン有効
                        button10.Enabled = true; // Target Speedボタン有効
                        button11.Enabled = true; // Target Speedボタン有効
                        button12.Enabled = true; // Target Torqueボタン有効
                        button13.Enabled = true; // Target Torqueボタン有効
                        button14.Enabled = true; // Target Torqueボタン有効
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 1)) // Status
                {
                    present_position2 = position_deg3;
                    textBox8.Text = present_position2.ToString("F1");
                    present_speed2 = (short)present_speed;
                    textBox9.Text = present_speed2.ToString();
                    present_torque2 = (short)present_torque;
                    textBox10.Text = present_torque2.ToString();
                    present_temperature2 = (short)present_temperature;
                    textBox11.Text = present_temperature2.ToString();
                    present_voltage2 = (float)present_voltage / 10;
                    textBox12.Text = present_voltage2.ToString("F1");

                    parameter_read_counter++;
                    if (parameter_read_counter == 1)
                    {
                        makeProtocolParamGetSetRead("0x08"); // 現在角度
                    }
                    else if (parameter_read_counter == 2)
                    {
                        makeProtocolParamGetSetRead("0x09"); // 現在速度
                    }
                    else if (parameter_read_counter == 3)
                    {
                        makeProtocolParamGetSetRead("0x0A"); // 現在トルク
                    }
                    else if (parameter_read_counter == 4)
                    {
                        makeProtocolParamGetSetRead("0x0B"); // 現在温度
                    }
                    else if (parameter_read_counter >= 5)
                    {
                        makeProtocolParamGetSetRead("0x0C"); // 現在電圧
                        parameter_read_counter = 0;
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 2)) // Operation
                {
                    tabPage3.Cursor = Cursors.Default;
                    button15.Enabled = true; // Initializeボタン有効
                    button16.Enabled = true; // Rebootボタン有効
                    button17.Enabled = true; // WriteROMボタン有効
                }
                else if ((servo_flag == true) && (parameter_tab == 3)) // Error
                {
                    if (parameter_tab3 == 0)
                    {
                        if ((Info_Total & 0x00000001) != 0)
                            lbLed3.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Hardware Noticeランプ点灯
                        else
                            lbLed3.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Hardware Noticeランプ消灯

                        if ((Info_Total & 0x00000002) != 0)
                            lbLed4.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Hardware Warningランプ点灯
                        else
                            lbLed4.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Hardware Warningランプ消灯

                        if ((Info_Total & 0x00000004) != 0)
                            lbLed5.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Hardware Faultランプ点灯
                        else
                            lbLed5.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Hardware Faultランプ消灯

                        if ((Info_Total & 0x00000100) != 0)
                            lbLed6.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Software Noticeランプ点灯
                        else
                            lbLed6.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Software Noticeランプ消灯

                        if ((Info_Total & 0x00000200) != 0)
                            lbLed7.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Software Warningランプ点灯
                        else
                            lbLed7.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Software Warningランプ消灯

                        if ((Info_Total & 0x00000400) != 0)
                            lbLed8.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Software Faultランプ点灯
                        else
                            lbLed8.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Software Faultランプ消灯

                        if ((Info_Total & 0x00010000) != 0)
                            lbLed9.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Communication Noticeランプ点灯
                        else
                            lbLed9.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Communication Noticeランプ消灯

                        if ((Info_Total & 0x00020000) != 0)
                            lbLed10.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Communication Warningランプ点灯
                        else
                            lbLed10.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Communication Warningランプ消灯

                        if ((Info_Total & 0x00040000) != 0)
                            lbLed11.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Communication Faultランプ点灯
                        else
                            lbLed11.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Communication Faultランプ消灯

                        if ((Info_Total & 0x01000000) != 0)
                            lbLed12.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Other Noticeランプ点灯
                        else
                            lbLed12.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Other Noticeランプ消灯

                        if ((Info_Total & 0x02000000) != 0)
                            lbLed13.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Other Warningランプ点灯
                        else
                            lbLed13.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Other Warningランプ消灯

                        if ((Info_Total & 0x04000000) != 0)
                            lbLed14.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Total Other Faultランプ点灯
                        else
                            lbLed14.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Total Other Faultランプ消灯

                        makeProtocolParamGetSetRead("0x18"); // Info Total
                    }
                    else if (parameter_tab3 == 1)
                    {
                        if ((Info_Notice & 0x00000100) != 0)
                            lbLed15.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Limit Angle CWランプ点灯
                        else
                            lbLed15.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Limit Angle CWランプ消灯

                        if ((Info_Notice & 0x00000200) != 0)
                            lbLed16.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Limit Angle CCWランプ点灯
                        else
                            lbLed16.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Limit Angle CCWランプ消灯

                        if ((Info_Notice & 0x00000400) != 0)
                            lbLed17.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Limit Speed CWランプ点灯
                        else
                            lbLed17.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Limit Speed CWランプ消灯

                        if ((Info_Notice & 0x00000800) != 0)
                            lbLed18.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Limit Speed CCWランプ点灯
                        else
                            lbLed18.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Limit Speed CCWランプ消灯

                        if ((Info_Notice & 0x00001000) != 0)
                            lbLed19.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Torque Angle CWランプ点灯
                        else
                            lbLed19.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Torque Angle CWランプ消灯

                        if ((Info_Notice & 0x00002000) != 0)
                            lbLed20.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Torque Angle CCWランプ点灯
                        else
                            lbLed20.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Torque Angle CCWランプ消灯

                        if ((Info_Notice & 0x00100000) != 0)
                            lbLed39.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Torque Angle CCWランプ点灯
                        else
                            lbLed39.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Torque Angle CCWランプ消灯

                        if ((Info_Notice & 0x00200000) != 0)
                            lbLed40.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Torque Angle CCWランプ点灯
                        else
                            lbLed40.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Torque Angle CCWランプ消灯

                        if ((Info_Notice & 0x00400000) != 0)
                            lbLed41.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Notice Software Torque Angle CCWランプ点灯
                        else
                            lbLed41.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Notice Software Torque Angle CCWランプ消灯

                        makeProtocolParamGetSetRead("0x19"); // Info Notice
                    }
                    else if (parameter_tab3 == 2)
                    {
                        if ((Info_Warning & 0x00000010) != 0)
                            lbLed21.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Hardware Angle Sencer Mag. too weakランプ点灯
                        else
                            lbLed21.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Hardware Angle Sencer Mag. too weakランプ消灯

                        if ((Info_Warning & 0x00000020) != 0)
                            lbLed22.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Hardware Angle Sencer Mag. too strongランプ点灯
                        else
                            lbLed22.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off;// Info_Warning Hardware Angle Sencer Mag. too strongランプ消灯

                        if ((Info_Warning & 0x00000040) != 0)
                            lbLed23.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Hardware memoryランプ点灯
                        else
                            lbLed23.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Hardware memoryランプ消灯

                        if ((Info_Warning & 0x00000100) != 0)
                            lbLed24.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Software Limit Temperature Highランプ点灯
                        else
                            lbLed24.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Software Limit Temperature Highランプ消灯

                        if ((Info_Warning & 0x00000200) != 0)
                            lbLed25.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Software Limit Temperature Lowランプ点灯
                        else
                            lbLed25.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Software Limit Temperature Lowランプ消灯

                        if ((Info_Warning & 0x00000400) != 0)
                            lbLed26.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Other Limit Voltage Highランプ点灯
                        else
                            lbLed26.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Other Limit Voltage Highランプ消灯

                        if ((Info_Warning & 0x00000800) != 0)
                            lbLed27.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Other Limit Voltage Lowランプ点灯
                        else
                            lbLed27.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Other Limit Voltage Lowランプ消灯

                        if ((Info_Warning & 0x00004000) != 0)
                            lbLed29.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Software CPU Clock errorランプ点灯
                        else
                            lbLed29.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Software CPU Clock errorランプ消灯

                        if ((Info_Warning & 0x00008000) != 0)
                            lbLed30.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Warning Software Save Data errorランプ点灯
                        else
                            lbLed30.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Warning Software Save Data errorランプ消灯

                        makeProtocolParamGetSetRead("0x1A"); // Info Warning
                    }
                    else if (parameter_tab3 == 3)
                    {
                        if ((Info_Fault & 0x00000001) != 0)
                            lbLed31.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Hardware Motor Driver Faultランプ点灯
                        else
                            lbLed31.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Hardware Motor Driver Faultランプ消灯

                        if ((Info_Fault & 0x00000002) != 0)
                            lbLed32.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green;// Info_Fault Hardware Hall Sensor Faultランプ点灯
                        else
                            lbLed32.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Hardware Hall Sensor Faultランプ消灯

                        if ((Info_Fault & 0x00000010) != 0)
                            lbLed33.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Hardware Angle Sencer Interface Faultランプ点灯
                        else
                            lbLed33.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Hardware Angle Sencer Interface Faultランプ消灯

                        if ((Info_Fault & 0x00000020) != 0)
                            lbLed34.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Hardware Angle Sencer Magnet undetectableランプ点灯
                        else
                            lbLed34.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Hardware Angle Sencer Magnet undetectableランプ消灯

                        if ((Info_Fault & 0x00000100) != 0)
                            lbLed35.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Software CPU Clock undetectableランプ点灯
                        else
                            lbLed35.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Software CPU Clock undetectableランプ消灯

                        if ((Info_Fault & 0x00000200) != 0)
                            lbLed36.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Software CPU Watch dog detectableランプ点灯
                        else
                            lbLed36.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Software CPU Watch dog detectableランプ消灯

                        if ((Info_Fault & 0x00000400) != 0)
                            lbLed37.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Software CPU ROM Faultランプ点灯
                        else
                            lbLed37.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Software CPU ROM Faultランプ消灯

                        if ((Info_Fault & 0x00000800) != 0)
                            lbLed38.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Green; // Info_Fault Software CPU RAM Faultランプ点灯
                        else
                            lbLed38.LedColor = MyCtrls.Leds.LBLed.LedColorEnum.Off; // Info_Fault Software CPU RAM Faultランプ消灯

                        makeProtocolParamGetSetRead("0x1B"); // Info Fault
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 4)) // Configuration
                {
                    if (Configuration_counter == 1)
                    {
                        tabPage5.Cursor = Cursors.WaitCursor;
                        makeProtocolParamGetSetRead("0x20"); // Enable Torque
                    }
                    else if (Configuration_counter == 2)
                    {
                        comboBox2.SelectedIndex = Enable_Torque;
                        makeProtocolParamGetSetRead("0x21"); // Enable Soft Start
                    }
                    else if (Configuration_counter == 3)
                    {
                        if (Enable_Soft_Start == 0) // Soft Start OFF
                            checkBox2.Checked = false;
                        else if (Enable_Soft_Start == 1) // Soft Start ON
                            checkBox2.Checked = true;
                        makeProtocolParamGetSetRead("0x22"); // Enable Smoothing
                    }
                    else if (Configuration_counter == 4)
                    {
                        if (Enable_Smoothing == 0) // Smoothing OFF
                            checkBox3.Checked = false;
                        else if (Enable_Smoothing == 1) // Smoothing ON
                            checkBox3.Checked = true;
                        makeProtocolParamGetSetRead("0x23"); // Enable Reverse
                    }
                    else if (Configuration_counter == 5)
                    {
                        if (Enable_Reverse == 0) // Reverse OFF
                            checkBox4.Checked = false;
                        else if (Enable_Reverse == 1) // Reverse ON
                            checkBox4.Checked = true;
                        makeProtocolParamGetSetRead("0x24"); // Enable MultiTurn
                    }
                    else if (Configuration_counter == 6)
                    {
                        if (Enable_MultiTurn == 0) // MultiTurn OFF
                            checkBox5.Checked = false;
                        else if (Enable_MultiTurn == 1) // MultiTurn ON
                            checkBox5.Checked = true;
                        makeProtocolParamGetSetRead("0x25"); // Enable Speed/Torque Control
                    }
                    else if (Configuration_counter == 7)
                    {
                        if (Enable_Speed_Torque_Control == 0) // MultiTurn OFF
                            checkBox6.Checked = false;
                        else if (Enable_Speed_Torque_Control == 1) // MultiTurn ON
                            checkBox6.Checked = true;
                        makeProtocolParamGetSetRead("0x26"); // No command Operation
                    }
                    else if (Configuration_counter == 8)
                    {
                        comboBox3.SelectedIndex = No_command_Operation;
                        makeProtocolParamGetSetRead("0x27"); // No command Time
                    }
                    else if (Configuration_counter == 9)
                    {
                        numericUpDown14.Value = No_command_Time;
                        makeProtocolParamGetSetRead("0x2A"); // OC Protection
                    }
                    else if (Configuration_counter == 10)
                    {
                        numericUpDown37.Value = Decimal.Divide(OC_Protection, 10);
                        Configuration_counter = 11;
                        tabPage5.Cursor = Cursors.Default;
                        button18.Enabled = true; // No command Timeボタン有効
                        button42.Enabled = true; // OC Protection ボタン有効
                    }
                    else if (Configuration_button_clicked == 10)
                    {
                        makeProtocolParamGetSetRead("0x2A");
                    }
                    else if (Configuration_button_clicked == 110)
                    {
                        numericUpDown37.Value = Decimal.Divide(OC_Protection, 10);
                        Configuration_button_clicked = 0;
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 5)) // Control
                {
                    if (Control_counter == 1)
                    {
                        tabPage6.Cursor = Cursors.WaitCursor;
                        makeProtocolParamGetSetRead("0x2C"); // Angle Prop Gain
                    }
                    else if (Control_counter == 2)
                    {
                        numericUpDown17.Value = Angle_Prop_Gain;
                        makeProtocolParamGetSetRead("0x2D"); // Angle Diff Gain
                    }
                    else if (Control_counter == 3)
                    {
                        numericUpDown18.Value = Angle_Diff_Gain;
                        makeProtocolParamGetSetRead("0x2E"); // Angle Dead band
                    }
                    else if (Control_counter == 4)
                    {
                        numericUpDown19.Value = Angle_Dead_band;
                        makeProtocolParamGetSetRead("0x30"); // Speed Prop Gain
                    }
                    else if (Control_counter == 5)
                    {
                        numericUpDown20.Value = Speed_Prop_Gain;
                        makeProtocolParamGetSetRead("0x31"); // Speed Intg Gain
                    }
                    else if (Control_counter == 6)
                    {
                        numericUpDown21.Value = Speed_Intg_Gain;
                        makeProtocolParamGetSetRead("0x32"); // Speed Dead band
                    }
                    else if (Control_counter == 7)
                    {
                        numericUpDown22.Value = Speed_Dead_band;
                        makeProtocolParamGetSetRead("0x33"); // Speed Intg Limit
                    }
                    else if (Control_counter == 8)
                    {
                        numericUpDown23.Value = Speed_Intg_Limit;
                        makeProtocolParamGetSetRead("0x34"); // PWMIN_PulseWidth_Neutral
                    }
                    else if (Control_counter == 9)
                    {
                        numericUpDown39.Value = PWMIN_PulseWidth_Neutral;
                        makeProtocolParamGetSetRead("0x35"); // PWMIN_PulseWidth_Range
                    }
                    else if (Control_counter == 10)
                    {
                        numericUpDown40.Value = PWMIN_PulseWidth_Range;
                        makeProtocolParamGetSetRead("0x36"); // PWMIN_PulseWidth_Target
                    }
                    else if (Control_counter == 11)
                    {
                        if (comboBox4.SelectedIndex == 0)
                        {
                            numericUpDown41.Maximum = 3600;
                            numericUpDown41.Value = Decimal.Divide(PWMIN_PulseWidth_Target, 10);
                        }
                        else if (comboBox4.SelectedIndex == 1)
                        {
                            numericUpDown41.Maximum = 3600;
                            numericUpDown41.Value = PWMIN_PulseWidth_Target;
                        }
                        makeProtocolParamGetSetRead("0x37"); // PWMIN_Target_Mode
                    }
                    else if (Control_counter == 12)
                    {
                        comboBox4.SelectedIndex = PWMIN_Target_Mode;
                        comboBox4_Selected_Now = comboBox4.SelectedIndex;
                        if (comboBox4.SelectedIndex == 0)
                        {
                            numericUpDown41.DecimalPlaces = 1;
                            numericUpDown41.Increment = 0.1m;
                            numericUpDown41.Maximum = 360;
                            label94.Text = "deg";
                        }
                        else if (comboBox4.SelectedIndex == 1)
                        {
                            numericUpDown41.Maximum = 3600;
                            numericUpDown41.DecimalPlaces = 0;
                            numericUpDown41.Increment = 1;
                            label94.Text = "rpm";
                        }
                        makeProtocolParamGetSetRead("0x2F"); // Boost
                    }
                    else if (Control_counter == 13)
                    {
                        numericUpDown38.Value = Decimal.Divide(Angle_Boost, 10);

                        Control_counter = 14;
                        tabPage6.Cursor = Cursors.Default;
                        button21.Enabled = true; // Angle Prop Gainボタン有効
                        button22.Enabled = true; // Angle Diff Gainボタン有効
                        button23.Enabled = true; // Angle Dead bandボタン有効
                        button24.Enabled = true; // Speed Prop Gainボタン有効
                        button25.Enabled = true; // Speed Intg Gainボタン有効
                        button26.Enabled = true; // Speed Dead bandボタン有効
                        button27.Enabled = true; // Speed Intg Limitボタン有効
                        button43.Enabled = true; // PWMIN_PulseWidth_Neutralボタン有効
                        button44.Enabled = true; // PWMIN_PulseWidth_Rangeボタン有効
                        button45.Enabled = true; // PWMIN_PulseWidth_Targetボタン有効
                        button46.Enabled = true; // Angle_Boostボタン有効
                    }
                    else if (Control_button_clicked == 8)
                    {
                        makeProtocolParamGetSetRead("0x34");
                    }
                    else if (Control_button_clicked == 9)
                    {
                        makeProtocolParamGetSetRead("0x35");
                    }
                    else if (Control_button_clicked == 13)
                    {
                        makeProtocolParamGetSetRead("0x2F");
                    }
                    else if (Control_button_clicked == 108)
                    {
                        numericUpDown39.Value = PWMIN_PulseWidth_Neutral;
                        Control_button_clicked = 0;
                    }
                    else if (Control_button_clicked == 109)
                    {
                        numericUpDown40.Value = PWMIN_PulseWidth_Range;
                        Control_button_clicked = 0;
                    }
                    else if (Control_button_clicked == 113)
                    {
                        numericUpDown38.Value = Decimal.Divide(Angle_Boost, 10);
                        Control_button_clicked = 0;
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 6)) // Limit
                {
                    if (Limit_counter == 1)
                    {
                        tabPage7.Cursor = Cursors.WaitCursor;
                        makeProtocolParamGetSetRead("0x38"); // Limit Angle CW
                    }
                    else if (Limit_counter == 2)
                    {
                        numericUpDown24.Value = (decimal)Limit_Angle_CW / 10;
                        makeProtocolParamGetSetRead("0x39"); // Limit Angle CCW
                    }
                    else if (Limit_counter == 3)
                    {
                        numericUpDown25.Value = (decimal)Limit_Angle_CCW / 10;
                        makeProtocolParamGetSetRead("0x3A"); // Limit Speed CW
                    }
                    else if (Limit_counter == 4)
                    {
                        numericUpDown26.Value = Limit_Speed_CW;
                        makeProtocolParamGetSetRead("0x3B"); // Limit Speed CCW
                    }
                    else if (Limit_counter == 5)
                    {
                        numericUpDown27.Value = Limit_Speed_CCW;
                        makeProtocolParamGetSetRead("0x3C"); // Limit Torque CW
                    }
                    else if (Limit_counter == 6)
                    {
                        numericUpDown28.Value = Limit_Torque_CW;
                        makeProtocolParamGetSetRead("0x3D"); // Limit Torque CCW
                    }
                    else if (Limit_counter == 7)
                    {
                        numericUpDown29.Value = Limit_Torque_CCW;
                        makeProtocolParamGetSetRead("0x3E"); // Limit Temperature High
                    }
                    else if (Limit_counter == 8)
                    {
                        numericUpDown30.Value = Limit_Temperature_High;
                        makeProtocolParamGetSetRead("0x3F"); // Limit Temperature Low
                    }
                    else if (Limit_counter == 9)
                    {
                        numericUpDown31.Value = Limit_Temperature_Low;
                        makeProtocolParamGetSetRead("0x40"); // Limit Voltage High
                    }
                    else if (Limit_counter == 10)
                    {
                        numericUpDown32.Value = Decimal.Divide(Limit_Voltage_High, 10);
                        makeProtocolParamGetSetRead("0x41"); // Limit Voltage Low
                    }
                    else if (Limit_counter == 11)
                    {
                        numericUpDown33.Value = Decimal.Divide(Limit_Voltage_Low, 10);
                        Limit_counter = 12;
                        tabPage7.Cursor = Cursors.Default;
                        button28.Enabled = true; // Limit Angle CWボタン有効
                        button29.Enabled = true; // Limit Angle CCWボタン有効
                        button30.Enabled = true; // Limit Speed CWボタン有効
                        button31.Enabled = true; // Limit Speed CCWボタン有効
                        button32.Enabled = true; // Limit Torque CWボタン有効
                        button33.Enabled = true; // Limit Torque CCWボタン有効
                        button34.Enabled = true; // Limit Temperature Highボタン有効
                        button35.Enabled = true; // Limit Temperature Lowボタン有効
                        button36.Enabled = true; // Limit Voltage Highボタン有効
                        button37.Enabled = true; // Limit Voltage Lowボタン有効
                    }
                    else if (Limit_button_clicked == 3)
                    {
                        makeProtocolParamGetSetRead("0x3A"); // Limit Speed CW
                    }
                    else if (Limit_button_clicked == 4)
                    {
                        makeProtocolParamGetSetRead("0x3B");
                    }
                    else if (Limit_button_clicked == 9)
                    {
                        makeProtocolParamGetSetRead("0x40");
                    }
                    else if (Limit_button_clicked == 10)
                    {
                        makeProtocolParamGetSetRead("0x41");
                    }
                    else if (Limit_button_clicked == 103)
                    {
                        numericUpDown26.Value = Limit_Speed_CW;
                        Limit_button_clicked = 0;
                    }
                    else if (Limit_button_clicked == 104)
                    {
                        numericUpDown27.Value = Limit_Speed_CCW;
                        Limit_button_clicked = 0;
                    }
                    else if (Limit_button_clicked == 109)
                    {
                        numericUpDown32.Value = Decimal.Divide(Limit_Voltage_High, 10);
                        Limit_button_clicked = 0;
                    }
                    else if (Limit_button_clicked == 110)
                    {
                        numericUpDown33.Value = Decimal.Divide(Limit_Voltage_Low, 10);
                        Limit_button_clicked = 0;
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 7)) // Option
                {
                    if (Option_counter == 1)
                    {
                        tabPage8.Cursor = Cursors.WaitCursor;
                        makeProtocolParamGetSetRead("0x44"); // Origin Position
                    }
                    else if (Option_counter == 2)
                    {
                        numericUpDown34.Value = (decimal)Origin_Position / 10;
                        makeProtocolParamGetSetRead("0x45"); // Actuator ID
                    }
                    else if (Option_counter == 3)
                    {
                        numericUpDown35.Value = Actuator_ID;
                        makeProtocolParamGetSetRead("0x46"); // DroneCAN Node ID
                    }
                    else if (Option_counter == 4)
                    {
                        numericUpDown36.Value = DroneCAN_Node_ID;
                        Option_counter = 5;
                        tabPage8.Cursor = Cursors.Default;
                        button38.Enabled = true; // Origin Positionボタン有効
                        button39.Enabled = true; // Actuator IDボタン有効
                        button40.Enabled = true; // DroneCAN Node IDボタン有効
                    }
                }
                else if ((servo_flag == true) && (parameter_tab == 8)) // Manufacture
                {
                    if (Manufacture_counter == 1)
                    {
                        tabPage9.Cursor = Cursors.WaitCursor;
                        makeProtocolParamGetSetRead("0x50"); // Model Number
                    }
                    else if (Manufacture_counter == 2)
                    {
                        textBox1.Text = Model_Number.ToString();
                        makeProtocolParamGetSetRead("0x51"); // Unique Number
                    }
                    else if (Manufacture_counter == 3)
                    {
                        textBox2.Text = Unique_Number.ToString();
                        makeProtocolParamGetSetRead("0x52"); // Firmware Version
                    }
                    else if (Manufacture_counter == 4)
                    {
                        Firmware_Version += 10000;
                        textBox3.Text = string.Format("{0}.{1}{2}", Firmware_Version.ToString()[1]
                            , Firmware_Version.ToString()[2], Firmware_Version.ToString()[3]);
                        Firmware_Version -= 10000;
                        makeProtocolParamGetSetRead("0x53"); // Hardware Version
                    }
                    else if (Manufacture_counter == 5)
                    {
                        if (Hardware_Version > 10000)
                        {
                            textBox4.Text = string.Format("{0}{1}.{2}{3}", Hardware_Version.ToString()[0]
                                , Hardware_Version.ToString()[1], Hardware_Version.ToString()[2], Hardware_Version.ToString()[3]);
                        }
                        else
                        {
                            textBox4.Text = string.Format("{0}.{1}{2}", Hardware_Version.ToString()[0]
                                , Hardware_Version.ToString()[1], Hardware_Version.ToString()[2]);
                        }
                        makeProtocolParamGetSetRead("0x58"); // Manufacture year
                    }
                    else if (Manufacture_counter == 6)
                    {
                        makeProtocolParamGetSetRead("0x59"); // Manufacture month
                    }
                    else if (Manufacture_counter == 7)
                    {
                        makeProtocolParamGetSetRead("0x5A"); // Manufacture date
                    }
                    else if (Manufacture_counter == 8)
                    {
                        makeProtocolParamGetSetRead("0x5B"); // Manufacture hours
                    }
                    else if (Manufacture_counter == 9)
                    {
                        makeProtocolParamGetSetRead("0x5C"); // Manufacture minutes
                    }
                    else if (Manufacture_counter == 10)
                    {
                        textBox5.Text = Manufacture_year.ToString();
                        textBox5.AppendText("/");
                        textBox5.AppendText(Manufacture_month.ToString());
                        textBox5.AppendText("/");
                        textBox5.AppendText(Manufacture_date.ToString());
                        textBox5.AppendText(" ");
                        textBox5.AppendText(Manufacture_hours.ToString());
                        textBox5.AppendText(":");
                        if (Manufacture_minutes < 10)
                            textBox5.AppendText("0");
                        textBox5.AppendText(Manufacture_minutes.ToString());
                        tabPage9.Cursor = Cursors.Default;
                        Manufacture_counter = 11;
                    }
                }
            }
        }


        /********************************************************************************/
        /*  センサグラフ                                                                */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (update_flag == false)
            {
                byte chart_points = 120;

                if ((servo_flag == true) && (parameter_tab == 1)) // Status
                {
                    chart1.Series[0].Points.Clear();
                    for (byte i = (byte)(chart_points - 1); i > 0; i--)
                    {
                        position_buf[i] = position_buf[i - 1];
                    }
                    position_buf[0] = present_position / 10;

                    for (byte i = 0; i < chart_points; i++)
                    {
                        chart1.Series[0].Points.AddXY(i * 0.5, position_buf[i]);
                    }

                    chart1.Series[1].Points.Clear();
                    for (byte i = (byte)(chart_points - 1); i > 0; i--)
                    {
                        speed_buf[i] = speed_buf[i - 1];
                    }
                    speed_buf[0] = (short)present_speed;

                    for (byte i = 0; i < chart_points; i++)
                    {
                        chart1.Series[1].Points.AddXY(i * 0.5, speed_buf[i]);
                    }

                    chart1.Series[2].Points.Clear();
                    for (byte i = (byte)(chart_points - 1); i > 0; i--)
                    {
                        trque_buf[i] = trque_buf[i - 1];
                    }
                    trque_buf[0] = (short)present_torque;

                    for (byte i = 0; i < chart_points; i++)
                    {
                        chart1.Series[2].Points.AddXY(i * 0.5, trque_buf[i]);
                    }

                    chart1.Series[3].Points.Clear();
                    for (byte i = (byte)(chart_points - 1); i > 0; i--)
                    {
                        temprature_buf[i] = temprature_buf[i - 1];
                    }
                    temprature_buf[0] = (short)present_temperature;

                    for (byte i = 0; i < chart_points; i++)
                    {
                        chart1.Series[3].Points.AddXY(i * 0.5, temprature_buf[i]);
                    }

                    chart1.Series[4].Points.Clear();
                    for (byte i = (byte)(chart_points - 1); i > 0; i--)
                    {
                        voltage_buf[i] = voltage_buf[i - 1];
                    }
                    voltage_buf[0] = (short)present_voltage / 10;

                    for (byte i = 0; i < chart_points; i++)
                    {
                        chart1.Series[4].Points.AddXY(i * 0.5, voltage_buf[i]);
                    }
                }
            }
        }


        /********************************************************************************/
        /*  ウィンドウを閉じる                                                          */
        /*      [注記]                                                                  */
        /********************************************************************************/
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("C\r"); // C:CAN通信無効(SLCAN変換器)
                serialPort1.Close();
            }
        }


    }
}
