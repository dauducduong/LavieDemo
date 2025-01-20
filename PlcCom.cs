using S7.Net;
using System;
using System.Windows;

namespace LavieDemo
{
    public class PlcCom
    {
        private Plc plc;
        public bool Connected { get; private set; }

        // Kết nối PLC
        public void Connect(string ipAddress)
        {
            try
            {
                plc = new Plc(CpuType.S71200, ipAddress, 0, 0);
                plc.Open();
                Connected = plc.IsConnected;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PLC Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Connected = false;
            }
        }

        // Ngắt kết nối PLC
        public void Disconnect()
        {
            if (plc != null && plc.IsConnected)
            {
                plc.Close();
                Connected = false;
            }
        }

        // Đọc dữ liệu từ PLC
        public string ReadData(string address)
        {
            try
            {
                if (Connected)
                {
                    var result = plc.Read(address).ToString();
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    throw new InvalidOperationException("PLC is not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Reading PLC data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        // Ghi dữ liệu vào PLC
        public void WriteData(string address, object value)
        {
            try
            {
                if (Connected)
                {
                    plc.Write(address, value);
                }
                else
                {
                    throw new InvalidOperationException("PLC is not connected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Writing PLC data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
