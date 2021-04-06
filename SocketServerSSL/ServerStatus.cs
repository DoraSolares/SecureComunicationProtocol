﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServerSSL
{
    public class ServerStatus
    {
        public string GPUName { get; set; }
        public string GPUStatus { get; set; }
        public string GPUDriverVersion { get; set; }
        public string ProcessorName { get; set; }
        public string ProcessorCurrentClockSpeed { get; set; }
        public string OS { get; set; }
        public string OSVersion { get; set; }

        public ServerStatus()
        {

        }

        public ServerStatus(string GPUName, string GPUStatus, string GPUDriverVersion, string ProcessorName, string ProcessorCurrentClockSpeed, string OS, string OSVersion)
        {
            this.GPUName = GPUName;
            this.GPUStatus = GPUStatus;
            this.GPUDriverVersion = GPUDriverVersion;
            this.ProcessorName = ProcessorName;
            this.ProcessorCurrentClockSpeed = ProcessorCurrentClockSpeed;
            this.OS = OS;
            this.OSVersion = OSVersion;
        }
    }
}
