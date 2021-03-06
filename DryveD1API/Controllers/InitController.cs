using System;
using System.Net.Sockets;
using System.Threading;
using DryveD1API.Common;
using DryveD1API.Modules;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DryveD1API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class InitController : ControllerBase
    {
        /// <summary>
        /// This method starts the initialization process
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <returns>True when initialization is finished</returns>
        [HttpGet("{hostIp}/{port}")]
        public bool Init(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            ModesOfOperation modesOfOperation = new ModesOfOperation();
            modesOfOperation.Write(connection.socket, (ModesOfOperation.ModesEnum)1);

            while (modesOfOperation.ReadDisplay(connection.socket) != ModesOfOperation.ModesEnum.ProfilePosition)
            {
                Thread.Sleep(10);
            }

            Reset(connection.socket);
            ShutDown(connection.socket);
            SwitchOn(connection.socket);
            EnableOperation(connection.socket);
            return true;
        }

        private void Reset(Socket s)
        {
            ControlWord controlWord = new ControlWord();
            // Byte 19: 6
            controlWord.Bit01 = true; // 2
            controlWord.Write(s);
            Thread.Sleep(10);
        }

        private void ShutDown(Socket s)
        {
            ControlWord controlWord = new ControlWord();
            // Byte 19: 6
            controlWord.Bit01 = true; // 2
            controlWord.Bit02 = true; // 4
            controlWord.Write(s);

            StatusWord statusWord = new StatusWord();
            while (!(statusWord.Bit00 && statusWord.Bit05 // 33
                && statusWord.Bit09)) // 2
            {
                statusWord.Read(s);
                Thread.Sleep(10);
            }
        }

        private void SwitchOn(Socket s)
        {
            ControlWord controlWord = new ControlWord();
            // Byte 19: 7
            controlWord.Bit00 = true; // 1
            controlWord.Bit01 = true; // 2
            controlWord.Bit02 = true; // 4
            controlWord.Write(s);

            StatusWord statusWord = new StatusWord();
            while (!(statusWord.Bit00 && statusWord.Bit01 && statusWord.Bit05 // 35
                && statusWord.Bit09)) // 2
            {
                statusWord.Read(s);
                Thread.Sleep(10);
            }
        }

        private void EnableOperation(Socket s)
        {
            ControlWord controlWord = new ControlWord();
            // Byte 19: 15
            controlWord.Bit00 = true; // 1
            controlWord.Bit01 = true; // 2
            controlWord.Bit02 = true; // 4
            controlWord.Bit03 = true; // 8
            controlWord.Write(s);

            StatusWord statusWord = new StatusWord();
            while (!(statusWord.Bit00 && statusWord.Bit01 && statusWord.Bit02 && statusWord.Bit05 // 39
                && statusWord.Bit09)) // 2
            {
                statusWord.Read(s);
                Thread.Sleep(10);
            }
        }
    }
}
