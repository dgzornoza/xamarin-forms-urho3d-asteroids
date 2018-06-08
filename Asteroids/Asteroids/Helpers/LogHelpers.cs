using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

namespace Asteroids.Helpers
{


    /// <summary>
    /// Clase de ayuda con funcionalidades comunes de ayuda para el uso de un registro de log
    /// </summary>
    public static class Log
    {

        #region [Variables miembro]

        private static readonly object _fileLock = new Object();

        /// <summary>
        /// Enumeracion con los tipos de entrada del log
        /// </summary>
        public enum EnumLogType
        {
            /// <summary>Traza</summary>
            TRACE,
            /// <summary>Depuracion</summary>
            DEBUG,
            /// <summary>Informacion</summary>
            INFO,
            /// <summary>Alerta</summary>
            WARNING,
            /// <summary>Error</summary>
            ERROR,
            /// <summary>Error grave</summary>
            FATAL
        }

        /// <summary>
        /// Enumeracion con los diferentes tipos de niveles de alertas en los logs, mediante estos niveles se podra filtrar que alertas saldran o no en los logs.
        /// Los niveles funcionan en orden inverso (nivel 1 = maximo)
        /// </summary>
        public enum EnumWarningLevel
        {
            /// <summary>Nivel 1 (maximo), saldran todos los warnings</summary>
            LEVEL_1 = 0x1,
            /// <summary>Nivel 2, saldran todos los warnings registrados para este nivel o inferiores</summary>
            LEVEL_2 = 0x2,
            /// <summary>Nivel 3, saldran todos los warnings registrados para este nivel o inferiores</summary>
            LEVEL_3 = 0x3,
            /// <summary>Nivel 4, saldran todos los warnings registrados para este nivel o inferiores</summary>
            LEVEL_4 = 0x4,
            /// <summary>Nivel 5 (minimo), saldran solo los warnings registrados para este nivel</summary>
            LEVEL_5 = 0x5,
        }

        /// <summary>
        /// variable para guardar el nivel de warning usado para el registro en el log
        /// </summary>
        private static EnumWarningLevel _warningLevel;

        #endregion [Variables miembro]


        #region [Inicializacion]

        /// <summary>
        /// Constructor estatico de la clase para inicializar el helper
        /// </summary>
        static Log()
        {
            // crear el directorio por defecto donde seran almacenados los logs
            LogPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // añadir el directorio por defecto 'Log'
            LogPath = System.IO.Path.Combine(LogPath, "log");

            // almacenar el nivel de warnings por defecto = 3
            _warningLevel = EnumWarningLevel.LEVEL_3;

            // crear el directorio de logs si no existe
            try
            {
                if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
            
        }

        #endregion [Inicializacion]


        #region [Propiedades]

        /// <summary>
        /// Propiedad para obtener o establecer el directorio donde seran registrados los logs.
        /// NOTA: la ruta debe tener permisos suficientes.
        /// </summary>
        /// <remarks>La ruta debe tener permisos suficientes.</remarks>
        public static string LogPath { get; set; }

        /// <summary>
        /// Propiedad para obtener o establecer el nombre del archivo donde seran registrados los logs.
        /// Si el nombre es null, sera establecido el nombre por defecto con la fecha del dia.
        /// </summary>
        public static string LogFileName { get; set; }

        #endregion [Propiedades]


        #region [Funciones publicas]

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema
        /// NOTA: esta funcion registra un Log con una entrada de informacion, para otros tipos de entradas, ver funciones sobrecargadas
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        public static void RegisterLog(this string _log)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(EnumLogType.INFO, null, null, EnumWarningLevel.LEVEL_3);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema
        /// NOTA: esta funcion registra un Log con una entrada de informacion, para otros tipos de entradas, ver funciones sobrecargadas
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_warningLevel">Nivel de warning para registrar en el log, la configuracion permite filtrar niveles de warnings (1 maximo, 5 minimo)</param>
        public static void RegisterLog(this string _log, EnumWarningLevel _warningLevel)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(EnumLogType.INFO, null, null, _warningLevel);
        }
        
