using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCSharp1
{
    public class Person
    {
        private string _name; //姓名
        private int _deviceID; //设备ID

        private double _lastLongtitude = 0;//上一次的经度
        private double _lastLatitude = 0;//上一次的纬度

        private double _longtitude = 0;//当前经度
        private double _latitude = 0;//当前纬度

        bool _bUpdatePosition = false;//更新位置
        
        public Person(string Name, int deviceID)
        {
            this._name = Name;
            this._deviceID = deviceID;
        }

        //姓名
        public string Name
        {
            get { return _name; }
        }

        //设备ID
        public int DeviceID
        {
            get { return _deviceID; }
        }

        //经度
        public double Longtitude
        {
            get { return _longtitude; }
        }

        //纬度
        public double Latitude
        {
            get { return _latitude; }
        }

        //是否更新位置
        public bool BUpdatePosition
        {
            get { return _bUpdatePosition; }
            set { _bUpdatePosition = value; }
        }


        //设置位置
        public void SetPosition(double Longtitude, double Latitude)
        {
            _lastLongtitude = _longtitude;
            _lastLatitude = _latitude;

            _longtitude = Longtitude;
            _latitude = Latitude;

            _bUpdatePosition = true;

        }


    }
}
