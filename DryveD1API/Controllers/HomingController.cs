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
    ///  Read and write homing specific values.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HomingController : ControllerBase
    {

        /// <summary>
        /// This method starts the homing process
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <returns>True when homing is finished</returns>
        [HttpGet("{hostIp}/{port}")]
        public bool Homing(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            ModesOfOperation modesOfOperation = new ModesOfOperation();
            modesOfOperation.Write(connection.socket, ModesOfOperation.ModesEnum.Homing);
            while (modesOfOperation.ReadDisplay(connection.socket) != ModesOfOperation.ModesEnum.Homing)
            {
                Thread.Sleep(100);
            }
            ControlWord controlWord = new ControlWord();
            // Byte 19: 31
            controlWord.Bit00 = true; // 1
            controlWord.Bit01 = true; // 2
            controlWord.Bit02 = true; // 4
            controlWord.Bit03 = true; // 8
            controlWord.Bit04 = true; // 16
            controlWord.Write(connection.socket);

            StatusWord statusWord = new StatusWord();
            while (!(statusWord.Bit00 && statusWord.Bit01 && statusWord.Bit02 && statusWord.Bit05 // 39
                && statusWord.Bit09 && statusWord.Bit10 && statusWord.Bit12)) // 22
            {
                statusWord.Read(connection.socket);
                Thread.Sleep(100);
            }
            return true;
        }

        /// <summary>
        /// 6084h<br />
        /// Indication of deceleration.
        /// </summary>
        /// <param name="hostIp"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        [HttpGet("Method/{hostIp}/{port}")]
        public sbyte GetMethod(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            var telegram = new Telegram();
            telegram.Set(0, AddressConst.HomingMethod, 4);
            var response = telegram.SendAndReceive(connection.socket);
            var result = (sbyte)response.Byte19;
            return result;
        }

        /// <summary>
        /// 6084h<br />
        /// Indication of deceleration.
        /// </summary>
        /// <param name="hostIp"></param>
        /// <param name="port"></param>
        /// <param name="homingMethod"></param>
        [HttpPut("Method/{hostIp}/{port}")]
        public void SetMethod(string hostIp, int port, [FromBody] sbyte homingMethod)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            byte[] data = BitConverter.GetBytes(homingMethod);
            var telegram = new Telegram();
            telegram.Length = 20;
            telegram.Set(1, AddressConst.HomingMethod, 4, 0, data[0]);
            var response = telegram.SendAndReceive(connection.socket);
        }

        /// <summary>
        /// 6099h sub1<br />
        /// Speeds during Search for Switch
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <returns>Switch search velocity</returns>
        [HttpGet("SwitchSearchVelocity/{hostIp}/{port}")]
        public double GetSwitchSearchVelocity(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            var telegram = new Telegram();
            telegram.Set(0, AddressConst.HomingSwitchSearchVelocity, 4);
            var response = telegram.SendAndReceive(connection.socket);
            var result = BitConverter.ToUInt32(new byte[] { response.Byte19, response.Byte20, response.Byte21, response.Byte22 }, 0) / connection.MultiplicationFactor;
            return result;
        }

        /// <summary>
        /// 6099h sub1<br />
        /// Speeds during Search for Switch
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <param name="switchSearchVelocity"></param>
        [HttpPut("SwitchSearchVelocity/{hostIp}/{port}")]
        public void SetSwitchSearchVelocity(string hostIp, int port, [FromBody] double switchSearchVelocity)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            byte[] data = BitConverter.GetBytes((uint)(switchSearchVelocity * connection.MultiplicationFactor));
            var telegram = new Telegram();
            telegram.Length = 23;
            telegram.Set(1, AddressConst.HomingSwitchSearchVelocity, 4, data[0], data[1], data[2], data[3]);
            var response = telegram.SendAndReceive(connection.socket);
        }

        /// <summary>
        /// 6099h sub2<br />
        /// Speeds during Search for Zero
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <returns>Switch search velocity</returns>
        [HttpGet("ZeroSearchVelocity/{hostIp}/{port}")]
        public double GetZeroSearchVelocity(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            var telegram = new Telegram();
            telegram.Set(0, AddressConst.HomingZeroSearchVelocity, 4);
            var response = telegram.SendAndReceive(connection.socket);
            var result = BitConverter.ToUInt32(new byte[] { response.Byte19, response.Byte20, response.Byte21, response.Byte22 }, 0) / connection.MultiplicationFactor;
            return result;
        }

        /// <summary>
        /// 6099h sub2<br />
        /// Speeds during Search for Zero
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <param name="zeroSearchVelocity"></param>
        [HttpPut("ZeroSearchVelocity/{hostIp}/{port}")]
        public void SetZeroSearchVelocity(string hostIp, int port, [FromBody] double zeroSearchVelocity)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            byte[] data = BitConverter.GetBytes((uint)(zeroSearchVelocity * connection.MultiplicationFactor));
            var telegram = new Telegram();
            telegram.Length = 23;
            telegram.Set(1, AddressConst.HomingZeroSearchVelocity, 4, data[0], data[1], data[2], data[3]);
            var response = telegram.SendAndReceive(connection.socket);
        }

        /// <summary>
        /// 609Ah<br />
        /// Indication of the acceleration during homing runs.
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <returns></returns>
        [HttpGet("Acceleration/{hostIp}/{port}")]
        public double GetAcceleration(string hostIp, int port)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            var telegram = new Telegram();
            telegram.Set(0, AddressConst.HomingAcceleration, 4);
            var response = telegram.SendAndReceive(connection.socket);
            var result = BitConverter.ToUInt32(new byte[] { response.Byte19, response.Byte20, response.Byte21, response.Byte22 }, 0) / connection.MultiplicationFactor;
            return result;
        }

        /// <summary>
        /// 609Ah<br />
        /// Indication of the acceleration during homing runs.
        /// </summary>
        /// <param name="hostIp">Ip Address of the Dryve D1 Controller</param>
        /// <param name="port">Port of the Dryve D1 Controller</param>
        /// <param name="acceleration"></param>
        [HttpPut("Acceleration/{hostIp}/{port}")]
        public void SetAcceleration(string hostIp, int port, [FromBody] uint acceleration)
        {
            var connection = ModbusSocket.GetConnection(hostIp, port);
            byte[] data = BitConverter.GetBytes((uint)(acceleration * connection.MultiplicationFactor));
            var telegram = new Telegram();
            telegram.Length = 23;
            telegram.Set(1, AddressConst.HomingAcceleration, 4, data[0], data[1], data[2], data[3]);
            var response = telegram.SendAndReceive(connection.socket);
        }
    }
}