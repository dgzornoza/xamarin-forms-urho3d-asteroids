using System;
using System.Collections.Generic;
using System.Text;

namespace Asteroids.Helpers
{
    /// <summary>
    /// Clase de ayuda con funcionalidades comunes para trabajar con excepciones
    /// </summary>
    public static class Exceptions
    {

        #region [Funciones publicas]

        /// <summary>
        /// Metodo extensor para registrar una excepcion en el sistema, mediante este metodo se dispone de un unico
        /// sistema de registro de excepciones para toda la aplicacion
        /// </summary>
        /// <typeparam name="T">Clase desde la cual se genera la excepcion</typeparam>
        /// <param name="_exception">referencia al objeto de la excepcion</param>
        /// <param name="_infoEx">Informacion extra a escribir en la excepcion al inicio.</param>
        public static void RegisterException<T>(this Exception _exception, string _infoEx = null)
        {
            // verificar parametros de entrada
            if (_exception == null) return;

            // formatear la excepcion en una cadena y registrar en el log
            Log.RegisterLog(_exception._getExceptionStringFormat(_infoEx), Log.EnumLogType.ERROR, typeof(T), _exception.TargetSite.Name);
        }

        /// <summary>
        /// Funcion para obtener una cadena formateada con la excepcion especificada
        /// </summary>
        /// <param name="_obj">referencia al objeto de la excepcion</param>
        /// <param name="_infoEx">Informacion extra a escribir en el log al inicio.</param>
        /// <returns>cadena con la excepcion formateada</returns>
        public static string GetExceptionStringFormat(this Exception _obj, string _infoEx = null)
        {
            return _getExceptionStringFormat(_obj, _infoEx);
        }

        #endregion [Funciones publicas]


        #region [Funciones privadas]

        /// <summary>
        /// Funcion para obtener una cadena formateada con la excepcion especificada
        /// </summary>
        /// <param name="_obj">referencia al objeto de la excepcion</param>
        /// <param name="_infoEx">Informacion extra a escribir en el log al inicio.</param>
        /// <returns>cadena con la excepcion formateada</returns>
        private static string _getExceptionStringFormat(this Exception _obj, string _infoEx = null)
        {
            string infoEx = _infoEx ?? string.Empty;

            // crear el formato del mensaje con la excepcion
            string result = string.Format("{0}Exception: {1}{0}Type: {2}{0}InnerException: {3}{0}InfoEx: {4}{0}Stack: {5}{0}",
                System.Environment.NewLine,
                _obj.Message,
                _obj.GetType().FullName,
                _obj.InnerException,
                infoEx,
                _obj.StackTrace);

            // Si es una excepcion de carga de tipos de algun ensamblado, se registra tambien
            if (_obj is System.Reflection.ReflectionTypeLoadException)
            {
                // hacer un cast y obtener la excepcion
                result += string.Format("{0}{1}{0}", System.Environment.NewLine,
                    _getReflectionTypeLoadException(_obj as System.Reflection.ReflectionTypeLoadException));
            }

            return result;
        }

        /// <summary>
        /// Funcion para obtener la cadena con los errores de una excepcion de carga de tipos
        /// </summary>
        /// <param name="_exception">_excepcion de carga de tipos</param>
        /// <returns>Cadena con el error de carga de tipos</returns>
        private static string _getReflectionTypeLoadException(System.Reflection.ReflectionTypeLoadException _exception)
        {
            System.Text.StringBuilder sb = new StringBuilder();

            // recorrer todas las excepciones de carga de ensamblado
            foreach (Exception exSub in _exception.LoaderExceptions)
            {
                // añadir mensaje de excepcion
                sb.AppendLine(exSub.Message);

                // en caso de ser una excepcion de ensamblado no encontrado, se añade el error
                if (exSub is System.IO.FileNotFoundException)
                {
                    System.IO.FileNotFoundException exFileNotFound = exSub as System.IO.FileNotFoundException;
                    if (exFileNotFound != null && !string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }

                sb.AppendLine();
            }

            // retornar cadena con la excepcion
            return sb.ToString();
        }


        #endregion [Funciones privadas]
    }
}
