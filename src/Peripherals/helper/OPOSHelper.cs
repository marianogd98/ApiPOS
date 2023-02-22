using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.helper
{
    #region class ResultCodeH
    //CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
    public class ResultCodeH
    {
        #region Константы
        /////////////////////////////////////////////////////////////////////
        // OPOS "ResultCode" Property Constants
        /////////////////////////////////////////////////////////////////////
        public const int OPOS_SUCCESS = 0;
        public const int OPOS_E_CLOSED = 101;
        public const int OPOS_E_CLAIMED = 102;
        public const int OPOS_E_NOTCLAIMED = 103;
        public const int OPOS_E_NOSERVICE = 104;
        public const int OPOS_E_DISABLED = 105;
        public const int OPOS_E_ILLEGAL = 106;
        public const int OPOS_E_NOHARDWARE = 107;
        public const int OPOS_E_OFFLINE = 108;
        public const int OPOS_E_NOEXIST = 109;
        public const int OPOS_E_EXISTS = 110;
        public const int OPOS_E_FAILURE = 111;
        public const int OPOS_E_TIMEOUT = 112;
        public const int OPOS_E_BUSY = 113;
        public const int OPOS_E_EXTENDED = 114;

        public const int OPOS_OR_ALREADYOPEN = 301;
        public const int OPOS_OR_REGBADNAME = 302;
        public const int OPOS_OR_REGPROGID = 303;
        public const int OPOS_OR_CREATE = 304;
        public const int OPOS_OR_BADIF = 305;
        public const int OPOS_OR_FAILEDOPEN = 306;
        public const int OPOS_OR_BADVERSION = 307;
        public const int OPOS_ORS_NOPORT = 401;
        public const int OPOS_ORS_NOTSUPPORTED = 402;
        public const int OPOS_ORS_CONFIG = 403;
        #endregion

        #region Message
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static string Message(int _ResultCode)
        {
            switch (_ResultCode)
            {
                case OPOS_SUCCESS:
                    return "OPOS - Exitoso";
                case OPOS_E_CLOSED:
                    return "OPOS - El objeto esta cerrado";
                case OPOS_E_CLAIMED:
                    return "OPOS - Objeto capturado por alguien";
                case OPOS_E_NOTCLAIMED:
                    return "OPOS - Objeto no capturado";
                case OPOS_E_NOSERVICE:
                    return "OPOS - Servicio no admitido";
                case OPOS_E_DISABLED:
                    return "OPOS - Objeto deshabilitado";
                case OPOS_E_ILLEGAL:
                    return "OPOS - Inaceptable";
                case OPOS_E_NOHARDWARE:
                    return "OPOS - Sin equipo";
                case OPOS_E_OFFLINE:
                    return "OPOS - Apagado";
                case OPOS_E_NOEXIST:
                    return "OPOS - El objeto no existe";
                case OPOS_E_EXISTS:
                    return "OPOS - El objeto ya existe";
                case OPOS_E_FAILURE:
                    return "OPOS - Falla general";
                case OPOS_E_TIMEOUT:
                    return "OPOS - Tiempo agotado";
                case OPOS_E_BUSY:
                    return "OPOS - El dispositivo está ocupado";
                case OPOS_E_EXTENDED:
                    return "OPOS - Error extendido";
                case OPOS_OR_ALREADYOPEN:
                    return "OPOS - El objeto ya está abierto";
                case OPOS_OR_REGBADNAME:
                    return "OPOS - No hay ningún dispositivo con este nombre en el registro";
                case OPOS_OR_REGPROGID:
                    return "OPOS - Error de configuración del dispositivo en el registro";
                case OPOS_OR_CREATE:
                    return "OPOS - No se pudo crear el objeto de servicio";
                case OPOS_OR_BADIF:
                    return "OPOS - El objeto de servicio no admite todos los métodos o propiedades requeridos";
                case OPOS_OR_FAILEDOPEN:
                    return "OPOS - Error de apertura desconocido";
                case OPOS_OR_BADVERSION:
                    return "OPOS - Versión de objeto de servicio no válida";
                case OPOS_ORS_NOPORT:
                    return "OPOS - Error de acceso al puerto de E / S";
                case OPOS_ORS_NOTSUPPORTED:
                    return "OPOS - El objeto de servicio no es compatible con el dispositivo especificado";
                case OPOS_ORS_CONFIG:
                    return "OPOS - Error al leer la configuración del registro";
                default:
                    return "OPOS - Código de retorno desconocido";
            }
        }
        #endregion

        #region Check
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static void Check(int _ResultCode)
        {
            if (_ResultCode != OPOS_SUCCESS)
                throw new Exception(ResultCodeH.Message(_ResultCode));
        }
        #endregion
    }
    #endregion

    #region class StatusUpdateH
    //CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
    public class StatusUpdateH
    {
        public const int OPOS_SUE_POWER_ONLINE = 2001; // (added in 1.3)
        public const int OPOS_SUE_POWER_OFF = 2002; // (added in 1.3)
        public const int OPOS_SUE_POWER_OFFLINE = 2003; // (added in 1.3)
        public const int OPOS_SUE_POWER_OFF_OFFLINE = 2004; // (added in 1.3)

        public const int OPOS_SUE_UF_PROGRESS = 2100; // (added in 1.9)
        public const int OPOS_SUE_UF_COMPLETE = 2200; // (added in 1.9)
        public const int OPOS_SUE_UF_COMPLETE_DEV_NOT_RESTORED = 2205; // (added in 1.9)
        public const int OPOS_SUE_UF_FAILED_DEV_OK = 2201; // (added in 1.9)
        public const int OPOS_SUE_UF_FAILED_DEV_UNRECOVERABLE = 2202; // (added in 1.9)
        public const int OPOS_SUE_UF_FAILED_DEV_NEEDS_FIRMWARE = 2203; // (added in 1.9)
        public const int OPOS_SUE_UF_FAILED_DEV_UNKNOWN = 2204; // (added in 1.9)

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static string Message(int _StatusUpdateCode)
        {
            switch (_StatusUpdateCode)
            {
                case OPOS_SUE_POWER_ONLINE:
                    return "Comidas incluidas";
                case OPOS_SUE_POWER_OFF:
                    return "Apagado";
                case OPOS_SUE_POWER_OFFLINE:
                    return "El dispositivo está en modo de ahorro de energía.";
                case OPOS_SUE_POWER_OFF_OFFLINE:
                    return "El dispositivo está apagado o en modo de ahorro de energía";
                case OPOS_SUE_UF_PROGRESS:
                    return "Actualizar el progreso del firmware";
                case OPOS_SUE_UF_COMPLETE:
                    return "Actualización de firmware complet";
                case OPOS_SUE_UF_COMPLETE_DEV_NOT_RESTORED:
                    return "Actualización de firmware completa, dispositivo no restaurado";
                case OPOS_SUE_UF_FAILED_DEV_OK:
                    return "Error al actualizar el firmware, dispositivo correcto";
                case OPOS_SUE_UF_FAILED_DEV_UNRECOVERABLE:
                    return "Error al actualizar el firmware, el dispositivo no se puede recuperar";
                case OPOS_SUE_UF_FAILED_DEV_NEEDS_FIRMWARE:
                    return "Error al actualizar el firmware, el dispositivo necesita firmware";
                case OPOS_SUE_UF_FAILED_DEV_UNKNOWN:
                    return "Error al actualizar el firmware, se desconoce el estado del dispositivo";
                default:
                    return "Código de estado desconocido";
            }
        }
    }
    #endregion

    #region class PowerStateH
    //CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC
    public class PowerStateH
    {
        public const int OPOS_PS_UNKNOWN = 2000;
        public const int OPOS_PS_ONLINE = 2001;
        public const int OPOS_PS_OFF = 2002;
        public const int OPOS_PS_OFFLINE = 2003;
        public const int OPOS_PS_OFF_OFFLINE = 2004;

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static string Message(int _PowerStateCode)
        {
            switch (_PowerStateCode)
            {
                case OPOS_PS_UNKNOWN:
                    return "Desconocido";
                case OPOS_PS_ONLINE:
                    return "Comidas incluidas";
                case OPOS_PS_OFF:
                    return "Apagado";
                case OPOS_PS_OFFLINE:
                    return "Modo de ahorro de energía";
                case OPOS_PS_OFF_OFFLINE:
                    return "Modo apagado o ahorro de energía";
                default:
                    return "Código de estado de energía desconocido";
            }
        }
    }
    #endregion
}
