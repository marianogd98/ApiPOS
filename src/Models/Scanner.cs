using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OposScanner_CCO;
using OPOSCONSTANTSLib;
using Models.helpers;

namespace Models
{
    
    public class Scanner
    {
        public OPOSScannerClass _Scanner;
        
        
        public Scanner()
        {
            try
            {
                _Scanner = new OPOSScannerClass();
                //_Scanner.ErrorEvent += new _IOPOSScannerEvents_ErrorEventEventHandler(s_ErrorEvent);
                //_Scanner.DataEvent += new _IOPOSScannerEvents_DataEventEventHandler(s_DataEvent);

                ResultCodeH.Check(_Scanner.Open("RS232Scanner"));
                ResultCodeH.Check(_Scanner.ClaimDevice(7000));

                //MessageBox.Show(this.s.DeviceName.ToString());

                _Scanner.DeviceEnabled = true;
                ResultCodeH.Check(_Scanner.ResultCode);

                _Scanner.AutoDisable = true;
                ResultCodeH.Check(_Scanner.ResultCode);

                _Scanner.DataEventEnabled = true;
                ResultCodeH.Check(_Scanner.ResultCode);
            }
            catch (Exception _e)
            {
                Console.WriteLine(_e.Message.ToString());
            }
        }

        void s_DataEvent(int Status)
        {

            //_Scanner.ScanData;
            _Scanner.DeviceEnabled = true;
            _Scanner.DataEventEnabled = true;
        }
     



    }
}
