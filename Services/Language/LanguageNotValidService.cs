using ChatBOT.Core;
using System;

namespace ChatBOT.Services
{
    public class LanguageNotValidService : ILanguageService
    {
        public string GetText()
        {
            return @"Las cosas que puedo hacer son:
                    - Consultar la ficha de una asignatura.
                    - Consultar información de un profesor, como puede ser su horario de tutoria.
                    - Consultar el horario del grado.
                    - Otras consultas relacionadas con la UNEX. (Basadas en el FAQ)";
        }
    }
}