        /// <summary>
        /// Metodo extensor para registrar un log en el sistema del tipo especificado
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_entryType">tipo de entrada en el log (error, warning, etc.). Por defecto es informacion</param>
        public static void RegisterLog(this string _log, EnumLogType _entryType)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(_entryType, null, null, EnumWarningLevel.LEVEL_3);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema del tipo especificado
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_entryType">tipo de entrada en el log (error, warning, etc.). Por defecto es informacion</param>
        /// <param name="_warningLevel">Nivel de warning para registrar en el log, la configuracion permite filtrar niveles de warnings (1 maximo, 5 minimo)</param>
        public static void RegisterLog(this string _log, EnumLogType _entryType, EnumWarningLevel _warningLevel)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(_entryType, null, null, _warningLevel);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema de tipo informacion registrando el nombre de clase y metodo
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="_method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        public static void RegisterLog(this string _log, Type _type, string _method)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(EnumLogType.INFO, _type, _method, EnumWarningLevel.LEVEL_3);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema de tipo informacion registrando el nombre de clase y metodo
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="_method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        /// <param name="_warningLevel">Nivel de warning para registrar en el log, la configuracion permite filtrar niveles de warnings (1 maximo, 5 minimo)</param>
        public static void RegisterLog(this string _log, Type _type, string _method, EnumWarningLevel _warningLevel)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(EnumLogType.INFO, _type, _method, _warningLevel);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema con nombre de clase y metodo
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_entryType">tipo de entrada en el log (error, warning, etc.)</param>
        /// <param name="_type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="_method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        public static void RegisterLog(this string _log, EnumLogType _entryType, Type _type, string _method)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(null, _entryType, _type, _method, EnumWarningLevel.LEVEL_3);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema con nombre de clase y metodo
        /// </summary>
        /// <param name="_log">referencia a la cadena del log a registrar</param>
        /// <param name="_entryType">tipo de entrada en el log (error, warning, etc.)</param>
        /// <param name="_type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="_method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        /// <param name="_warningLevel">Nivel de warning para registrar en el log, la configuracion permite filtrar niveles de warnings (1 maximo, 5 minimo)</param>
        public static void RegisterLog(this string _log, EnumLogType _entryType, Type _type, string _method, EnumWarningLevel _warningLevel)
        {
            // llamar a la funcion sobrecargada correspondiente
            _log.RegisterLog(null, _entryType, _type, _method, _warningLevel);
        }

        /// <summary>
        /// Metodo extensor para registrar un log en el sistema con nombre de archivo, clase y metodo
        /// </summary>
        /// <param name="log">referencia a la cadena del log a registrar</param>
        /// <param name="filename">
        /// Nombre o ruta\Nombre del archivo de texto en el que sera registrado el log.
        /// Si esta vacio o es nulo, se establecera el nombre por defecto 'Log_{0}.log' donde {0} es la fecha en formato 'MMyyy'                
        /// </param>
        /// <param name="entryType">tipo de entrada en el log (error, warning, etc.)</param>
        /// <param name="type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        /// <param name="warningLevel">Nivel de warning para registrar en el log, la configuracion permite filtrar niveles de warnings (1 maximo, 5 minimo)</param>
        public static void RegisterLog(this string log, string filename, EnumLogType entryType, Type type, string method, 
            EnumWarningLevel warningLevel = EnumWarningLevel.LEVEL_3)
        {
            // verificar si es un warning, el nivel para no registrarse
            if (entryType == EnumLogType.WARNING && (int)warningLevel > (int)Log._warningLevel) return;

            // obtener la cadena con el log formateado
            string logString = Log._getLogStringFormat(log, entryType, type, method);
            // mostrar el log en los resultados del depurador
            Log._showInDebugger(log);

            // registrar en archivo de texto, usar el nombre del parametro de entrada, en caso de no existir, sera el nombre de la propiedad, 
            // si no existe tampoco, sera usado el nombre por defecto
            Log._writeFile(filename ?? LogFileName, logString);
        }

        /// <summary>
        /// Funcion para obtener el contenido actual de los archivos de logs
        /// </summary>
        public static string GetLogsContent()
        {            
            lock (_fileLock)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var fileName in Directory.EnumerateFiles(LogPath))
                {
                    builder.Append(File.ReadAllText(fileName));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Funcion para eliminar los logs actualmente almacenados
        /// </summary>
        public static void DeleteCurrentLogs()
        {
            lock (_fileLock)
            {
                try
                {
                    if (Directory.Exists(LogPath)) Directory.Delete(LogPath, true);
                    Directory.CreateDirectory(LogPath);
                }
                catch (Exception ex)
                {
                    Trace.Write(ex);
                }
                
            }
        }

        #endregion [Funciones publicas]


        #region [Funciones privadas]

        /// <summary>
        /// funcion para mostrar un log en el depurador del entorno de desarrollo
        /// NOTA: esta funcion solo genera el codigo en modo de depuracion, en otros modos no hace nada.  
        /// </summary>
        /// <param name="_log">referencia a la cadena con el log</param>
        /// <remarks>
        /// el atributo 'DebuggerStepThrough' se establece para que no pare en un breakpoint dentro de
        /// esta funcion, si se desea que se pare hay que comentar el atributo.
        /// </remarks>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private static void _showInDebugger(string _log)
        {
            Trace.Write(_log);
        }

        /// <summary>
        /// funcion para escribir el log en una entrada en un archivo de texto con nombre especificado
        /// </summary>
        /// <param name="_log">referencia a la cadena con el log</param>
        /// <param name="_filename">
        /// Nombre o ruta\Nombre del archivo de texto en el que sera registrado el log.
        /// Si esta vacio o es nulo, se establecera el nombre por defecto 'Log_{0}.log' donde {0} es la fecha en formato 'MMyyy'
        /// </param>
        private static void _writeFile(string _filename, string _log)
        {
            // verificar parametros
            if (_log == null) return;

            // establecer el nombre de archivo por defecto a guardar con el log (1 archivo por dia)
            if (string.IsNullOrWhiteSpace(_filename)) _filename = string.Format("Log_{0}.log", DateTime.Now.ToString("yyyyMMdd"));                   
          
            // Crear la ruta con nombre de archivo, si no existe una ruta absoluta en el nombre del archivo, se establece la ruta actual registrada            
            string fullPath = (System.IO.Path.IsPathRooted(_filename)) ? _filename : System.IO.Path.Combine(LogPath, _filename);

            // escribir en el archivo el log
            try
            {
                lock (_fileLock)
                {                    
                    System.IO.File.AppendAllText(fullPath, _log);
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }


        /// <summary>
        /// Funcion para obtener una cadena formateada con el registro de log
        /// </summary>
        /// <param name="_log">cadena con la informacion a registrar en el log</param>
        /// <param name="_logType">Tipo de log a registrar</param>
        /// <param name="_type">Clase que sera registrada en el log, null si no se quiere establecer</param>
        /// <param name="_method">Metodo que sera registrado en el log, null o cadena vacia si no se quiere establecer</param>
        /// <returns>cadena con el registro de log formateado</returns>
        private static string _getLogStringFormat(string _log, EnumLogType _logType, Type _type = null, string _method = null)
        {
            // crear el formato a usar, dependiendo de los parametros de entrada
            // Log
            string format = "{0}@LOG {1}";
            // Fecha
            format += " @DATE: {2}";
            // Clase
            format += (null != _type) ? " @CLASS: {3}" : string.Empty;
            // funcion
            format += (!string.IsNullOrWhiteSpace(_method)) ? " @FUNCTION: {4}" : string.Empty;                   
            // Log
            format += " @MSG: {5}";

            // crear el mensaje con el registro de log
            return string.Format(format,
                System.Environment.NewLine,
                Enum.GetName(typeof(EnumLogType), _logType),
                System.DateTime.Now.ToString(),
                _type,
                _method,
                _log);
        }

        #endregion [Funciones privadas]
    }
}
